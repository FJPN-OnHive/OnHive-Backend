using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Students;
using OnHive.Students.Domain.Models;
using System.Text.Json;

namespace OnHive.Students.Domain.Abstractions.Services
{
    public interface IStudentsService
    {
        Task<StudentDto?> GetByIdAsync(string studentId, LoggedUserDto? loggedUser);

        Task<StudentDto?> GetByLoggedUserAsync(LoggedUserDto? loggedUser);

        Task<PaginatedResult<StudentDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<StudentDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<StudentDto?> SaveAsync(StudentDto studentDto, LoggedUserDto? loggedUser);

        Task<StudentDto?> CreateAsync(StudentDto studentDto, LoggedUserDto loggedUser);

        Task<StudentDto?> UpdateAsync(StudentDto studentDto, LoggedUserDto loggedUser);

        Task<StudentDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<StudentCourseResumeDto> GetCourse(LoggedUserDto loggedUser, string courseId);

        Task<StudentLessonsDto> GetLesson(LoggedUserDto loggedUser, string courseId, string lessonId);

        Task<StudentCourseResumeDto?> ValidateEnrollment(string userId, string courseId, LoggedUserDto loggedUser);

        Task<StudentCourseResumeDto?> ValidateEnrollment(string courseId, LoggedUserDto loggedUser);

        Task<PaginatedResult<StudentCourseResumeDto>> GetCourses(RequestFilter filter, LoggedUserDto loggedUser);

        Task<StudentDto?> Enroll(EnrollmentMessage enrollment, LoggedUserDto loggedUser);

        Task<StudentDto?> InternalEnroll(EnrollmentMessage enrollment);

        Task<StudentDto?> GetByCodeAsync(string studentCode, LoggedUserDto loggedUser);

        Task<StudentDto?> DeleteEnrollment(string studentId, string courseId);

        Task DeleteEnrollments(string userId);

        Task<List<string>> GetEnrollments(string userId);

        Task<StudentProgressResponseDto> SetProgress(LoggedUserDto loggedUser, StudentLessonProgressDto lessonProgress, string hostUrl);

        Task<StudentDto?> UnEnrollment(string userId, string courseId, LoggedUserDto loggedUser);

        Task<StudentDto?> FreeEnroll(EnrollmentMessage enrollment, LoggedUserDto loggedUser);

        Task EmmitCertificateAsync(string courseId, string hostUrl, LoggedUserDto loggedUser);

        Task EmmitCertificateAsync(string userId, string courseId, LoggedUserDto loggedUser, string hostUrl, bool reemission = true);

        Task<Stream?> EnrollmentReport(EnrollmentReportFilter filter, LoggedUserDto? loggedUser);

        Task<List<EnrollmentResumeDto>> EnrollmentResumeReport(EnrollmentReportFilter filter, LoggedUserDto? loggedUser);

        Task<List<SyntheticEnrollmentDto>> EnrollmentSynthetic(EnrollmentReportFilter filter, LoggedUserDto loggedUser);

        Task<PaginatedResult<StudentUserDto>> GetStudentUsersByFilterAsync(RequestFilter filter, LoggedUserDto loggedUser);

        string EnrollmentReportAsync(EnrollmentReportFilter filter, LoggedUserDto user);

        string SurveyReportAsync(EnrollmentReportFilter filter, LoggedUserDto? loggedUser, bool isSatisfaction);

        string CertificatesPendingEmmitAsync(LoggedUserDto user);
    }
}