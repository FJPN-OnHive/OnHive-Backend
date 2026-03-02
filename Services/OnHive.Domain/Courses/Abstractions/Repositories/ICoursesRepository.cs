using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Courses;

namespace EHive.Courses.Domain.Abstractions.Repositories
{
    public interface ICoursesRepository : IRepositoryBase<Course>
    {
        Task<List<Course>> GetAllActive(string tenantId);

        Task<PaginatedResult<Course>> GetByFilterAndUserAsync(List<string> ids, RequestFilter filter, string? tenantId);
    }
}