using AutoMapper;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Users.Domain.Models;
using Serilog;
using EHive.Core.Library.Enums.Users;
using EHive.Core.Library.Exceptions;

namespace EHive.Users.Services
{
    public class UserProfilesService : IUserProfilesService
    {
        private readonly IUserProfilesRepository userProfilesRepository;
        private readonly IUsersRepository usersRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public UserProfilesService(IUserProfilesRepository userProfilesRepository, IUsersRepository usersRepository, UsersApiSettings usersApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.userProfilesRepository = userProfilesRepository;
            this.usersRepository = usersRepository;
            this.usersApiSettings = usersApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<List<UserProfileCompleteDto>> GetByUserIdAsync(string userId)
        {
            var userProfile = await userProfilesRepository.GetByUserIdAsync(userId);
            var result = mapper.Map<List<UserProfileCompleteDto>>(userProfile);
            foreach (var profile in result)
            {
                await CompleteUserData(profile);
            }
            return result;
        }

        public async Task<List<UserProfileDto>> GetByUserIdManagementAsync(string userId)
        {
            var userProfile = await userProfilesRepository.GetByUserIdAsync(userId);
            var result = mapper.Map<List<UserProfileDto>>(userProfile);
            return result;
        }

        public async Task<PaginatedResult<UserProfileCompleteDto>> GetByTypeAsync(RequestFilter filter, ProfileTypes type, string tenantId)
        {
            var result = await userProfilesRepository.GetByFilterAndTypeAsync(filter, type, tenantId);
            if (result != null)
            {
                var resultDto = new PaginatedResult<UserProfileCompleteDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<UserProfileCompleteDto>>(result.Itens)
                };
                foreach (var userProfile in resultDto.Itens)
                {
                    await CompleteUserData(userProfile);
                }
                return resultDto;
            }
            return new PaginatedResult<UserProfileCompleteDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<UserProfileCompleteDto>()
            };
        }

        private async Task<UserProfileCompleteDto> CompleteUserData(UserProfileCompleteDto userProfile)
        {
            var user = await usersRepository.GetByIdAsync(userProfile.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            userProfile.Name = user.Name;
            userProfile.Surname = user.Surname;
            userProfile.SocialName = user.SocialName;
            userProfile.Gender = user.Gender;
            if (userProfile.PublicEmail)
            {
                userProfile.MainEmail = user.MainEmail;
            }
            return userProfile;
        }

        public async Task<UserProfileDto?> GetByIdAsync(string userProfileId)
        {
            var userProfile = await userProfilesRepository.GetByIdAsync(userProfileId);
            return mapper.Map<UserProfileDto>(userProfile);
        }

        public async Task<PaginatedResult<UserProfileDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await userProfilesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<UserProfileDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<UserProfileDto>>(result.Itens)
                };
            }
            return new PaginatedResult<UserProfileDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<UserProfileDto>()
            };
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var userProfiles = await userProfilesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<UserProfileDto>>(userProfiles);
        }

        public async Task<UserProfileDto> SaveAsync(UserProfileDto userProfileDto, LoggedUserDto? loggedUser)
        {
            var userProfile = mapper.Map<UserProfile>(userProfileDto);
            ValidatePermissions(userProfile, loggedUser?.User);
            userProfile.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            userProfile.CreatedAt = DateTime.UtcNow;
            userProfile.CreatedBy = string.IsNullOrEmpty(userProfile.CreatedBy) ? loggedUser?.User?.Id : userProfile.CreatedBy;
            userProfile.IsActive = true;
            var response = await userProfilesRepository.SaveAsync(userProfile);
            return mapper.Map<UserProfileDto>(response);
        }

        public async Task<UserProfileDto> CreateAsync(UserProfileDto userProfileDto, LoggedUserDto? loggedUser)
        {
            if (!userProfileDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var userProfile = mapper.Map<UserProfile>(userProfileDto);
            ValidatePermissions(userProfile, loggedUser?.User);
            userProfile.Id = string.Empty;
            userProfile.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            userProfile.IsActive = true;
            var response = await userProfilesRepository.SaveAsync(userProfile, loggedUser.User.Id);
            return mapper.Map<UserProfileDto>(response);
        }

        public async Task<UserProfileDto?> UpdateAsync(UserProfileDto userProfileDto, LoggedUserDto? loggedUser)
        {
            if (!userProfileDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var userProfile = mapper.Map<UserProfile>(userProfileDto);
            ValidatePermissions(userProfile, loggedUser?.User);
            var currentUserProfile = await userProfilesRepository.GetByIdAsync(userProfile.Id);
            if (currentUserProfile == null || currentUserProfile.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            userProfile.IsActive = true;
            var response = await userProfilesRepository.SaveAsync(userProfile, loggedUser.User.Id);
            return mapper.Map<UserProfileDto>(response);
        }

        private void ValidatePermissions(UserProfile userProfile, UserDto? loggedUser)
        {
            if (loggedUser != null && userProfile.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID UserProfile/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    userProfile.Id, userProfile.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}