using OnHive.Configuration.Library.Extensions;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Courses.Domain.Models;
using OnHive.Events.Api.DependencyInjection;

namespace OnHive.Courses.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureCoursesApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<CoursesApiSettings>();
            builder.Services.AddCourses();
            builder.ConfigureEventRegister("Courses");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CourseCreated,
                Message = "Event triggered when a course is created",
                Origin = "Courses",
                Tags = new List<string> { "Course", "Created" },
                Fields = new Dictionary<string, string> {
                    { "CourseId", "The id of the course" },
                    { "CourseName", "Name Of the Course" },
                    { "Current Values", "Current Values" },
                    { "PreviousValues", "Previous values" },
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.CourseUpdated,
                Message = "Event triggered when a Course is updated",
                Origin = "Courses",
                Tags = new List<string> { "Course", "Updated" },
                Fields = new Dictionary<string, string> {
                   { "CourseId", "The id of the course" },
                    { "CourseName", "Name Of the Course" },
                    { "Current Values", "Current Values" },
                    { "PreviousValues", "Previous values" },
                }
            });

            return builder;
        }
    }
}