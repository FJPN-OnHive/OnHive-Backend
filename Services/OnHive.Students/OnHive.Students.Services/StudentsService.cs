using AutoMapper;
using ClosedXML.Excel;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Certificates.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Enums.Courses;
using OnHive.Core.Library.Enums.Students;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Storages.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Repositories;
using OnHive.Students.Domain.Abstractions.Services;
using OnHive.Students.Domain.Models;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Videos.Domain.Abstractions.Services;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace OnHive.Students.Services
{
    public class StudentsService : IStudentsService
    {
        private readonly IStudentsRepository studentsRepository;
        private readonly IStudentReportsRepository studentReportsRepository;
        private readonly IUsersService usersService;
        private readonly ICoursesService coursesService;
        private readonly ICertificatesService certificatesService;
        private readonly IProductsService productsService;
        private readonly IExamsService examsService;
        private readonly IVideosService videosService;
        private readonly IStorageFilesService storageFilesService;

        private readonly StudentsApiSettings studentsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEventRegister eventRegister;
        private readonly IStudentActivitiesService activitiesService;

        public StudentsService(IStudentsRepository studentsRepository,
                               IStudentReportsRepository studentReportsRepository,
                               StudentsApiSettings studentsApiSettings,
                               IMapper mapper,
                               IEventRegister eventRegister,
                               IServicesHub servicesHub)
        {
            this.studentsRepository = studentsRepository;
            this.studentReportsRepository = studentReportsRepository;
            this.studentsApiSettings = studentsApiSettings;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            this.activitiesService = servicesHub.StudentActivitiesService;
            this.usersService = servicesHub.UsersService;
            this.productsService = servicesHub.ProductsService;
            this.coursesService = servicesHub.CoursesService;
            this.certificatesService = servicesHub.CertificatesService;
            this.examsService = servicesHub.ExamsService;
            this.videosService = servicesHub.VideosService;
            this.storageFilesService = servicesHub.StorageFilesService;
            logger = Log.Logger;
        }

        public async Task<Stream?> EnrollmentReport(EnrollmentReportFilter filter, LoggedUserDto? loggedUser)
        {
            var enrollments = await studentsRepository.GetAnalyticByReportFilterAsync(filter, loggedUser?.User?.TenantId);
            return EnrollmentsToCsvStream(enrollments.ToList());
        }

        public string EnrollmentReportAsync(EnrollmentReportFilter filter, LoggedUserDto? loggedUser)
        {
            var fileName = $"enrollments_{DateTime.Now.ToLocalTime().ToString("yyyy_MM_dd_HH_mm_ss")}.xlsx";
            var reportId = Guid.NewGuid().ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    var enrollments = await studentsRepository.GetAnalyticByReportFilterAsync(filter, loggedUser?.User?.TenantId);
                    var fileStream = EnrollmentsToXlsxStream(enrollments.ToList());
                    var formData = new StorageFileDto()
                    {
                        TenantId = loggedUser!.User!.TenantId,
                        Name = fileName,
                        Description = "Student enrollments report",
                        Public = false
                    };
                    var reportFileResponse = await storageFilesService.UploadFileAsync(fileStream, formData, "reports");
                    if (reportFileResponse != null)
                    {
                        var report = new StudentReport
                        {
                            Id = reportId,
                            IsActive = true,
                            ReportName = "EnrollmentReportAsync",
                            FileUrl = reportFileResponse.FileUrl,
                            ReportDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = loggedUser.User.Id,
                            TenantId = loggedUser!.User!.TenantId
                        };
                        await studentReportsRepository.SaveAsync(report);
                        logger.Information($"Student Enrollments report Stored: {reportId}");
                    }
                    else
                    {
                        logger.Error($"Student Enrollments report error: {reportId}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error generating Student Enrollments report: {ex.Message}");
                }
            });

            return reportId;
        }

        public string SurveyReportAsync(EnrollmentReportFilter filter, LoggedUserDto? loggedUser, bool isSatisfaction)
        {
            var queryType = "Inicial$";
            var fileType = "initial";
            var reportName = "SurveyReportAsync-Initial";
            if (isSatisfaction)
            {
                queryType = "Satisfação$";
                fileType = "satisfaction";
                reportName = "SurveyReportAsync-Final";
            }
            var fileName = $"survey_{fileType}_{DateTime.Now.ToLocalTime().ToString("yyyy_MM_dd_HH_mm_ss")}.xlsx";
            var reportId = Guid.NewGuid().ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    var storageHttpClient = new HttpClient();
                    var surveys = await studentsRepository.GetSurveyReportByFilterAsync(filter, loggedUser?.User?.TenantId, queryType);
                    var fileStream = SurveyToXlsxStream(surveys.ToList());
                    var formData = new StorageFileDto()
                    {
                        TenantId = loggedUser!.User!.TenantId,
                        Name = fileName,
                        Description = "Student surveys report",
                        Public = false
                    };
                    var reportFileResponse = await storageFilesService.UploadFileAsync(fileStream, formData, "reports");
                    if (reportFileResponse != null)
                    {
                        var report = new StudentReport
                        {
                            Id = reportId,
                            IsActive = true,
                            ReportName = reportName,
                            FileUrl = reportFileResponse.FileUrl,
                            ReportDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = loggedUser.User.Id,
                            TenantId = loggedUser!.User!.TenantId
                        };
                        await studentReportsRepository.SaveAsync(report);
                        logger.Information($"Student Surveys report Stored: {fileName}");
                    }
                    else
                    {
                        logger.Error($"Student Surveys report error: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error generating Student Survey report: {ex.Message}");
                }
            });

            return reportId;
        }

        public string CertificatesPendingEmmitAsync(LoggedUserDto loggedUser)
        {
            var fileName = $"enrollments_{DateTime.Now.ToLocalTime().ToString("yyyy_MM_dd_HH_mm_ss")}.csv";
            var reportId = Guid.NewGuid().ToString();

            _ = Task.Run(async () =>
            {
                try
                {
                    var storageHttpClient = new HttpClient();
                    var filter = new EnrollmentReportFilter();
                    filter.InitialDate = DateTime.UtcNow.AddYears(-1);
                    filter.FinalDate = DateTime.UtcNow;
                    var enrollments = await studentsRepository.GetAnalyticByReportFilterAsync(filter, loggedUser?.User?.TenantId);
                    var (emmitList, fileStream) = EnrollmentsToPendingCertificate(enrollments.ToList());

                    foreach (var emmission in emmitList)
                    {
                        try
                        {
                            await EmmitCertificateAsync(emmission.UserId, emmission.CourseId, loggedUser);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"Error emmiting certificate: {ex.Message}, ");
                        }
                    }
                    var formData = new StorageFileDto()
                    {
                        TenantId = loggedUser!.User!.TenantId,
                        Name = fileName,
                        Description = "Student certificate emmit report",
                        Public = false
                    };
                    var reportFileResponse = await storageFilesService.UploadFileAsync(fileStream, formData, "reports");
                    if (reportFileResponse != null)
                    {
                        var report = new StudentReport
                        {
                            Id = reportId,
                            IsActive = true,
                            ReportName = "PendingCertificateEmmitReportAsync",
                            FileUrl = reportFileResponse.FileUrl,
                            ReportDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = loggedUser.User.Id,
                            TenantId = loggedUser!.User!.TenantId
                        };
                        await studentReportsRepository.SaveAsync(report);
                        logger.Information($"Student certificate emmit report Stored: {fileName}");
                    }
                    else
                    {
                        logger.Error($"Student certificate emmit report error: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error generating Student Enrollments report: {ex.Message}");
                }
            });

            return reportId;
        }

        public async Task<List<SyntheticEnrollmentDto>> EnrollmentSynthetic(EnrollmentReportFilter filter, LoggedUserDto loggedUser)
        {
            var enrollments = await studentsRepository.GetSyntheticByReportFilterAsync(filter, loggedUser?.User?.TenantId);
            return enrollments.ToList();
        }

        public async Task<List<EnrollmentResumeDto>> EnrollmentResumeReport(EnrollmentReportFilter filter, LoggedUserDto? loggedUser)
        {
            var students = await studentsRepository.GetByReportFilterAsync(filter, loggedUser?.User?.TenantId);
            if (students != null && students.Any())
            {
                var enrollments = students
                    .SelectMany(s => s.Courses
                    .Select(c => new EnrollmentResumeDto
                    {
                        TenantId = s.TenantId,
                        CourseId = c.Id,
                        TotalStudents = 1,
                        CertificatesEmitted = string.IsNullOrEmpty(c.CertificateId) ? 0 : 1,
                        LastEnrollmentDate = c.StartTime.ToLocalTime(),
                        LastAccess = c.LastAccessedDate.ToLocalTime()
                    }))
                    .GroupBy(enrollments => new { enrollments.TenantId, enrollments.CourseId })
                    .Select(g => new EnrollmentResumeDto
                    {
                        TenantId = g.Key.TenantId,
                        CourseId = g.Key.CourseId,
                        TotalStudents = g.Sum(x => x.TotalStudents),
                        CertificatesEmitted = g.Sum(x => x.CertificatesEmitted),
                        LastEnrollmentDate = g.Max(x => x.LastEnrollmentDate),
                        LastAccess = g.Max(x => x.LastAccess)
                    }).ToList();

                foreach (var enrollment in enrollments)
                {
                    try
                    {
                        var course = await GetCourse(enrollment.CourseId ?? string.Empty);
                        enrollment.CourseName = course.Name;
                        enrollment.CourseCode = course.Code;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error getting enrollment for course {enrollment.CourseId}: {ex.Message}");
                    }
                }

                if (filter.Courses.Any())
                {
                    enrollments = enrollments.Where(e => e.CourseName != null && filter.Courses.Exists(c => e.CourseName.Contains(c))).ToList();
                }

                enrollments = enrollments.OrderByDescending(e => e.LastEnrollmentDate).ToList();

                return enrollments;
            }
            return [];
        }

        public async Task<StudentDto?> GetByIdAsync(string studentId, LoggedUserDto? loggedUser)
        {
            var student = await studentsRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                return null;
            }
            ValidatePermissions(student, loggedUser?.User);
            var result = mapper.Map<StudentDto>(student);
            result.Courses.RemoveAll(c => !c.IsActive);
            result.Courses = await FillCoursesInfo(result.Courses);
            return result;
        }

        public async Task<StudentDto?> GetByLoggedUserAsync(LoggedUserDto? loggedUser)
        {
            var student = await studentsRepository.GetByUserIdAsync(loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser)));
            if (student == null)
            {
                return null;
            }
            var result = mapper.Map<StudentDto>(student);
            result.Courses.RemoveAll(c => !c.IsActive);
            result.Courses = await FillCoursesInfo(result.Courses);
            if (student.UserId == loggedUser.User.Id)
            {
                await activitiesService.RegisterActivity(loggedUser, null, null, StudentEventTypes.ListStudentCourses, "Listar Cursos", "Cursos listados para aluno");
            }
            return result;
        }

        public async Task<StudentDto?> GetByCodeAsync(string studentCode, LoggedUserDto loggedUser)
        {
            var student = await studentsRepository.GetByStudentCodeAsync(studentCode);
            if (student == null)
            {
                return null;
            }
            ValidatePermissions(student, loggedUser?.User);
            var result = mapper.Map<StudentDto>(student);
            result.Courses.RemoveAll(c => !c.IsActive);
            result.Courses = await FillCoursesInfo(result.Courses);
            if (student.UserId == loggedUser.User.Id)
            {
                await activitiesService.RegisterActivity(loggedUser, null, null, StudentEventTypes.ListStudentCourses, "Listar Cursos", "Cursos listados para aluno");
            }
            return result;
        }

        public async Task<PaginatedResult<StudentDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await studentsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                result.Itens.ForEach(c => c.Courses.RemoveAll(c => !c.IsActive));
                return new PaginatedResult<StudentDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<StudentDto>>(result.Itens)
                };
            }
            return new PaginatedResult<StudentDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = []
            };
        }

        public async Task<PaginatedResult<StudentUserDto>> GetStudentUsersByFilterAsync(RequestFilter filter, LoggedUserDto loggedUser)
        {
            var result = await studentsRepository.GetStudentUserByFilter(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                result.Itens.ForEach(c => c.Courses.RemoveAll(c => !c.IsActive));
                var paginatedResult = new PaginatedResult<StudentUserDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<StudentUserDto>>(result.Itens)
                };
                paginatedResult.Itens.ForEach(c => c.Student.Courses.ForEach(sc => ClearCourse(sc)));
                return paginatedResult;
            }
            return new PaginatedResult<StudentUserDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = []
            };
        }

        public async Task<IEnumerable<StudentDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var students = await studentsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<StudentDto>>(students);
        }

        public async Task<StudentDto> SaveAsync(StudentDto studentDto, LoggedUserDto? loggedUser)
        {
            var student = mapper.Map<Student>(studentDto);
            ValidatePermissions(student, loggedUser?.User);
            student.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            student.CreatedAt = DateTime.UtcNow;
            student.CreatedBy = string.IsNullOrEmpty(student.CreatedBy) ? loggedUser.User.Id : student.CreatedBy;
            var response = await studentsRepository.SaveAsync(student);
            return mapper.Map<StudentDto>(response);
        }

        public async Task<StudentDto> CreateAsync(StudentDto studentDto, LoggedUserDto loggedUser)
        {
            var student = mapper.Map<Student>(studentDto);
            ValidatePermissions(student, loggedUser.User);
            student.Id = string.Empty;
            student.TenantId = loggedUser.User.TenantId;
            student.IsActive = true;
            var response = await studentsRepository.SaveAsync(student, loggedUser.User.Id);
            RegisterEventStudent(EventKeys.StudentCreated, "Student created", loggedUser, response);
            return mapper.Map<StudentDto>(response);
        }

        public async Task<StudentDto?> UpdateAsync(StudentDto studentDto, LoggedUserDto loggedUser)
        {
            var student = mapper.Map<Student>(studentDto);
            ValidatePermissions(student, loggedUser.User);
            var currentStudent = await studentsRepository.GetByIdAsync(student.Id);
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await studentsRepository.SaveAsync(student, loggedUser.User.Id);
            RegisterEventStudent(EventKeys.StudentUpdated, "Student updated", loggedUser, response);
            return mapper.Map<StudentDto>(response);
        }

        public async Task<StudentDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentStudent = await studentsRepository.GetByIdAsync(patch.GetId());
            if (currentStudent == null || currentStudent.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentStudent = patch.PatchEntity(currentStudent);
            ValidatePermissions(currentStudent, loggedUser.User);
            if (!mapper.Map<StudentDto?>(currentStudent).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await studentsRepository.SaveAsync(currentStudent, loggedUser.User.Id);
            RegisterEventStudent(EventKeys.StudentUpdated, "Student updated", loggedUser, response);
            return mapper.Map<StudentDto>(response);
        }

        public async Task<StudentCourseResumeDto> GetCourse(LoggedUserDto loggedUser, string courseId)
        {
            var student = await studentsRepository.GetByUserIdAsync(loggedUser?.User?.Id) ?? throw new NotFoundException("Student not found");
            var studentCourse = student.Courses.Find(c => c.Id == courseId && c.IsActive) ?? throw new NotFoundException("Course not found");
            studentCourse.LastAccessedDate = DateTime.UtcNow;
            await studentsRepository.SaveAsync(student, loggedUser.User.Id);
            var fullCourse = await GetCourse(studentCourse.Id);
            if (await UpdateStudentCourse(studentCourse, fullCourse))
            {
                await studentsRepository.SaveAsync(student);
            }
            var result = mapper.Map<StudentCourseDto>(studentCourse);
            result = await FillCourseInfo(result, fullCourse);
            await activitiesService.RegisterActivity(loggedUser, result, null, StudentEventTypes.CourseAccess, "Acessar Curso", $"Curso {result.Name} Acessado");
            return mapper.Map<StudentCourseResumeDto>(result);
        }

        public async Task<StudentCourseResumeDto?> ValidateEnrollment(string courseId, LoggedUserDto loggedUser)
        {
            return await ValidateEnrollment(loggedUser.User.Id, courseId, loggedUser);
        }

        public async Task<StudentCourseResumeDto?> ValidateEnrollment(string userId, string courseId, LoggedUserDto loggedUser)
        {
            var student = await studentsRepository.GetByUserIdAsync(userId) ?? throw new NotFoundException("Student not found");
            var result = student.Courses.Find(c => c.Id == courseId && c.IsActive);
            if (result == null)
            {
                return null;
            }
            if (userId != student.UserId && !loggedUser.User.Permissions.Contains(studentsApiSettings.StudentsAdminPermission ?? string.Empty))
            {
                throw new UnauthorizedAccessException();
            }
            return mapper.Map<StudentCourseResumeDto>(result);
        }

        public async Task<PaginatedResult<StudentCourseResumeDto>> GetCourses(RequestFilter filter, LoggedUserDto loggedUser)
        {
            var student = await studentsRepository.GetByUserIdAsync(loggedUser?.User?.Id) ?? throw new NotFoundException("Student not found");
            student.Courses.RemoveAll(c => !c.IsActive);
            var pageCount = 0;
            var total = 0;
            if (filter.PageLimit > 0)
            {
                total = student.Courses.Count();
                var chunks = student.Courses.Chunk(filter.PageLimit);
                pageCount = chunks.Count();
                if (filter.Page <= 0) filter.Page = 1;
                if (filter.Page > pageCount) filter.Page = pageCount;
                student.Courses = chunks.ElementAt(filter.Page - 1).ToList();
            }

            var result = await FillCoursesInfo(student.Courses.Select(c => mapper.Map<StudentCourseDto>(c)).ToList());
            foreach (var item in result)
            {
                item.Disciplines = [];
                item.Annotations = [];
            }
            var lastCourse = result.MaxBy(c => c.LastAccessedDate);
            if (lastCourse != null) lastCourse.IsLastAccessed = true;
            await activitiesService.RegisterActivity(loggedUser, null, null, StudentEventTypes.ListStudentCourses, "Listar Cursos", "Cursos listados para aluno");
            return PaginatedResult<StudentCourseResumeDto>.Create(mapper.Map<List<StudentCourseResumeDto>>(result), pageCount, filter.Page, total);
        }

        public async Task<StudentLessonsDto> GetLesson(LoggedUserDto loggedUser, string courseId, string lessonId)
        {
            var student = await studentsRepository.GetByUserIdAsync(loggedUser?.User?.Id) ?? throw new NotFoundException("Student not found");
            var course = student.Courses.Find(c => c.Id == courseId && c.IsActive) ?? throw new NotFoundException("Course not found");
            var lesson = course.Disciplines.SelectMany(d => d.Lessons).FirstOrDefault(l => l.Id == lessonId) ?? throw new NotFoundException("Lesson not found");
            lesson.LastAccessDate = DateTime.UtcNow;
            await studentsRepository.SaveAsync(student, loggedUser.User.Id);
            var result = mapper.Map<StudentLessonsDto>(lesson);
            await activitiesService.RegisterActivity(loggedUser, mapper.Map<StudentCourseDto>(course), result, StudentEventTypes.LessonAccess, "Acessar Aula", $"Aula {result.Name} acessada do curso {course.Id} Acessado");
            result = await FillLessonInfo(result, loggedUser);
            return result;
        }

        public async Task<StudentProgressResponseDto> SetProgress(LoggedUserDto loggedUser, StudentLessonProgressDto lessonProgress, string hostUrl)
        {
            var result = new StudentProgressResponseDto();
            var student = await studentsRepository.GetByUserIdAsync(loggedUser?.User?.Id) ?? throw new NotFoundException("Student not found");
            var course = student.Courses.Find(c => c.Id == lessonProgress.CourseId && c.IsActive) ?? throw new NotFoundException("Course not found");
            var lesson = course.Disciplines.SelectMany(d => d.Lessons).FirstOrDefault(l => l.Id == lessonProgress.LessonId) ?? throw new NotFoundException("Lesson not found");
            var fullCourse = await GetCourse(course.Id);
            var enrichedCourseDto = await CreateEnrichedStudentCourseDto(course, fullCourse);
            lesson.Progress = lessonProgress.Progress;
            switch (lessonProgress.State)
            {
                case StudentLessonState.InProgress:
                    if (lesson.State == StudentLessonState.Pending)
                    {
                        lesson.StartTime = DateTime.UtcNow;
                    }
                    await activitiesService.RegisterActivity(loggedUser, enrichedCourseDto, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.LessonProgress, "Progresso de aula", $"Progresso da aula {lesson.Id} definida do curso {course.Id}: {lessonProgress.Progress}");
                    break;

                case StudentLessonState.Finished:
                    lesson.EndTime = DateTime.UtcNow;
                    lesson.Progress = 100;
                    await activitiesService.RegisterActivity(loggedUser, enrichedCourseDto, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.LessonEnd, "Aula Finalizada", $"Aula finalizada {lesson.Id} definida do curso {course.Id}");
                    break;

                default:
                    await activitiesService.RegisterActivity(loggedUser, enrichedCourseDto, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.LessonProgress, "Progresso de aula pendente", $"Progresso da aula, para pendente, {lesson.Id} definida do curso {course.Id}: {lessonProgress.Progress}");
                    break;
            }

            lesson.State = lessonProgress.State;

            if (lesson.Type == LessonTypes.Exam && lessonProgress.ExamSubmit != null)
            {
                result = await ProcessExamSubmit(lesson, lessonProgress.ExamSubmit, mapper.Map<StudentCourseDto>(course), loggedUser);
                lesson.Exam.StudentScore = result.StudentExamResult.Score;
                lesson.Exam.State = result.StudentExamResult.State;
                lesson.State = result.StudentExamResult.State == StudentExamState.Approved ? StudentLessonState.Finished : StudentLessonState.Pending;
                lesson.Progress = result.StudentExamResult.State == StudentExamState.Approved ? 100 : 0;
                lesson.Exam.SubmitDate = result.StudentExamResult.SubmitDate;
                lesson.Exam.CurrentTry = result.StudentExamResult.Try;
                lesson.Exam.Questions = mapper.Map<List<StudentExamQuestion>>(result.StudentExamResult.Responses);
            }
            else if (lesson.Type == LessonTypes.Survey && lessonProgress.ExamSubmit != null)
            {
                result = await ProcessSurveySubmit(lesson, lessonProgress.ExamSubmit, enrichedCourseDto, loggedUser);
                lesson.Exam.StudentScore = result.StudentExamResult.Score;
                lesson.Exam.State = StudentExamState.Approved;
                lesson.Progress = 100;
                lesson.State = StudentLessonState.Finished;
                lesson.Exam.SubmitDate = result.StudentExamResult.SubmitDate;
                lesson.Exam.CurrentTry = result.StudentExamResult.Try;
                lesson.Exam.Questions = mapper.Map<List<StudentExamQuestion>>(result.StudentExamResult.Responses);
            }
            else
            {
                result = new StudentProgressResponseDto
                {
                    CourseId = course.Id,
                    DisciplineId = lesson.DisciplineId,
                    LessonId = lesson.Id,
                    CourseState = course.State,
                    LessonProgress = lesson.Progress,
                    LessonState = lesson.State
                };
            }

            if (lesson.State == StudentLessonState.Finished)
            {
                course.State = course.Disciplines.All(d => d.Lessons.All(l => l.State == StudentLessonState.Finished)) ? StudentCourseState.Finished : StudentCourseState.InProgress;
                if (course.State == StudentCourseState.Finished)
                {
                    course.EndTime = DateTime.UtcNow;
                    course.CertificateId = await ProcessCertificateEmission(course, student, loggedUser.User, hostUrl);
                    await activitiesService.RegisterActivity(loggedUser, enrichedCourseDto, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.CourseEnd, "Curso Finalizado", $"Curso Finalizado {course.Id}");
                }
            }
            else if (course.State == StudentCourseState.Finished)
            {
                course.State = StudentCourseState.InProgress;
            }
            await studentsRepository.SaveAsync(student, loggedUser.User.Id);

            return result;
        }

        public async Task<StudentDto?> InternalEnroll(EnrollmentMessage enrollment)
        {
            var currentStudent = await studentsRepository.GetByUserIdAsync(enrollment.UserId);
            if (currentStudent == null)
            {
                currentStudent = new Student
                {
                    UserId = enrollment.UserId,
                    Code = CodeHelper.GenerateNumericCode(studentsApiSettings.StudentCodeSize),
                    TenantId = enrollment.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = enrollment.UserId,
                    Courses = [],
                    IsActive = true
                };
            }
            var product = await GetProduct(enrollment.ProductId);
            var course = await GetCourse(product.ItemId);
            if (currentStudent.Courses.Exists(c => c.Id == course.Id && c.IsActive))
            {
                throw new DuplicatedException($"Student {currentStudent.UserId} already enrolled in course {course.Id}");
            }
            else if (currentStudent.Courses.Exists(c => c.Id == course.Id && !c.IsActive))
            {
                currentStudent.Courses.Find(c => c.Id == course.Id).IsActive = true;
                currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
                return GetStudentResume(currentStudent);
            }
            var user = await GetUser(currentStudent.UserId);
            currentStudent = ProcessEnrollment(enrollment, currentStudent, product, course);
            currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
            RegisterEventEnrollment(EventKeys.EnrollmentCreated, "Enrollment created", user, currentStudent, course, product);
            await activitiesService.RegisterActivity(mapper.Map<StudentDto>(currentStudent), mapper.Map<StudentCourseDto>(course), null, StudentEventTypes.Enroll, "Matrícula Realizada", $"Matrícula Realizada {course.Id}");
            return GetStudentResume(currentStudent);
        }

        public async Task<StudentDto?> Enroll(EnrollmentMessage enrollment, LoggedUserDto loggedUser)
        {
            var currentStudent = await studentsRepository.GetByUserIdAsync(enrollment.UserId);
            if (currentStudent == null)
            {
                currentStudent = new Student
                {
                    UserId = enrollment.UserId,
                    Code = CodeHelper.GenerateNumericCode(studentsApiSettings.StudentCodeSize),
                    TenantId = enrollment.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = enrollment.UserId,
                    Courses = [],
                    IsActive = true
                };
            }
            ValidatePermissions(currentStudent, loggedUser.User);
            var product = await GetProduct(enrollment.ProductId);
            var course = await GetCourse(product.ItemId);
            if (currentStudent.Courses.Exists(c => c.Id == course.Id && c.IsActive))
            {
                throw new DuplicatedException($"Student {currentStudent.UserId} already enrolled in course {course.Id}");
            }
            else if (currentStudent.Courses.Exists(c => c.Id == course.Id && !c.IsActive))
            {
                currentStudent.Courses.Find(c => c.Id == course.Id).IsActive = true;
                currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
                return mapper.Map<StudentDto?>(currentStudent);
            }
            var user = await GetUser(currentStudent.UserId);
            currentStudent = ProcessEnrollment(enrollment, currentStudent, product, course);
            currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
            RegisterEventEnrollment(EventKeys.EnrollmentCreated, "Enrollment created", user, currentStudent, course, product);
            await activitiesService.RegisterActivity(mapper.Map<StudentDto>(currentStudent), mapper.Map<StudentCourseDto>(currentStudent.Courses.Find(c => c.Id == course.Id)), null, StudentEventTypes.Enroll, "Matrícula Realizada", $"Matrícula Realizada {course.Id}");
            return GetStudentResume(currentStudent);
        }

        public async Task<StudentDto?> FreeEnroll(EnrollmentMessage enrollment, LoggedUserDto loggedUser)
        {
            var currentStudent = await studentsRepository.GetByUserIdAsync(enrollment.UserId);
            if (currentStudent == null)
            {
                currentStudent = new Student
                {
                    UserId = enrollment.UserId,
                    Code = CodeHelper.GenerateNumericCode(studentsApiSettings.StudentCodeSize),
                    TenantId = enrollment.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = enrollment.UserId,
                    Courses = [],
                    IsActive = true
                };
            }
            ValidatePermissions(currentStudent, loggedUser.User);
            var product = await GetProduct(enrollment.ProductId);
            if (product != null && product.FullPrice != 0)
            {
                throw new InvalidEnumArgumentException($"Product {product.Id} is not free");
            }
            var course = await GetCourse(product.ItemId);
            if (currentStudent.Courses.Exists(c => c.Id == course.Id))
            {
                throw new DuplicatedException($"Student {currentStudent.UserId} already enrolled in course {course.Id}");
            }
            else if (currentStudent.Courses.Exists(c => c.Id == course.Id && !c.IsActive))
            {
                currentStudent.Courses.Find(c => c.Id == course.Id).IsActive = true;
                currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
                return GetStudentResume(currentStudent);
            }
            var user = await GetUser(currentStudent.UserId);
            currentStudent = ProcessEnrollment(enrollment, currentStudent, product, course);
            currentStudent = await studentsRepository.SaveAsync(currentStudent, enrollment.UserId);
            RegisterEventEnrollment(EventKeys.EnrollmentCreated, "Enrollment created", user, currentStudent, course, product);
            var estudentCourse = currentStudent.Courses.Find(c => c.Id == course.Id);
            await activitiesService.RegisterActivity(mapper.Map<StudentDto>(currentStudent), mapper.Map<StudentCourseDto>(estudentCourse), null, StudentEventTypes.Enroll, "Matrícula Gratuita Realizada", $"Matrícula Gratuita Realizada {course.Id}");
            return GetStudentResume(currentStudent);
        }

        public async Task<StudentDto?> UnEnrollment(string studentId, string courseId, LoggedUserDto loggedUser)
        {
            var currentStudent = await studentsRepository.GetByUserIdAsync(studentId);
            if (currentStudent == null)
            {
                throw new NotFoundException($"Student {studentId} not found");
            }
            await activitiesService.RegisterActivity(mapper.Map<StudentDto>(currentStudent), mapper.Map<StudentCourseDto>(currentStudent.Courses.Find(c => c.Id == courseId)), null, StudentEventTypes.UnEnroll, "Matrícula Cancelada", $"Matrícula Cancelada {courseId} Por {loggedUser.User.MainEmail} - {loggedUser.User.Id}");
            currentStudent.Courses.Where(c => c.Id == courseId).ToList().ForEach(course => course.IsActive = false);
            var result = await studentsRepository.SaveAsync(currentStudent);
            if (result != null)
            {
                result.Courses.RemoveAll(result => !result.IsActive);
            }
            return mapper.Map<StudentDto?>(result);
        }

        public async Task<StudentDto?> DeleteEnrollment(string studentId, string courseId)
        {
            var currentStudent = await studentsRepository.GetByIdAsync(studentId) ?? throw new NotFoundException($"Student {studentId} not found");
            var currentCourse = currentStudent.Courses.Find(c => c.Id == courseId) ?? throw new NotFoundException($"Course {courseId} not found for student {studentId}");
            var course = await GetCourse(currentCourse.Id);
            var product = await GetProduct(currentCourse.ProductId);
            var user = await GetUser(currentStudent.UserId);
            currentStudent.Courses.Remove(currentCourse);
            await studentsRepository.SaveAsync(currentStudent);
            RegisterEventEnrollment(EventKeys.EnrollmentDeleted, "Enrollment deleted", user, currentStudent, course, product);
            return mapper.Map<StudentDto?>(currentStudent);
        }

        public async Task<List<string>> GetEnrollments(string userId)
        {
            var result = new List<string>();
            var currentStudent = await studentsRepository.GetByUserIdAsync(userId);
            if (currentStudent == null || currentStudent.Courses.Count == 0)
            {
                return result;
            }
            result = currentStudent.Courses.Where(c => c.IsActive).Select(c => c.Id).ToList();
            return result;
        }

        public async Task DeleteEnrollments(string userId)
        {
            var currentStudent = await studentsRepository.GetByUserIdAsync(userId);
            if (currentStudent == null || currentStudent.Courses.Count == 0)
            {
                return;
            }
            currentStudent.Courses.ForEach(c =>
            {
                c.Id = $"{c.Id}-{CodeHelper.GenerateNumericCode(10)}";
                c.IsActive = false;
                c.Disciplines = [];
                c.Annotations = [];
            });
            var user = await GetUser(currentStudent.UserId);
            RegisterEventAllEnrollmentsDelete(user, currentStudent);
            await studentsRepository.SaveAsync(currentStudent);
        }

        public Task EmmitCertificateAsync(string courseId, string hostUrl, LoggedUserDto loggedUser)
        {
            return EmmitCertificateAsync(loggedUser?.User?.Id ?? string.Empty, courseId, loggedUser, hostUrl, false);
        }

        public async Task EmmitCertificateAsync(string userId, string courseId, LoggedUserDto loggedUser, string hostUrl, bool reemission = true)
        {
            var student = await studentsRepository.GetByUserIdAsync(userId) ?? throw new NotFoundException("Student not found");
            var user = await GetUser(student.UserId) ?? throw new NotFoundException("User not found");
            var course = student.Courses.Find(c => c.Id == courseId && c.IsActive) ?? throw new NotFoundException("Course not found");
            if (!string.IsNullOrEmpty(course.CertificateId) && !reemission)
            {
                throw new InvalidOperationException($"Certificate already emmited for course {courseId}.");
            }

            if (student.UserId != loggedUser?.User?.Id && !loggedUser!.User!.Permissions.Contains(studentsApiSettings!.StudentsAdminPermission!))
            {
                throw new UnauthorizedAccessException("User does not have permission to emit certificate for this course.");
            }
            if (course.State != StudentCourseState.Finished && !reemission)
            {
                throw new InvalidOperationException($"Course {courseId} is not finished");
            }
            course.CertificateId = await ProcessCertificateEmission(course, student, user, hostUrl);
            await studentsRepository.SaveAsync(student);
        }

        private StudentCourseDto ClearCourse(StudentCourseDto course)
        {
            if (course.Disciplines.Any())
            {
                var totalLessons = course.Disciplines?.SelectMany(d => d.Lessons).Count() ?? 0;
                course.Progress = totalLessons > 0 ? course.Disciplines!.SelectMany(d => d.Lessons).Sum(l => l.Progress) / totalLessons : 0;
            }
            course.Disciplines = [];
            return course;
        }

        private Stream EnrollmentsToCsvStream(List<AnalyticEnrollment> Enrollments)
        {
            var result = $"Data Matricula;Telefone;CPF;Email;Nome Aluno;Genero;Ultimo Acesso;Situcao;Curso;Descricao;Data Finalizacao;Data da Emissao do Certificado;Progresso\n";
            foreach (var enrollment in Enrollments)
            {
                var totalLessons = enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Count() ?? 0;
                var progress = totalLessons > 0 ? enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Sum(l => l.Progress) / totalLessons : 0;
                var certificateDate = enrollment.CertificateDate == DateTime.MinValue ? "" : enrollment.CertificateDate.ToString("dd/MM/yyyy HH:mm:ss");
                var endTime = enrollment.EndTime == DateTime.MinValue ? "" : enrollment.EndTime.ToString("dd/MM/yyyy HH:mm:ss");
                var lastAccessDate = enrollment.LastAccessedDate == DateTime.MinValue ? "" : enrollment.LastAccessedDate.ToString("dd/MM/yyyy HH:mm:ss");
                var gender = enrollment.Gender switch
                {
                    "MALE" => "Masculino",
                    "FEMALE" => "Feminino",
                    "OPTOUT" => "Não Informado",
                    "OTHER" => "Outro",
                    _ => "Não Informado"
                };
                result += $"\n{enrollment.EnrollmentDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")};{enrollment.PhoneNumber};{enrollment.CPF};{enrollment.Email};{enrollment.Name};{gender};{lastAccessDate};{enrollment.State};{enrollment.ProductName};{enrollment.ProductDescription};{endTime};{certificateDate};{progress}";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream EnrollmentsToXlsxStream(List<AnalyticEnrollment> Enrollments)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Enrollments");

            worksheet.Cell("A1").Value = "Data Matricula";
            worksheet.Cell("B1").Value = "Telefone";
            worksheet.Cell("C1").Value = "CPF";
            worksheet.Cell("D1").Value = "Email";
            worksheet.Cell("E1").Value = "Nome Aluno";
            worksheet.Cell("F1").Value = "Genero";
            worksheet.Cell("G1").Value = "Ultimo Acesso";
            worksheet.Cell("H1").Value = "Situcao";
            worksheet.Cell("I1").Value = "Cod. Curso";
            worksheet.Cell("J1").Value = "Curso";
            worksheet.Cell("K1").Value = "Descricao";
            worksheet.Cell("L1").Value = "Data Finalizacao";
            worksheet.Cell("M1").Value = "Data da Emissao do Certificado";
            worksheet.Cell("N1").Value = "Progresso";

            var row = 2;
            foreach (var enrollment in Enrollments)
            {
                var totalLessons = enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Count() ?? 0;
                var progress = totalLessons > 0 ? enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Sum(l => l.Progress) / totalLessons : 0;
                var certificateDate = enrollment.CertificateDate == DateTime.MinValue ? "" : enrollment.CertificateDate.ToString("dd/MM/yyyy HH:mm:ss");
                var endTime = enrollment.EndTime == DateTime.MinValue ? "" : enrollment.EndTime.ToString("dd/MM/yyyy HH:mm:ss");
                var lastAccessDate = enrollment.LastAccessedDate == DateTime.MinValue ? "" : enrollment.LastAccessedDate.ToString("dd/MM/yyyy HH:mm:ss");
                var gender = enrollment.Gender switch
                {
                    "MALE" => "Masculino",
                    "FEMALE" => "Feminino",
                    "OPTOUT" => "Não Informado",
                    "OTHER" => "Outro",
                    _ => "Não Informado"
                };

                worksheet.Cell($"A{row}").Value = enrollment.EnrollmentDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell($"B{row}").Value = enrollment.PhoneNumber;
                worksheet.Cell($"C{row}").Value = enrollment.CPF;
                worksheet.Cell($"D{row}").Value = enrollment.Email;
                worksheet.Cell($"E{row}").Value = enrollment.Name;
                worksheet.Cell($"F{row}").Value = gender;
                worksheet.Cell($"G{row}").Value = lastAccessDate;
                worksheet.Cell($"H{row}").Value = enrollment.State;
                worksheet.Cell($"I{row}").Value = enrollment.ProductCode;
                worksheet.Cell($"J{row}").Value = enrollment.ProductName;
                worksheet.Cell($"K{row}").Value = enrollment.ProductDescription;
                worksheet.Cell($"L{row}").Value = endTime;
                worksheet.Cell($"M{row}").Value = certificateDate;
                worksheet.Cell($"N{row}").Value = progress;
                row += 1;
            }
            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Stream SurveyToXlsxStream(List<AnalyticSurvey> surveys)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Enrollments");

            surveys = surveys.DistinctBy(s => new { s.CPF, s.StudentExam.SubmitDate, s.ProductName }).ToList();

            worksheet.Cell("A1").Value = "Data Submissao";
            worksheet.Cell("B1").Value = "Telefone";
            worksheet.Cell("C1").Value = "CPF";
            worksheet.Cell("D1").Value = "Email";
            worksheet.Cell("E1").Value = "Nome Aluno";
            worksheet.Cell("F1").Value = "Genero";
            worksheet.Cell("G1").Value = "Curso";
            worksheet.Cell("H1").Value = "Descricao";

            var row = 2;
            foreach (var survey in surveys)
            {
                var gender = survey.Gender switch
                {
                    "MALE" => "Masculino",
                    "FEMALE" => "Feminino",
                    "OPTOUT" => "Não Informado",
                    "OTHER" => "Outro",
                    _ => "Não Informado"
                };

                worksheet.Cell($"A{row}").Value = survey.StudentExam.SubmitDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cell($"B{row}").Value = survey.PhoneNumber;
                worksheet.Cell($"C{row}").Value = survey.CPF;
                worksheet.Cell($"D{row}").Value = survey.Email;
                worksheet.Cell($"E{row}").Value = survey.Name;
                worksheet.Cell($"F{row}").Value = gender;
                worksheet.Cell($"G{row}").Value = survey.ProductName;
                worksheet.Cell($"H{row}").Value = survey.ProductDescription;

                var currentColum = "I";
                foreach (var answer in survey.StudentExam.Questions)
                {
                    var question = survey.Exam.Questions.FirstOrDefault(q => q.Id == answer.Id);
                    if (question == null) continue;
                    worksheet.Cell($"{currentColum}{row}").Value = question.Title;
                    currentColum = NextColumn(currentColum);
                    if (question.Type == QuestionTypes.SingleChoice || question.Type == QuestionTypes.MultipleChoice || answer.ResponseOptions.Any())
                    {
                        var value = string.Join(", ", question.Options.Where(o => answer.ResponseOptions.Any(r => r == o.Id)).Select(a => a.Body));
                        if (string.IsNullOrEmpty(value))
                        {
                            value = string.Join(",", answer.ResponseOptions);
                        }
                        worksheet.Cell($"{currentColum}{row}").Value = value;
                    }
                    else
                    {
                        worksheet.Cell($"{currentColum}{row}").Value = answer.ResponseText;
                    }
                    currentColum = NextColumn(currentColum);
                }

                row += 1;
            }
            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private string NextColumn(string currentColumn)
        {
            if (!currentColumn.EndsWith("Z"))
            {
                return currentColumn.Substring(0, currentColumn.Length - 1) + (char)(currentColumn[currentColumn.Length - 1] + 1);
            }
            else
            {
                return "AA";
            }
        }

        private (List<StudentCourseRelation>, Stream) EnrollmentsToPendingCertificate(List<AnalyticEnrollment> Enrollments)
        {
            var resultFile = $"UserId;CourseId;ProductId;Data Matricula;Telefone;CPF;Email;Nome Aluno;Genero;Ultimo Acesso;Situcao;Curso;Descricao;Data Finalizacao;Data da Emissao do Certificado;Progresso\n";
            var result = new List<StudentCourseRelation>();
            foreach (var enrollment in Enrollments)
            {
                var relation = new StudentCourseRelation();
                relation.CourseId = enrollment.Course.Id;
                relation.ProductId = enrollment.Course.ProductId;
                relation.UserId = enrollment.UserId;
                var totalLessons = enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Count() ?? 0;
                var progress = totalLessons > 0 ? enrollment.Course.Disciplines?.SelectMany(d => d.Lessons).Sum(l => l.Progress) / totalLessons : 0;
                relation.Progress = progress;
                if (relation.Progress >= 100 && enrollment.CertificateDate <= DateTime.MinValue)
                {
                    result.Add(relation);
                    var certificateDate = enrollment.CertificateDate == DateTime.MinValue ? "" : enrollment.CertificateDate.ToString("dd/MM/yyyy HH:mm:ss");
                    var endTime = enrollment.EndTime == DateTime.MinValue ? "" : enrollment.EndTime.ToString("dd/MM/yyyy HH:mm:ss");
                    var lastAccessDate = enrollment.LastAccessedDate == DateTime.MinValue ? "" : enrollment.LastAccessedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    var gender = enrollment.Gender switch
                    {
                        "MALE" => "Masculino",
                        "FEMALE" => "Feminino",
                        "OPTOUT" => "Não Informado",
                        "OTHER" => "Outro",
                        _ => "Não Informado"
                    };
                    resultFile += $"\n{enrollment.UserId};{enrollment.Course.Id};{enrollment.Course.ProductId};{enrollment.EnrollmentDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")};{enrollment.PhoneNumber};{enrollment.CPF};{enrollment.Email};{enrollment.Name};{gender};{lastAccessDate};{enrollment.State};{enrollment.ProductName};{enrollment.ProductDescription};{endTime};{certificateDate};{progress}";
                }
            }
            return (result, new MemoryStream(Encoding.UTF8.GetBytes(resultFile)));
        }

        private async Task<StudentCourseDto> CreateEnrichedStudentCourseDto(StudentCourse studentCourse, CourseDto fullCourse)
        {
            var courseDto = mapper.Map<StudentCourseDto>(studentCourse);
            courseDto.Name = fullCourse.Name;
            courseDto.Description = fullCourse.Description;
            courseDto.Body = fullCourse.Body;
            courseDto.ImageUrl = fullCourse.ImageUrl;
            courseDto.Thumbnail = fullCourse.Thumbnail;
            courseDto.Category = fullCourse.Categories?.FirstOrDefault();
            courseDto.Tags = fullCourse.Tags;
            return courseDto;
        }

        private StudentDto? GetStudentResume(Student currentStudent)
        {
            currentStudent.Courses.RemoveAll(c => !c.IsActive);
            var result = mapper.Map<StudentDto?>(currentStudent);
            result?.Courses.ForEach(c =>
            {
                c.Disciplines = [];
            });
            return result;
        }

        private async Task<string> ProcessCertificateEmission(StudentCourse studentCourse, Student student, UserDto user, string hostUrl)
        {
            var course = await GetCourse(studentCourse.Id);
            if (course == null)
            {
                throw new NotFoundException($"Course {studentCourse.Id} not found");
            }
            if (string.IsNullOrEmpty(course.CertificateId))
            {
                throw new NotFoundException($"Course missing certificate id: {studentCourse.Id}");
            }
            if (!course.HasCertificate && !string.IsNullOrEmpty(course.CertificateId))
            {
                return string.Empty;
            }
            var certificateRequest = new CertificateEmissionRequestDto
            {
                TenantId = user.TenantId,
                CertificateID = course.CertificateId ?? string.Empty,
                User = user,
                Student = mapper.Map<StudentDto>(student),
                Course = mapper.Map<StudentCourseDto>(studentCourse)
            };
            var response = await certificatesService.EmmitCertificate(certificateRequest, hostUrl);
            if (string.IsNullOrEmpty(response))
            {
                logger.Error($"Error emitting certificate: {certificateRequest.Course.EnrollmentCode}");
                return string.Empty;
            }
            await activitiesService.RegisterActivity(mapper.Map<StudentDto>(student), mapper.Map<StudentCourseDto>(studentCourse), null, StudentEventTypes.CertificateEmission, "Certificado Emitido", $"Certificado Emitido {course.Id}");
            var result = response;
            return result.Replace("\"", "");
        }

        private async Task<StudentProgressResponseDto> ProcessExamSubmit(StudentLesson lesson, StudentExamSubmitDto examSubmit, StudentCourseDto studentCourse, LoggedUserDto loggedUser)
        {
            var course = await GetCourse(lesson.CourseId);
            if (course == null)
            {
                throw new NotFoundException($"Course {lesson.CourseId} not found");
            }
            var discipline = course.Disciplines.Find(d => d.Id == lesson.DisciplineId);
            if (discipline == null)
            {
                throw new NotFoundException($"Discipline {lesson.DisciplineId} not found");
            }
            var courseLesson = discipline.Lessons.Find(l => l.Id == lesson.Id);
            if (courseLesson == null)
            {
                throw new NotFoundException($"Lesson {lesson.Id} not found");
            }
            if (courseLesson.Exam == null || lesson.Exam == null)
            {
                throw new NotFoundException($"Exam {examSubmit.ExamId} not found");
            }
            if (lesson.Exam != null && lesson.Exam.ExamVersionNumber != courseLesson.Exam.VersionNumber)
            {
                var examId = string.IsNullOrEmpty(lesson.Exam.ExamId) ? lesson.Exam.Id : lesson.Exam.ExamId;
                courseLesson.Exam = await GetExamVersion(examId, lesson.Exam.ExamVersionNumber);
            }
            lesson.Exam.CurrentTry += 1;
            if (lesson.Exam.CurrentTry > courseLesson.Exam.MaxRetries)
            {
                throw new InvalidOperationException($"Max retries exceeded for exam {examSubmit.ExamId}");
            }
            if (!lesson.Exam.Questions.TrueForAll(q => examSubmit.Questions.Exists(r => r.Id == q.Id)))
            {
                throw new InvalidOperationException($"All questions are mandatory {examSubmit.ExamId}");
            }

            var result = new StudentProgressResponseDto
            {
                CourseId = course.Id,
                DisciplineId = lesson.DisciplineId,
                LessonId = lesson.Id,
                LessonProgress = lesson.Progress,
                LessonState = lesson.State,
                StudentExamResult = new StudentExamResultDto()
                {
                    SubmitDate = DateTime.UtcNow,
                    IsCompleted = true,
                    Try = lesson.Exam.CurrentTry,
                    RemainingRetries = courseLesson.Exam.MaxRetries - lesson.Exam.CurrentTry,
                }
            };
            foreach (var response in examSubmit.Questions)
            {
                var question = courseLesson.Exam.Questions.Find(q => q.Id == response.Id) ?? throw new NotFoundException($"Question {response.Id} not found");
                result.StudentExamResult.Responses.Add(new StudentExamQuestionDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    AuxText = question.AuxText,
                    ResponseText = response.ResponseText,
                    ResponseOptions = response.ResponseOptions,
                    QuestionsOptions = question.Options.Select(o => new StudentExamQuestionOptionDto
                    {
                        Id = o.Id,
                        Order = o.Order,
                        Letter = o.Letter,
                        Body = o.Body
                    }).ToList()
                });

                if (question.Type == QuestionTypes.SingleChoice)
                {
                    var correctAnswer = question.Options.Find(o => o.IsCorrect);
                    if (correctAnswer != null && response.ResponseOptions.FirstOrDefault() == correctAnswer.Id)
                    {
                        result.StudentExamResult.Responses.Last().Score = question.Value;
                        result.StudentExamResult.Responses.Last().Correct = true;
                    }
                    else
                    {
                        result.StudentExamResult.Responses.Last().Score = 0;
                        result.StudentExamResult.Responses.Last().Correct = false;
                    }
                }
                else if (question.Type == QuestionTypes.MultipleChoice)
                {
                    var correctAnswers = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();

                    if (correctAnswers.TrueForAll(ca => response.ResponseOptions.Exists(o => o == ca)))
                    {
                        result.StudentExamResult.Responses.Last().Score = question.Value;
                        result.StudentExamResult.Responses.Last().Correct = true;
                    }
                    else
                    {
                        result.StudentExamResult.Responses.Last().Score = 0;
                        result.StudentExamResult.Responses.Last().Correct = false;
                    }
                }
            }
            result.StudentExamResult.Score = result.StudentExamResult.Responses.Sum(r => r.Score);
            if (result.StudentExamResult.Responses.All(r => r.Correct ?? false))
            {
                result.StudentExamResult.Score = courseLesson.Exam.TotalScore;
            }
            if (result.StudentExamResult.Score >= courseLesson.Exam.RequiredScore)
            {
                result.StudentExamResult.State = StudentExamState.Approved;
                await activitiesService.RegisterActivity(loggedUser, studentCourse, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.LessonPass, "Prova: Aprovado", $"Prova: Aprovado {lesson.Id} do curso {course.Id}, Resultado: {result.StudentExamResult.Score}");
            }
            else
            {
                result.StudentExamResult.State = StudentExamState.Reproved;
                await activitiesService.RegisterActivity(loggedUser, studentCourse, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.LessonFail, "Prova: Reprovado", $"Prova: Reprovado {lesson.Id} do curso {course.Id}, Resultado: {result.StudentExamResult.Score}");
            }
            return result;
        }

        private async Task<StudentProgressResponseDto> ProcessSurveySubmit(StudentLesson lesson, StudentExamSubmitDto examSubmit, StudentCourseDto studentCourse, LoggedUserDto loggedUser)
        {
            var course = await GetCourse(lesson.CourseId);
            if (course == null)
            {
                throw new NotFoundException($"Course {lesson.CourseId} not found");
            }
            var discipline = course.Disciplines.Find(d => d.Id == lesson.DisciplineId);
            if (discipline == null)
            {
                throw new NotFoundException($"Discipline {lesson.DisciplineId} not found");
            }
            var courseLesson = discipline.Lessons.Find(l => l.Id == lesson.Id);
            if (courseLesson == null)
            {
                throw new NotFoundException($"Lesson {lesson.Id} not found");
            }
            if (courseLesson.Exam == null || lesson.Exam == null)
            {
                throw new NotFoundException($"Survey {examSubmit.ExamId} not found");
            }
            if (lesson.Exam != null && lesson.Exam.ExamVersionNumber != courseLesson.Exam.VersionNumber)
            {
                var examId = string.IsNullOrEmpty(lesson.Exam.ExamId) ? lesson.Exam.Id : lesson.Exam.ExamId;
                courseLesson.Exam = await GetExamVersion(examId, lesson.Exam.ExamVersionNumber);
            }
            lesson.Exam.CurrentTry += 1;
            if (lesson.Exam.CurrentTry > courseLesson.Exam.MaxRetries)
            {
                throw new InvalidOperationException($"Max retries exceeded for Survey {examSubmit.ExamId}");
            }
            if (!courseLesson.Exam.Questions.Where(q => !q.Optional).ToList().TrueForAll(q => examSubmit.Questions.Exists(r => r.Id == q.Id)))
            {
                throw new InvalidOperationException($"Nandatory questions missing {examSubmit.ExamId}");
            }

            var result = new StudentProgressResponseDto
            {
                CourseId = course.Id,
                DisciplineId = lesson.DisciplineId,
                LessonId = lesson.Id,
                LessonProgress = lesson.Progress,
                LessonState = lesson.State,
                StudentExamResult = new StudentExamResultDto()
                {
                    SubmitDate = DateTime.UtcNow,
                    IsCompleted = true,
                    Try = lesson.Exam.CurrentTry,
                    RemainingRetries = courseLesson.Exam.MaxRetries - lesson.Exam.CurrentTry,
                    State = StudentExamState.Approved,
                }
            };
            foreach (var response in examSubmit.Questions)
            {
                var question = courseLesson.Exam.Questions.Find(q => q.Id == response.Id) ?? throw new NotFoundException($"Question {response.Id} not found");
                result.StudentExamResult.Responses.Add(new StudentExamQuestionDto
                {
                    Id = response.Id,
                    Correct = true,
                    Score = 0,
                    ResponseText = response.ResponseText,
                    ResponseOptions = response.ResponseOptions,
                    QuestionsOptions = response.ResponseOptions.Select(o => new StudentExamQuestionOptionDto
                    {
                        Id = o,
                        Letter = question.Options.Find(r => r.Id == o)?.Letter ?? string.Empty,
                        Body = question.Options.Find(r => r.Id == o)?.Body ?? string.Empty
                    }).ToList()
                });
            }
            await activitiesService.RegisterActivity(loggedUser, studentCourse, mapper.Map<StudentLessonsDto>(lesson), StudentEventTypes.SurveyResponse, "Avaliação respondida", $"Avaliação respondida {lesson.Id} do curso {course.Id}");
            return result;
        }

        private Student? ProcessEnrollment(EnrollmentMessage enrollment, Student? currentStudent, ProductDto product, CourseDto course)
        {
            var newCourse = new StudentCourse
            {
                Id = course.Id,
                VId = course.VId,
                VersionNumber = course.VersionNumber,
                OrderId = enrollment.OrderId,
                ProductId = product.Id,
                State = StudentCourseState.New,
                StartTime = DateTime.UtcNow,
                EnrollmentCode = CodeHelper.GenerateNumericCode(studentsApiSettings.StudentCodeSize),
                IsActive = true,
                Disciplines = []
            };

            foreach (var discipline in course.Disciplines.OrderBy(d => d.Order))
            {
                newCourse.Disciplines.Add(new StudentDiscipline
                {
                    Id = discipline.Id,
                    VId = discipline.VId,
                    VersionNumber = discipline.VersionNumber,
                    Lessons = []
                });
                foreach (var lesson in discipline.Lessons.OrderBy(l => l.Order))
                {
                    newCourse.Disciplines.Last().Lessons.Add(new StudentLesson
                    {
                        CourseId = course.Id,
                        Id = lesson.Id,
                        VId = lesson.VId,
                        VersionNumber = lesson.VersionNumber,
                        DisciplineId = discipline.Id,
                        Type = lesson.Type ?? LessonTypes.Lesson,
                        State = StudentLessonState.Pending,
                        Annotations = [],
                        Progress = 0,
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow,
                        LastAccessDate = DateTime.MinValue,
                        Exam = (lesson.Type == LessonTypes.Exam || lesson.Type == LessonTypes.Survey) && lesson.Exam != null ? new StudentExam
                        {
                            Id = lesson.Exam.VId,
                            VId = lesson.Exam.VId,
                            ExamVId = lesson.Exam.VId,
                            ExamVersionNumber = lesson.Exam.VersionNumber,
                            VersionNumber = lesson.Exam.VersionNumber,
                            Version = lesson.Exam.Version,
                            CourseId = course.Id,
                            LessonId = lesson.Id,
                            StudentScore = 0,
                            CurrentTry = 0,
                            TimeProgress = 0,
                            State = StudentExamState.Pending,
                            SubmitDate = DateTime.MinValue,
                            Questions = lesson.Exam.Questions.Select(q => new StudentExamQuestion
                            {
                                Id = q.Id,
                                State = StudentExamQuestionState.Pending,
                                Score = 0,
                                Correct = false
                            }).ToList()
                        } : null
                    });
                }
            }
            var firstLesson = course.Disciplines.Where(d => d.Lessons.Any()).OrderBy(d => d.Order).FirstOrDefault().Lessons.OrderBy(l => l.Order).FirstOrDefault();
            if (firstLesson != null)
            {
                newCourse.Disciplines.SelectMany(d => d.Lessons).First(l => l.Id == firstLesson.Id).LastAccessDate = DateTime.UtcNow;
            }
            currentStudent?.Courses.Add(newCourse);
            return currentStudent;
        }

        private async Task<List<StudentCourseDto>> FillCoursesInfo(List<StudentCourseDto> courses)
        {
            foreach (var course in courses)
            {
                var fullCourse = await GetCourse(course.Id);
                if (fullCourse == null)
                {
                    logger.Warning($"Course {course.Id} not found for student {course.Id}");
                    continue;
                }
                await FillCourseInfo(course, fullCourse);
            }
            return courses;
        }

        private async Task<StudentCourseDto> FillCourseInfo(StudentCourseDto course, CourseDto fullCourse)
        {
            var totalLessons = course.Disciplines?.SelectMany(d => d.Lessons).Count() ?? 0;
            var progress = totalLessons > 0 ? course.Disciplines?.SelectMany(d => d.Lessons).Sum(l => l.Progress) / totalLessons : 0;
            course.Id = fullCourse.Id;
            course.Name = fullCourse.Name;
            course.Description = fullCourse.Description;
            course.Body = fullCourse.Body;
            course.Thumbnail = fullCourse.Thumbnail;
            course.ImageUrl = fullCourse.ImageUrl;
            course.Category = fullCourse.Categories?.FirstOrDefault();
            course.Tags = fullCourse.Tags;
            course.Progress = progress ?? 0;
            foreach (var discipline in course.Disciplines)
            {
                var courseDiscipline = fullCourse.Disciplines.Find(d => d.Id == discipline.Id);
                discipline.Id = courseDiscipline?.Id ?? string.Empty;
                discipline.Name = courseDiscipline?.Name ?? string.Empty;
                discipline.Code = courseDiscipline?.Code ?? string.Empty;
                discipline.Order = courseDiscipline?.Order ?? 0;
                discipline.Lessons.ForEach(lesson =>
                {
                    var courseLesson = fullCourse.Disciplines.Find(d => d.Id == discipline.Id)?.Lessons.Find(l => l.Id == lesson.Id);
                    if (courseLesson == null) return;
                    lesson.Id = courseLesson.Id;
                    lesson.Type = courseLesson.Type ?? LessonTypes.Lesson;
                    lesson.Name = courseLesson.Name;
                    lesson.Order = courseLesson.Order;
                    lesson.Description = courseLesson.Description;
                    lesson.TotalTimeMinutes = courseLesson.TotalTimeMinutes ?? 0;
                    lesson.IsLastAccessed = false;
                });
                discipline.Lessons = discipline.Lessons.OrderBy(l => l.Order).ToList();
            }
            course.Disciplines = course.Disciplines.OrderBy(d => d.Order).ToList();
            course.Disciplines.SelectMany(d => d.Lessons).MaxBy(l => l.LastAccessDate).IsLastAccessed = true;
            return course;
        }

        private async Task<bool> UpdateStudentCourse(StudentCourse course, CourseDto fullCourse)
        {
            var modified = false;
            var removedDisciplines = course.Disciplines.Where(d => !fullCourse.Disciplines!.Any(sd => sd.Id == d.Id)).ToList();
            if (removedDisciplines.Count > 0)
            {
                modified = true;
                foreach (var discipline in removedDisciplines)
                {
                    course.Disciplines.Remove(discipline);
                }
            }

            foreach (var discipline in course.Disciplines)
            {
                var removedLessons = discipline.Lessons.Where(l => !fullCourse.Disciplines!.Any(sd => sd.Id == discipline.Id && sd.Lessons.Any(sl => sl.Id == l.Id))).ToList();
                if (removedLessons.Count > 0)
                {
                    modified = true;
                    foreach (var lesson in removedLessons)
                    {
                        discipline.Lessons.Remove(lesson);
                    }
                }

                foreach (var lesson in discipline.Lessons)
                {
                    var fullLesson = fullCourse!.Disciplines!.Find(d => d.Id == discipline.Id)?.Lessons.Find(l => l.Id == lesson.Id);
                    if (fullLesson == null) continue;
                    lesson.Type = fullLesson.Type ?? LessonTypes.Lesson;
                }

                var newLessons = fullCourse.Disciplines!.Find(d => d.Id == discipline.Id)?.Lessons.Where(l => !discipline.Lessons.Any(sl => sl.Id == l.Id)).ToList() ?? [];
                if (newLessons.Count > 0)
                {
                    modified = true;
                    foreach (var lesson in newLessons)
                    {
                        discipline.Lessons.Add(new StudentLesson
                        {
                            CourseId = course.Id,
                            Id = lesson.Id,
                            DisciplineId = discipline.Id,
                            Type = lesson.Type ?? LessonTypes.Lesson,
                            State = StudentLessonState.Pending,
                            Annotations = [],
                            Progress = 0,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow,
                            LastAccessDate = DateTime.MinValue,
                            Exam = (lesson.Type == LessonTypes.Exam || lesson.Type == LessonTypes.Survey) && lesson.Exam != null ? new StudentExam
                            {
                                CourseId = course.Id,
                                LessonId = lesson.Id,
                                Id = lesson.Exam.Id,
                                StudentScore = 0,
                                CurrentTry = 0,
                                TimeProgress = 0,
                                State = StudentExamState.Pending,
                                SubmitDate = DateTime.MinValue,
                                Questions = lesson.Exam.Questions.Select(q => new StudentExamQuestion
                                {
                                    Id = q.Id,
                                    State = StudentExamQuestionState.Pending,
                                    Score = 0,
                                    Correct = false
                                }).ToList()
                            } : null
                        });
                    }
                }
            }

            var newDisciplines = fullCourse.Disciplines!.Where(d => !course.Disciplines.Any(sd => sd.Id == d.Id)).ToList();
            if (newDisciplines.Count > 0)
            {
                modified = true;
                foreach (var discipline in newDisciplines)
                {
                    course.Disciplines.Add(new StudentDiscipline
                    {
                        Id = discipline.Id,
                        Lessons = []
                    });
                    foreach (var lesson in discipline.Lessons)
                    {
                        course.Disciplines.Last().Lessons.Add(new StudentLesson
                        {
                            CourseId = course.Id,
                            Id = lesson.Id,
                            DisciplineId = discipline.Id,
                            Type = lesson.Type ?? LessonTypes.Lesson,
                            State = StudentLessonState.Pending,
                            Annotations = [],
                            Progress = 0,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow,
                            LastAccessDate = DateTime.MinValue,
                            Exam = (lesson.Type == LessonTypes.Exam || lesson.Type == LessonTypes.Survey) && lesson.Exam != null ? new StudentExam
                            {
                                CourseId = course.Id,
                                LessonId = lesson.Id,
                                Id = lesson.Exam.Id,
                                StudentScore = 0,
                                CurrentTry = 0,
                                TimeProgress = 0,
                                State = StudentExamState.Pending,
                                SubmitDate = DateTime.MinValue,
                                Questions = lesson.Exam.Questions.Select(q => new StudentExamQuestion
                                {
                                    Id = q.Id,
                                    State = StudentExamQuestionState.Pending,
                                    Score = 0,
                                    Correct = false
                                }).ToList()
                            } : null
                        });
                    }
                }
            }

            return modified;
        }

        private async Task<StudentLessonsDto> FillLessonInfo(StudentLessonsDto lesson, LoggedUserDto loggdUser)
        {
            var fullCourse = await GetCourse(lesson.CourseId);
            var courseDiscipline = fullCourse.Disciplines.Find(d => d.Id == lesson.DisciplineId);
            var courseLesson = courseDiscipline?.Lessons.Find(l => l.Id == lesson.Id);
            if (courseLesson == null) return lesson;
            lesson.Id = courseLesson.Id;
            lesson.Type = courseLesson.Type ?? LessonTypes.Lesson;
            lesson.Name = courseLesson.Name;
            lesson.Description = courseLesson.Description;
            lesson.Body = courseLesson.Body;
            lesson.Code = courseLesson.Code;
            lesson.Thumbnail = courseLesson.Thumbnail;
            lesson.ImageUrl = courseLesson.ImageUrl;
            lesson.VideoUrl = courseLesson.VideoUrl;
            lesson.EmbeddedVideo = courseLesson.EmbeddedVideo;
            lesson.TotalTimeMinutes = courseLesson.TotalTimeMinutes ?? 0;
            lesson.Materials = courseLesson.Materials;
            if (courseLesson.Type == LessonTypes.Video && !string.IsNullOrEmpty(courseLesson.VideoId))
            {
                lesson.VideoUrl = await GetVideoUrlAsync(courseLesson.VideoId, loggdUser);
            }
            else if ((courseLesson.Type == LessonTypes.Exam || courseLesson.Type == LessonTypes.Survey) && courseLesson.Exam != null)
            {
                if (lesson.Exam != null && lesson.Exam.ExamVersionNumber != courseLesson.Exam.VersionNumber)
                {
                    var examId = string.IsNullOrEmpty(lesson.Exam.ExamId) ? lesson.Exam.Id : lesson.Exam.ExamId;
                    courseLesson.Exam = await GetExamVersion(examId, lesson.Exam.ExamVersionNumber);
                }

                if (lesson.Exam == null)
                {
                    lesson.Exam = new StudentExamDto
                    {
                        Id = courseLesson.Exam.Id,
                        StudentScore = 0,
                        CurrentTry = 0,
                        TimeProgress = 0,
                        State = StudentExamState.Pending,
                        SubmitDate = DateTime.MinValue,
                        Questions = courseLesson.Exam.Questions.Select(q => new StudentExamQuestionDto
                        {
                            Order = q.Order,
                            Score = 0,
                            Correct = false
                        }).ToList()
                    };
                }

                lesson.Exam.MaxRetries = courseLesson.Exam.MaxRetries;
                lesson.Exam.RequiredScore = courseLesson.Exam.RequiredScore;
                lesson.Exam.TotalScore = courseLesson.Exam.TotalScore;
                lesson.Exam.RequiredLessons = courseLesson.Exam.RequiredLessons;
                lesson.Exam.Code = courseLesson.Exam.Code;
                lesson.Exam.Name = courseLesson.Exam.Name;
                lesson.Exam.Description = courseLesson.Exam.Description;
                lesson.Exam.Questions.ForEach(q =>
                {
                    var courseQuestion = courseLesson.Exam.Questions.Find(r => r.Id == q.Id);
                    if (courseQuestion != null)
                    {
                        q.Id = courseQuestion.Id;
                        q.Order = courseQuestion.Order;
                        q.Optional = courseQuestion.Optional;
                        q.Title = courseQuestion.Title;
                        q.Type = courseQuestion.Type;
                        q.AuxText = courseQuestion.AuxText;
                        q.QuestionsOptions = courseQuestion.Options.Select(o => new StudentExamQuestionOptionDto
                        {
                            Id = o.Id,
                            Order = o.Order,
                            Letter = o.Letter,
                            Body = o.Body
                        }).ToList();
                    }
                });
                lesson.Exam.Questions = lesson.Exam.Questions.OrderBy(q => q.Order).ToList();
            }
            return lesson;
        }

        private async Task<string?> GetVideoUrlAsync(string videoId, LoggedUserDto loggedUser)
        {
            var video = await videosService.GetByIdAsync(videoId);
            var result = string.Empty;
            if (video != null)
            {
                result = video.VideoUrl;
                if (result.StartsWith("\"")) result = result.Substring(1);
                if (result.EndsWith("\"")) result = result.Substring(0, result.Length - 1);
            }
            else
            {
                logger.Error($"Error retrieving video url {videoId}");
            }
            return result;
        }

        private async Task<ExamDto> GetExamVersion(string examId, int versionNumber)
        {
            return await examsService.GetVersionInternalAsync(examId, versionNumber) ?? throw new ArgumentException("Exam not found");
        }

        private async Task<UserDto> GetUser(string userId)
        {
            return await usersService.GetByIdAsync(userId) ?? throw new ArgumentException("User not found");
        }

        private async Task<ProductDto> GetProduct(string productId)
        {
            return await productsService.GetByIdAsync(productId) ?? throw new ArgumentException("Product not found");
        }

        private async Task<CourseDto> GetCourse(string courseId)
        {
            return await coursesService.GetByIdAsync(courseId) ?? throw new ArgumentException("Course not found");
        }

        private void ValidatePermissions(Student student, UserDto? loggedUser)
        {
            if (loggedUser == null
                || student.TenantId != loggedUser.TenantId
                || (loggedUser.Id != student.UserId && !loggedUser.Permissions.Contains(studentsApiSettings.StudentsAdminPermission ?? string.Empty)))
            {
                logger.Warning("Unauthorized update mismatch tenantID Student/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    student.Id, student.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private void RegisterEventStudent(string key, string message, LoggedUserDto loggedUser, Student student)
        {
            try
            {
                _ = eventRegister.RegisterEvent(loggedUser.User.TenantId, loggedUser.User.Id, key, message,
                                                    ["UserId", "StudentId", "StudentCode", "UserMainEmail", "UserLogin", "UserName"],
                                                    loggedUser.User.Id, student.Id, student.Code, loggedUser.User.MainEmail, loggedUser.User.Login, loggedUser.User.Name);
            }
            catch (Exception ex)
            {
                logger.Error("Fail registering event {key} for student {student}: -- error: {message}", key, student.Id, ex.Message, ex);
            }
        }

        private void RegisterEventEnrollment(string key, string message, UserDto user, Student student, CourseDto course, ProductDto product)
        {
            try
            {
                _ = eventRegister.RegisterEvent(user.TenantId, user.Id, key, message,
                                                    ["UserId", "StudentId", "CourseId", "ProductId", "ProductSku", "ProductName", "ProductDescription", "CourseName", "CourseDescription", "StudentCode", "UserMainEmail", "UserLogin", "UserName"],
                                                    user.Id, student.Id, course.Id, product.Id, product.Sku, product.Name, product.Description, course.Name, course.Description, student.Code, user.MainEmail, user.Login, user.Name);
            }
            catch (Exception ex)
            {
                logger.Error("Fail registering event {key} for student {student}: -- error: {message}", key, student.Id, ex.Message, ex);
            }
        }

        private void RegisterEventAllEnrollmentsDelete(UserDto user, Student student)
        {
            try
            {
                _ = eventRegister.RegisterEvent(user.TenantId, user.Id, EventKeys.AllEnrollmentsDeleted, "All Enrollments Deleted",
                                                    ["UserId", "StudentId", "StudentCode", "UserMainEmail", "UserLogin", "UserName"],
                                                    user.Id, student.Id, student.Code, user.MainEmail, user.Login, user.Name);
            }
            catch (Exception ex)
            {
                logger.Error("Fail registering event {key} for student {student}: -- error: {message}", EventKeys.AllEnrollmentsDeleted, student.Id, ex.Message, ex);
            }
        }
    }
}