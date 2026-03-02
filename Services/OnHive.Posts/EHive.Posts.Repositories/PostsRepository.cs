using MongoDB.Driver;
using EHive.Core.Library.Entities.Posts;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Posts.Domain.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Helpers;
using MongoDB.Bson;

namespace EHive.Posts.Repositories
{
    public class PostsRepository : MongoRepositoryBase<BlogPost>, IPostsRepository
    {
        public PostsRepository(MongoDBSettings settings) : base(settings, "Posts")
        {
        }

        public Task<List<BlogPost>> GetAllActive(string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.TenantId, tenantId)
                & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published)
                & Builders<BlogPost>.Filter.Eq(i => i.Visibility, Core.Library.Enums.Posts.PostVisibility.Public);
            return collection.Find(filter).ToListAsync();
        }

        public Task<List<BlogPost>> GetReadyToPublish()
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Scheduled)
                & Builders<BlogPost>.Filter.Lte(i => i.PublishDate, DateTime.UtcNow);
            return collection.Find(filter).ToListAsync();
        }

        public async Task<long> DeleteUnsaved(DateTime olderThan)
        {
            var filter = Builders<BlogPost>.Filter.Lt(i => i.CreatedAt, olderThan)
                & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Draft);
            var result = await collection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public Task<BlogPost> GetBySlug(string slug, string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.Slug, slug)
                & Builders<BlogPost>.Filter.Eq(i => i.TenantId, tenantId)
                & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published)
                & Builders<BlogPost>.Filter.Eq(i => i.Visibility, Core.Library.Enums.Posts.PostVisibility.Public);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        public Task<List<BlogPost>> GetPublishedByCourseAsync(string courseId, string? tenantId)
        {
            var queryFilter = Builders<BlogPost>.Filter.Eq(e => e.TenantId, tenantId)
             & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published)
             & Builders<BlogPost>.Filter.AnyStringIn(i => i.RequiredCourses, courseId);
            return collection.Find(queryFilter).ToListAsync();
        }

        public Task<List<string>> GetSlugsAsync(string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.TenantId, tenantId);
            return collection.Find(filter).Project(p => p.Slug).ToListAsync();
        }

        public Task<BlogPost?> GetByAlternativeSlug(string slug, string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.TenantId, tenantId)
              & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published)
              & Builders<BlogPost>.Filter.Eq(i => i.Visibility, Core.Library.Enums.Posts.PostVisibility.Public)
              & Builders<BlogPost>.Filter.Exists(i => i.AlternativeSlugs)
              & Builders<BlogPost>.Filter.AnyEq(i => i.AlternativeSlugs, slug);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        public Task<List<BlogPost>> GetPublishedCanonicalListAsync(string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(i => i.TenantId, tenantId)
                & Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published)
                & Builders<BlogPost>.Filter.Exists(i => i.CanonicalUrl)
                & Builders<BlogPost>.Filter.Ne(i => i.CanonicalUrl, string.Empty);
            return collection.Find(filter).ToListAsync();
        }

        public async Task<PaginatedResult<BlogPost>> GetPublishedByFilterAsync(RequestFilter filter, string? tenantId, bool publicOnly)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<BlogPost>(filter, tenantId, false);
            queryFilter = Builders<BlogPost>.Filter.And(queryFilter, Builders<BlogPost>.Filter.Eq(i => i.Status, Core.Library.Enums.Posts.PostStatus.Published));
            if (publicOnly)
            {
                queryFilter = Builders<BlogPost>.Filter.And(queryFilter, Builders<BlogPost>.Filter.Eq(i => i.Visibility, Core.Library.Enums.Posts.PostVisibility.Public));
            }
            var result = collection.Find(queryFilter);
            var pageCount = 1L;
            var count = 0L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<BlogPost>(filter));
            }
            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                count = await result.CountDocumentsAsync();
                result
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<BlogPost>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        public async Task<FilterScope> GetFilterDataAsync(string tenantId)
        {
            var filter = Builders<BlogPost>.Filter.Eq(p => p.TenantId, tenantId) & Builders<BlogPost>.Filter.Eq(p => p.Status, Core.Library.Enums.Posts.PostStatus.Published);
            var result = new FilterScope();

            result.Fields.Add(new FilterScopeField
            {
                Field = "Categories",
                Values = (await collection
                            .Find(filter)
                            .Project(p => p.Categories)
                            .ToListAsync())
                            .SelectMany(c => c)
                            .Distinct()
                            .ToList()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "Tags",
                Values = (await collection
                            .Find(filter)
                            .Project(p => p.Tags)
                            .ToListAsync())
                            .SelectMany(c => c)
                            .Distinct()
                            .ToList()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "Author",
                Values = (await collection
                           .Find(filter)
                           .Project(p => p.Author)
                           .ToListAsync())
                           .Distinct()
                           .ToList()
            });

            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPost>(Builders<BlogPost>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPost>(Builders<BlogPost>.IndexKeys.Ascending(i => i.Categories)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPost>(Builders<BlogPost>.IndexKeys.Ascending(i => i.AuthorId)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPost>(Builders<BlogPost>.IndexKeys.Ascending(i => i.Slug)));
            collection.Indexes.CreateOne(new CreateIndexModel<BlogPost>(Builders<BlogPost>.IndexKeys.Text(i => i.Title)));
        }
    }
}