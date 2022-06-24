using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.Parsers;
using Xunit;

namespace PdfLexer.Tests
{
    public class StringTests
    {
        [InlineData("(Test) ", "(Test)")]
        [InlineData("(Test Test Test )\r\n", "(Test Test Test )")]
        [InlineData("(Test (Test) Test )\n", "(Test (Test) Test )")]
        [InlineData("(Test \\\\(Test) Test )\n", "(Test \\\\(Test) Test )")]
        [InlineData("(Test \\) Test )\n", "(Test \\) Test )")]
        [InlineData("(Test \\\\\\) Test )\n", "(Test \\\\\\) Test )")]
        [InlineData("() ", "()")]
        [InlineData("()", "()")]
        [Theory]
        public void It_Gets_Literals(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var gotit = StringParser.GetString(bytes, out ReadOnlySpan<byte> result);
            Assert.True(gotit);
            var outputBytes = Encoding.ASCII.GetBytes(output);
            for (var i=0;i<outputBytes.Length;i++)
            {
                Assert.Equal(outputBytes[i], result[i]);
            }
        }

        [InlineData("(Test) ", "Test")]
        [InlineData("(Test\\ Test) ", "Test Test")]  // ignore random \
        [InlineData("(Test \\t) ", "Test \t")] // tab
        [InlineData("(Test \\n\\r) ", "Test \n\r")] // keep new lines by espace
        [InlineData("(Test \\\nNextLine) ", "Test NextLine")] // allows split strings by line
        [InlineData("(Test \\\rNextLine) ", "Test NextLine")]
        [InlineData("(Test \\\r\nNextLine) ", "Test NextLine")]
        [InlineData("(Test Test\rTest )\r\n", "Test Test\rTest ")] // keeps new lines
        [InlineData("(Test Test\r\nTest )\r\n", "Test Test\r\nTest ")]
        [InlineData("(Test Test\nTest )\r\n", "Test Test\nTest ")]
        [InlineData("(Test Test Test )\r\n", "Test Test Test ")]
        [InlineData("(Test (Test) Test )\n", "Test (Test) Test ")]
        [InlineData("(Test \\\\(Test) Test )\n", "Test \\(Test) Test ")]
        [InlineData("(Test \\) Test )\n", "Test ) Test ")]
        [InlineData("(Test \\\\\\) Test )\n", "Test \\) Test ")]
        [InlineData("() ", "")]
        [InlineData("()", "")]
        [Theory]
        public async Task It_Gets_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 4, minimumReadSize: 1));
            var parser = new StringParser(new ParsingContext());
            ReadResult result;
            while (!(result = await reader.ReadAsync()).IsCompleted)
            {
                var buffer = result.Buffer;
                if (ReadIt(ref buffer, out var pos))
                {
                    var stringResult = parser.builder.ToString();
                    Assert.Equal(output, stringResult);
                }
                reader.AdvanceTo(pos);
            }

            bool ReadIt(ref ReadOnlySequence<byte> buffer, out SequencePosition pos)
            {
                var sr = new SequenceReader<byte>(buffer);
                var r = parser.TryReadStringLiteral(ref sr);
                pos = sr.Position;
                return r;
            }
        }

        [InlineData("<> ", "<>")]
        [InlineData("<1234564879879> ", "<1234564879879>")]
        [InlineData("<123> ", "<123>")]
        [Theory]
        public void It_Gets_Hex(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var gotit = StringParser.GetString(bytes, out ReadOnlySpan<byte> result);
            Assert.True(gotit);
            var outputBytes = Encoding.ASCII.GetBytes(output);
            for (var i = 0; i < outputBytes.Length; i++)
            {
                Assert.Equal(outputBytes[i], result[i]);
            }
        }

        [InlineData("(Test)", "Test", -1)]
        [InlineData("(Test Test Test ) ", "Test Test Test ", -1)]
        [InlineData("(Test (Test) Test ) ", "Test (Test) Test ", -1)]
        [InlineData("() ", "", -1)]
        [Theory]
        public void It_Parses_Literals(string input, string output, int length)
        {
            if (length == -1) { length = input.Trim().Length; }
            var bytes = Encoding.ASCII.GetBytes(input);
            var count = StringParser.ParseString(bytes, out Span<char> result);
            Assert.Equal(output, result.ToString());
            Assert.Equal(length, count);
        }
    }
}