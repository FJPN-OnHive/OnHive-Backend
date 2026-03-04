using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Tenants;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Layout
{
    public class NavMenuBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public TenantDto? Tenant { get; set; }

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (AdminService?.LoggedUser != null)
            {
                try
                {
                    Tenant = await AdminService.TenantsService.GetTenant(AdminService.LoggedUser.User.TenantId, AdminService.LoggedUser.Token);
                }
                catch (UnauthorizedAccessException ex)
                {
                    await AdminService.Logout("");
                }
            }
            await base.OnInitializedAsync();
        }

        public async Task Logout()
        {
            await AdminService.Logout(string.Empty);
            NavigationManager.NavigateTo("");
            AdminService.notifyUpdate();
        }

        public async Task TriggerProductCourseSearch()
        {
            var result = await AdminService.SearchService.TriggerProductCourseSearch(false, AdminService?.LoggedUser?.Token ?? string.Empty);
            Message = $"Busca de produtos e cursos atualizada ({result}).";
            ShowMessage = true;
        }

        public async Task TriggerProductCourseSearchFull()
        {
            var result = await AdminService.SearchService.TriggerProductCourseSearch(true, AdminService?.LoggedUser?.Token ?? string.Empty);
            Message = $"Busca de produtos e cursos atualizada ({result}).";
            ShowMessage = true;
        }

        public async Task TriggerSearchFull()
        {
            var result = await AdminService.SearchService.TriggerSearch(AdminService?.LoggedUser?.Token ?? string.Empty);
            Message = $"Busca geral atualizada ({result}).";
            ShowMessage = true;
        }

        public bool CheckPermission(PermissionFilters filter)
        {
            return filter switch
            {
                PermissionFilters.Admin => AdminService.LoggedUser.User.Permissions.Contains("tenants_admin")
                                            || AdminService.LoggedUser.User.Permissions.Contains("users_admin")
                                            || AdminService.LoggedUser.User.Permissions.Contains("roles_admin")
                                            || AdminService.LoggedUser.User.Permissions.Contains("events_admin")
                                            || AdminService.LoggedUser.User.Permissions.Contains("users_create")
                                            || AdminService.LoggedUser.User.Permissions.Contains("users_update"),
                PermissionFilters.Tenant => AdminService.LoggedUser.User.Permissions.Contains("tenants_admin"),
                PermissionFilters.User => AdminService.LoggedUser.User.Permissions.Contains("users_admin"),
                PermissionFilters.UserGroups => AdminService.LoggedUser.User.Permissions.Contains("users_create")
                                                && AdminService.LoggedUser.User.Permissions.Contains("users_update")
                                                && AdminService.LoggedUser.User.Permissions.Contains("users_read"),
                PermissionFilters.Roles => AdminService.LoggedUser.User.Permissions.Contains("roles_admin"),
                PermissionFilters.Register => (AdminService.LoggedUser.User.Permissions.Contains("products_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("products_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("products_update"))
                                            ||
                                            (AdminService.LoggedUser.User.Permissions.Contains("courses_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("courses_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("courses_update")),
                PermissionFilters.Products => AdminService.LoggedUser.User.Permissions.Contains("products_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("products_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("products_update"),
                PermissionFilters.Courses => AdminService.LoggedUser.User.Permissions.Contains("courses_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("courses_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("courses_update"),
                PermissionFilters.Events => AdminService.LoggedUser.User.Permissions.Contains("events_read"),
                PermissionFilters.EventsAdmin => AdminService.LoggedUser.User.Permissions.Contains("events_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("events_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("events_update"),
                PermissionFilters.Automations => AdminService.LoggedUser.User.Permissions.Contains("automations_read"),
                PermissionFilters.AutomationsAdmin => AdminService.LoggedUser.User.Permissions.Contains("automations_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("automations_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("automations_update"),
                PermissionFilters.Redirects => AdminService.LoggedUser.User.Permissions.Contains("redirect_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("redirect_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("redirect_update"),
                PermissionFilters.Media => (AdminService.LoggedUser.User.Permissions.Contains("storages_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_create"))
                                            ||
                                            (AdminService.LoggedUser.User.Permissions.Contains("videos_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("videos_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("videos_read"))
                                            ||
                                            (AdminService.LoggedUser.User.Permissions.Contains("redirect_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("redirect_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("redirect_update"))
                                            ||
                                            (AdminService.LoggedUser.User.Permissions.Contains("posts_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("posts_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("posts_create")),
                PermissionFilters.Videos => AdminService.LoggedUser.User.Permissions.Contains("videos_update")
                                             && AdminService.LoggedUser.User.Permissions.Contains("videos_create")
                                             && AdminService.LoggedUser.User.Permissions.Contains("videos_read"),
                PermissionFilters.Images => AdminService.LoggedUser.User.Permissions.Contains("storages_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_create"),
                PermissionFilters.Files => AdminService.LoggedUser.User.Permissions.Contains("storages_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("storages_create"),
                PermissionFilters.Posts => AdminService.LoggedUser.User.Permissions.Contains("posts_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("posts_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("posts_create"),

                PermissionFilters.Dict => AdminService.LoggedUser.User.Permissions.Contains("dict_update")
                                            && AdminService.LoggedUser.User.Permissions.Contains("dict_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("dict_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("dict_delete"),
                PermissionFilters.MessageChannels => AdminService.LoggedUser.User.Permissions.Contains("messageChannels_read")
                                            && AdminService.LoggedUser.User.Permissions.Contains("messageChannels_create")
                                            && AdminService.LoggedUser.User.Permissions.Contains("messageChannels_update"),
                PermissionFilters.MessageAdmin => AdminService.LoggedUser.User.Permissions.Contains("messages_admin"),
                PermissionFilters.EmailTemplates => AdminService.LoggedUser.User.Permissions.Contains("emails_read")
                                                && AdminService.LoggedUser.User.Permissions.Contains("emails_create")
                                                && AdminService.LoggedUser.User.Permissions.Contains("emails_update"),
                PermissionFilters.SearchAdmin => AdminService.LoggedUser.User.Permissions.Contains("search_admin"),
                PermissionFilters.Jobs => AdminService.LoggedUser.User.Permissions.Contains("carreiras_create")
                                          && AdminService.LoggedUser.User.Permissions.Contains("carreiras_update"),
                PermissionFilters.Integrations => AdminService.LoggedUser.User.Permissions.Contains("integrations_read")
                      && AdminService.LoggedUser.User.Permissions.Contains("integrations_update")
                      && AdminService.LoggedUser.User.Permissions.Contains("integrations_create"),
                PermissionFilters.StudentAdmin => AdminService.LoggedUser.User.Permissions.Contains("students_admin"),

                _ => false,
            };
        }
    }
}