using System.ComponentModel.DataAnnotations;

namespace OnHive.Core.Library.Validations.Common
{
    public static class ValidateExtension
    {
        public static bool Validate(this object input, out List<string> result)
        {
            result = new List<string>();
            var context = new ValidationContext(input);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(input, context, results, true))
            {
                result.AddRange(results.Select(r => r.ErrorMessage ?? string.Join(",", r.MemberNames)));
            }
            return !result.Any();
        }
    }
}