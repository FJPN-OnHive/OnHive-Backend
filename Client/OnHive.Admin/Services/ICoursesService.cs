using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Admin.Services
{
    public interface ICoursesService : IServiceBase<CourseDto>
    {
        Task<List<CourseResumeDto>> GetAllResume(string tenantId, string token);

        string GetExportCoursesUrl(ExportFormats exportType, string tenantId, bool activeOnly);
    }
}