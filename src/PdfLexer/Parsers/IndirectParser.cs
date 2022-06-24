using System;

namespace PdfLexer.Parsers
{
    public class IndirectSequences
    {
        public static byte[] obj = new byte[] { (byte)'o', (byte)'b', (byte)'j' };
        public static byte[] endobj = new byte[] { (byte)'e', (byte)'n', (byte)'d',
            (byte)'o', (byte)'b', (byte)'j' };
        public static byte[] stream = new byte[] { (byte)'s', (byte)'t', (byte)'r',
            (byte)'e', (byte)'a', (byte)'m' };
        public static byte[] endstream = new byte[] { (byte)'e', (byte)'n', (byte)'d',(byte)'s', (byte)'t', (byte)'r',
            (byte)'e', (byte)'a', (byte)'m' };
        public static byte[] length = new byte[] { (byte)'/', (byte)'L', (byte)'e',
            (byte)'n', (byte)'g', (byte)'t', (byte)'h' };
        public static byte[] strartxref = new byte[] { (byte)'s', (byte)'t', (byte)'a',
            (byte)'r', (byte)'t', (byte)'x', (byte)'r', (byte)'e', (byte)'f' };
    }

    public struct IndirectReference
    {
        public int ObjectNumber { get; set; }
        public int Generation { get; set; }
    }
    public class IndirectParser
    {
        public static IndirectReference ParseIndirectReference(ReadOnlySpan<byte> bytes, int offset)
        {
            throw new NotImplementedException();
            // var used = NumberParser.GetInt(bytes, offset, out int objNum);
            // if (used == -1)
            // {
            //     throw new ApplicationException("Incomplete or non-integer found for indirect reference.");
            // }
            // offset += used;
            // offset = CommonUtil.SkipWhiteSpaces(bytes, offset);
            // if (offset == -1)
            // {
            //     throw new ApplicationException("Second token not found for indirect reference.");
            // }
            // used = NumberParser.GetInt(bytes, offset, out int gen);
            // if (used == -1)
            // {
            //     throw new ApplicationException("Second token not integer for indirect reference.");
            // }
            // offset += used;
            // offset = CommonUtil.SkipWhiteSpaces(bytes, offset);
            // if (offset == -1)
            // {
            //     throw new ApplicationException("Third token not found for indirect reference.");
            // }
            // if (bytes[offset] != (byte)'R')
            // {
            //     throw new ApplicationException("Indirect reference did not complete with R.");
            // }
            // return new IndirectReference
            // {
            //     ObjectNumber = objNum,
            //     Generation = gen
            // };
        }
        /// <summary>
        /// Gets length of bytes used by an indirect reference.
        /// Should only be used after confirming token type is indirect ref
        /// as this method assumes it is correctly structured.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int CountIndirectRef(ReadOnlySpan<byte> bytes)
        {
            return bytes.IndexOf((byte)'R') + 1;
        }
    }
}