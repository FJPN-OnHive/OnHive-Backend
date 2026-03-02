using EHive.Tenants.Domain.Exceptions;
using Serilog;

namespace EHive.Tenants.Api.Middlewares
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
            catch (MissingParameterException ex)
            {
                Log.Logger.Warning("Missing parameter: {message}", ex.Message, ex);
                context.Response.StatusCode = 404;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
            catch (DuplicatedParameterException ex)
            {
                Log.Logger.Warning("Duplicated parameter: {message}", ex.Message, ex);
                context.Response.StatusCode = 409;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Logger.Warning("Request Unauthorized: {message}", ex.Message, ex);
                context.Response.StatusCode = 401;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Request error: {message}", ex.Message, ex);
                context.Response.StatusCode = 500;
                context.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ex.Message));
            }
        }
    }
}