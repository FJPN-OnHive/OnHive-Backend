using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Entities.Search;

namespace OnHive.Search.Domain.Abstractions.Repositories
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