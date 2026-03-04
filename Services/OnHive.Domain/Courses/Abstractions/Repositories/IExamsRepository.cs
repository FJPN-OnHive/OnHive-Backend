using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Courses;

namespace OnHive.Courses.Domain.Abstractions.Repositories
{
    public interface IExamsRepository : IRepositoryBase<Exam>
    {
        Task<List<Exam>> GetAllIdMigrate();
    }
}