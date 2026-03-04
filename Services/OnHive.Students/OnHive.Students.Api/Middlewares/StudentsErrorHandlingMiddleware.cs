using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Exceptions;
using Serilog;
using System.ComponentModel;
using System.Text.Json;

namespace OnHive.Students.Api.Middlewares
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
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(Response<StudentDto>.Unauthorized(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
            catch (DuplicatedException ex)
            {
                Log.Logger.Warning("Duplicated: {message}", ex.Message, ex);
                context.Response.StatusCode = 200;
                var result = JsonSerializer.Serialize(Response<StudentDto>.Duplicated(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
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
            catch (Exception ex)
            {
                Log.Logger.Error("Request error: {message}", ex.Message, ex);
                context.Response.StatusCode = 500;
                var result = JsonSerializer.Serialize(Response<StudentDto>.Error(ex.Message));
                var writer = context.Response.BodyWriter;
                await writer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(result));
                await writer.CompleteAsync();
            }
        }
    }
}