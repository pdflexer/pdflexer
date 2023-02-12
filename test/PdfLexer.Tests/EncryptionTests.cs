using PdfLexer.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class EncryptionTests
{
    [Fact]
    public void It_Handles_AES_128_Streams()
    {
        RunSingle("passwords_aes_128.pdf", true, false);
        RunSingle("passwords_aes_128.pdf", false, true);
        RunSingle("passwords_aes_128.pdf", true, true);
    }

    [Fact]
    public void It_Handles_AES_256_Streams()
    {
        RunSingle("passwords_aes_256.pdf", true, false);
        RunSingle("passwords_aes_256.pdf", false, true);
        RunSingle("passwords_aes_256.pdf", true, true);
    }

    [Fact]
    public void It_Handles_AES_256_Hardened_Streams()
    {
        RunSingle("passwords_aes_256_hardened.pdf", false, true);
        RunSingle("passwords_aes_256_hardened.pdf", true, false);
        RunSingle("passwords_aes_256_hardened.pdf", true, true);
    }

    [Fact]
    public void It_Handles_RC4_rev2_Streams()
    {
        RunSingle("passwords_rc4_rev2.pdf", true, false);
        RunSingle("passwords_rc4_rev2.pdf", false, true);
        RunSingle("passwords_rc4_rev2.pdf", true, true);
    }

    [Fact]
    public void It_Handles_RC4_rev3_Streams()
    {
        RunSingle("passwords_rc4_rev3.pdf", true, false);
        RunSingle("passwords_rc4_rev3.pdf", false, true);
        RunSingle("passwords_rc4_rev3.pdf", true, true);
    }

    [Fact]
    public void It_Handles_String()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "issue9972-1.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));

        var pg = doc.Pages.First();

        var words = SimpleWordReader.GetWords(doc.Context, pg);
        Assert.Contains("Personal", words);
    }


    [Fact]
    public void It_Handles_Writing_From_Encrypted()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfrs");
        var pdf = Path.Combine(pdfRoot, "passwords_aes_256_hardened.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), new ParsingOptions
        {
            OwnerPass = "ownerpassword",
            UserPass = "userpassword"
        });

        var pg = doc.Pages.First();

        using var doc2 = PdfDocument.Create();
        doc2.Pages.AddRange(doc.Pages);
        var result = doc2.Save();

        using var d3 = PdfDocument.Open(result);

        var words = SimpleWordReader.GetWords(d3.Context, d3.Pages.First());
        Assert.Contains("Hello", words);
        Assert.Contains("World!", words);
    }

    [Fact]
    public void It_handles_pr6531()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfjs");
        var pdf = Path.Combine(pdfRoot, "pr6531_2.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), new DocumentOptions
        {
            UserPass = "asdfasdf"
        });

        var pg = doc.Pages.First();
        var annot = pg.NativeObject.Get<PdfArray>(PdfName.Annots).First().GetAs<PdfDictionary>().Get<PdfString>(PdfName.Contents);
        Assert.Equal("Bluebeam should be encrypting this.", annot.Value);
    }

    private void RunSingle(string name, bool owner, bool user)
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdfRoot = Path.Combine(tp, "pdfs", "pdfrs");
        var pdf = Path.Combine(pdfRoot, name);
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf), new DocumentOptions
        {
            OwnerPass = owner ? "ownerpassword" : null,
            UserPass = user ? "userpassword" : null
        });

        var pg = doc.Pages.First();

        var words = SimpleWordReader.GetWords(doc.Context, pg);
        Assert.Contains("Hello", words);
        Assert.Contains("World!", words);
    }
}
