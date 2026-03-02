namespace EHive.Users.Domain.Exceptions
{
    public class NotValidatedEmailException : Exception
    {
        public NotValidatedEmailException(string? message) : base(message)
        {
        }
    }
}