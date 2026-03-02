using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Enums.Common;

namespace EHive.Admin.Services
{
    public interface ICoursesService : IServiceBase<CourseDto>
    {
        Task<List<CourseResumeDto>> GetAllResume(string tenantId, string token);

        string GetExportCoursesUrl(ExportFormats exportType, string tenantId, bool activeOnly);
    }
}