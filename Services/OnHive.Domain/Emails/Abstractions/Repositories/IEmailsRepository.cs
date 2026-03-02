using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Emails;

namespace EHive.Emails.Domain.Abstractions.Repositories
{
    public interface IEmailsRepository : IRepositoryBase<EmailTemplate>
    {
        Task<EmailTemplate> GetByCodeAsync(string templateCode, string tenantId);
    }
}