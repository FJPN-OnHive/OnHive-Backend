using MongoDB.Driver;
using OnHive.Core.Library.Entities.Events;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Events.Domain.Abstractions.Repositories;
using System.Text.Json;
using MongoDB.Bson;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Extensions;

namespace OnHive.Events.Repositories
{
    public class WebHooksRepository : MongoRepositoryBase<WebHook>, IWebHooksRepository
    {
        private readonly IMongoDatabase database;

        public WebHooksRepository(MongoDBSettings settings) : base(settings, "WebHooks")
        {
            database = mongoClient.GetDatabase(settings.DataBase);
        }

        public async Task ExecuteAction(string tenantId, WebHookAction action, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var indexValue = string.Empty;
            var value = string.Empty;
            switch (action.SourceType)
            {
                case Core.Library.Enums.WebHook.WebHookFieldSourceTypes.Body:
                    indexValue = GetValueFromBody(action.SourceIndexField, body);
                    value = GetValueFromBody(action.SourceField, body);
                    break;

                case Core.Library.Enums.WebHook.WebHookFieldSourceTypes.Query:
                    indexValue = query.GetValueOrDefault(action.SourceIndexField);
                    value = query.GetValueOrDefault(action.SourceField);
                    break;

                case Core.Library.Enums.WebHook.WebHookFieldSourceTypes.Header:
                    indexValue = headers.GetValueOrDefault(action.SourceIndexField);
                    value = headers.GetValueOrDefault(action.SourceField);
                    break;

                default:
                    throw new NotImplementedException();
            }
            if (string.IsNullOrEmpty(indexValue))
            {
                throw new InvalidOperationException("Index value not found");
            }
            if (value == null)
            {
                throw new InvalidOperationException("Value not found");
            }
            var collection = database.GetCollection<BsonDocument>(action.TargetCollection);
            var filter = Builders<BsonDocument>.Filter.Eq("TenantId", tenantId)
                & Builders<BsonDocument>.Filter.Eq(action.TargetIndexField, indexValue);

            var current = await collection.Find(filter).FirstOrDefaultAsync();
            UpdateDefinition<BsonDocument> updateDefinition = null;
            var type = typeof(string);
            if (current == null)
            {
                throw new NotFoundException("Value not found");
            }
            if (!current.Elements.Any(e => e.Name == action.TargetField))
            {
                throw new NotFoundException($"Field {action.TargetField} not found");
            }
            switch (action.Type)
            {
                case Core.Library.Enums.WebHook.WebHookActionTypes.Replace:
                    if (CheckType(current.GetElement(action.TargetField), value, out type))
                    {
                        updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, BsonValue.Create(Convert.ChangeType(value, type)));
                    }
                    break;

                case Core.Library.Enums.WebHook.WebHookActionTypes.Increment:
                    if (CheckType(current.GetElement(action.TargetField), value, out type))
                    {
                        if (type == typeof(int))
                        {
                            var incrementValue = int.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToInt32();
                            var newValue = currentValue + incrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                        else if (type == typeof(float))
                        {
                            var incrementValue = float.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToDouble();
                            var newValue = currentValue + incrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                        else if (type == typeof(double))
                        {
                            var incrementValue = double.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToDouble();
                            var newValue = currentValue + incrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                    }
                    break;

                case Core.Library.Enums.WebHook.WebHookActionTypes.Decrement:
                    if (CheckType(current.GetElement(action.TargetField), value, out type))
                    {
                        if (type == typeof(int))
                        {
                            var decrementValue = int.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToInt32();
                            var newValue = currentValue - decrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                        else if (type == typeof(float))
                        {
                            var decrementValue = float.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToDouble();
                            var newValue = currentValue - decrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                        else if (type == typeof(double))
                        {
                            var decrementValue = double.Parse(value);
                            var currentValue = current.GetElement(action.TargetField).Value.ToDouble();
                            var newValue = currentValue - decrementValue;
                            updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                        }
                    }
                    break;

                case Core.Library.Enums.WebHook.WebHookActionTypes.Append:
                    if (CheckType(current.GetElement(action.TargetField), value, out type))
                    {
                        if (type != typeof(string))
                        {
                            break;
                        }
                        var appendValue = value.ToString();
                        var currentValue = current.GetElement(action.TargetField).Value.ToString();
                        var newValue = currentValue + appendValue;
                        updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                    }
                    break;

                case Core.Library.Enums.WebHook.WebHookActionTypes.Prepend:
                    if (CheckType(current.GetElement(action.TargetField), value, out type))
                    {
                        if (type != typeof(string))
                        {
                            break;
                        }
                        var appendValue = value.ToString();
                        var currentValue = current.GetElement(action.TargetField).Value.ToString();
                        var newValue = appendValue + currentValue;
                        updateDefinition = Builders<BsonDocument>.Update.Set(action.TargetField, newValue);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            var idFilter = Builders<BsonDocument>.Filter.Eq("_id", current.GetValue("_id").ToString());
            await collection.UpdateOneAsync(idFilter, updateDefinition);
        }

        private bool CheckType(BsonElement bsonElement, string value, out Type result)
        {
            var type = bsonElement.Value.BsonType;
            result = typeof(string);
            switch (type)
            {
                case BsonType.String:
                    return true;

                case BsonType.Double:
                case BsonType.Decimal128:
                    if (double.TryParse(value, out _))
                    {
                        result = typeof(double);
                        return true;
                    }
                    return false;

                case BsonType.Boolean:

                    if (bool.TryParse(value, out _))
                    {
                        result = typeof(bool);
                        return true;
                    }
                    return false;

                case BsonType.DateTime:
                    if (DateTime.TryParse(value, out _))
                    {
                        result = typeof(DateTime);
                        return true;
                    }
                    return false;

                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.Timestamp:
                    if (int.TryParse(value, out _))
                    {
                        result = typeof(int);
                        return true;
                    }
                    return false;

                case BsonType.EndOfDocument:
                case BsonType.Document:
                case BsonType.Array:
                case BsonType.Binary:
                case BsonType.Undefined:
                case BsonType.ObjectId:
                case BsonType.Null:
                case BsonType.RegularExpression:
                case BsonType.JavaScript:
                case BsonType.Symbol:
                case BsonType.JavaScriptWithScope:
                case BsonType.MinKey:
                case BsonType.MaxKey:
                    return false;

                default:
                    return false;
            }
        }

        public Task<WebHook?> GetBySlug(string tenantId, string slug)
        {
            var filter = Builders<WebHook>.Filter.Eq(i => i.TenantId, tenantId) & Builders<WebHook>.Filter.Eq(i => i.Slug, slug);
            return collection.Find(filter).FirstOrDefaultAsync();
        }

        private string GetValueFromBody(string key, JsonDocument? body)
        {
            var result = string.Empty;

            if (body != null)
            {
                var value = body.GetElementByPath(key);
                return value.ToString();
            }

            return result;
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<WebHook>(Builders<WebHook>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}