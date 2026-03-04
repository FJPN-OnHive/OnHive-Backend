using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using OnHive.Payments.Domain.Exceptions;
using Serilog;
using System.Text.Json;

namespace OnHive.Payments.Api.Middlewares
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
            catch (InvalidPaymentProviderException ex)
            {
                Log.Logger.Warning("Invalid payment provider: {message}", ex.Message, ex);
                context.Response.StatusCode = 400;
                var result = JsonSerializer.Serialize(Response<string>.Invalid(ex.Message));
                context.Response.Headers.ContentType = "application/json";
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (InvalidPaymentException ex)
            {
                Log.Logger.Warning("Invalid payment: {message}", ex.Message, ex);
                context.Response.StatusCode = 400;
                var result = JsonSerializer.Serialize(Response<string>.Invalid(ex.Message));
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