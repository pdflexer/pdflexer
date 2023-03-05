using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_IsPresent : EFunBase
{
    public EFunc_IsPresent(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            if (Inputs.Count == 1)
            {
                if ((Inputs[0] as INode).IsSingleValue())
                {
                    AddVar(Inputs[0]);
                } else
                {
                    Inputs[0].Write(sb);
                }
            }
            if (Inputs.Count > 1)
            {
                sb.Append("(");
                AddVar(Inputs[0]);
                sb.Append(" && !(");
                Inputs[1].Write(sb);
                sb.Append("))");

            }            
        }

        void AddVar(EGroup grp)
        {
            var val = (grp.Children[0] as EValue)?.Text;
            if (val != null && int.TryParse(val, out var i))
            {
                sb.Append($"(obj.Count > " + (i - 1) + ")");
            }
            else if (val?.Contains("::") ?? false)
            {
                sb.Append($"(");
                grp.Write(sb);
                sb.Append($" != null)");
            } else
            {
                sb.Append($"obj.ContainsKey(");
                grp.Write(sb);
                sb.Append($")");
            }
        }
    }
}