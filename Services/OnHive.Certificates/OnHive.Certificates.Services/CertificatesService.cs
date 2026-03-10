using AutoMapper;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Entities.Certificates;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Certificates.Domain.Abstractions.Repositories;
using OnHive.Certificates.Domain.Abstractions.Services;
using OnHive.Certificates.Domain.Models;
using Serilog;
using System.Text.Json;
using OnHive.Core.Library.Validations.Common;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Abstractions.Repositories;

namespace OnHive.Certificates.Services
{
    public class CertificatesService : ICertificatesService
    {
        private readonly ICertificatesRepository certificatesRepository;
        private readonly ICertificateMountsRepository certificateMountsRepository;
        private readonly ITenantsRepository tenantsRepository;
        private readonly ICoursesRepository coursesRepository;
        private readonly CertificatesApiSettings certificatesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public CertificatesService(ICertificatesRepository certificatesRepository,
                                   ICertificateMountsRepository certificateMountsRepository,
                                   CertificatesApiSettings certificatesApiSettings,
                                   IMapper mapper,
                                   ITenantsRepository tenantsRepository,
                                   ICoursesRepository coursesRepository)
        {
            this.certificatesRepository = certificatesRepository;
            this.certificateMountsRepository = certificateMountsRepository;
            this.certificatesApiSettings = certificatesApiSettings;
            this.mapper = mapper;
            this.tenantsRepository = tenantsRepository;
            this.coursesRepository = coursesRepository;
            logger = Log.Logger;
        }

        public async Task<string> EmmitCertificate(CertificateEmissionRequestDto certificateRequest, string hostUrl)
        {
            if (string.IsNullOrEmpty(certificateRequest.Student.TenantId))
            {
                throw new ArgumentException("Student TenantId is required");
            }

            var certificateTemplate = await certificatesRepository.GetByIdAsync(certificateRequest.CertificateID);
            if (certificateTemplate == null)
            {
                throw new ArgumentException("Certificate not found");
            }

            if (certificateTemplate.TenantId != certificateRequest.Student.TenantId)
            {
                throw new UnauthorizedAccessException("Unauthorized access to certificate");
            }

            var existingCertificateId = await certificateMountsRepository.CertificateExistsAsync(
                certificateRequest.Student.Id,
                certificateRequest.Course.Id,
                certificateRequest.Student.TenantId);

            if (!string.IsNullOrEmpty(existingCertificateId))
            {
                logger.Warning($"Certificate already exists for this student and course, re-emminting: {certificateRequest.Student.Id} - {certificateRequest.Course.Id}");
                return await ReEmmitCertificate(existingCertificateId, certificateTemplate, certificateRequest);
            }

            var certificate = new CertificateMount
            {
                CertificateId = certificateTemplate.Id,
                StudentId = certificateRequest.Student.Id,
                CourseId = certificateRequest.Course.Id,
                CourseName = certificateRequest.Course.Name ?? string.Empty,
                TenantId = certificateTemplate.TenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmissionDate = certificateRequest.EmissionDate ?? DateTime.UtcNow,
                CertificateKey = Guid.NewGuid().ToString(),
                ThumbnailUrl = certificateRequest.Course.ImageUrl,
                Title = certificateTemplate.Title,
                UserId = certificateRequest.User.Id,
                IsActive = true,
                Body = certificateTemplate.TemplateBody
            };

            certificate.ValidationUrl = $"www.{hostUrl}/v1/Certificate/Validate/{certificate.CertificateKey}";
            certificate = await ApplyTemplateAsync(certificate, certificateRequest);
            certificate = await certificateMountsRepository.SaveAsync(certificate);

            if (certificate == null)
            {
                throw new ArgumentException("Error saving certificate");
            }

            return certificate.Id;
        }

