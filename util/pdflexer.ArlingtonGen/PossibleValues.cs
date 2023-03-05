using pdflexer.ArlingtonGen.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace pdflexer.ArlingtonGen;

internal class PossibleValues
{
    Row Row;

    public GenBase Gen { get; }
    public string VarName { get; private set; }

    public PossibleValues(GenBase gen, string varName = "val")
    {
        Row = gen.Row;
        Gen = gen;
        VarName = varName;
        VariableContext.VarName = Row.Key;
        VariableContext.VarSub = varName;
        VariableContext.Context = gen.Row;
    }

    private static Regex badVar = new Regex("@[0-9\\*]+");

    public string GetPossibleValueCheck()
    {
        if (string.IsNullOrEmpty(Row.PossibleValues))
        {
            return "// no value restrictions";
        }

        return SingleValue(Row.Type, Row.PossibleValues);
    }

    public string GetPossibleValueCheck(string type)
    {
        var types = Row.Type.Split(';').ToList();
        var pv = Row.PossibleValues.Split(";");
        var possible = pv.Length == 1 ? pv[0] : pv[types.IndexOf(type)];
        if (string.IsNullOrEmpty(possible) || possible.Trim() == "[]")
        {
            return "// no value restrictions";
        }

        return SingleValue(type, possible);
    }

    private static Regex hasArrayVal = new Regex("@[0-9]+");

    private string SingleValue(string type, string value)
    {
        var vars = new List<string>();
        GetVals(value, vars);
        vars = vars.Distinct().ToList();

        var values = SplitWithFns(value.TrimStart('[').TrimEnd(']'));
        if (values.Any(x=> x.Trim() == "*"))
        {
            return "// no value restictions";
        }

        var head = "";

        if (hasArrayVal.IsMatch(value))
        {
            var prev = VariableContext.VarSub;
            VariableContext.VarSub = "v";
            VariableContext.Vars[Row.Key] = "v";
            VariableContext.Vars["@" + Row.Key] = "v";
            head = "IPdfObject v = " + prev + ";";
        }

        if (type == "string-text")
        {
            var prev = VariableContext.VarSub;
            VariableContext.VarSub = VariableContext.VarSub + ".Value";
        }

        var defs = string.Join('\n', vars.Select(v => Gen.GetSetter(v, type, "val")));

        var sb = new StringBuilder();
        var rv = values.Where(v => v.Contains("RequiredValue")).ToList();
        if (rv.Any())
        {
            defs += "\n// TODO required value checks";
        }
        var nr = values.Where(v => !v.Contains("RequiredValue")).ToList();
        for (var p = 0; p < nr.Count; p++)
        {
            VariableContext.InEval = false;
            VariableContext.Wrapper = VariableContext.ValWrapper;
            var parts = Exp.Tokenize(nr[p]);
            for (var i = 0; i < parts.Count; i++)
            {
                parts[i].Write(sb);
                if (i < parts.Count - 1)
                {
                    sb.Append(" || ");
                }
            }

            if (p < nr.Count - 1)
            {
                sb.Append(" || ");
            }
        }


        var checks = sb.ToString();
        //var checks = string.Join(" || ", values.Select(x => Unwrap(x)));
        if (type == "name" || type == "number" || type == "integer" || type == "string-text")
        {

            return $$"""
{{head}}
{{defs}}
if (!({{checks}})) 
{
    ctx.Fail<APM_{{Gen.RootName}}_{{Gen.Key}}>($"Invalid value {{{VarName}}}, allowed are: {{value}}");
}
""";
        }
        return "// TODO value checks " + type;
    }

    public static List<string> SplitWithFns(string text)
    {
        var parts = new List<string>();
        int d = 0;
        int l = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '(')
            {
                d++;
            }
            else if (c == ')')
            {
                d--;
            }
            else if (d == 0 && c == ',')
            {
                parts.Add(text.Substring(l, i - l));
                l = i + 1;
            }
        }
        parts.Add(text.Substring(l, text.Length - l));
        return parts;
    }

    private void GetVals(string value, List<string> results)
    {
        
        foreach (var item in SplitWithFns(value.TrimStart('[').TrimEnd(']')).Select(x=> new Exp(x)))
        {
            foreach (var nm in item.GetRequiredValues())
            {
                results.Add(nm);
            }
        }
        return;
    }
}
