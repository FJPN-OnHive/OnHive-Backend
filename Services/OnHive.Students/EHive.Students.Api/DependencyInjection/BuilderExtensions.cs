using EHive.Configuration.Library.Extensions;
using EHive.Core.Library.Contracts.Events;
using EHive.Students.Domain.Models;
using EHive.Events.Api.DependencyInjection;

namespace EHive.Students.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureStudentsApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<StudentsApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers(); builder.ConfigureEventRegister("Students");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.StudentCreated,
                Message = "Event triggered when an student is created",
                Origin = "Students",
                Tags = new List<string> { "student", "Created" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the student" }        ,
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.StudentUpdated,
                Message = "Event triggered when an student is updated",
                Origin = "Students",
                Tags = new List<string> { "student", "updated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the student" }        ,
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.StudentDeleted,
                Message = "Event triggered when an student is deleted",
                Origin = "Students",
                Tags = new List<string> { "student", "deleted" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the student" }        ,
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.EnrollmentCreated,
                Message = "Event triggered when an Enrollment is created",
                Origin = "Enrollments",
                Tags = new List<string> { "Enrollment", "Created" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the Student" },
                    { "CourseId", "Course Id"},
                    { "ProductId", "Product Id"},
                    { "ProductSku", "Product Sku"},
                    { "ProductName", "Product Name"},
                    { "ProductDescription", "Product Dscription"},
                    { "CourseName", "Course Name"},
                    { "CourseDescription", "Course Description"},
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.EnrollmentUpdated,
                Message = "Event triggered when an Enrollment is updated",
                Origin = "Enrollments",
                Tags = new List<string> { "Enrollment", "updated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the Student" },
                    { "CourseId", "Course Id"},
                    { "ProductId", "Product Id"},
                    { "ProductSku", "Product Sku"},
                    { "ProductName", "Product Name"},
                    { "ProductDescription", "Product Dscription"},
                    { "CourseName", "Course Name"},
                    { "CourseDescription", "Course Description"},
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.EnrollmentDeleted,
                Message = "Event triggered when an Enrollment is deleted",
                Origin = "Enrollments",
                Tags = new List<string> { "Enrollment", "deleted" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the Student" },
                    { "CourseId", "Course Id"},
                    { "ProductId", "Product Id"},
                    { "ProductSku", "Product Sku"},
                    { "ProductName", "Product Name"},
                    { "ProductDescription", "Product Dscription"},
                    { "CourseName", "Course Name"},
                    { "CourseDescription", "Course Description"},
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.AllEnrollmentsDeleted,
                Message = "Event triggered when all Enrollments are deleted",
                Origin = "Enrollments",
                Tags = new List<string> { "Enrollment", "deleted", "all" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "StudentId", "The id of the Student" },
                    { "CourseId", "Course Id"},
                    { "StudentCode", "Student Code"},
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" }
                }
            });

            return builder;
        }
    }
}