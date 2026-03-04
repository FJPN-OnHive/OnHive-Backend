using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Events;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class AutomationsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IClipboardHelper ClipboardService { get; set; }

        public AutomationDto? SelectedAutomation { get; set; } = null;

        public List<HeaderDto>? SelectedHeaders { get; set; } = null;

        public List<AutomationDto> Automations { get; set; }

        public List<EventConfigDto> Configs { get; set; }

        public bool Loading { get; set; } = true;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public string AutomationSearch { get; set; } = string.Empty;

        private AutomationConditionDto backupCondition = new();

        private HeaderDto backupHeader = new();

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/automations");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/automations");
            }
            await LoadAutomations();
            await LoadConfigs();
            Loading = false;
        }

        protected async Task LoadAutomations()
        {
            try
            {
                Automations = await AdminService.AutomationsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/automations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        protected async Task LoadConfigs()
        {
            try
            {
                Configs = await AdminService.EventsConfigService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/automations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        protected async Task SaveAutomation()
        {
            if (SelectedAutomation != null)
            {
                try
                {
                    if (SelectedAutomation.Type == Core.Library.Enums.Events.AutomationType.Email)
                    {
                        SelectedAutomation.WebHook = null;
                    }
                    else
                    {
                        SelectedAutomation.Email = null;
                        SelectedAutomation.WebHook.Headers = new Dictionary<string, string>(SelectedHeaders.Select(h => h.ToKeyValuePair()));
                    }
                    SelectedAutomation.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    await AdminService.AutomationsService.Save(SelectedAutomation, string.IsNullOrEmpty(SelectedAutomation.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Automação salva com sucesso.";
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

        protected async Task DeleteAutomation()
        {
            if (SelectedAutomation != null && !string.IsNullOrEmpty(SelectedAutomation.Id))
            {
                try
                {
                    if (await AdminService.AutomationsService.Delete(SelectedAutomation.Id, AdminService?.LoggedUser?.Token ?? string.Empty))
                    {
                        Message = "Automação deletada com sucesso.";
                    }
                    else
                    {
                        Message = "Automação não encontrada para deleção.";
                    }
                    ShowMessage = true;
                    SelectedAutomation = null;
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

        public void BackupCondition(object conditiion)
        {
            backupCondition = new();
            backupCondition.Condition = ((AutomationConditionDto)conditiion).Condition;
            backupCondition.Field = ((AutomationConditionDto)conditiion).Field;
            backupCondition.Type = ((AutomationConditionDto)conditiion).Type;
            StateHasChanged();
        }

        public void ResetCondition(object conditiion)
        {
            ((AutomationConditionDto)conditiion).Condition = backupCondition.Condition;
            ((AutomationConditionDto)conditiion).Field = backupCondition.Field;
            ((AutomationConditionDto)conditiion).Type = backupCondition.Type;
            StateHasChanged();
        }

        public void BackupHeader(object header)
        {
            backupHeader.Value = ((HeaderDto)header).Value;
            backupHeader.Key = ((HeaderDto)header).Key;
            StateHasChanged();
        }

        public void ResetHeader(object header)
        {
            ((HeaderDto)header).Value = backupHeader.Value;
            ((HeaderDto)header).Key = backupHeader.Key;
            StateHasChanged();
        }

        protected async Task SelectAutomation(AutomationDto automation)
        {
            SelectedAutomation = automation;
            if (SelectedAutomation.Email == null)
            {
                SelectedAutomation.Email = new();
            }
            if (SelectedAutomation.WebHook == null)
            {
                SelectedAutomation.WebHook = new();
            }
            SelectedHeaders = SelectedAutomation.WebHook.Headers.Select(h => new HeaderDto { Key = h.Key, Value = h.Value }).ToList();
            StateHasChanged();
        }

        protected string RowClassSelector(AutomationDto item, int rowIndex)
        {
            return (item.Id.Equals(SelectedAutomation?.Id, StringComparison.InvariantCultureIgnoreCase))
                ? "mud-info"
                : "";
        }

        protected List<EventConfigFieldsDto> GetFields()
        {
            return Configs?.Find(c => c.Key == SelectedAutomation?.EventKey)?.Fields ?? [];
        }
    }
}