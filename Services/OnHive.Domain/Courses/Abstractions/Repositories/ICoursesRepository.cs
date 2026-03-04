using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Courses;

namespace OnHive.Courses.Domain.Abstractions.Repositories
{
    public interface ICoursesRepository : IRepositoryBase<Course>
    {
        Task<List<Course>> GetAllActive(string tenantId);

        Task<PaginatedResult<Course>> GetByFilterAndUserAsync(List<string> ids, RequestFilter filter, string? tenantId);
    }
}