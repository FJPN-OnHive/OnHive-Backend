using EHive.Storages.Domain.Abstractions.Repositories;
using EHive.Storages.Domain.Abstractions.Services;
using EHive.Storages.Domain.Mappers;
using EHive.Storages.Repositories;
using EHive.Storages.Services;

namespace EHive.Storages.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IStorageImagesRepository, StorageImagesRepository>()
                .AddTransient<IStorageFilesRepository, StorageFilesRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IStorageImagesService, StorageImagesService>()
                .AddTransient<IStorageFilesService, StorageFilesService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}