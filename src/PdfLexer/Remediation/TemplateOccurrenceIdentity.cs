namespace PdfLexer.Remediation;

internal static class TemplateOccurrenceIdentity
{
    internal static string Root(string? templateId, string? version) => templateId is { Length: > 0 }
        ? $"template:{Uri.EscapeDataString(templateId)}@{Uri.EscapeDataString(version ?? string.Empty)}:Document"
        : "template:Document";

    internal static string Append(string parent, string localName, int occurrence) =>
        $"{parent}/{Uri.EscapeDataString(localName)}[{occurrence}]";

    /// <summary>
    /// Appends a compiled-program slot occurrence using its canonical path and declared local
    /// name. The legacy assembly overload intentionally continues to use the string overload.
    /// </summary>
    internal static string Append(string parent, SlotRef slot, string localName, int occurrence)
    {
        ArgumentNullException.ThrowIfNull(slot);
        if (!slot.IsAbsolute || slot.IsRoot)
            throw new ArgumentException("Template occurrence slots must be absolute non-root paths.", nameof(slot));
        if (!SlotRef.IsValidSegment(localName))
            throw new ArgumentException($"Invalid template local name '{localName}'.", nameof(localName));

        var lastSeparator = slot.Path.LastIndexOf('/');
        var canonicalName = slot.Path[(lastSeparator + 1)..];
        if (!string.Equals(canonicalName, localName, StringComparison.Ordinal))
            throw new ArgumentException(
                $"Local name '{localName}' does not match canonical slot '{slot.Path}'.", nameof(localName));
        if (occurrence < 1)
            throw new ArgumentOutOfRangeException(nameof(occurrence));

        return Append(parent, localName, occurrence);
    }

    internal static bool TryParseSlot(string? identity, out SlotRef? slot)
    {
        slot = null;
        if (identity == null || !identity.StartsWith("template:", StringComparison.Ordinal)) return false;
        var document = identity.IndexOf(":Document", StringComparison.Ordinal);
        var start = document >= 0
            ? document + ":Document".Length
            : identity.StartsWith("template:Document", StringComparison.Ordinal)
                ? "template:Document".Length
                : -1;
        if (start < 0 || start == identity.Length) return false;
        var parts = identity[(start + 1)..].Split('/');
        var names = new List<string>();
        foreach (var part in parts)
        {
            var bracket = part.LastIndexOf('[');
            if (bracket <= 0 || !part.EndsWith(']')) return false;
            if (!int.TryParse(part[(bracket + 1)..^1], out var occurrence) || occurrence < 1) return false;
            try
            {
                var name = Uri.UnescapeDataString(part[..bracket]);
                if (!SlotRef.IsValidSegment(name)) return false;
                names.Add(name);
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
        try
        {
            slot = SlotRef.Absolute("/" + string.Join('/', names));
            return true;
        }
        catch (ArgumentException)
        {
            slot = null;
            return false;
        }
    }
}
