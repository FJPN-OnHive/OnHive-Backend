using EHive.Admin.Services;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class TransactionsBase : ComponentBase
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
                await AdminService.Logout("/transactions");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/transactions");
            }
            Loading = false;
        }
    }
}