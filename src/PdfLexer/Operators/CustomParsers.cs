using PdfLexer.Content;
using PdfLexer.Lexing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;

public partial class PdfOperator
{
    
    public delegate IPdfOperation? ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands);

    internal static IPdfOperation ParseBDC(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        if (operands.Count == 0)
        {
            ctx.Error("BDC had no operands, falling back to BMC w/ unknown.");
            return new BMC_Op(new PdfName("Unknown"));
        }
        if (operands.Count == 1)
        {
            if (operands[0].Type == PdfTokenType.NameObj)
            {
                ctx.Error("BDC only had single operands.");
                return new BDC_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
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
            return new BDC_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetPdfItem(data, di.StartAt, out _, doc));
        }
        return new BDC_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length, doc));
    }

    internal static DP_Op ParseDP(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
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
                return new DP_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
            }
            throw new PdfLexerException("DP only had single operand and was not name.");
        }
        var di = operands[1];
        var doc = ctx.CurrentSource?.Document;
        ctx.CurrentSource = null; // force no lazy
        return new DP_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length, doc));
    }


    internal static SC_Op ParseSC(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var colors = new List<double>();
        foreach (var val in operands)
        {
            colors.Add(ParseDouble(ctx, data, val));
        }
        return new SC_Op(colors);
    }

    internal static sc_Op Parsesc(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var colors = new List<double>();
        foreach (var val in operands)
        {
            colors.Add(ParseDouble(ctx, data, val));
        }
        return new sc_Op(colors);
    }

    internal static scn_Op Parsescn(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        PdfName? name = null;
        var colors = new List<double>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(ParseDouble(ctx, data, operands[i]));
            }
        }
        return new scn_Op(colors, name);
    }

    internal static SCN_Op ParseSCN(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        PdfName? name = null;
        var colors = new List<double>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(ParseDouble(ctx, data, operands[i]));
            }
        }
        return new SCN_Op(colors, name);
    }

    internal static Tj_Op ParseTj(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new Tj_Op(text);
    }

    internal static TJ_Op ParseTJ(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var items = new List<TJ_Item<double>>(operands.Count - 2);
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Item<double>
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
                    new TJ_Item<double>
                    {
                        Shift = (double)ParseDecimal(ctx, data, op) // TODO
                    });
            }
        }
        return new TJ_Op(items);
    }

    internal static void ParseTJLazy(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands, List<TJ_Lazy_Item> items)
    {
        int i = 0;
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Lazy_Item
                    {
                        OpNum = i
                    });
            }
            else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
            {
                items.Add(
                    new TJ_Lazy_Item
                    {
                        Shift = (double)ParseDecimal(ctx, data, op),
                        OpNum = -1
                    });
            }
            i++;
        }
    }

    internal static void ParseTJLazy<T>(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands, List<TJ_Lazy_Item<T>> items) 
        where T: struct, IFloatingPoint<T>
    {
        int i = 0;
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Lazy_Item<T>
                    {
                        OpNum = i
                    });
            }
            else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
            {
                items.Add(
                    new TJ_Lazy_Item<T>
                    {
                        Shift = FPC<T>.Util.Parse<T>(ctx, data, op),
                        OpNum = -1
                    });
            }
            i++;
        }
    }

    internal static singlequote_Op Parsesinglequote(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new singlequote_Op(text);
    }

    internal static doublequote_Op Parsedoublequote(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var aw = ParseDouble(ctx, data, operands[0]);
        var ac = ParseDouble(ctx, data, operands[1]);
        var op = operands[2];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new doublequote_Op(aw, ac, text);
    }

    internal static d_Op Parsed(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var da = ParsePdfArray(ctx, data, operands[0]);
        double dp = 0;
        bool end = false;
        for (var i = 1; i < operands.Count; i++)
        {
            if (end) { dp = ParseDouble(ctx, data, operands[i]); break; }
            if (operands[i].Type == PdfTokenType.ArrayEnd) { end = true; }
        }
        return new d_Op(da, dp);
    }
}
