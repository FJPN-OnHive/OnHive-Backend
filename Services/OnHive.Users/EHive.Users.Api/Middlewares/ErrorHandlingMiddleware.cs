using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Exceptions;
using EHive.Users.Domain.Exceptions;
using Serilog;
using System.Text.Json;

namespace EHive.Users.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (NotValidatedEmailException ex)
            {
                Log.Logger.Warning("Not validated Email: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                var result = JsonSerializer.Serialize(Response<UserDto>.EmailNotValidated(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (DuplicatedUserException ex)
            {
                Log.Logger.Warning("Duplicated user: {message}", ex.Message, ex);
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(Response<UserDto>.Duplicated(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (DuplicatedException ex)
            {
                Log.Logger.Warning("Duplicated: {message}", ex.Message, ex);
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(Response<UserDto>.Duplicated(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (InvalidUserException ex)
            {
                Log.Logger.Warning("Invalid user: {message}", ex.Message, ex);
                context.Response.StatusCode = 200;
                context.Response.Headers.ContentType = "application/json";
                var result = JsonSerializer.Serialize(Response<UserDto>.Invalid(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Logger.Warning("Request Unauthorized: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                context.Response.Headers.ContentType = "application/json";
                var result = JsonSerializer.Serialize(Response<UserDto>.Unauthorized(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Request error: {message}", ex.Message, ex);
                context.Response.StatusCode = 500;
                context.Response.Headers.ContentType = "application/json";
                var result = JsonSerializer.Serialize(Response<UserDto>.Error(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
        }
    }
}