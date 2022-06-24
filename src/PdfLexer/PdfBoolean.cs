﻿using System.IO;

namespace PdfLexer
{
    public class PdfBoolean : IPdfObject
    {
        public static PdfBoolean True { get; } = new PdfBoolean(true);
        public static PdfBoolean False { get; } = new PdfBoolean(false);
        internal static byte[] TrueBytes = new byte[4] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        internal static byte[] FalseBytes = new byte[5] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

        public PdfBoolean(bool value)
        {
            Value = value;
        }
        public bool Value { get; }
        public bool IsIndirect => false;
        public PdfObjectType Type => PdfObjectType.BooleanObj;

    }
}