using EHive.Admin.Helpers;
using EHive.Admin.Models;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Redirects;
using EHive.Core.Library.Contracts.Storages;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace EHive.Admin.Pages
{
    public class ImagesBase : ComponentBase
    {
        private const long MAXALLOWEDSIZE = 1024 * 1024 * 10; // 10MB

        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IClipboardHelper ClipboardService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public EnvironmentSettings EnvironmentSettings { get; set; }

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public List<StorageImageFileDto> Images = [];

        public StorageImageFileDto SelectedImage { get; set; }

        public string ImageSearch { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string NewCategory { get; set; } = string.Empty;

        public string NewTag { get; set; } = string.Empty;

        public bool noConvert { get; set; } = false;

        private Stream fileStream;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/images");
            }
            await LoadImages();
            Loading = false;
            await base.OnInitializedAsync();
        }

        private async Task LoadImages()
        {
            try
            {
                Images = await AdminService.StorageService.GetImagesAsync(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/images");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task SaveImage()
        {
            Loading = true;
            try
            {
                if (string.IsNullOrEmpty(SelectedImage.Id))
                {
                    SelectedImage = await AdminService.StorageService.UploadImageAsync(SelectedImage, fileStream, noConvert, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                else
                {
                    SelectedImage = await AdminService.StorageService.UpdateImageAsync(SelectedImage, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                Message = $"Imagem salva com sucesso.";
                ShowMessage = true;
                await LoadImages();
            }
            catch (DuplicatedException)
            {
                Message = $"A imagem com id {SelectedImage?.ImageId ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/images");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar a imagem.";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task UploadImage(IBrowserFile file)
        {
            if (file != null)
            {
                fileStream = file.OpenReadStream(MAXALLOWEDSIZE);
                if (fileStream != null)
                {
                    SelectedImage = new StorageImageFileDto
                    {
                        Id = string.Empty,
                        TenantId = AdminService.LoggedUser.User.TenantId,
                        OriginalFileName = file.Name,
                        Name = file.Name.Split('.')[0],
                        ImageId = file.Name.Split('.')[0]
                    };
                }
            }
        }

        public async Task DeleteImage(StorageImageFileDto image)
        {
            Loading = true;
            try
            {
                await AdminService.StorageService.DeleteImageAsync(image.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                Message = $"Imagem removida com sucesso.";
                ShowMessage = true;
                SelectedImage = null;
                await LoadImages();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/images");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao deletar a imagem.";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public void SelectImage(StorageImageFileDto image)
        {
            SelectedImage = image;
        }

        public string RowClassSelectorImages(StorageImageFileDto item, int rowIndex)
        {
            return (item.Equals(SelectedImage))
                ? "mud-info"
                : "";
        }

        public void CopyToClipboard(string url)
        {
            ClipboardService.CopyToClipboard(url);
            Message = "Link copiado para a área de transferência.";
            ShowMessage = true;
        }
    }
}