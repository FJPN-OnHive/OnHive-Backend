using EHive.Core.Library.Contracts.Certificates;
using EHive.Core.Library.Contracts.Common;
using Serilog;
using System.Text.Json;

namespace EHive.Certificates.Api.Middlewares
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
            catch (ArgumentException ex)
            {
                Log.Logger.Warning("CErtificate not found: {message}", ex.Message, ex);
                context.Response.StatusCode = 404;
                var result = JsonSerializer.Serialize(Response<CertificateDto>.Empty(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Logger.Warning("Request Unauthorized: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                var result = JsonSerializer.Serialize(Response<CertificateDto>.Unauthorized(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Request error: {message}", ex.Message, ex);
                context.Response.StatusCode = 500;
                var result = JsonSerializer.Serialize(Response<CertificateDto>.Error(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
        }
    }
}