namespace OnHive.Core.Library.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string? message) : base(message)
        {
        }
    }
}