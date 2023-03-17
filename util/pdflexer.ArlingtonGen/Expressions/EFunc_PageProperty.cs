using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;


internal class EFunc_PageProperty : EFunBase, INode
{
    public EFunc_PageProperty(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            sb.Append($"ctx.GetPage(");
            Children[0].Write(sb);
            sb.Append(")?.Get(");
            using var vs = new VarScope(VariableHandling.MustBeVal);
            Children[1].Write(sb);
            sb.Append(")");
        }

    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        var val = (Children[0] as EValue)?.Text;
        if (val != null)
        {
            yield return val;
        }
    }
}