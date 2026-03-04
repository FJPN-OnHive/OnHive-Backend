using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Messages;

namespace OnHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessageUsersRepository : IRepositoryBase<MessageUser>
    {
        Task<IEnumerable<MessageUser>> GetByMessageAsync(string messageId, string tenantId);

        Task<IEnumerable<MessageUser>> GetByUserAsync(string userId, bool newOnly, string tenantId);

        Task<PaginatedResult<MessageUser>> GetByUserFilterAsync(RequestFilter filter, string userId, string tenantId);
    }
}