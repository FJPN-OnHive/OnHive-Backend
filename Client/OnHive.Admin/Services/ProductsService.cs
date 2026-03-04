using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Enums.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class ProductsService : ServiceBase<ProductDto>, IProductsService
    {
        public ProductsService(HttpClient httpClient) : base(httpClient, "/v1/Product")
        {
        }

        public async Task<string> RefreshProductsPricesAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/RefreshPrices");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Ok";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public string GetExportProducUrl(ExportFormats exportType, string tenantId, bool activeOnly)
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
                ExportFormats.GoogleXml => originalString + baseUrl + $"s/Export/{tenantId}?format=googlexml{activeOnlyString}",
                ExportFormats.GoogleTsv => originalString + baseUrl + $"s/Export/{tenantId}?format=googletsv{activeOnlyString}",
                _ => originalString + baseUrl + $"s/Export/{tenantId}?format=csv{activeOnlyString}"
            };
        }
    }
}