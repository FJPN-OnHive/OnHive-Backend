using AutoMapper;
using EHive.Core.Library.Contracts.Messages;
using EHive.Core.Library.Entities.Messages;

namespace EHive.Messages.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapMessageToMessageDto();
            MapMessageUserToMessageUserDto();
            MapMessageChannelToMessageChannelDto();
        }

        private void MapMessageToMessageDto()
        {
            CreateMap<MessageFrom, MessageFromDto>()
                .ReverseMap();

            CreateMap<Message, MessageDto>()
                .ReverseMap();
        }

        private void MapMessageUserToMessageUserDto()
        {
            CreateMap<MessageUser, MessageUserDto>()
                .ReverseMap();
        }

        private void MapMessageChannelToMessageChannelDto()
        {
            CreateMap<MessageChannel, MessageChannelDto>()
                .ReverseMap();
        }
    }
}