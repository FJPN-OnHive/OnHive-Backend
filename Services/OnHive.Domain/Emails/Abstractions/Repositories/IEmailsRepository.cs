using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Emails;

namespace OnHive.Emails.Domain.Abstractions.Repositories
{
    public interface IEmailsRepository : IRepositoryBase<EmailTemplate>
    {
        Task<EmailTemplate> GetByCodeAsync(string templateCode, string tenantId);
    }
}