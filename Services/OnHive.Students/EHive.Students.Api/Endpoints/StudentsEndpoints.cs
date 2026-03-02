using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Entities.Certificates;
using EHive.Core.Library.Entities.Students;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Students.Domain.Models;
using EHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;

namespace EHive.Students.Api.Endpoints
{
    public static class StudentsEndpoints
    {
        public static WebApplication MapStudentsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Student/{StudentId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string studentId) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(studentId, user);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("GetStudentById")
            .WithDescription("Get students by ID")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<StudentDto>>();

            app.MapGet("v1/Student/ByCode/{StudentCode}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string studentCode) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByCodeAsync(studentCode, user);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("GetStudentByCode")
            .WithDescription("Get students by Code")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<StudentDto>>();

            app.MapGet("v1/Student", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetByLoggedUserAsync(user);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("GetStudentByLoggedUser")
            .WithDescription("Get students by Logged User")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<StudentDto>>();

            app.MapGet("v1/Students", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<StudentDto>>.Ok(result));
            })
            .WithName("GetStudents")
            .WithDescription("Get all students")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<PaginatedResult<StudentDto>>>();

            app.MapGet("v1/StudentUsers", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetStudentUsersByFilterAsync(filter, user);
                return Results.Ok(Response<PaginatedResult<StudentUserDto>>.Ok(result));
            })
            .WithName("GetStudentUsers")
            .WithDescription("Get all Student Users")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<PaginatedResult<StudentUserDto>>>();

            app.MapPost("v1/Student", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] StudentDto studentDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(studentDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("CreateStudent")
            .WithDescription("Create a student")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentDto>>();

            app.MapPut("v1/Student", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] StudentDto studentDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(studentDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("UpdateStudent")
            .WithDescription("Update a student")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentDto>>();

            app.MapPatch("v1/Student", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
            .WithName("PatchStudent")
            .WithDescription("Patch a student")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<StudentDto>>();

            app.MapGet("v1/Student/Courses", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetCourses(filter, loggedUser);
                if (result == null) return Results.Ok(Response<PaginatedResult<StudentCourseResumeDto>>.Empty());
                return Results.Ok(Response<PaginatedResult<StudentCourseResumeDto>>.Ok(result));
            })
            .WithName("StudentCourses")
            .WithDescription("Get student enrolled courses")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_read"))
            .Produces<Response<PaginatedResult<StudentCourseResumeDto>>>();

            app.MapGet("v1/Student/Course/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetCourse(loggedUser, courseId);
                if (result == null) return Results.Ok(Response<StudentCourseResumeDto>.Empty());
                return Results.Ok(Response<StudentCourseResumeDto>.Ok(result));
            })
           .WithName("StudentCourse")
           .WithDescription("Get student enrolled course bY Id")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create(""))
           .Produces<Response<StudentCourseResumeDto>>();

