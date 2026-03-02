using AutoMapper;
using EHive.Core.Library.Contracts.Emails;
using EHive.Core.Library.Entities.Emails;

namespace EHive.Emails.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapEmailTemplateToEmailTemplateDto();
        }

        private void MapEmailTemplateToEmailTemplateDto()
        {
            CreateMap<EmailTemplate, EmailTemplateDto>()
                .ReverseMap();
        }
    }
}