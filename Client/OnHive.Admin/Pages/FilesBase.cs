using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace OnHive.Admin.Pages
{
    public class FilesBase : ComponentBase
    {
        private const long MAXALLOWEDSIZE = 1024 * 1024 * 100; // 100MB

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

        public List<StorageFileDto> Files = [];

        public StorageFileDto SelectedFile { get; set; }

        public string FileSearch { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string NewCategory { get; set; } = string.Empty;

        public string NewTag { get; set; } = string.Empty;

        private Stream fileStream;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/files");
            }
            await LoadFiles();
            Loading = false;
            await base.OnInitializedAsync();
        }

        private async Task LoadFiles()
        {
            try
            {
                Files = await AdminService.StorageService.GetFilesAsync(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/files");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task SaveFile()
        {
            Loading = true;
            try
            {
                if (string.IsNullOrEmpty(SelectedFile.Id))
                {
                    SelectedFile = await AdminService.StorageService.UploadFileAsync(SelectedFile, fileStream, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                else
                {
                    SelectedFile = await AdminService.StorageService.UpdateFileAsync(SelectedFile, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                Message = $"Arquivo salvo com sucesso.";
                ShowMessage = true;
                await LoadFiles();
            }
            catch (DuplicatedException)
            {
                Message = $"O arquivo com id {SelectedFile?.FileId ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/files");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar o arquivo.";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task UploadFile(IBrowserFile file)
        {
            if (file != null)
            {
                fileStream = file.OpenReadStream(MAXALLOWEDSIZE);
                if (fileStream != null)
                {
                    SelectedFile = new StorageFileDto
                    {
                        Id = string.Empty,
                        TenantId = AdminService.LoggedUser.User.TenantId,
                        OriginalFileName = file.Name,
                        Name = file.Name.Split('.')[0],
                        FileId = file.Name.Split('.')[0]
                    };
                }
            }
        }

        public async Task DeleteFile(StorageFileDto file)
        {
            Loading = true;
            try
            {
                await AdminService.StorageService.DeleteFileAsync(file.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                Message = $"Arquivo removido com sucesso.";
                ShowMessage = true;
                SelectedFile = null;
                await LoadFiles();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/files");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao deletar o arquivo.";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public void SelectFile(StorageFileDto file)
        {
            SelectedFile = file;
        }

        public string RowClassSelectorImages(StorageFileDto item, int rowIndex)
        {
            return (item.Equals(SelectedFile))
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