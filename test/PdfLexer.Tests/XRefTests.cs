using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using Xunit;

namespace PdfLexer.Tests
{
    public class XRefTests
    {
        [InlineData(@"1234xref
2 5
0004658443 00000 n 
0004658589 00000 n 
0004658687 00000 n 
0004658785 00000 n 
0004658836 00000 n 
trailer
<</Size 3388/Info 3387 0 R/ID [<5b61ecda9bd3cb8125e4ce579fe6240c><56bf60814128464e7c7da4840439bde2>]/Root 3386 0 R>>
startxref
0004
%%EOF
", 5, 6)]
        [InlineData(@"123456789xref
10 6
0004658443 00000 n 
0004658589 00000 n 
0004658687 00000 n 
0004658785 00000 n 
0004658836 00000 n 
0004658840 00000 n 
trailer
<</Size 3388/Info 3387 0 R/ID [<5b61ecda9bd3cb8125e4ce579fe6240c><56bf60814128464e7c7da4840439bde2>]/Root 3386 0 R>>
startxref
000000009
%%EOF

", 6, 15)]
        [Theory]
        public void ItGetsXrefOffset(string input, int objectCount, int lastObjNum)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ctx = new ParsingContext();
            var parser = new XRefParser(ctx);
            var source = new InMemoryDataSource(ctx, bytes);
            var data = parser.LoadCrossReference(source);
            Assert.Equal(objectCount, data.Item1.Count);
            Assert.Equal(lastObjNum, data.Item1.Max(x=>x.Reference.ObjectNumber));
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
startxref
0
%%EOF
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
<</Size 8619/Root 8617 0 R/Info 8618 0 R/ID [<b31927b8053b797df62c3dad88848ff9><b31927b8053b797df62c3dad88848ff9>]>>
startxref
0
%%EOF", 8, 313465, 101)]
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
<</Size 8619/Root 8617 0 R/Info 8618 0 R/ID [<b31927b8053b797df62c3dad88848ff9><b31927b8053b797df62c3dad88848ff9>]>>
startxref
0
%%EOF", 7, 313465, 42)]
        [Theory]
        public void ItGetsXRefTableMultiSection(string input, int entries, long lastOffset, int lastObj)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ctx = new ParsingContext(new ParsingOptions { Eagerness = Eagerness.FullEager });
            var parser = new XRefParser(ctx);
            var source = new InMemoryDataSource(ctx, bytes);
            var (results, trailer) = parser.LoadCrossReference(source);
            Assert.Equal(entries, results.Count);
            var last = results.OrderByDescending(x=>x.Reference.ObjectNumber).First();
            Assert.Equal(lastOffset, last.Offset);
            Assert.Equal(lastObj, last.Reference.ObjectNumber);
        }

        [InlineData(@"0 6
0000000000 65535 f 
0000314102 00000 n 
0000000015 00000 n 
0004653352 00000 n 
0000000130 00000 n 
0000313460 00000 n 
", 6, 313460, 5)]
        [InlineData(@"0 6
0000000000 65535 f 
0000314102 00000 n 
0000000015 00000 n 
0004653352 00000 n 
0000000130 00000 n 
0000313460 00000 n 
100 2
0000000135 00000 n 
0000313465 00000 n 
", 8, 313465, 101)]
        [InlineData(@"0 1
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
", 7, 313465, 42)]
        [Theory]
        public void ItGetsXRefEntries(string input, int entries, long lastOffset, int lastObj)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ctx = new ParsingContext();
            var parser = new XRefParser(ctx);
            var source = new InMemoryDataSource(ctx, bytes);
            var data = new List<XRefEntry>();
            var results = parser.GetEntries(bytes, data);
            Assert.Equal(entries, results.Count);
            var last = results.Single(x => x.Reference.ObjectNumber == lastObj);
            Assert.Equal(lastOffset, last.Offset);
            Assert.Equal(lastObj, last.Reference.ObjectNumber);
        }

        [InlineData(@"0 1 1 R", 4, 1)]
        [Theory]
        public async Task ItReadsTokenSequences(string input, int entries, int nullEntries)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var results = new List<IPdfObject>();
            var ctx = new ParsingContext();
            var pipe = PipeReader.Create(ms);
            var result = await pipe.ReadTokenSequence(ctx, ParseOp.IndirectReference, results);
            Assert.Equal(entries, results.Count);
            Assert.Equal(nullEntries, results.Count(x => x==null));
        }

        [InlineData(@"0 1 obj
<</Key/Value>>
endobj", PdfObjectType.DictionaryObj)]
        [InlineData(@"0 1 obj
<</Key(this is a (nested) long\) string)>>
endobj", PdfObjectType.DictionaryObj)]
        [InlineData(@"1001 0 obj
[1 0 3 <</Key/Value>>]
endobj", PdfObjectType.ArrayObj)]
        [Theory]
        public async Task ItReadsIndirectObjectsUsingMultiSequences(string input, PdfObjectType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var results = new List<IPdfObject>();
            var ctx = new ParsingContext();
            var pipe = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 1));
            var result = await pipe.ReadTokenSequence(ctx, ParseOp.IndirectObject, results);
            Assert.Equal(5, results.Count);
            var item = results[3];
            Assert.Equal(type, item.Type);
        }

        [InlineData(@"0 1 obj
<</Key/Value>>
endobj", PdfObjectType.DictionaryObj)]
        [InlineData(@"1001 0 obj
[1 0 3 <</Key/Value>>]
endobj", PdfObjectType.ArrayObj)]
        [Theory]
        public async Task ItReadsIndirectObjectsUsingSequences(string input, PdfObjectType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var results = new List<IPdfObject>();
            var ctx = new ParsingContext();
            var pipe = PipeReader.Create(ms);
            var result = await pipe.ReadTokenSequence(ctx, ParseOp.IndirectObject, results);
            Assert.Equal(5, results.Count);
            var item = results[3];
            Assert.Equal(type, item.Type);
        }

        [InlineData(@"<</Key/Value/Key1/Value/Key2/Value>>", PdfTokenType.DictionaryStart, PdfObjectType.DictionaryObj)]
        [InlineData(@"/Name", PdfTokenType.NameObj, PdfObjectType.NameObj)]
        [InlineData(@"xref",PdfTokenType.Xref, PdfObjectType.NullObj)]
        [Theory]
        public async Task ItReadsNextObject(string input, PdfTokenType token, PdfObjectType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var results = new List<IPdfObject>();
            var ctx = new ParsingContext();
            var pipe = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 1));
            var result = await pipe.ReadNextObjectOrToken(ctx);
            if (type != PdfObjectType.NullObj)
            {
                Assert.Equal(type, result.Obj.Type);
            } else
            {
                Assert.Equal(token, result.Type);
            }
        }

        [InlineData(@"<</Key/Value/Key1/Value/Key2/Value>>             <</Key/Value>>", PdfObjectType.DictionaryObj, PdfObjectType.DictionaryObj)]
        [InlineData(@"/Name                 10", PdfObjectType.NameObj, PdfObjectType.NumericObj)]
        [Theory]
        public async Task ItReadsMultipleObject(string input, PdfObjectType obj1, PdfObjectType obj2)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var ms = new MemoryStream(bytes);
            var results = new List<IPdfObject>();
            var ctx = new ParsingContext();
            var pipe = PipeReader.Create(ms, new StreamPipeReaderOptions(bufferSize: 10, minimumReadSize: 1));
            var result = await pipe.ReadNextObjectOrToken(ctx);
            Assert.Equal(obj1, result.Obj.Type);
            result = await pipe.ReadNextObjectOrToken(ctx);
            Assert.Equal(obj2, result.Obj.Type);
        }
    }
}