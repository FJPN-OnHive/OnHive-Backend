using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.HealthChecks;
using OnHive.Database.Library.Models;
using OnHive.HealthCheck.Library.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace OnHive.Database.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureMongoDb(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<MongoDBSettings>();
            var mongoSettings = builder.Services.BuildServiceProvider().GetRequiredService<MongoDBSettings>();

            BsonSerializer.RegisterIdGenerator(typeof(string), new StringObjectIdGenerator());

            var healthCheckCollection = builder.Services.BuildServiceProvider().GetService<IHealthCheckCollection>();
            healthCheckCollection?.AddHealthCheck(new MongoDbHealthCheck(mongoSettings));

            return builder;
        }

        public static WebApplicationBuilder ConfigureLiteDb(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<LiteDBSettings>();
            return builder;
        }
    }
}