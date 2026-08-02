using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using PdfLexer.Writing;
using PdfLexer.Operators;

namespace PdfLexer.Tests;

internal static class RemediationFixtureGenerator
{
    internal static PdfDocument CreateInvoiceInput() =>
        CreateInvoiceInput(CreateEmbeddedFont());

    internal static PdfDocument CreateStrictInvoiceInput()
        => CreateInvoiceInput(CreateEmbeddedFont());

    private static PdfDocument CreateInvoiceInput(IWritableFont font)
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Invoice 10042", 18);
        WriteLine(writer, font, 40, 720, "Invoice #");
        WriteLine(writer, font, 150, 720, "INV-10042");
        writer.Save()
            .Font(font, 12)
            .WordSpacing(180)
            .TextMove(90, 680)
            .Text("Item Qty Amount")
            .Restore();
        WriteLine(writer, font, 90, 658, "Widget");
        WriteLine(writer, font, 300, 658, "2");
        WriteLine(writer, font, 500, 658, "10.00");
        WriteLine(writer, font, 90, 636, "Service");
        WriteLine(writer, font, 300, 636, "1");
        WriteLine(writer, font, 500, 636, "90.00");
        WriteLine(writer, font, 40, 600, "Subtotal");
        WriteLine(writer, font, 150, 600, "100.00");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateStatementInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Account Statement", 18);
        WriteLine(writer, font, 40, 710, "Bill To");
        WriteLine(writer, font, 40, 688, "Ada Lovelace");
        WriteLine(writer, font, 40, 666, "123 Analytical Engine Way");
        WriteLine(writer, font, 40, 644, "London");
        WriteLine(writer, font, 40, 610, "Ship To");
        WriteLine(writer, font, 40, 588, "Same as billing");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateReportInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Quarterly Report", 18);
        WriteLine(writer, font, 40, 712, "Overview");
        WriteLine(writer, font, 40, 690, "Revenue increased across all regions.");
        WriteLine(writer, font, 40, 660, "Optional Notes");
        WriteLine(writer, font, 40, 638, "No remediation exceptions were observed.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateFormInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Registration Form", 18);
        WriteLine(writer, font, 40, 710, "Full name");
        WriteLine(writer, font, 180, 710, "Ada Lovelace");
        WriteLine(writer, font, 40, 680, "Email");
        WriteLine(writer, font, 180, 680, "ada@example.com");
        WriteLine(writer, font, 40, 640, "I agree to receive notices.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateMultiColumnInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Policy Update", 18);
        WriteLine(writer, font, 40, 710, "Sidebar");
        WriteLine(writer, font, 40, 688, "Important dates");
        WriteLine(writer, font, 220, 710, "Main Column");
        WriteLine(writer, font, 220, 688, "The updated policy applies next quarter.");
        WriteLine(writer, font, 220, 666, "Review the summary before filing.");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateMixedPageSizeInput()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        var letter = doc.AddPage(PageSize.LETTER);
        using (var writer = letter.GetWriter())
        {
            WriteLine(writer, font, 40, 750, "Mixed Size Notice", 18);
            WriteLine(writer, font, 40, 710, "Letter page content");
            WriteLine(writer, font, 520, 24, "Page 1");
        }

        var a4 = doc.AddPage(PageSize.A4);
        using (var writer = a4.GetWriter())
        {
            WriteLine(writer, font, 40, 790, "Continuation", 18);
            WriteLine(writer, font, 40, 750, "A4 page content");
            WriteLine(writer, font, 500, 24, "Page 2");
        }

