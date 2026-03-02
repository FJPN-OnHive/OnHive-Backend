using EHive.Courses.Domain.Abstractions.Repositories;
using EHive.Courses.Domain.Abstractions.Services;
using EHive.Courses.Domain.Mappers;
using EHive.Courses.Repositories;
using EHive.Courses.Services;

namespace EHive.Courses.Api.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCourses(this IServiceCollection services)
        {
            return services
                .AddRepositories()
                .AddServices()
                .AddMappers();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient<ICoursesRepository, CoursesRepository>()
                .AddTransient<ILessonsRepository, LessonsRepository>()
                .AddTransient<IExamsRepository, ExamsRepository>()
                .AddTransient<IDisciplineRepository, DisciplineRepository>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ICoursesService, CoursesService>()
                .AddTransient<ILessonsService, LessonsService>()
                .AddTransient<IExamsService, ExamsService>()
                .AddTransient<IDisciplineService, DisciplineService>();
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MappersConfig));
        }
    }
}