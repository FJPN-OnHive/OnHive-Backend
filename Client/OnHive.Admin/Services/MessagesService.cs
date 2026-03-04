using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class MessagesService : ServiceBase<MessageDto>, IMessagesService
    {
        public MessagesService(HttpClient httpClient) : base(httpClient, "/v1/Message")
        {
        }

        public async Task<List<MessageUserDto>> GetAllByUser(bool newOnly, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}s/User?newOnly={newOnly}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<List<MessageUserDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload ?? [];
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

        public async Task<MessageUserDto> SaveUserMessageAsync(MessageUserDto dto, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{baseUrl}/User", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<MessageUserDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
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
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicatedException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                try
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Response<MessageUserDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    throw new HttpRequestException(result?.Message);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<bool> SendMessageAsync(MessageDto dto)
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{baseUrl}/Send", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return true;
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicatedException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                try
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    throw new HttpRequestException(result?.Message);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}