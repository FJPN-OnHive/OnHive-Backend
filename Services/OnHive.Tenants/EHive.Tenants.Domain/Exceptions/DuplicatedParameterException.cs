namespace EHive.Tenants.Domain.Exceptions
{
    public class DuplicatedParameterException : Exception
    {
        public DuplicatedParameterException()
        { }

        public DuplicatedParameterException(string? message) : base(message)
        {
        }
    }
}