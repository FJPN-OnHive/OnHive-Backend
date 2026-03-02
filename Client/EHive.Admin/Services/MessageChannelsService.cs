using EHive.Core.Library.Contracts.Messages;

namespace EHive.Admin.Services
{
    public class MessageChannelsService : ServiceBase<MessageChannelDto>, IMessageChannelsService
    {
        public MessageChannelsService(HttpClient httpClient) : base(httpClient, "/v1/MessageChannel")
        {
        }
    }
}