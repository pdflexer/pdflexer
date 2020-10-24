using System;
using System.Text;
using PdfLexer.Objects.Parsers;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class NameTests
    {
        [InlineData("/Test/Value", true, "/Test")]
        [InlineData("/Test ", true, "/Test")]
        [InlineData("/Test<<Dict", true, "/Test")]
        [InlineData("/Test[0 0", true, "/Test")]
        [Theory]
        public void It_Gets_Names(string input, bool found, string output)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var gotit = NameParser.GetNameBytes(bytes, out ReadOnlySpan<byte> nameBytes);
            Assert.Equal(found, gotit);
            if (gotit)
            {
                var outputBytes = Encoding.ASCII.GetBytes(output);
                for (var i = 0; i < outputBytes.Length; i++)
                {
                    Assert.Equal(outputBytes[i], nameBytes[i]);
                }
            }
        }

        [InlineData("/Test ", "Test", -1)]
        [InlineData("/Test  ", "Test", -1)]
        [InlineData("/Test\r", "Test", 5)]
        [InlineData("/Test\n", "Test", 5)]
        [InlineData("/Test\r\n", "Test", 5)]
        [InlineData("/Test#20Test ", "Test Test", -1)]
        [InlineData("/PANTONE#205757#20CV ", "PANTONE 5757 CV", -1)]
        [InlineData("/paired#28#29parentheses ", "paired()parentheses", -1)]
        [InlineData("/The_Key_of_F#23_Minor ", "The_Key_of_F#_Minor", -1)]
        [InlineData("/A#42 ", "AB", -1)]
        [Theory]
        public void It_Parses_Names(string input, string output, int length)
        {
            if (length == -1) { length = input.Trim().Length; }
            var bytes = Encoding.ASCII.GetBytes(input);
            var found = NameParser.GetNameBytes(bytes, out ReadOnlySpan<byte> nameBytes);
            Assert.True(found);
            var count = NameParser.ParseName(nameBytes, out Span<char> result);
            Assert.Equal(output, result.ToString());
            Assert.Equal(length, count);
        }
    }
}