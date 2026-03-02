using EHive.Core.Library.Abstractions.Validation;
using System.ComponentModel.DataAnnotations;

namespace EHive.Core.Library.Attributes
{
    public class ValidValues : ValidationAttribute
    {
        private readonly List<string> values;
        private readonly StringComparison comparison;
        private string currentValue = string.Empty;

        public ValidValues(params string[] values)
        {
            this.values = values.ToList();
            comparison = StringComparison.InvariantCultureIgnoreCase;
        }

        public ValidValues(StringComparison comparison, params string[] values)
        {
            this.values = values.ToList();
            this.comparison = comparison;
        }

        public ValidValues(Type data)
        {
            if (Activator.CreateInstance(data) is not IStringValidationData dataValues)
            {
                throw new ArgumentException(nameof(data));
            }
            values = dataValues?.Values.ToList() ?? new();
            comparison = StringComparison.InvariantCultureIgnoreCase;
        }

        public ValidValues(StringComparison comparison, Type data)
        {
            if (Activator.CreateInstance(data) is not IStringValidationData dataValues)
            {
                throw new ArgumentException(nameof(data));
            }
            values = dataValues?.Values.ToList() ?? new();
            this.comparison = comparison;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Invalid value for {name} ({currentValue}), allowed values {string.Join(", ", values) }";
        }

        public override bool IsValid(object? value)
        {
            currentValue = value as string ?? string.Empty;
            return value != null && values.Any(v => v.Equals((string)value, comparison));
        }
    }
}