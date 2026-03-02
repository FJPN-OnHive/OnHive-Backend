using BlazorMonaco.Editor;
using EHive.Admin.Helpers;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Carreiras;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace EHive.Admin.Pages
{
    public class JobsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        public List<JobDto> Jobs { get; set; } = new();

        public JobDto? SelectedJob { get; set; } = null;

        public bool ShowMessage { get; set; } = false;

        public bool ShowDeleteMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string JobsSearch { get; set; } = string.Empty;

        public RadzenHtmlEditor Editor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/jobs");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/jobs");
            }
            await LoadJobs();
        }

        public async Task SaveJob()
        {
            try
            {
                if (SelectedJob != null)
                {
                    SelectedJob.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    var result = await AdminService.JobsService.Save(SelectedJob, string.IsNullOrEmpty(SelectedJob.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    SelectedJob.Id = result.Id;
                    await LoadJobs();
                    await ClearAsync();
                    Message = "Vaga salva com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/jobs");
            }
            catch (DuplicatedException)
            {
                Message = $"A vaga {SelectedJob?.Title ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar vaga";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadJobs()
        {
            Loading = true;
            try
            {
                Jobs = await AdminService.JobsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/jobs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
                StateHasChanged();
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                Message = string.Empty;
                if (SelectedJob != null)
                {
                    var result = await AdminService.JobsService.Delete(SelectedJob.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                    if (result)
                    {
                        Jobs.Remove(SelectedJob);
                        await ClearAsync();
                        Message = "Vaga deletada com sucesso";
                        ShowMessage = true;
                    }
                    else
                    {
                        Message = "Vaga não encontrada";
                        ShowMessage = true;
                    }
                }
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/jobs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task ClearAsync()
        {
            SelectedJob = null;
            StateHasChanged();
        }

        public async Task SelectJobAsync(JobDto job)
        {
            SelectedJob = job;
            StateHasChanged();
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedJob != null ? SelectedJob.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }

        public string RowClassSelectorJob(JobDto item, int rowIndex)
        {
            return (item.Equals(SelectedJob))
                ? "mud-info"
                : "";
        }
    }
}