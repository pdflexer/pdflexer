using System;
using System.Buffers;
using System.Text;
using PdfLexer.Lexing;
using Xunit;

namespace PdfLexer.Tests
{
    public class DetectionTests
    {
        [InlineData(" (Test) ", true, 1, 6, true, PdfTokenType.StringObj)]
        [InlineData("<1111> ", true, 0, 6, true, PdfTokenType.StringObj)]
        [InlineData("<1111>", true, 0, 6, true, PdfTokenType.StringObj)]
        [InlineData("<<", true,0, 2, true,PdfTokenType.DictionaryStart)]
        [InlineData(" true ", true,1, 4, true,PdfTokenType.BooleanObj)]
        [InlineData("  false ", true, 2, 5, true,PdfTokenType.BooleanObj)]
        [InlineData("null",true, 0, 4, true,PdfTokenType.NullObj)]
        [InlineData("0", true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData("1.0", true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData(" .1", true, 1, 2, true,PdfTokenType.NumericObj)]
        [InlineData(" -1 ", true, 1, 2, true,PdfTokenType.NumericObj)]
        [InlineData(" -1.0 ", true, 1, 4, true,PdfTokenType.NumericObj)]
        [InlineData(" stream\r\n",true, 1, 8, true,PdfTokenType.StartStream)]
        [InlineData(" stream\r", false, 1, 0, false, PdfTokenType.NullObj)]
        [InlineData(" stream", false, 1, 0, false, PdfTokenType.NullObj)]
        [InlineData("stre", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData("stream", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData(" stream\n",true, 1, 7, true,PdfTokenType.StartStream)]
        [InlineData(" endstream",true, 1, 9, true,PdfTokenType.EndStream)]
        [InlineData(" endstream ",true, 1, 9, true,PdfTokenType.EndStream)]
        [InlineData("endstream",true, 0, 9, true,PdfTokenType.EndStream)]
        [InlineData("endstr", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData("endobj", true,0, 6, true, PdfTokenType.EndObj)]
        [InlineData("1 0 R",true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData(" 1 0 R", true,1, 1, true,PdfTokenType.NumericObj)]
        [InlineData("/Name ",true, 0, 5, true,PdfTokenType.NameObj)]
        [InlineData("/Name",true, 0, 5, true, PdfTokenType.NameObj)]
        [InlineData("1 0",true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData("101 0.1",true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData("901 10",true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData("  %901 10", false, 2, 2, true,PdfTokenType.NullObj)]
        [InlineData("  %asdf%\r\n.1 ", true,10, 2, true,PdfTokenType.NumericObj)]
        [InlineData("  %asdf%\n.1 ",true, 9, 2, true,PdfTokenType.NumericObj)]
        [InlineData("  %asdf\n(.1 10 R", false, 8, 0, true,PdfTokenType.StringObj)]
        [InlineData("[", true,0, 1, true,PdfTokenType.ArrayStart)]
        [InlineData("1<", true,0, 1, true,PdfTokenType.NumericObj)]
        [Theory]
        public void It_Lexes_Span(string input, bool success, int startPos, int expectedLength, bool completed, PdfTokenType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var buffer = new ReadOnlySpan<byte>(bytes);

            var start = PdfSpanLexer.TryReadNextToken(buffer, out var tokenType, out int length);

            if (success)
            {
                Assert.True(start > -1);
                Assert.Equal(type, tokenType);
                Assert.Equal(startPos, start);
                Assert.Equal(expectedLength, length);
            }
            else
            {
                Assert.Equal(-1, start);
            }
        }

#if NET50
        [InlineData(" (Test) ", true, 1, 6, true, PdfTokenType.StringObj)]
        [InlineData("<1111> ", true, 0, 6, true, PdfTokenType.StringObj)]
        [InlineData("<<", true,0, 2, true,PdfTokenType.DictionaryStart)]
        [InlineData(" true ", true,1, 4, true,PdfTokenType.BooleanObj)]
        [InlineData("  false ", true, 2, 5, true,PdfTokenType.BooleanObj)]
        [InlineData("null",true, 0, 4, true,PdfTokenType.NullObj)]
        [InlineData("0", true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData("1.0", true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData(" .1", true, 1, 2, true,PdfTokenType.NumericObj)]
        [InlineData(" -1 ", true, 1, 2, true,PdfTokenType.NumericObj)]
        [InlineData(" -1.0 ", true, 1, 4, true,PdfTokenType.NumericObj)]
        [InlineData(" stream\r\n",true, 1, 8, true,PdfTokenType.StartStream)]
        [InlineData(" stream\r", false, 1, 0, false, PdfTokenType.NullObj)]
        [InlineData("stre", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData("stream", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData(" stream\n",true, 1, 7, true,PdfTokenType.StartStream)]
        [InlineData(" endstream",true, 1, 9, true,PdfTokenType.EndStream)]
        [InlineData("endstream",true, 0, 9, true,PdfTokenType.EndStream)]
        [InlineData("endstr", false, 0, 0, false, PdfTokenType.NullObj)]
        [InlineData("endobj", true,0, 6, true, PdfTokenType.EndObj)]
        [InlineData("endobj",true, 0, 6, false, PdfTokenType.EndObj)]
        [InlineData("1 0 R",true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData(" 1 0 R", true,1, 1, true,PdfTokenType.NumericObj)]
        [InlineData("/Name ",true, 0, 5, true,PdfTokenType.NameObj)]
        [InlineData("/Name",true, 0, 5, true, PdfTokenType.NameObj)]
        [InlineData("1 0",true, 0, 1, true,PdfTokenType.NumericObj)]
        [InlineData("101 0.1",true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData("901 10",true, 0, 3, true,PdfTokenType.NumericObj)]
        [InlineData("  %901 10", false, 2, 2, true,PdfTokenType.NullObj)]
        [InlineData("  %asdf%\r\n.1 ", true,10, 2, true,PdfTokenType.NumericObj)]
        [InlineData("  %asdf%\n.1 ",true, 9, 2, true,PdfTokenType.NumericObj)]
        [InlineData("  %asdf\n(.1 10 R", false, 8, 0, true,PdfTokenType.StringObj)]
        [InlineData("[", true,0, 1, true,PdfTokenType.ArrayStart)]
        [InlineData("1<", true,0, 1, true,PdfTokenType.NumericObj)]
        [Theory]
        public void It_Lexes_Sequence(string input, bool success, int startPos, int expectedLength, bool completed, PdfTokenType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var sequence = new ReadOnlySequence<byte>(bytes);
            
            var reader = new SequenceReader<byte>(sequence);
            // var reader = PipeReader.Create(stream);
            var result = reader.TryReadNextToken(completed, out var tokenType, out var start);
            if (success)
            {
                var data = sequence.Slice(start, reader.Position);
                Assert.True(result);
                Assert.Equal(type, tokenType);
                Assert.Equal(startPos, sequence.GetOffset(start));
                Assert.Equal(expectedLength, (int)sequence.GetOffset(reader.Position)-startPos);
                Assert.Equal(sequence.GetPosition(startPos + expectedLength) ,reader.Position);
            }
            else
            {
                Assert.False(result);
                Assert.Equal(startPos, sequence.GetOffset(start));
                Assert.Equal(sequence.GetPosition(startPos).GetInteger() ,reader.Position.GetInteger());
            }
        }
#endif
    }
}