using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Exceptions;
using Serilog;
using System.Data;
using System.Text.Json;

namespace EHive.Storages.Api.Middlewares
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
            catch (UnauthorizedAccessException ex)
            {
                Log.Logger.Warning("Request Unauthorized: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                var result = JsonSerializer.Serialize(Response<string>.Unauthorized(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (NotFoundException ex)
            {
                Log.Logger.Warning("Not found: {message}", ex.Message, ex);
                context.Response.StatusCode = 404;
                var result = JsonSerializer.Serialize(Response<string>.Empty(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (DuplicateNameException ex)
            {
                Log.Logger.Warning("Duplicated image id: {message}", ex.Message, ex);
                context.Response.StatusCode = 409;
                var result = JsonSerializer.Serialize(Response<string>.Empty(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (ArgumentException ex)
            {
                Log.Logger.Warning("Invalid format: {message}", ex.Message, ex);
                context.Response.StatusCode = 400;
                var result = JsonSerializer.Serialize(Response<string>.Empty(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Request error: {message}", ex.Message, ex);
                context.Response.StatusCode = 500;
                var result = JsonSerializer.Serialize(Response<UserDto>.Error(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
        }
    }
}