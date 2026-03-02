using EHive.Core.Library.Contracts.Certificates;

namespace EHive.Admin.Services
{
    public interface ICertificatesService : IServiceBase<CertificateDto>
    {
        Task<CertificateMountDto?> GetEmmitedCertificateById(string certificateId, string token);
    }
}