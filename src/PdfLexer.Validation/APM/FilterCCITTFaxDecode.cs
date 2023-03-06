// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FilterCCITTFaxDecode : APM_FilterCCITTFaxDecode__Base
{
}

internal partial class APM_FilterCCITTFaxDecode__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FilterCCITTFaxDecode";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FilterCCITTFaxDecode_K, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_EndOfLine, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_EncodedByteAlign, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_Columns, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_Rows, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_EndOfBlock, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_BlackIs1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_DamagedRowsBeforeError, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterCCITTFaxDecode_Blackls1, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FilterCCITTFaxDecode>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "K", "EndOfLine", "EncodedByteAlign", "Columns", "Rows", "EndOfBlock", "BlackIs1", "DamagedRowsBeforeError", "Blackls1"
    };
    


}

/// <summary>
/// FilterCCITTFaxDecode_K Table 11
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_K : APM_FilterCCITTFaxDecode_K__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_K__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_K";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterCCITTFaxDecode_K>(obj, "K", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_EndOfLine 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_EndOfLine : APM_FilterCCITTFaxDecode_EndOfLine__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_EndOfLine__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_EndOfLine";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FilterCCITTFaxDecode_EndOfLine>(obj, "EndOfLine", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_EncodedByteAlign 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_EncodedByteAlign : APM_FilterCCITTFaxDecode_EncodedByteAlign__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_EncodedByteAlign__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_EncodedByteAlign";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FilterCCITTFaxDecode_EncodedByteAlign>(obj, "EncodedByteAlign", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_Columns 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_Columns : APM_FilterCCITTFaxDecode_Columns__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_Columns__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_Columns";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterCCITTFaxDecode_Columns>(obj, "Columns", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Columns = obj.Get("Columns");
        if (!(gte(Columns,0))) 
        {
            ctx.Fail<APM_FilterCCITTFaxDecode_Columns>($"Invalid value {val}, allowed are: [fn:Eval(@Columns>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_Rows 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_Rows : APM_FilterCCITTFaxDecode_Rows__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_Rows__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_Rows";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterCCITTFaxDecode_Rows>(obj, "Rows", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Rows = obj.Get("Rows");
        if (!(gte(Rows,0))) 
        {
            ctx.Fail<APM_FilterCCITTFaxDecode_Rows>($"Invalid value {val}, allowed are: [fn:Eval(@Rows>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_EndOfBlock 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_EndOfBlock : APM_FilterCCITTFaxDecode_EndOfBlock__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_EndOfBlock__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_EndOfBlock";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FilterCCITTFaxDecode_EndOfBlock>(obj, "EndOfBlock", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_BlackIs1 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_BlackIs1 : APM_FilterCCITTFaxDecode_BlackIs1__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_BlackIs1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_BlackIs1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FilterCCITTFaxDecode_BlackIs1>(obj, "BlackIs1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_DamagedRowsBeforeError 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_DamagedRowsBeforeError : APM_FilterCCITTFaxDecode_DamagedRowsBeforeError__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_DamagedRowsBeforeError__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_DamagedRowsBeforeError";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterCCITTFaxDecode_DamagedRowsBeforeError>(obj, "DamagedRowsBeforeError", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        
        var DamagedRowsBeforeError = obj.Get("DamagedRowsBeforeError");
        if (!(gte(DamagedRowsBeforeError,0))) 
        {
            ctx.Fail<APM_FilterCCITTFaxDecode_DamagedRowsBeforeError>($"Invalid value {val}, allowed are: [fn:Eval(@DamagedRowsBeforeError>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterCCITTFaxDecode_Blackls1 
/// </summary>
internal partial class APM_FilterCCITTFaxDecode_Blackls1 : APM_FilterCCITTFaxDecode_Blackls1__Base
{
}


internal partial class APM_FilterCCITTFaxDecode_Blackls1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterCCITTFaxDecode_Blackls1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FilterCCITTFaxDecode_Blackls1>(obj, "Blackls1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

