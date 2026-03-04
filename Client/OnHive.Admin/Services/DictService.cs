using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Dict;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class DictService : ServiceBase<ValueRegistryDto>, IDictService
    {
        public DictService(HttpClient httpClient) : base(httpClient, "/v1/Value")
        {
        }

        public async Task<ValueRegistryDto?> GetCompleteDataAsync(string tenantId, string group, string key)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/Complete/{tenantId}/{group}/{key}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<ValueRegistryDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
                }
                else if (result?.Code == Core.Library.Enums.Common.ResponseCode.Empty)
                {
                    return null;
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
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
    }
}