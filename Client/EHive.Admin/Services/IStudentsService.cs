using EHive.Admin.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Students;

namespace EHive.Admin.Services
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