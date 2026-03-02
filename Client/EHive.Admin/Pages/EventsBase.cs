using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Contracts.Users;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EHive.Admin.Pages
{
    public class EventsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public bool Loading { get; set; } = true;

        public bool LoadingList { get; set; } = true;

        public bool LoadingSelected { get; set; } = true;

        public List<EventResumeDto> Events { get; set; }

        public EventRegisterDto? SelectedEvent { get; set; } = null;

        public DateRange DateFilter { get; set; } = new DateRange(DateTime.Now.AddDays(-1), DateTime.Now);

        public IEnumerable<string> Origins { get; set; } = [];

        public IEnumerable<string> Keys { get; set; } = [];

        public IEnumerable<string> SelectedOrigins { get; set; } = [];

        public IEnumerable<string> SelectedKeys { get; set; } = [];

        public IEnumerable<UserDto> Users { get; set; } = [];

        public IEnumerable<UserDto> SelectedUsers { get; set; } = [];

        public string Tag { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/events");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/events");
            }
            await LoadUsers();
            await LoadConfigs();
            await LoadEvents();
            Loading = false;
            LoadingList = false;
            LoadingSelected = false;
        }

        public async Task SelectEvent(EventResumeDto eventRegister)
        {
            LoadingSelected = true;
            SelectedEvent = await AdminService.EventsService.GetById(eventRegister.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
            LoadingSelected = false;
            StateHasChanged();
        }

        public async Task LoadEvents()
        {
            try
            {
                LoadingList = true;
                var filter = new RequestFilter
                {
                    Filter =
                    {
                        new FilterField
                        {
                            Field = "Date",
                            Operator = "ge",
                            Value = DateFilter.Start?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd")
                        },
                        new FilterField
                        {
                            Field = "Date",
                            Operator = "le",
                            Value = DateFilter.End?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")
                        },
                    }
                };
                if (Keys.Any())
                {
                    filter.Filter.Add(new FilterField
                    {
                        Field = "Key",
                        Operator = "in",
                        Value = string.Join(",", Keys)
                    });
                }
                if (Origins.Any())
                {
                    filter.Filter.Add(new FilterField
                    {
                        Field = "Origin",
                        Operator = "in",
                        Value = string.Join(",", Origins)
                    });
                }
                if (SelectedUsers.Any())
                {
                    filter.Filter.Add(new FilterField
                    {
                        Field = "UserId",
                        Operator = "in",
                        Value = string.Join(",", SelectedUsers.Select(u => u.Id))
                    });
                }
                Events = await AdminService.EventsService.GetByFilter(filter, AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                LoadingList = false;
            }
        }

        protected async Task LoadConfigs()
        {
            try
            {
                var configs = await AdminService.EventsConfigService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                Origins = configs.Select(c => c.Origin).Distinct().ToList();
                Keys = configs.Select(c => c.Key).Distinct().ToList();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        protected async Task LoadUsers()
        {
            try
            {
                Users = await AdminService.UsersService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        protected string RowClassSelector(EventResumeDto item, int rowIndex)
        {
            return (item.Id.Equals(SelectedEvent?.Id, StringComparison.InvariantCultureIgnoreCase))
                ? "mud-info"
                : "";
        }

        protected string GetUserSelectionText(List<string> selectedValues)
        {
            if (selectedValues.Count == 0)
            {
                return $"Nenhum usuário selecionado";
            }
            else if (selectedValues.Count == 1)
            {
                return $"{selectedValues.Count} usuário selecionado";
            }
            else
            {
                return $"{selectedValues.Count} usuários selecionados";
            }
        }

        protected string GetKeySelectionText(List<string> selectedValues)
        {
            if (selectedValues.Count == 0)
            {
                return $"Nenhuma chave selecionada";
            }
            else if (selectedValues.Count == 1)
            {
                return $"{selectedValues.Count} chave selecionada";
            }
            else
            {
                return $"{selectedValues.Count} chaves selecionadas";
            }
        }

        protected string GetOriginSelectionText(List<string> selectedValues)
        {
            if (selectedValues.Count == 0)
            {
                return $"Nenhuma origem selecionada";
            }
            else if (selectedValues.Count == 1)
            {
                return $"{selectedValues.Count} origem selecionada";
            }
            else
            {
                return $"{selectedValues.Count} origens selecionadas";
            }
        }
    }
}