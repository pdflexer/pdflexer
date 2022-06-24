using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Lexing
{
    public class ParseOp
    {
        public static List<ParseOp> IndirectObject = new List<ParseOp>
        {
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.StartObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.WildCard },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.EndObj }
        };

        public static List<ParseOp> IndirectReference = new List<ParseOp>
        {
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.IndirectRef }
        };

        public static List<ParseOp> FindXrefOffset = new List<ParseOp>
        {
            new ParseOp { Type = ParseOpType.ScanToAndSkip, ScanSequence = IndirectSequences.strartxref },
            new ParseOp { Type = ParseOpType.ReadToken, Token = PdfTokenType.NumericObj }
        };

        public ParseOpType Type { get; set; }
        public PdfTokenType Token { get; set; }
        public byte[] ScanSequence {get;set;}

        public delegate bool TryRetrantParseStep(ParsingContext ctx, ref SequenceReader<byte> reader, bool isSequenceComplete, out object result);
    }

    public interface IRentrantParseStep
    {
        bool TryCompleteStep(ref SequenceReader<byte> reader, bool isComplete);
    }

    public enum ParseOpType
    {
        ScanToAndSkip,
        ReadToken
    }
}
