using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Entities.Students;
using OnHive.Students.Domain.Models;

namespace OnHive.Students.Domain.Abstractions.Repositories
{
    public interface IStudentsRepository : IRepositoryBase<Student>
    {
        Task<List<Student>> GetAllByUserIdAsync(string? userId);

        Task<List<Student>> GetByReportFilterAsync(EnrollmentReportFilter filter, string? tenantId);

        Task<Student> GetByStudentCodeAsync(string studentCode);

        Task<Student> GetByUserIdAsync(string userId);

        Task<IEnumerable<SyntheticEnrollmentDto>> GetSyntheticByReportFilterAsync(EnrollmentReportFilter filter, string tenantId);

        Task<IEnumerable<AnalyticEnrollment>> GetAnalyticByReportFilterAsync(EnrollmentReportFilter filter, string? tenantId);

        Task<IEnumerable<AnalyticSurvey>> GetSurveyReportByFilterAsync(EnrollmentReportFilter filter, string? tenantId, string type);

        Task<PaginatedResult<StudentUser>> GetStudentUserByFilter(RequestFilter filter, string? tenantId, bool activeOnly = true);
    }
}