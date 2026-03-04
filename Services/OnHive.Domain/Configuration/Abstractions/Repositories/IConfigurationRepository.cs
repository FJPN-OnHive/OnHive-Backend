using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Configuration;

namespace OnHive.Configuration.Domain.Abstractions.Repositories
{
    public interface IConfigurationRepository : IRepositoryBase<ConfigItem>
    {
        Task<ConfigItem> GetByKeyAsync(string key);
    }
}