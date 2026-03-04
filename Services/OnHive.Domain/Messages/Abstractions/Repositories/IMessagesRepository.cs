using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Messages;

namespace OnHive.Messages.Domain.Abstractions.Repositories
{
    public interface IMessagesRepository : IRepositoryBase<Message>
    {
        Task<List<Message>> GetByFromAsync(string from, string origin, string tenantId);
    }
}