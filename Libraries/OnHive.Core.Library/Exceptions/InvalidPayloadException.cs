namespace EHive.Core.Library.Domain.Exceptions
{
    public class InvalidPayloadException : Exception
    {
        public InvalidPayloadException(List<string> invalidFields) : base(string.Join(";", invalidFields))
        {
        }
    }
}