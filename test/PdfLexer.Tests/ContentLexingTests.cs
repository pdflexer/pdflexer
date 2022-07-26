using PdfLexer.Filters;
using PdfLexer.Lexing;
using PdfLexer.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace PdfLexer.Tests
{
    public class ContentLexingTests
    {
        [InlineData("<+U,m+D#G#De*R\"B-:W(@qfW~>", "Test encoding ascii", true)]
        [InlineData("<+U,m+D#G#De*R\"B-:W(@qfW~>", "Test encoding ascii", false)]
        [Theory]
        public void It_Decodes(string encoded, string decoded, bool singleByte)
        {
            var data = Encoding.ASCII.GetBytes(encoded);
            var decoder = new Ascii85Filter();
            var ms = new MemoryStream(data);
            var result = decoder.Decode(ms, null);
            var rms = new MemoryStream();
            if (singleByte)
            {
                int b = 0;
                while ((b = result.ReadByte()) != -1)
                {
                    rms.WriteByte((byte)b);
                }
            } else
            {
                result.CopyTo(rms);
            }
            var str = Encoding.UTF8.GetString(rms.ToArray());
            Assert.Equal(decoded, str);
        }

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

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void It_Handles_Inline(bool crlf)
        {
            var txt = @"100 0 0 100 0 0 cm
BI /W 4 /H 4 /CS /RGB /BPC 8
ID
00000z0z00zzz00z0zzz0zzzEI aazazaazzzaazazzzazzz
EI
100 0 0 100 0 0 cm";
            if (crlf)
            {
                txt = txt.Replace("\r\n", "\n");
                txt = txt.Replace("\n", "\r\n");
            } else
            {
                txt = txt.Replace("\r\n", "\n");
            }
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
            scanner.SkipCurrent();
            var bi = scanner.Peek();
            Assert.Equal(PdfOperatorType.BI, bi);
            var img = scanner.GetCurrentOperation();
            scanner.SkipCurrent();
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);

            // just skip
            scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
            scanner.SkipCurrent();
            bi = scanner.Peek();
            Assert.Equal(PdfOperatorType.BI, bi);
            scanner.SkipCurrent();
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);

            // just data
            scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
            scanner.SkipCurrent();
            bi = scanner.Peek();
            Assert.Equal(PdfOperatorType.BI, bi);
            var imgData = scanner.GetCurrentData();
            Assert.Equal(crlf ? 86 : 83, imgData.Length);
            bi = scanner.Peek();
            Assert.Equal(PdfOperatorType.BI, bi);
            scanner.SkipCurrent();
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
        }

        [Fact]
        public void It_Handles_Inline_At_End()
        {
            var txt = @"100 0 0 100 0 0 cm
BI /W 4 /H 4 /CS /RGB /BPC 8
ID
00000z0z00zzz00z0zzz0zzzEI aazazaazzzaazazzzazzz
EI";
            txt = txt.Replace("\r\n", "\n");
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.BI, scanner.Peek());
            var imgData = scanner.GetCurrentData();
            Assert.Equal(83, imgData.Length);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.EOC, scanner.Peek());

            // read data
            scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            cm = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, cm);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.BI, scanner.Peek());
            var op = scanner.GetCurrentOperation();
            var opImage = op as InlineImage_Op;
            Assert.Equal(48, opImage?.allData?.Length ?? 0);
            scanner.SkipCurrent();
            Assert.Equal(PdfOperatorType.EOC, scanner.Peek());
        }

        [Fact]
        public void It_Handles_DP()
        {
            var txt = @"/DocumentSetup << /PageOrigin [ 11
12
]
/RulerOrigin [ 0
0
]
>>
DP
q
Q";
            txt = txt.Replace("\r\n", "\n");
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var dp = scanner.Peek();
            Assert.Equal(PdfOperatorType.DP, dp);
            var op = (DP_Op)scanner.GetCurrentOperation();
            Assert.Equal("/DocumentSetup", op.tag);
            Assert.Equal(PdfObjectType.DictionaryObj, op.props.Type);
            var dict = (PdfDictionary)op.props;
            Assert.Equal(2, dict.Count);
            scanner.SkipCurrent();
            var q = scanner.Peek();
            Assert.Equal(PdfOperatorType.q, q);
        }

        [Fact]
        public void It_Handles_BDC_Inline()
        {
            var txt = @"/Layer << /Printed true
/Visible true
/Preview true
/Dimmed false
/Title (Calque 1)
/Transparency false
/flatLayer false
/Editable true
/Color [ 20224
32768
65535
]
>>
BDC
q";
            txt = txt.Replace("\r\n", "\n");
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var bdc = scanner.Peek();
            Assert.Equal(PdfOperatorType.BDC, bdc);
            var op = scanner.GetCurrentOperation();
            scanner.SkipCurrent();
            var q = scanner.Peek();
            Assert.Equal(PdfOperatorType.q, q);
        }

        [Fact]
        public void It_Fixes_Bad_Name()
        {
            var txt = @"0.1 /F1 Do";
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var doOp = scanner.Peek();
            Assert.Equal(PdfOperatorType.Do, doOp);
            var succeeded = scanner.TryGetCurrentOperation(out var doOpObj);
            Assert.True(succeeded);
            Assert.Equal(PdfOperatorType.Do, doOpObj.Type);
            var obj = (Do_Op)doOpObj;
            Assert.Equal("F1", obj.name);
        }

        [Fact]
        public void It_Fixes_Bad_Numbers()
        {
            var txt = @"100 0 0 100 /Bad 0 0 cm";
            var data = Encoding.ASCII.GetBytes(txt);

            // read then skip
            var scanner = new ContentScanner(new Parsers.ParsingContext(), data);
            var op = scanner.Peek();
            Assert.Equal(PdfOperatorType.cm, op);
            var succeeded = scanner.TryGetCurrentOperation(out var opObj);
            Assert.True(succeeded);
            Assert.Equal(PdfOperatorType.cm, opObj.Type);
            var obj = (cm_Op)opObj;
            Assert.Equal(100, obj.a);
            Assert.Equal(0, obj.b);
            Assert.Equal(0, obj.c);
            Assert.Equal(100, obj.d);
            Assert.Equal(0, obj.e);
            Assert.Equal(0, obj.f);
        }
    }
}
