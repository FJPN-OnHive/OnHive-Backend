using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Courses.Domain.Models;
using OnHive.Enrich.Library.Extensions;
using Serilog;
using System.Text.Json;

namespace OnHive.Courses.Services
{
    public class ExamsService : IExamsService
    {
        private readonly IExamsRepository examsRepository;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ExamsService(IExamsRepository examsRepository, CoursesApiSettings coursesApiSettings, IMapper mapper)
        {
            this.examsRepository = examsRepository;
            this.coursesApiSettings = coursesApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<ExamDto?> GetByIdAsync(string examId, UserDto? loggedUser)
        {
            var exam = await examsRepository.GetByVIdLatestAsync(examId);
            exam = VerifyAnswerViewPermission(loggedUser, exam);
            var result = mapper.Map<ExamDto>(exam);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<PaginatedResult<ExamDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await examsRepository.GetByFilterAsync(filter, loggedUser?.TenantId);
            if (result != null)
            {
                result.Itens = VerifyAnswerViewPermission(loggedUser, result.Itens);
                var itens = mapper.Map<List<ExamDto>>(result.Itens);
                itens.ForEach(async i => await i.LoadEnrichmentAsync());
                return new PaginatedResult<ExamDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = itens
                };
            }
            return new PaginatedResult<ExamDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<ExamDto>()
            };
        }

        public async Task<IEnumerable<ExamDto>> GetAllAsync(UserDto? loggedUser)
        {
            var exams = await examsRepository.GetAllAsync(loggedUser?.TenantId);
            exams = VerifyAnswerViewPermission(loggedUser, exams);
            var result = mapper.Map<List<ExamDto>>(exams);
            result.ForEach(async i => await i.LoadEnrichmentAsync());
            return result;
        }

        public async Task<ExamDto> SaveAsync(ExamDto examDto, UserDto? loggedUser)
        {
            var exam = mapper.Map<Exam>(examDto);
            ValidatePermissions(exam, loggedUser);
            var current = await examsRepository.GetByVIdAsync(exam.VId, exam.VersionNumber);
            if (current != null)
            {
                exam.Id = current.Id;
            }
            exam.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            exam.CreatedAt = DateTime.UtcNow;
            exam.IsActive = true;
            exam.CreatedBy = string.IsNullOrEmpty(exam.CreatedBy) ? loggedUser.Id : exam.CreatedBy;
            exam.Questions.ForEach(q => q.Id = string.IsNullOrEmpty(q.Id) ? Guid.NewGuid().ToString() : q.Id);
            exam.Questions.ForEach(q => q.Options.ForEach(o => o.Id = string.IsNullOrEmpty(o.Id) ? Guid.NewGuid().ToString() : o.Id));
            var response = await examsRepository.SaveAsync(exam);
            return mapper.Map<ExamDto>(response);
        }

        public async Task<ExamDto> CreateAsync(ExamDto examDto, UserDto loggedUser)
        {
            var exam = mapper.Map<Exam>(examDto);
            ValidatePermissions(exam, loggedUser);
            exam.Id = string.Empty;
            exam.TenantId = loggedUser.TenantId;
            exam.IsActive = true;
            exam.Questions.ForEach(q => q.Id = string.IsNullOrEmpty(q.Id) ? Guid.NewGuid().ToString() : q.Id);
            exam.Questions.ForEach(q => q.Options.ForEach(o => o.Id = string.IsNullOrEmpty(o.Id) ? Guid.NewGuid().ToString() : o.Id));
            var response = await examsRepository.SaveAsync(exam, loggedUser.Id);
            return mapper.Map<ExamDto>(response);
        }

