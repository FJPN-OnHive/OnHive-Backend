using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Tenants;

namespace OnHive.Tenants.Domain.Abstractions.Repositories
{
    public interface ITenantsRepository : IRepositoryBase<Tenant>
    {
        Task<List<Tenant>> GetAllAsync();

        Task<Tenant?> GetByDomainAsync(string domain);

        Task<Tenant> GetBySlugAsync(string slug);
    }
}