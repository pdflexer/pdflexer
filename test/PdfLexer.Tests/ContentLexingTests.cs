using PdfLexer.Lexing;
using PdfLexer.Operators;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace PdfLexer.Tests
{
    public class ContentLexingTests
    {
        [Fact]
        public void It_Lexes_Stream()
        {
            var text = @"q
0.06000 0 0 -0.06000 1 594 cm

870 1095 768 176 re
h
W* n
Q
f";
            var data = Encoding.ASCII.GetBytes(text);

            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            Assert.Equal(PdfOperatorType.q, scanner.Peek());
            Assert.Empty(scanner.Operands);
            var op = scanner.GetCurrentOperation();
            Assert.Equal(PdfOperatorType.q, op.Type);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.cm, scanner.Peek());
            Assert.Equal(6, scanner.Operands.Count);
            op = scanner.GetCurrentOperation();
            Assert.Equal(PdfOperatorType.cm, op.Type);
            var cm = (cm_Op)op;
            Assert.Equal(0.06m, cm.a);
            Assert.Equal(0m, cm.b);
            Assert.Equal(0m, cm.c);
            Assert.Equal(-0.06m, cm.d);
            Assert.Equal(1m, cm.e);
            Assert.Equal(594m, cm.f);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.re, scanner.Peek());
            Assert.Equal(4, scanner.Operands.Count);
            op = scanner.GetCurrentOperation();
            Assert.Equal(PdfOperatorType.re, op.Type);
            var re = (re_Op)op;
            Assert.Equal(870m, re.x);
            Assert.Equal(1095m, re.y);
            Assert.Equal(768m, re.width);
            Assert.Equal(176m, re.height);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.h, scanner.Peek());
            Assert.Empty(scanner.Operands);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.W_Star, scanner.Peek());
            Assert.Empty(scanner.Operands);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.n, scanner.Peek());
            Assert.Empty(scanner.Operands);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.Q, scanner.Peek());
            Assert.Empty(scanner.Operands);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.f, scanner.Peek());
            Assert.Empty(scanner.Operands);
            scanner.SkipCurrent();
        }

        [Fact]
        public void It_Reads_And_Writes_Stream()
        {
            var text = "q\n0.06000 0 0 -0.06000 1 594 cm\n870 1095 768 176 re\nh\nW*\nn\nQ\nf\n";
            var data = Encoding.ASCII.GetBytes(text);

            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var ops = new List<IPdfOperation>();
            PdfOperatorType nxt = PdfOperatorType.Unknown;
            while ((nxt = scanner.Peek()) != PdfOperatorType.EOC)
            {
                ops.Add(scanner.GetCurrentOperation());
                scanner.SkipCurrent();
            }

            Assert.Equal(8, ops.Count);

            var str = new MemoryStream();
            foreach (var op in ops)
            {
                op.Serialize(str);
                str.WriteByte((byte)'\n');
            }
            var contents = Encoding.ASCII.GetString(str.ToArray());
            Assert.Equal(text, contents);
        }
    }
}
