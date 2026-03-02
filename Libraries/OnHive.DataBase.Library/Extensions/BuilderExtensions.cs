using EHive.Configuration.Library.Extensions;
using EHive.Database.Library.HealthChecks;
using EHive.Database.Library.Models;
using EHive.HealthCheck.Library.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EHive.Database.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureMongoDb(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<MongoDBSettings>();
            var mongoSettings = builder.Services.BuildServiceProvider().GetService<MongoDBSettings>();

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