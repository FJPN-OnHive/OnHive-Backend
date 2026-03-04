using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using Serilog;
using System.Text.Json;


namespace OnHive.Catalog.Api.Middlewares
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
            catch (RedirectException ex)
            {
                Log.Logger.Warning(ex.Message, ex);
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(ex.Response.GetResponse());
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Logger.Warning("Request Unauthorized: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                var result = JsonSerializer.Serialize(Response<UserDto>.Unauthorized(ex.Message));
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
