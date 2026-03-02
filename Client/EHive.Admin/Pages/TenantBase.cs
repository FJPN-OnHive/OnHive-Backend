using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Tenants;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace EHive.Admin.Pages
{
    public class TenantBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public TenantDto CurrentTenant { get; set; } = new();

        public string TenantMessage { get; set; } = string.Empty;

        public List<FeatureDto> Features { get; set; } = [];

        public HashSet<FeatureDto> SelectedFeatures { get; set; } = [];

        public bool ShowMessage { get; set; } = false;

        public string SearchFeature { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/tenant");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/tenant");
            }
            await LoadFeatures();
            await LoadTenant();
            Loading = false;
        }

        public async Task SaveTenant()
        {
            try
            {
                TenantMessage = string.Empty;
                CurrentTenant.Features = SelectedFeatures.Select(f => f.Key).ToList();
                if (await AdminService.TenantsService.SaveTenant(CurrentTenant, AdminService.LoggedUser.Token))
                {
                    TenantMessage = "Tenant salvo com sucesso";
                }
                else
                {
                    TenantMessage = "Erro ao salvar o tenant";
                }
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/tenant");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadFeatures()
        {
            try
            {
                Features = await AdminService.TenantsService.GetFeatures(AdminService.LoggedUser.Token);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/tenant");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        private async Task LoadTenant()
        {
            try
            {
                CurrentTenant = await AdminService.TenantsService.GetTenant(AdminService.LoggedUser.User.TenantId, AdminService.LoggedUser.Token);
                SelectedFeatures = Features.Where(f => CurrentTenant.Features.Contains(f.Key)).ToHashSet();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/tenant");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }

    public class TenantFeaturePlaceholder
    {
        public string Key { get; set; } = string.Empty;
    }
}