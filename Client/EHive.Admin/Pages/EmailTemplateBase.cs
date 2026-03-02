using BlazorMonaco.Editor;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Emails;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class EmailTemplateBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<EmailTemplateDto> EmailTemplates { get; set; } = new();

        public EmailTemplateDto? SelectedTemplate { get; set; } = null;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string TemplateSearch { get; set; } = string.Empty;

        public StandaloneCodeEditor Editor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/emailTemplate");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/emailTemplate");
            }
            await LoadPosts();
            Loading = false;
        }

        public async Task SaveTemplate()
        {
            try
            {
                if (SelectedTemplate != null)
                {
                    SelectedTemplate.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedTemplate.Body = await Editor.GetValue();
                    await AdminService.EmailTemplateService.Save(SelectedTemplate, string.IsNullOrEmpty(SelectedTemplate.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Template salvo com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/emailTemplate");
            }
            catch (DuplicatedException)
            {
                Message = $"O template {SelectedTemplate?.Name ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Post";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadPosts()
        {
            try
            {
                EmailTemplates = await AdminService.EmailTemplateService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/emailTemplate");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAsync()
        {
            SelectedTemplate = null;
            StateHasChanged();
        }

        public async Task SelectTemplateAsync(EmailTemplateDto template)
        {
            SelectedTemplate = template;
        }

        public string RowClassSelectorPost(EmailTemplateDto item, int rowIndex)
        {
            return (item.Equals(SelectedTemplate))
                ? "mud-info"
                : "";
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedTemplate != null ? SelectedTemplate.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }
    }
}