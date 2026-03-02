using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Emails;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Extensions;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Exceptions;
using EHive.Users.Domain.Mappers;
using EHive.Users.Domain.Models;
using EHive.Users.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using System.Text.Json;

namespace EHive.Users.Tests
{
    public class UsersServiceTests
    {
        private readonly IMapper mapper;
        private readonly Mock<IEmailsService> mockEmailService;
        private readonly MockRepository mockRepository;
        private readonly Mock<IRolesRepository> mockRolesRepository;
        private readonly Mock<IUsersRepository> mockUsersRepository;
        private readonly Mock<IStudentsService> mockStudentsService;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly Mock<ITenantsService> mockTenantsService;
        private readonly Mock<IServicesHub> mockServiceHub;
        private readonly UsersApiSettings usersApiSettings;

        public UsersServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUsersRepository = mockRepository.Create<IUsersRepository>();
            mockRolesRepository = mockRepository.Create<IRolesRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mockTenantsService = mockRepository.Create<ITenantsService>();
            mockEmailService = mockRepository.Create<IEmailsService>();
            mockStudentsService = mockRepository.Create<IStudentsService>();
            mockServiceHub = mockRepository.Create<IServicesHub>();
            mockServiceHub.SetupGet(s => s.TenantsService).Returns(mockTenantsService.Object);
            mockServiceHub.SetupGet(s => s.EmailsService).Returns(mockEmailService.Object);
            mockServiceHub.SetupGet(s => s.StudentsService).Returns(mockStudentsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            usersApiSettings = new UsersApiSettings();
        }

        [Fact]
        public async Task ChangePasswordAsyncTest()
        {
            // Arrange
            var service = CreateUserService();

            var expected = GetExpectedUser();
            var testUser = GetLoggedUser();
            var changePasswordDto = new ChangePasswordDto
            {
                TenantId = "11111111111111",
                UserId = testUser.User.Id,
                OldPasswordHash = expected.PasswordHash,
                NewPassword = "se1@n1AS"
            };

            mockUsersRepository.Setup(r => r.GetByIdAsync(testUser.User.Id)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            await service.ChangePasswordAsync(changePasswordDto, testUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(testUser.User.Id), Times.Once);
            mockUsersRepository.Verify(r =>
                r.SaveAsync(It.Is<User>(u =>
                    u.PasswordHash == changePasswordDto.NewPassword.HashMd5()), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsyncTestWithLoggedUser()
        {
            // Arrange
            var service = CreateUserService();
            var testUser = GetLoggedUser();
            var newUserDto = new UserDto
            {
                Name = "New User",
                Surname = "test",
                Login = "newuser",
                Emails = [new UserEmailDto { Email = "newuser@example.com", IsMain = true, IsValidated = false }],
                NewPassword = "se1@n1AS",
                TenantId = testUser.User.TenantId,
                Documents = [ new UserDocumentDto
                {
                    DocumentType = "cpf",
                    DocumentNumber = "11111111111"
                }],
                Gender = "NONE",
                Nationality = "BR"
            };

            var createdUser = mapper.Map<User>(newUserDto);
            createdUser.Id = Guid.NewGuid().ToString();

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(createdUser);
            mockUsersRepository.Setup(r => r.GetByLoginAsync(newUserDto.Login, testUser.User.TenantId)).ReturnsAsync(() => default);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(newUserDto.MainEmail, testUser.User.TenantId)).ReturnsAsync(() => default);

            // Act
            var result = await service.CreateAsync(newUserDto, testUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be(newUserDto.Name);
            result.Login.Should().Be(newUserDto.Login);
            result.Emails.Should().Contain(e => e.Email.Equals(newUserDto.MainEmail));
            result.TenantId.Should().Be(newUserDto.TenantId);

            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), testUser.User.Id), Times.Once);
            mockUsersRepository.Verify(r => r.GetByLoginAsync(newUserDto.Login, testUser.User.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(newUserDto.MainEmail, testUser.User.TenantId), Times.Once);
        }

        [Fact]
        public async Task CreateAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var newUser = new UserDto
            {
                Name = "New User",
                Surname = "test",
                Login = "newuser",
                Emails = [new UserEmailDto { Email = "newuser@example.com", IsMain = true, IsValidated = false }],
                NewPassword = "se1@n1AS",
                TenantId = loggedUser.User.TenantId,
                Documents = [ new UserDocumentDto
                {
                    DocumentType = "cpf",
                    DocumentNumber = "11111111111"
                }],
                Gender = "NONE",
                Nationality = "BR"
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId)).ReturnsAsync((User)null);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(newUser.MainEmail, newUser.TenantId)).ReturnsAsync((User)null);

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync((User savedUser, string createdBy) =>
            {
                savedUser.Id = Guid.NewGuid().ToString();
                return savedUser;
            });

