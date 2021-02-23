﻿using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class NameSerializer : ISerializer<PdfName>
    {
        // private bool NeedsEscaping(PdfName obj)
        // {
        //     bool escapeNeeded = false;
        //     if (obj.NeedsEscaping == null)
        //     {
        //         if (obj.Value.IndexOfAny(needsEscaping) > -1)
        //         {
        //             escapeNeeded = true;
        //         }
        //     } else
        //     {
        //         escapeNeeded = obj.NeedsEscaping.Value;
        //     }
        // 
        //     return escapeNeeded;
        // }

        public void WriteToStream(PdfName obj, Stream stream)
        {
            // TODO well known values or grab from cache since we are saving key
            var buffer = ArrayPool<byte>.Shared.Rent(obj.Value.Length*3);
            var written = GetBytes(obj, buffer);
            stream.Write(buffer, 0, written);
            ArrayPool<byte>.Shared.Return(buffer);
            return;
        }

        public int GetBytes(PdfName obj, Span<byte> data)
        {
            data[0] = (byte)'/';
            var ci = 1; // TODO perf analysis
            for (var i=1;i<obj.Value.Length;i++)
            {
                var cc = obj.Value[i];
                if (cc == (char) 0 || cc ==  (char) 9 || cc ==  (char) 10 || cc == (char) 12 
                    || cc == (char) 13 || cc == (char) 32 || cc =='(' || cc == ')' || cc == '<' 
                    || cc == '>' || cc == '[' || cc == ']' || cc == '{' || cc == '}' || cc == '/' 
                    || cc == '%' || cc == '#')
                {
                    data[ci++] = (byte) '#';
                    var hex = ((int)cc).ToString("X2"); // TODO can make this better as well
                    data[ci++] = (byte) hex[0];
                    data[ci++] = (byte) hex[1];
                } else
                {
                    data[ci++] = (byte) cc;
                }
            }

            return ci;
        }

        private static char[] needsEscaping = new char[]
        {
            (char) 0, (char) 9, (char) 10, (char) 12, (char) 13, (char) 32,
            '(', ')', '<', '>', '[', ']', '{', '}', '/', '%', '#'
        };        
    }
}
