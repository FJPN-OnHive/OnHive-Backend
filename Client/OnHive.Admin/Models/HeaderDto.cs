namespace OnHive.Admin.Models
{
    public class HeaderDto
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KeyValuePair<string, string> ToKeyValuePair()
        {
            return new(Key, Value);
        }
    }
}