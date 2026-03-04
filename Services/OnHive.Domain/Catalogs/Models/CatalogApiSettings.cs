namespace OnHive.Catalog.Domain.Models
{
    public class CatalogApiSettings
    {
        public string? CatalogAdminPermission { get; set; } = "catalog_admin";

        public List<ProductType> ProductTypes { get; set; } = [new ProductType { Key = "course", BaseUrl = string.Empty, IsDefault = true }];
    }

    public class ProductType
    {
        public string? Key { get; set; }

        public string? BaseUrl { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}