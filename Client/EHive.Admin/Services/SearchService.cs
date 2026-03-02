using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Search;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class SearchService : ServiceBase<SearchResultDto>, ISearchService
    {
        public SearchService(HttpClient httpClient) : base(httpClient, "v1/Search")
        {
        }

        public async Task<int> TriggerProductCourseSearch(bool full, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var fullString = full ? "true" : "false";
            var response = await httpClient.GetAsync($"/v1/ProductCourse/Trigger?full={fullString}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<int>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
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
            return 0;
        }

        public async Task<int> TriggerSearch(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"/v1/Search/Trigger");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<int>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
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
            return 0;
        }
    }
}