using OnHive.Core.Library.Contracts.Messages;

namespace OnHive.Admin.Services
{
    public class MessageChannelsService : ServiceBase<MessageChannelDto>, IMessageChannelsService
    {
        public MessageChannelsService(HttpClient httpClient) : base(httpClient, "/v1/MessageChannel")
        {
        }
    }
}