using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Tenants;

namespace EHive.Tenants.Domain.Abstractions.Repositories
{
    public interface IFeaturesRepository : IRepositoryBase<SystemFeatures>
    {
        Task<SystemFeatures> GetAsync();
    }
}