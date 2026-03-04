using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.SystemParameters;

namespace OnHive.SystemParameters.Domain.Abstractions.Repositories
{
    public interface ISystemParametersRepository : IRepositoryBase<SystemParameter>
    {
        Task<IEnumerable<SystemParameter>> GetAllAsync();

        Task<IEnumerable<SystemParameter>> GetByGroupAsync(string group);
    }
}