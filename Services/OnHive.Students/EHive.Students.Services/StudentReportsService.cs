using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Entities.Students;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.Students.Domain.Abstractions.Repositories;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Students.Domain.Models;
using Serilog;
using System.Text.Json;

namespace EHive.Students.Services
{
    public class StudentReportsService : IStudentReportsService
    {
        private readonly IStudentReportsRepository studentReportsRepository;
        private readonly StudentsApiSettings studentsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public StudentReportsService(IStudentReportsRepository studentReportsRepository,
                               StudentsApiSettings studentsApiSettings,
                               IMapper mapper)
        {
            this.studentReportsRepository = studentReportsRepository;
            this.studentsApiSettings = studentsApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<StudentReportDto?> GetByIdAsync(string reportId, LoggedUserDto? loggedUser)
        {
            var studentReport = await studentReportsRepository.GetByIdAsync(reportId);
            if (studentReport == null)
            {
                return null;
            }
            ValidatePermissions(studentReport, loggedUser?.User);
            var result = mapper.Map<StudentReportDto>(studentReport);
            return result;
        }

        public async Task<PaginatedResult<StudentReportDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await studentReportsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<StudentReportDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<StudentReportDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StudentReportDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = []
            };
        }

        public async Task<PaginatedResult<StudentReportDto>> GetByFilterAndTypeAsync(RequestFilter filter, string type, LoggedUserDto loggedUser)
        {
            if(filter.AndFilter == null)
            {
                filter.AndFilter = [];
            }
            filter.AndFilter.Add(new FilterField { Field = "ReportName", Operator = "eq", Value = type });
            return await GetByFilterAsync(filter, loggedUser);
        }

        async Task<IEnumerable<StudentReportDto>> IStudentReportsService.GetAllAsync(LoggedUserDto? loggedUser)
        {
            var studentReports = await studentReportsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<StudentReportDto>>(studentReports);
        }

        public async Task<StudentReportDto?> SaveAsync(StudentReportDto studentReportDto, LoggedUserDto? loggedUser)
        {
            var studentReport = mapper.Map<StudentReport>(studentReportDto);
            ValidatePermissions(studentReport, loggedUser?.User);
            studentReport.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            studentReport.CreatedAt = DateTime.UtcNow;
            studentReport.CreatedBy = string.IsNullOrEmpty(studentReport.CreatedBy) ? loggedUser.User.Id : studentReport.CreatedBy;
            var response = await studentReportsRepository.SaveAsync(studentReport);
            return mapper.Map<StudentReportDto>(response);
        }

        public async Task<StudentReportDto?> CreateAsync(StudentReportDto studentReportDto, LoggedUserDto loggedUser)
        {
            var studentReport = mapper.Map<StudentReport>(studentReportDto);
            ValidatePermissions(studentReport, loggedUser.User);
            studentReport.Id = string.Empty;
            studentReport.TenantId = loggedUser.User.TenantId;
            studentReport.IsActive = true;
            var response = await studentReportsRepository.SaveAsync(studentReport, loggedUser.User.Id);
            return mapper.Map<StudentReportDto>(response);
        }

        public async Task<StudentReportDto?> UpdateAsync(StudentReportDto studentReportDto, LoggedUserDto loggedUser)
        {
            var studentReport = mapper.Map<StudentReport>(studentReportDto);
            ValidatePermissions(studentReport, loggedUser.User);
            var currentStudent = await studentReportsRepository.GetByIdAsync(studentReport.Id);
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await studentReportsRepository.SaveAsync(studentReport, loggedUser.User.Id);
            return mapper.Map<StudentReportDto>(response);
        }

        async Task<StudentReportDto?> IStudentReportsService.UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentStudentReport = await studentReportsRepository.GetByIdAsync(patch.GetId());
            if (currentStudentReport == null || currentStudentReport.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentStudentReport = patch.PatchEntity(currentStudentReport);
            ValidatePermissions(currentStudentReport, loggedUser.User);
            if (!mapper.Map<StudentReportDto?>(currentStudentReport).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await studentReportsRepository.SaveAsync(currentStudentReport, loggedUser.User.Id);
            return mapper.Map<StudentReportDto>(response);
        }

        private void ValidatePermissions(StudentReport studentReport, UserDto? loggedUser)
        {
            if (loggedUser == null || studentReport.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Student/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    studentReport.Id, studentReport.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

    }
}