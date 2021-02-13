using System;
using System.Text;
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