using System;
using System.Text;
using PdfLexer.Objects.Lazy;
using PdfLexer.Objects.Parsers;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class XRefTests
    {
        [InlineData("0 310", 0, 310)]
        [InlineData("0 310\r\n", 0, 310)]
        [InlineData("0 310\r", 0, 310)]
        [InlineData("0 310\n", 0 , 310)]
        [InlineData("0 310 ", 0, 310)]
        [InlineData("120  220 ", 120, 220)]
        [Theory]
        public void ItParsesXrefHeader(string header, int first, int second)
        {
            var chars = header.ToCharArray();
            var span = new Span<char>(chars);
            var (f,s) = XRefParser.ParseHeader(span);
            Assert.Equal(f, first);
            Assert.Equal(s, second);
        }

        [InlineData("0 310", 0, 310)]
        [InlineData("0 310\r\n", 0, 310)]
        [InlineData("0 310\r", 0, 310)]
        [InlineData("0 310\n", 0, 310)]
        [InlineData("0 310 ", 0, 310)]
        [InlineData("120  220 ", 120, 220)]
        [Theory]
        public void ItParsesXrefHeaderBytes(string header, uint first, uint second)
        {
            var bytes = Encoding.ASCII.GetBytes(header);
            var span = new Span<byte>(bytes);
            var (f, s) = XRefParser.ParseHeader(span);
            Assert.Equal(f, first);
            Assert.Equal(s, second);
        }

        [InlineData("0000000000 00000 f\n\r", 0, 0, true)]
        [InlineData("0000000001 00001 n\n\r", 1, 1, false)]
        [InlineData("9999999999 65535 n\n\r", 9999999999, 65535, false)]
        [Theory]
        public void ItParsesXrefRecord(string line, UInt64 offset, uint gen, bool isfree)
        {
            var bytes = Encoding.ASCII.GetBytes(line);
            var span = new Span<byte>(bytes);
            var buffer = new Span<char>(new char[10]);
            var result = XRefParser.ParseXrefRecord(span, buffer);
            Assert.Equal(offset, result.Offset);
            Assert.Equal(gen, result.Generation);
            Assert.Equal(isfree, result.IsFree);
        }

        [InlineData(@"0004658443 00000 n 
0004658589 00000 n 
0004658687 00000 n 
0004658785 00000 n 
0004658836 00000 n 
trailer
<</Size 3388/Info 3387 0 R/ID [<5b61ecda9bd3cb8125e4ce579fe6240c><56bf60814128464e7c7da4840439bde2>]/Root 3386 0 R>>
startxref
4658966
%%EOF
", 4658966)]
        [InlineData(@"0004658443 00000 n 
0004658589 00000 n 
0004658687 00000 n 
0004658785 00000 n 
0004658836 00000 n 
trailer
<</Size 3388/Info 3387 0 R/ID [<5b61ecda9bd3cb8125e4ce579fe6240c><56bf60814128464e7c7da4840439bde2>]/Root 3386 0 R>>
startxref
9999999999
%%EOF

", 9999999999)]
        [Theory]
        public void ItGetsXrefOffset(string input, ulong offset)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var result = XRefParser.GetXrefTableOffset(bytes);
            Assert.Equal(offset, result);
        }

        [InlineData(@"xref
0 6
0000000000 65535 f 
0000314102 00000 n 
0000000015 00000 n 
0004653352 00000 n 
0000000130 00000 n 
0000313460 00000 n 
trailer
<</Size 8619/Root 8617 0 R/Info 8618 0 R/ID [<b31927b8053b797df62c3dad88848ff9><b31927b8053b797df62c3dad88848ff9>]>>
", 6, 313460, 5)]
        [InlineData(@"xref
0 6
0000000000 65535 f 
0000314102 00000 n 
0000000015 00000 n 
0004653352 00000 n 
0000000130 00000 n 
0000313460 00000 n 
100 2
0000000135 00000 n 
0000313465 00000 n 
trailer
<</Size 8619/Root 8617 0 R/Info 8618 0 R/ID [<b31927b8053b797df62c3dad88848ff9><b31927b8053b797df62c3dad88848ff9>]>>", 8, 313465, 101)]
        [InlineData(@"xref
0 1
0000000000 65535 f 
1 1
0000314102 00000 n 
2 2
0000000015 00000 n 
0004653352 00000 n 
40 3
0000000130 00000 n 
0000313460 00000 n 
0000313465 00000 n 
trailer
<</Size 8619/Root 8617 0 R/Info 8618 0 R/ID [<b31927b8053b797df62c3dad88848ff9><b31927b8053b797df62c3dad88848ff9>]>>", 7, 313465, 42)]
        [Theory]
        public void ItGetsXRefTable(string input, int entries, ulong lastOffset, uint lastObj)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var results = XRefTableParser.GetEntries(bytes, 0, out PdfLazyDictionary trailer);
            Assert.Equal(entries, results.Count);
            Assert.Equal(lastOffset, results[(int)lastObj].Offset);
            Assert.Equal(lastObj, results[(int)lastObj].ObjectNumber);
        }
    }
}