using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Redirects;

namespace OnHive.Redirects.Domain.Abstractions.Repositories
{
    public interface IRedirectRepository : IRepositoryBase<Redirect>
    {
        Task<bool> DeleteAsync(string redirectId, string id);

        Task<List<Redirect>> GetAllActive(string tenantId);

        Task<Redirect> GetByPathAsync(string tenantId, string path);
    }
}