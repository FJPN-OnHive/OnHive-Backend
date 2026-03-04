using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Students;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace OnHive.Admin.Pages
{
    public class EnrollmentsReportsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager navigationManager { get; set; }

        public List<SyntheticEnrollmentDto> Enrollments { get; set; } = [];

        public List<StudentReportDto> Reports { get; set; } = [];

        public List<CourseResumeDto> Courses { get; set; } = [];

        public HashSet<CourseResumeDto> SelectedCourses { get; set; } = new HashSet<CourseResumeDto>();

        public bool CoursesLoading { get; set; } = true;

        public string SearchCourse { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public DateTime? InitialDate { get; set; } = DateTime.Now.AddDays(-30);

        public DateTime? FinalDate { get; set; } = DateTime.Now;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/EnrollmentsReport");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/EnrollmentsReportd");
            }
            await LoadEnrollments();
            await LoadReports();
            _ = LoadCourses();
        }

        public async Task LoadEnrollments()
        {
            Loading = true;
            try
            {
                var coursesIds = SelectedCourses.Select(c => c.Id).ToList();
                Enrollments = await AdminService.StudentsService.GetEnrollmentsSyntheticAsync(InitialDate!.Value, FinalDate!.Value, coursesIds, AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/EnrollmentsReport");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao carregar os Alunos";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task LoadCourses()
        {
            CoursesLoading = true;
            try
            {
                Courses = await AdminService.CoursesService.GetAllResume(AdminService.LoggedUser.User.TenantId, AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/SurveyReport");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao carregar os cursos";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                Loading = false;
                throw;
            }
            finally
            {
                CoursesLoading = false;
                StateHasChanged();
            }
        }

        public async Task LoadReports()
        {
            Loading = true;
            try
            {
                Reports = await AdminService.StudentReportsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                Reports = Reports.Where(r => r.ReportName == "EnrollmentReportAsync").OrderByDescending(r => r.ReportDate).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/EnrollmentsReport");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao carregar os relatórios";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task AnalyticReportAsync()
        {
            Loading = true;
            try
            {
                var coursesIds = SelectedCourses.Select(c => c.Id).ToList();
                var report = await AdminService.StudentsService.GetEnrollmentsAnalyticAsync(InitialDate!.Value, FinalDate!.Value, coursesIds, AdminService?.LoggedUser?.Token ?? string.Empty);
                Message = $"Relatório está sendo gerado, em alguns minutos atualize a página para vê-lo na lista abaixo.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/EnrollmentsReport");
            }
            catch (Exception ex)
            {
                Message = $"Erro ao carregar os Alunos";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                Loading = false;
                throw;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task ExportAsync()
        {
            var csvString = "Data;Tenant;Curso;Código;Matrículas;Certificados Emitidos";

            foreach (var enrollment in Enrollments)
            {
                csvString += $"\n{enrollment.Date.ToString("dd/MM/yyyy")};{enrollment.TenantId};{enrollment.ProductName};{enrollment.ProductSku};{enrollment.Enrollments};{enrollment.Certificates}";
            }
            var bytes = Encoding.Unicode.GetBytes(csvString);
            var filename = $"onhive_matriculas_e_certificados_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.csv";
            await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
        }

        public async Task DownloadReportAsync(StudentReportDto report)
        {
            var fileId = System.IO.Path.GetFileName(report.FileUrl);
            var reportStream = await AdminService.StorageService.GetPrivateFileStreamAsync(fileId, AdminService?.LoggedUser?.Token ?? string.Empty);
            using (MemoryStream ms = new MemoryStream())
            {
                reportStream.CopyTo(ms);
                var bytes = ms.ToArray();
                var filename = $"onhive_matriculas_e_certificados_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.xlsx";
                await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
            }
        }
    }
}