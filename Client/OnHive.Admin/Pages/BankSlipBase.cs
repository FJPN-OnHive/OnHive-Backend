using OnHive.Admin.Services;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class BankSlipBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/bankslip");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/bankslip");
            }
            Loading = false;
        }
    }
}