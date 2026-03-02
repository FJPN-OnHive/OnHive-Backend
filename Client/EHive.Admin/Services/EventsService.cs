using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class EventsService : ServiceBase<EventRegisterDto>, IEventsService
    {
        public EventsService(HttpClient httpClient) : base(httpClient, "/v1/EventRegister")
        {
        }

        public async Task<List<EventResumeDto>> GetByFilter(RequestFilter filter, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}s/Resume?{filter}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<EventResumeDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload?.Itens ?? [];
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return [];
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