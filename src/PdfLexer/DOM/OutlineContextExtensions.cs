namespace PdfLexer.DOM;

/// <summary>
/// Scoped helpers for building outline trees without positional <see cref="IOutlineContext.Back"/>.
/// </summary>
public static class OutlineContextExtensions
{
    public static IOutlineContext Configure(
        this IOutlineContext context,
        Action<IOutlineContext> configure)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configure);
        configure(context);
        return context;
    }

    public static IOutlineContext AddSection(
        this IOutlineContext context,
        string title,
        Action<IOutlineContext> configure,
        bool isOpen = true,
        double[]? color = null,
        int? style = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configure);
        var child = context.AddSection(title, isOpen, color, style);
        configure(child);
        return child;
    }
}
