using EHive.Admin.Helpers;
using EHive.Admin.Models;
using EHive.Admin.Services;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Contracts.Videos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.IO;
using EHive.Core.Library.Contracts.Storages;

namespace EHive.Admin.Pages
{
    public class VideosBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IClipboardHelper ClipboardService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public EnvironmentSettings EnvironmentSettings { get; set; }

        [Inject]
        public IVideoService VideoService { get; set; }

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public List<VideoDto> Videos = [];
        public VideoDto SelectedVideo { get; set; }

        public string VideoSearch { get; set; } = string.Empty;

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
                await AdminService.Logout("/videos");
            }

            await LoadVideos();
            Loading = false;

            await base.OnInitializedAsync();
        }

        private async Task LoadVideos()
        {
            try
            {
                Videos = await VideoService.GetVideosAsync(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/videos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error (LoadVideos): {ex.Message}");
                throw;
            }
        }

        public async Task SaveVideo()
        {
            if (SelectedVideo == null)
                return;

            Loading = true;
            try
            {
                var token = AdminService?.LoggedUser?.Token ?? string.Empty;

                if (string.IsNullOrEmpty(SelectedVideo.Id))

                {
                    SelectedVideo = await VideoService.CreateVideoAsync(SelectedVideo, fileStream, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Vídeo salvo com sucesso.";
                }
                else
                {
                    SelectedVideo = await VideoService.UpdateVideoAsync(SelectedVideo, fileStream, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Vídeo atualizado com sucesso.";
                }
                ShowMessage = true;
                await LoadVideos();
            }
            catch (DuplicatedException)
            {
                Message = $"Já existe um vídeo com id {SelectedVideo?.Id ?? string.Empty}.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/videos");
            }
            catch (Exception ex)
            {
                Message = "Erro ao salvar o vídeo.";
                ShowMessage = true;
                Console.WriteLine($"Error (SaveVideo): {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task UploadVideo(IBrowserFile file)
        {
            if (file != null)
            {
                fileStream = file.OpenReadStream();
                if (fileStream != null)
                {
                    SelectedVideo = new VideoDto
                    {
                        Id = string.Empty,
                        TenantId = AdminService.LoggedUser.User.TenantId,
                        FileName = file.Name,
                        Name = file.Name.Split('.')[0],
                        VideoId = file.Name.Split('.')[0]
                    };
                }
            }
        }

        public void SelectVideo(VideoDto video)
        {
            SelectedVideo = video;
        }

        public string RowClassSelectorVideos(VideoDto item, int rowIndex)
        {
            return (item == SelectedVideo) ? "mud-info" : "";
        }

        public void CopyToClipboard(string url)
        {
            ClipboardService.CopyToClipboard(url);
            Message = "Link copiado para a área de transferência.";
            ShowMessage = true;
        }
    }
}