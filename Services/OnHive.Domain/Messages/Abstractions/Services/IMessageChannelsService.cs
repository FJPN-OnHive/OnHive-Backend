using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Messages;
using EHive.Core.Library.Contracts.Login;

namespace EHive.Messages.Domain.Abstractions.Services
{
    public interface IMessageChannelsService
    {
        Task<MessageChannelDto?> GetByIdAsync(string messageChannelId);

        Task<PaginatedResult<MessageChannelDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<MessageChannelDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<MessageChannelDto> SaveAsync(MessageChannelDto messageChannelDto, LoggedUserDto? user);

        Task<MessageChannelDto> CreateAsync(MessageChannelDto messageChannelDto, LoggedUserDto? loggedUser);

        Task<MessageChannelDto?> UpdateAsync(MessageChannelDto messageChannelDto, LoggedUserDto? loggedUser);

        Task<bool> DeleteAsync(string messageId, LoggedUserDto loggedUser);

        Task<List<string>> GetByTenant(RequestFilter filter, string tenantId);
    }
}