namespace EHive.Core.Library.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string? message) : base(message)
        {
        }
    }
}