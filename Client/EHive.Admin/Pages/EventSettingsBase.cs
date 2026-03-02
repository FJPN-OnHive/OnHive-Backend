using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Events;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class EventSettingsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public EventConfigDto? SelectedConfig { get; set; } = null;

        public List<EventConfigDto> EventsConfigs { get; set; }

        public bool Loading { get; set; } = true;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        protected async Task SaveConfig()
        {
            if (SelectedConfig != null)
            {
                try
                {
                    await AdminService.EventsConfigService.Save(SelectedConfig, false, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Configuração salva com sucesso.";
                    ShowMessage = true;
                    StateHasChanged();
                }
                catch (UnauthorizedAccessException)
                {
                    await AdminService.Logout("/eventsettings");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/eventsettings");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/eventsettings");
            }
            await LoadConfigs();
            Loading = false;
        }

        protected async Task LoadConfigs()
        {
            try
            {
                EventsConfigs = await AdminService.EventsConfigService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/eventsettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task SelectEvent(EventConfigDto eventConfig)
        {
            SelectedConfig = eventConfig;
            StateHasChanged();
        }

        protected string RowClassSelector(EventConfigDto item, int rowIndex)
        {
            return (item.Id.Equals(SelectedConfig?.Id, StringComparison.InvariantCultureIgnoreCase))
                ? "mud-info"
                : "";
        }
    }
}