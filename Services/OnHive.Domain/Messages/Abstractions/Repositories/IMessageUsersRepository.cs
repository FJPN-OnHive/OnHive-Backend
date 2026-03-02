using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Messages;

namespace EHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessageUsersRepository : IRepositoryBase<MessageUser>
    {
        Task<IEnumerable<MessageUser>> GetByMessageAsync(string messageId, string tenantId);

        Task<IEnumerable<MessageUser>> GetByUserAsync(string userId, bool newOnly, string tenantId);

        Task<PaginatedResult<MessageUser>> GetByUserFilterAsync(RequestFilter filter, string userId, string tenantId);
    }
}