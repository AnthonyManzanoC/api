using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LamillaEscudero.Application.Common;

public static partial class StringExtensions
{
    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
    private static partial Regex InvalidSlugCharsRegex();

    [GeneratedRegex(@"[\s-]+", RegexOptions.Compiled)]
    private static partial Regex MultiSeparatorRegex();

    public static string ToSlug(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var withoutAccents = builder.ToString().Normalize(NormalizationForm.FormC);
        var withoutSpecialChars = InvalidSlugCharsRegex().Replace(withoutAccents, " ");
        return MultiSeparatorRegex().Replace(withoutSpecialChars, "-").Trim('-');
    }
}