        return doc;
    }

    private static IWritableFont CreateEmbeddedFont()
    {
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(testDir, "Roboto-Regular.ttf");
        return TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));
    }

    private static IWritableFont CreateUnicodeEmbeddedFont()
    {
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(testDir, "Roboto-Regular.ttf");
        return TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
    }

    private static void WriteLine(
        ContentWriter<double> writer,
        IWritableFont font,
        double x,
        double y,
        string text,
        double size = 12)
    {
        writer.Save().Font(font, size).TextMove(x, y).Text(text).Restore();
    }

    // C-01: Unicode and normalization
    internal static PdfDocument CreateC01Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateUnicodeEmbeddedFont();
        using var writer = page.GetWriter();

        // Each construct is paired with its normalized equivalent.
        WriteLine(writer, font, 40, 770, "\uFB01 \uFB02");
        WriteLine(writer, font, 40, 750, "fi fl");

        WriteLine(writer, font, 40, 720, "multi\u00ADpart");
        WriteLine(writer, font, 40, 700, "multi-part");

        WriteLine(writer, font, 40, 670, "word\u00A0gap");
        WriteLine(writer, font, 40, 630, "word gap");

        WriteLine(writer, font, 40, 600, "INV\u201110042");
        WriteLine(writer, font, 40, 580, "INV-10042");

        WriteLine(writer, font, 40, 550, "\u201Cquote\u201D \u2018single\u2019");
        WriteLine(writer, font, 40, 530, "\"quote\" 'single'");

        WriteLine(writer, font, 40, 500, "cafe\u0301");
        WriteLine(writer, font, 40, 480, "caf\u00E9");

        WriteLine(writer, font, 40, 450, "three   spaces");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    /// <summary>
    /// The same document written in plain ASCII. Normalization affects matching only, so this must
    /// remediate to identical structure and identical MCID assignment.
    /// </summary>
    internal static PdfDocument CreateC01AsciiInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateUnicodeEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 770, "fi fl");
        WriteLine(writer, font, 40, 750, "fi fl");

        WriteLine(writer, font, 40, 720, "multi-part");
        WriteLine(writer, font, 40, 700, "multi-part");

        WriteLine(writer, font, 40, 670, "word gap");
        WriteLine(writer, font, 40, 630, "word gap");

        WriteLine(writer, font, 40, 600, "INV-10042");
        WriteLine(writer, font, 40, 580, "INV-10042");

        WriteLine(writer, font, 40, 550, "\"quote\" 'single'");
        WriteLine(writer, font, 40, 530, "\"quote\" 'single'");

        // Precomposed on both lines: "é" has no ASCII spelling, so the twin drops the decomposed
        // sequence rather than the character.
        WriteLine(writer, font, 40, 500, "café");
        WriteLine(writer, font, 40, 480, "café");

        WriteLine(writer, font, 40, 450, "three spaces");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC02Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. One Tj
        WriteLine(writer, font, 40, 750, "Invoice #: INV-12345");

        // 2. TJ array with word gap adjustments (no space char)
        writer.Save().Font(font, 12).TextMove(40, 720);
        writer.Op(new TJ_Op(new List<TJ_Item<double>>
        {
            // Word gaps spelled as glyph adjustments with no space character — PDF/UA 7.2. Kept out of
            // the "Invoice" line rule's reach so the words can be asserted directly.
            new() { Data = Encode(font, "Statement") },
            new() { Shift = -700 },
            new() { Data = Encode(font, "#:") },
            new() { Shift = -700 },
            new() { Data = Encode(font, "STM-12345") }
        }));
        writer.Restore();

        // 3. Three separate Tj operators split at word boundaries
        writer.Save().Font(font, 12).TextMove(40, 690).Text("Invoice").Restore();
        writer.Save().Font(font, 12).TextMove(90, 690).Text("#:").Restore();
        writer.Save().Font(font, 12).TextMove(110, 690).Text("INV-12345").Restore();

        // 4. One word split mid-token across two Tj operators
        writer.Save().Font(font, 12).TextMove(40, 660).Text("Invoice #: INV-").Restore();
        writer.Save().Font(font, 12).TextMove(130, 660).Text("12345").Restore();

        // 5. Line claimed by P rule
        WriteLine(writer, font, 40, 630, "Subtotal Total");

        // 6. TJ with adjustments for two words
        writer.Save().Font(font, 12).TextMove(40, 600);
        writer.Op(new TJ_Op(new List<TJ_Item<double>>
        {
            new() { Data = Encode(font, "Amount") },
            new() { Shift = -500 },
            new() { Data = Encode(font, "Paid") }
        }));
        writer.Restore();

        // Lines 7-10 with non-default text state (Tw, Tc, Tz, Ts)
        writer.Save().Font(font, 12).WordSpacing(2).TextMove(40, 570).Text("Spacing Test Line").Restore();
        writer.Save().Font(font, 12).Op(new Tc_Op(1.5)).TextMove(40, 540).Text("Char Spacing Line").Restore();
        writer.Save().Font(font, 12).Op(new Tz_Op(120)).TextMove(40, 510).Text("Horizontal Scale Line").Restore();
        writer.Save().Font(font, 12).Op(new Ts_Op(3)).TextMove(40, 480).Text("Text Rise Line").Restore();

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    private static byte[] Encode(IWritableFont font, string text)
    {
        var encoded = new List<byte>();
        var buffer = new byte[4];
        foreach (var character in font.ConvertFromUnicode(text, 0, text.Length, buffer))
        {
            encoded.AddRange(buffer.Take(character.ByteCount));
        }
        return encoded.ToArray();
    }

    // C-03: Graphical content
    internal static PdfDocument CreateC03Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. Logo image top-left (100x40)
        var imgStream = new PdfStream();
        imgStream.Dictionary[PdfName.Subtype] = PdfName.Image;
        imgStream.Dictionary[PdfName.Width] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.Height] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.ColorSpace] = PdfName.DeviceGray;
        imgStream.Dictionary[PdfName.BitsPerComponent] = new PdfIntNumber(8);
        imgStream.Contents = new PdfByteArrayStreamContents(new byte[100]);
        var logoImg = new XObjImage(imgStream);
        writer.Image(logoImg, 40, 700, 100, 40);

        // 2. Second image placed in footer
        writer.Image(logoImg, 40, 24, 50, 16);

        // 3. Table ruling lines (horizontal and vertical)
        writer.Save().SetStrokingRGB(0, 0, 0).LineWidth(1)
            .MoveTo(40, 500).LineTo(550, 500).Stroke()
            .MoveTo(40, 500).LineTo(40, 400).Stroke()
            .Restore();

        // 4. Filled background rectangle behind header band
        writer.Save().SetFillRGB(230, 230, 230).Rect(40, 650, 510, 30).Fill().Restore();

        // 5. Clip-only path (W n)
        writer.Save().Rect(40, 550, 200, 50).Clip().EndPathNoOp().Restore();

        // 6. Form XObject placed twice
        var formWriter = new FormWriter(100, 40);
        formWriter.Rect(0, 0, 100, 40).Stroke();
        var repeatedForm = formWriter.Complete();
        writer.Form(repeatedForm, 200, 700);
        writer.Form(repeatedForm, 350, 700);

        // 7. Shading fill
        var shadingDict = new PdfDictionary
        {
            [new PdfName("ShadingType")] = new PdfIntNumber(2),
            [PdfName.ColorSpace] = PdfName.DeviceGray,
            [new PdfName("Coords")] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(0), new PdfIntNumber(100), new PdfIntNumber(0) },
            [new PdfName("Extend")] = new PdfArray { PdfBoolean.True, PdfBoolean.True },
            [new PdfName("Function")] = new PdfDictionary
            {
                [new PdfName("FunctionType")] = new PdfIntNumber(2),
                [new PdfName("Domain")] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(1) },
                [new PdfName("C0")] = new PdfArray { new PdfDoubleNumber(0.9) },
                [new PdfName("C1")] = new PdfArray { new PdfDoubleNumber(1.0) },
                [new PdfName("N")] = new PdfIntNumber(1)
            }
        };
        writer.Shading(shadingDict);

        // 8. Ordinary body text
        writer.Save().SetFillRGB(0, 0, 0).Font(font, 12).TextMove(40, 450).Text("Graphical Page Body Text").Restore();
        writer.Save().SetFillRGB(0, 0, 0).Font(font, 12).TextMove(520, 24).Text("Page 1").Restore();

        return doc;
    }

    internal static PdfDocument CreateC08Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        var page1 = doc.AddPage(PageSize.LETTER);
        var page2 = doc.AddPage(PageSize.LETTER);
        using (var writer = page1.GetWriter())
        {
            WriteLine(writer, font, 72, 700, "Go to details");
        }
        using (var writer = page2.GetWriter())
        {
            WriteLine(writer, font, 72, 700, "Visit example");
            WriteLine(writer, font, 72, 620, "Details target");
        }

        var internalLink = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 70, 695, 160, 715 },
            [PdfName.Contents] = PdfString.CreateTextString("Go to details"),
            [PdfName.Border] = new PdfArray { 0, 0, 0 },
            [PdfName.Dest] = new PdfArray { page2.NativeObject.Indirect(), PdfName.Fit }
        };
        page1.AddAnnotation(internalLink);

        var uriLink = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 70, 695, 160, 715 },
            [PdfName.Contents] = PdfString.CreateTextString("Visit example"),
            [PdfName.Border] = new PdfArray { 0, 0, 0 },
            [PdfName.A] = new PdfDictionary
            {
                [PdfName.S] = (PdfName)"URI",
                [(PdfName)"URI"] = PdfString.CreateTextString("https://example.com")
            }
        };
        page2.AddAnnotation(uriLink);

        var stamp = ExistingAnnotation(page1, "Stamp", new PdfRect<double>(420, 680, 500, 720), "Approved");
        page1.AddAnnotation(stamp);
        var popup = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Popup",
            [PdfName.Rect] = new PdfArray { 500, 650, 580, 720 },
            [PdfName.F] = new PdfIntNumber(2),
            [(PdfName)"Parent"] = stamp.Indirect()
        };
        page1.AddAnnotation(popup);

        var attachment = ExistingAnnotation(page2, "FileAttachment", new PdfRect<double>(420, 600, 450, 630), "Invoice attachment");
        var embedded = new PdfStream();
        embedded.Dictionary[PdfName.TypeName] = PdfName.EmbeddedFile;
        embedded.Contents = new PdfByteArrayStreamContents(Encoding.UTF8.GetBytes("attachment"));
        attachment[(PdfName)"FS"] = new PdfDictionary
        {
            [PdfName.TypeName] = (PdfName)"Filespec",
            [PdfName.F] = new PdfString("attachment.txt"),
            [(PdfName)"UF"] = PdfString.CreateTextString("attachment.txt"),
            [(PdfName)"AFRelationship"] = (PdfName)"Data",
            [PdfName.EF] = new PdfDictionary { [PdfName.F] = embedded.Indirect() }
        };
        page2.AddAnnotation(attachment);
        return doc;
    }

    private static PdfDictionary ExistingAnnotation(
        PdfPage page,
        string subtype,
        PdfRect<double> bounds,
        string contents)
    {
        var appearanceWriter = new FormWriter(bounds.Width(), bounds.Height());
        appearanceWriter.Rect(0, 0, bounds.Width(), bounds.Height()).Stroke();
        var appearance = appearanceWriter.Complete();
        return new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)subtype,
            [PdfName.Rect] = PdfRectangle.FromContentModel(bounds).NativeObject,
            [PdfName.Contents] = PdfString.CreateTextString(contents),
            [(PdfName)"AP"] = new PdfDictionary { [PdfName.N] = appearance.NativeObject.Indirect() }
        };
    }

    internal static PdfDocument CreateC10Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageNumber = 1; pageNumber <= 4; pageNumber++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            WriteLine(writer, font, 72, 755, "Acme Corp - Confidential");
            WriteLine(writer, font, 72, 680, $"Body page {pageNumber}");
            WriteLine(writer, font, 250, 390, "DRAFT", 28);
            WriteLine(writer, font, 270, 24, $"Page {pageNumber} of 4");
        }
        return doc;
    }

    internal static PdfDocument CreateC04Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Claimed Line 1");
        WriteLine(writer, font, 40, 720, "Unclaimed Line 2");
        WriteLine(writer, font, 40, 690, "Unclaimed Line 3");

        // Unclaimed decorative image
        var imgStream = new PdfStream();
        imgStream.Dictionary[PdfName.Subtype] = PdfName.Image;
        imgStream.Dictionary[PdfName.Width] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.Height] = new PdfIntNumber(10);
        imgStream.Dictionary[PdfName.ColorSpace] = PdfName.DeviceGray;
        imgStream.Dictionary[PdfName.BitsPerComponent] = new PdfIntNumber(8);
        imgStream.Contents = new PdfByteArrayStreamContents(new byte[100]);
        writer.Image(new XObjImage(imgStream), 40, 600, 50, 50);

        // Unclaimed path
        writer.Save().SetStrokingRGB(0, 0, 0).LineWidth(1).MoveTo(40, 550).LineTo(200, 550).Stroke().Restore();

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC05Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Invoice Statement", 18);
        WriteLine(writer, font, 40, 700, "Invoice Total: $500.00");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC06aInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        WriteLine(writer, font, 40, 750, "Account Statement", 18);
        WriteLine(writer, font, 40, 710, "Bill To");
        WriteLine(writer, font, 40, 688, "Ada Lovelace");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC06bInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // Label changed to "Statement of Account" and Bill To shifted right by 30pt
        WriteLine(writer, font, 40, 750, "Statement of Account", 18);
        WriteLine(writer, font, 70, 710, "Bill To");
        WriteLine(writer, font, 70, 688, "Ada Lovelace");
        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC12Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageIndex = 0; pageIndex < 5; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            if (pageIndex == 0)
            {
                WriteLine(writer, font, 40, 760, "Continued Table Fixture", 18);
            }

            // Header row repeats on every table page; body rows continue across all four.
            if (pageIndex < 4)
            {
                WriteLine(writer, font, 260, 720, "Item");
                WriteLine(writer, font, 420, 720, "Amount");
                WriteLine(writer, font, 260, 690, $"Widget{pageIndex + 1}");
                WriteLine(writer, font, 420, 690, $"{(pageIndex + 1) * 10}.00");
                WriteLine(writer, font, 260, 660, $"Gadget{pageIndex + 1}");
                WriteLine(writer, font, 420, 660, $"{(pageIndex + 1) * 5}.00");
            }

            // The terms section spans pages 4-5, starting below the subtotal that ends the table's
            // flow. Flow regions may not overlap, so a second cross-page flow has to begin where the
            // first one ends rather than running beside it.
            if (pageIndex == 3)
            {
                WriteLine(writer, font, 260, 600, "Total");
                WriteLine(writer, font, 420, 600, "150.00");
                WriteLine(writer, font, 40, 520, "Terms and Conditions");
                WriteLine(writer, font, 40, 490, "Payment is due within thirty days.");
            }
            if (pageIndex == 4)
            {
                WriteLine(writer, font, 40, 740, "Late fees apply after sixty days.");
                WriteLine(writer, font, 40, 710, "End of Terms");
            }

            WriteLine(writer, font, 520, 24, $"Page {pageIndex + 1} of 5");
        }

        return doc;
    }

    internal static PdfDocument CreateC23Input()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();

        // 1. Separate operators, value to the right
        WriteLine(writer, font, 40, 750, "Invoice #");
        WriteLine(writer, font, 150, 750, "INV-10042");

        // 2. Single operator: label and value together
        WriteLine(writer, font, 40, 720, "Invoice #: INV-10042");

        // 3. Label above, value below
        WriteLine(writer, font, 40, 690, "Due Date");
        WriteLine(writer, font, 40, 670, "2026-01-15");

        // 4. Label with value 2 columns right, unrelated field between
        WriteLine(writer, font, 40, 640, "PO Number");
        WriteLine(writer, font, 180, 640, "Status: Active");
        WriteLine(writer, font, 350, 640, "PO-9988");

        // 5. Empty value label
        WriteLine(writer, font, 40, 610, "Notes:");
        WriteLine(writer, font, 40, 580, "Next Section Header");

        // 6. Right-aligned value in totals block
        WriteLine(writer, font, 40, 550, "Total Amount");
        WriteLine(writer, font, 500, 550, "$150.00");

        WriteLine(writer, font, 520, 24, "Page 1");
        return doc;
    }

    internal static PdfDocument CreateC29aInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "1.");
        WriteLine(writer, font, 80, 720, "Outer item body");
        WriteLine(writer, font, 100, 680, "a.");
        WriteLine(writer, font, 140, 650, "Inner item body");
        return doc;
    }

    internal static PdfDocument CreateC29bInput()
    {
        var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var font = CreateEmbeddedFont();
        using var writer = page.GetWriter();
        WriteLine(writer, font, 40, 750, "Section Alpha", 18);
        WriteLine(writer, font, 40, 720, "Alpha introduction");
        WriteLine(writer, font, 60, 680, "Alpha detail", 14);
        WriteLine(writer, font, 60, 650, "Alpha detail body");
        WriteLine(writer, font, 40, 520, "Section Beta", 18);
        WriteLine(writer, font, 40, 490, "Beta introduction");
        WriteLine(writer, font, 60, 450, "Beta detail", 14);
        WriteLine(writer, font, 60, 420, "Beta detail body");
        return doc;
    }

    internal static PdfDocument CreateC30Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();
        for (var pageIndex = 0; pageIndex < 2; pageIndex++)
        {
            var page = doc.AddPage(PageSize.LETTER);
            using var writer = page.GetWriter();
            WriteLine(writer, font, 40, 740, $"Template section {pageIndex + 1}");
        }
        return doc;
    }

    internal static PdfDocument CreateC24Input()
    {
        var doc = PdfDocument.Create();
        var font = CreateEmbeddedFont();

        // Page 1
        var page1 = doc.AddPage(PageSize.LETTER);
        using (var writer1 = page1.GetWriter())
        {
            WriteLine(writer1, font, 40, 750, "Header Date 2026-01-01", 16);
            WriteLine(writer1, font, 40, 600, "Table Date");
            WriteLine(writer1, font, 150, 600, "Table Amount");
            WriteLine(writer1, font, 40, 500, "Footer Date 2026-12-31");
            WriteLine(writer1, font, 40, 450, "Totals Amount");
            WriteLine(writer1, font, 520, 24, "Page 1");
        }

        // Page 2
        var page2 = doc.AddPage(PageSize.LETTER);
        using (var writer2 = page2.GetWriter())
        {
            WriteLine(writer2, font, 40, 750, "Header Date 2026-01-01", 16);
            WriteLine(writer2, font, 40, 500, "Footer Date 2026-12-31");
            WriteLine(writer2, font, 520, 24, "Page 2");
        }

        return doc;
    }

}
