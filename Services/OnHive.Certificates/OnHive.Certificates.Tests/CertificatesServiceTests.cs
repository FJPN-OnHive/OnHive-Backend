using AutoMapper;
using OnHive.Certificates.Domain.Abstractions.Repositories;
using OnHive.Certificates.Domain.Mappers;
using OnHive.Certificates.Domain.Models;
using OnHive.Certificates.Services;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Certificates;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace OnHive.Certificates.Tests
{
    public class CertificatesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ICertificatesRepository> mockCertificatesRepository;
        private readonly Mock<ICertificateMountsRepository> mockCertificateMountsRepository;
        private readonly Mock<ITenantsService> mockTenantsService;
        private readonly Mock<ICoursesService> mockCoursesService;
        private readonly Mock<IServicesHub> mockServicesHub;

        private readonly CertificatesApiSettings certificatesApiSettings;
        private readonly IMapper mapper;

        public CertificatesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockCertificatesRepository = mockRepository.Create<ICertificatesRepository>();
            mockCertificateMountsRepository = mockRepository.Create<ICertificateMountsRepository>();
            mockCoursesService = mockRepository.Create<ICoursesService>();
            mockTenantsService = mockRepository.Create<ITenantsService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.CoursesService).Returns(mockCoursesService.Object);
            mockServicesHub.SetupGet(s => s.TenantsService).Returns(mockTenantsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            certificatesApiSettings = new CertificatesApiSettings();
            certificatesApiSettings.CertificatesAdminPermission = "certificates_admin";
        }

        [Fact]
        public async Task GetCertificates_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Certificate>
            {
                new Certificate
                {
                    TenantId = user.TenantId
                }
            };

            mockCertificatesRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockCertificatesRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCertificateByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Certificate>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
                }
            };

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = testUser.TenantId
                    }
                }
            };

            mockCertificatesRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Certificate> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CertificateDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockCertificatesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCertificatesByFilter_NotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = testUser.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CertificateDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockCertificatesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCertificate_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockCertificatesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockCertificatesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveCertificate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CertificateDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockCertificatesRepository.Setup(r => r.SaveAsync(It.IsAny<Certificate>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CertificateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCertificatesRepository.Verify(r => r.SaveAsync(It.IsAny<Certificate>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateCertificate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CertificateDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockCertificatesRepository.Setup(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CertificateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCertificatesRepository.Verify(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCertificate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CertificateDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockCertificatesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCertificatesRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CertificateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCertificatesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCertificatesRepository.Verify(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCertificateUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "22222222222222"
            };

            var input = new CertificateDto
            {
                Id = expected.Id,
                TenantId = expected.TenantId
            };

            mockCertificatesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCertificatesRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            try
            {
                _ = await service.UpdateAsync(input, testUser);
                Assert.Fail("Should throw UnauthorizedAccessException");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<UnauthorizedAccessException>();
            }

            // Assert
            mockCertificatesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockCertificatesRepository.Verify(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id), Times.Never);
        }

        [Fact]
        public async Task PatchCertificate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Certificate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CertificateDto
            {
                Id = expected.Id,
                TenantId = testUser!.TenantId,
                Code = "Test"
            };

            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockCertificatesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCertificatesRepository.Setup(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CertificateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCertificatesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCertificatesRepository.Verify(r => r.SaveAsync(It.IsAny<Certificate>(), testUser.Id), Times.Once);
        }

        private CertificatesService CreateService()
        {
            return new CertificatesService(
                mockCertificatesRepository.Object,
                mockCertificateMountsRepository.Object,
                certificatesApiSettings,
                mapper,
                mockServicesHub.Object);
        }

        private UserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "staff", "certificates_admin"],
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            };
        }
    }
}