using PdfLexer.Content.Model;
using PdfLexer.Content;
using System.Numerics;

namespace PdfLexer;

public static class GfxStateExtensions
{
    internal static List<IClippingSection<T>> Clip<T>(this List<IClippingSection<T>>? existing, PdfRect<T> rect, PdfRect<T> boundary)
        where T : struct, IFloatingPoint<T>
    {
        var copy = existing == null ? new List<IClippingSection<T>>() : existing.ToList();
        copy.Add(new ClippingInfo<T>(GfxMatrix<T>.Identity, new List<SubPath<T>> {
                    new SubPath<T>
                    {
                        XPos = rect.LLx,
                        YPos = rect.LLy,
                        Operations = new List<IPathCreatingOp<T>>
                            {
                                new re_Op<T>(rect.LLx, rect.LLy, rect.URx-rect.LLx, rect.URy - rect.LLy),
                                new re_Op<T>(boundary.LLx, boundary.LLy, boundary.URx-boundary.LLx, boundary.URy - boundary.LLy),
                            }
                    }
            }, true));
        return copy;
    }

    internal static List<IClippingSection<T>> ClipExcept<T>(this List<IClippingSection<T>>? existing, PdfRect<T> rect) 
        where T : struct, IFloatingPoint<T>
    {
        var copy = existing == null ? new List<IClippingSection<T>>() : existing.ToList();
        copy.Add(new ClippingInfo<T>(GfxMatrix<T>.Identity, new List<SubPath<T>> {
                    new SubPath<T>
                    {
                        XPos = rect.LLx,
                        YPos = rect.LLy,
                        Operations = new List<IPathCreatingOp<T>>
                            {

                                new re_Op<T>(rect.LLx, rect.LLy, rect.URx-rect.LLx, rect.URy - rect.LLy),
                            }
                    }
            }, false));
        return copy;
    }
}
