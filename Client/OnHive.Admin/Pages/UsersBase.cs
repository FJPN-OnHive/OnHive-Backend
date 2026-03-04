using OnHive.Admin.Base;
using OnHive.Admin.Helpers;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Enums.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace OnHive.Admin.Pages
{
    public class UsersBase : PaginatedComponentBase<UserDto>
    {
        [Inject] public IAdminService AdminService { get; set; }
        [Inject] public IClipboardHelper ClipboardHelper { get; set; }
        public List<UserDto> Users { get; set; } = [];
        public UserDto SelectedUser { get; set; }
        public string SelectedProfileType { get; set; } = string.Empty;
        public UserProfileDto SelectedUserProfile { get; set; }
        public List<RoleDto> Roles { get; set; } = [];
        public string RolesSearch { get; set; } = string.Empty;
        public HashSet<RoleDto> SelectedRoles { get; set; } = [];
        public AddressDto SelectedAdress { get; set; } = new();
        public UserEmailDto SelectedEmail { get; set; } = new();
        public List<UserProfileDto> UserProfiles { get; set; } = new();
        public string ResetPassword { get; set; } = string.Empty;
        public string SearchRole { get; set; } = string.Empty;
        public string UserProfileSearch { get; set; } = string.Empty;
        public bool ShowMessage { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Loading { get; set; } = true;
        public string TempPassword { get; set; } = string.Empty;

        public Dictionary<string, object> InputAttributes { get; set; } = new() {
            { "autocomplete", "off" },
            { "aria-autocomplete", "none" },
            { "list", "autocompleteOff" }
        };

        private string Token => AdminService?.LoggedUser?.Token ?? string.Empty;
        private string TenantId => AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/users");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/users");
            }
            IsLoading = true;
            await LoadRoles();
            await LoadDataAsync();
            IsLoading = false;
            Loading = false;
        }

        private UserDocumentDto backupDocument = new();

        private UserEmailDto backupEmail = new();

        private AddressDto backupAddress = new();

        public async Task SaveUser()
        {
            try
            {
                if (SelectedUser != null)
                {
                    var isNewUser = string.IsNullOrEmpty(SelectedUser.Id);
                    if (!string.IsNullOrEmpty(ResetPassword))
                    {
                        SelectedUser.NewPassword = ResetPassword;
                    }
                    else if (!isNewUser)
                    {
                        SelectedUser.IsChangePasswordRequested = false;
                    }
                    SelectedUser.Roles = SelectedRoles.Select(r => r.Name).ToList();
                    SelectedUser.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedUser = await AdminService.UsersService.Save(SelectedUser, isNewUser, AdminService?.LoggedUser?.Token ?? string.Empty);
                    await SaveProfiles();
                    if (isNewUser)
                    {
                        Users.Add(SelectedUser);
                    }
                    Clear();
                    Message = "Usúario salvo com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/users");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Usuário: {ex.Message}";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task SaveProfiles()
        {
            foreach (var profile in UserProfiles.Where(p => !string.IsNullOrEmpty(p.Title)))
            {
                profile.UserId = SelectedUser.Id;
                profile.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                await AdminService.UserProfileService.Save(profile, string.IsNullOrEmpty(profile.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            await LoadProfiles();
            SelectedProfileType = Constants.ProfileTypes.FirstOrDefault();
        }

        public void Clear()
        {
            SelectedUser = new UserDto
            {
                BirthDate = DateTime.Now.AddYears(-18),
                Gender = "NONE",
                Nationality = "BR",
                IsChangePasswordRequested = true,
                Documents = [],
                Addresses = []
            };
            ResetPassword = string.Empty;
            SelectedRoles = [];
            StateHasChanged();
        }

        public async Task SelectUserAsync(UserDto user)
        {
            SelectedUser = user;
            SelectedRoles = new(Roles.Where(r => user.Roles.Exists(ur => ur.Equals(r.Name, StringComparison.InvariantCultureIgnoreCase))));
            SelectedUser.Addresses.ForEach(a => a.Country = string.IsNullOrEmpty(a.Country.Code) ? new CountryDto { Code = "BR", Name = "Brasil" } : a.Country);
            SelectedUser.Addresses.ForEach(a => a.District = string.IsNullOrEmpty(a.District) ? Constants.UFs[0] : a.District);
            ResetPassword = string.Empty;
            await LoadProfiles();
            StateHasChanged();
        }

        public async Task LoadProfiles()
        {
            SelectedProfileType = Constants.ProfileTypes.FirstOrDefault();
            SelectedUserProfile = null;
            if (SelectedUser != null)
            {
                UserProfiles = await AdminService.UserProfileService.GetByUserId(SelectedUser.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
            }
        }

        public async Task SelectUserProfileAsync(string userProfileType)
        {
            if (userProfileType != Constants.ProfileTypes.FirstOrDefault())
            {
                var index = Constants.ProfileTypes.IndexOf(userProfileType);
                var type = Enum.Parse(typeof(ProfileTypes), index.ToString());
                var profile = UserProfiles.FirstOrDefault(p => p.Type.Equals(type));
                if (profile != null)
                {
                    SelectedUserProfile = profile;
                }
                else
                {
                    SelectedUserProfile = new UserProfileDto
                    {
                        Type = (ProfileTypes)type,
                        UserId = SelectedUser.Id,
                        TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty,
                        PublicEmail = false
                    };
                    UserProfiles.Add(SelectedUserProfile);
                }
            }
            else
            {
                SelectedUserProfile = null;
            }
            StateHasChanged();
        }

        public async Task ValidateEmail(UserEmailDto email)
        {
            try
            {
                if (SelectedUser != null)
                {
                    Loading = true;
                    email = await AdminService.UsersService.ValidateEmail(email.Email, SelectedUser.Id, SelectedUser.TenantId, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Clear();
                    await LoadDataAsync();
                    Message = "Email validado com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/users");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                ApplyUserSearchFilter();
                var paginatedUsers = await AdminService.UsersService.GetPaginated(Filter, Token);

                CalculateTotalPages(paginatedUsers.PageCount, paginatedUsers.Total, paginatedUsers.Itens?.Count ?? 0);
                Users = paginatedUsers.Itens ?? [];

                Users.ForEach(u => u.Gender = string.IsNullOrEmpty(u.Gender) ? "NONE" : u.Gender);
                Users.ForEach(u => u.Nationality = string.IsNullOrEmpty(u.Nationality) ? "BR" : u.Nationality);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/users");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ApplyUserSearchFilter()
        {
            Filter.OrFilter = [];

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                return;
            }

            var searchTerm = SearchTerm.Trim();

            Filter.OrFilter.Add(new FilterField
            {
                Field = "Name",
                Operator = "reg",
                Value = searchTerm
            });

            Filter.OrFilter.Add(new FilterField
            {
                Field = "Login",
                Operator = "reg",
                Value = searchTerm
            });

            Filter.OrFilter.Add(new FilterField
            {
                Field = "Email",
                Operator = "reg",
                Value = searchTerm
            });

            Filter.OrFilter.Add(new FilterField
            {
                Field = "Document",
                Operator = "reg",
                Value = searchTerm
            });
        }

        private async Task LoadRoles()
        {
            try
            {
                Roles = await AdminService.RolesService.GetAll(Token);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/users");
            }
        }

        public void BackupDocument(object document)
        {
            backupDocument = new();
            backupDocument.DocumentType = ((UserDocumentDto)document).DocumentType;
            backupDocument.DocumentNumber = ((UserDocumentDto)document).DocumentNumber;
            StateHasChanged();
        }

        public void ResetDocument(object document)
        {
            ((UserDocumentDto)document).DocumentType = backupDocument.DocumentType;
            ((UserDocumentDto)document).DocumentNumber = backupDocument.DocumentNumber;
            StateHasChanged();
        }

        public void BackupEmail(object email)
        {
            backupEmail = new();
            backupEmail.Email = ((UserEmailDto)email).Email;
            backupEmail.IsMain = ((UserEmailDto)email).IsMain;
            backupEmail.IsValidated = ((UserEmailDto)email).IsValidated;
            StateHasChanged();
        }

        public void ResetEmail(object email)
        {
            ((UserEmailDto)email).Email = backupEmail.Email;
            ((UserEmailDto)email).IsMain = backupEmail.IsMain;
            ((UserEmailDto)email).IsValidated = backupEmail.IsValidated;
            StateHasChanged();
        }

        public void CheckMainEmail(object email)
        {
            if (((UserEmailDto)email).IsMain)
            {
                SelectedUser.Emails
                    .Where(e => !e.Equals(((UserEmailDto)email)))
                    .ToList()
                    .ForEach(e => e.IsMain = false);
            }
            if (SelectedUser.Emails.Count != 0 && !SelectedUser.Emails.Any(e => e.IsMain))
            {
                SelectedUser.Emails[0].IsMain = true;
            }
            StateHasChanged();
        }

        public void BackupAddress(object address)
        {
            backupAddress = new();
            backupAddress.Name = ((AddressDto)address).Name;
            backupAddress.AddressLines = ((AddressDto)address).Name;
            backupAddress.Number = ((AddressDto)address).Number;
            backupAddress.Complement = ((AddressDto)address).Complement;
            backupAddress.District = ((AddressDto)address).District;
            backupAddress.City = ((AddressDto)address).District;
            backupAddress.State = ((AddressDto)address).State;
            backupAddress.Country = ((AddressDto)address).Country;
            backupAddress.ZipCode = ((AddressDto)address).ZipCode;
            backupAddress.IsMainAddress = ((AddressDto)address).IsMainAddress;
            StateHasChanged();
        }

        public void ResetAddress(object address)
        {
            ((AddressDto)address).Name = backupAddress.Name;
            ((AddressDto)address).Name = backupAddress.AddressLines;
            ((AddressDto)address).Number = backupAddress.Number;
            ((AddressDto)address).Complement = backupAddress.Complement;
            ((AddressDto)address).District = backupAddress.District;
            ((AddressDto)address).District = backupAddress.City;
            ((AddressDto)address).State = backupAddress.State;
            ((AddressDto)address).Country = backupAddress.Country;
            ((AddressDto)address).ZipCode = backupAddress.ZipCode;
            ((AddressDto)address).IsMainAddress = backupAddress.IsMainAddress;
            StateHasChanged();
        }

        public void CheckMainAddress(object address)
        {
            if (((AddressDto)address).IsMainAddress)
            {
                SelectedUser.Addresses
                    .Where(e => !e.Equals(((AddressDto)address)))
                    .ToList()
                    .ForEach(e => e.IsMainAddress = false);
            }
            StateHasChanged();
        }

        public void AddNewProfile()
        {
            StateHasChanged();
        }

        public string RowClassSelectorUsers(UserDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorEmails(UserEmailDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser.Emails))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorDocuments(UserDocumentDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser.Documents))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorAddress(AddressDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser.Addresses))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorProfile(UserProfileDto item, int rowIndex)
        {
            return (item.Equals(UserProfiles))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectRoles(RoleDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser.Roles))
                ? "mud-info"
                : "";
        }

        public void CopyTempPassword()
        {
            if (!string.IsNullOrEmpty(TempPassword))
            {
                ClipboardHelper.CopyToClipboard(TempPassword);
            }
        }

        public async Task CreateTempPasswordAsync()
        {
            try
            {
                if (SelectedUser != null)
                {
                    Loading = true;
                    TempPassword = await AdminService.UsersService.CreateTempPassword(SelectedUser.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Senha temporária criada.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/users");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Loading = false;
            }
        }
    }
}