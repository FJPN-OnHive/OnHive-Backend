using MongoDB.Driver;
using OnHive.Core.Library.Entities.Search;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Search.Domain.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Core.Library.Contracts.Common;

namespace OnHive.Search.Repositories
{
    public class ProductCourseSearchRepository : MongoRepositoryBase<ProductCourseSearch>, IProductCourseSearchRepository
    {
        private readonly IMongoCollection<Course> courseCollection;
        private readonly IMongoCollection<Product> productCollection;

        public ProductCourseSearchRepository(MongoDBSettings settings) : base(settings, "ProductCourse")
        {
            courseCollection = mongoClient.GetDatabase(settings.DataBase).GetCollection<Course>("Courses");
            productCollection = mongoClient.GetDatabase(settings.DataBase).GetCollection<Product>("Products");
        }

        public async Task<DateTime> GetLowerDate()
        {
            var lowerDate = await collection.Aggregate().SortByDescending(i => i.SnapshotDate).Limit(1).FirstOrDefaultAsync();
            return lowerDate?.SnapshotDate ?? DateTime.MinValue;
        }

        public async Task<List<Course>> GetUpdatedCourses(DateTime limitDate)
        {
            var queryFilter = Builders<Course>.Filter.Gt(e => e.UpdatedAt, limitDate);
            var result = courseCollection.Find(queryFilter);
            return await result.ToListAsync();
        }

        public async Task<List<Product>> GetUpdatedProduct(DateTime limitDate, List<string> notInIds)
        {
            var queryFilter = Builders<Product>.Filter.Gt(e => e.UpdatedAt, limitDate)
                & Builders<Product>.Filter.Nin(e => e.Id, notInIds);
            var result = productCollection.Find(queryFilter);
            return await result.ToListAsync();
        }

        public async Task<Course> GetCourseById(string courseId)
        {
            var queryFilter = Builders<Course>.Filter.Eq(e => e.Id, courseId);
            var result = courseCollection.Find(queryFilter);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetProductsByCourseId(string courseId)
        {
            var queryFilter = Builders<Product>.Filter.Eq(e => e.ItemId, courseId);
            var result = productCollection.Find(queryFilter);
            return await result.ToListAsync();
        }

        public async Task<List<Course>> GetCourseByIds(List<string> courseIds)
        {
            var queryFilter = Builders<Course>.Filter.In(e => e.Id, courseIds);
            var result = courseCollection.Find(queryFilter);
            return await result.ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCourseIds(List<string> courseIds)
        {
            var queryFilter = Builders<Product>.Filter.In(e => e.ItemId, courseIds);
            var result = productCollection.Find(queryFilter);
            return await result.ToListAsync();
        }

        public async Task<FilterScope> GetFilterDataAsync(string tenantId)
        {
            var filter = Builders<ProductCourseSearch>.Filter.Eq(p => p.TenantId, tenantId)
                        & Builders<ProductCourseSearch>.Filter.Eq(p => p.IsActive, true);

            var result = new FilterScope();

            result.Fields.Add(new FilterScopeField
            {
                Field = "Categories",
                Values = (await collection
                             .Find(filter)
                             .ToListAsync())
                             .SelectMany(c => c.Categories)
                             .Distinct()
                             .ToList()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "FullPrice",
                MinValue = (await collection
                            .Find(filter)
                            .SortBy(p => p.FullPrice)
                            .FirstOrDefaultAsync())?
                            .FullPrice.ToString(),

                MaxValue = (await collection
                            .Find(filter)
                            .SortByDescending(p => p.FullPrice)
                            .FirstOrDefaultAsync())?
                            .FullPrice.ToString()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "LowPrice",
                MinValue = (await collection
                            .Find(filter)
                            .SortBy(p => p.LowPrice)
                            .FirstOrDefaultAsync())?
                            .LowPrice.ToString(),

                MaxValue = (await collection
                            .Find(filter)
                            .SortByDescending(p => p.LowPrice)
                            .FirstOrDefaultAsync())?
                            .LowPrice.ToString()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "Sales",
                MinValue = (await collection
                            .Find(filter)
                            .SortBy(p => p.Sales)
                            .FirstOrDefaultAsync())?
                            .Sales.ToString(),

                MaxValue = (await collection
                            .Find(filter)
                            .SortByDescending(p => p.Sales)
                            .FirstOrDefaultAsync())?
                            .Sales.ToString()
            });
            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Ascending(i => i.CourseId)));
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Ascending(i => i.ProductId)));
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Ascending(i => i.Code)));
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Ascending(i => i.UpdatedAt)));
            collection.Indexes.CreateOne(new CreateIndexModel<ProductCourseSearch>(Builders<ProductCourseSearch>.IndexKeys.Text(i => i.Name).Text(i => i.Description).Text(i => i.Category).Text(i => i.Tags)));
        }
    }
}