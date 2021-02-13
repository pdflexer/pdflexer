using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.Objects.Parsers;
using Xunit;

namespace PdfLexer.Objects.Tests
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


        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 3 0 R/Contents 2 0 R/Resources<</XObject<</Xf1 1 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 375 0 R/Contents 381 0 R/Resources<</XObject<</Xf123 380 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</Font<</F3 1031 0 R/F2 1032 0 R/F1 1033 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]/XObject<</Xf18 1662 0 R/img17 1663 0 R>>>>/Type/XObject/Filter/FlateDecode/Length 374/Matrix[1 0 0 1 0 0]>>")]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths[0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 1698 0 R/Subtype/TrueType>>")]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 111/Height 20>>")]
        [Theory]
        public void It_Gets_Real_Dicts(string data)
        {
            // Do_Get_Dict(data, true, data);
        }

        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 3 0 R/Contents 2 0 R/Resources<</XObject<</Xf1 1 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 375 0 R/Contents 381 0 R/Resources<</XObject<</Xf123 380 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>")]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</Font<</F3 1031 0 R/F2 1032 0 R/F1 1033 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]/XObject<</Xf18 1662 0 R/img17 1663 0 R>>>>/Type/XObject/Filter/FlateDecode/Length 374/Matrix[1 0 0 1 0 0]>>")]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths[0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 1698 0 R/Subtype/TrueType>>")]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 111/Height 20>>")]
        [Theory]
        public void It_Skips_Real_Dicts(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            reader.AdvancePast((byte) '<');
            var result = reader.AdvanceToDictEnd();
            Assert.True(result);
            Assert.Equal(bytes.Length, reader.Consumed);
        }

        [InlineData("[/PDF /Text /ImageB /ImageC <</Test <</Internal false>>>>]")]
        [Theory]
        public void It_Skips_Array(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            reader.AdvancePast((byte) '[');
            var result = reader.AdvanceToArrayEnd();
            Assert.True(result);
            Assert.Equal(bytes.Length, reader.Consumed);
        }

        [InlineData(
            "<</Type/Page/MediaBox[0 0 612 792]/Parent 3 0 R/Contents 2 0 R/Resources<</XObject<</Xf1 1 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>",
            5, 10)]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 375 0 R/Contents 381 0 R/Resources<</XObject<</Xf123 380 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>",
            5, 10)]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</Font<</F3 1031 0 R/F2 1032 0 R/F1 1033 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]/XObject<</Xf18 1662 0 R/img17 1663 0 R>>>>/Type/XObject/Filter/FlateDecode/Length 374/Matrix[1 0 0 1 0 0]>>", 
            5, 10)]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths[0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 1698 0 R/Subtype/TrueType>>",
            8, 263)]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 111/Height 20>>",
            5, 10)]
        [Theory]
        public async Task It_Parses_Real_Dicts(string data, int top, int total)
        {
            await ParseDict(data, top, total);
        }

        [InlineData(
            "<</Type/Page/MediaBox[0 0 612 792]/Parent 3 0 R/Contents 2 0 R/Resources<</XObject<</Xf1 1 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>",
            5, 10)]
        [InlineData("<</Type/Page/MediaBox[0 0 612 792]/Parent 375 0 R/Contents 381 0 R/Resources<</XObject<</Xf123 380 0 R>>/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]>>>>",
            5, 10)]
        [InlineData("<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</Font<</F3 1031 0 R/F2 1032 0 R/F1 1033 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]/XObject<</Xf18 1662 0 R/img17 1663 0 R>>>>/Type/XObject/Filter/FlateDecode/Length 374/Matrix[1 0 0 1 0 0]>>", 
            8, 10)]
        [InlineData("<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths[0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 1698 0 R/Subtype/TrueType>>",
            8, 263)]
        [InlineData("<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 111/Height 20>>",
            9, 10)]
        [Theory]
        public async Task It_Lazy_Parses_Real_Dicts(string data, int top, int total)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var source = new InMemoryDataSource(bytes);
            var ms = new MemoryStream(bytes);
            var pipe = PipeReader.Create(ms);

            var result = await pipe.ReadAsync();
            var parser = new LazyNestedParser(source);
            while (parser.ParseNestedItem(result.Buffer, result.IsCompleted))
            { }
            var value = parser.GetItem();
            var dict = value as PdfDictionary;
            Assert.NotNull(dict);
            Assert.Equal(top, dict.Count);
            Assert.False(dict.IsModified);
            dict.IsModified = true;
            var rs = new MemoryStream();
            value.WriteObject(rs);
            var br = rs.ToArray();
            var text = Encoding.ASCII.GetString(br);
            Assert.Equal(data, text);
        }

        
        [Fact]
        public void It_Tokenizes_Sub_Dicts()
        {
            var data = "<</Type/Page/MediaBox[0 0 612 792]>>";
            var bytes = Encoding.ASCII.GetBytes(data);
            var source = new InMemoryDataSource(bytes);
            var nested = new NestedTokenizer(source, 0);
            var lengths = new List<int>() { 5, 5, 9, 13 };
            var types = new List<PdfObjectType>() {PdfObjectType.NameObj, PdfObjectType.NameObj, PdfObjectType.NameObj, PdfObjectType.ArrayObj };
            int i = 0;
            while (nested.Read())
            {
                var type = nested.ObjectType;
                var info = nested.GetCurrentInfo();
                Assert.Equal(types[i], type);
                Assert.Equal(lengths[i], info.Length);
                i++;
            }
        }

        public async Task ParseDict(string data, int mainCount, int totalCount)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var source = new InMemoryDataSource(bytes);
            var ms = new MemoryStream(bytes);
            var pipe = PipeReader.Create(ms);

            var result = await pipe.ReadAsync();
            var parser = new NestedParser(source);
            while (parser.TryParseDict(result.Buffer, result.IsCompleted))
            { }
            var value = parser.GetValue();
            var dict = value as PdfDictionary;
            Assert.NotNull(dict);
            Assert.Equal(mainCount, dict.Count);
            Assert.Equal(totalCount, CountItems(dict));


            int CountItems(IParsedPdfObject obj)
            {
                int count = 0;
                if (obj is PdfDictionary dict)
                {
                    foreach (var item in dict)
                    {
                        count++;
                        if (item.Value.Type == PdfObjectType.DictionaryObj || item.Value.Type == PdfObjectType.ArrayObj)
                        {
                            count += CountItems(item.Value.Parsed);
                        }
                    }
                } else if (obj is PdfArray arr)
                {
                    foreach (var item in arr)
                    {
                        count++;
                        if (item.Type == PdfObjectType.DictionaryObj || item.Type == PdfObjectType.ArrayObj)
                        {
                            count += CountItems(item.Parsed);
                        }
                    }
                }

                return count;
            }

        }

        private void Do_Tokenize_Dict(string data, bool found, string expected)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            var source = new InMemoryDataSource(bytes);
            var nested = new NestedTokenizer(source, 0);
            while (nested.Read())
            {

            }

            var info = nested.GetTotalInfo();
            Assert.Equal(0, info.StartPos);
            Assert.Equal(data.Length, info.Length);
        }
    }
}