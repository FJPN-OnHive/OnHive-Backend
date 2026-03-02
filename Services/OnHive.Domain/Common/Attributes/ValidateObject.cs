using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace EHive.Core.Library.Attributes
{
    public class ValidateObject : ValidationAttribute
    {
        private List<ValidationResult> results = new();
        private readonly bool allowNull = false;

        public ValidateObject(bool allowNull = false)
        {
            this.allowNull = allowNull;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Invalid value for {name} - {string.Join(", ", results.Select(r => r.ErrorMessage))}";
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return allowNull;
            results = new List<ValidationResult>();

            if (value is not ICollection)
            {
                var context = new ValidationContext(value);
                Validator.TryValidateObject(value, context, results, true);
            }
            else
            {
                var resultList = new List<ValidationResult>();
                foreach (var item in (ICollection)value)
                {
                    var context = new ValidationContext(item);
                    if (!Validator.TryValidateObject(item, context, resultList, true))
                    {
                        results.AddRange(resultList);
                    }
                }
            }
            return !results.Any();
        }
    }
}