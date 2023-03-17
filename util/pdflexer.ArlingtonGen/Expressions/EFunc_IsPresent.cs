using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_IsPresent : EFunBase, INode
{
    public EFunc_IsPresent(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            if (Children.Count == 1)
            {
                if ((Children[0] as INode).IsSingleValue())
                {
                    AddVar(Children[0]);
                } else
                {
                    using var ev = new VarScope(VariableHandling.Either);
                    Children[0].Write(sb);
                }
            } else if (Children.Count > 1)
            {
                sb.Append("(!");
                AddVar(Children[0]);
                sb.Append(" || (");
                Children[1].Write(sb);
                sb.Append("))");

            }            
        }

        void AddVar(INode grp)
        {
            var val = (grp as EValue)?.Text;
            if (val != null && int.TryParse(val, out var i))
            {
                sb.Append($"(obj.Count > " + (i - 1) + ")");
            }
            else if (val?.Contains("::") ?? false)
            {
                using var ev = new VarScope(VariableHandling.MustBeObj);
                sb.Append($"(");
                grp.Write(sb);
                sb.Append($" != null)");
            } else
            {
                using var ev = new VarScope(VariableHandling.MustBeVal);
                sb.Append($"obj.ContainsKey(");
                grp.Write(sb);
                sb.Append($")");
            }
        }
    }


    IEnumerable<string> INode.GetRequiredValues()
    {
        var val = (Children[0] as EValue)?.Text;
        if (val != null && !val.Contains("*") && !val.Contains("::") && !int.TryParse(val, out _))
        {
            // first value is key
            if (Children.Count > 1)
            {
                foreach (var var in Children[1].GetRequiredValues())
                {
                    yield return var;
                }
            }
        }
        else
        {
            foreach (var item in Children)
            {
                foreach (var var in item.GetRequiredValues())
                {
                    yield return var;
                }
            }
        }
    }
}