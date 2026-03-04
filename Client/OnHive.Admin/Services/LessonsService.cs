using OnHive.Core.Library.Contracts.Courses;

namespace OnHive.Admin.Services
{
    public class LessonsService : ServiceBase<LessonDto>, ILessonsService
    {
        public LessonsService(HttpClient httpClient) : base(httpClient, "/v1/Lesson")
        {
        }
    }
}