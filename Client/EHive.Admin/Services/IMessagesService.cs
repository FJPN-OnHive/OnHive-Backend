using EHive.Core.Library.Contracts.Messages;

namespace EHive.Admin.Services
{
    public interface IMessagesService : IServiceBase<MessageDto>
    {
        Task<List<MessageUserDto>> GetAllByUser(bool newOnly, string token);

        Task<MessageUserDto> SaveUserMessageAsync(MessageUserDto dto, string token);

        Task<bool> SendMessageAsync(MessageDto message);
    }
}