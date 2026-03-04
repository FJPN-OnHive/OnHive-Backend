using OnHive.Core.Library.Enums.Redirects;

namespace OnHive.Core.Library.Entities.Redirects
{
    public class Redirect : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string RedirectUrl { get; set; } = string.Empty;

        public bool PassParameters { get; set; } = true;

        public RedirectType Type { get; set; } = RedirectType.Temporary;
    }
}