using MongoDB.Driver;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace EHive.Core.Library.Helpers
{
    public static class MongoDbFilterConverter
    {
        public static FilterDefinition<T> ConvertFilter<T>(RequestFilter filter, string? tenantId, bool activeOnly) where T : EntityBase
        {
            List<FilterDefinition<T>> resultList = [];
            List<FilterDefinition<T>> resultAndList = [];
            List<FilterDefinition<T>> resultOrList = [];
            FilterDefinition<T>? result = null;

            foreach (var field in filter.Filter)
            {
                SetFilterList(resultList, field);
            }

            foreach (var field in filter.AndFilter)
            {
                SetFilterList(resultAndList, field);
            }

            foreach (var field in filter.OrFilter)
            {
                SetFilterList(resultOrList, field);
            }

            if (!string.IsNullOrEmpty(filter.Text))
            {
                resultList.Add(Builders<T>.Filter.Text(filter.Text));
            }

            if (resultList.Any())
            {
                result = (filter.Type.ToUpper() == "AND")
                            ? Builders<T>.Filter.And(resultList)
                            : Builders<T>.Filter.Or(resultList);
            }

            if (result != null)
            {
                result = Builders<T>.Filter.And(result, Builders<T>.Filter.Eq(e => e.ActiveVersion, true));
            }
            else
            {
                result = Builders<T>.Filter.Or(Builders<T>.Filter.Eq(e => e.ActiveVersion, true), Builders<T>.Filter.Not(Builders<T>.Filter.Exists(e => e.ActiveVersion)));
            }

            if (resultAndList.Any())
            {
                var resultAndFilter = Builders<T>.Filter.And(resultAndList);
                if (result != null)
                    result = Builders<T>.Filter.And(result, resultAndFilter);
                else
                    result = resultAndFilter;
            }

            if (resultOrList.Any())
            {
                var resultOrFilter = Builders<T>.Filter.Or(resultOrList);
                if (result != null)
                    result = Builders<T>.Filter.And(result, resultOrFilter);
                else
                    result = resultOrFilter;
            }

            if (activeOnly)
            {
                if (result != null)
                    result = Builders<T>.Filter.And(result, Builders<T>.Filter.Eq(e => e.TenantId, tenantId), Builders<T>.Filter.Eq(e => e.IsActive, true));
                else
                    result = Builders<T>.Filter.And(Builders<T>.Filter.Eq(e => e.TenantId, tenantId), Builders<T>.Filter.Eq(e => e.IsActive, true));
            }
            else
            {
                if (result != null)
                    result = Builders<T>.Filter.And(result, Builders<T>.Filter.Eq(e => e.TenantId, tenantId));
                else
                    result = Builders<T>.Filter.Eq(e => e.TenantId, tenantId);
            }

            return result ?? Builders<T>.Filter.Empty;
        }

        private static void SetFilterList<T>(List<FilterDefinition<T>> resultList, FilterField field) where T : EntityBase
        {
            var fieldName = FindFieldName<T>(field.Field);
            if (string.IsNullOrEmpty(fieldName)) return;
            switch (field.Operator.ToUpper())
            {
                case "REG":
                    var regEx = BsonRegularExpression.Create(new Regex(field.Value, RegexOptions.IgnoreCase));
                    resultList.Add(Builders<T>.Filter.Regex(fieldName, regEx));
                    break;

                case "GT":
                    resultList.Add(Builders<T>.Filter.Gt(fieldName, field.Value));
                    break;

                case "LT":
                    resultList.Add(Builders<T>.Filter.Lt(fieldName, field.Value));
                    break;

                case "GTE":
                    resultList.Add(Builders<T>.Filter.Gte(fieldName, field.Value));
                    break;

                case "LTE":
                    resultList.Add(Builders<T>.Filter.Lte(fieldName, field.Value));
                    break;

                case "EQ":
                    resultList.Add(Builders<T>.Filter.Eq(fieldName, field.Value));
                    break;

                case "NE":
                    resultList.Add(Builders<T>.Filter.Ne(fieldName, field.Value));
                    break;

                case "IN":
                    resultList.Add(Builders<T>.Filter.In(fieldName, field.Value.Split(",")));
                    break;

                case "NIN":
                    resultList.Add(Builders<T>.Filter.Nin(fieldName, field.Value.Split(",")));
                    break;

                case "BTW":
                    resultList.Add(Builders<T>.Filter.And(Builders<T>.Filter.Gte(fieldName, field.Value), Builders<T>.Filter.Lte(fieldName, field.ValueVariantion)));
                    break;

                case "BTWX":
                    resultList.Add(Builders<T>.Filter.And(Builders<T>.Filter.Gt(fieldName, field.Value), Builders<T>.Filter.Lt(fieldName, field.ValueVariantion)));
                    break;

                default:
                    throw new ArgumentException($"Invalid Operator: {field.Operator}");
            }
        }

        public static SortDefinition<T> ConvertSort<T>(RequestFilter filter) where T : EntityBase
        {
            var result = new List<SortDefinition<T>>();

            foreach (var field in filter.Sort)
            {
                var fieldName = FindFieldName<T>(field.Field);
                if (string.IsNullOrEmpty(fieldName)) continue;
                switch (field.Order.ToUpper())
                {
                    case "ASC":
                    case "A":
                        result.Add(Builders<T>.Sort.Ascending(fieldName));
                        break;

                    case "DESC":
                    case "D":
                        result.Add(Builders<T>.Sort.Descending(fieldName));
                        break;

                    default:
                        throw new ArgumentException($"Invalid sort order: {field.Order}");
                }
            }
            return Builders<T>.Sort.Combine(result);
        }

        private static string FindFieldName<T>(string field) where T : EntityBase
        {
            return typeof(T)
                .GetMembers()
                .FirstOrDefault(f => f.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))?
                .Name ?? string.Empty;
        }
    }
}