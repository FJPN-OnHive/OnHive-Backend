using OnHive.Core.Library.Contracts.Courses;

namespace OnHive.Admin.Services
{
    public interface IExamsService : IServiceBase<ExamDto>
    {
        Task<ExamDto?> SaveVersion(ExamDto dto, string token);
    }
}