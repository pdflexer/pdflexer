using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using System.IO;
using System.Text;
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

        [Fact]
        public void It_Dedups_In_Memory_Refs()
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

            // var copied1 = writer.AddItem(dictionary);

            var ms = new MemoryStream();
            var wtx = new WritingContext(ms);
            var ref1 = PdfIndirectRef.Create(dictionary);
            var ref2 = PdfIndirectRef.Create(dictionary);
            wtx.WriteIndirectObject(ref1);
            wtx.WriteIndirectObject(ref2);

            //var str = Encoding.ASCII.GetString(ms.ToArray());
            Assert.Equal(2, wtx.writtenObjs.Count);
        }

        [Fact]
        public void It_Dedups_External_Refs()
        {
            var ctx = new ParsingContext();
            ctx.SourceId = 100;
            var writer = new WritableInMemorySource(ctx);

            var dictionary = new PdfDictionary();
            var dictionary2 = new PdfDictionary();
            var ref1 = new ExistingIndirectRef(ctx, 1, 0);
            ref1.Object = dictionary;
            var ref2 = new ExistingIndirectRef(ctx, 2, 0);
            ref2.Object = dictionary2;

            dictionary.Add("/Ref",  ref2);
            dictionary.Add("/Value", new PdfIntNumber(1));
            dictionary2.Add("/Ref", ref1);
            dictionary2.Add("/Value", new PdfIntNumber(2));

            // var copied1 = writer.AddItem(dictionary);

            var ms = new MemoryStream();
            var wtx = new WritingContext(ms);
            wtx.WriteIndirectObject(ref1);
            wtx.WriteIndirectObject(ref2);

            var str = Encoding.ASCII.GetString(ms.ToArray());
            Assert.Equal(2, wtx.writtenObjs.Count);
        }

        [Fact]
        public void It_Dedups_New_To_Existing_Refs()
        {
            var ctx = new ParsingContext();
            ctx.SourceId = 100;
            var writer = new WritableInMemorySource(ctx);

            var dictionary = new PdfDictionary();
            var dictionary2 = new PdfDictionary();
            var ref1 = new ExistingIndirectRef(ctx, 1, 0);
            ref1.Object = dictionary;
            var ref2 = new ExistingIndirectRef(ctx, 2, 0);
            ref2.Object = dictionary2;

            dictionary.Add("/Ref",  ref2);
            dictionary.Add("/Value", new PdfIntNumber(1));
            dictionary2.Add("/Ref", ref1);
            dictionary2.Add("/Value", new PdfIntNumber(2));

            // var copied1 = writer.AddItem(dictionary);

            var ms = new MemoryStream();
            var wtx = new WritingContext(ms);
            wtx.WriteIndirectObject(PdfIndirectRef.Create(dictionary));
            wtx.WriteIndirectObject(ref2);

            var str = Encoding.ASCII.GetString(ms.ToArray());
            Assert.Equal(2, wtx.writtenObjs.Count);
        }
    }
}
