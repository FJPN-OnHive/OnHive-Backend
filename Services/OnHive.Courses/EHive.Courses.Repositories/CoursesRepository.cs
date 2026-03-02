using MongoDB.Driver;
using EHive.Core.Library.Entities.Courses;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Courses.Domain.Abstractions.Repositories;
using EHive.Core.Library.Entities.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Helpers;
using SharpCompress.Common;

namespace EHive.Courses.Repositories
{
    public class CoursesRepository : MongoRepositoryBase<Course>, ICoursesRepository
    {
        public CoursesRepository(MongoDBSettings settings) : base(settings, "Courses")
        {
        }

        public async Task<List<Course>> GetAllActive(string tenantId)
        {
            var filter = Builders<Course>.Filter.Eq(p => p.TenantId, tenantId)
                           & Builders<Course>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<PaginatedResult<Course>> GetByFilterAndUserAsync(List<string> ids, RequestFilter filter, string? tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<Course>(filter, tenantId, true);
            queryFilter = queryFilter & Builders<Course>.Filter.In(p => p.Id, ids);

            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<Course>(filter));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result = result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<Course>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = await result.CountDocumentsAsync(),
                Itens = await result.ToListAsync()
            };
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Course>(Builders<Course>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Course>(Builders<Course>.IndexKeys
                .Text(i => i.Name)
                .Text(i => i.Description)
                .Text(i => i.Body)));
        }
    }
}