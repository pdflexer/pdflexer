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
        private static readonly PdfDecimalNumber Zero = new PdfDecimalNumber(0);
        public override PdfDecimalNumber Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer[0] == (byte) '+')
            {
                if (buffer.Length == 1) { return Zero;  }
                buffer = buffer.Slice(1);
            } else if (buffer.Length == 1)
            {
                switch (buffer[0])
                {
                    case (byte) '-':
                    case (byte) '0':
                    case (byte) '.':
                        return Zero;
                }
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
