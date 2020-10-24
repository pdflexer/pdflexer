using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Objects.Parsers
{
    public class StringParser
    {
        public static bool GetString(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {
            if (data[0] == (byte)'(')
            {
                return GetStringLiteral(data, out results);
            }
            else if (data[0] == (byte)'<')
            {
                return GetStringHex(data, out results);
            }
            else
            {
                throw new ApplicationException("Invalid string, first char not ( or <.");
            }
        }
        private static bool GetStringLiteral(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {
            int depth = 1;
            int end = -1;
            for (var i = 1; i < data.Length; i++)
            {
                if (data[i] == '(' && data[i - 1] != '\\')
                {
                    depth++;
                }
                else if (data[i] == ')' && data[i - 1] != '\\')
                {
                    depth--;
                }
                if (depth == 0)
                {
                    end = i + 1;
                    break;
                }
            }
            if (end == -1)
            {
                if (depth != 0)
                {
                    results = null;
                    return false;
                }
                end = data.Length;
            }
            results = data.Slice(0, end);
            return true;
        }
        private static bool GetStringHex(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {
            var loc = data.IndexOf((byte)'>');
            if (loc == -1)
            {
                results = null;
                return false;
            }
            results = data.Slice(0, loc + 1);
            return true;
        }

        public static int ParseString(ReadOnlySpan<byte> data, out Span<char> results)
        {
            if (data[0] == (byte)'(')
            {
                return ParseStringLiteral(data, out results);
            }
            else if (data[0] == (byte)'<')
            {
                // hex bytes
                throw new NotImplementedException("Hex string todo");
            }
            else
            {
                throw new ApplicationException("Invalid string, first char not ( or <.");
            }
        }

        private static int ParseStringLiteral(ReadOnlySpan<byte> data, out Span<char> results)
        {
            int depth = 1;
            int end = -1;
            List<int> escapes = null;
            for (var i = 1; i < data.Length; i++)
            {
                if (data[i] == '(' && data[i - 1] != '\\')
                {
                    depth++;
                }
                else if (data[i] == ')' && data[i - 1] != '\\')
                {
                    depth--;
                }
                else if (data[i] == '\\' && data[i - 1] != '\\')
                {
                    if (escapes == null)
                    {
                        escapes = new List<int>();
                    }
                    escapes.Add(i);
                }
                if (depth == 0)
                {
                    end = i;
                    break;
                }
            }
            if (end == -1)
            {
                if (depth != 0)
                {
                    throw new ApplicationException("Unbalanced string literal.");
                }
                end = data.Length - 1;
            }
            results = new Span<char>(new char[end - 1]);
            Encoding.ASCII.GetChars(data.Slice(1, end - 1), results);
            // ReplaceEspacedChars(results, out results);
            return end + 1;
        }
        private static char[] numeric = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        // private static void ReplaceEspacedChars(Span<char> chars, out Span<char> results)
        // {
        //     var espace = chars.IndexOf('\\');
        //     if (espace == -1)
        //     {
        //         results = chars;
        //     } else
        //     {
        //         int pos = 0;
        //         var search = chars;
        //         switch (search[pos+1])
        //         {
        //             case 'n':
        //                 search[pos] = '\n';
        //                 break;
        //             case 'r':
        //                 search[pos] = '\r';
        //                 break;
        //             case 't':
        //                 search[pos] = '\t';
        //                 break;
        //             case 'b':
        //                 search[pos] = '\b';
        //                 break;
        //             case 'f':
        //                 search[pos] = '\f';
        //                 break;
        //             case '(':
        //                 search[pos] = '(';
        //                 break;
        //             case ')':
        //                 search[pos] = ')';
        //                 break;
        //             case '\\':
        //                 search[pos] = '\\';
        //                 break;
        //             default:
        // 
        //         }
        //         search = chars.Slice(pos)
        //         results = null;
        //     }
        // }
    }
}