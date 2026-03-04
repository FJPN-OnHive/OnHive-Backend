using OnHive.Storages.Domain.Abstractions.Repositories;
using OnHive.Storages.Domain.Abstractions.Services;
using OnHive.Storages.Domain.Mappers;
using OnHive.Storages.Repositories;
using OnHive.Storages.Services;

namespace OnHive.Storages.Api.DependencyInjection
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