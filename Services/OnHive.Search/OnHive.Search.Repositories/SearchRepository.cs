using MongoDB.Driver;
using OnHive.Core.Library.Entities.Search;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Search.Domain.Abstractions.Repositories;
using OnHive.Search.Domain.Models;
using MongoDB.Bson;
using Serilog;

namespace OnHive.Search.Repositories
{
    public class SearchRepository : MongoRepositoryBase<SearchResult>, ISearchRepository
    {
        private readonly IMongoDatabase database;

        public SearchRepository(MongoDBSettings settings) : base(settings, "Search")
        {
            var client = new MongoClient(settings.ConnectionString);
            database = client.GetDatabase(settings.DataBase);
        }

        public async Task<List<SearchResult>> GetDataAsync(Target target)
        {
            var result = new List<SearchResult>();
            var collection = database.GetCollection<BsonDocument>(target.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Empty;
            var bsonResult = await collection.Find(filter).ToListAsync();
            foreach (var item in bsonResult)
            {
                try
                {
                    var searchResult = new SearchResult();
                    searchResult.Id = item.GetValue("_id").ToString() ?? string.Empty;
                    searchResult.SourceId = item.GetValue("_id").ToString() ?? string.Empty;
                    searchResult.TenantId = item.GetValue("TenantId").ToString() ?? string.Empty;
                    searchResult.Type = target.Type;
                    if (!string.IsNullOrEmpty(target.ValueField) && item.TryGetValue(target.ValueField, out var valueField) && valueField.BsonType != BsonType.Null)
                    {
                        searchResult.Value = valueField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.DescriptionField) && item.TryGetValue(target.DescriptionField, out var descriptionField) && descriptionField.BsonType != BsonType.Null)
                    {
                        searchResult.Description = descriptionField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.ImageField) && item.TryGetValue(target.ImageField, out var imageField) && imageField.BsonType != BsonType.Null)
                    {
                        searchResult.SourceImageUrl = imageField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.ImageAltTextField) && item.TryGetValue(target.ImageAltTextField, out var imageAltTextField) && imageAltTextField.BsonType != BsonType.Null)
                    {
                        searchResult.SourceImageAltText = imageAltTextField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.SlugField) && item.TryGetValue(target.SlugField, out var slugField) && slugField.BsonType != BsonType.Null)
                    {
                        searchResult.SourceSlug = slugField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.UrlField) && item.TryGetValue(target.UrlField, out var urlField) && urlField.BsonType != BsonType.Null)
                    {
                        searchResult.SourceUrl = urlField.AsString ?? string.Empty;
                    }
                    if (!string.IsNullOrEmpty(target.UpdatedAtField) && item.TryGetValue(target.UpdatedAtField, out var updatedAtField) && updatedAtField.BsonType != BsonType.Null)
                    {
                        searchResult.UpdatedAt = updatedAtField.AsLocalTime;
                    }
                    else if (item.TryGetValue("UpdatedAt", out var defaultUpdatedAtField) && defaultUpdatedAtField.BsonType != BsonType.Null)
                    {
                        searchResult.UpdatedAt = defaultUpdatedAtField.AsLocalTime;
                    }

                    searchResult.IsActive = true;
                    foreach (var criteria in target.ActiveCriteria)
                    {
                        if (item.TryGetValue(criteria.Field, out var field) && field.BsonType != BsonType.Null)
                        {
                            if (criteria.Values != field.ToString())
                            {
                                searchResult.IsActive = false;
                                break;
                            }
                        }
                    }
                    result.Add(searchResult);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Error getting search data from collection {collection}", target.CollectionName);
                }
            }
            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<SearchResult>(Builders<SearchResult>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<SearchResult>(Builders<SearchResult>.IndexKeys.Ascending(i => i.Type)));
            collection.Indexes.CreateOne(new CreateIndexModel<SearchResult>(Builders<SearchResult>.IndexKeys.Text(i => i.Value).Text(i => i.Description)));
        }
    }
}