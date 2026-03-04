using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Certificates;

namespace OnHive.Certificates.Domain.Abstractions.Repositories
{
    public interface ICertificateMountsRepository : IRepositoryBase<CertificateMount>
    {
        Task<PaginatedResult<CertificateMount>> GetByFilterUserAsync(RequestFilter filter, string userId, string? tenantId);

        Task<CertificateMount> GetByKeyAsync(string certificateKey);

        Task<string> CertificateExistsAsync(string studentId, string courseId, string tenantId);
    }
}