using AutoMapper;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Entities.Certificates;

namespace OnHive.Certificates.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapCertificateToCertificateDto();
            MapCertificateMountToCertificateMountDto();
        }

        private void MapCertificateToCertificateDto()
        {
            CreateMap<Certificate, CertificateDto>()
                .ReverseMap();
        }

        private void MapCertificateMountToCertificateMountDto()
        {
            CreateMap<CertificateMount, CertificateMountDto>()
                .ReverseMap();

            CreateMap<CertificateMount, CertificateMountPublicDto>()
                .ReverseMap();
        }
    }
}
