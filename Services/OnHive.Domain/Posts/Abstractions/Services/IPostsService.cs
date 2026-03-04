using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Posts;
using OnHive.Core.Library.Contracts.Login;
using System.Text.Json;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Posts.Domain.Abstractions.Services
{
    public interface IPostsService
    {
        Task<string> HouseKeeping();

        Task<string> ScheduleExecute();

        Task<BlogPostDto?> GetByIdAsync(string postId);

        Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId);

        Task<PaginatedResult<BlogPostDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<PaginatedResult<BlogPostDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<PaginatedResult<BlogPostDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId);

        Task<PaginatedResult<BlogPostDto>> GetResumeByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<List<BlogPostDto>> GetByCourseAsync(string courseId, LoggedUserDto? loggedUser);

        Task<IEnumerable<BlogPostDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<BlogPostDto> SaveAsync(BlogPostDto BlogPostDto, LoggedUserDto? user);

        Task<BlogPostDto> CreateAsync(BlogPostDto BlogPostDto, LoggedUserDto? loggedUser);

        Task<BlogPostDto?> UpdateAsync(BlogPostDto BlogPostDto, LoggedUserDto? loggedUser);

        Task<bool> LikeAsync(string postId, LoggedUserDto? loggedUser);

        Task<bool> UnlikeAsync(string postId, LoggedUserDto? loggedUser);

        Task<bool> DeleteByIdAsync(string postId, LoggedUserDto loggedUser);

        Task<FilterScope> GetFilterScopeAsync(string tenantId);

        Task<List<string>> GetSlugsAsync(string tenantId);

        Task<BlogPostDto> CreateDraftAsync(BlogPostDto blogPostDto, LoggedUserDto loggedUser);

        Task<BlogPostDto> PatchAsync(JsonDocument blogPostPatch, LoggedUserDto loggedUser);

        Task<Stream> GetExportData(ExportFormats exportFormat, string tenantId, bool activeOnly);

        Task<Dictionary<string, string>> GetCanonicalAsync(string tenantId);
    }
}