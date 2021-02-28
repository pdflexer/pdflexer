using PdfLexer.IO;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PdfLexer.Tests
{
    public class WriterTests
    {
        [Fact]
        public void It_Copies_Cyclical_Refs()
        {
            var ctx = new ParsingContext();
            ctx.SourceId = 100;
            var writer = new WritableInMemorySource(ctx);

            var dictionary = new PdfDictionary();
            var dictionary2 = new PdfDictionary();

            dictionary.Add("/Ref",  PdfIndirectRef.Create(dictionary2));
            dictionary.Add("/Value", new PdfIntNumber(1));
            dictionary2.Add("/Ref", PdfIndirectRef.Create(dictionary));
            dictionary2.Add("/Value", new PdfIntNumber(2));

            var copied1 = writer.AddItem(dictionary).GetValue<PdfDictionary>();
            var val1 = copied1.GetRequiredValue<PdfNumber>("/Value");
            Assert.Equal(1, val1);
            var val2 = copied1.GetRequiredValue<PdfDictionary>("/Ref").GetRequiredValue<PdfNumber>("/Value");
            Assert.Equal(2, val2);
        }
    }
}
