using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PdfLexer.Parsers
{
    public class DecimalParser : Parser<PdfDecimalNumber>
    {
        public override PdfDecimalNumber Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer[0] == (byte) '+')
            {
                buffer = buffer.Slice(1);
            }
            return new PdfDecimalNumber(GetDecimal(buffer));
        }

        private decimal GetDecimal(ReadOnlySpan<byte> buffer)
        {
            if (!Utf8Parser.TryParse(buffer, out decimal val, out int consumed))
            {
                throw new ApplicationException("Bad data for double number: " + Encoding.ASCII.GetString(buffer));
            }
            Debug.Assert(consumed == buffer.Length, "consumed == buffer.Length for double");

            return val;
        }
    }
}
