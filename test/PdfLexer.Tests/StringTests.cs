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
        [InlineData("(\\171)", "y")] // octal
        [InlineData("( \\1a)", " 1a")] 
        [InlineData("( \\17a)", " 17a")] 
        [InlineData("( \\171a)", " ya")] 
        [InlineData("(Test \\\rNext\\147Line) ", "Test NextgLine")]
        [Theory]
        public async Task It_Gets_Literals_Reader(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 4, minimumReadSize: 1));
            var parser = new StringParser(new ParsingContext());

            // sync
            var result = parser.Parse(reader);
            Assert.Equal(output, result.Value);
            // async
            ms.Seek(0, SeekOrigin.Begin);
            reader = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 4, minimumReadSize: 1));
            result = await parser.ParseAsync(reader);
            Assert.Equal(output, result.Value);
            // span
            CheckSpanBased();

            void CheckSpanBased()
            {
                var span = new Span<byte>(bytes);
                result = parser.Parse(span);
                Assert.Equal(output, result.Value);
            }
        }

        [InlineData("<> ", "<>")]
        [InlineData("<1234564879879> ", "<1234564879879>")]
        [InlineData("<123> ", "<123>")]
        [Theory]
        public void It_Gets_Hex(string input, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            // var gotit = StringParser.GetString(bytes, out ReadOnlySpan<byte> result);
            // Assert.True(gotit);
            // var outputBytes = Encoding.ASCII.GetBytes(output);
            // for (var i = 0; i < outputBytes.Length; i++)
            // {
            //     Assert.Equal(outputBytes[i], result[i]);
            // }
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
        public void It_Gets_Literals(string input, int pos)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            var result = StringParser.TryAdvancePastString(ref reader);
            Assert.True(result);
            Assert.Equal(seq.GetPosition(pos), reader.Position);
            // var gotit = StringParser.GetString(bytes, out ReadOnlySpan<byte> result);
            // Assert.True(gotit);
            // var outputBytes = Encoding.ASCII.GetBytes(output);
            // for (var i=0;i<outputBytes.Length;i++)
            // {
            //     Assert.Equal(outputBytes[i], result[i]);
            // }
        }
    }
}