using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Admin.Services
{
    public class RedirectsService : ServiceBase<RedirectDto>, IRedirectsService
    {
        public RedirectsService(HttpClient httpClient) : base(httpClient, "/v1/Redirect")
        {
        }

        public string GetExportRedirectsUrl(ExportFormats exportType, string tenantId, bool exportActiveOnly)
        {
            var originalString = httpClient.BaseAddress.OriginalString;
            if (originalString.EndsWith("/"))
            {
                originalString = originalString.Substring(0, originalString.Length - 1);
            }
            var activeOnlyString = exportActiveOnly ? "" : "&activeOnly=false";
            return exportType switch
            {
                ExportFormats.Json => originalString + baseUrl + $"s/Export/{tenantId}?format=json{activeOnlyString}",
                ExportFormats.Xml => originalString + baseUrl + $"s/Export/{tenantId}?format=xml{activeOnlyString}",
                ExportFormats.GoogleXml => originalString + baseUrl + $"s/Export/{tenantId}?format=googlexml{activeOnlyString}",
                ExportFormats.GoogleTsv => originalString + baseUrl + $"s/Export/{tenantId}?format=googletsv{activeOnlyString}",
                _ => originalString + baseUrl + $"s/Export/{tenantId}?format=csv{activeOnlyString}"
            };
        }
    }
}