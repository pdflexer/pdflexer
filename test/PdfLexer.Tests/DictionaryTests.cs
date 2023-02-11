using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using Xunit;

namespace PdfLexer.Tests
{
    public class DictionaryTests
    {
        [InlineData("<</Test false>>  ", true, "<</Test false>>")]
        [InlineData("<</Test 1 0 R>>  ", true, "<</Test 1 0 R>>")]
        [InlineData("<</Test 1>>  ", true, "<</Test 1>>")]
        [InlineData("<</Test (string)>>  ", true, "<</Test (string)>>")]
        [InlineData("<</Test <AAAFF>>>  ", true, "<</Test <AAAFF>>>")]
        [InlineData("<</Test 1.0>>  ", true, "<</Test 1.0>>")]
        [InlineData("<</Test false>", false, "")]
        [InlineData("<</Test [false]>>", true, "<</Test [false]>>")]
        [InlineData("<</Test null>>", true, "<</Test null>>")]
        [InlineData("<</Test false/Test2 /Name >>  ", true, "<</Test false/Test2 /Name >>")]
        [Theory]
        public void It_Gets_Dict_Basic(string data, bool found, string expected)
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager, ThrowOnErrors = true });
            ctx.CurrentSource = new InMemoryDataSource(new PdfDocument(), new byte[0]);
            if (!found)
            {
                Assert.ThrowsAny<Exception>(() => ctx.DictionaryParser.Parse(buffer));
            }
            else
            {
                var result = ctx.DictionaryParser.Parse(buffer);
            }

            // Do_Get_Dict(data, found, expected);
        }

        [InlineData("<</Test <</Internal false>>>>  ", true, "<</Test <</Internal false>>>>")]
        [InlineData("<</Test <</Internal false>>", false, "")]
        [InlineData("<</Test <</Internal <</Name /Value>>>>>>", true, "<</Test <</Internal <</Name /Value>>>>>>")]
        [Theory]
        public void It_Gets_Nested_Dict(string data, bool found, string expected)
        {
            // Do_Get_Dict(data, found, expected);
        }


        [InlineData("<</FICL:Enfocus 57 0 R/ViewerPreferences<</Direction/L2R>>/Metadata 2592 0 R/Pages 1 0 R/Type/Catalog/OutputIntents[<</Info(ISO Coated)/S/GTS_PDFX/OutputConditionIdentifier(FOGRA27)/OutputCondition(Offset printing, according to ISO/DIS 12647-2:2003, OFCOM,  paper type 1 or 2 = coated art, 115 g/m2, screen ruling 60 cm-1, positive-acting plates.)/DestOutputProfile 2590 0 R/Type/OutputIntent/RegistryName(http://www.color.org)>>]>>")]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 3 0 R/Contents 2 0 R/Resources<</XObject<</Xf1 1 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 375 0 R/Contents 381 0 R/Resources<</XObject<</Xf123 380 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</Font<</F3 1031 0 R/F2 1032 0 R/F1 1033 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]/XObject<</Xf18 1662 0 R/img17 1663 0 R>>>>/Type/XObject/Filter/FlateDecode/Length 374/Matrix[1 0 0 1 0 0]>>")]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths[0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 1698 0 R/Subtype/TrueType>>")]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 111/Height 20>>")]
        [InlineData("<</BaseFont/USPSBarCode1/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 195 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0]/FontDescriptor 16 0 R/Subtype/TrueType>>")]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 17 0 R/Subtype/TrueType>>")]
        [InlineData("<</BaseFont/CourierNewPS-BoldMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 0 600 0 600 600 600 600 600 600 600 600 600 600 600 0 600 0 0 600 600 600 600 600 600 600 600 600 600 600 600 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600]/FontDescriptor 18 0 R/Subtype/TrueType>>")]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19189 19 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>")]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 115/Height 20>>")]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19186 20 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>")]
        [InlineData("<</Test (string)/Test2 (string2)>>")]
        [InlineData("<</Test <AAAFF>/Test2 <AAAFF>>>")]
        [InlineData("<</Key (Test \\\\(Test) Test )/Key2 (\\216\\217)/Key3 (Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLineTest \\\rNextLineTest \\\rNextLine)>>")]
        [InlineData(@"<</CapHeight 709/Ascent 723/Descent -241/StemV 69/FontBBox[-170 -331 1024 903]/ItalicAngle
0/Flags 6/Style<</Panose<010502020300000000000000>>>/Type/FontDescriptor/FontName/Ryumin-Light>>")]
        [InlineData("<</Length    55/Type/XRef/Root 308 0 R/Info 306 0 R/ID[<6A30A00E31C4B9E6B2A80F691B5E16F3><3441BE26705B2D4D3B19FE454BA2778E>]/Size 547/Prev 116/Index[7 1 306 1 308 1 396 1 540 2 543 4]/W[1 3 1]/DecodeParms<</Columns 5/Predictor 12>>/Filter/FlateDecode>>stream")]
        [Theory]
        public void It_Gets_Real_Dicts(string data)
        {
            var buffer = Encoding.ASCII.GetBytes(data);

            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            ctx.CurrentSource = new InMemoryDataSource(new PdfDocument(), new byte[0]); // to make IR work
            var parser = new DictionaryParser(ctx);
            var dict = parser.Parse(buffer);
            var seq = new ReadOnlySequence<byte>(buffer);
            var dict3 = parser.Parse(seq);
            ctx.Options.Eagerness = Eagerness.Lazy;
            var parser2 = new DictionaryParser(ctx);
            var dict2 = parser2.Parse(buffer);
            var dict4 = parser2.Parse(seq);
            // Do_Get_Dict(data, true, data);
        }

        [InlineData("<</SubData <</Data 1/SubData<</Data1 1/Data2 10 /Data3 [1 10 1 25 100 500 1 2 4 8 9]>>>>>>", 673)]
        [Theory]
        public void It_Gets_Count_From_Dict(string data, int count)
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var parser = new DictionaryParser(new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager }));
            var dict = parser.Parse(buffer);
            Assert.Equal(count, GetCounts(dict.Values));
            var seq = new ReadOnlySequence<byte>(buffer);
            var dic2 = parser.Parse(seq);
            Assert.Equal(count, GetCounts(dic2.Values));
            int GetCounts(IEnumerable<IPdfObject> obj)
            {
                var lc = 0;
                foreach (var item in obj)
                {
                    if (item is PdfDictionary d)
                    {
                        lc += GetCounts(d.Values);
                    }
                    else if (item is PdfArray arr)
                    {
                        lc += GetCounts(arr);
                    }
                    else if (item is PdfIntNumber num)
                    {
                        lc += num.Value;
                    }
                }
                return lc;
            }
        }

        [Fact]
        public void It_Repairs_Nested_Bad_Data()
        {
            var text = @"<<
/Root 1 0 R
/Size 6
/ID [<904e5a162f03815bfbf836e313c7e¤Iž@Á#Ø½÷PÓyq`z÷ëRÎŽZ6äqÎÖ„ìDX€T»‘ƒÆ3õ§™@;#‰ÏÞž ¨Qæ|¨?7lU";
            var data = Encoding.ASCII.GetBytes(text);
            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            ctx.CurrentSource = new InMemoryDataSource(new PdfDocument(), new byte[0]); // to make IR work
            var parser = new DictionaryParser(ctx);
            var dict = parser.Parse(data);
            Assert.Equal(6, (int)dict.GetRequiredValue<PdfNumber>(PdfName.Size));
        }

        [Fact]
        public void It_Repairs_Nested_Bad_Data2()
        {
            var text = @"<<
/Length 10 
/Size <</One/Two
/ID [] >>";
            var data = Encoding.ASCII.GetBytes(text);
            var parser = new DictionaryParser(new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager }));
            var dict = parser.Parse(data);
            Assert.Equal(10, (int)dict.GetRequiredValue<PdfNumber>(PdfName.Length));
        }

        [Fact]
        public void It_Repairs_Imbalanced_Dict()
        {
            var text = @"<<
1 /Two 2
>>";
            var data = Encoding.ASCII.GetBytes(text);
            var parser = new DictionaryParser(new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager }));
            var dict = parser.Parse(data);
            Assert.Equal(2, (int)dict.GetRequiredValue<PdfNumber>(new PdfName("Two")));
        }

        [Fact]
        public void It_Repairs_Dict_With_End_IndirectRef()
        {
            var text = @"
<<
/Pages 2 0 R
";
            var data = Encoding.ASCII.GetBytes(text);
            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            ctx.CurrentSource = new InMemoryDataSource(new PdfDocument(), new byte[0]); // to make IR work
            var parser = new DictionaryParser(ctx);
            var dict = parser.Parse(data);
            var ir = dict[PdfName.Pages] as PdfIndirectRef;
            Assert.Equal(2, ir.Reference.ObjectNumber);
        }


        [InlineData("<</FICL:Enfocus 57 0 R/ViewerPreferences<</Direction/L2R>>/Metadata 2592 0 R/Pages 1 0 R/Type/Catalog/OutputIntents[<</Info(ISO Coated)/S/GTS_PDFX/OutputConditionIdentifier(FOGRA27)/OutputCondition(Offset printing, according to ISO/DIS 12647-2:2003, OFCOM,  paper type 1 or 2 = coated art, 115 g/m2, screen ruling 60 cm-1, positive-acting plates.)/DestOutputProfile 2590 0 R/Type/OutputIntent/RegistryName(http://www.color.org)>>]>>", 42)]
        [Theory]
        public void ScannerTests(string data, int count)
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 30, minimumReadSize: 15));
            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            ctx.CurrentSource = new InMemoryDataSource(new PdfDocument(), new byte[0]); // to make IR work
            var scanner = new PipeScanner(ctx, reader);
            int cnt = 0;
            while (true)
            {
                var token = scanner.Peek();
                if (token == PdfTokenType.EOS)
                {
                    break;
                }
                cnt++;
                scanner.SkipCurrent();
            }
            Assert.Equal(count, cnt);
        }

        [Fact]
        public void Scanner_Skips_Dict()
        {
            var buffer = Encoding.ASCII.GetBytes("[<</Test/Value>>/Name]");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            int cnt = 0;
            while (true)
            {
                var token = scanner.Peek();
                if (token == PdfTokenType.EOS)
                {
                    break;
                }
                if (token == PdfTokenType.DictionaryStart)
                {
                    scanner.SkipObject();
                }
                else
                {
                    cnt++;
                    scanner.SkipCurrent();
                }
            }
            Assert.Equal(3, cnt);
        }

        [Fact]
        public void Scanner_Skips_Array()
        {
            var buffer = Encoding.ASCII.GetBytes("<</Name[/Test/Value/Test/Value/Test/Value/Test/Value]>>");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            int cnt = 0;
            while (true)
            {
                var token = scanner.Peek();
                if (token == PdfTokenType.EOS)
                {
                    break;
                }
                if (token == PdfTokenType.ArrayStart)
                {
                    scanner.SkipObject();
                }
                else
                {
                    cnt++;
                    scanner.SkipCurrent();
                }
            }
            Assert.Equal(3, cnt);
        }

        [Fact]
        public void Scanner_Skips_Multiple_Types()
        {
            var buffer = Encoding.ASCII.GetBytes("/Name[/Test/Value/Test/Value/Test/Value/Test/Value]<<AE>>(string)");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            int cnt = 0;
            var oss = new List<long>();
            while (true)
            {
                var token = scanner.Peek();
                if (token == PdfTokenType.EOS)
                {
                    break;
                }
                scanner.SkipObject();
                oss.Add(scanner.GetStartOffset());
            }
            var expected = new List<long> { 5, 51, 57, 65 };
            Assert.True(expected.SequenceEqual(oss));
        }

        [Fact]
        public void Scanner_Copies_Array()
        {
            var buffer = Encoding.ASCII.GetBytes("/Name[/Test/Value/Test/Value/Test/Value/Test/Value]<<AE>>(string)");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            var mso = new MemoryStream();
            var token = scanner.Peek();
            Assert.Equal(PdfTokenType.NameObj, token);
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.ArrayStart, scanner.Peek());
            var dat = scanner.GetAndSkipCurrentData();
            foreach (var seg in dat)
            {
                mso.Write(seg.Span);
            }
            var copied = Encoding.ASCII.GetString(mso.ToArray());
            Assert.Equal("[/Test/Value/Test/Value/Test/Value/Test/Value]", copied);
            Assert.Equal(PdfTokenType.DictionaryStart, scanner.Peek());
        }

        [Fact]
        public void Scanner_Copies_String()
        {
            var buffer = Encoding.ASCII.GetBytes("/Name(string)/Name");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            var mso = new MemoryStream();
            var token = scanner.Peek();
            Assert.Equal(PdfTokenType.NameObj, token);
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.StringStart, scanner.Peek());
            var dat = scanner.GetAndSkipCurrentData();
            foreach (var seg in dat)
            {
                mso.Write(seg.Span);
            }
            var copied = Encoding.ASCII.GetString(mso.ToArray());
            Assert.Equal("(string)", copied);
            Assert.Equal(PdfTokenType.NameObj, scanner.Peek());
        }
        [Fact]
        public void Scanner_Copies_Dict()
        {
            var buffer = Encoding.ASCII.GetBytes("/Name<</A/B>>(string)");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            var mso = new MemoryStream();
            var token = scanner.Peek();
            Assert.Equal(PdfTokenType.NameObj, token);
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.DictionaryStart, scanner.Peek());
            var dat = scanner.GetAndSkipCurrentData();
            foreach (var seg in dat)
            {
                mso.Write(seg.Span);
            }
            var copied = Encoding.ASCII.GetString(mso.ToArray());
            Assert.Equal("<</A/B>>", copied);
            Assert.Equal(PdfTokenType.StringStart, scanner.Peek());
        }

        [Fact]
        public void Scanner_Parses_Array()
        {
            var buffer = Encoding.ASCII.GetBytes("<</Name[/Test/Value/Test/Value/Test/Value/Test/Value]>>");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            int cnt = 0;
            while (true)
            {
                var token = scanner.Peek();
                if (token == PdfTokenType.EOS)
                {
                    break;
                }
                if (token == PdfTokenType.ArrayStart)
                {
                    var obj = scanner.GetCurrentObject();
                }
                else
                {
                    cnt++;
                    scanner.SkipCurrent();
                }
            }
            Assert.Equal(3, cnt);
        }

        [Fact]
        public void Scanner_Finds_Endstream_After()
        {
            var buffer = Encoding.ASCII.GetBytes("123456789endstream123456789");
            var scanner = new Scanner(new ParsingContext(), buffer, 4);
            var result = scanner.TryFindEndStream();
            Assert.True(result);
            Assert.Equal(9, scanner.Position);
        }

        [Fact]
        public void Scanner_Finds_Endstream_Adjusts_For_NewLine()
        {
            var buffer = Encoding.ASCII.GetBytes("12345678\nendstream123456789");
            var scanner = new Scanner(new ParsingContext(), buffer, 4);
            var result = scanner.TryFindEndStream();
            Assert.True(result);
            Assert.Equal(8, scanner.Position);
        }

        [Fact]
        public void Scanner_Finds_Endstream_After_Multiple()
        {
            var buffer = Encoding.ASCII.GetBytes("123456789endstream123456789endstream");
            var scanner = new Scanner(new ParsingContext(), buffer, 4);
            var result = scanner.TryFindEndStream();
            Assert.True(result);
            Assert.Equal(9, scanner.Position);
        }

        [Fact]
        public void Scanner_Finds_Endstream_Before()
        {
            var buffer = Encoding.ASCII.GetBytes("123456789endstream123456789123456789");
            var scanner = new Scanner(new ParsingContext(), buffer, 22);
            var result = scanner.TryFindEndStream();
            Assert.True(result);
            Assert.Equal(9, scanner.Position);
        }

        [Fact]
        public void Scanner_Finds_Endstream_Before_Closest()
        {
            var buffer = Encoding.ASCII.GetBytes("123456789endstream12345678endstream9123456789");
            var scanner = new Scanner(new ParsingContext(), buffer, 33);
            var result = scanner.TryFindEndStream();
            Assert.True(result);
            Assert.Equal(26, scanner.Position);
        }

        [Fact]
        public void Scanner_Skips_Offset_Returned()
        {
            var buffer = Encoding.ASCII.GetBytes("dadf 9 9 sdf0a /df// af0980 1 0 obj\r\n<</Name[/Test/Value]>>");
            var ms = new MemoryStream(buffer);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 5));
            var scanner = new PipeScanner(new ParsingContext(), reader);
            Assert.True(scanner.TrySkipToToken(Encoding.ASCII.GetBytes("obj"), 10));
            Assert.Equal(32, scanner.GetOffset());
            scanner.ScanBackTokens(2, 10);
            Assert.Equal(27, scanner.GetOffset());
            Assert.Equal(PdfTokenType.NumericObj, scanner.Peek());
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.NumericObj, scanner.Peek());
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.StartObj, scanner.Peek());
        }
    }
}