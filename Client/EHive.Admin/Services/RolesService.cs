using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class RolesService : ServiceBase<RoleDto>, IRolesService
    {
        public RolesService(HttpClient httpClient) : base(httpClient, "v1/Role")
        {
        }

        public async Task<List<string>> GetPermissions(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"/v1/Permissions");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<List<string>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload ?? [];
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
            return [];
        }
    }
}