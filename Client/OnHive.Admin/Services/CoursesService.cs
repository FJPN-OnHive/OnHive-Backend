using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Enums.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class CoursesService : ServiceBase<CourseDto>, ICoursesService
    {
        public CoursesService(HttpClient httpClient) : base(httpClient, "/v1/Course")
        {
        }

        public async Task<List<CourseResumeDto>> GetAllResume(string tenantId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}s/Resume/{tenantId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<CourseResumeDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload?.Itens ?? [];
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
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

        public string GetExportCoursesUrl(ExportFormats exportType, string tenantId, bool activeOnly)
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