using BlazorMonaco.Editor;
using EHive.Admin.Helpers;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace EHive.Admin.Pages
{
    public class CoursesBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        public List<CourseDto> Courses { get; set; } = new();

        public List<CourseDto> SelectedRelatedCourses { get; set; } = new();

        public List<CourseDto> RelatedCourses { get; set; } = [];

        public CourseDto? SelectedCourse { get; set; } = null;

        public List<DisciplineDto> Disciplines { get; set; } = [];

        public List<UserDto> AllUsers { get; set; } = new();

        public List<UserDto> SelectedUsers { get; set; } = new();

        public string SearchUser { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public bool DisciplineLoading { get; set; } = true;

        public bool RelatedCoursesLoading { get; set; } = true;

        public bool UsersLoading { get; set; } = true;

        public bool SelectedDisciplineLoading { get; set; } = true;

        public bool SelectedRelatedCoursesLoading { get; set; } = true;

        public bool SelectedUsersLoading { get; set; } = true;

        public int CoursesTotalPages { get; set; } = 0;

        public int DisciplinesTotalPages { get; set; } = 0;

        public int RelatedCoursesTotalPages { get; set; } = 0;

        public int UsersTotalPages { get; set; } = 0;

        public string DisciplineSearch { get; set; } = string.Empty;

        public string CourseSearch { get; set; } = string.Empty;

        public string RelatedCourseSearch { get; set; } = string.Empty;

        public string UsersSearch { get; set; } = string.Empty;

        public string SearchDiscipline { get; set; } = string.Empty;

        public string NewRequirement { get; set; } = string.Empty;

        public string NewCategory { get; set; } = string.Empty;

        public ExportFormats ExportType = ExportFormats.Csv;

        public bool ExportActiveOnly { get; set; } = true;

        public bool DisciplinesExpanded = false;

        public bool CoursesExpanded = false;

        public bool UsersExpanded = false;

        public StandaloneCodeEditor Editor { get; set; }

        public RequestFilter CoursesFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter DisciplinesFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter SelectedDisciplinesFilter = new() { Page = 1, PageLimit = 999 };

        public RequestFilter RelatedCoursesFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter SelectedRelatedCoursesFilter = new() { Page = 1, PageLimit = 999 };

        public RequestFilter UsersFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter SelectedUsersFilter = new() { Page = 1, PageLimit = 999 };

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/courses");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/courses");
            }
            await LoadCourses();
            await LoadRelatedCourses();
            await LoadDisciplines();
            await LoadUsers();
            Loading = false;
        }

        public async Task LoadUsers()
        {
            try
            {
                UsersLoading = true;
                if (!string.IsNullOrEmpty(UsersSearch))
                {
                    UsersFilter.OrFilter = [
                     new FilterField { Field = "Login" , Operator = "reg", Value = UsersSearch },
                        new FilterField { Field = "Name" , Operator = "reg", Value = UsersSearch }];
                }
                else
                {
                    UsersFilter.OrFilter = [];
                }
                var paginatedUsers = await AdminService.UsersService.GetByProfilePaginated("teacher", UsersFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                UsersTotalPages = (int)paginatedUsers.PageCount;
                AllUsers = paginatedUsers.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                UsersLoading = false;
            }
        }

        public async Task SaveCourse()
        {
            try
            {
                if (SelectedCourse == null)
                    return;

                if (VerifyDuplicated())
                {
                    return;
                }

                SelectedCourse.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                SelectedCourse.RelatedCourses = SelectedRelatedCourses.Select(c => c.Id).ToList();
                SelectedCourse.Body = await Editor.GetValue();
                SelectedCourse.Staff = SelectedUsers.Select(u => new CourseStaffDto { UserId = u.Id, Role = Core.Library.Enums.Courses.CourseRoles.Teacher, Observation = string.Empty }).ToList();

                await AdminService.CoursesService.Save(
                    SelectedCourse,
                    string.IsNullOrEmpty(SelectedCourse.Id),
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );
                await LoadCourses();

                Message = "Curso salvo com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (DuplicatedException)
            {
                Message = $"O curso {SelectedCourse?.Code ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (InvalidPayloadException ex)
            {
                Message = ex.Message;
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Curso";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        private bool VerifyDuplicated()
        {
            var duplicatedCode = Courses.Any(c =>
                string.Equals(c.Code, SelectedCourse.Code, StringComparison.OrdinalIgnoreCase)
                && c.Id != SelectedCourse.Id
            );

            var duplicatedSlug = Courses.Any(c =>
                string.Equals(c.Slug, SelectedCourse.Slug, StringComparison.OrdinalIgnoreCase)
                && c.Id != SelectedCourse.Id
            );

            if (duplicatedCode)
            {
                Message = $"Já existe um curso com o código '{SelectedCourse.Code}'.";
                ShowMessage = true;
                return true;
            }

            if (duplicatedSlug)
            {
                Message = $"Já existe um curso com o slug '{SelectedCourse.Slug}'.";
                ShowMessage = true;
                return true;
            }
            return false;
        }

        public async Task LoadCourses()
        {
            Loading = true;
            try
            {
                if (!string.IsNullOrEmpty(CourseSearch))
                {
                    CoursesFilter.OrFilter = [
                     new FilterField { Field = "Code" , Operator = "reg", Value = CourseSearch },
                        new FilterField { Field = "Name" , Operator = "reg", Value = CourseSearch }];
                }
                else
                {
                    CoursesFilter.OrFilter = [];
                }
                var paginatedCourses = await AdminService.CoursesService.GetPaginated(CoursesFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                CoursesTotalPages = (int)paginatedCourses.PageCount;
                Courses = paginatedCourses.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task LoadSelectedDisciplines()
        {
            SelectedDisciplineLoading = true;
            try
            {
                if (SelectedCourse != null && SelectedCourse.Disciplines.Any())
                {
                    SelectedDisciplinesFilter.OrFilter = [];
                    var paginatedDisciplines = await AdminService.DisciplinesService.GetByIdsPaginated(
                        SelectedCourse.Disciplines.Select(d => d.Id).ToList(),
                        SelectedDisciplinesFilter,
                        AdminService?.LoggedUser?.Token ?? string.Empty);
                    SelectedCourse.Disciplines = paginatedDisciplines.Itens;
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                SelectedDisciplineLoading = false;
            }
        }

        public async Task LoadDisciplines()
        {
            DisciplineLoading = true;
            try
            {
                if (!string.IsNullOrEmpty(DisciplineSearch))
                {
                    DisciplinesFilter.OrFilter = [
                     new FilterField { Field = "Code" , Operator = "reg", Value = DisciplineSearch },
                        new FilterField { Field = "Name" , Operator = "reg", Value = DisciplineSearch }];
                }
                else
                {
                    DisciplinesFilter.OrFilter = [];
                }
                var paginatedDisciplines = await AdminService.DisciplinesService.GetPaginated(DisciplinesFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                DisciplinesTotalPages = (int)paginatedDisciplines.PageCount;
                Disciplines = paginatedDisciplines.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                DisciplineLoading = false;
            }
        }

        public void AddDiscipline(DisciplineDto discipline)
        {
            if (SelectedCourse != null && !SelectedCourse.Disciplines.Exists(l => l.Id == discipline.Id))
            {
                SelectedCourse.Disciplines.Add(discipline);
                StateHasChanged();
            }
        }

        public void RemoveDiscipline(DisciplineDto discipline)
        {
            if (SelectedCourse != null && SelectedCourse.Disciplines.Exists(l => l.Id == discipline.Id))
            {
                SelectedCourse.Disciplines.Remove(discipline);
                StateHasChanged();
            }
        }

        public void RemoveRelatedCourse(CourseDto relatedCourse)
        {
            if (SelectedCourse != null && SelectedRelatedCourses.Exists(l => l.Id == relatedCourse.Id))
            {
                SelectedRelatedCourses.Remove(relatedCourse);
                StateHasChanged();
            }
        }

        public void AddRelatedCourse(CourseDto relatedCourse)
        {
            if (SelectedCourse != null && !SelectedRelatedCourses.Exists(l => l.Id == relatedCourse.Id))
            {
                SelectedRelatedCourses.Add(relatedCourse);
                StateHasChanged();
            }
        }

        public void AddUser(UserDto user)
        {
            if (SelectedCourse != null && !SelectedUsers.Exists(l => l.Id == user.Id))
            {
                SelectedUsers.Add(user);
                StateHasChanged();
            }
        }

        public void RemoveUser(UserDto user)
        {
            if (SelectedCourse != null && SelectedUsers.Exists(l => l.Id == user.Id))
            {
                SelectedUsers.Remove(user);
                StateHasChanged();
            }
        }

        public void Clear()
        {
            SelectedCourse = null;
            SelectedRelatedCourses = [];
            _ = Editor.SetValue(string.Empty);
            StateHasChanged();
        }

        public async void SelectCourse(CourseDto course)
        {
            SelectedCourse = course;
            await LoadSelectedDisciplines();
            await LoadSelectedRelatedCourses();
            await LoadSelectedUsers();
            if (!SelectedCourse.LaunchDate.HasValue)
            {
                SelectedCourse.LaunchDate = DateTime.MinValue;
            }
            if (Editor != null)
            {
                await Editor.SetValue(SelectedCourse.Body);
            }
            StateHasChanged();
        }

        public async Task LoadRelatedCourses()
        {
            try
            {
                RelatedCoursesLoading = true;
                if (!string.IsNullOrEmpty(RelatedCourseSearch))
                {
                    RelatedCoursesFilter.OrFilter = [
                     new FilterField { Field = "Code" , Operator = "reg", Value = DisciplineSearch },
                     new FilterField { Field = "Name" , Operator = "reg", Value = DisciplineSearch }];
                }
                else
                {
                    RelatedCoursesFilter.OrFilter = [];
                }
                var paginatedRelatedCourses = await AdminService.CoursesService.GetPaginated(
                    RelatedCoursesFilter,
                    AdminService?.LoggedUser?.Token ?? string.Empty);
                RelatedCoursesTotalPages = (int)paginatedRelatedCourses.PageCount;
                RelatedCourses = paginatedRelatedCourses.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                RelatedCoursesLoading = false;
            }
        }

        public async Task LoadSelectedRelatedCourses()
        {
            try
            {
                SelectedRelatedCoursesLoading = true;
                if (SelectedCourse != null && SelectedCourse.RelatedCourses.Any())
                {
                    var paginatedRelatedCourses = await AdminService.CoursesService.GetByIdsPaginated(
                        SelectedCourse.RelatedCourses,
                        SelectedRelatedCoursesFilter,
                        AdminService?.LoggedUser?.Token ?? string.Empty);
                    RelatedCourses = paginatedRelatedCourses.Itens;
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                SelectedRelatedCoursesLoading = false;
            }
        }

        public async Task LoadSelectedUsers()
        {
            try
            {
                SelectedUsersLoading = true;
                if (SelectedCourse != null && SelectedCourse.Staff.Any())
                {
                    var paginatedUsers = await AdminService.UsersService.GetByIdsPaginated(
                            SelectedCourse.Staff.Select(s => s.UserId).ToList(),
                            SelectedUsersFilter,
                            AdminService?.LoggedUser?.Token ?? string.Empty
                        );
                    SelectedUsers = paginatedUsers.Itens;
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/courses");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                SelectedUsersLoading = false;
            }
        }

        public async Task ExportAsync()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", AdminService?.LoggedUser?.Token ?? string.Empty);
                var result = await httpClient.GetAsync(exportUrl);
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var bytes = Encoding.UTF8.GetBytes(content);
                    var extession = ExportType switch
                    {
                        ExportFormats.Json => "json",
                        ExportFormats.Xml => "xml",
                        _ => "csv"
                    };
                    var filename = $"courses.{extession}";
                    await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
                }
            }
        }

        public string GetExportLink()
        {
            return AdminService.ProductsService.GetExportProducUrl(ExportType, AdminService?.LoggedUser?.User?.TenantId ?? string.Empty, ExportActiveOnly);
        }

        public void CopyExportLink()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                ClipboardHelper.CopyToClipboard(exportUrl);
                Message = $"O Link do arquivo de exportação copiado.";
                ShowMessage = true;
            }
        }

        public void ChangeExportType(ExportFormats format)
        {
            ExportType = format;
        }

        public string RowClassSelectorCourses(CourseDto item, int rowIndex)
        {
            return (item.Equals(SelectedCourse))
                ? "mud-info"
                : "";
        }

        public async Task ChangeCoursesPageLimit(int pagesize)
        {
            CoursesFilter.PageLimit = pagesize;
            CoursesFilter.Page = 1;
            await LoadCourses();
        }

        public async Task ChangeDisciplinesPageLimit(int pagesize)
        {
            DisciplinesFilter.PageLimit = pagesize;
            DisciplinesFilter.Page = 1;
            await LoadDisciplines();
        }

        public async Task ChangeRelatedCoursesPageLimit(int pagesize)
        {
            RelatedCoursesFilter.PageLimit = pagesize;
            RelatedCoursesFilter.Page = 1;
            await LoadRelatedCourses();
        }

        public async Task ChangeUsersPageLimit(int pagesize)
        {
            UsersFilter.PageLimit = pagesize;
            UsersFilter.Page = 1;
            await LoadUsers();
        }

        public async Task CoursesPageChanged(int i)
        {
            CoursesFilter.Page = i;
            await LoadCourses();
        }

        public async Task DisciplinesPageChanged(int i)
        {
            DisciplinesFilter.Page = i;
            await LoadDisciplines();
        }

        public async Task RelatedCoursesPageChanged(int i)
        {
            RelatedCoursesFilter.Page = i;
            await LoadRelatedCourses();
        }

        public async Task UsersPageChanged(int i)
        {
            UsersFilter.Page = i;
            await LoadUsers();
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedCourse != null ? SelectedCourse.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }
    }
}