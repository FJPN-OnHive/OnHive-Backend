using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Orders;

namespace EHive.Orders.Domain.Abstractions.Repositories
{
    public interface ICartsRepository : IRepositoryBase<Cart>
    {
        Task<IEnumerable<Cart>> GetByUserIdAsync(string id, string tenantId);
    }
}