        public async Task<ExamDto?> UpdateAsync(ExamDto examDto, UserDto loggedUser)
        {
            var exam = mapper.Map<Exam>(examDto);
            ValidatePermissions(exam, loggedUser);
            var currentExam = await examsRepository.GetByVIdAsync(exam.Id, exam.VersionNumber);
            if (currentExam == null || currentExam.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            exam.Id = currentExam.Id;
            exam.IsActive = true;
            exam.Questions.ForEach(q => q.Id = string.IsNullOrEmpty(q.Id) ? Guid.NewGuid().ToString() : q.Id);
            exam.Questions.ForEach(q => q.Options.ForEach(o => o.Id = string.IsNullOrEmpty(o.Id) ? Guid.NewGuid().ToString() : o.Id));
            var response = await examsRepository.SaveAsync(exam, loggedUser.Id);
            return mapper.Map<ExamDto>(response);
        }

        public async Task<ExamDto?> UpdateAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentExam = await examsRepository.GetByIdAsync(patch.GetId());
            if (currentExam == null || currentExam.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentExam = patch.PatchEntity(currentExam);
            if (!mapper.Map<ExamDto>(currentExam).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(currentExam, loggedUser);
            currentExam.IsActive = true;
            var response = await examsRepository.SaveAsync(currentExam, loggedUser.Id);
            return mapper.Map<ExamDto>(response);
        }

        private Exam VerifyAnswerViewPermission(UserDto? user, Exam exam)
        {
            return VerifyAnswerViewPermission(user, new List<Exam> { exam }).FirstOrDefault() ?? exam;
        }

        private List<Exam> VerifyAnswerViewPermission(UserDto? user, List<Exam> exams)
        {
            if (user != null)
            {
                if (!user?.Permissions.Any(p => p.Equals("courses_admin", StringComparison.InvariantCultureIgnoreCase)
                                        || p.Equals("courses_update", StringComparison.InvariantCultureIgnoreCase)
                                        || p.Equals("courses_create", StringComparison.InvariantCultureIgnoreCase)) ?? true)
                {
                    exams.ForEach(e => e?.Questions.ForEach(q => q.Options.ForEach(o => o.IsCorrect = false)));
                }
            }
            else
            {
                exams.ForEach(e => e?.Questions.ForEach(q => q.Options.ForEach(o => o.IsCorrect = false)));
            }
            return exams;
        }

        private void ValidatePermissions(Exam exam, UserDto? loggedUser)
        {
            if (loggedUser != null && exam.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Exam/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    exam.Id, exam.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        public async Task<ExamDto?> CreateVersionAsync(ExamDto examDto, UserDto loggedUser)
        {
            var exam = mapper.Map<Exam>(examDto);
            ValidatePermissions(exam, loggedUser);
            var currentExam = await examsRepository.GetByVIdAsync(exam.VId, exam.VersionNumber);
            if (currentExam == null || currentExam.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            exam.IsActive = true;
            exam.Questions.ForEach(q => q.Id = string.IsNullOrEmpty(q.Id) ? Guid.NewGuid().ToString() : q.Id);
            exam.Questions.ForEach(q => q.Options.ForEach(o => o.Id = string.IsNullOrEmpty(o.Id) ? Guid.NewGuid().ToString() : o.Id));
            var response = await examsRepository.SaveVersionAsync(exam, loggedUser.Id);
            return mapper.Map<ExamDto>(response);
        }

        public async Task<List<ExamDto>> GetVersionsAsync(string vId, UserDto loggedUser)
        {
            var versions = await examsRepository.GetByVIdAsync(vId);
            return mapper.Map<List<ExamDto>>(VerifyAnswerViewPermission(loggedUser, versions));
        }

        public async Task<ExamDto?> GetVersionAsync(string vId, int versionNumber, UserDto loggedUser)
        {
            var exam = await examsRepository.GetByVIdAsync(vId, versionNumber);
            if (exam == null) return null;
            return mapper.Map<ExamDto>(VerifyAnswerViewPermission(loggedUser, exam));
        }

        public async Task<ExamDto?> GetVersionInternalAsync(string vId, int versionNumber)
        {
            var exam = await examsRepository.GetByVIdAsync(vId, versionNumber);
            if (exam == null) return null;
            return mapper.Map<ExamDto>(VerifyAnswerViewPermission(null, exam));
        }
    }
}