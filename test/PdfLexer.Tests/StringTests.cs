using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class StringTests
    {
        [InlineData("(Test)", "Test")]
        [InlineData("(Test\\ Test)", "Test Test")]  // ignore random \
        [InlineData("(Test \\t)", "Test \t")] // tab
        [InlineData("(Test \\n\\r)", "Test \n\r")] // keep new lines by espace
        [InlineData("(Test \\\nNextLine)", "Test NextLine")] // allows split strings by line
        [InlineData("(Test \\\rNextLine)", "Test NextLine")]
        [InlineData("(Test \\\r\nNextLine)", "Test NextLine")]
        [InlineData("(Test Test\rTest )", "Test Test\rTest ")] // keeps new lines
        [InlineData("(Test Test\r\nTest )", "Test Test\r\nTest ")]
        [InlineData("(Test Test\nTest )", "Test Test\nTest ")]
        [InlineData("(Test Test Test )", "Test Test Test ")]
        [InlineData("(Test (Test) Test )", "Test (Test) Test ")]
        [InlineData("(Test \\\\(Test) Test )", "Test \\(Test) Test ")]
        [InlineData("(Test \\) Test )", "Test ) Test ")]
        [InlineData("(Test \\\\\\) Test )", "Test \\) Test ")]
        [InlineData("()", "")]
        [InlineData("(\\171)", "y")] // octal
        [InlineData("( \\1a)", " 1a")]
        [InlineData("( \\171a)", " ya")]
        [InlineData("(partial octal \\53\\171)", "partial octal +y")]
        [Theory]
        public void It_Parses_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);
        }

        [InlineData("(Test)", "Test")]
        [InlineData("(Test \\t)", "Test \t")] // tab
        [InlineData("(Test \\n\\r)", "Test \n\r")] // keep new lines by espace
        [InlineData("(Test Test Test )", "Test Test Test ")]
        [InlineData("(Test \\\\\\(Test Test )", "Test \\(Test Test ")]
        [InlineData("(Test \\) Test )", "Test ) Test ")]
        [InlineData("(Test \\\\\\) Test )", "Test \\) Test ")]
        [InlineData("(Test \\036 Test )", "Test \u001E Test ")]
        [InlineData("()", "")]
        [InlineData("(\\216\\217)", "\u008e\u008f")] // octal unhappy
        [InlineData("(Test \\310Line)", "Test ??Line")] // "happy" > 128 chars
        [Theory]
        public void It_Parses_And_Serializes_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var serializer = new StringSerializer();

            // scan past
            int s = 0;
            var succeeded = StringParser.AdvancePastStringLiteral(bytes, ref s);
            Assert.True(succeeded);
            Assert.Equal(input.Length, s);

            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);

            var rms = new MemoryStream();
            serializer.WriteToStream(result, rms);
            Assert.Equal(input, Encoding.ASCII.GetString(rms.ToArray()));
        }

        [InlineData("<> ", "")]
        [InlineData("<303132> ", "012")]
        [InlineData("<30313> ", "010")]
        [Theory]
        public void It_Gets_Hex(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var parser = new StringParser(new ParsingContext());
            var span = new Span<byte>(bytes);
            var result = parser.Parse(span);
            Assert.Equal(output, result.Value);
        }

        [InlineData("(Test) ", 6)]
        [InlineData("(Test Test Test )\r\n", 17)]
        [InlineData("(Test (Test) Test )\n", 19)]
        [InlineData("(Test \\\\(Test) Test )\n", 21)]
        [InlineData("(Test \\) Test )\n", 15)]
        [InlineData("(Test \\\\\\) Test )\n", 17)]
        [InlineData("() ", 2)]
        [InlineData("()", 2)]
        [Theory]
        public void It_Skips_String_Literals(string input, int pos)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            var result = StringParser.TryAdvancePastString(ref reader);
            Assert.True(result);
            Assert.Equal(seq.GetPosition(pos), reader.Position);
        }

        [Fact]
        public void It_Parses_And_Serialized_Literal_UTF16BE()
        {
            var root = PathUtil.GetPathFromSegmentOfCurrent("test");
            var data = File.ReadAllBytes(Path.Combine(root, "raw-utf16-string-literal.txt"));
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal(PdfStringType.Literal, result.StringType);
            Assert.Equal(PdfTextEncodingType.UTF16BE, result.Encoding);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            Assert.True(data.SequenceEqual(output));
        }

        [Fact]
        public void It_Parses_And_Serialized_Hex_UTF16BE()
        {
            var text = "<FEFF00540065007300740069006E006700480065007800550074006600310036>";
            var data = Encoding.ASCII.GetBytes(text);
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal("TestingHexUtf16", result.Value);
            Assert.Equal(PdfStringType.Hex, result.StringType);
            Assert.Equal(PdfTextEncodingType.UTF16BE, result.Encoding);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            Assert.True(data.SequenceEqual(output));
        }

        [Fact]
        public void It_Parses_And_Serializes_Raw_Bytes()
        {
            var root = PathUtil.GetPathFromSegmentOfCurrent("test");
            var data = File.ReadAllBytes(Path.Combine(root, "raw-byte-string.txt"));
            var parser = new StringParser(new ParsingContext());
            var result = parser.Parse(data);
            Assert.Equal(PdfStringType.Literal, result.StringType);
            var serializer = new StringSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            var output = ms.ToArray();
            File.WriteAllBytes(Path.Combine(root, "raw-byte-string-out.txt"), output);
            Assert.True(data.SequenceEqual(output));
        }
    }
}