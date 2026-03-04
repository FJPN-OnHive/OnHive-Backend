using OnHive.Students.Domain.Abstractions.Repositories;
using OnHive.Students.Domain.Abstractions.Services;
using OnHive.Students.Domain.Mappers;
using OnHive.Students.Repositories;
using OnHive.Students.Services;

namespace OnHive.Students.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<IStudentsRepository, StudentsRepository>()
                .AddTransient<IStudentReportsRepository, StudentReportsRepository>()
                .AddTransient<IStudentActivitiesRepository, StudentActivitiesRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IStudentsService, StudentsService>()
                .AddTransient<IStudentReportsService, StudentReportsService>()
                .AddTransient<IStudentActivitiesService, StudentActivitiesService>();
        }

        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}