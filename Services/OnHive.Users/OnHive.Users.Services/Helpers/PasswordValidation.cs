using System.Text.RegularExpressions;

namespace OnHive.Users.Services.Helpers
{
    public static class PasswordValidation
    {
        public static List<string> Validate(string password, string pattern)
        {
            var result = new List<string>();
            var regex = new Regex(pattern);
            if (!regex.IsMatch(password))
            {
                result.Add("Invalid Password: characters/size");
            }
            result.RemoveAll(string.IsNullOrEmpty);
            return result;
        }
    }
}