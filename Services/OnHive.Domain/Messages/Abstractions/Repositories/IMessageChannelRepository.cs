using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Messages;

namespace EHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessageChannelRepository : IRepositoryBase<MessageChannel>
    {
        Task<MessageChannel> GetByCodeAsync(string channelCode, string tenantId);
    }
}