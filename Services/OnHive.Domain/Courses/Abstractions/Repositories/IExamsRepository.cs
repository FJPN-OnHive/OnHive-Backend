using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Courses;

namespace EHive.Courses.Domain.Abstractions.Repositories
{
    public interface IExamsRepository : IRepositoryBase<Exam>
    {
        Task<List<Exam>> GetAllIdMigrate();
    }
}