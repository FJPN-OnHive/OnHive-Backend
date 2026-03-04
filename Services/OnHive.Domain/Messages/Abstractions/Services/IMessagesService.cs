using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Contracts.Login;
using System.Text.Json;

namespace OnHive.Messages.Domain.Abstractions.Services
{
    public interface IMessagesService
    {
        Task<MessageDto?> GetByIdAsync(string messageId, LoggedUserDto loggedUser);

        Task<PaginatedResult<MessageDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<MessageDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<MessageDto> SaveAsync(MessageDto messageDto, LoggedUserDto? user);

        Task<MessageDto> CreateAsync(MessageDto messageDto, LoggedUserDto? loggedUser);

        Task<MessageDto?> UpdateAsync(MessageDto messageDto, LoggedUserDto? loggedUser);

        Task SendMessageAsync(MessageDto messageDto);

        Task<MessageDto> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<List<MessageUserDto>> GetListByUserAsync(LoggedUserDto loggedUser, bool newOnly);

        Task<MessageUserDto> PatchUserMessageAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<MessageUserDto> UpdateUserMessageAsync(MessageUserDto messageDto, LoggedUserDto loggedUser);
    }
}