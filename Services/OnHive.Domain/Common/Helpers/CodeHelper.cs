namespace EHive.Core.Library.Helpers
{
    public static class CodeHelper
    {
        public static string GenerateAlphanumericCode(int size)
        {
            var result = string.Empty;
            for (var i = 0; i < size; i++)
            {
                var rand = new Random();
                var charValue = rand.Next(65, 91);
                var numberValue = rand.Next(48, 58);
                var type = rand.Next(1, 3);
                if (type == 1)
                {
                    result += (char)numberValue;
                }
                else
                {
                    result += (char)charValue;
                }
            }
            return result;
        }

        public static string GenerateAlphaCode(int size)
        {
            var result = string.Empty;
            for (var i = 0; i < size; i++)
            {
                var rand = new Random();
                var charValue = rand.Next(65, 91);
                result += (char)charValue;
            }
            return result;
        }

        public static string GenerateNumericCode(int size)
        {
            var result = string.Empty;
            for (var i = 0; i < size; i++)
            {
                var rand = new Random();
                var numberValue = rand.Next(48, 58);
                result += (char)numberValue;
            }
            return result;
        }

        public static string NormalizeSlug(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove diacritics (accents)
            var normalized = input.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Convert to lower case
            var result = sb.ToString().ToLowerInvariant();

            // Replace spaces with '-'
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", "-");

            // Remove any remaining non-alphanumeric characters except '-'
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9\-]", "");

            return result;
        }
    }
}