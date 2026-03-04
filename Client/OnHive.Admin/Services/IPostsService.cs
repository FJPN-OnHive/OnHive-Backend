using OnHive.Core.Library.Contracts.Posts;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Admin.Services
{
    public interface IPostsService : IServiceBase<BlogPostDto>
    {
        string GetExportPostsUrl(ExportFormats exportType, string tenantId, bool activeOnly);
    }
}