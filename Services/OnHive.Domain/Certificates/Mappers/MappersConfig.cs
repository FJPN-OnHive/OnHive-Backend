using AutoMapper;
using EHive.Core.Library.Contracts.Certificates;
using EHive.Core.Library.Entities.Certificates;

namespace EHive.Certificates.Domain.Mappers
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