        private async Task<string> ReEmmitCertificate(string existingCertificateId, Certificate certificateTemplate, CertificateEmissionRequestDto certificateRequest)
        {
            var existingCertificate = await certificateMountsRepository.GetByIdAsync(existingCertificateId);

            if (existingCertificate == null)
            {
                return string.Empty;
            }

            existingCertificate.CertificateId = certificateTemplate.Id;
            existingCertificate.StudentId = certificateRequest.Student.Id;
            existingCertificate.CourseId = certificateRequest.Course.Id;
            existingCertificate.CourseName = certificateRequest.Course.Name ?? string.Empty;
            existingCertificate.TenantId = certificateTemplate.TenantId;
            existingCertificate.UpdatedAt = DateTime.UtcNow;
            existingCertificate.Title = certificateTemplate.Title;
            existingCertificate.UserId = certificateRequest.User.Id;
            existingCertificate.IsActive = true;
            existingCertificate.Body = certificateTemplate.TemplateBody;
            certificateRequest.EmissionDate = existingCertificate.EmissionDate;
            existingCertificate = await ApplyTemplateAsync(existingCertificate, certificateRequest);
            existingCertificate = await certificateMountsRepository.SaveAsync(existingCertificate);

            if (existingCertificate == null)
            {
                throw new ArgumentException("Error saving certificate");
            }

            return existingCertificate.Id;
        }

        private async Task<CertificateMount> ApplyTemplateAsync(CertificateMount certificate, CertificateEmissionRequestDto certificateRequest)
        {
            var course = await GetCourse(certificateRequest.Course.Id);
            if (course == null)
            {
                throw new ArgumentException("Course not found");
            }
            var tenant = await GetTenant(certificateRequest.Student.TenantId);
            if (tenant == null)
            {
                throw new ArgumentException("Tenant not found");
            }
            var fieldsData = GetFieldsDict(certificateRequest, course, tenant);
            foreach (var item in fieldsData)
            {
                certificate.Title = certificate.Title.Replace($"{{{item.Key}}}", item.Value);
                certificate.Body = certificate.Body.Replace($"{{{item.Key}}}", item.Value);
            }
            return certificate;
        }

        private async Task<TenantDto> GetTenant(string tenantId)
        {
            var tenant = await tenantsRepository.GetByIdAsync(tenantId) ?? throw new ArgumentException("Tenant not found");
            return mapper.Map<TenantDto>(tenant);
        }

        private async Task<CourseDto> GetCourse(string courseId)
        {
            var course = await coursesRepository.GetByIdAsync(courseId) ?? throw new ArgumentException("Course not found");
            return mapper.Map<CourseDto>(course);
        }

        private Dictionary<string, string> GetFieldsDict(CertificateEmissionRequestDto certificateRequest, CourseDto course, TenantDto tenant)
        {
            var fields = new Dictionary<string, string>
            {
                { "Student.Code", certificateRequest.Student.Code },
                { "Student.Name", certificateRequest.User.Name },
                { "Student.Surname", certificateRequest.User.Surname },
                { "Student.SocialName", certificateRequest.User.SocialName ?? string.Empty },
                { "Student.Email", certificateRequest.User.MainEmail },
                { "Student.Phone", certificateRequest.User.PhoneNumber ?? string.Empty },
                { "Course.Id", course.Id},
                { "Course.Name", course.Name ?? string.Empty},
                { "Course.Description", course.Description ?? string.Empty},
                { "Course.EnrollmentCode", certificateRequest.Course.EnrollmentCode ?? string.Empty},
                { "Course.Progress", certificateRequest.Course.Progress.ToString()},
                { "Course.StartDate", certificateRequest.Course.StartTime.ToLocalTime().ToString("dd/MM/yyyy")},
                { "Course.EndDate", certificateRequest.Course.EndTime.ToLocalTime().ToString("dd/MM/yyyy")},
                { "Course.StartTime", certificateRequest.Course.StartTime.ToLocalTime().ToString("t")},
                { "Course.EndTime", certificateRequest.Course.EndTime.ToLocalTime().ToString("t")},
                { "Course.Thumbnail", certificateRequest.Course.Thumbnail ?? string.Empty },
                { "Course.Duration", course.Duration.ToString() ?? string.Empty },
                { "Tenant.Name", tenant.Name ?? string.Empty },
                { "Tenant.Email", tenant.Email ?? string.Empty },
                { "Tenant.CNPJ", tenant.CNPJ ?? string.Empty }
            };

            if (certificateRequest.User.Documents != null && certificateRequest.User.Documents.Any() && certificateRequest.User.Documents.FirstOrDefault() != null)
            {
                fields.Add("Student.DocumentType", certificateRequest.User.Documents[0].DocumentType);
                fields.Add("Student.Document", certificateRequest.User.Documents[0].DocumentNumber);
            }
            else
            {
                fields.Add("Student.DocumentType", string.Empty);
                fields.Add("Student.Document", string.Empty);
            }

            return fields;
        }

