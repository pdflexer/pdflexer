namespace PdfLexer.TextLayout;

public enum TextFlowBreakBefore
{
    Auto,
    Always
}

public enum TextFlowBreakAfter
{
    Auto,
    Always
}

public enum TextFlowBreakInside
{
    Auto,
    Avoid
}

public enum TextFragmentBreakReason
{
    None,
    Overflow,
    ForcedBreakBefore,
    ForcedBreakAfter
}

public sealed record TextFragmentBreak(
    TextFragmentBreakReason Reason,
    TextBreakKind BoundaryKind,
    bool IsForced)
{
    public static readonly TextFragmentBreak None = new(TextFragmentBreakReason.None, TextBreakKind.None, false);
}

public sealed record TextFragmentResult<TContent>(
    TextBoxLayoutResult FragmentLayout,
    TContent FragmentContent,
    TContent? RemainderContent,
    TextFragmentBreak Break,
    bool HasRemainder);
