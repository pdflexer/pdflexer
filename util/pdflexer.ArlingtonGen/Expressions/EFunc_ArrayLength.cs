using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_ArrayLength : EFunBase, INode
{
    public EFunc_ArrayLength(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using var es = new VarScope(VariableHandling.MustBeObj);
        sb.Append($"((");
        Children[0].Write(sb);
        sb.Append($" as PdfArray)?.Count)");
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        var val = (Children[0] as EValue)?.Text;
        if (val != null && val != "*" && !int.TryParse(val, out _))
        {
            yield return val;
        } else
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
