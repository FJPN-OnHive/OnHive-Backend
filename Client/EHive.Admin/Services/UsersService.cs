using EHive.Admin.Components;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace EHive.Admin.Services
{
    public class UsersService : ServiceBase<UserDto>, IUsersService
    {
        public UsersService(HttpClient httpClient) : base(httpClient, "/v1/User")
        {
        }

        public async Task<bool> ChangePassword(ChangePasswordDto changePasswordDto, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(changePasswordDto), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync("v1/User/ChangePassword", content);
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new ArgumentException(result?.Message);
            }
            else
            {
                throw new UnauthorizedAccessException(response?.ReasonPhrase);
            }
        }

        public async Task<string> CreateTempPassword(string userId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"v1/User/{userId}/CreateTempPassword");
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Payload ?? string.Empty;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<string>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new ArgumentException(result?.Message);
            }
            else
            {
                throw new UnauthorizedAccessException(response?.ReasonPhrase);
            }
        }

        public async Task<PaginatedResult<UserDto>> GetByProfilePaginated(string profile, RequestFilter filter, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            var response = await httpClient.GetAsync($"v1/Users/ByProfile/{profile}?{filter.ToString()}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<UserDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<UserEmailDto?> ValidateEmail(string email, string userId, string tenantId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/ValidateEmail/{tenantId}/{userId}/{email}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<UserEmailDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}