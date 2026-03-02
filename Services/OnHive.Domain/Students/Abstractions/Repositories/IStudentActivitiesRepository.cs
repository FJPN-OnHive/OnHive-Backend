using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Students;

namespace EHive.Students.Domain.Abstractions.Repositories
{
    public interface IStudentActivitiesRepository : IRepositoryBase<StudentActivity>
    {
        Task<List<StudentActivity>> GetByCourseIdAsync(string courseId, string tenantID);

        Task<List<StudentActivity>> GetByCourseandLessonIdAsync(string courseId, string LessonId, string tenantID);

        Task<List<StudentActivity>> GetByCourseIdUserAsync(string userId, string courseId, string tenantID);

        Task<List<StudentActivity>> GetByCourseandLessonIdUserAsync(string userId, string courseId, string LessonId, string tenantID);

        Task<List<StudentActivity>> GetAllByUserIdAsync(string? userId);
    }
}