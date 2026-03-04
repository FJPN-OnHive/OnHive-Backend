using AutoMapper;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Mappers;
using OnHive.Tenants.Services;
using FluentAssertions;
using Moq;

namespace OnHive.Tenants.Tests
{
    public class FeaturesServiceTests
    {
        private readonly IMapper mapper;
        private MockRepository mockRepository;
        private Mock<IFeaturesRepository> mockFeaturesRepository;

        public FeaturesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockFeaturesRepository = mockRepository.Create<IFeaturesRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
        }

        [Fact]
        public async Task GetAllFeatures()
        {
            // Arrange
            var service = CreateService();

            var expected = new List<SystemFeatures>
            {
                new SystemFeatures
                {
                    Features = new List<Feature>
                    {
                        new Feature
                            {
                                Key = "FEAT1",
                                Name = "Test",
                                Description = "Test",
                                Service = "Teste"
                            },
                        new Feature
                            {
                                Key = "FEAT2",
                                Name = "Test2",
                                Description = "Test2",
                                Service = "Test2"
                            }
                    }
                }
            };
            mockFeaturesRepository.Setup(r => r.GetAllAsync(It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(expected[0].Features.Count());
            result.Should().SatisfyRespectively(
                f => f.Key.Should().Be(expected[0].Features[0].Key),
                f => f.Key.Should().Be(expected[0].Features[1].Key));
            mockFeaturesRepository.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task MigrateFeatures()
        {
            // Arrange
            var service = this.CreateService();
            var expected = new SystemFeatures
            {
                Features = new List<Feature>
                {
                    new Feature
                    {
                        Key = "FEAT1",
                        Name = "Test",
                        Description = "Test",
                        Service = "Teste"
                    },
                    new Feature
                    {
                        Key = "FEAT2",
                        Name = "Test2",
                        Description = "Test2",
                        Service = "Test2"
                    }
                }
            };

            mockFeaturesRepository.Setup(r => r.GetAsync()).ReturnsAsync(expected);
            mockFeaturesRepository.Setup(r => r.SaveAsync(It.IsAny<SystemFeatures>(), It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            await service.Migrate();

            // Assert
            mockFeaturesRepository.Verify(r => r.GetAsync(), Times.Once);
            mockFeaturesRepository.Verify(r => r.SaveAsync(It.IsAny<SystemFeatures>(), It.IsAny<string>()), Times.Once);
        }

        private FeaturesService CreateService()
        {
            return new FeaturesService(
                mockFeaturesRepository.Object,
                mapper);
        }

        private UserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsValidated = true, IsMain = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "tenants_admin", "tenant_create", "staff"],
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