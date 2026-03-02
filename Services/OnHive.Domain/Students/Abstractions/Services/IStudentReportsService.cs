using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Students;
using System.Text.Json;

namespace EHive.Students.Domain.Abstractions.Services
{
    public interface IStudentReportsService
    {
        Task<StudentReportDto?> GetByIdAsync(string reportId, LoggedUserDto? loggedUser);

        Task<PaginatedResult<StudentReportDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<StudentReportDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<StudentReportDto?> SaveAsync(StudentReportDto studentReportDto, LoggedUserDto? loggedUser);

        Task<StudentReportDto?> CreateAsync(StudentReportDto studentReportDto, LoggedUserDto loggedUser);

        Task<StudentReportDto?> UpdateAsync(StudentReportDto studentReportDto, LoggedUserDto loggedUser);

        Task<StudentReportDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<PaginatedResult<StudentReportDto>> GetByFilterAndTypeAsync(RequestFilter filter, string type, LoggedUserDto loggedUser);
    }
}