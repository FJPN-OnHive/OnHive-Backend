using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Dict;
using OnHive.Core.Library.Contracts.Redirects;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class DictBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<ValueRegistryDto> Values { get; set; } = new();

        public ValueRegistryDto? SelectedValue { get; set; }

        public bool ShowDeleteMessage { get; set; } = false;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string ValueSearch { get; set; } = string.Empty;

        public string NewTag { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/dict");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/dict");
            }
            await LoadValues();
            await base.OnInitializedAsync();
            Loading = false;
        }

        public async Task LoadValues()
        {
            Loading = true;
            Values = await AdminService.DictService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
        }

        public async Task SaveValue()
        {
            if (SelectedValue == null)
                return;

            Loading = true;
            try
            {
                bool existeNomeDuplicado = Values.Any(v =>
                    string.Equals(v.Name, SelectedValue.Name, StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrEmpty(SelectedValue.Id) || v.Id != SelectedValue.Id)
                );

                if (existeNomeDuplicado)
                {
                    ShowMessage = true;
                    Message = $"Já existe um registro no dicionário com o nome '{SelectedValue.Name}'.";
                    Loading = false;
                    return;
                }

                SelectedValue.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;

                if (string.IsNullOrEmpty(SelectedValue.Id))
                {
                    await AdminService.DictService.Save(SelectedValue, true, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                else
                {
                    await AdminService.DictService.Save(SelectedValue, false, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                await LoadValues();
                ShowMessage = true;
                Message = "Value salvo com sucesso";
            }
            catch (Exception ex)
            {
                ShowMessage = true;
                Message = ex.Message;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task DeleteValue()
        {
            try
            {
                if (SelectedValue != null && !string.IsNullOrEmpty(SelectedValue.Id))
                {
                    SelectedValue.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    await AdminService.DictService.Delete(SelectedValue.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                if (SelectedValue != null)
                {
                    Values.Remove(SelectedValue);
                    SelectedValue = null;
                    Message = "Valor deletado com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/Dict");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao deletar Valor";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }

        public void SelectValue(ValueRegistryDto value)
        {
            SelectedValue = value;
            StateHasChanged();
        }

        public string RowClassSelectorValue(ValueRegistryDto item, int rowIndex)
        {
            return (item.Equals(SelectedValue))
                ? "mud-info"
                : "";
        }
    }
}