using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        if (!VariableContext.InEval && Text[0] == '@' && Text.Substring(1) == VariableContext.VarName) 
        {
            sw.Append(sw.Append(VariableContext.VarSub));
            return;
        }
        if (VariableContext.Vars.TryGetValue(Text, out var value))
        {
            sw.Append(value);
            return;
        }
        if (!VariableContext.InEval && VariableContext.Context?.Type == "name" && Text[0] != '@' && !Text.Contains("*"))
        {
            sw.Append("\"" + Text + "\"");
            return;
        }
        if (int.TryParse(Text, out _))
        {
            sw.Append(Text);
        }
        else if (decimal.TryParse(Text, out _))
        {
            sw.Append(Text + "m");
        }
        else if (Text[0] == '@')
        {
            if (Text.Substring(1) == VariableContext.VarName)
            {
                sw.Append(VariableContext.VarSub);
                return;
            }

            if (int.TryParse(Text.Substring(1), out var i))
            {
                sw.Append("obj.Get(");
                sw.Append(Text.Substring(1));
                sw.Append(")");
            }
            else
            {
                sw.Append(Text);
            }
            // if (Text.Contains("*") || int.TryParse(Text.Substring(1), out _))
            // {
            //     sw.Append(ExpValueWrapper.VarName);
            // } else
            // {
            //     sw.Append(Text);
            // }

        }
        else if (Text[0] == '\'')
        {
            sw.Append("\"" + Text.Trim('\'') + "\"");
        }
        else if (Text == VariableContext.VarName)
        {
            sw.Append(VariableContext.VarSub);
        }
        else if (Text == "true")
        {
            sw.Append("PdfBoolean.True");
        } else if (Text == "false")
        {
            sw.Append("PdfBoolean.False");
        }
        else
        {
            sw.Append("\"" + Text + "\"");
        }
    }

    public IEnumerable<string> GetRequiredValues()
    {
        if (decimal.TryParse(Text, out _))
        {
            yield break;
        }
        if (Text == "true" || Text == "false")
        {
            yield break;
        }

        if (!VariableContext.InEval)
        {
            if (Text[0] == '@' || Text.Contains("::"))
            {
                yield return Text;
            }
        } else
        {
            yield return Text;
        }
        
    }
}