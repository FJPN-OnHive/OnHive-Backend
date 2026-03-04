using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class UserGroupsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<UserGroupDto> Groups { get; set; } = [];

        public UserGroupDto? SelectedGroup { get; set; }

        public HashSet<UserDto> SelectedUsers { get; set; } = new();

        public List<UserDto> AllUsers { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public bool ShowDeleteMessage { get; set; } = false;

        public string SearchGroup { get; set; } = string.Empty;

        public string SearchUser { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/userGroups");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/userGroups");
            }
            await LoadGroups();
            Loading = false;
        }

        public async Task SaveGroup()
        {
            try
            {
                Message = string.Empty;
                if (SelectedGroup != null)
                {
                    if (Groups.Any(g =>
                           string.Equals(g.Name, SelectedGroup.Name, StringComparison.OrdinalIgnoreCase)
                           && (string.IsNullOrEmpty(SelectedGroup.Id) || g.Id != SelectedGroup.Id)))
                    {
                        Message = $"O Grupo {SelectedGroup.Name} já existe.";
                        ShowMessage = true;
                        return;
                    }

                    SelectedGroup.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedGroup.UsersIds = SelectedUsers.Select(u => u.Id).ToList();
                    SelectedGroup = await AdminService.UserGroupsService.Save(SelectedGroup, string.IsNullOrEmpty(SelectedGroup.Id), AdminService.LoggedUser.Token);
                    await LoadGroups();
                    Message = "Grupo salvo com sucesso";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/userGroups");
            }
            catch (DuplicatedException)
            {
                Message = $"O Grupo {SelectedGroup?.Name ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task DeleteGroup()
        {
            try
            {
                Message = string.Empty;
                if (SelectedGroup != null)
                {
                    var result = await AdminService.UserGroupsService.Delete(SelectedGroup.Id, AdminService.LoggedUser.Token);
                    if (result)
                    {
                        Groups.Remove(SelectedGroup);
                        SelectedGroup = new();
                        SelectedUsers = new();
                        Message = "Grupo deletado com sucesso";
                        ShowMessage = true;
                    }
                    else
                    {
                        Message = "Grupo não encontrado";
                        ShowMessage = true;
                    }
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/userGroups");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SelectGroup(UserGroupDto group)
        {
            SelectedGroup = group;
            SelectedUsers = AllUsers.Where(u => group.UsersIds.Contains(u.Id)).Distinct().ToHashSet();
            StateHasChanged();
        }

        public void Clear()
        {
            SelectedGroup = new();
            SelectedUsers = new();
            StateHasChanged();
        }

        public string RowClassSelector(UserGroupDto item, int rowIndex)
        {
            return (item.Equals(SelectedGroup))
                ? "mud-info"
                : "";
        }

        private async Task LoadGroups()
        {
            try
            {
                AllUsers = await AdminService.UsersService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                Groups = await AdminService.UserGroupsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/userGroups");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}