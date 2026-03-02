using EHive.Admin.Helpers;
using EHive.Admin.Services;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class HomeBase : ComponentBase
    {
        [Parameter]
        public string Slug { get; set; } = string.Empty;

        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public string Callback { get; set; } = string.Empty;

        protected override Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            AdminService.TenantSlug = Slug;
            CheckCallback();
            return base.OnInitializedAsync();
        }

        private void CheckCallback()
        {
            if (NavigationManager.TryGetQueryString<string>("callback", out var callback))
            {
                Callback = callback;
            }
        }
    }
}