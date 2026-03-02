using EHive.Core.Library.Contracts.Users;
using EHive.Observability.Library.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Log = Serilog.Log;

namespace EHive.Observability.Library.Middlewares
{
    public class TracingMiddleware
    {
        private readonly RequestDelegate next;

        public TracingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public static UserDto? GetUser(HttpContext context)
        {
            if (context.Items.ContainsKey("user"))
            {
                return (UserDto?)context.Items["user"];
            }
            var token = GetToken(context);
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

        public static string? GetToken(HttpContext context)
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault();
            token = token?.Replace("bearer ", "", StringComparison.InvariantCultureIgnoreCase);
            token = token?.Replace("jwt ", "", StringComparison.InvariantCultureIgnoreCase);
            return token;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tracingInfo = new TracingInfo();
            var currentApplication = System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName.Replace(".exe", "");

            if (context.Request.Headers.TryGetValue("tracing_id", out var tracingId))
            {
                tracingInfo.TracingId = tracingId;
            }
            else
            {
                tracingInfo.TracingId = TracingInfo.NewTracingId();
                context.Request.Headers.Add("tracing_id", tracingInfo.TracingId);
            }

            if (context.Request.Headers.TryGetValue("previous_tracing_id", out var previousTracingId))
            {
                tracingInfo.PreviousTracingId = previousTracingId;
            }
            else
            {
                tracingInfo.PreviousTracingId = string.Empty;
                context.Request.Headers.Add("previous_tracing_id", tracingInfo.PreviousTracingId);
            }

            if (context.Request.Headers.TryGetValue("origin", out var origin))
            {
                tracingInfo.Origin = origin;
            }
            else
            {
                tracingInfo.Origin = currentApplication;
                context.Request.Headers.Add("origin", tracingInfo.Origin);
            }

            var user = GetUser(context);
            if (user != null)
            {
                tracingInfo.User = user.Login;
                tracingInfo.UserId = user.Id;
                tracingInfo.Tenant = user.Tenant?.Domain;
                tracingInfo.TenantId = user.TenantId;
                Log.Logger.ForContext("user", user.Login);
                Log.Logger.ForContext("user_id", user.Id);
                Log.Logger.ForContext("tenant", user.Tenant?.Domain);
                Log.Logger.ForContext("tenant_id", user.TenantId);
            }

            Log.Logger.ForContext("tracing_id", tracingInfo.TracingId);
            Log.Logger.ForContext("previous_tracing_id", tracingInfo.PreviousTracingId);
            Log.Logger.ForContext("origin", tracingInfo.Origin);
            await next(context);
        }
    }
}