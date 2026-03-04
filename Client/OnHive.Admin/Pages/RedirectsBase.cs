using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Enums.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace OnHive.Admin.Pages
{
    public class RedirectsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public EnvironmentSettings EnvironmentSettings { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public List<RedirectDto> Redirects { get; set; } = [];

        public RedirectDto? SelectedRedirect { get; set; } = null;

        public RedirectDto? backupRedirect = null;

        public bool ShowMessage { get; set; } = false;

        public bool ShowDeleteMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public bool IsEditing { get; set; } = false;

        public string RedirectSearch { get; set; } = string.Empty;

        public ExportFormats ExportType = ExportFormats.Csv;

        public bool ExportActiveOnly { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/redirects");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/redirects");
            }
            await LoadRedirects();
            Loading = false;
        }

        private async Task LoadRedirects()
        {
            try
            {
                var redirects = await AdminService.RedirectsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                if (redirects != null)
                {
                    SelectedRedirect = null;
                    backupRedirect = null;
                    Redirects = redirects;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/redirects");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        public async Task SaveRedirect()
        {
            try
            {
                if (SelectedRedirect != null)
                {
                    if (!SelectedRedirect.RedirectUrl.Trim().ToLower().StartsWith("https://") && !SelectedRedirect.RedirectUrl.Trim().ToLower().StartsWith("http://"))
                    {
                        SelectedRedirect.RedirectUrl = $"https://{SelectedRedirect.RedirectUrl}";
                    }
                    SelectedRedirect.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    var savedRedirect = await AdminService.RedirectsService.Save(SelectedRedirect, string.IsNullOrEmpty(SelectedRedirect.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    SelectedRedirect.Id = savedRedirect?.Id ?? string.Empty;
                    Message = "Redirect salvo com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/redirects");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Redirect";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        public async Task DeleteRedirect()
        {
            try
            {
                if (SelectedRedirect != null && !string.IsNullOrEmpty(SelectedRedirect.Id))
                {
                    SelectedRedirect.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    await AdminService.RedirectsService.Delete(SelectedRedirect.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                if (SelectedRedirect != null)
                {
                    Redirects.Remove(SelectedRedirect);
                    SelectedRedirect = null;
                    Message = "Redirect deletado com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/redirects");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao deletar Redirect";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        public void SelectRedirect(RedirectDto redirect)
        {
            SelectedRedirect = redirect;
            StateHasChanged();
        }

        public void BackupRedirect(object redirect)
        {
            backupRedirect = new();
            backupRedirect.Id = ((RedirectDto)redirect).Id;
            backupRedirect.RedirectUrl = ((RedirectDto)redirect).RedirectUrl;
            backupRedirect.TenantId = ((RedirectDto)redirect).TenantId;
            backupRedirect.Type = ((RedirectDto)redirect).Type;
            backupRedirect.Description = ((RedirectDto)redirect).Description;
            backupRedirect.Name = ((RedirectDto)redirect).Name;
            backupRedirect.PassParameters = ((RedirectDto)redirect).PassParameters;
            backupRedirect.Path = ((RedirectDto)redirect).Path;
            StateHasChanged();
        }

        public void ResetRedirect(object redirect)
        {
            ((RedirectDto)redirect).Id = backupRedirect.Id;
            ((RedirectDto)redirect).RedirectUrl = backupRedirect.RedirectUrl;
            ((RedirectDto)redirect).TenantId = backupRedirect.TenantId;
            ((RedirectDto)redirect).Type = backupRedirect.Type;
            ((RedirectDto)redirect).Description = backupRedirect.Description;
            ((RedirectDto)redirect).Name = backupRedirect.Name;
            ((RedirectDto)redirect).PassParameters = backupRedirect.PassParameters;
            ((RedirectDto)redirect).Path = backupRedirect.Path;
            StateHasChanged();
        }

        public void CopyToClipboard(RedirectDto redirect)
        {
            ClipboardHelper.CopyToClipboard($"{EnvironmentSettings.ApiHost}/v1/Redirect/{AdminService?.LoggedUser?.User?.TenantId}/{redirect.Path}");
            Message = "Link copiado para a área de transferência.";
            ShowMessage = true;
        }

        public string RowClassSelectorRedirect(RedirectDto item, int rowIndex)
        {
            return (item.Equals(SelectedRedirect))
                ? "mud-info"
                : "";
        }

        public async Task ExportAsync()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", AdminService?.LoggedUser?.Token ?? string.Empty);
                var result = await httpClient.GetAsync(exportUrl);
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var bytes = Encoding.UTF8.GetBytes(content);
                    var extession = ExportType switch
                    {
                        ExportFormats.Json => "json",
                        ExportFormats.Xml => "xml",
                        _ => "csv"
                    };
                    var filename = $"redirects.{extession}";
                    await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
                }
            }
        }

        public string GetExportLink()
        {
            return AdminService.RedirectsService.GetExportRedirectsUrl(ExportType, AdminService?.LoggedUser?.User?.TenantId ?? string.Empty, ExportActiveOnly);
        }

        public void CopyExportLink()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                ClipboardHelper.CopyToClipboard(exportUrl);
                Message = $"O Link do arquivo de exportação copiado.";
                ShowMessage = true;
            }
        }

        public void ChangeExportType(ExportFormats format)
        {
            ExportType = format;
        }
    }
}