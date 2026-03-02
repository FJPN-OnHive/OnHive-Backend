using AutoMapper;
using EHive.Configuration.Library.Exceptions;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Emails;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Tenants;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Users;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Extensions;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.Enrich.Library.Extensions;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Users.Domain.Exceptions;
using EHive.Users.Domain.Models;
using EHive.Users.Services.Helpers;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;
using System.Text.Json;

namespace EHive.Users.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;
        private readonly IRolesRepository rolesRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEmailsService emailService;
        private readonly IEventRegister eventRegister;
        private readonly ITenantsService tenantsService;
        private readonly IStudentsService studentsService;

        public UsersService(IUsersRepository usersRepository,
                            IRolesRepository rolesRepository,
                            UsersApiSettings usersApiSettings,
                            IServicesHub servicesHub,
                            IMapper mapper,
                            IEventRegister eventRegister)
        {
            this.usersRepository = usersRepository;
            this.rolesRepository = rolesRepository;
            this.usersApiSettings = usersApiSettings;
            this.emailService = servicesHub.EmailsService;
            this.studentsService = servicesHub.StudentsService;
            this.tenantsService = servicesHub.TenantsService;
            this.eventRegister = eventRegister;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<UserDto?> GetByIdAsync(string userId, LoggedUserDto loggedUser)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null) return null;
            ValidatePermission(user, loggedUser.User);
            var result = mapper.Map<UserDto>(user);
            result.Permissions = await GetPermissions(user);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<UserDto?> GetByIdAsync(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null) return null;
            var result = mapper.Map<UserDto>(user);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<UserDto?> GetByLogin(string userLogin, LoggedUserDto loggedUser)
        {
            var user = await usersRepository.GetByLoginAsync(userLogin, loggedUser.User.TenantId);
            if (user == null) return null;
            ValidatePermission(user, loggedUser.User);
            var result = mapper.Map<UserDto>(user);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<UserDto?> GetByMainEmail(string userEmail, LoggedUserDto loggedUser)
        {
            var user = await usersRepository.GetByMainEmailAsync(userEmail, loggedUser.User.TenantId);
            if (user == null) return null;
            ValidatePermission(user, loggedUser.User);
            var result = mapper.Map<UserDto>(user);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<PaginatedResult<UserDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await usersRepository.GetByFilterAsync(filter, loggedUser?.User.TenantId);
            var resultList = mapper.Map<List<UserDto>>(result.Itens);
            var paginatedResult = new PaginatedResult<UserDto>
            {
                Page = result.Page,
                PageCount = result.PageCount,
                Total = result.Total,
                Itens = resultList
            };
            paginatedResult.Itens.ForEach(async i => await i.LoadEnrichmentAsync());
            return paginatedResult;
        }

        public async Task<PaginatedResult<UserDto>> GetByFilterAndProfileAsync(RequestFilter filter, ProfileTypes profile, LoggedUserDto loggedUser)
        {
            var result = await usersRepository.GetByFilterAndProfileAsync(filter, profile, loggedUser?.User.TenantId);
            var resultList = mapper.Map<List<UserDto>>(result.Itens);
            var paginatedResult = new PaginatedResult<UserDto>
            {
                Page = result.Page,
                PageCount = result.PageCount,
                Total = result.Total,
                Itens = resultList
            };
            paginatedResult.Itens.ForEach(async i => await i.LoadEnrichmentAsync());
            return paginatedResult;
        }

        public async Task<PaginatedResult<UserDto>> GetByIdsAsync(List<string> usersIds, RequestFilter filter, UserDto loggedUser)
        {
            var result = await usersRepository.GetByFilterAndIdsAsync(filter, usersIds, loggedUser?.TenantId);
            var resultList = mapper.Map<List<UserDto>>(result.Itens);
            var paginatedResult = new PaginatedResult<UserDto>
            {
                Page = result.Page,
                PageCount = result.PageCount,
                Total = result.Total,
                Itens = resultList
            };
            paginatedResult.Itens.ForEach(async i => await i.LoadEnrichmentAsync());
            return paginatedResult;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var users = await usersRepository.GetAllAsync(loggedUser?.User.TenantId);
            var result = mapper.Map<List<UserDto>>(users);
            result.ForEach(async i => await i.LoadEnrichmentAsync());
            return result;
        }

        public async Task<UserDto> ValidateEmailAsync(string code, string tenantId)
        {
            var currentUser = await usersRepository.GetByMainEmailCodeAsync(code, tenantId);
            var currentEmail = currentUser?.Emails
                .Find(e => e.IsMain && e.ValidationCodes.Any(c => c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)));
            if (currentUser == null
                || currentEmail == null
                || currentEmail.IsValidated
                || !currentEmail.ValidationCodes
                        .Any(c => c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)
                                    && c.ExpirationDate.CompareTo(DateTime.UtcNow) >= 0))
            {
                logger.Warning("Invalid validation email code: {code} for tenant {tenant}", code, tenantId);
                throw new InvalidUserException(new List<string> { $"Invalid validation email code: {code} for tenant {tenantId}" });
            }
            currentEmail.IsValidated = true;
            currentEmail.ValidationCodes.RemoveAll(c => c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));
            currentEmail.ValidationCodes.RemoveAll(c => c.ExpirationDate.CompareTo(DateTime.UtcNow) < 0);
            await usersRepository.SaveAsync(currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task<UserDto> ValidateEmailAsync(string email, string userId, string tenantId)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            var currentEmail = currentUser?.Emails.Find(e => e.Email == email);
            if (currentUser == null
                || currentEmail == null)
            {
                logger.Warning("Invalid user email: {userId} for tenant {tenant}", userId, tenantId);
                throw new InvalidUserException(new List<string> { $"Invalid email: {userId} for tenant {tenantId}" });
            }
            currentEmail.IsValidated = true;
            currentEmail.ValidationCodes.Clear();
            await usersRepository.SaveAsync(currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task ChangePasswordAsync(ChangePasswordDto changePassword, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(changePassword.UserId) ?? throw new UnauthorizedAccessException();
            ValidatePermission(currentUser, loggedUser.User);
            if (!currentUser.PasswordHash.Equals(changePassword.OldPasswordHash.HashMd5(), StringComparison.InvariantCultureIgnoreCase)
                && !currentUser.PasswordHash.Equals(changePassword.OldPasswordHash, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Password");
            }
            ValidatePassword(changePassword.NewPassword);
            currentUser.PasswordHash = changePassword.NewPassword.HashMd5();
            currentUser.IsChangePasswordRequested = false;
            _ = await usersRepository.SaveAsync(currentUser);
        }

        public async Task<string> CreateTempPasswordAsync(string userId, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            var newPassword = CodeHelper.GenerateAlphanumericCode(12);
            currentUser.TempPassword = new TempPassword
            {
                PasswordHash = newPassword.HashMd5(),
                ExpirationDate = DateTime.UtcNow.AddMinutes(30)
            };
            await usersRepository.SaveAsync(currentUser);
            Log.Information("Temp password created for user {User} by {Admin}", userId, loggedUser.User.Id);
            RegisterEvent(EventKeys.TempPasswordCreated, "Temp password created", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return newPassword;
        }

        public async Task ResendMainEmailValidationAsync(string email, string tenantId)
        {
            var currentUser = await usersRepository.GetByMainEmailAsync(email, tenantId);
            if (currentUser != null)
            {
                await SendValidationEmail(email, currentUser);
            }
        }

        public async Task ResendEmailValidationAsync(string userId, string email, string tenantId)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser != null)
            {
                await SendValidationEmail(email, currentUser);
            }
        }

        public async Task<UserDto> CreateAsync(UserDto newUser, LoggedUserDto loggedUser)
        {
            if (!newUser.Validate(out var validationResult))
            {
                throw new InvalidUserException(validationResult);
            }
            var user = mapper.Map<User>(newUser);
            user = ClearMasks(user);
            user.Id = string.Empty;
            user.TenantId = loggedUser.User.TenantId;
            VerifyUserBasicInfo(user);
            ValidatePassword(newUser.NewPassword);
            user.PasswordHash = newUser.NewPassword.HashMd5();
            await VerifyExistingUser(user);
            var response = await usersRepository.SaveAsync(user, loggedUser.User.Id);
            foreach (var email in user.Emails.Where(e => !e.IsValidated))
            {
                await SendValidationEmail(email.Email, user);
            }
            RegisterEvent(EventKeys.UserCreated, "User created", loggedUser.User.TenantId, loggedUser.User.Id, user);
            var result = mapper.Map<UserDto>(response);
            result.CustomAttributes = newUser.CustomAttributes;
            await result.SaveEnrichmentAsync();
            return result;
        }

        public async Task<UserDto> CreateAsync(SignInUserDto newUser)
        {
            if (!newUser.Validate(out var validationResult))
            {
                throw new InvalidUserException(validationResult);
            }
            await ValidateTenant(newUser);
            if (await usersRepository.GetByEmailAsync(newUser.Email, newUser.TenantId) != null)
            {
                throw new DuplicatedUserException(nameof(newUser.Email));
            }
            var user = mapper.Map<User>(newUser);
            user = ClearMasks(user);
            user.Id = string.Empty;
            user.Roles = string.IsNullOrEmpty(usersApiSettings.DefaultRole) ? new() : new List<string> { usersApiSettings.DefaultRole };
            user.Emails[0].ValidationCodes = new List<ValidationCode>
                {
                    new ValidationCode
                    {
                        Code = CodeHelper.GenerateAlphaCode(usersApiSettings.ValidationCodesSize),
                        ExpirationDate = DateTime.UtcNow.AddMinutes(usersApiSettings.ValidationCodesDurationMinutes)
                    }
                };
            user.IsActive = true;
            VerifyUserBasicInfo(user);
            ValidatePassword(newUser.Password);
            await VerifyExistingUser(user);
            var response = await usersRepository.SaveAsync(user);
            RegisterEvent(EventKeys.UserRegistered, "User created by Register", user.TenantId, user.Id, user);
            logger.Information("User {userId} created by Register", user.Id);
            await SendEmail(user, usersApiSettings.UserEmailValidationUrl);
            return mapper.Map<UserDto>(response);
        }

        public async Task<UserDto?> UpdateAsync(UserDto userDto, LoggedUserDto loggedUser)
        {
            if (!userDto.Validate(out var validationResult))
            {
                throw new InvalidUserException(validationResult);
            }
            var user = mapper.Map<User>(userDto);
            user = ClearMasks(user);
            var currentUser = await usersRepository.GetByIdAsync(user.Id);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(userDto.NewPassword))
            {
                ValidatePassword(userDto.NewPassword);
                user.PasswordHash = userDto.NewPassword.HashMd5();
            }
            currentUser = await UpdateFieldsAsync(currentUser, user, loggedUser);
            ValidatePermission(currentUser, loggedUser.User);
            var response = await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User updated", loggedUser.User.TenantId, loggedUser.User.Id, user);
            var result = mapper.Map<UserDto>(response);
            result.CustomAttributes = userDto.CustomAttributes;
            await result.SaveEnrichmentAsync();
            return result;
        }

        public async Task<UserDto?> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(patch.GetId());
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            currentUser = patch.PatchEntity(currentUser);
            currentUser = ClearMasks(currentUser);
            if (!mapper.Map<UserDto>(currentUser).Validate(out var validationResult))
            {
                throw new InvalidUserException(validationResult);
            }
            ValidatePermission(currentUser, loggedUser.User);
            currentUser = CompleteDefaultFields(currentUser);
            var response = await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User updated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(response);
        }

        public async Task<UserDto?> UpdateRolesAsync(UserDto userDto, LoggedUserDto loggedUser)
        {
            if (!userDto.Validate(out var validationResult))
            {
                throw new InvalidUserException(validationResult);
            }
            if (!loggedUser.User.Permissions.Exists(p => p.Equals(usersApiSettings.UserAdminPermission, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new UnauthorizedAccessException();
            }
            var user = mapper.Map<User>(userDto);
            var currentUser = await usersRepository.GetByIdAsync(user.Id);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            currentUser.Roles = user.Roles;
            await VerifyRoles(currentUser.Roles, loggedUser.User.TenantId);
            var response = await usersRepository.SaveAsync(currentUser);
            RegisterEvent(EventKeys.UserUpdated, "User roles updated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(response);
        }

        public async Task<bool> AddEmailsAsync(string userId, string email, LoggedUserDto loggedUser)
        {
            if (email.Length > 256)
            {
                throw new InvalidUserException(new List<string> { "Email" });
            }
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return false;
            }
            ValidatePermission(currentUser, loggedUser.User);
            if (currentUser.Emails.Any(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                || await usersRepository.GetByEmailAsync(email, loggedUser.User.TenantId) != null)
            {
                throw new InvalidUserException(new List<string> { "Duplicated - Cannot add duplicated email" });
            }
            var newEmail = new UserEmail { Email = email, IsMain = false, IsValidated = false };
            currentUser.Emails.Add(newEmail);
            await usersRepository.SaveAsync(currentUser);
            await SendValidationEmail(email, currentUser);
            RegisterEvent(EventKeys.UserUpdated, "User Email Added", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return true;
        }

        public async Task<bool> RemoveEmailsAsync(string userId, string email, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return false;
            }
            ValidatePermission(currentUser, loggedUser.User);
            if (!currentUser.Emails.Any(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidUserException(new List<string> { "Missing Email - Email not found" });
            }
            if (currentUser.Emails.Any(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && e.IsMain))
            {
                throw new InvalidUserException(new List<string> { "IsMain - Cannot remove main email" });
            }
            currentUser.Emails.RemoveAll(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && !e.IsMain);
            await usersRepository.SaveAsync(currentUser);
            RegisterEvent(EventKeys.UserUpdated, "User Email Removed", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return true;
        }

        public async Task<bool> SetMainEmailsAsync(string userId, string email, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId)
            {
                return false;
            }
            ValidatePermission(currentUser, loggedUser.User);
            if (!currentUser.Emails.Any(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidUserException(new List<string> { "Missing Email - Email not found" });
            }
            if (currentUser.Emails.Any(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && !e.IsValidated))
            {
                throw new InvalidUserException(new List<string> { "Not Validated - Email not validated" });
            }
            currentUser.Emails.ForEach(e => e.IsMain = e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            RegisterEvent(EventKeys.UserUpdated, "Main Email Updated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            await usersRepository.SaveAsync(currentUser);
            return true;
        }

        public async Task Migrate(bool isProduction)
        {
            var file = GetMigrationFile(isProduction);
            foreach (var user in file)
            {
                var current = await usersRepository.GetByLoginAsync(user.Login, user.TenantId);
                if (current == null)
                {
                    await usersRepository.SaveAsync(user);
                }
            }
        }

        public Task DeactivateUser(string userId, LoggedUserDto loggedUser)
        {
            return UserToggleActive(false, userId, loggedUser);
        }

        public Task ReactivateUser(string userId, LoggedUserDto loggedUser)
        {
            return UserToggleActive(true, userId, loggedUser);
        }

        public async Task Anonymize(string userId, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId) return;
            ValidatePermission(currentUser, loggedUser.User);
            currentUser.Name = "Anonymized";
            currentUser.Surname = "Anonymized";
            currentUser.SocialName = "Anonymized";
            currentUser.Login = $"{currentUser.Id}-Anonymized";
            currentUser.Documents = new List<UserDocument>();
            currentUser.Emails = new List<UserEmail>();
            currentUser.PhoneNumber = "Anonymized";
            currentUser.MobilePhoneNumber = "Anonymized";
            currentUser.Addresses = new List<Address>();
            currentUser.Roles = new List<string>();
            currentUser.IsActive = false;
            currentUser.UpdatedAt = DateTime.UtcNow;
            currentUser.UpdatedBy = loggedUser.User.Id;
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User Anonymized", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
        }

        public async Task DeleteAccountDataAsync(string userId, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId) return;
            ValidatePermission(currentUser, loggedUser.User);
            currentUser.Surname = "";
            currentUser.SocialName = "";
            currentUser.Documents = new List<UserDocument>();
            currentUser.Addresses = new List<Address>();
            currentUser.PhoneNumber = "";
            currentUser.MobilePhoneNumber = "";
            currentUser.UpdatedAt = DateTime.UtcNow;
            currentUser.UpdatedBy = loggedUser.User.Id;
            currentUser.Emails.RemoveAll(e => !e.IsMain);
            await DeleteCoursesAsync(currentUser);
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User Account Data Deleted", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
        }

        public async Task<UserDto?> AddAddressAsync(string userId, AddressDto address, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            address.ZipCode = address.ZipCode.RemoveMask();
            ValidatePermission(currentUser, loggedUser.User);
            currentUser.Addresses.Add(mapper.Map<Address>(address));
            if (currentUser.Addresses.Count == 1)
            {
                currentUser.Addresses[0].IsMainAddress = true;
            }
            else if (address.IsMainAddress)
            {
                currentUser.Addresses.ForEach(a => a.IsMainAddress = false);
                currentUser.Addresses.Find(a => a.Name == address.Name).IsMainAddress = true;
            }
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User Address added", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task<UserDto?> SetMainAddressAsync(string userId, string addressName, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            ValidatePermission(currentUser, loggedUser.User);
            if (!currentUser.Addresses.Any(a => a.Name == addressName))
            {
                throw new NotFoundException("Address not found");
            }
            currentUser.Addresses.ForEach(a => a.IsMainAddress = false);
            currentUser.Addresses.Find(a => a.Name == addressName).IsMainAddress = true;
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User main address Set", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task<UserDto?> UpdateAddressAsync(string userId, AddressDto address, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            ValidatePermission(currentUser, loggedUser.User);
            address.ZipCode = address.ZipCode.RemoveMask();
            var currentAddress = currentUser.Addresses.Find(a => a.Name == address.Name) ?? throw new NotFoundException("Address not found");
            currentAddress.AddressLines = address.AddressLines;
            currentAddress.City = address.City;
            currentAddress.State = address.State.Code;
            currentAddress.Country = address.Country.Code;
            currentAddress.ZipCode = address.ZipCode;
            currentAddress.IsMainAddress = address.IsMainAddress;
            if (currentAddress.IsMainAddress)
            {
                currentUser.Addresses.ForEach(a => a.IsMainAddress = false);
                currentAddress.IsMainAddress = true;
            }
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User address Updated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task<UserDto?> RemoveAddressAsync(string userId, string addressName, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            ValidatePermission(currentUser, loggedUser.User);
            var currentAddress = currentUser.Addresses.Find(a => a.Name == addressName) ?? throw new NotFoundException("Address not found");
            if (currentAddress.IsMainAddress)
            {
                throw new InvalidUserException(new List<string> { "Main Address - Cannot remove main address" });
            }
            currentUser.Addresses.Remove(currentAddress);
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            RegisterEvent(EventKeys.UserUpdated, "User address removed", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            return mapper.Map<UserDto>(currentUser);
        }

        public async Task<string> GetLastValidationCodeAsync(string userId, string email, LoggedUserDto loggedUser)
        {
            if (!loggedUser.User.Permissions.Contains(usersApiSettings.UserAdminPermission)) throw new UnauthorizedAccessException();
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found");
            var userEmail = user.Emails.Find(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            if (userEmail == null) throw new NotFoundException("Email not found");
            if (userEmail.ValidationCodes.Count == 0) throw new NotFoundException("Validation code not found");
            return userEmail.ValidationCodes.Last().Code;
        }

        private User ClearMasks(User user)
        {
            user.Documents.ForEach(d => d.DocumentNumber = d.DocumentNumber.RemoveMask());
            user.PhoneNumber = user.PhoneNumber.RemoveMask();
            user.MobilePhoneNumber = user.MobilePhoneNumber.RemoveMask();
            user.Addresses.ForEach(a => a.ZipCode = a.ZipCode.RemoveMask());
            return user;
        }

        private async Task DeleteCoursesAsync(User currentUser)
        {
            await studentsService.DeleteEnrollments(currentUser.Id);
        }

        private async Task UserToggleActive(bool isActive, string userId, LoggedUserDto loggedUser)
        {
            var currentUser = await usersRepository.GetByIdAsync(userId);
            if (currentUser == null || currentUser.TenantId != loggedUser.User.TenantId) return;
            if (currentUser.IsActive == isActive) return;
            currentUser.IsActive = isActive;
            currentUser.UpdatedAt = DateTime.UtcNow;
            currentUser.UpdatedBy = loggedUser.User.Id;
            ValidatePermission(currentUser, loggedUser.User);
            await usersRepository.SaveAsync(currentUser, loggedUser.User.Id);
            if (isActive)
            {
                RegisterEvent(EventKeys.UserActivated, "User Activated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            }
            else
            {
                RegisterEvent(EventKeys.UserDeactivated, "User Deactivated", loggedUser.User.TenantId, loggedUser.User.Id, currentUser);
            }
        }

        private async Task ValidateTenant(SignInUserDto user)
        {
            var result = await tenantsService.GetByIdAsync(user.TenantId);
            if (result == null)
            {
                logger.Warning("Invalid tenant {tenantId} for user: {user}", user.TenantId, user.Email);
                throw new InvalidUserException(new List<string> { "TenantId" });
            }
        }

        private async Task SendEmail(User user, string link, string email = "")
        {
            if (string.IsNullOrEmpty(email)) email = user.MainEmail;
            try
            {
                var tenant = await GetTenantAsync(user.TenantId);
                var processedLink = link;
                if (tenant != null && !string.IsNullOrEmpty(tenant.Domain))
                {
                    processedLink = processedLink.Replace("<DOMAIN>", $"{tenant.Domain}.");
                }
                else
                {
                    processedLink = processedLink.Replace("<DOMAIN>", "");
                }
                var currentEmail = user.Emails.FirstOrDefault(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
                if (currentEmail == null) return;
                var emailValidation = new EmailSendDto
                {
                    TenantId = user.TenantId,
                    SendTo = new List<string> { currentEmail.Email },
                    Fields = new Dictionary<string, string> {
                    { "CODE", currentEmail.ValidationCodes.Last().Code },
                    { "NAME", user.Name },
                    { "LINK", $"{processedLink}?tenantId={user.TenantId}&email={currentEmail.Email}&user={user.Id}&code={currentEmail.ValidationCodes.Last().Code}" }
                },
                    TemplateCode = usersApiSettings.UserEmailValidationTemplate,
                    ServiceCode = usersApiSettings.UserEmailValidationService
                };
                _ = emailService.ComposeEmail(emailValidation);
                logger.Information("Email message sent for user {user} to {email}", user.Id, currentEmail.Email);
            }
            catch (Exception ex)
            {
                logger.Error("Fail sending email message for user {user}: -- error: {message}", user.Id, ex.Message, ex);
            }
        }

        private async Task<TenantDto?> GetTenantAsync(string tenantId)
        {
            return await tenantsService.GetByIdAsync(tenantId);
        }

        private async Task SendValidationEmail(string email, User? currentUser)
        {
            var currentEmail = currentUser?.Emails.Find(e => e.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            if (currentUser == null || currentEmail == null)
            {
                logger.Warning("Send email validation code: Email not found {email} for user: {user}", email, currentUser?.Id);
                return;
            }
            if (currentEmail.IsValidated)
            {
                logger.Warning("Email already validated: {email} for user: {user}", email, currentUser?.Id);
                return;
            }
            currentEmail.IsValidated = false;
            currentEmail.ValidationCodes.Add(new ValidationCode
            {
                ExpirationDate = DateTime.UtcNow.AddMinutes(usersApiSettings.ValidationCodesDurationMinutes),
                Code = CodeHelper.GenerateAlphaCode(usersApiSettings.ValidationCodesSize)
            });
            _ = await usersRepository.SaveAsync(currentUser);
            await SendEmail(currentUser, usersApiSettings.UserEmailValidationUrl, email);
        }

        private void ValidatePassword(string password)
        {
            var passeordValidation = PasswordValidation.Validate(password, usersApiSettings.PasswordPattern);
            if (passeordValidation.Any())
            {
                throw new InvalidUserException(passeordValidation);
            }
        }

        private async Task VerifyRoles(List<string> roles, string tenantId)
        {
            foreach (var role in roles)
            {
                _ = await rolesRepository.GetByNameAsync(role, tenantId) ?? throw new ArgumentException($"Invalid role {role}");
            }
        }

        private User CompleteDefaultFields(User user)
        {
            user.Gender = !string.IsNullOrEmpty(user.Gender) ? user.Gender : "NONE";
            return user;
        }

        private async Task<User> UpdateFieldsAsync(User currentUser, User user, LoggedUserDto loggedUser)
        {
            if (user.Login != currentUser.Login)
            {
                if (await VerifyLoginChange(currentUser, user))
                {
                    currentUser.Login = user.Login;
                }
                else
                {
                    throw new DuplicatedUserException($"Login {user.Login} already exists.");
                }
            }
            currentUser.Name = user.Name;
            currentUser.Surname = user.Surname;
            if (user.Documents != null && user.Documents.Count > 0)
            {
                currentUser.Documents = user.Documents;
            }
            if (user.Addresses != null && user.Addresses.Count > 0)
            {
                currentUser.Addresses = user.Addresses;
            }
            currentUser.SocialName = user.SocialName;
            currentUser.BirthDate = user.BirthDate;
            currentUser.Gender = user.Gender;
            currentUser.Nationality = user.Nationality;
            currentUser.PhoneNumber = user.PhoneNumber;
            currentUser.MobilePhoneNumber = user.MobilePhoneNumber;
            currentUser.IsForeigner = user.IsForeigner;
            currentUser.Occupation = user.Occupation;
            if (loggedUser != null && loggedUser.User != null
                && user.Roles != null && user.Roles.Count != 0
                && loggedUser.User.Permissions.Contains(usersApiSettings.UserAdminPermission))
            {
                currentUser.IsChangePasswordRequested = user.IsChangePasswordRequested;
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    currentUser.PasswordHash = user.PasswordHash;
                }
                currentUser.Roles = user.Roles;
            }

            return currentUser;
        }

        private async Task<bool> VerifyLoginChange(User currentUser, User user)
        {
            var existingUsers = await usersRepository.GetByLoginAsync(user.Login, currentUser.TenantId);
            return existingUsers == null;
        }

        private void VerifyUserBasicInfo(User user)
        {
            var fieldList = new List<string>();
            if (string.IsNullOrEmpty(user.TenantId))
            {
                fieldList.Add(nameof(user.TenantId));
            }
            if (string.IsNullOrEmpty(user.Login))
            {
                fieldList.Add(nameof(user.Login));
            }
            if (string.IsNullOrEmpty(user.Name))
            {
                fieldList.Add(nameof(user.Name));
            }
            if (!user.Emails.Any(e => e.IsMain && !string.IsNullOrEmpty(e.Email)))
            {
                fieldList.Add(nameof(user.Emails));
            }
            if (DateTime.UtcNow.Year - user.BirthDate.Year < usersApiSettings.MinAge)
            {
                fieldList.Add(nameof(user.BirthDate));
            }

            if (fieldList.Any())
            {
                throw new InvalidUserException(fieldList);
            }
        }

        private async Task VerifyExistingUser(User user)
        {
            var currentUser = await usersRepository.GetByLoginAsync(user.Login, user.TenantId);
            if (currentUser != null)
            {
                throw new DuplicatedUserException(nameof(user.Login));
            }

            currentUser = await usersRepository.GetByMainEmailAsync(user.MainEmail, user.TenantId);
            if (currentUser != null)
            {
                throw new DuplicatedUserException(nameof(user.MainEmail));
            }
        }

        private void ValidatePermission(User user, UserDto? loggedUser)
        {
            if (loggedUser != null
                && user.Id != loggedUser.Id
                && !loggedUser.Permissions.Exists(p => p.Equals(usersApiSettings.UserAdminPermission, StringComparison.InvariantCultureIgnoreCase)))
            {
                logger.Warning("Unauthorized user update: {userid}, logged user: {loggedUserId}", user.Id, loggedUser.Id);
                throw new UnauthorizedAccessException();
            }

            if (loggedUser != null && user.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized user update mismatch tenantID user/tenant: {userid} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    user.Id, user.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private List<User> GetMigrationFile(bool isProduction)
        {
            var environment = isProduction ? "prod" : "staging";
            var file = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(Path.Join("Migrations", $"defaultUsers.{environment}.json")));
            if (file == null)
            {
                logger.Error("Missing features migration file.");
                throw new MissingConfigurationException<List<User>>($"defaultUsers.{environment}.json");
            }
            return file;
        }

        private void RegisterEvent(string key, string message, string tenantId, string UserId, User user)
        {
            try
            {
                _ = eventRegister.RegisterEvent(tenantId, UserId, key, message,
                                                    ["UserId", "UserMainEmail", "UserLogin", "UserName", "UserPhone"],
                                                    user.Id, user.MainEmail, user.Login, user.Name, user.PhoneNumber);
            }
            catch (Exception ex)
            {
                logger.Error("Fail registering event {key} for user {user}: -- error: {message}", key, user.Id, ex.Message, ex);
            }
        }

        private async Task<List<string>> GetPermissions(User user)
        {
            var roles = await rolesRepository.GetAllAsync(user.TenantId);
            return roles.Where(r => user.Roles.Contains(r.Name))
                        .SelectMany(r => r.Permissions)
                        .Distinct()
                        .ToList();
        }
    }
}