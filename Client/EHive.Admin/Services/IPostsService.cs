using EHive.Core.Library.Contracts.Posts;
using EHive.Core.Library.Enums.Common;

namespace EHive.Admin.Services
{
    public interface IPostsService : IServiceBase<BlogPostDto>
    {
        string GetExportPostsUrl(ExportFormats exportType, string tenantId, bool activeOnly);
    }
}