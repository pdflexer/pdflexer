using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.Fonts;
using PdfLexer.Writing;

namespace PdfLexer.DOM;

public sealed class LinkAnnotation
{
    internal LinkAnnotation(
        PdfPage page,
        PdfDictionary annotation,
        StructureNode? structureDestinationTarget = null,
        PdfArray? structureDestinationTemplate = null)
    {
        Page = page;
        NativeObject = annotation;
        StructureDestinationTarget = structureDestinationTarget;
        StructureDestinationTemplate = structureDestinationTemplate;
    }

    public PdfPage Page { get; }

    public PdfDictionary NativeObject { get; }

    internal StructureNode? StructureDestinationTarget { get; }

    internal PdfArray? StructureDestinationTemplate { get; }
}

public sealed class WidgetAnnotation
{
    internal WidgetAnnotation(PdfDocument document, PdfPage page, PdfDictionary annotation, PdfDictionary field)
    {
        Document = document;
        Page = page;
        NativeObject = annotation;
        Field = field;
    }

    public PdfDocument Document { get; }

    public PdfPage Page { get; }

    public PdfDictionary NativeObject { get; }

    public PdfDictionary Field { get; }
}

public sealed class FormFieldAppearanceOptions
{
    public required IWritableFont Font { get; init; }
    public double FontSize { get; init; } = 12;
    public bool NeedAppearances { get; init; }
}

public enum ChoiceFieldKind
{
    Combo,
    List
}

public sealed record RadioButtonOption(PdfPage Page, PdfRect<double> Rect, string Value, bool Selected = false, string? Label = null);

public sealed class RadioGroupAnnotation
{
    internal RadioGroupAnnotation(PdfDictionary field, IReadOnlyList<WidgetAnnotation> widgets)
    {
        Field = field;
        Widgets = widgets;
    }

    public PdfDictionary Field { get; }
    public IReadOnlyList<WidgetAnnotation> Widgets { get; }
}

public static class AnnotationFactory
{
    public static LinkAnnotation CreateLink(PdfPage page, PdfRect<double> rect, IPdfObject destination, string? contents = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        annotation[PdfName.Dest] = destination;
        page.AddAnnotation(annotation);
        return new LinkAnnotation(page, annotation);
    }

    public static LinkAnnotation CreateStructureLink(
        PdfPage page,
        PdfRect<double> rect,
        StructureNode destination,
        string contents,
        PdfArray? destinationTemplate = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        page.AddAnnotation(annotation);
        return new LinkAnnotation(
            page,
            annotation,
            destination,
            destinationTemplate ?? new PdfArray { PdfNull.Value, PdfName.Fit });
    }

    public static LinkAnnotation CreateLinkAction(PdfPage page, PdfRect<double> rect, PdfDictionary action, string? contents = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        annotation[PdfName.A] = action;
        page.AddAnnotation(annotation);
        return new LinkAnnotation(page, annotation);
    }

