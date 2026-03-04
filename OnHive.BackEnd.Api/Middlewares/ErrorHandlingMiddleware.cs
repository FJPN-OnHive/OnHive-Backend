using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using OnHive.Tenants.Domain.Exceptions;
using OnHive.Users.Domain.Exceptions;
using Serilog;
using System.ComponentModel;
using System.Text.Json;

namespace OnHive.Backend.Api.Middlewaress
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
            catch (MissingParameterException ex)
            {
                Log.Logger.Warning("Missing parameter: {message}", ex.Message, ex);
                context.Response.StatusCode = 404;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
            catch (DuplicatedParameterException ex)
            {
                Log.Logger.Warning("Duplicated: {message}", ex.Message, ex);
                context.Response.StatusCode = 409;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
            catch (InvalidEnumArgumentException ex)
            {
                Log.Logger.Warning("Invalid argument: {message}", ex.Message, ex);
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(Response<StudentDto>.Invalid(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (NotFoundException ex)
            {
                Log.Logger.Warning("Not Found: {message}", ex.Message, ex);
                context.Response.StatusCode = 404;
                var result = JsonSerializer.Serialize(Response<StudentDto>.Error(ex.Message));
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