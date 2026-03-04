using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Tenants.Domain.Abstractions.Services
{
    public interface IFeaturesService
    {
        Task<IEnumerable<FeatureDto>> GetAllAsync();

        Task Migrate();
    }
}