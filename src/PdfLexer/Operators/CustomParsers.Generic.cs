using PdfLexer.Content;
using PdfLexer.Lexing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;

public partial class PdfOperator
{
    public delegate IPdfOperation<T>? ParseOp<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>;

    internal static IPdfOperation<T> ParseBDC<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        if (operands.Count == 0)
        {
            ctx.Error("BDC had no operands, falling back to BMC w/ unknown.");
            return new BMC_Op<T>(new PdfName("Unknown"));
        }
        if (operands.Count == 1)
        {
            if (operands[0].Type == PdfTokenType.NameObj)
            {
                ctx.Error("BDC only had single operands.");
                return new BDC_Op<T>(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
            }

            throw new PdfLexerException("DP only had single operand and was not name.");
        }
        if (operands.Count > 2 && (operands[1].Type != PdfTokenType.ArrayStart && operands[1].Type != PdfTokenType.DictionaryStart))
        {
            ctx.Error("BDC had more than two operands, using first two.");
        }
        var di = operands[1];
        var doc = ctx.CurrentSource?.Document;
        ctx.CurrentSource = null; // force no lazy
        if (di.Type != PdfTokenType.ArrayStart || di.Type != PdfTokenType.DictionaryStart)
        {
            return new BDC_Op<T>(ParsePdfName(ctx, data, operands[0]), ctx.GetPdfItem(data, di.StartAt, out _, doc));
        }
        return new BDC_Op<T>(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length, doc));
    }

    internal static DP_Op<T> ParseDP<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        if (operands.Count == 0)
        {
            throw new PdfLexerException("DP had no operands.");
        }
        if (operands.Count == 1)
        {
            if (operands[0].Type == PdfTokenType.NameObj)
            {
                ctx.Error("DP only had single operands.");
                return new DP_Op<T>(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
            }
            throw new PdfLexerException("DP only had single operand and was not name.");
        }
        var di = operands[1];
        var doc = ctx.CurrentSource?.Document;
        ctx.CurrentSource = null; // force no lazy
        return new DP_Op<T>(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length, doc));
    }


    internal static SC_Op<T> ParseSC<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var colors = new List<T>();
        foreach (var val in operands)
        {
            colors.Add(FPC<T>.Util.Parse<T>(ctx, data, val));
        }
        return new SC_Op<T>(colors);
    }

    internal static sc_Op<T> Parsesc<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var colors = new List<T>();
        foreach (var val in operands)
        {
            colors.Add(FPC<T>.Util.Parse<T>(ctx, data, val));
        }
        return new sc_Op<T>(colors);
    }

    internal static scn_Op<T> Parsescn<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        PdfName? name = null;
        var colors = new List<T>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(FPC<T>.Util.Parse<T>(ctx, data, operands[i]));
            }
        }
        return new scn_Op<T>(colors, name);
    }

    internal static SCN_Op<T> ParseSCN<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        PdfName? name = null;
        var colors = new List<T>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(FPC<T>.Util.Parse<T>(ctx, data, operands[i]));
            }
        }
        return new SCN_Op<T>(colors, name);
    }

    internal static Tj_Op<T> ParseTj<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new Tj_Op<T>(text);
    }

    internal static TJ_Op<T> ParseTJ<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var items = new List<TJ_Item<T>>(operands.Count - 2);
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Item<T>
                    {
                        Data = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length)) // TODO don't allocate arrays here, need to 
                                                                                            // find solution for Writing since it won't 
                                                                                            // have access to source data span if we just 
                                                                                            // track refs
                    });
            }
            else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
            {
                items.Add(
                    new TJ_Item<T>
                    {
                        Shift = FPC<T>.Util.Parse<T>(ctx, data, op)
                    });
            }
        }
        return new TJ_Op<T>(items);
    }
    internal static singlequote_Op<T> Parsesinglequote<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new singlequote_Op<T>(text);
    }

    internal static doublequote_Op<T> Parsedoublequote<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var aw = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
        var ac = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
        var op = operands[2];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new doublequote_Op<T>(aw, ac, text);
    }

    internal static d_Op<T> Parsed<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        where T : struct, IFloatingPoint<T>
    {
        var da = ParsePdfArray(ctx, data, operands[0]);
        T dp = T.Zero;
        bool end = false;
        for (var i = 1; i < operands.Count; i++)
        {
            if (end) { dp = FPC<T>.Util.Parse<T>(ctx, data, operands[i]); break; }
            if (operands[i].Type == PdfTokenType.ArrayEnd) { end = true; }
        }
        return new d_Op<T>(da, dp);
    }
}
