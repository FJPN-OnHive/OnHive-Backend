using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Certificates;

namespace EHive.Certificates.Domain.Abstractions.Repositories
{
    public interface ICertificateMountsRepository : IRepositoryBase<CertificateMount>
    {
        Task<PaginatedResult<CertificateMount>> GetByFilterUserAsync(RequestFilter filter, string userId, string? tenantId);

        Task<CertificateMount> GetByKeyAsync(string certificateKey);

        Task<string> CertificateExistsAsync(string studentId, string courseId, string tenantId);
    }
}