using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Entities.Students;
using EHive.Students.Domain.Models;

namespace EHive.Students.Domain.Abstractions.Repositories
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