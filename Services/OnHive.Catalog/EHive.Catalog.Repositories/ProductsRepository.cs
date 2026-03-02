using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Catalog;
using EHive.Core.Library.Helpers;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;
using System.Xml.Linq;

namespace EHive.Catalog.Repositories
{
    public class ProductsRepository : MongoRepositoryBase<Product>, IProductsRepository
    {
        public ProductsRepository(MongoDBSettings settings) : base(settings, "Products")
        {
        }

        public async Task<List<Product>> GetAllActive(string tenantId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                           & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<PaginatedResult<Product>> GetByFilterActiveAsync(RequestFilter filter, string tenantId)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<Product>(filter, tenantId, true);
            var result = collection.Find(queryFilter);
            var count = 0L;
            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result = result.Sort(MongoDbFilterConverter.ConvertSort<Product>(filter));
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

            return new PaginatedResult<Product>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        public async Task<List<Product>> GetByIdsAsync(List<string> productIds)
        {
            var filter = Builders<Product>.Filter.In(p => p.Id, productIds)
                            & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<Product>> GetByItemIdsAsync(List<string> itensIds)
        {
            var filter = Builders<Product>.Filter.In(p => p.ItemId, itensIds)
                           & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByName(string tenantId, string name)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<Product>.Filter.StringIn(p => p.Name, name)
                            & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<Product> GetBySku(string tenantId, string sku)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<Product>.Filter.Eq(p => p.Sku, sku)
                            & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Product> GetBySlug(string tenantId, string slug)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                            & Builders<Product>.Filter.Eq(p => p.Slug, slug)
                            & Builders<Product>.Filter.Eq(p => p.IsActive, true);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Product> GetByAlternativeSlug(string slug, string tenantId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                          & Builders<Product>.Filter.Eq(p => p.IsActive, true)
                          & Builders<Product>.Filter.Exists(p => p.AlternativeSlugs)
                          & Builders<Product>.Filter.AnyEq(p => p.AlternativeSlugs, slug);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<FilterScope> GetFilterDataAsync(string tenantId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.TenantId, tenantId)
                         & Builders<Product>.Filter.Eq(p => p.IsActive, true);
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
                Field = "Tags",
                Values = (await collection
                            .Find(filter)
                            .ToListAsync())
                            .SelectMany(c => c.Tags)
                            .Distinct()
                            .ToList()
            });

            result.Fields.Add(new FilterScopeField
            {
                Field = "FullPrice",
                MinValue = (await collection
                            .Find(filter)
                            .SortBy(p => p.FullPrice)
                            .FirstOrDefaultAsync())
                            .ToString(),

                MaxValue = (await collection
                            .Find(filter)
                            .SortByDescending(p => p.FullPrice)
                            .FirstOrDefaultAsync())
                            .FullPrice.ToString()
            });
            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(i => i.Sku)));
            collection.Indexes.CreateOne(new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(i => i.Slug)));
            collection.Indexes.CreateOne(new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Text(i => i.Name).Text(i => i.Description)));
        }
    }
}