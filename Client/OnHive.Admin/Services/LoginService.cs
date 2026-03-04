using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using System.Text;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class LoginService : ILoginService
    {
        private readonly HttpClient httpClient;

        public LoginService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<LoginResponseDto> Login(LoginDto login)
        {
            login.RemindMe = false;
            login.AppName = "ADMIN";
            httpClient.DefaultRequestHeaders.Authorization = null;
            var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/v1/Login", content);
            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Response<LoginResponseDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
            {
                return result.Payload ?? throw new UnauthorizedAccessException(result.Message);
            }
            else
            {
                throw new UnauthorizedAccessException(result?.Message);
            }
        }
    }
}