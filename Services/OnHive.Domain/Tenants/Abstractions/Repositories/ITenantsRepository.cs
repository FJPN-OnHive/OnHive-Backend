using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Tenants;

namespace EHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantsRepository : IRepositoryBase<Tenant>
    {
        Task<List<Tenant>> GetAllAsync();

        Task<Tenant?> GetByDomainAsync(string domain);

        Task<Tenant> GetBySlugAsync(string slug);
    }
}