﻿using System;
using System.Buffers.Text;
using System.IO;
using System.Text;
using PdfLexer.Parsers;

namespace PdfLexer.Serializers
{
    public class NumberSerializer : ISerializer<PdfNumber>
    {
        private ParsingContext _ctx;

        public NumberSerializer(ParsingContext ctx)
        {
            _ctx = ctx;
        }
        public void WriteToStream(PdfNumber obj, Stream stream)
        {
            var count = GetBytes(obj, _ctx.Buffer);
            stream.Write(_ctx.Buffer, 0, count);
        }

        public int GetBytes(PdfNumber obj, Span<byte> data)
        {
            if (obj is PdfIntNumber integer)
            {
                if (!Utf8Formatter.TryFormat(integer.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber integer: " + Encoding.ASCII.GetString(data));
                }

                return written;
            } else if (obj is PdfDecimalNumber dn)
            {
                if (!Utf8Formatter.TryFormat(dn.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber decimal: " + Encoding.ASCII.GetString(data));
                }
                return written;
            } else if (obj is PdfLongNumber ln)
            {
                if (!Utf8Formatter.TryFormat(ln.Value, data, out var written))
                {
                    throw new ApplicationException("Unable for write PdfNumber long: " + Encoding.ASCII.GetString(data));
                }
                return written;
            }
            
            throw new NotImplementedException("Unknown number type: " + obj.GetType());
        }
    }
}