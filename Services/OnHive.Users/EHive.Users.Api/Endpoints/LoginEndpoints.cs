using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Users.Services.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Users.Api.Endpoints
{
    internal static class LoginEndpoints
    {
        internal static WebApplication MapLoginEndpoints(this WebApplication app)
        {
            app.MapPost("v1/Login", async (HttpContext context, [FromServices] ILoginService service, [FromBody] LoginDto userId) =>
            {
                var result = await service.LoginAsync(userId);
                return Results.Ok(Response<LoginResponseDto>.Ok(result));
            })
            .WithName("Login")
            .WithDescription("Execute login, returs user information and jwt token.")
            .WithTags("Login")
            .Produces<Response<LoginResponseDto>>()
            .AllowAnonymous();

            //app.MapGet("v1/Internal/Login/Impersonate/{userId}", async (HttpContext context, [FromServices] ILoginService service, [FromRoute] string userId) =>
            //{
            //    var result = await service.ImpersonateAsync(userId);
            //    if (result == null) return Results.Unauthorized();
            //    return Results.Ok(Response<LoginResponseDto>.Ok(result));
            //})
            //.WithName("ImpersonateUser")
            //.WithDescription("Internal impersonate user for system use")
            //.WithTags("Internal")
            //.Produces<Response<LoginResponseDto>>()
            //.AllowAnonymous();

            //app.MapPost("v1/Internal/Login/Basic/Expired", async (HttpContext context, [FromServices] ILoginService service, [FromBody] LoginDto login) =>
            //{
            //    var result = await service.LoginBasicExpiredAsync(login);
            //    if (result == null) return Results.Unauthorized();
            //    return Results.Ok(Response<LoginResponseDto>.Ok(result));
            //})
            //.WithName("LoginBasicExpired")
            //.WithDescription("Internal login, with expired token for testing only")
            //.WithTags("Internal")
            //.Produces<Response<LoginResponseDto>>()
            //.AllowAnonymous();

            app.MapPost("v1/Login/PasswordRecover", async (HttpContext context, [FromServices] ILoginService service, [FromBody] RecoverPasswordDto recoverPassword) =>
            {
                await service.PasswordRecoverAsync(recoverPassword);
                return Results.Ok(Response<string>.Ok());
            })
            .WithName("PasswordRecover")
            .WithDescription("Password recovery (second step)")
            .WithTags("Login")
            .Produces<Response<string>>()
            .AllowAnonymous();

            app.MapGet("v1/Login/RequestPasswordRecover", async (HttpContext context, [FromServices] ILoginService service, [FromQuery] string email, [FromQuery] string tenantId) =>
            {
                await service.RequestPasswordRecoverAsync(email, tenantId);
                return Results.Ok(Response<string>.Ok());
            })
           .WithName("RequestPasswordRecover")
           .WithDescription("Password recovery request (first step)")
           .WithTags("Login")
           .Produces<Response<string>>()
           .AllowAnonymous();

            app.MapGet("v1/Login/Refresh", async (HttpContext context, [FromServices] ILoginService service) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Unauthorized();
                var token = context.GetToken();
                if (string.IsNullOrEmpty(token)) return Results.Unauthorized();
                var result = await service.RefreshToken(token, loggedUser);
                return Results.Ok(Response<LoginResponseDto>.Ok(result));
            })
           .WithName("RefreshToken")
           .WithDescription("Refresh token, used when a token expires to get a new token, if 'remember me' flag was set to true on login.")
           .WithTags("Login")
           .Produces<Response<LoginResponseDto>>()
           .AllowAnonymous();

            app.MapGet("v1/Login/Validate", async (HttpContext context) =>
            {
                var loggedUser = context.GetUser();
                if (loggedUser == null) return Results.Ok(Response<string>.Invalid("Invalid"));
                var token = context.GetToken();
                if (!TokenHelper.ValidateToken(token)) return Results.Ok(Response<string>.Invalid("Invalid"));
                return Results.Ok(Response<string>.Ok("Valid"));
            })
            .WithName("ValidateToken")
            .WithDescription("Validate token.")
            .WithTags("Login")
            .Produces<Response<string>>()
            .AllowAnonymous();

            return app;
        }
    }
}