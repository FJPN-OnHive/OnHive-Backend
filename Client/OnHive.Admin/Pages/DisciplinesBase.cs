using BlazorMonaco.Editor;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class DisciplinesBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<DisciplineDto> Disciplines { get; set; } = new();

        public DisciplineDto? SelectedDiscipline { get; set; }

        public List<LessonDto> Lessons { get; set; } = [];

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool DisciplineLoading { get; set; } = true;

        public bool LessonsLoading { get; set; } = true;
        public bool SelectedLessonsLoading { get; set; } = true;

        public string DisciplineSearch { get; set; } = string.Empty;

        public string SearchLesson { get; set; } = string.Empty;

        public string SearchSelectedLesson { get; set; } = string.Empty;

        public StandaloneCodeEditor Editor { get; set; }

        public int LessonsTotalPages { get; set; } = 0;

        public int SelectedLessonsTotalPages { get; set; } = 0;

        public int DisciplinesTotalPages { get; set; } = 0;

        public RequestFilter DisciplinesFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter LessonsFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter SelectedLessonsFilter = new() { Page = 1, PageLimit = 999 };

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/disciplines");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/disciplines");
            }
            await LoadDisciplines();
            await LoadLessons();
            DisciplineLoading = false;
        }

        public async Task SaveDiscipline()
        {
            try
            {
                if (SelectedDiscipline == null)
                    return;

                var duplicatedCode = Disciplines.Any(d =>
                    string.Equals(d.Code, SelectedDiscipline.Code, StringComparison.OrdinalIgnoreCase)
                    && d.Id != SelectedDiscipline.Id
                );

                if (duplicatedCode)
                {
                    Message = $"Já existe uma disciplina com o código '{SelectedDiscipline.Code}'.";
                    ShowMessage = true;
                    return;
                }

                SelectedDiscipline.Exams = [];
                SelectedDiscipline.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                SelectedDiscipline.Body = await Editor.GetValue();

                await AdminService.DisciplinesService.Save(
                    SelectedDiscipline,
                    string.IsNullOrEmpty(SelectedDiscipline.Id),
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );
                await LoadDisciplines();
                Message = "Disciplina salva com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/disciplines");
            }
            catch (DuplicatedException)
            {
                Message = $"A disciplina {SelectedDiscipline?.Code ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = "Erro ao salvar Disciplina";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
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
                await AdminService.Logout("/disciplines");
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

        public async Task LoadSelectedLessons()
        {
            try
            {
                SelectedLessonsLoading = true;
                if (SelectedDiscipline != null && SelectedDiscipline.Lessons.Any())
                {
                    LessonsFilter.OrFilter = [];
                    var paginatedLessons = await AdminService.LessonsService.GetByIdsPaginated(
                        SelectedDiscipline.Lessons.Select(l => l.Id).ToList(),
                        SelectedLessonsFilter,
                        AdminService?.LoggedUser?.Token ?? string.Empty);
                    SelectedLessonsTotalPages = (int)paginatedLessons.PageCount;
                    SelectedDiscipline.Lessons = paginatedLessons.Itens;
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/disciplines");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                SelectedLessonsLoading = false;
            }
        }

        public async Task LoadLessons()
        {
            LessonsLoading = true;
            try
            {
                if (!string.IsNullOrEmpty(SearchLesson))
                {
                    LessonsFilter.OrFilter = [
                     new FilterField { Field = "Code" , Operator = "reg", Value = SearchLesson },
                        new FilterField { Field = "Name" , Operator = "reg", Value = SearchLesson }];
                }
                else
                {
                    LessonsFilter.OrFilter = [];
                }
                var paginatedLessons = await AdminService.LessonsService.GetPaginated(LessonsFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                LessonsTotalPages = (int)paginatedLessons.PageCount;
                Lessons = paginatedLessons.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/disciplines");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                LessonsLoading = false;
            }
        }

        public void AddLesson(LessonDto lesson)
        {
            if (SelectedDiscipline != null && !SelectedDiscipline.Lessons.Exists(l => l.Id == lesson.Id))
            {
                SelectedDiscipline.Lessons.Add(lesson);
                StateHasChanged();
            }
        }

        public void RemoveLesson(LessonDto lesson)
        {
            if (SelectedDiscipline != null && SelectedDiscipline.Lessons.Exists(l => l.Id == lesson.Id))
            {
                SelectedDiscipline.Lessons.Remove(lesson);
                StateHasChanged();
            }
        }

        public void Clear()
        {
            SelectedDiscipline = new();
            StateHasChanged();
        }

        public async Task SelectDisciplineAsync(DisciplineDto discipline)
        {
            SelectedDiscipline = discipline;
            await LoadSelectedLessons();
            StateHasChanged();
        }

        public string RowClassSelector(DisciplineDto item, int rowIndex)
        {
            return (item.Equals(SelectedDiscipline))
                ? "mud-info"
                : "";
        }

        public async Task ChangeLessonsPageLimit(int pagesize)
        {
            LessonsFilter.PageLimit = pagesize;
            LessonsFilter.Page = 1;
            await LoadLessons();
        }

        public async Task ChangeSelectedLessonsPageLimit(int pagesize)
        {
            SelectedLessonsFilter.PageLimit = pagesize;
            SelectedLessonsFilter.Page = 1;
            await LoadSelectedLessons();
        }

        public async Task ChangeDisciplinesPageLimit(int pagesize)
        {
            DisciplinesFilter.PageLimit = pagesize;
            DisciplinesFilter.Page = 1;
            await LoadDisciplines();
        }

        public async Task LessonsPageChanged(int i)
        {
            LessonsFilter.Page = i;
            await LoadLessons();
        }

        public async Task DisciplinesPageChanged(int i)
        {
            DisciplinesFilter.Page = i;
            await LoadDisciplines();
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedDiscipline != null ? SelectedDiscipline.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }
    }
}