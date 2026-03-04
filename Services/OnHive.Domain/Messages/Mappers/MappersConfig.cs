using AutoMapper;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Entities.Messages;

namespace OnHive.Messages.Domain.Mappers
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