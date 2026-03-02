using EHive.Core.Library.Contracts.Courses;

namespace EHive.Admin.Services
{
    public class LessonsService : ServiceBase<LessonDto>, ILessonsService
    {
        public LessonsService(HttpClient httpClient) : base(httpClient, "/v1/Lesson")
        {
        }
    }
}