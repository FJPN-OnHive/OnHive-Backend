using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.SystemParameters;

namespace EHive.SystemParameters.Domain.Abstractions.Repositories
{
    public interface ISystemParametersRepository : IRepositoryBase<SystemParameter>
    {
        Task<IEnumerable<SystemParameter>> GetAllAsync();

        Task<IEnumerable<SystemParameter>> GetByGroupAsync(string group);
    }
}