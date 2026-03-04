namespace OnHive.Users.Domain.Exceptions
{
    public class InvalidUserException : Exception
    {
        public InvalidUserException(List<string> invalidFields) : base(string.Join(";", invalidFields))
        {
        }
    }
}