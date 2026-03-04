using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class UserProfileService : ServiceBase<UserProfileDto>, IUserProfileService
    {
        public UserProfileService(HttpClient httpClient) : base(httpClient, "/v1/UserProfile")
        {
        }

        public async Task<List<UserProfileDto>> GetByUserId(string userId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}s/Management/ByUser/{userId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<List<UserProfileDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
    }
}