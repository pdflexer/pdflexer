using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_ArrayLength : EFunBase, INode
{
    public EFunc_ArrayLength(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"((");
        Inputs[0].Write(sb);
        sb.Append($" as PdfArray)?.Count)");
        // using (var es = new EvalScope(ExpValueWrapper.GetFromObj))
        // {
        // 
        // }
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
