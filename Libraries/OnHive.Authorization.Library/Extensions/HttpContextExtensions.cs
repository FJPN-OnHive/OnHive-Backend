using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace OnHive.Authorization.Library.Extensions
{
    public static class HttpContextExtensions
    {
        public static UserDto? GetUser(this HttpContext context)
        {
            if (context.Items.ContainsKey("user"))
            {
                return (UserDto?)context.Items["user"];
            }
            var token = context.GetToken();
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                if (jwtSecurityToken.Payload.TryGetValue("user", out var userObject))
                {
                    var user = JsonSerializer.Deserialize<UserDto>(userObject.ToString() ?? "", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    context.Items.Add("user", user);
                    return user;
                }
            }
            return null;
        }

        public static string? GetToken(this HttpContext context, bool clear = true)
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault();
            if (clear)
            {
                token = token?.Replace("bearer ", "", StringComparison.InvariantCultureIgnoreCase);
                token = token?.Replace("jwt ", "", StringComparison.InvariantCultureIgnoreCase);
            }
            return token;
        }

        public static LoggedUserDto? GetLoggedUser(this HttpContext context, bool clear = true)
        {
            return new LoggedUserDto(GetUser(context), GetToken(context, clear) ?? string.Empty);
        }
    }
}