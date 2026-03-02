using EHive.Core.Library.Contracts.Students;

namespace EHive.Admin.Services
{
    public class StudentReportsService : ServiceBase<StudentReportDto>, IStudentReportsService
    {
        public StudentReportsService(HttpClient httpClient) : base(httpClient, "/v1/StudentReport")
        {
        }
    }
}