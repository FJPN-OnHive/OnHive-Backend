namespace EHive.Core.Library.Entities.Catalog
{
    public class UserCoupon : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string CouponId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;
    }
}