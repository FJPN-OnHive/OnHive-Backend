using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class MauticIntegrationBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public MauticIntegrationDto MauticIntegrationSettings { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/mauticintegration");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/mauticintegration");
            }
            await LoadSettings();
            Loading = false;
        }

        public async Task SaveSettings()
        {
            try
            {
                Message = string.Empty;
                if (await AdminService.MauticIntegrationService.SaveIntegrationSettings(MauticIntegrationSettings, AdminService.LoggedUser.Token))
                {
                    Message = "Configuração salva com sucesso";
                }
                else
                {
                    Message = "Erro ao salvar a configuração";
                }
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/mauticintegration");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public void CreateApikey()
        {
            if (MauticIntegrationSettings != null)
            {
                MauticIntegrationSettings.IntegrationAPIKey = Guid.NewGuid().ToString();
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                MauticIntegrationSettings = await AdminService.MauticIntegrationService.GetIntegrationSettings(AdminService.LoggedUser.User.TenantId, AdminService.LoggedUser.Token);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/mauticintegration");
            }
            catch (NotFoundException ex)
            {
                MauticIntegrationSettings = new MauticIntegrationDto
                {
                    IntegrationAPIKey = Guid.NewGuid().ToString(),
                    IsActive = false,
                    MauticUrl = string.Empty,
                    MauticClientId = string.Empty,
                    MauticClientSecret = string.Empty
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}