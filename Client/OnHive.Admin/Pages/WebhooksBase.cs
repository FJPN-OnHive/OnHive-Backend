using BlazorMonaco.Editor;
using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Enums.WebHook;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class WebhooksBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IClipboardHelper ClipboardService { get; set; }

        [Inject]
        public EnvironmentSettings Settings { get; set; }

        public List<WebHookDto> Webhooks { get; set; } = [];

        public WebHookDto? SelectedWebhook { get; set; } = null;

        public WebHookStepDto? SelectedStep { get; set; } = null;

        public WebHookActionDto? SelectedAction { get; set; } = null;

        public bool Loading { get; set; } = true;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public string WebhookSearch { get; set; } = string.Empty;

        public StandaloneCodeEditor JavascriptEditor { get; set; }

        public StandaloneCodeEditor LuaEditor { get; set; }

        public StandaloneCodeEditor PythonEditor { get; set; }

        public List<UserDto> Users = [];

        public UserDto SelectedPersonificationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/webhooks");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/webhooks");
            }
            await LoadUsers();
            await LoadWebHooks();
            Loading = false;
        }

        private async Task LoadUsers()
        {
            Users = await AdminService.UsersService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            var roles = await AdminService.RolesService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            var adminRoles = roles.Where(r => r.IsAdmin).Select(r => r.Name).ToList();
            Users = Users.Where(u => !adminRoles.Any(r => u.Roles.Contains(r))).ToList();
        }

        protected async Task LoadWebHooks()
        {
            try
            {
                Webhooks = await AdminService.WebhookService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/webhooks");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        protected async Task SaveWebhook()
        {
            if (SelectedWebhook != null)
            {
                try
                {
                    SelectedWebhook.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedWebhook.UserId = SelectedPersonificationUser != null ? SelectedPersonificationUser.Id : string.Empty;
                    var newWebhook = await AdminService.WebhookService.Save(SelectedWebhook, string.IsNullOrEmpty(SelectedWebhook.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    SelectedWebhook.Id = newWebhook.Id;
                    Message = "Webhook salvo com sucesso.";
                    ShowMessage = true;
                    StateHasChanged();
                }
                catch (UnauthorizedAccessException)
                {
                    await AdminService.Logout("/webhooks");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        protected async Task DeleteWebhook()
        {
            if (SelectedWebhook != null && !string.IsNullOrEmpty(SelectedWebhook.Id))
            {
                try
                {
                    if (await AdminService.AutomationsService.Delete(SelectedWebhook.Id, AdminService?.LoggedUser?.Token ?? string.Empty))
                    {
                        Message = "Webhook deletado com sucesso.";
                    }
                    else
                    {
                        Message = "Webhook não encontrado para deleção.";
                    }
                    ShowMessage = true;
                    SelectedWebhook = null;
                    StateHasChanged();
                }
                catch (UnauthorizedAccessException)
                {
                    await AdminService.Logout("/webhooks");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        protected async Task SelectWebhook(WebHookDto webhook)
        {
            SelectedWebhook = webhook;
            SelectedPersonificationUser = Users.FirstOrDefault(u => u.Id == SelectedWebhook.UserId);
            StateHasChanged();
        }

        protected async Task SelectStep(WebHookStepDto webhookStep)
        {
            await CommitStepScript();
            SelectedStep = webhookStep;
            await PythonEditor.SetValue(SelectedStep.Script);
            await LuaEditor.SetValue(SelectedStep.Script);
            await JavascriptEditor.SetValue(SelectedStep.Script);
            StateHasChanged();
        }

        protected async Task SelectAction(WebHookActionDto webhookAction)
        {
            SelectedAction = webhookAction;
            StateHasChanged();
        }

        public void ChangeStepType(WebHookStepTypes type)
        {
            if (SelectedStep != null)
            {
                SelectedStep.Type = type;
            }
        }

        public void ChangeActionType(WebHookActionTypes type)
        {
            if (SelectedAction != null)
            {
                SelectedAction.Type = type;
            }
        }

        public void ChangeActionSourceType(WebHookFieldSourceTypes type)
        {
            if (SelectedAction != null)
            {
                SelectedAction.SourceType = type;
            }
        }

        protected string RowClassSelector(WebHookDto item, int rowIndex)
        {
            return (item.Id.Equals(SelectedWebhook?.Id, StringComparison.InvariantCultureIgnoreCase))
                ? "mud-info"
                : "";
        }

        protected string RowStepSelector(WebHookStepDto item, int rowIndex)
        {
            return item.Equals(SelectedStep)
                ? "mud-info"
                : "";
        }

        protected string RowActionSelector(WebHookActionDto item, int rowIndex)
        {
            return item.Equals(SelectedAction)
                ? "mud-info"
                : "";
        }

        public StandaloneEditorConstructionOptions JavascriptEditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "javascript",
                Value = SelectedStep != null ? SelectedStep.Script : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }

        public StandaloneEditorConstructionOptions LuaEditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "lua",
                Value = SelectedStep != null ? SelectedStep.Script : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }

        public StandaloneEditorConstructionOptions PythonEditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "python",
                Value = SelectedStep != null ? SelectedStep.Script : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }

        public async Task CommitStepScript()
        {
            if (SelectedStep != null)
            {
                if (SelectedStep.Type == WebHookStepTypes.JavaScript)
                {
                    SelectedStep.Script = await JavascriptEditor.GetValue();
                }
                else if (SelectedStep.Type == WebHookStepTypes.Lua)
                {
                    SelectedStep.Script = await LuaEditor.GetValue();
                }
                else if (SelectedStep.Type == WebHookStepTypes.Python)
                {
                    SelectedStep.Script = await PythonEditor.GetValue();
                }
                else
                {
                    SelectedStep.Script = string.Empty;
                }
            }
        }

        public async Task CopyAddress(WebHookDto item)
        {
            var baseAddress = Settings.ApiHost + "/v1/WebHook/Receive/";
            if (item.UseAuthorization)
            {
                baseAddress += item.Slug;
            }
            else
            {
                baseAddress += AdminService.LoggedUser?.User?.TenantId + "/" + item.Slug;
            }
            baseAddress += "?apiKey=" + item.ApiKey;
            await ClipboardService.CopyToClipboard(baseAddress);
        }
    }
}