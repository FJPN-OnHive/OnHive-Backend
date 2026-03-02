using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Messages;

namespace EHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessagesRepository : IRepositoryBase<Message>
    {
        Task<List<Message>> GetByFromAsync(string from, string origin, string tenantId);
    }
}