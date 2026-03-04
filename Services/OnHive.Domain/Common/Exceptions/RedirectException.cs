using OnHive.Core.Library.Contracts.Common;

namespace OnHive.Core.Library.Exceptions
{
    public class RedirectException : Exception
    {
        public RedirectResponse Response { get; set; }

        public RedirectException(string message, RedirectResponse response) : base(message)
        {
            Response = response;
        }

        public RedirectException(string type, string url, string slug, string code) : base($"Redirect {type} to {slug} with {code}")
        {
            Response = new RedirectResponse { Type = type, Url = url, Slug = slug, RedirectCode = code };
        }
    }
}