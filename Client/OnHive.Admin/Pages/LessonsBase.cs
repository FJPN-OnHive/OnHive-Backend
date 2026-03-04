using BlazorMonaco.Editor;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Enums.Courses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen.Blazor;

namespace OnHive.Admin.Pages
{
    public class LessonsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<LessonDto> Lessons { get; set; } = new();

        public LessonDto? SelectedLesson { get; set; }

        public List<ExamDto> Exams { get; set; } = [];

        public ExamDto? SelectedExam { get; set; }

        public MaterialDto? SelectedMaterial { get; set; }

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool LessonsLoading { get; set; } = true;
        public bool ExamsLoading { get; set; } = true;

        public string LessonSearch { get; set; } = string.Empty;

        public string SearchExam { get; set; } = string.Empty;

        public string SearchMaterial { get; set; } = string.Empty;

        public RadzenHtmlEditor Editor { get; set; }

        public LessonTypes SelectedLessonType { get; set; } = LessonTypes.Video;

        public MaterialTypes SelectedMaterialType { get; set; } = MaterialTypes.Pdf;

        public int LessonsTotalPages { get; set; } = 0;
        public int ExamsTotalPages { get; set; } = 0;

        public RequestFilter LessonsFilter = new() { Page = 1, PageLimit = 10 };

        public RequestFilter ExamsFilter = new() { Page = 1, PageLimit = 10 };

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/lessons");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/lessons");
            }
            await LoadLessons();
            await LoadExams();
        }

        public async Task SaveLesson()
        {
            try
            {
                if (SelectedLesson == null)
                    return;

                var duiplicatedName = Lessons.Any(l =>
                    string.Equals(l.Name, SelectedLesson.Name, StringComparison.OrdinalIgnoreCase)
                    && l.Id != SelectedLesson.Id
                );

                var duplicatedCode = Lessons.Any(l =>
                    string.Equals(l.Code, SelectedLesson.Code, StringComparison.OrdinalIgnoreCase)
                    && l.Id != SelectedLesson.Id
                );

                if (duplicatedCode)
                {
                    Message = $"Já existe uma aula com o código '{SelectedLesson.Code}'.";
                    ShowMessage = true;
                    return;
                }

                SelectedLesson.Exam = SelectedExam;
                SelectedLesson.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                SelectedLesson.Type = SelectedLessonType;

                await AdminService.LessonsService.Save(
                    SelectedLesson,
                    string.IsNullOrEmpty(SelectedLesson.Id),
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );
                await LoadLessons();
                Message = "Aula salva com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/lessons");
            }
            catch (Exception ex)
            {
                Message = "Erro ao salvar Aula";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadLessons()
        {
            LessonsLoading = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(LessonSearch))
                {
                    LessonsFilter.OrFilter = [
                     new FilterField { Field = "Code" , Operator = "reg", Value = LessonSearch },
                        new FilterField { Field = "Name" , Operator = "reg", Value = LessonSearch }];
                }
                else
                {
                    LessonsFilter.OrFilter = [];
                }
                var paginatedLessons = await AdminService.LessonsService.GetPaginated(LessonsFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                LessonsTotalPages = (int)paginatedLessons.PageCount;
                Lessons = paginatedLessons.Itens ?? [];
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/lessons");
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

        public async Task LoadExams()
        {
            try
            {
                ExamsLoading = true;
                if (!string.IsNullOrWhiteSpace(SearchExam))
                {
                    ExamsFilter.OrFilter = [
                new FilterField { Field = "Code" , Operator = "reg", Value = SearchExam },
                    new FilterField { Field = "Description" , Operator = "reg", Value = SearchExam }];
                }
                else
                {
                    ExamsFilter.OrFilter = [];
                }
                var paginatedExams = await AdminService.ExamsService.GetPaginated(ExamsFilter, AdminService?.LoggedUser?.Token ?? string.Empty);
                ExamsTotalPages = (int)paginatedExams.PageCount;
                Exams = paginatedExams.Itens ?? [];
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/lessons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                ExamsLoading = false;
            }
        }

        public async Task LessonsPageChanged(int i)
        {
            LessonsFilter.Page = i;
            await LoadLessons();
        }

        public async Task ExamsPageChanged(int i)
        {
            ExamsFilter.Page = i;
            await LoadExams();
        }

        public void ChangeMaterialType(MaterialTypes type)
        {
            if (SelectedMaterial == null)
                return;
            SelectedMaterial.Type = type;
            SelectedMaterialType = type;
        }

        public void ChangeLessonType(LessonTypes type)
        {
            if (SelectedLesson == null)
                return;
            SelectedLesson.Type = type;
            SelectedLessonType = type;
        }

        public void Clear()
        {
            SelectedLesson = new();
            SelectedExam = null;
            StateHasChanged();
        }

        public void SelectLesson(LessonDto lesson)
        {
            SelectedLesson = lesson;
            SelectedLessonType = SelectedLesson.Type ?? LessonTypes.Lesson;
            SelectedExam = Exams.FirstOrDefault(e => SelectedLesson?.Exam?.Id == e.Id);
            StateHasChanged();
        }

        public void SelectExam(ExamDto exam)
        {
            SelectedExam = exam;
            StateHasChanged();
        }

        public string RowClassSelector(LessonDto item, int rowIndex)
        {
            return (item.Equals(SelectedLesson))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorMaterial(MaterialDto item, int rowIndex)
        {
            return (item.Equals(SelectedMaterial))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorExam(ExamDto item, int rowIndex)
        {
            return (item.Equals(SelectedExam))
                ? "mud-info"
                : "";
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedLesson != null ? SelectedLesson.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }

        public async Task ChangeLessonsPageLimit(int pagesize)
        {
            LessonsFilter.PageLimit = pagesize;
            LessonsFilter.Page = 1;
            await LoadLessons();
        }

        public async Task ChangeExamsPageLimit(int pagesize)
        {
            ExamsFilter.PageLimit = pagesize;
            ExamsFilter.Page = 1;
            await LoadExams();
        }

        public async Task SearchLessons()
        {
            LessonsFilter.Page = 1;
            await LoadLessons();
        }

        public async Task SearchExams()
        {
            ExamsFilter.Page = 1;
            await LoadExams();
        }

        public async Task OnLessonsSearchKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await SearchLessons();
            }
        }

        public async Task OnExamsSearchKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await SearchExams();
            }
        }
    }
}
