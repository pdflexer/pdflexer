using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen;

internal class SpecialCase
{
    Row Row;

    public SpecialCase(Row row)
    {
        Row = row;
    }

    public string GetSpecialCase()
    {
        if (string.IsNullOrEmpty(Row.SpecialCase))
        {
            return "// no special cases";
        }

        return "// TODO special case";
    }

    public string GetSpecialCase(string type)
    {
        var types = Row.Type.Split(';').ToList();
        var sc = Row.SpecialCase.Split(";");
        var special = sc.Length == 1 ? sc[0] : sc[types.IndexOf(type)];
        if (string.IsNullOrEmpty(special) || special.Trim() == "[]")
        {
            return "// no special cases";
        }

        return "// TODO special case";
    }
}
