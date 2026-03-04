using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Users;
using System.Text.Json;

namespace OnHive.Emails.Domain.Abstractions.Services
{
    public interface IEmailsService
    {
        Task<EmailTemplateDto?> GetByIdAsync(string emailTemplateId);

        Task<PaginatedResult<EmailTemplateDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<EmailTemplateDto>> GetAllAsync(UserDto? loggedUser);

        Task<EmailTemplateDto> SaveAsync(EmailTemplateDto emailTemplateDto, UserDto? user);

        Task<EmailTemplateDto> CreateAsync(EmailTemplateDto emailTemplateDto, UserDto loggedUser);

        Task<EmailTemplateDto?> UpdateAsync(EmailTemplateDto emailTemplateDto, UserDto loggedUser);

        Task<EmailTemplateDto?> PatchAsync(JsonDocument patch, UserDto loggedUser);

        Task<bool> DeleteAsync(string id, UserDto loggedUser);

        Task ComposeEmail(EmailSendDto message);

        Task Migrate();
    }
}