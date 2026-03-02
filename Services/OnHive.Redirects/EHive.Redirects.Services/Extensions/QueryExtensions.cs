using Microsoft.AspNetCore.Http;

namespace EHive.Redirects.Services.Extensions
{
    public static class QueryExtensions
    {
        public static string GetQuery(this IQueryCollection query)
        {
            if (query == null || query.Count == 0)
            {
                return string.Empty;
            }
            return "?" + string.Join("&", query.Select(q => $"{q.Key}={q.Value}"));
        }
    }
}