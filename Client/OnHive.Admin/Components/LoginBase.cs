using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OnHive.Admin.Components
{
    public class LoginBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Callback { get; set; } = string.Empty;

        public LoginDto LoginData { get; set; } = new();

        public InputType PasswordInput = InputType.Password;

        public bool SaveTenant { get; set; } = false;

        public bool LoginFailed { get; set; } = false;

        public List<TenantResumeDto> TenantResumes { get; set; } = [];

        public TenantResumeDto? CurrentTenantResumeDto { get; set; } = null;

        public bool TenantPreselected { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await AdminService.VerifyLogin();
            await LoadTenants();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.LoadSettings();
                if (!string.IsNullOrEmpty(AdminService.Settings.SavedTenant))
                {
                    SaveTenant = true;
                    CurrentTenantResumeDto = TenantResumes.Find(t => t.TenantId.Equals(AdminService.Settings.SavedTenant, StringComparison.InvariantCultureIgnoreCase));
                    StateHasChanged();
                }
            }
            else
            {
                AdminService.notifyUpdate();
            }
            if (!string.IsNullOrEmpty(AdminService.TenantSlug))
            {
                CurrentTenantResumeDto = TenantResumes.Find(t => t.Slug.Equals(AdminService.TenantSlug, StringComparison.InvariantCultureIgnoreCase));
                TenantPreselected = true;
                StateHasChanged();
            }

            await base.OnInitializedAsync();
        }

        public async Task LoadTenants()
        {
            if (AdminService.TenantsService != null)
            {
                TenantResumes = await AdminService.TenantsService.GetTenantsResumesAsync();
            }
        }

        public async Task Login()
        {
            if (CurrentTenantResumeDto == null)
            {
                return;
            }
            LoginData.TenantId = CurrentTenantResumeDto?.TenantId ?? string.Empty;
            LoginFailed = !await AdminService.Login(LoginData);
            if (!LoginFailed && SaveTenant)
            {
                AdminService.Settings.SavedTenant = LoginData.TenantId;
                await AdminService.SaveSettings();
                if (AdminService.LoggedUser.User.IsChangePasswordRequested)
                {
                    NavigationManager.NavigateTo("/changepassword");
                }
                if (!string.IsNullOrEmpty(Callback))
                {
                    NavigationManager.NavigateTo(Callback);
                }
            }
            else if (!SaveTenant)
            {
                AdminService.Settings.SavedTenant = string.Empty;
                await AdminService.SaveSettings();
            }
            AdminService.notifyUpdate();
        }
    }
}