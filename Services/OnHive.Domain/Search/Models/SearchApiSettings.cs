using System.ComponentModel.DataAnnotations;

namespace OnHive.Search.Domain.Models
{
    public class SearchApiSettings
    {
        public string? SearchAdminPermission { get; set; } = "search_admin";

        public List<Target>? Targets { get; set; } = [
            new Target{
                Type = "COURSES",
                CollectionName = "courses",
                ValueField = "Name",
                DescriptionField = "Description",
                ImageField = "Thumbnail",
                SlugField = "Slug",
                UrlField = "Url",
                UpdatedAtField = "UpdatedAt",
                ActiveCriteria = [
                    new ActiveCriteria{
                        Field = "IsActive",
                        Values = "true"
                    }
                ]
            },
            new Target{
                Type = "POSTS",
                CollectionName = "Posts",
                ValueField = "Title",
                DescriptionField = "Description",
                ImageField = "Thumbnail",
                SlugField = "Slug",
                UrlField = "",
                UpdatedAtField = "PublishDate",
                ActiveCriteria = [
                    new ActiveCriteria{
                        Field = "Status",
                        Values = "3"
                    },
                    new ActiveCriteria{
                        Field = "Visibility",
                        Values = "0"
                    }
                ]
            }
            ];
    }

    public class Target
    {
        public string Type { get; set; } = string.Empty;

        public string CollectionName { get; set; } = string.Empty;

        public string ValueField { get; set; } = string.Empty;

        public string DescriptionField { get; set; } = string.Empty;

        public string ImageField { get; set; } = string.Empty;

        public string ImageAltTextField { get; set; } = string.Empty;

        public string SlugField { get; set; } = string.Empty;

        public string UrlField { get; set; } = string.Empty;

        public string UpdatedAtField { get; set; } = "UpdatedAt";

        public List<ActiveCriteria> ActiveCriteria { get; set; } = [];
    }

    public class ActiveCriteria
    {
        public string Field { get; set; } = string.Empty;

        public string Values { get; set; } = string.Empty;
    }
}