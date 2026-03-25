using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AssoInternesBrest.API.Utils
{
    public static partial class SlugGenerator
    {
        public static string Generate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // passer en minuscules
            var lower = input.ToLowerInvariant();

            // enlever les accents
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            var noAccents = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // enlever caractères spéciaux
            var cleaned = SpecialCharsRegex().Replace(noAccents, "");

            // remplacer espaces par tirets
            var hyphenated = SpacesRegex().Replace(cleaned, "-");

            // enlever tirets multiples
            var collapsed = MultiHyphenRegex().Replace(hyphenated, "-");

            // enlever tirets début/fin
            return collapsed.Trim('-');
        }

        [GeneratedRegex(@"[^a-z0-9\s-]")]
        private static partial Regex SpecialCharsRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex SpacesRegex();

        [GeneratedRegex(@"-+")]
        private static partial Regex MultiHyphenRegex();
    }
}