using OnHive.Core.Library.Contracts.Certificates;

namespace OnHive.Admin.Services
{
    public interface ICertificatesService : IServiceBase<CertificateDto>
    {
        Task<CertificateMountDto?> GetEmmitedCertificateById(string certificateId, string token);
    }
}