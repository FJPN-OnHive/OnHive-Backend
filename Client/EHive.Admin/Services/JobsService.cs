using EHive.Core.Library.Contracts.Carreiras;

namespace EHive.Admin.Services
{
    public class JobsService : ServiceBase<JobDto>, IJobsService
    {
        public JobsService(HttpClient httpClient) : base(httpClient, "/v1/Job")
        {
        }
    }
}