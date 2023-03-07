using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen;

internal class IndirectRef
{
    Row Row;

    public GenBase GenBase { get; }

    public IndirectRef(GenBase gen, Row row)
    {
        Row = row;
        GenBase = gen;
    }

    public string GetIndirectRefEnum()
    {
        // TODO
        switch (Row.IndirectReference)
        {
            case "fn:MustBeDirect()":
                return "IndirectRequirement.MustBeDirect";
            case "TRUE":
                return "IndirectRequirement.MustBeIndirect";
            case "FALSE":
            default:
                return "IndirectRequirement.Either";
        }
    }


    public bool IsSimple()
    {
        switch (Row.IndirectReference)
        {
            case "FALSE":
            case "TRUE":
                return true;
            default:
                return false;
        }
    }

    public bool TryGetSimple(string type, out bool val)
    {
        var types = Row.Type.Split(';').ToList();
        var irs = Row.IndirectReference.Split(";");
        var requiresIndirect = irs.Length == 1 ? irs[0] : irs[types.FindIndex(x=> x.Contains(type))];

        switch (requiresIndirect)
        {
            case "[FALSE]":
            case "FALSE":
                val = false;
                return true;
            case "[TRUE]":
            case "TRUE":
                val = true;
                return true;
            default:
                val = false;
                return false;
        }
    }

    public string GetComplex(string type)
    {
        var types = Row.Type.Split(';').ToList();
        var irs = Row.IndirectReference.Split(";");
        var requiresIndirect = irs.Length == 1 ? irs[0] : irs[types.FindIndex(x => x.Contains(type))];

        if (requiresIndirect == "fn:MustBeDirect()")
        {
            return $$"""
if (wasIR) {
    ctx.Fail<APM_{{GenBase.RootName}}_{{GenBase.Key}}>("{{Row.Key}} is required to be direct when a {{type}}");
    return;
}
""";
        }

        bool mustBeDirect = false;
        if (requiresIndirect.StartsWith("fn:MustBeDirect"))
        {
            requiresIndirect = requiresIndirect[15..^1];
            mustBeDirect = true;
        } else if (requiresIndirect.StartsWith("fn:MustBeIndirect"))
        {
            requiresIndirect = requiresIndirect[18..^1];
            mustBeDirect = false;
        } else
        {
            throw new ApplicationException("Unkonwn IR: " + requiresIndirect);
        }

        var exp = new Exp(requiresIndirect);
        var vars = exp.GetRequiredValues().Distinct();
        var defs = string.Join('\n', vars.Select(v => GenBase.GetSetter(v, type, "val")));

        return $$"""
{{defs}}
if ({{exp.GetText()}} && {{(mustBeDirect ? "wasIr" : "!wasIr")}}) {
    ctx.Fail<APM_{{GenBase.RootName}}_{{GenBase.Key}}>("{{Row.Key}} is required to be {{(mustBeDirect ? "direct" : "indirect")}} when a {{type}}");
    return;
}
""";
    }
}
