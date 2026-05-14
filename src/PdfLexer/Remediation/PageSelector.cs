using System.Collections.ObjectModel;

namespace PdfLexer.Remediation;

/// <summary>
/// Declarative selector for pages that a rule should evaluate against.
/// </summary>
public abstract record PageSelector
{
    /// <summary>Selects the first page.</summary>
    public static PageSelector First { get; } = new FirstPageSelector();

    /// <summary>Selects the last page.</summary>
    public static PageSelector Last { get; } = new LastPageSelector();

    /// <summary>Selects every page.</summary>
    public static PageSelector Every { get; } = new EveryPageSelector();

    /// <summary>Selects a zero-based inclusive page range.</summary>
    public static PageSelector Range(int fromInclusive, int toInclusive) => new PageRangeSelector(fromInclusive, toInclusive);

    /// <summary>Selects odd or even one-based page numbers.</summary>
    public static PageSelector Parity(PageParity parity) => new PageParitySelector(parity);

    /// <summary>Debug representation used in reports.</summary>
    public abstract string DebugString { get; }

    /// <summary>Returns whether the zero-based page index is included.</summary>
    public abstract bool Includes(int zeroBasedPageIndex, int pageCount);

    /// <summary>Returns the selected zero-based page indexes for a document page count.</summary>
    public IReadOnlyList<int> SelectPages(int pageCount)
    {
        if (pageCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageCount));
        }

        var pages = new List<int>();
        for (var i = 0; i < pageCount; i++)
        {
            if (Includes(i, pageCount))
            {
                pages.Add(i);
            }
        }

        return new ReadOnlyCollection<int>(pages);
    }
}

/// <summary>
/// One-based page parity.
/// </summary>
public enum PageParity
{
    /// <summary>Odd one-based pages.</summary>
    Odd,
    /// <summary>Even one-based pages.</summary>
    Even
}

/// <summary>Selects the first page.</summary>
public sealed record FirstPageSelector : PageSelector
{
    public override string DebugString => "Pages.First";

    public override bool Includes(int zeroBasedPageIndex, int pageCount) => pageCount > 0 && zeroBasedPageIndex == 0;
}

/// <summary>Selects the last page.</summary>
public sealed record LastPageSelector : PageSelector
{
    public override string DebugString => "Pages.Last";

    public override bool Includes(int zeroBasedPageIndex, int pageCount) => pageCount > 0 && zeroBasedPageIndex == pageCount - 1;
}

/// <summary>Selects all pages.</summary>
public sealed record EveryPageSelector : PageSelector
{
    public override string DebugString => "Pages.Every";

    public override bool Includes(int zeroBasedPageIndex, int pageCount) => zeroBasedPageIndex >= 0 && zeroBasedPageIndex < pageCount;
}

/// <summary>Selects a zero-based inclusive page range.</summary>
public sealed record PageRangeSelector : PageSelector
{
    /// <summary>Creates a page-range selector.</summary>
    public PageRangeSelector(int fromInclusive, int toInclusive)
    {
        if (fromInclusive < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fromInclusive));
        }

        if (toInclusive < fromInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(toInclusive), "Range end must be greater than or equal to range start.");
        }

        FromInclusive = fromInclusive;
        ToInclusive = toInclusive;
    }

    /// <summary>Zero-based first selected page.</summary>
    public int FromInclusive { get; }

    /// <summary>Zero-based last selected page.</summary>
    public int ToInclusive { get; }

    public override string DebugString => $"Pages.Range({FromInclusive},{ToInclusive})";

    public override bool Includes(int zeroBasedPageIndex, int pageCount) =>
        zeroBasedPageIndex >= FromInclusive &&
        zeroBasedPageIndex <= ToInclusive &&
        zeroBasedPageIndex < pageCount;
}

/// <summary>Selects pages by one-based parity.</summary>
public sealed record PageParitySelector(PageParity PageParity) : PageSelector
{
    public override string DebugString => $"Pages.Parity({PageParity})";

    public override bool Includes(int zeroBasedPageIndex, int pageCount)
    {
        if (zeroBasedPageIndex < 0 || zeroBasedPageIndex >= pageCount)
        {
            return false;
        }

        var oneBased = zeroBasedPageIndex + 1;
        return PageParity == PageParity.Odd ? oneBased % 2 == 1 : oneBased % 2 == 0;
    }
}
