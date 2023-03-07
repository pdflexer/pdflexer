using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EGroup : INode
{
    public EGroup(List<INode> children)
    {
        Children = children;
    }

    public List<INode> Children { get; }

    public void Write(StringBuilder sw)
    {
        if (Children.Count == 3 && Children[1] is EOp op)
        {
            if (op.Text == "==")
            {
                sw.Append("eq");
            }
            else if (op.Text == "!=")
            {
                sw.Append("!eq");
            }
            else if (op.Text == "<")
            {
                sw.Append("lt");
            }
            else if (op.Text == ">")
            {
                sw.Append("gt");
            }
            else if (op.Text == "<=")
            {
                sw.Append("lte");
            }
            else if (op.Text == ">=")
            {
                sw.Append("gte");
            }
            else if (op.Text == "%")
            {
                sw.Append("mod");
            }
            else if (op.Text == "+")
            {
                sw.Append("plus");
            }
            else if (op.Text == "*")
            {
                sw.Append("mult");
            }
            else
            {
                sw.Append("(");
                foreach (var part in Children)
                {
                    part.Write(sw);
                }
                sw.Append(")");
                return;
            }
            sw.Append("(");
            Children[0].Write(sw);
            sw.Append(",");
            Children[2].Write(sw);
            sw.Append(")");


        }
        else
        {
            foreach (var part in Children)
            {
                part.Write(sw);
            }
        }

    }

    public IEnumerable<string> GetRequiredValues()
    {
        using var es = new VarScope(VariableHandling.Either);
        foreach (var obj in Children)
        {
            foreach (var val in obj.GetRequiredValues())
            {
                yield return val;
            }
        }
    }
}