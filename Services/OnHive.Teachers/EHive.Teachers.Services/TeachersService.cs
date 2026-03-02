using AutoMapper;
using EHive.Core.Library.Contracts.Teachers;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Entities.Teachers;
using EHive.Core.Library.Contracts.Common;
using EHive.Teachers.Domain.Abstractions.Repositories;
using EHive.Teachers.Domain.Abstractions.Services;
using EHive.Teachers.Domain.Models;
using Serilog;
using System.Text.Json;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Domain.Exceptions;

namespace EHive.Teachers.Services
{
    public class TeachersService : ITeachersService
    {
        private readonly ITeachersRepository teachersRepository;
        private readonly TeachersApiSettings teachersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public TeachersService(ITeachersRepository teachersRepository, TeachersApiSettings teachersApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.teachersRepository = teachersRepository;
            this.teachersApiSettings = teachersApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<TeacherDto?> GetByIdAsync(string teacherId)
        {
            var teacher = await teachersRepository.GetByIdAsync(teacherId);
            return mapper.Map<TeacherDto>(teacher);
        }

        public async Task<PaginatedResult<TeacherDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await teachersRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<TeacherDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<TeacherDto>>(result.Itens)
                };
            }
            return new PaginatedResult<TeacherDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<TeacherDto>()
            };
        }

        public async Task<IEnumerable<TeacherDto>> GetAllAsync(UserDto? loggedUser)
        {
            var teachers = await teachersRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<TeacherDto>>(teachers);
        }

        public async Task<TeacherDto> SaveAsync(TeacherDto teacherDto, UserDto? loggedUser)
        {
            var teacher = mapper.Map<Teacher>(teacherDto);
            ValidatePermissions(teacher, loggedUser);
            teacher.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            teacher.CreatedAt = DateTime.UtcNow;
            teacher.CreatedBy = string.IsNullOrEmpty(teacher.CreatedBy) ? loggedUser.Id : teacher.CreatedBy;

            var response = await teachersRepository.SaveAsync(teacher);
            return mapper.Map<TeacherDto>(response);
        }

        public async Task<TeacherDto> CreateAsync(TeacherDto teacherDto, UserDto loggedUser)
        {
            var teacher = mapper.Map<Teacher>(teacherDto);
            ValidatePermissions(teacher, loggedUser);
            teacher.Id = string.Empty;
            teacher.TenantId = loggedUser.TenantId;
            var response = await teachersRepository.SaveAsync(teacher, loggedUser.Id);
            return mapper.Map<TeacherDto>(response);
        }

        public async Task<TeacherDto?> UpdateAsync(TeacherDto teacherDto, UserDto loggedUser)
        {
            var teacher = mapper.Map<Teacher>(teacherDto);
            ValidatePermissions(teacher, loggedUser);
            var currentTeacher = await teachersRepository.GetByIdAsync(teacher.Id);
            if (currentTeacher == null || currentTeacher.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            var response = await teachersRepository.SaveAsync(teacher, loggedUser.Id);
            return mapper.Map<TeacherDto>(response);
        }

        public async Task<TeacherDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentTeacher = await teachersRepository.GetByIdAsync(patch.GetId());
            if (currentTeacher == null || currentTeacher.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentTeacher = patch.PatchEntity(currentTeacher);
            ValidatePermissions(currentTeacher, loggedUser.User);
            if (!mapper.Map<TeacherDto?>(currentTeacher).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await teachersRepository.SaveAsync(currentTeacher, loggedUser.User.Id);
            return mapper.Map<TeacherDto>(response);
        }

        private void ValidatePermissions(Teacher teacher, UserDto? loggedUser)
        {
            if (loggedUser == null
               || teacher.TenantId != loggedUser.TenantId
               || (loggedUser.Id != teacher.UserId && !loggedUser.Permissions.Contains(teachersApiSettings.TeachersAdminPermission ?? string.Empty)))
            {
                logger.Warning("Unauthorized update mismatch tenantID Teacher/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    teacher.Id, teacher.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}