using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace pdflexer.ArlingtonGen.Expressions;



internal class EValue : INode
{
    public List<INode> Children { get; } = new List<INode>();

    public string Text;
    public EValue(string text)
    {
        Text = text;
    }

    public void Write(StringBuilder sw)
    {
        VariableContext.Wrapper(sw, this, WriteInternal);
    }

    private void WriteInternal(StringBuilder sw)
    {
        if (VariableContext.Handling == VariableHandling.MustBeObj)
        {
            WriteObj(Text);
        }
        else if (VariableContext.Handling == VariableHandling.MustBeVal)
        {
            WriteVal(sw, Text);
        } else
        {
            if (Text[0] == '@' || Text.Contains("::"))
            {
                if (VariableContext.Vars.TryGetValue(Text, out var value))
                {
                    sw.Append(value);
                    return;
                }
                WriteObj(Text);
            } else
            {
                WriteVal(sw, Text);
            }
        }

        void WriteObj(string val)
        {
            if (val[0] == '@' && val.Substring(1) == VariableContext.VarName)
            {
                sw.Append(VariableContext.VarSub);
                return;
            }
            if (VariableContext.Vars.TryGetValue(val, out var value))
            {
                sw.Append(value);
                return;
            }
            if (int.TryParse(val.Substring(1), out var i))
            {
                sw.Append("obj.Get(");
                sw.Append(Text.Substring(1));
                sw.Append(")");
            }
            else
            {
                sw.Append(val);
            }
        }


    }

    public static void WriteVal(StringBuilder sw, string val)
    {
        if (!VariableContext.InEval)
        {
            switch (VariableContext.CurrentType)
            {
                case NonEvalType.String:
                    val = val.Trim('\'');
                    sw.Append("\"" + val + "\"");
                    return;
                case NonEvalType.Name:
                    if (val == "true") { sw.Append("\"true\""); return; }
                    if (val == "false") { sw.Append("\"false\""); return; }
                    var nm = val.Replace("-", "").Replace(".", "");
                    if (char.IsNumber(nm[0]))
                    {
                        nm = "N" + nm;
                    }
                    sw.Append("PdfName." + nm);
                    PdfNames.Add($"public static readonly PdfName {nm} = new (\"{val}\", false);");
                    return;
                case NonEvalType.Number:
                    if (decimal.TryParse(val, out _))
                    {
                        sw.Append(val + "m");
                    }
                    else
                    {
                        sw.Append(val);
                    }
                    break;
            }
            return;
        }
        if (int.TryParse(val, out _))
        {
            sw.Append(val);
        }
        else if (decimal.TryParse(val, out _))
        {
            if (!VariableContext.InEval)
            {
                sw.Append("\"" + val + "\"");
            }
            else
            {
                sw.Append(val + "m");
            }

        }
        else if (val[0] == '\'')
        {
            sw.Append("\"" + val.Trim('\'') + "\"");
        }
        else if (val == VariableContext.VarName)
        {
            sw.Append(VariableContext.VarSub);
        }
        else if (val == "true")
        {
            sw.Append("PdfBoolean.True");
        }
        else if (val == "false")
        {
            sw.Append("PdfBoolean.False");
        }
        else
        {
            if (val == "true") { sw.Append("\"true\""); return; }
            if (val == "false") { sw.Append("\"false\""); return; }
            var nm = val.Replace("-", "").Replace(".", "");
            if (char.IsNumber(nm[0]))
            {
                nm = "N" + nm;
            }
            sw.Append("PdfName." + nm);
            PdfNames.Add($"public static readonly PdfName {nm} = new (\"{val}\", false);");
            // sw.Append("\"" +     z + "\"");
        }
        return;
    }

    public static HashSet<string> PdfNames = new HashSet<string>();
    public IEnumerable<string> GetRequiredValues()
    {
        if (VariableContext.Handling == VariableHandling.MustBeObj)
        {
            yield return Text;
        }
        else if (VariableContext.Handling == VariableHandling.MustBeVal)
        {
            yield break;
        }
        else
        {
            if (decimal.TryParse(Text, out _))
            {
                yield break;
            }
            if (Text == "true" || Text == "false")
            {
                yield break;
            }

            if (Text[0] == '@' || Text.Contains("::"))
            {
                yield return Text;
            }
        }
    }
    public bool IsSingleValue()
    {
        return true;
    }
}