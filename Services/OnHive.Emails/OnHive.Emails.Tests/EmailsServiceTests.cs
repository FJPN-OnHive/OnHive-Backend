using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Emails;
using OnHive.Emails.Domain.Abstractions.Repositories;
using OnHive.Emails.Domain.Mappers;
using OnHive.Emails.Domain.Models;
using OnHive.Emails.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace OnHive.Emails.Tests
{
    public class EmailsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IEmailsRepository> mockEmailsRepository;
        private readonly EmailsApiSettings emailsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public EmailsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockEmailsRepository = mockRepository.Create<IEmailsRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            emailsApiSettings = new EmailsApiSettings
            {
                EmailServices = new List<EmailService>
                {
                    new EmailService
                    {
                        Server = "http://localhost",
                        Port = 1004,
                        IsDefault = true,
                        Key = "MOCK_EMAIL",
                        IsMock = true
                    }
                }
            };
            emailsApiSettings.EmailsAdminPermission = "emails_admin";
        }

        [Fact]
        public async Task Migration_Test()
        {
            // Arrange
            var service = CreateService();

            mockEmailsRepository.Setup(r => r.GetAllAsync(It.IsAny<string>())).ReturnsAsync(new List<EmailTemplate>());
            mockEmailsRepository.Setup(r => r.SaveAsync(It.IsAny<EmailTemplate>(), It.IsAny<string>())).ReturnsAsync(new EmailTemplate());

            // Act
            await service.Migrate();

            // Assert
            mockEmailsRepository.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.Is<EmailTemplate>(t => t.Code == "EMAIL_VALIDATION"), It.IsAny<string>()), Times.Once);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.Is<EmailTemplate>(t => t.Code == "PASSWORD_RECOVERY"), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ComposeEmail_Test()
        {
            // Arrange
            var service = CreateService();

            var emailSend = new EmailSendDto()
            {
                TenantId = "11111111111111",
                TemplateCode = "EMAIL_VALIDATION",
                AccountCode = "DEFAULT",
                ServiceCode = "DEFAULT",
                From = "test@test.com",
                SendTo = new List<string>() { "test_dest@test.com" },
                Attachments = new List<string>(),
                Fields = new Dictionary<string, string>() { { "CODE", "123456" }, { "NAME", "test name" } },
            };

            var template = new EmailTemplate
            {
                Code = "EMAIL_VALIDATION",
                Name = "Email validation",
                Description = "Email Validation",
                Body = "<h1>&CODE;</h1>",
                Subject = "validation &NAME;",
            };

            mockEmailsRepository.Setup(r => r.GetByCodeAsync(emailSend.TemplateCode, emailSend.TenantId)).ReturnsAsync(template);
            var request = mockHttpHandler.When(HttpMethod.Post, $"{emailsApiSettings.EmailServices[0].Server}:{emailsApiSettings.EmailServices[0].Port}/v1/internal/emailSend")
                .Respond(HttpStatusCode.OK);

            // Act
            await service.ComposeEmail(emailSend);

            // Assert
            mockEmailsRepository.Verify(r => r.GetByCodeAsync(emailSend.TemplateCode, emailSend.TenantId), Times.Once);
            mockHttpHandler.GetMatchCount(request).Should().Be(1);
        }

        [Fact]
        public async Task GetEmailTemplates_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<EmailTemplate>
            {
                new EmailTemplate
                {
                    TenantId = user.TenantId
                }
            };

            mockEmailsRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockEmailsRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetEmailTemplateByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<EmailTemplate>
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

            mockEmailsRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, false))
                .ReturnsAsync(new PaginatedResult<EmailTemplate> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<EmailTemplateDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockEmailsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetEmailTemplatesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<EmailTemplateDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockEmailsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetEmailTemplate_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockEmailsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockEmailsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveEmailTemplate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new EmailTemplateDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockEmailsRepository.Setup(r => r.SaveAsync(It.IsAny<EmailTemplate>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EmailTemplateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.IsAny<EmailTemplate>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateEmailTemplate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new EmailTemplateDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockEmailsRepository.Setup(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EmailTemplateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailTemplate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new EmailTemplateDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockEmailsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEmailsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EmailTemplateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEmailsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailTemplateUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "22222222222222"
            };

            var input = new EmailTemplateDto
            {
                Id = expected.Id,
                TenantId = expected.TenantId
            };

            mockEmailsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEmailsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockEmailsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id), Times.Never);
        }

        [Fact]
        public async Task PatchEmailTemplate_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new EmailTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var inputDto = new EmailTemplateDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId,
                Name = "Test"
            };

            var input = JsonDocument.Parse(JsonSerializer.Serialize(inputDto));

            mockEmailsRepository.Setup(r => r.GetByIdAsync(inputDto.Id))
               .ReturnsAsync(expected);

            mockEmailsRepository.Setup(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.PatchAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EmailTemplateDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEmailsRepository.Verify(r => r.GetByIdAsync(inputDto.Id), Times.Once);
            mockEmailsRepository.Verify(r => r.SaveAsync(It.IsAny<EmailTemplate>(), testUser.Id), Times.Once);
        }

        private EmailsService CreateService()
        {
            return new EmailsService(
                mockEmailsRepository.Object,
                emailsApiSettings,
                mapper,
                httpClient);
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
                Permissions = ["admin", "emails_admin", "staff"],
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