using EHive.Core.Library.Contracts.Courses;

namespace EHive.Admin.Services
{
    public class DisciplinesService : ServiceBase<DisciplineDto>, IDisciplinesService
    {
        public DisciplinesService(HttpClient httpClient) : base(httpClient, "/v1/Discipline")
        {
        }
    }
}