using OnHive.Teachers.Domain.Abstractions.Repositories;
using OnHive.Teachers.Domain.Abstractions.Services;
using OnHive.Teachers.Domain.Mappers;
using OnHive.Teachers.Repositories;
using OnHive.Teachers.Services;

namespace OnHive.Teachers.Api.DependencyInjection
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