            app.MapGet("v1/Student/Lesson/{courseId}/{lessonId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string courseId, [FromRoute] string lessonId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetLesson(loggedUser, courseId, lessonId);
                if (result == null) return Results.Ok(Response<StudentCourseResumeDto>.Empty());
                return Results.Ok(Response<StudentLessonsDto>.Ok(result));
            })
           .WithName("StudentLesson")
           .WithDescription("Get student course and lesson by Id")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_read"))
           .Produces<Response<StudentLessonsDto>>();

            app.MapPut("v1/Student/Progress", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] StudentLessonProgressDto lessonProgress) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.SetProgress(loggedUser, lessonProgress, context.Request.Host.Value);
                if (result == null) return Results.Ok(Response<StudentProgressResponseDto>.Empty());
                return Results.Ok(Response<StudentProgressResponseDto>.Ok(result));
            })
           .WithName("SetStudentLessonProgress")
           .WithDescription("Set student lesson progress")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_read"))
           .Produces<Response<StudentProgressResponseDto>>();

            app.MapPost("v1/Student/Enroll", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] EnrollmentMessage enrollment) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.Enroll(enrollment, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
           .WithName("StudentEnroll")
           .WithDescription("Enroll a student to an course, create student if does not exists.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_admin"))
           .Produces<Response<StudentDto>>();

            app.MapPost("v1/Student/FreeEnroll", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] EnrollmentMessage enrollment) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.FreeEnroll(enrollment, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
           .WithName("StudentFreeEnroll")
           .WithDescription("Enroll a student to an free course, create student if does not exists.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_admin"))
           .Produces<Response<StudentDto>>();

            app.MapDelete("v1/Student/Enroll/{studentId}/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string studentId, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.DeleteEnrollment(studentId, courseId);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
           .WithName("DeleteStudentEnroll")
           .WithDescription("Delete an enrollment of an student to an course.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_admin"))
           .Produces<Response<StudentDto>>();

            app.MapPut("v1/Student/UnEnroll/{userId}/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string userId, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UnEnrollment(userId, courseId, loggedUser);
                if (result == null) return Results.Ok(Response<StudentDto>.Empty());
                return Results.Ok(Response<StudentDto>.Ok(result));
            })
           .WithName("UnEnroll")
           .WithDescription("Deactivate an enrollment of an student to an course.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_admin"))
           .Produces<Response<StudentDto>>();

            //  app.MapGet("v1/Internal/Student/Enrolls/{userId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string userId) =>
            //  {
            //      var loggedUser = context.GetLoggedUser();
            //      if (loggedUser == null) return Results.Unauthorized();
            //      var result = await service.GetEnrollments(userId);
            //      return Results.Ok(result);
            //  })
            //.WithName("GetStudentEnrolls")
            //.WithDescription("Get all students enrollments by user Id.")
            //.WithTags("Internal")
            //.Produces<List<string>>()
            //.AllowAnonymous();

            //  app.MapDelete("v1/Internal/Student/Enrolls/{userId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string userId) =>
            //  {
            //      var loggedUser = context.GetLoggedUser();
            //      if (loggedUser == null) return Results.Unauthorized();
            //      await service.DeleteEnrollments(userId);
            //      return Results.Ok(Response<StudentDto>.Ok());
            //  })
            //.WithName("DeleteStudentEnrolls")
            //.WithDescription("Delete all students enrollments by user Id.")
            //.WithTags("Internal")
            //.Produces<Response<string>>()
            //.AllowAnonymous();

            //  app.MapPost("v1/Internal/Student/Enroll", async (HttpContext context, [FromServices] IStudentsService service, [FromBody] EnrollmentMessage enrollmentMessage) =>
            //  {
            //      await service.InternalEnroll(enrollmentMessage);
            //      return Results.Ok(Response<StudentDto>.Ok());
            //  })
            //.WithName("Enroll Student")
            //.WithDescription("Enroll Student to course")
            //.WithTags("Internal")
            //.Produces<Response<string>>()
            //.AllowAnonymous();

            app.MapGet("v1/Student/ValidateEnrollment/{userId}/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string userId, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.ValidateEnrollment(userId, courseId, loggedUser);
                if (result == null) return Results.Ok(Response<StudentCourseResumeDto>.Empty());
                return Results.Ok(Response<StudentCourseResumeDto>.Ok(result));
            })
           .WithName("ValidateEnrollment")
           .WithDescription("Validate an student enrollment.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_read"))
           .Produces<Response<StudentCourseResumeDto>>();

            app.MapGet("v1/Student/ValidateEnrollment/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.ValidateEnrollment(courseId, loggedUser);
                if (result == null) return Results.Ok(Response<StudentCourseResumeDto>.Empty());
                return Results.Ok(Response<StudentCourseResumeDto>.Ok(result));
            })
           .WithName("ValidateEnrollmentForLogged")
           .WithDescription("Validate an student enrollment for logged user.")
           .WithTags("Students")
           .WithMetadata(PermissionConfig.Create("students_read"))
           .Produces<Response<StudentCourseResumeDto>>();

            app.MapGet("v1/Student/EmmitCertificate/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string courseId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.EmmitCertificateAsync(courseId, context.Request.Host.Value, loggedUser);
                return Results.Accepted();
            })
             .WithName("EmmitCertificateForLoggedByCourse")
             .WithDescription("Try to emmit certificate for logged user")
             .WithMetadata(PermissionConfig.Create("students_update"))
             .WithTags("Students");

            app.MapGet("v1/Student/EmmitCertificate/{userId}/{courseId}", async (HttpContext context, [FromServices] IStudentsService service, [FromRoute] string userId, [FromRoute] string courseId, [FromQuery(Name = "reemmit")] bool reemmit = false) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.EmmitCertificateAsync(userId, courseId, loggedUser, context.Request.Host.Value, reemmit);
                return Results.Accepted();
            })
             .WithName("EmmitCertificateByCourseAndUser")
             .WithDescription("Try to emmit certificate for user")
             .WithMetadata(PermissionConfig.Create("students_admin"))
             .WithTags("Students");

            app.MapGet("v1/Students/Enrollments", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = await service.EnrollmentReport(filter, user);
                return Results.File(result, "text/csv", $"matriculas_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
            })
            .WithName("GetStudentsEnrollments")
            .WithDescription("Get all students enrollments")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Stream>();

            app.MapGet("v1/Students/Enrollments/Synthetic", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = await service.EnrollmentSynthetic(filter, user);
                return Results.Ok(Response<List<SyntheticEnrollmentDto>>.Ok(result));
            })
            .WithName("GetStudentsEnrollmentsSynthetic")
            .WithDescription("Get all students enrollments Synthetic")
            .WithTags("Students")
            .Produces<Response<List<SyntheticEnrollmentDto>>>()
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Enrollments/AsyncAnalytic", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = service.EnrollmentReportAsync(filter, user);
                return Results.Accepted($"v1/StudentReport/{result}", Response<string>.Ok(result));
            })
           .WithName("GetEnrollmentReportAsync")
           .WithDescription("Get enrollments report Async")
           .WithTags("Students")
           .Produces<Response<string>>()
           .WithMetadata(PermissionConfig.Create("students_admin"))
           .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Surveys/AsyncAnalytic/Initial", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = service.SurveyReportAsync(filter, user, false);
                return Results.Accepted($"v1/StudentReport/{result}", Response<string>.Ok(result));
            })
            .WithName("GetSurveyReportInitialAsync")
            .WithDescription("Get surveys report Initial Async")
            .WithTags("Students")
            .Produces<Response<string>>()
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Surveys/AsyncAnalytic/Final", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = service.SurveyReportAsync(filter, user, true);
                return Results.Accepted($"v1/StudentReport/{result}", Response<string>.Ok(result));
            })
            .WithName("GetSurveyReportFinalAsync")
            .WithDescription("Get surveys report Final Async")
            .WithTags("Students")
            .Produces<Response<string>>()
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Certificates/AsyncPendingEmmit", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var result = service.CertificatesPendingEmmitAsync(user);
                return Results.Accepted($"v1/StudentReport/{result}", Response<string>.Ok(result));
            })
            .WithName("CertificatesAsyncPendingEmmit")
            .WithDescription("Certificates Async Pending Emmit")
            .WithTags("Students")
            .Produces<Response<string>>()
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Enrollments/Analytic", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = await service.EnrollmentReport(filter, user);
                return Results.File(result, "text/csv", $"matriculas_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");
            })
            .WithName("GetEnrollmentReport")
            .WithDescription("Get enrollments report")
            .WithTags("Students")
            .Produces<Stream>()
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .WithRequestTimeout(TimeSpan.FromMinutes(20));

            app.MapGet("v1/Students/Enrollments/Resume", async (HttpContext context, [FromServices] IStudentsService service) =>
            {
                var user = context.GetLoggedUser();
                if (user == null) return Results.Unauthorized();
                var filter = new EnrollmentReportFilter();

                if (context.Request.Query.ContainsKey("initial_date") && DateTime.TryParse(context.Request.Query["initial_date"], out var initialDate))
                {
                    filter.InitialDate = initialDate;
                    filter.InitialDate = new DateTime(filter.InitialDate.Year, filter.InitialDate.Month, filter.InitialDate.Day, 0, 0, 0);
                }

                if (context.Request.Query.ContainsKey("final_date") && DateTime.TryParse(context.Request.Query["final_date"], out var finallDate))
                {
                    filter.FinalDate = finallDate;
                    filter.FinalDate = new DateTime(filter.FinalDate.Year, filter.FinalDate.Month, filter.FinalDate.Day, 23, 59, 59);
                }

                if (context.Request.Query.ContainsKey("courses"))
                {
                    filter.Courses = context.Request.Query["courses"].ToString().Split(",").ToList();
                }

                var result = await service.EnrollmentResumeReport(filter, user);
                return Results.Ok(Response<List<EnrollmentResumeDto>>.Ok(result));
            })
            .WithName("GetStudentsEnrollmentsResume")
            .WithDescription("Get all students enrollments Resume")
            .WithTags("Students")
            .WithMetadata(PermissionConfig.Create("students_admin"))
            .Produces<Response<List<EnrollmentResumeDto>>>();

            return app;
        }
    }
}