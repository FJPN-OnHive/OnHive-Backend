namespace EHive.Core.Library.Enums.Catalog
{
    public enum CouponValidationResponseCodes
    {
        Valid = 0,
        Expired = 1,
        NotStarted = 2,
        InvalidCoupon = 3,
        MissingProduct = 4,
        InvalidProduct = 5,
        InvalidCategory = 6,
        UserLimitReached = 7,
        CouponLimitReached = 8
    }
}