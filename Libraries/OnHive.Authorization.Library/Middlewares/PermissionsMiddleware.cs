using OnHive.Authorization.Library.Extensions;
using OnHive.Authorization.Library.Models;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnHive.Domains.Common.Helpers;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace OnHive.Authorization.Library.Middlewares
{
    public class PermissionsMiddleware
    {
        private readonly RequestDelegate next;

        public PermissionsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var result = ValidateAccess(context);
            if (string.IsNullOrEmpty(result))
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = 401;
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Response<UserDto>.Error(result))));
                await writer.CompleteAsync();
            }
        }

        private static string ValidateAccess(HttpContext context)
        {
            if (!ValidateToken(context))
            {
                Log.Logger.Warning("Permission denied invalid JWT token: {token}", context.GetToken());
                return "Permission denied invalid JWT token.";
            }

            var endpointName = GetEndpointName(context);
            var user = context.GetUser();
            var permissions = context.GetEndpoint()?.Metadata.GetMetadata<PermissionConfig>();
            if (permissions != null && permissions.Permissions.Any(p => !p.Equals("unvalidated_email", StringComparison.CurrentCultureIgnoreCase)))
            {
                if (!permissions.Permissions
                    .Any(p =>
                        user != null
                        && user.Permissions != null
                        && (user.Permissions.Any(p => p.Equals(p, StringComparison.CurrentCultureIgnoreCase))
                            || user.Permissions.Any(pc => pc.Equals(PermissionConsts.Admin, StringComparison.CurrentCultureIgnoreCase)))))

                {
                    Log.Logger.Warning("User {user} permission denied ({permissions}) for {endpoint}", user?.Login, permissions, endpointName);
                    return $"User {user?.Login} permission denied";
                }

                if (!permissions.Permissions
                    .Any(p =>
                        user != null
                        && user.Permissions != null
                        && p.Equals(PermissionConsts.SystemAdmin, StringComparison.CurrentCultureIgnoreCase)
                        && user.Permissions.Any(p => p.Equals(p, StringComparison.CurrentCultureIgnoreCase))))
                {
                    Log.Logger.Warning("User {user} system permission denied ({permissions}) for {endpoint}", user?.Login, permissions, endpointName);
                    return $"User {user?.Login} system permission denied";
                }

                if (user != null
                    && !user.Emails.Any(e => e.IsValidated && e.IsMain)
                    && !(permissions.AllowUnvalidatedEmail
                        || user.Permissions.Any(p => !p.Equals(PermissionConsts.Admin, StringComparison.CurrentCultureIgnoreCase))))
                {
                    Log.Logger.Warning("User {user} permission denied ({permissions}) for {endpoint}, email not validated.", user?.Login, permissions, endpointName);
                    return $"User {user?.Login} email not validated.";
                }
            }

            return string.Empty;
        }

        private static bool ValidateToken(HttpContext context)
        {
            if (context.GetEndpoint() is Endpoint endpoint)
            {
                var settings = ServiceProviderFactory.ServiceProvider?.GetService<AuthSettings>();
                if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null || settings == null)
                {
                    return true;
                }
                var token = context.GetToken();
                if (token == null)
                {
                    Log.Logger.Warning("JWT token validation failed: Missing token");
                    return false;
                }
                var key = Encoding.ASCII.GetBytes(settings?.Secret ?? string.Empty);
                var audience = settings?.Audience;
                var issuer = settings?.Issuer;
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = !string.IsNullOrEmpty(issuer),
                        ValidIssuer = issuer,
                        ValidateAudience = !string.IsNullOrEmpty(audience),
                        ValidAudience = audience,
                        ClockSkew = TimeSpan.Zero
                    }, out var validatedToken);
                }
                catch (Exception ex)
                {
                    Log.Logger.Warning("JWT token validation failed: {message}", ex.Message);
                    return false;
                }
            }
            return true;
        }

        private static string GetEndpointName(HttpContext context)
        {
            if (context.GetEndpoint() is Endpoint endpoint)
            {
                return endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? string.Empty;
            }
            return string.Empty;
        }
    }
}