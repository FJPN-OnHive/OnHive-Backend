using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using System.Text.Json;

namespace OnHive.Certificates.Domain.Abstractions.Services
{
    public interface ICertificatesService
    {
        Task<CertificateDto?> GetByIdAsync(string CertificateId);

        Task<PaginatedResult<CertificateDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser);

        Task<IEnumerable<CertificateDto>> GetAllAsync(UserDto? loggedUser);

        Task<CertificateDto> SaveAsync(CertificateDto CertificateDto, UserDto? user);

        Task<CertificateDto> CreateAsync(CertificateDto CertificateDto, UserDto loggedUser);

        Task<CertificateDto?> UpdateAsync(CertificateDto CertificateDto, UserDto loggedUser);

        Task<CertificateDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser);

        Task<string> EmmitCertificate(CertificateEmissionRequestDto certificateRequest, string hostUrl);

        Task<CertificateMountPublicDto> ValidateCertificate(string certificateKey);

        Task<CertificateMountDto> GetEmmitedByIdAsync(string certificateId);

        Task<PaginatedResult<CertificateMountDto>> GetEmmitedByFilterAsync(RequestFilter filter, UserDto user);

        Task<PaginatedResult<CertificateMountDto>> GetEmmitedByFilterUserAsync(RequestFilter filter, UserDto user);
    }
}