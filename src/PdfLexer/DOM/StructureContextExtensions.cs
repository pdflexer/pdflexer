namespace PdfLexer.DOM;

/// <summary>
/// Scoped helpers for building logical structure trees without positional <see cref="IStructureContext.Back"/>.
/// </summary>
public static class StructureContextExtensions
{
    public static IStructureContext Configure(
        this IStructureContext context,
        Action<IStructureContext> configure)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configure);
        configure(context);
        return context;
    }

    public static IStructureContext AddElement(
        this IStructureContext context,
        string type,
        Action<IStructureContext> configure,
        string? title = null,
        string? id = null,
        string? language = null) =>
        AddScoped(context, configure, parent => parent.AddElement(type, title, id, language));

    public static IStructureContext AddElement(
        this IStructureContext context,
        string type,
        string? title,
        Action<IStructureContext> configure,
        string? id = null,
        string? language = null) =>
        AddScoped(context, configure, parent => parent.AddElement(type, title, id, language));

    public static IStructureContext AddPart(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddPart());

    public static IStructureContext AddPart(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddPart(title));

    public static IStructureContext AddSection(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddSection());

    public static IStructureContext AddSection(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddSection(title));

    public static IStructureContext AddDiv(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddDiv());

    public static IStructureContext AddDiv(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddDiv(title));

    public static IStructureContext AddBlockQuote(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddBlockQuote());

    public static IStructureContext AddBlockQuote(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddBlockQuote(title));

    public static IStructureContext AddCaption(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddCaption());

    public static IStructureContext AddCaption(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddCaption(title));

    public static IStructureContext AddTOC(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTOC());

    public static IStructureContext AddTOC(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTOC(title));

    public static IStructureContext AddTOCI(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTOCI());

    public static IStructureContext AddTOCI(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTOCI(title));

    public static IStructureContext AddParagraph(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddParagraph());

    public static IStructureContext AddParagraph(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddParagraph(title));

    public static IStructureContext AddSpan(
        this IStructureContext context,
        Action<IStructureContext> configure,
        string? lang = null) =>
        AddScoped(context, configure, parent => parent.AddSpan(lang: lang));

    public static IStructureContext AddSpan(
        this IStructureContext context,
        string? title,
        Action<IStructureContext> configure,
        string? lang = null) =>
        AddScoped(context, configure, parent => parent.AddSpan(title, lang));

    public static IStructureContext AddList(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddList());

    public static IStructureContext AddList(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddList(title));

    public static IStructureContext AddListItem(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddListItem());

    public static IStructureContext AddListItem(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddListItem(title));

    public static IStructureContext AddLabel(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddLabel());

    public static IStructureContext AddLabel(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddLabel(title));

    public static IStructureContext AddListBody(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddListBody());

    public static IStructureContext AddListBody(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddListBody(title));

    public static IStructureContext AddTable(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTable());

    public static IStructureContext AddTable(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTable(title));

    public static IStructureContext AddTableHead(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableHead());

    public static IStructureContext AddTableHead(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableHead(title));

    public static IStructureContext AddTableBody(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableBody());

    public static IStructureContext AddTableBody(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableBody(title));

    public static IStructureContext AddTableFoot(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableFoot());

    public static IStructureContext AddTableFoot(this IStructureContext context, string? title, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddTableFoot(title));

    public static IStructureContext AddRow(this IStructureContext context, Action<IStructureContext> configure) =>
        AddScoped(context, configure, parent => parent.AddRow());

    public static IStructureContext AddHeaderCell(
        this IStructureContext context,
        Action<IStructureContext> configure,
        int rowSpan = 1,
        int colSpan = 1) =>
        AddScoped(context, configure, parent => parent.AddHeaderCell(rowSpan, colSpan));

    public static IStructureContext AddDataCell(
        this IStructureContext context,
        Action<IStructureContext> configure,
        int rowSpan = 1,
        int colSpan = 1) =>
        AddScoped(context, configure, parent => parent.AddDataCell(rowSpan, colSpan));

    private static IStructureContext AddScoped(
        IStructureContext context,
        Action<IStructureContext> configure,
        Func<IStructureContext, IStructureContext> add)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configure);
        var child = add(context);
        configure(child);
        return child;
    }
}
