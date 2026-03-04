using Microsoft.AspNetCore.Http;
using OnHive.Core.Library.Contracts.Common;

namespace OnHive.WebExtensions.Library
{
    public static class RequestQueryExtensions
    {
        public static bool HasQueryFilter(this HttpContext context)
        {
            return context.Request.Query.Any()
                && (context.Request.Query.ContainsKey("page")
                    || context.Request.Query.ContainsKey("page_limit")
                    || context.Request.Query.ContainsKey("sort")
                    || context.Request.Query.ContainsKey("filter")
                    || context.Request.Query.ContainsKey("and_filter")
                    || context.Request.Query.ContainsKey("or_filter")
                    || context.Request.Query.ContainsKey("type")
                    || context.Request.Query.ContainsKey("text"));
        }

        public static RequestFilter GetFilter(this HttpContext context)
        {
            var result = new RequestFilter();
            if (context.HasQueryFilter())
            {
                if (context.Request.Query.ContainsKey("page")
                    && int.TryParse(context.Request.Query["page"], out var page))
                {
                    result.Page = page;
                }
                if (context.Request.Query.ContainsKey("page_limit")
                    && int.TryParse(context.Request.Query["page_limit"], out var pageLimit))
                {
                    result.PageLimit = pageLimit;
                }
                if (context.Request.Query.ContainsKey("filter"))
                {
                    result.Filter = ParseFilter(context.Request.Query["filter"].ToString());
                }
                if (context.Request.Query.ContainsKey("and_filter"))
                {
                    result.AndFilter = ParseFilter(context.Request.Query["and_filter"].ToString());
                }
                if (context.Request.Query.ContainsKey("or_filter"))
                {
                    result.OrFilter = ParseFilter(context.Request.Query["or_filter"].ToString());
                }
                if (context.Request.Query.ContainsKey("sort"))
                {
                    result.Sort = ParseSort(context.Request.Query["sort"].ToString());
                }
                if (context.Request.Query.ContainsKey("type"))
                {
                    result.Type = context.Request.Query["type"].ToString();
                }
                if (context.Request.Query.ContainsKey("text"))
                {
                    result.Text = context.Request.Query["text"].ToString();
                }
            }
            return result;
        }

        private static List<FilterField> ParseFilter(string input)
        {
            var result = new List<FilterField>();
            var filters = input.Split(';');
            foreach (var filter in filters)
            {
                if (FilterField.TryParse(filter, out var field) && field != null)
                {
                    result.Add(field);
                }
            }
            return result;
        }

        private static List<FieldSort> ParseSort(string input)
        {
            var result = new List<FieldSort>();
            var sorts = input.Split(';');
            foreach (var sort in sorts)
            {
                if (FieldSort.TryParse(sort, out var field) && field != null)
                {
                    result.Add(field);
                }
            }
            return result;
        }
    }
}