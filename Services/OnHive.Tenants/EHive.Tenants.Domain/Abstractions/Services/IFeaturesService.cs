using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Tenants.Domain.Abstractions.Services
{
    public interface IFeaturesService
    {
        Task<IEnumerable<FeatureDto>> GetAllAsync();

        Task Migrate();
    }
}