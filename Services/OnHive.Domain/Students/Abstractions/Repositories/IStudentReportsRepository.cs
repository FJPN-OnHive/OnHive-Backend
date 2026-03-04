using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Students;

namespace OnHive.Students.Domain.Abstractions.Repositories
{
    public interface IStudentReportsRepository : IRepositoryBase<StudentReport>
    {
    }
}