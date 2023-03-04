using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_RectHeight : EFunBase, INode
{
    public EFunc_RectHeight(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("RectHeight(");
        using (var es = new EvalScope())
        {
            foreach (var dep in Inputs)
            {
                dep.Write(sb);
            }
        }
        sb.Append(")");
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
