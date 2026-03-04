using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Messages;

namespace OnHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessageChannelRepository : IRepositoryBase<MessageChannel>
    {
        Task<MessageChannel> GetByCodeAsync(string channelCode, string tenantId);
    }
}