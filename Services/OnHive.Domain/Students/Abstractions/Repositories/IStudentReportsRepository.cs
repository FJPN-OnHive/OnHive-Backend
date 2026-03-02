using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Students;

namespace EHive.Students.Domain.Abstractions.Repositories
{
    public interface IStudentReportsRepository : IRepositoryBase<StudentReport>
    {
    }
}