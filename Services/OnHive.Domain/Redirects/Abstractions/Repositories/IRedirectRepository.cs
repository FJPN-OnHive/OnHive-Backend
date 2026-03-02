using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Redirects;

namespace EHive.Redirects.Domain.Abstractions.Repositories
{
    public interface IRedirectRepository : IRepositoryBase<Redirect>
    {
        Task<bool> DeleteAsync(string redirectId, string id);

        Task<List<Redirect>> GetAllActive(string tenantId);

        Task<Redirect> GetByPathAsync(string tenantId, string path);
    }
}