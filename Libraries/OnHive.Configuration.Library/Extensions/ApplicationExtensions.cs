using Microsoft.AspNetCore.Builder;

namespace EHive.Configuration.Library.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication AddSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            return app;
        }
    }
}