        public async Task<CertificateMountPublicDto> ValidateCertificate(string certificateKey)
        {
            var certificate = await certificateMountsRepository.GetByKeyAsync(certificateKey);
            if (certificate == null)
            {
                throw new ArgumentException("Certificate not found");
            }
            return mapper.Map<CertificateMountPublicDto>(certificate);
        }

        public async Task<CertificateMountDto> GetEmmitedByIdAsync(string certificateId)
        {
            var certificate = await certificateMountsRepository.GetByIdAsync(certificateId);
            return mapper.Map<CertificateMountDto>(certificate);
        }

        public async Task<PaginatedResult<CertificateMountDto>> GetEmmitedByFilterAsync(RequestFilter filter, UserDto user)
        {
            var result = await certificateMountsRepository.GetByFilterAsync(filter, user?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<CertificateMountDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<CertificateMountDto>>(result.Itens)
                };
            }
            return new PaginatedResult<CertificateMountDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CertificateMountDto>()
            };
        }

        public async Task<PaginatedResult<CertificateMountDto>> GetEmmitedByFilterUserAsync(RequestFilter filter, UserDto user)
        {
            var result = await certificateMountsRepository.GetByFilterUserAsync(filter, user.Id, user?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<CertificateMountDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<CertificateMountDto>>(result.Itens)
                };
            }
            return new PaginatedResult<CertificateMountDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CertificateMountDto>()
            };
        }

        public async Task<CertificateDto?> GetByIdAsync(string CertificateId)
        {
            var Certificate = await certificatesRepository.GetByIdAsync(CertificateId);
            return mapper.Map<CertificateDto>(Certificate);
        }

        public async Task<PaginatedResult<CertificateDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await certificatesRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<CertificateDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<CertificateDto>>(result.Itens)
                };
            }
            return new PaginatedResult<CertificateDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CertificateDto>()
            };
        }

        public async Task<IEnumerable<CertificateDto>> GetAllAsync(UserDto? loggedUser)
        {
            var Certificates = await certificatesRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<CertificateDto>>(Certificates);
        }

        public async Task<CertificateDto> SaveAsync(CertificateDto CertificateDto, UserDto? loggedUser)
        {
            var Certificate = mapper.Map<Certificate>(CertificateDto);
            ValidatePermissions(Certificate, loggedUser);
            Certificate.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            Certificate.CreatedAt = DateTime.UtcNow;
            Certificate.IsActive = true;
            Certificate.CreatedBy = string.IsNullOrEmpty(Certificate.CreatedBy) ? loggedUser.Id : Certificate.CreatedBy;

            var response = await certificatesRepository.SaveAsync(Certificate);
            return mapper.Map<CertificateDto>(response);
        }

        public async Task<CertificateDto> CreateAsync(CertificateDto CertificateDto, UserDto loggedUser)
        {
            var Certificate = mapper.Map<Certificate>(CertificateDto);
            ValidatePermissions(Certificate, loggedUser);
            Certificate.Id = string.Empty;
            Certificate.TenantId = loggedUser.TenantId;
            Certificate.IsActive = true;
            var response = await certificatesRepository.SaveAsync(Certificate, loggedUser.Id);
            return mapper.Map<CertificateDto>(response);
        }

        public async Task<CertificateDto?> UpdateAsync(CertificateDto CertificateDto, UserDto loggedUser)
        {
            var Certificate = mapper.Map<Certificate>(CertificateDto);
            ValidatePermissions(Certificate, loggedUser);
            var currentCertificate = await certificatesRepository.GetByIdAsync(Certificate.Id);
            if (currentCertificate == null || currentCertificate.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            Certificate.IsActive = true;
            var response = await certificatesRepository.SaveAsync(Certificate, loggedUser.Id);
            return mapper.Map<CertificateDto>(response);
        }

        public async Task<CertificateDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentCertificate = await certificatesRepository.GetByIdAsync(patch.GetId());
            if (currentCertificate == null || currentCertificate.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentCertificate = patch.PatchEntity(currentCertificate);
            ValidatePermissions(currentCertificate, loggedUser);
            if (!mapper.Map<CertificateDto?>(currentCertificate).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await certificatesRepository.SaveAsync(currentCertificate, loggedUser.Id);
            return mapper.Map<CertificateDto>(response);
        }

        private void ValidatePermissions(Certificate Certificate, UserDto? loggedUser)
        {
            if (loggedUser != null && Certificate.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Certificate/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    Certificate.Id, Certificate.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}