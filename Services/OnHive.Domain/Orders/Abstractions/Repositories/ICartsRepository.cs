using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Orders;

namespace OnHive.Orders.Domain.Abstractions.Repositories
{
    public interface ICartsRepository : IRepositoryBase<Cart>
    {
        Task<IEnumerable<Cart>> GetByUserIdAsync(string id, string tenantId);
    }
}