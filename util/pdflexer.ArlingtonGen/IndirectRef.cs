using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen;

internal class IndirectRef
{
    Row Row;

    public IndirectRef(Row row)
    {
        Row = row;
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
        return "false";
    }
}
