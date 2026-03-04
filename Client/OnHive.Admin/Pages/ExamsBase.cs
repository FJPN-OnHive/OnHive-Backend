using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Enums.Courses;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Threading.Tasks;

namespace OnHive.Admin.Pages
{
    public class ExamsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<ExamDto> Exams { get; set; } = [];

        public ExamDto SelectedExam { get; set; }

        public ExamQuestionDto? SelectedQuestion { get; set; } = null;

        public QuestionOptionDto? SelectedQuestionOption { get; set; } = null;

        public QuestionOptionDto? backupQuestionOption = null;

        public bool ShowSaveMessage { get; set; } = false;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string ExamSearch { get; set; } = string.Empty;

        public int TotalPages { get; set; } = 0;

        public CultureInfo culture => CultureInfo.GetCultureInfo("pt-BR");

        public RequestFilter Filter = new() { Page = 1, PageLimit = 10 };

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/exams");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/exams");
            }
            await LoadExams();
            Loading = false;
        }

        public async Task SaveExam()
        {
            try
            {
                if (SelectedExam == null)
                    return;

                bool existeNomeDuplicado = Exams.Any(e =>
                    string.Equals(e.Name, SelectedExam.Name, StringComparison.OrdinalIgnoreCase)
                    && (e.Id != SelectedExam.Id)
                );

                if (existeNomeDuplicado)
                {
                    Message = $"Já existe um exame com o nome '{SelectedExam.Name}'.";
                    ShowMessage = true;
                    return;
                }

                SelectedExam.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                await AdminService.ExamsService.Save(
                    SelectedExam,
                    string.IsNullOrEmpty(SelectedExam.Id),
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );
                await LoadExams();
                Message = "Exame salvo com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/exams");
            }
            catch (Exception ex)
            {
                Message = "Erro ao salvar exame";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task SaveExamNewVersion()
        {
            try
            {
                if (SelectedExam == null || string.IsNullOrEmpty(SelectedExam.Id))
                    return;

                bool existeNomeDuplicado = Exams.Any(e =>
                    string.Equals(e.Name, SelectedExam.Name, StringComparison.OrdinalIgnoreCase)
                    && (e.Id != SelectedExam.Id)
                );

                if (existeNomeDuplicado)
                {
                    Message = $"Já existe um exame com o nome '{SelectedExam.Name}'.";
                    ShowMessage = true;
                    return;
                }

                SelectedExam.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                await AdminService.ExamsService.SaveVersion(
                    SelectedExam,
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );
                await LoadExams();
                Message = "Versão do exame salvo com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/exams");
            }
            catch (Exception ex)
            {
                Message = "Erro ao salvar Versão do exame";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task CopyExam()
        {
            if (SelectedExam == null)
                return;
            SelectedExam.Id = string.Empty;
            SelectedExam.VId = string.Empty;
            SelectedExam.Name += " - Cópia";
            await SaveExam();
        }

        public async Task LoadExams()
        {
            Loading = true;
            try
            {
                Filter.OrFilter = [
                    new FilterField { Field = "Code" , Operator = "reg", Value = ExamSearch },
                    new FilterField { Field = "Description" , Operator = "reg", Value = ExamSearch }];
                var PaginatedExams = await AdminService.ExamsService.GetPaginated(Filter, AdminService?.LoggedUser?.Token ?? string.Empty);
                TotalPages = (int)PaginatedExams.PageCount;
                Exams = PaginatedExams.Itens;
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/exams");
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

        public async Task PageChanged(int i)
        {
            Filter.Page = i;
            await LoadExams();
        }

        public void Clear()
        {
            SelectedExam = new();
            StateHasChanged();
        }

        public void SelectExam(ExamDto Exam)
        {
            SelectedExam = Exam;
            StateHasChanged();
        }

        public void SelectQueston(ExamQuestionDto question)
        {
            SelectedQuestion = question;
            StateHasChanged();
        }

        public void SelectQuestonOption(QuestionOptionDto questionOption)
        {
            SelectedQuestionOption = questionOption;
            StateHasChanged();
        }

        public void BackupQuestionOption(object email)
        {
            backupQuestionOption = new();
            backupQuestionOption.Letter = ((QuestionOptionDto)email).Letter;
            backupQuestionOption.Order = ((QuestionOptionDto)email).Order;
            backupQuestionOption.Body = ((QuestionOptionDto)email).Body;
            backupQuestionOption.IsCorrect = ((QuestionOptionDto)email).IsCorrect;
            StateHasChanged();
        }

        public void ResetQuestionOption(object email)
        {
            ((QuestionOptionDto)email).Letter = backupQuestionOption.Letter;
            ((QuestionOptionDto)email).Order = backupQuestionOption.Order;
            ((QuestionOptionDto)email).Body = backupQuestionOption.Body;
            ((QuestionOptionDto)email).IsCorrect = backupQuestionOption.IsCorrect;
            StateHasChanged();
        }

        public void CheckQuestionOption(object option)
        {
            if (SelectedQuestion != null && SelectedQuestion.Type == QuestionTypes.SingleChoice)
            {
                if (((QuestionOptionDto)option).IsCorrect)
                {
                    SelectedQuestion.Options
                        .Where(e => !e.Equals(((QuestionOptionDto)option)))
                        .ToList()
                        .ForEach(e => e.IsCorrect = false);
                }
            }
            if (SelectedQuestion.Options.Count != 0 && !SelectedQuestion.Options.Any(e => e.IsCorrect))
            {
                SelectedQuestion.Options[0].IsCorrect = true;
            }
            StateHasChanged();
        }

        public void ChangeQuestionType(QuestionTypes type)
        {
            SelectedQuestion.Type = type;
            if (type != QuestionTypes.Dissertative)
            {
                CheckQuestionOption(SelectedQuestion.Options.FirstOrDefault(o => o.IsCorrect));
            }
        }

        public string RowClassSelector(ExamDto item, int rowIndex)
        {
            return (item.Equals(SelectedExam))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorQuestion(ExamQuestionDto item, int rowIndex)
        {
            return (item.Equals(SelectedQuestion))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorQuestionOption(QuestionOptionDto item, int rowIndex)
        {
            return (item.Equals(SelectedQuestionOption))
                ? "mud-info"
                : "";
        }

        public async Task ChangePageLimit(int pagesize)
        {
            Filter.PageLimit = pagesize;
            Filter.Page = 1;
            await LoadExams();
        }
    }
}