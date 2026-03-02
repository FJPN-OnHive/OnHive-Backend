using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Configuration;

namespace EHive.Configuration.Domain.Abstractions.Repositories
{
    public interface IConfigurationRepository : IRepositoryBase<ConfigItem>
    {
        Task<ConfigItem> GetByKeyAsync(string key);
    }
}