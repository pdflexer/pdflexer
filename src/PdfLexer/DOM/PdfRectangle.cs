using PdfLexer.Content;
using System.Numerics;

namespace PdfLexer.DOM
{
    public class PdfRectangle
    {
        internal PdfArray NativeObject { get; }

        public PdfRectangle() : this(new PdfArray())
        { }

        public PdfRectangle(PdfArray array)
        {
            NativeObject = array;
            var c = 4 - NativeObject.Count;
            if (c < 1) { return; }
            for (; c > 0; c--)
            {
                NativeObject.Add(PdfCommonNumbers.Zero);
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
            get => new PdfDecimalNumber((decimal)URx - (decimal)LLx);
        }

        public PdfNumber Height
        {
            get => new PdfDecimalNumber((decimal)URy - (decimal)LLy);
        }

        private PdfNumber GetValue(int i)
        {
            if (NativeObject[i] is PdfNumber n)
            {
                return n;
            }
            var nn = PdfCommonNumbers.Zero;
            NativeObject[i] = nn;
            return nn;
        }

        private void SetValue(int i, PdfNumber value)
        {
            NativeObject[i] = value;
        }

        [return: NotNullIfNotNull("array")]
        public static implicit operator PdfRectangle?(PdfArray? array) => array == null ? null : new PdfRectangle(array);
        [return: NotNullIfNotNull("rect")]
        public static implicit operator PdfArray?(PdfRectangle? rect) => rect?.NativeObject;

        public static PdfRectangle Zeros { get; } = new PdfRectangle();

        public PdfRectangle CloneShallow() => NativeObject.CloneShallow()!;

        public PdfRect<T> ToContentModel<T>() where T : struct, IFloatingPoint<T>
        {
            return new PdfRect<T>
            {
                LLx = FPC<T>.Util.FromDouble<T>(LLx),
                URx = FPC<T>.Util.FromDouble<T>(URx),
                LLy = FPC<T>.Util.FromDouble<T>(LLy),
                URy = FPC<T>.Util.FromDouble<T>(URy),
            };
        }

        public static PdfRectangle FromContentModel<T>(PdfRect<T> rect) where T : struct, IFloatingPoint<T>
        {
            return new PdfRectangle
            {
                LLx = FPC<T>.Util.ToDecimal<T>(rect.LLx),
                URx = FPC<T>.Util.ToDecimal<T>(rect.URx),
                LLy = FPC<T>.Util.ToDecimal<T>(rect.LLy),
                URy = FPC<T>.Util.ToDecimal<T>(rect.URy),
            };
        }

        public override string ToString()
        {
            return $"[{LLx} {LLy} {URx} {URy}]";
        }
    }
}
