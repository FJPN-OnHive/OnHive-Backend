using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Catalog;
using EHive.Core.Library.Entities.Courses;
using EHive.Core.Library.Entities.Search;

namespace EHive.Search.Domain.Abstractions.Repositories
{
    public interface IProductCourseSearchRepository : IRepositoryBase<ProductCourseSearch>
    {
        public Task<List<Course>> GetUpdatedCourses(DateTime limitDate);

        public Task<List<Product>> GetUpdatedProduct(DateTime limitDate, List<string> notInIds);

        public Task<DateTime> GetLowerDate();

        public Task<Course> GetCourseById(string courseId);

        public Task<List<Course>> GetCourseByIds(List<string> courseIds);

        public Task<List<Product>> GetProductsByCourseId(string courseId);

        public Task<List<Product>> GetProductsByCourseIds(List<string> courseIds);
        Task<FilterScope> GetFilterDataAsync(string tenantId);
    }
}