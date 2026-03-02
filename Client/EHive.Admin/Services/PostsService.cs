using EHive.Core.Library.Contracts.Posts;
using EHive.Core.Library.Enums.Common;

namespace EHive.Admin.Services
{
    public class PostsService : ServiceBase<BlogPostDto>, IPostsService
    {
        public PostsService(HttpClient httpClient) : base(httpClient, "/v1/Post")
        {
        }

        public string GetExportPostsUrl(ExportFormats exportType, string tenantId, bool activeOnly)
        {
            var originalString = httpClient.BaseAddress.OriginalString;
            if (originalString.EndsWith("/"))
            {
                originalString = originalString.Substring(0, originalString.Length - 1);
            }
            var activeOnlyString = activeOnly ? "" : "&activeOnly=false";
            return exportType switch
            {
                ExportFormats.Json => originalString + baseUrl + $"s/Export/{tenantId}?format=json{activeOnlyString}",
                ExportFormats.Xml => originalString + baseUrl + $"s/Export/{tenantId}?format=xml{activeOnlyString}",
                _ => originalString + baseUrl + $"s/Export/{tenantId}?format=csv{activeOnlyString}"
            };
        }
    }
}