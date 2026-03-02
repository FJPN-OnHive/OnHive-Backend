using AutoMapper;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Users.Domain.Models;
using Serilog;
using System.Text.Json;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Helpers;
using EHive.Users.Repositories;

namespace EHive.Users.Services
{
    public class UserGroupsService : IUserGroupsService
    {
        private readonly IUserGroupsRepository usersRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public UserGroupsService(IUserGroupsRepository usersRepository, UsersApiSettings usersApiSettings, IMapper mapper)
        {
            this.usersRepository = usersRepository;
            this.usersApiSettings = usersApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<UserGroupDto?> GetByIdAsync(string userGroupId)
        {
            var userGroup = await usersRepository.GetByIdAsync(userGroupId);
            return mapper.Map<UserGroupDto>(userGroup);
        }

        public async Task<PaginatedResult<UserGroupDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await usersRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<UserGroupDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<UserGroupDto>>(result.Itens)
                };
            }
            return new PaginatedResult<UserGroupDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<UserGroupDto>()
            };
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var userGroups = await usersRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<UserGroupDto>>(userGroups);
        }

        public async Task<UserGroupDto> SaveAsync(UserGroupDto userGroupDto, LoggedUserDto? loggedUser)
        {
            var userGroup = mapper.Map<UserGroup>(userGroupDto);
            ValidatePermissions(userGroup, loggedUser?.User);
            userGroup.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            userGroup.CreatedAt = DateTime.UtcNow;
            userGroup.CreatedBy = string.IsNullOrEmpty(userGroup.CreatedBy) ? loggedUser?.User?.Id : userGroup.CreatedBy;

            var response = await usersRepository.SaveAsync(userGroup);
            return mapper.Map<UserGroupDto>(response);
        }

        public async Task<UserGroupDto> CreateAsync(UserGroupDto userGroupDto, LoggedUserDto? loggedUser)
        {
            if (!userGroupDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var userGroup = mapper.Map<UserGroup>(userGroupDto);
            ValidatePermissions(userGroup, loggedUser?.User);
            userGroup.Id = string.Empty;
            userGroup.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            await ValidateDuplicate(userGroup);
            var response = await usersRepository.SaveAsync(userGroup, loggedUser.User.Id);
            return mapper.Map<UserGroupDto>(response);
        }

        public async Task<UserGroupDto?> UpdateAsync(UserGroupDto userGroupDto, LoggedUserDto? loggedUser)
        {
            if (!userGroupDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var userGroup = mapper.Map<UserGroup>(userGroupDto);
            ValidatePermissions(userGroup, loggedUser?.User);
            var currentUserGroup = await usersRepository.GetByIdAsync(userGroup.Id);
            if (currentUserGroup == null || currentUserGroup.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            await ValidateDuplicate(userGroup);
            var response = await usersRepository.SaveAsync(userGroup, loggedUser.User.Id);
            return mapper.Map<UserGroupDto>(response);
        }

        public async Task<bool> DeleteAsync(string userGroupId, LoggedUserDto loggedUser)
        {
            var current = await usersRepository.GetByIdAsync(userGroupId);
            if (current == null || current.TenantId != loggedUser.User.TenantId)
            {
                return false;
            }
            ValidatePermissions(current, loggedUser.User);
            await usersRepository.DeleteAsync(userGroupId);
            return true;
        }

        public async Task<UserGroupDto?> PatchAsync(JsonDocument userGroupDto, LoggedUserDto? loggedUser)
        {
            var current = await usersRepository.GetByIdAsync(userGroupDto.GetId());
            if (current == null || current.TenantId != loggedUser?.User?.TenantId)
            {
                throw new NotFoundException("User group not found");
            }
            var patched = userGroupDto.PatchEntity(current);
            ValidatePermissions(patched, loggedUser?.User);
            await ValidateDuplicate(patched);
            var response = await usersRepository.SaveAsync(patched, loggedUser.User.Id);
            return mapper.Map<UserGroupDto>(response);
        }

        private async Task ValidateDuplicate(UserGroup userGroup)
        {
            var filter = new RequestFilter
            {
                AndFilter = new List<FilterField>
                {
                      new FilterField
                    {
                        Field = "Id",
                        Value = userGroup.Id,
                        Operator = "ne"
                    },
                    new FilterField
                    {
                        Field = "Name",
                        Value = userGroup.Name,
                        Operator = "reg"
                    }
                }
            };
            var current = await usersRepository.GetByFilterAsync(filter, userGroup.TenantId, false);
            if (current != null && current.Itens.Any())
            {
                throw new DuplicatedException("User group already exists");
            }
        }

        private void ValidatePermissions(UserGroup userGroup, UserDto? loggedUser)
        {
            if (loggedUser != null && userGroup.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID UserGroup/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    userGroup.Id, userGroup.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}