            // Act
            var result = await service.CreateAsync(newUser, loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be(newUser.Name);
            result.Login.Should().Be(newUser.Login);
            result.Emails.Should().Contain(e => e.Email.Equals(newUser.MainEmail));
            result.TenantId.Should().Be(newUser.TenantId);

            mockUsersRepository.Verify(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(newUser.MainEmail, newUser.TenantId), Times.Once);

            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task SignInAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var newUser = new SignInUserDto
            {
                Name = "New User",
                Surname = "test",
                Login = "newuser",
                Email = "newuser@example.com",
                Password = "se1@n1AS",
                TenantId = "11111111111111",
                Document = new UserDocumentDto
                {
                    DocumentType = "cpf",
                    DocumentNumber = "11111111111"
                }
            };
            var tenant = new TenantDto
            {
                Id = newUser.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId)).ReturnsAsync((User)null);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId)).ReturnsAsync((User)null);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User savedUser, string createdBy) =>
            {
                savedUser.Id = Guid.NewGuid().ToString();
                return savedUser;
            });
            mockTenantsService.Setup(t => t.GetByIdAsync(newUser.TenantId)).ReturnsAsync(tenant);

            // Act
            var result = await service.CreateAsync(newUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be(newUser.Name);
            result.Login.Should().Be(newUser.Login);
            result.Emails.Should().Contain(e => e.Email.Equals(newUser.Email));
            result.TenantId.Should().Be(newUser.TenantId);

            mockUsersRepository.Verify(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SignInAsyncMandatoryFieldsFailTest()
        {
            // Arrange
            var service = CreateUserService();
            var newUser = new SignInUserDto
            {
                Name = "",
                Login = "",
                Email = "",
                TenantId = "",
                Document = new UserDocumentDto
                {
                    DocumentType = "cpf",
                    DocumentNumber = "111111"
                }
            };
            var tenant = new TenantDto
            {
                Id = newUser.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId)).ReturnsAsync((User)null);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId)).ReturnsAsync((User)null);
            mockTenantsService.Setup(t => t.GetByIdAsync(newUser.TenantId)).ReturnsAsync(tenant);

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User savedUser, string createdBy) =>
            {
                savedUser.Id = Guid.NewGuid().ToString();
                return savedUser;
            });

            try
            {
                // Act
                var result = await service.CreateAsync(newUser);
                Assert.Fail("Should raise exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                (ex as InvalidUserException).Message.Should().Contain("Name");
                (ex as InvalidUserException).Message.Should().Contain("Email");
                (ex as InvalidUserException).Message.Should().Contain("TenantId");
            }

            mockUsersRepository.Verify(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId), Times.Never);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId), Times.Never);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignInAsyncInvalidDocumentTypeFailTest()
        {
            // Arrange
            var service = CreateUserService();
            var newUser = new SignInUserDto
            {
                Name = "",
                Login = "",
                Email = "",
                TenantId = "",
                Document = new UserDocumentDto
                {
                    DocumentType = "wrongType",
                    DocumentNumber = "111111"
                }
            };
            var tenant = new TenantDto
            {
                Id = newUser.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId)).ReturnsAsync((User)null);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId)).ReturnsAsync((User)null);
            mockTenantsService.Setup(t => t.GetByIdAsync(newUser.TenantId)).ReturnsAsync(tenant);

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User savedUser, string createdBy) =>
            {
                savedUser.Id = Guid.NewGuid().ToString();
                return savedUser;
            });

            try
            {
                // Act
                var result = await service.CreateAsync(newUser);
                Assert.Fail("Should raise exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                (ex as InvalidUserException).Message.Should().Contain("Document");
            }

            mockUsersRepository.Verify(r => r.GetByLoginAsync(newUser.Login, newUser.TenantId), Times.Never);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(newUser.Email, newUser.TenantId), Times.Never);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();

            var expected = new List<User>
            {
                new User { Id = "1", Name = "User 1" },
                new User { Id = "2", Name = "User 2" },
             };

            mockUsersRepository.Setup(r => r.GetAllAsync(loggedUser.User.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expected.Count);
            result.Should().SatisfyRespectively(
                first => first.Id.Should().Be(expected[0].Id),
                second => second.Id.Should().Be(expected[1].Id));
            mockUsersRepository.Verify(r => r.GetAllAsync(loggedUser.User.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetByFilterAsyncTest()
        {
            // Arrange
            var service = CreateUserService();

            var filter = new RequestFilter();
            var loggedUser = GetLoggedUser();

            var expected = new PaginatedResult<User>
            {
                Page = 1,
                PageCount = 2,
                Itens = new List<User>
                {
                    new User { Id = "1", Name = "User 1" },
                    new User { Id = "2", Name = "User 2" },
                }
            };

            mockUsersRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser.User.TenantId, true)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(expected.Page);
            result.PageCount.Should().Be(expected.PageCount);
            result.Itens.Should().SatisfyRespectively(
               first => first.Id.Should().Be(expected.Itens[0].Id),
               second => second.Id.Should().Be(expected.Itens[1].Id));
            mockUsersRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser.User.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetUser_Test()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();

            var expected = new User
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser.User.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockRolesRepository.Setup(r => r.GetAllAsync(expected.TenantId)).ReturnsAsync(new List<Role>() { new Role { Name = "admin", TenantId = loggedUser.User.TenantId, Permissions = loggedUser.User.Permissions } });

            // Act
            var result = await service.GetByIdAsync(expected.Id, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            mockUsersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task GetUserByLogin_Test()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();

            var expected = new User
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser.User.TenantId,
                Login = "test"
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(expected.Login, It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            var result = await service.GetByLogin(expected.Login, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            mockUsersRepository.Verify(r => r.GetByLoginAsync(expected.Login, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmail_Test()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();

            var expected = new User
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser.User.TenantId,
                Login = "test",
                Emails = new List<UserEmail> { new UserEmail { Email = "test@test.com", IsMain = true, IsValidated = true } }
            };

            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(expected.MainEmail, It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            var result = await service.GetByMainEmail(expected.MainEmail, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(expected.MainEmail, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetUsers_Test()
        {
            // Arrange
            var service = CreateUserService();

            var user = GetLoggedUser();

            var expected = new List<User>
            {
                new User
                {
                    Id = user.User.Id
                }
            };

            mockUsersRepository.Setup(r => r.GetAllAsync(user.User.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expected.Count);
            result.Should().SatisfyRespectively(first => first.Id.Should().Be(expected[0].Id));
            mockUsersRepository.Verify(r => r.GetAllAsync(user.User.TenantId), Times.Once);
        }

        [Fact]
        public async Task ResendMainEmailValidationAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var expected = GetExpectedUser();
            expected.Emails[0].IsValidated = false;

            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(expected.MainEmail, expected.TenantId)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(expected);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            var tenant = new TenantDto
            {
                Id = expected.TenantId
            };

            mockTenantsService.Setup(t => t.GetByIdAsync(expected.TenantId)).ReturnsAsync(tenant);

            // Act
            await service.ResendMainEmailValidationAsync(expected.MainEmail, expected.TenantId);

            // Assert
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(expected.MainEmail, expected.TenantId), Times.Once);

            expected.Emails[0].IsValidated.Should().BeFalse();
            expected.Emails[0].ValidationCodes.Should().NotBeEmpty();
            mockEmailService.Verify(e => e.ComposeEmail(
                    It.Is<EmailSendDto>(dto =>
                        dto.SendTo.Contains(expected.MainEmail) &&
                        dto.Fields.ContainsKey("CODE") &&
                        dto.Fields.ContainsValue(expected.Emails[0].ValidationCodes[0].Code))),
                Times.Once);
        }

        [Fact]
        public async Task ResendSecondaryEmailValidationAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var expected = GetExpectedUser();
            var secondaryEmail = "test@mail.com";

            expected.Emails.Add(new UserEmail { Email = secondaryEmail, IsMain = false, IsValidated = false });
            mockUsersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(expected);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            var tenant = new TenantDto
            {
                Id = expected.TenantId
            };

            mockTenantsService.Setup(t => t.GetByIdAsync(expected.TenantId)).ReturnsAsync(tenant);

            // Act
            await service.ResendEmailValidationAsync(expected.Id, secondaryEmail, expected.TenantId);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);

            expected.Emails.Find(e => e.Email == secondaryEmail).IsValidated.Should().BeFalse();
            expected.Emails.Find(e => e.Email == secondaryEmail).ValidationCodes.Should().NotBeEmpty();
            mockEmailService.Verify(e => e.ComposeEmail(
                   It.Is<EmailSendDto>(dto =>
                       dto.SendTo.Contains(secondaryEmail) &&
                       dto.Fields.ContainsKey("CODE") &&
                       dto.Fields.ContainsValue(expected.Emails.Find(e => e.Email == secondaryEmail).ValidationCodes[0].Code))),
               Times.Once);
        }

        [Fact]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            var updatedUserDto = new UserDto
            {
                TenantId = userToUpdate.TenantId,
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
                    new UserDocumentDto
                    {
                        DocumentType = "cpf",
                        DocumentNumber = "11111111111"
                    }
                },
                Addresses = new List<AddressDto>
                {
                    new AddressDto { Name = "main", City = "test", AddressLines = "...", Country = new CountryDto { Code = "BR" }, State = new StateDto { Code = "PE" }, Number = "S/N", District = "Center", ZipCode = "123456-123" }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            var result = await service.UpdateAsync(updatedUserDto, loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(updatedUserDto.Id);

            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task PatchAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            var input = $"{{\"id\":\"{updatedUser.Id}\", \"tenantId\":\"{updatedUser.TenantId}\", \"name\":\"NewName\" }}";

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            var result = await service.PatchAsync(JsonDocument.Parse(input), loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(updatedUserDto.Id);

            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateRolesAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetLoggedUser().User;
            var expected = GetExpectedUser();
            var rolesToUpdate = new List<RoleDto>
            {
                new() { Name  = "role1"},
                new() { Name  = "role2"}
            };
            userToUpdate.Roles = rolesToUpdate.Select(r => r.Name).ToList();
            var updatedUser = mapper.Map<User>(userToUpdate);
            updatedUser.Roles = rolesToUpdate.Select(s => s.Name).ToList();

            mockUsersRepository.Setup(r => r.GetByIdAsync(userToUpdate.Id)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(updatedUser);
            mockRolesRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), loggedUser.User.TenantId)).ReturnsAsync((string roleName, string tenantId) =>
            {
                return new Role
                {
                    Name = roleName,
                    TenantId = tenantId
                };
            });

            // Act
            var result = await service.UpdateRolesAsync(userToUpdate, loggedUser);

            // Assert
            result.Should().NotBeNull();
            result.Roles.Should().BeEquivalentTo(rolesToUpdate.Select(r => r.Name));

            mockUsersRepository.Verify(r => r.GetByIdAsync(userToUpdate.Id), Times.Once);
            mockRolesRepository.Verify(r => r.GetByNameAsync(It.IsAny<string>(), loggedUser.User.TenantId), Times.Exactly(rolesToUpdate.Count));
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ValidateEmailAsyncTest()
        {
            // Arrange
            var service = CreateUserService();

            var token = "123456";
            var user = GetExpectedUser();
            user.Emails[0].IsValidated = false;
            user.Emails[0].ValidationCodes = new List<ValidationCode> { new ValidationCode { Code = token, ExpirationDate = DateTime.UtcNow.AddDays(60) } };

            mockUsersRepository.Setup(r => r.GetByMainEmailCodeAsync(token, user.TenantId)).ReturnsAsync(user);

            // Act
            await service.ValidateEmailAsync(token, user.TenantId);

            // Assert
            mockUsersRepository.Verify(r => r.GetByMainEmailCodeAsync(token, user.TenantId), Times.Once);
            user.Emails[0].IsValidated.Should().BeTrue();
            user.Emails[0].ValidationCodes.Should().BeEmpty();
        }

        [Fact]
        public async Task AddEmailAsyncTest()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            var tenant = new TenantDto
            {
                Id = user.TenantId
            };

            mockTenantsService.Setup(t => t.GetByIdAsync(user.TenantId)).ReturnsAsync(tenant);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            // Act
            var result = await service.AddEmailsAsync(user.Id, newEmail, loggedUser);

            // Assert
            result.Should().BeTrue();
            mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
            mockUsersRepository.Verify(r =>
                r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => !e.IsMain && !e.IsValidated && e.Email == newEmail)), It.IsAny<string>()), Times.Exactly(2));
            mockEmailService.Verify(e => e.ComposeEmail(
                  It.Is<EmailSendDto>(dto =>
                       dto.SendTo.Contains(newEmail) &&
                       dto.Fields.ContainsKey("CODE") &&
                       dto.Fields.ContainsValue(user.Emails.Last().ValidationCodes[0].Code))),
               Times.Once);
        }

        [Fact]
        public async Task AddEmailAsyncTest_Duplicated()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();
            user.Emails.Add(new UserEmail { Email = newEmail, IsMain = false, IsValidated = true });

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.GetByEmailAsync(newEmail, user.TenantId)).ReturnsAsync(() => null);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            // Act
            try
            {
                var result = await service.AddEmailsAsync(user.Id, newEmail, loggedUser);
                Assert.Fail("Should throw invalid user exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                ex.Message.Should().Contain("Duplicated");
                mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
                mockUsersRepository.Verify(r => r.GetByEmailAsync(newEmail, user.TenantId), Times.Never);
                mockUsersRepository.Verify(r =>
                    r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => !e.IsMain && !e.IsValidated && e.Email == newEmail)), It.IsAny<string>()), Times.Never);
                mockEmailService.Verify(e => e.ComposeEmail(
                      It.Is<EmailSendDto>(dto =>
                          dto.SendTo.Contains(newEmail) &&
                          dto.Fields.ContainsKey("CODE") &&
                          dto.Fields.ContainsValue(user.Emails[0].ValidationCodes[0].Code))),
                  Times.Never);
            }
        }

        [Fact]
        public async Task AddEmailAsyncTest_Duplicated_OtherUser()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();

            mockUsersRepository.Setup(r => r.GetByEmailAsync(newEmail, user.TenantId)).ReturnsAsync(() => new User());
            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            // Act
            try
            {
                var result = await service.AddEmailsAsync(user.Id, newEmail, loggedUser);
                Assert.Fail("Should throw invalid user exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                ex.Message.Should().Contain("Duplicated");
                mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
                mockUsersRepository.Verify(r => r.GetByEmailAsync(newEmail, user.TenantId), Times.Once);
                mockUsersRepository.Verify(r =>
                    r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => !e.IsMain && !e.IsValidated && e.Email == newEmail)), It.IsAny<string>()), Times.Never);
                mockEmailService.Verify(e => e.ComposeEmail(
                      It.Is<EmailSendDto>(dto =>
                           dto.SendTo.Contains(newEmail) &&
                           dto.Fields.ContainsKey("CODE") &&
                           dto.Fields.ContainsValue(user.Emails[0].ValidationCodes[0].Code))),
                   Times.Never);
            }
        }

        [Fact]
        public async Task SetMainEmail()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();
            user.Emails.Add(new UserEmail { Email = newEmail, IsValidated = true, IsMain = false });

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            var result = await service.SetMainEmailsAsync(user.Id, newEmail, loggedUser);

            // Assert
            result.Should().BeTrue();
            mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
            mockUsersRepository.Verify(r =>
                r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == newEmail)
                                            && u.Emails.Any(e => !e.IsMain && e.IsValidated && e.Email == user.Emails[0].Email)), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task SetMainEmail_NotFound()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            try
            {
                var result = await service.SetMainEmailsAsync(user.Id, newEmail, loggedUser);
                Assert.Fail("Should throw invalid user exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                ex.Message.Should().Contain("Missing");
                mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
                mockUsersRepository.Verify(r =>
                    r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == newEmail)
                                                && u.Emails.Any(e => !e.IsMain && e.IsValidated && e.Email == user.Emails[0].Email)), It.IsAny<string>()),
                    Times.Never);
            }
        }

        [Fact]
        public async Task RemoveEmail()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();
            user.Emails.Add(new UserEmail { Email = newEmail, IsValidated = true, IsMain = false });

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            var result = await service.RemoveEmailsAsync(user.Id, newEmail, loggedUser);

            // Assert
            result.Should().BeTrue();
            mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
            mockUsersRepository.Verify(r =>
                r.SaveAsync(It.Is<User>(u => !u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == newEmail)
                                            && u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == user.Emails[0].Email)), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveEmail_NotFound()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            try
            {
                var result = await service.RemoveEmailsAsync(user.Id, newEmail, loggedUser);
                Assert.Fail("Should throw invalid user exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                ex.Message.Should().Contain("Missing");
                mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
                mockUsersRepository.Verify(r =>
                    r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == newEmail)
                                                && u.Emails.Any(e => !e.IsMain && e.IsValidated && e.Email == user.Emails[0].Email)), It.IsAny<string>()),
                    Times.Never);
            }
        }

        [Fact]
        public async Task RemoveEmail_IsMain()
        {
            // Arrange
            var service = CreateUserService();
            var newEmail = "test2@email.com";
            var user = GetExpectedUser();
            var loggedUser = GetLoggedUser();
            user.Emails[0].IsMain = false;
            user.Emails.Add(new UserEmail { Email = newEmail, IsValidated = true, IsMain = true });

            mockUsersRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            try
            {
                var result = await service.RemoveEmailsAsync(user.Id, newEmail, loggedUser);
                Assert.Fail("Should throw invalid user exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidUserException>();
                ex.Message.Should().Contain("IsMain");
                mockUsersRepository.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
                mockUsersRepository.Verify(r =>
                    r.SaveAsync(It.Is<User>(u => u.Emails.Any(e => e.IsMain && e.IsValidated && e.Email == newEmail)
                                                && u.Emails.Any(e => !e.IsMain && e.IsValidated && e.Email == user.Emails[0].Email)), It.IsAny<string>()),
                    Times.Never);
            }
        }

        [Fact]
        public async Task Migrate()
        {
            // Arrange
            var service = CreateUserService();
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            await service.Migrate(false);

            // Assert
            mockUsersRepository.Verify(r => r.GetByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.Login == "admin"), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
              new UserDocumentDto
              {
                  DocumentType = "cpf",
                  DocumentNumber = "11111111111"
              }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            await service.DeactivateUser(userToUpdate.Id, loggedUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.IsActive == false), loggedUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task ReactivateUserTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            userToUpdate.IsActive = false;
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
              new UserDocumentDto
              {
                  DocumentType = "cpf",
                  DocumentNumber = "11111111111"
              }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            await service.ReactivateUser(userToUpdate.Id, loggedUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.IsActive == true), loggedUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task DeactivateFailUserTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            userToUpdate.IsActive = false;
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
              new UserDocumentDto
              {
                  DocumentType = "cpf",
                  DocumentNumber = "11111111111"
              }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            await service.DeactivateUser(userToUpdate.Id, loggedUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.IsActive == false), loggedUser.User.Id), Times.Never);
        }

        [Fact]
        public async Task ReactivateUserFailTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            userToUpdate.IsActive = true;
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
              new UserDocumentDto
              {
                  DocumentType = "cpf",
                  DocumentNumber = "11111111111"
              }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            await service.ReactivateUser(userToUpdate.Id, loggedUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.IsActive == true), loggedUser.User.Id), Times.Never);
        }

        [Fact]
        public async Task DeleteUserTest()
        {
            // Arrange
            var service = CreateUserService();
            var loggedUser = GetLoggedUser();
            var userToUpdate = GetExpectedUser();
            var updatedUserDto = new UserDto
            {
                Id = userToUpdate.Id,
                Name = "NovoNomeDeUsuario",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "novemail@example.com", IsMain = true, IsValidated = true } },
                Gender = "OPTOUT",
                Nationality = "BR",
                Documents =
                new List<UserDocumentDto> {
              new UserDocumentDto
              {
                  DocumentType = "cpf",
                  DocumentNumber = "11111111111"
              }
                }
            };
            var updatedUser = mapper.Map<User>(updatedUserDto);

            mockUsersRepository.Setup(r => r.GetByIdAsync(updatedUserDto.Id)).ReturnsAsync(userToUpdate);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), loggedUser.User.Id)).ReturnsAsync(updatedUser);

            // Act
            await service.Anonymize(userToUpdate.Id, loggedUser);

            // Assert
            mockUsersRepository.Verify(r => r.GetByIdAsync(updatedUserDto.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u =>
                u.IsActive == false
                && u.Name == "Anonymized"
                && u.Surname == "Anonymized"
                && u.SocialName == "Anonymized"
                && u.PhoneNumber == "Anonymized"
                && u.MobilePhoneNumber == "Anonymized"
                && u.Emails.Count == 0
                && u.Addresses.Count == 0
                && u.Roles.Count == 0), loggedUser.User.Id), Times.Once);
        }

        private static User GetExpectedUser()
        {
            var tenantId = "11111111111111";
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmail> { new UserEmail { Email = "Test@Test.com", IsValidated = true, IsMain = true } },
                IsActive = true,
                Roles = new List<string> { "admin", "users_admin", "users_read", "users_create", "users_update" },
                TenantId = tenantId,
                Gender = "OPTOUT",
                Nationality = "BR"
            };
        }

        private UsersService CreateUserService()
        {
            return new UsersService(
                mockUsersRepository.Object,
                mockRolesRepository.Object,
                usersApiSettings,
                mockServiceHub.Object,
                mapper,
                mockEventRegister.Object);
        }

        private LoggedUserDto GetLoggedUser()
        {
            var result = mapper.Map<UserDto>(GetExpectedUser());
            result.Roles = new List<string> { "admin" };
            result.Permissions = new List<string> { "admin", "users_admin" };
            result.Tenant = new TenantDto
            {
                Id = result.TenantId,
                Domain = "TestCo",
                Email = "Test@TestCo.com",
                Name = "TestCo",
                Features = new List<string> { "homolog" }
            };
            return new LoggedUserDto(result, "TOKEN");
        }
    }
}