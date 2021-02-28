using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using Xunit;

namespace PdfLexer.Tests
{
    public class DictionaryTests
    {
        [InlineData("<</Test false>>  ", true,  "<</Test false>>")]
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
            var ctx = new ParsingContext();
            if (!found)
            {
                Assert.ThrowsAny<Exception>(() => ctx.DictionaryParser.Parse(buffer));
            } else
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
        [Theory]
        public void It_Gets_Real_Dicts(string data)
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var parser = new DictionaryParser(new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager }));
            var dict = parser.Parse(buffer);
            var seq = new ReadOnlySequence<byte>(buffer);
            var dict3 = parser.Parse(seq);
            var parser2 = new DictionaryParser(new ParsingContext(new ParsingOptions { Eagerness = Eagerness.Lazy }));
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
                    } else if (item is PdfArray arr)
                    {
                        lc += GetCounts(arr);
                    } else if (item is PdfIntNumber num)
                    {
                        lc += num.Value;
                    }
                }
                return lc;
            }
        }
    }
}