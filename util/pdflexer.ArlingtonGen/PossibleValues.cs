using DotNext;
using pdflexer.ArlingtonGen.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DotNext.Generic.BooleanConst;

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

        var head = "";

        if (hasArrayVal.IsMatch(value))
        {
            var prev = VariableContext.VarSub;
            VariableContext.VarSub = "v";
            head = "IPdfObject v = " + prev + ";";
        } else if (value == "*")
        {

        }


        var defs = string.Join('\n', vars.Select(v => GetSetter(v, type)));
        var values = SplitWithFns(value.TrimStart('[').TrimEnd(']'));
        var sb = new StringBuilder();
        var nr = values.Where(v => !v.Contains("RequiredValue")).ToList();
        for (var p = 0; p < nr.Count; p++)
        {
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
        if (type == "name" || type == "number" || type == "integer")
        {

            return $$"""
{
{{head}}
{{defs}}
if (!({{checks}})) 
{
    ctx.Fail<APM_{{Gen.RootName}}_{{Gen.Key}}>($"Invalid value {{{VarName}}}, allowed are: {{value}}");
}
}
""";
        }
        return "// TODO value checks " + type;
    }

    private string GetSetter(string value, string type)
    {
        var typeDeclaration = type == "number" || type == "integer" ? "IPdfObject" : "var";
        var nm = value[0] == '@' ? value.Substring(1) : value;
        if (nm == Row.Key) 
        {
            if (badVar.IsMatch(value))
            {
                var nv = value.Replace("@", "x").Replace("*", "");
                VariableContext.VarSub = nv;
                return $"{typeDeclaration} {nv} = {VarName};";
            }
            else
            {
                VariableContext.VarSub = value;
                return $"{typeDeclaration} {value} = {VarName};";
            }
        }
        // return "";
        if (int.TryParse(nm, out _))
        {
            return "";
        }

        var cleansed = Regex.Replace(value, @"[^A-Za-z0-9]+", "");
        int? i = null;
        if (VariableContext.Vars.ContainsValue(cleansed + i?.ToString()))
        {
            if (i == null) { i = 2; } else { i++; }
        }
        var clean = cleansed + i?.ToString();
        VariableContext.Vars[value] = clean;

        return $"var {clean} = obj.Get(\"{nm}\");";
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
        foreach (var item in Exp.Tokenize(value.TrimStart('[').TrimEnd(']')))
        {
            foreach (var nm in item.GetRequiredValues())
            {
                results.Add(nm);
            }
        }
        return;
        // var r = GenBase.FuncRegex.Match(value);
        // if (!r.Success)
        // {
        //     return;
        // }


        var result = GenBase.ValueRegex.Matches(value); //r.Groups[2].Value);
        foreach (var v in result)
        {
            var mm = (Match)v;
            results.Add(mm.Value);
        }

    }

    private string Unwrap(string value)
    {
        value = GenBase.RepeatedVarRegex.Replace(value, VarName);
        var r = GenBase.AnyFuncRegex.Matches(value);
        if (r.Count == 0)
        {
            if (decimal.TryParse(value, out _))
            {
                return $"{VarName} == {value}";
            }
            return $"{VarName} == \"{value}\"";
        }

        foreach (var func in r)
        {
            var m = (Match)func;
            var v = m.Groups[2].Value;
            var i = m.Groups[2].Value.IndexOf(',');
            var val = v;
            if (i > -1)
            {
                v = m.Groups[2].Value.Substring(0, i);
                val = m.Groups[2].Value.Substring(i + 1, m.Groups[2].Value.Length - i - 1);
            }

            switch (m.Groups[1].Value)
            {
                case "Deprecated":
                    {
                        value = value.Replace(m.Value, $"(ctx.Version < {v}m && ({Unwrap(val)}))");
                        break;
                    }
                case "SinceVersion":
                    {
                        value = value.Replace(m.Value, $"(ctx.Version >= {v}m && ({Unwrap(val)}))");
                        break;
                    }
                case "Extension":
                    {
                        if (i == -1)
                        {
                            value = value.Replace(m.Value, $"(ctx.Extensions.Contains(\"{v}\")");
                        }
                        else
                        {
                            value = value.Replace(m.Value, $"(ctx.Extensions.Contains(\"{v}\") && ({Unwrap(val)}))");
                        }
                        break;
                    }
                case "Eval":
                    {
                        foreach (var stmt in GenBase.ValueOpRegex.Matches(val).GroupBy(v => v.Value))
                        {
                            var mm = stmt.First();
                            val = val.Replace(m.Value, $"(has{mm.Groups[1].Value} && (" + mm.Value + "))");
                        }
                        val = val.Replace(" mod ", " % ");
                        // var result = GenBase.ValueRegex.Matches(val);
                        // var vars = result.Select(m => m.Value).Distinct().ToList();
                        // foreach (var m in vars)
                        // {
                        //     var nm = m.Substring(1);
                        //     val = val.Replace(m, $"has{nm} && {m}");
                        // }
                        if (GenBase.AnyFuncRegex.IsMatch(val))
                        {
                            var matches = GenBase.AnyFuncRegex.Matches(val);
                            var fns = matches.Select(v => v.Value).Distinct().ToList();
                            foreach (var fn in fns)
                            {
                                val = val.Replace(fn, Unwrap(fn));
                            }
                        }
                        value = value.Replace(m.Value, $"({val})");
                        break;
                    }
                case "ArrayLength":
                    value = value.Replace(m.Value, "0");
                    break;
                default:
                    Console.WriteLine(value);
                    return "false";
            }
        }



        return value;
    }

}
