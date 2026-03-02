using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace EHive.Admin.Pages
{
    public class ChangePasswordBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Callback { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public ChangePasswordDto ChangePassword { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/changepassword");
            }
            await base.OnInitializedAsync();
        }

        public async Task ExecuteChange()
        {
            try
            {
                if (ChangePassword.NewPassword.Equals(ConfirmPassword)
                    && !string.IsNullOrEmpty(ChangePassword.NewPassword)
                    && !string.IsNullOrEmpty(ChangePassword.OldPasswordHash))
                {
                    ChangePassword.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    ChangePassword.UserId = AdminService?.LoggedUser?.User?.Id ?? string.Empty;
                    var result = await AdminService.UsersService.ChangePassword(ChangePassword, AdminService?.LoggedUser?.Token ?? string.Empty);
                    if (result)
                    {
                        if (!string.IsNullOrEmpty(Callback))
                        {
                            NavigationManager.NavigateTo(Callback);
                        }
                        ShowMessage = true;
                        Message = "Senha alterada com sucesso";
                        ChangePassword = new();
                        ConfirmPassword = string.Empty;
                        AdminService.LoggedUser.User.IsChangePasswordRequested = false;
                        AdminService?.notifyUpdate();
                    }
                    else
                    {
                        ShowMessage = true;
                        Message = "Senha não alterada";
                    }
                }
                else if (string.IsNullOrEmpty(ChangePassword.NewPassword)
                    || string.IsNullOrEmpty(ChangePassword.OldPasswordHash))
                {
                    ShowMessage = true;
                    Message = "Preencher a nova senha e a senha atual";
                }
                else
                {
                    ShowMessage = true;
                    Message = "As Senhas não conferem";
                }
            }
            catch (ArgumentException ex)
            {
                ShowMessage = true;
                Message = "A nova senha não atende aos requisitos mínimos.";
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowMessage = true;
                Message = "Senha atual inválida";
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowMessage = true;
                Message = "Erro ao altera a senha.";
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public bool ValidateNumbers(string value)
        {
            return Regex.IsMatch(value, @"\d");
        }

        public bool ValidateLetters(string value)
        {
            return Regex.IsMatch(value, @"[a-zA-Z]");
        }

        public bool ValidateUpperCase(string value)
        {
            return Regex.IsMatch(value, @"[A-Z]");
        }

        public bool ValidateEspecialChars(string value)
        {
            return Regex.IsMatch(value, "[\\^$@#!&+\\-=\\[\\]{}()%*_\\\\;':\"|\\,\\.\\/?~]");
        }

        public string PasswordPattern { get; set; } = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\\^$@#!&+\\-=\\[\\]{}()%*_\\\\;':\"|\\,\\.\\/?~])(?![^\\^$@#!&+\\-=\\[\\]{}()%*\\\\_;':\"|\\,\\.\\/?~a-zA-Z0-9])(^\\S{8,30}$)";
    }
}