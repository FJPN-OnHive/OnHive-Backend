namespace OnHive.Core.Library.Entities.Catalog
{
    public class Coupon : EntityBase
    {
        public string Key { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Discount { get; set; } = 0;

        public bool IsPercentage { get; set; } = false;

        public int Quantity { get; set; } = 0;

        public int UsesPerUser { get; set; } = 1;

        public List<string> Products { get; set; } = [];

        public List<string> Categories { get; set; } = [];

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);
    }
}