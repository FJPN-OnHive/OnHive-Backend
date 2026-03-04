namespace OnHive.Users.Domain.Exceptions
{
    public class DuplicatedUserException : Exception
    {
        public DuplicatedUserException(string? duplicatedKey) : base(duplicatedKey)
        {
        }
    }
}