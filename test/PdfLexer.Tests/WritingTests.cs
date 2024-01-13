using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests;

public class WritingTests
{
    [Fact]
    public void It_PrePends_Data()
    {
        using var doc = PdfDocument.Create();
        var pg = doc.AddPage();
        {
            using var writer = pg.GetWriter();
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .Text("Hello World")
                .Restore();
        }

        var first = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(first));

        {
            using var writer = pg.GetWriter(Writing.PageWriteMode.Pre);
            writer
                .Save()
                .Font(Writing.Base14.TimesRoman, 12)
                .Text("New")
                .Restore();
        }

        var second = doc.Save();
        Assert.Empty(SyntaxValidation.Validate(second));
    }
}
