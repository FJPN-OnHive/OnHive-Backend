using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace EHive.Admin.Pages
{
    public class RolesBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<RoleDto> Roles { get; set; } = [];

        public RoleDto? SelectedRole { get; set; }

        public HashSet<string> SelectedPermissions { get; set; } = new();

        public List<string> AllPermissions { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public string SearchRole { get; set; } = string.Empty;

        public string SearchPermission { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/roles");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/roles");
            }
            await LoadRoles();
            Loading = false;
        }

        public async Task SaveRole()
        {
            try
            {
                Message = string.Empty;
                if (SelectedRole != null)
                {
                    SelectedRole.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedRole.Permissions = SelectedPermissions.ToList();
                    SelectedRole = await AdminService.RolesService.Save(SelectedRole, string.IsNullOrEmpty(SelectedRole.Id), AdminService.LoggedUser.Token);
                    Message = "Role salvo com sucesso";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/roles");
            }
            catch (DuplicatedException)
            {
                Message = $"O Role {SelectedRole?.Name ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SelectRole(RoleDto role)
        {
            SelectedRole = role;
            SelectedPermissions = new(role.Permissions);
            StateHasChanged();
        }

        public void Clear()
        {
            SelectedRole = new();
            SelectedPermissions = new();
            StateHasChanged();
        }

        public string RowClassSelector(RoleDto item, int rowIndex)
        {
            return (item.Equals(SelectedRole))
                ? "mud-info"
                : "";
        }

        private async Task LoadRoles()
        {
            try
            {
                AllPermissions = await AdminService.RolesService.GetPermissions(AdminService?.LoggedUser?.Token ?? string.Empty);
                AllPermissions.Insert(0, "lms_access");
                AllPermissions.Insert(0, "account_access");
                AllPermissions.Insert(0, "cms_access");
                AllPermissions.Insert(0, "admin_access");
                Roles = await AdminService.RolesService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/roles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}