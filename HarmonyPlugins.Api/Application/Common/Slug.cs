using System.Globalization;
using System.Text;

namespace HarmonyPlugins.Api.Application.Common;

public static class Slug
{
    // "Economia & Lojas" -> "economia-lojas"
    public static string From(string text)
    {
        var builder = new StringBuilder();
        foreach (var c in text.Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsAsciiLetterOrDigit(c))
                builder.Append(c);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().TrimEnd('-');
    }
}
