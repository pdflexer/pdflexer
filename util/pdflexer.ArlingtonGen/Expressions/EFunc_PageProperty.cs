using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;


internal class EFunc_PageProperty : EFunBase, INode
{
    public EFunc_PageProperty(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            sb.Append($"(");
            Inputs[0].Write(sb);
            sb.Append(")?.GetAs<PdfDictionary>()?.Get(");
            Inputs[1].Write(sb);
            sb.Append(")");
        }

    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        var val = (Inputs[0].Children[0] as EValue)?.Text;
        if (val != null)
        {
            yield return val;
        }
    }
}