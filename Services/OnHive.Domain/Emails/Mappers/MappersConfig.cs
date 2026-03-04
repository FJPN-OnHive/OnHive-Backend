using AutoMapper;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Entities.Emails;

namespace OnHive.Emails.Domain.Mappers
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