namespace OnHive.Configuration.Library.Exceptions
{
    public class MissingConfigurationException<T> : Exception
    {
        public MissingConfigurationException(string message)
            : base($"Missing configuration: {typeof(T).Name} - {message}")
        {
        }
    }
}