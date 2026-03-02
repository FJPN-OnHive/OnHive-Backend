namespace EHive.Tenants.Domain.Exceptions
{
    public class MissingParameterException : Exception
    {
        public MissingParameterException()
        {
        }

        public MissingParameterException(string? message) : base(message)
        {
        }
    }
}