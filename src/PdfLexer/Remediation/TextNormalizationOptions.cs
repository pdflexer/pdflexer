using System.Globalization;
using System.Text;

namespace PdfLexer.Remediation;

/// <summary>Controls normalization applied to text predicate operands.</summary>
public sealed record TextNormalizationOptions
{
    /// <summary>The default normalization policy for remediation rules.</summary>
    public static TextNormalizationOptions Default { get; } = new();

    /// <summary>A policy that preserves extracted text exactly.</summary>
    public static TextNormalizationOptions None { get; } = new()
    {
        CompatibilityComposition = false,
        RemoveSoftHyphens = false,
        NormalizeWhitespace = false,
        FoldDashes = false,
        FoldQuotes = false
    };

    public bool CompatibilityComposition { get; init; } = true;
    public bool RemoveSoftHyphens { get; init; } = true;
    public bool NormalizeWhitespace { get; init; } = true;
    public bool FoldDashes { get; init; } = true;
    public bool FoldQuotes { get; init; } = true;

    /// <summary>Normalizes a string without changing its source character ranges.</summary>
    public string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var input = CompatibilityComposition ? value.Normalize(NormalizationForm.FormKC) : value;
        var result = new StringBuilder(input.Length);
        var pendingSpace = false;

        foreach (var rune in input.EnumerateRunes())
        {
            if (RemoveSoftHyphens && rune.Value == 0x00AD)
            {
                continue;
            }

            if (NormalizeWhitespace && IsUnicodeWhitespace(rune))
            {
                pendingSpace = result.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                result.Append(' ');
                pendingSpace = false;
            }

            if (FoldDashes && IsFoldedDash(rune.Value))
            {
                result.Append('-');
            }
            else if (FoldQuotes && rune.Value is >= 0x2018 and <= 0x201B)
            {
                result.Append('\'');
            }
            else if (FoldQuotes && rune.Value is >= 0x201C and <= 0x201F)
            {
                result.Append('"');
            }
            else
            {
                result.Append(rune.ToString());
            }
        }

        return result.ToString();
    }

    private static bool IsUnicodeWhitespace(Rune rune) =>
        Rune.IsWhiteSpace(rune) ||
        Rune.GetUnicodeCategory(rune) is UnicodeCategory.SpaceSeparator or
            UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator;

    private static bool IsFoldedDash(int value) =>
        value is >= 0x2010 and <= 0x2015 or 0x2212 or 0xFE58 or 0xFE63 or 0xFF0D;
}
