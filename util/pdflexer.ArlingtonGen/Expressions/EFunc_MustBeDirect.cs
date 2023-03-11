using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_MustBeDirect : EFunBase, INode
{
    public EFunc_MustBeDirect(List<INode> inputs) : base(inputs) { }
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
                    throw new NotImplementedException();
                }
            }
            if (Children.Count > 1)
            {
                throw new NotImplementedException();
            }            
        }

        void AddVar(INode node)
        {
            
            var val = (node as EValue)?.Text;
            if (val?.Contains("::") ?? false)
            {
                
                var sgs = val.Split("::").ToList();
                var i = 0;
                bool nullable = false;
                if (sgs[0] == VariableContext.VarName)
                {
                    sb.Append("(!val");
                    i = 1;
                } else
                {
                    sb.Append("(!obj");
                }
                
                for (; i < sgs.Count-1;i++)
                {
                    var item = sgs[i];
                    if (nullable)
                    {
                        sb.Append("?");
                    }
                    sb.Append(".Get(");
                    EValue.WriteVal(sb, item);
                    sb.Append(")");
                    nullable = true;
                }


                if (nullable)
                {
                    sb.Append("?");
                }
                sb.Append(".IsKeyIR(");
                EValue.WriteVal(sb, sgs.Last());
                sb.Append(")");
                if (nullable)
                {
                    sb.Append("?? true)");
                } else
                {
                    sb.Append(")");
                }
            } else if (val != null)
            {
                sb.Append("obj.IsKeyIR(");
                EValue.WriteVal(sb, val);
                sb.Append(")");
            }
        }
    }


    IEnumerable<string> INode.GetRequiredValues()
    {
        // this is only used for special case with no expressions currently
        yield break;
    }
}