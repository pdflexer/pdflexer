using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfRectangle
    {
        internal PdfArray Array { get; }

        public PdfRectangle() : this(new PdfArray())
        { }

        public PdfRectangle(PdfArray array)
        {
            Array = array;
            var c = 4 - Array.Count;
            if (c<1){ return; }
            for (;c>0;c--)
            {
                Array.Add(PdfCommonNumbers.Zero);
            }
        }

        public PdfNumber LLx
        {
            get => GetValue(0);
            set => SetValue(0, value);
        }

        public PdfNumber LLy
        {
            get => GetValue(1);
            set => SetValue(1, value);
        }

        public PdfNumber URx
        {
            get => GetValue(2);
            set => SetValue(2, value);
        }

        public PdfNumber URy
        {
            get => GetValue(3);
            set => SetValue(3, value);
        }

        public PdfNumber Width
        {
            get => URx - LLx;
        }

        public PdfNumber Height
        {
            get => URy - LLy;
        }

        private PdfNumber GetValue(int i)
        {
            if (Array[i] is PdfNumber n)
            {
                return n;
            }
            var nn = PdfCommonNumbers.Zero;
            Array[i] = nn;
            return nn;
        }

        private void SetValue(int i, PdfNumber value)
        {
            Array[i] = value;
        }

        public static implicit operator PdfRectangle(PdfArray array) => array == null ? null : new PdfRectangle(array);
        public static implicit operator PdfArray(PdfRectangle rect) => rect?.Array;

        public static PdfRectangle Zeros { get; } = new PdfRectangle();

        public PdfRectangle CloneShallow() => Array.CloneShallow();
    }
}
