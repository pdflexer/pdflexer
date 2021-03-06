﻿using System.IO;

namespace PdfLexer
{
    /// <summary>
    /// Pdf bool object type.
    /// Use <see cref="PdfBoolean.True"/> or <see cref="PdfBoolean.False"/> to access values.
    /// </summary>
    public class PdfBoolean : PdfObject
    {
        /// <summary>
        /// Pdf True object
        /// </summary>
        public static PdfBoolean True { get; } = new PdfBoolean(true);
        /// <summary>
        /// Pdf False object
        /// </summary>
        public static PdfBoolean False { get; } = new PdfBoolean(false);
        internal static byte[] TrueBytes = new byte[4] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        internal static byte[] FalseBytes = new byte[5] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

        internal PdfBoolean(bool value)
        {
            Value = value;
        }
        public bool Value { get; }
        public override PdfObjectType Type => PdfObjectType.BooleanObj;
    }
}
