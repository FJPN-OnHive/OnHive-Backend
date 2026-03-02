using EHive.Teachers.Domain.Abstractions.Repositories;
using EHive.Teachers.Domain.Abstractions.Services;
using EHive.Teachers.Domain.Mappers;
using EHive.Teachers.Repositories;
using EHive.Teachers.Services;

namespace EHive.Teachers.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ITeachersRepository, TeachersRepository>(); 
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ITeachersService, TeachersService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}
