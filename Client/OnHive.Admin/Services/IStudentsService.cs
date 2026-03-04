using OnHive.Admin.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Students;

namespace OnHive.Admin.Services
{
    public interface IStudentsService : IServiceBase<StudentDto>
    {
        Task<PaginatedResult<StudentUserDto>> GetStudentUsersPaginated(RequestFilter filter, string token);

        Task<StudentDto> EnrollAsync(EnrollmentMessage enrollmentMessage, string token);

        Task<StudentDto> UnenrollAsync(string userId, string courseId, string token);

        Task<List<SyntheticEnrollmentDto>> GetEnrollmentsSyntheticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, string token);

        Task<StudentReportDto> GetEnrollmentsAnalyticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, string token);

        Task<StudentReportDto> GetSurveyAnalyticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, bool isSatisfaction, string token);

        Task ReemitCertificateAsync(string courseId, string userId, string token);
    }
}