    public static WidgetAnnotation CreateTextWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        string? tooltip = null,
        bool print = true)
    {
        document.ValidateLegacyWidgetFactory();
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Widget);
        annotation[PdfName.FT] = PdfName.Tx;
        if (print)
        {
            annotation[PdfName.F] = new PdfIntNumber(4);
        }

        var field = new PdfDictionary
        {
            [PdfName.FT] = PdfName.Tx,
            [PdfName.T] = PdfString.CreateTextString(fieldName),
            [PdfName.Kids] = new PdfArray { annotation.Indirect() }
        };

        if (!string.IsNullOrEmpty(tooltip))
        {
            field[(PdfName)"TU"] = PdfString.CreateTextString(tooltip);
            annotation[(PdfName)"TU"] = PdfString.CreateTextString(tooltip);
        }

        annotation[PdfName.Parent] = field.Indirect();
        page.AddAnnotation(annotation);

        var acroForm = document.Catalog.GetOrCreateValue<PdfDictionary>((PdfName)"AcroForm");
        var fields = acroForm.GetOrCreateValue<PdfArray>((PdfName)"Fields");
        fields.Add(field.Indirect());

        return new WidgetAnnotation(document, page, annotation, field);
    }

    public static WidgetAnnotation CreateTextWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        FormFieldAppearanceOptions appearance,
        string? value = null,
        string? tooltip = null,
        bool print = true)
    {
        var result = CreateWidget(document, page, rect, fieldName, PdfName.Tx, appearance, tooltip, print);
        if (value != null)
        {
            result.Field[PdfName.V] = PdfString.CreateTextString(value);
            result.NativeObject[PdfName.V] = PdfString.CreateTextString(value);
        }
        result.NativeObject[(PdfName)"AP"] = new PdfDictionary
        {
            [PdfName.N] = CreateTextAppearance(rect, appearance, value ?? string.Empty).NativeObject.Indirect()
        };
        return result;
    }

    public static WidgetAnnotation CreateCheckboxWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        FormFieldAppearanceOptions appearance,
        bool isChecked = false,
        string onValue = "Yes",
        string? tooltip = null,
        bool print = true)
    {
        if (string.IsNullOrWhiteSpace(onValue) || onValue == "Off")
            throw new ArgumentException("The checkbox on-state must be non-empty and different from 'Off'.", nameof(onValue));
        var result = CreateWidget(document, page, rect, fieldName, PdfName.Btn, appearance, tooltip, print);
        var state = (PdfName)(isChecked ? onValue : "Off");
        result.Field[PdfName.V] = state;
        result.NativeObject[(PdfName)"AS"] = state;
        result.NativeObject[(PdfName)"AP"] = new PdfDictionary
        {
            [PdfName.N] = new PdfDictionary
            {
                [(PdfName)"Off"] = CreateButtonAppearance(rect, false, false).NativeObject.Indirect(),
                [(PdfName)onValue] = CreateButtonAppearance(rect, true, false).NativeObject.Indirect()
            }
        };
        return result;
    }

    public static RadioGroupAnnotation CreateRadioGroup(
        PdfDocument document,
        string fieldName,
        IEnumerable<RadioButtonOption> options,
        FormFieldAppearanceOptions appearance,
        string? tooltip = null,
        bool print = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateAppearance(document, appearance);
        var optionList = options.ToList();
        if (optionList.Count == 0) throw new ArgumentException("At least one radio option is required.", nameof(options));
        if (optionList.Select(x => x.Value).Any(string.IsNullOrWhiteSpace) ||
            optionList.Select(x => x.Value).Distinct(StringComparer.Ordinal).Count() != optionList.Count)
            throw new ArgumentException("Radio option values must be non-empty and unique.", nameof(options));
        if (optionList.Count(x => x.Selected) > 1)
            throw new ArgumentException("Only one radio option may be selected.", nameof(options));
        if (!print && document.IsStrictAccessibility)
        {
            throw new PdfAccessibilityConformanceException("Widget annotations require the Print flag.");
        }

        var field = CreateField(document, fieldName, PdfName.Btn, appearance, tooltip);
        field[(PdfName)"Ff"] = new PdfIntNumber(1 << 15);
        var kids = new PdfArray();
        field[PdfName.Kids] = kids;
        var selected = optionList.FirstOrDefault(x => x.Selected)?.Value ?? "Off";
        field[PdfName.V] = (PdfName)selected;
        var widgets = new List<WidgetAnnotation>();
        foreach (var option in optionList)
        {
            var annotation = CreateBaseAnnotation(option.Page, option.Rect, PdfName.Widget);
            if (print) annotation[PdfName.F] = new PdfIntNumber(4);
            annotation[PdfName.Parent] = field.Indirect();
            annotation[(PdfName)"AS"] = (PdfName)(option.Selected ? option.Value : "Off");
            var optionTooltip = option.Label ?? tooltip;
            ApplyTooltip(annotation, null, optionTooltip);
            annotation[(PdfName)"AP"] = new PdfDictionary
            {
                [PdfName.N] = new PdfDictionary
                {
                    [(PdfName)"Off"] = CreateButtonAppearance(option.Rect, false, true).NativeObject.Indirect(),
                    [(PdfName)option.Value] = CreateButtonAppearance(option.Rect, true, true).NativeObject.Indirect()
                }
            };
            kids.Add(annotation.Indirect());
            option.Page.AddAnnotation(annotation);
            widgets.Add(new WidgetAnnotation(document, option.Page, annotation, field));
        }
        return new RadioGroupAnnotation(field, widgets);
    }

    public static WidgetAnnotation CreateChoiceWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        IEnumerable<string> choices,
        ChoiceFieldKind kind,
        FormFieldAppearanceOptions appearance,
        string? selectedValue = null,
        string? tooltip = null,
        bool print = true)
    {
        ArgumentNullException.ThrowIfNull(choices);
        var values = choices.ToList();
        if (values.Count == 0 || values.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Choice fields require at least one non-empty option.", nameof(choices));
        if (selectedValue != null && !values.Contains(selectedValue, StringComparer.Ordinal))
            throw new ArgumentException("The selected value must be one of the supplied choices.", nameof(selectedValue));
        var result = CreateWidget(document, page, rect, fieldName, (PdfName)"Ch", appearance, tooltip, print);
        result.Field[(PdfName)"Opt"] = new PdfArray(values.Select(x => (IPdfObject)PdfString.CreateTextString(x)).ToList());
        if (kind == ChoiceFieldKind.Combo) result.Field[(PdfName)"Ff"] = new PdfIntNumber(1 << 17);
        if (selectedValue != null) result.Field[PdfName.V] = PdfString.CreateTextString(selectedValue);
        result.NativeObject[(PdfName)"AP"] = new PdfDictionary
        {
            [PdfName.N] = CreateTextAppearance(rect, appearance, selectedValue ?? string.Empty).NativeObject.Indirect()
        };
        return result;
    }

    public static WidgetAnnotation CreatePushButtonWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        string caption,
        FormFieldAppearanceOptions appearance,
        string? tooltip = null,
        bool print = true)
    {
        var result = CreateWidget(document, page, rect, fieldName, PdfName.Btn, appearance, tooltip, print);
        result.Field[(PdfName)"Ff"] = new PdfIntNumber(1 << 16);
        result.NativeObject[(PdfName)"MK"] = new PdfDictionary
        {
            [(PdfName)"CA"] = PdfString.CreateTextString(caption)
        };
        result.NativeObject[(PdfName)"AP"] = new PdfDictionary
        {
            [PdfName.N] = CreateTextAppearance(rect, appearance, caption).NativeObject.Indirect()
        };
        return result;
    }

    private static WidgetAnnotation CreateWidget(
        PdfDocument document, PdfPage page, PdfRect<double> rect, string fieldName,
        PdfName fieldType, FormFieldAppearanceOptions appearance, string? tooltip, bool print)
    {
        if (!print && document.IsStrictAccessibility)
        {
            throw new PdfAccessibilityConformanceException("Widget annotations require the Print flag.");
        }
        ValidateAppearance(document, appearance);
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Widget);
        if (print) annotation[PdfName.F] = new PdfIntNumber(4);
        var field = CreateField(document, fieldName, fieldType, appearance, tooltip);
        field[PdfName.Kids] = new PdfArray { annotation.Indirect() };
        annotation[PdfName.Parent] = field.Indirect();
        ApplyTooltip(annotation, field, tooltip);
        page.AddAnnotation(annotation);
        return new WidgetAnnotation(document, page, annotation, field);
    }

    private static PdfDictionary CreateField(
        PdfDocument document, string fieldName, PdfName fieldType,
        FormFieldAppearanceOptions appearance, string? tooltip)
    {
        var acroForm = document.Catalog.GetOrCreateValue<PdfDictionary>((PdfName)"AcroForm");
        var fields = acroForm.GetOrCreateValue<PdfArray>((PdfName)"Fields");
        var fontName = RegisterFont(acroForm, appearance.Font);
        var da = $"/{fontName.Value} {appearance.FontSize:0.###} Tf 0 g";
        if (!acroForm.ContainsKey((PdfName)"DA")) acroForm[(PdfName)"DA"] = new PdfString(da);
        if (appearance.NeedAppearances) acroForm[(PdfName)"NeedAppearances"] = PdfBoolean.True;
        var field = new PdfDictionary
        {
            [PdfName.FT] = fieldType,
            [PdfName.T] = PdfString.CreateTextString(fieldName),
            [(PdfName)"DA"] = new PdfString(da)
        };
        ApplyTooltip(null, field, tooltip);
        fields.Add(field.Indirect());
        return field;
    }

    private static PdfName RegisterFont(PdfDictionary acroForm, IWritableFont writable)
    {
        var fonts = acroForm.GetOrCreateValue<PdfDictionary>((PdfName)"DR")
            .GetOrCreateValue<PdfDictionary>(PdfName.Font);
        var font = writable.GetPdfFont();
        foreach (var entry in fonts)
        {
            if (ReferenceEquals(entry.Value.Resolve(), font)) return entry.Key;
        }
        var index = 1;
        PdfName name;
        do name = (PdfName)$"F{index++}"; while (fonts.ContainsKey(name));
        fonts[name] = font.Indirect();
        return name;
    }

    private static void ValidateAppearance(PdfDocument document, FormFieldAppearanceOptions appearance)
    {
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(appearance.Font);
        if (appearance.FontSize <= 0) throw new ArgumentOutOfRangeException(nameof(appearance), "Font size must be positive.");
        document.ValidateAppearanceFont(appearance.Font);
    }

    private static void ApplyTooltip(PdfDictionary? annotation, PdfDictionary? field, string? tooltip)
    {
        if (string.IsNullOrWhiteSpace(tooltip)) return;
        if (field != null) field[(PdfName)"TU"] = PdfString.CreateTextString(tooltip);
        if (annotation != null)
        {
            annotation[(PdfName)"TU"] = PdfString.CreateTextString(tooltip);
            annotation[PdfName.Contents] = PdfString.CreateTextString(tooltip);
        }
    }

    private static XObjForm CreateTextAppearance(PdfRect<double> rect, FormFieldAppearanceOptions options, string text)
    {
        var width = Math.Max(1, rect.Width());
        var height = Math.Max(1, rect.Height());
        var writer = new FormWriter(width, height);
        writer.Save();
        WriteFieldChrome(writer, width, height);

        // ISO 32000-1 12.7.3.3: variable text is enclosed in /Tx BMC ... EMC and clipped to the field.
        writer.MarkedContent(new MarkedContent("Tx"));
        writer.Save();
        writer.Rect(1, 1, Math.Max(0.1, width - 2), Math.Max(0.1, height - 2)).Clip(evenOdd: false).EndPathNoOp();
        writer.SetFillRGB(0, 0, 0).Font(options.Font, options.FontSize)
            .BeginText().TextMove(3, Math.Max(2, (height - options.FontSize) / 2)).Text(text).EndText();
        writer.Restore();
        writer.EndMarkedContent();

        writer.Restore();
        return writer.Complete();
    }

    private static XObjForm CreateButtonAppearance(PdfRect<double> rect, bool on, bool radio)
    {
        var width = Math.Max(1, rect.Width());
        var height = Math.Max(1, rect.Height());
        var writer = new FormWriter(width, height);
        writer.Save();
        WriteFieldChrome(writer, width, height);
        // No /Tx wrapper here: button appearances are not variable text.
        if (on)
        {
            var inset = Math.Max(2, Math.Min(width, height) / (radio ? 4 : 5));
            writer.SetFillRGB(0, 0, 0).Rect(inset, inset, Math.Max(1, width - 2 * inset), Math.Max(1, height - 2 * inset)).Fill();
        }
        writer.Restore();
        return writer.Complete();
    }

    /// <summary>
    /// Draws the default field background and border. /MK border and background colours are
    /// not honoured; see the known limitations in docs/accessibility-authoring.md.
    /// </summary>
    private static void WriteFieldChrome(FormWriter writer, double width, double height)
    {
        writer.SetFillRGB(255, 255, 255).Rect(0, 0, width, height).Fill();
        writer.SetStrokingRGB(0, 0, 0).LineWidth(1).Rect(.5, .5, width - 1, height - 1).Stroke();
    }

    private static PdfDictionary CreateBaseAnnotation(PdfPage page, PdfRect<double> rect, PdfName subtype, string? contents = null)
    {
        var annotation = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = subtype,
            [PdfName.P] = page.NativeObject.Indirect(),
            [PdfName.Rect] = PdfRectangle.FromContentModel(rect).NativeObject
        };

        if (!string.IsNullOrWhiteSpace(contents))
        {
            annotation[PdfName.Contents] = PdfString.CreateTextString(contents);
        }

        return annotation;
    }
}
