using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_RectHeight : EFunBase, INode
{
    public EFunc_RectHeight(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("RectHeight(");
        using var vs = new VarScope(VariableHandling.MustBeObj);
        foreach (var dep in Children)
        {
            dep.Write(sb);
        }
        sb.Append(")");
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        var prev = VariableContext.InEval;
        VariableContext.InEval = true;
        foreach (var item in Children.Cast<INode>().SelectMany(x=> x.GetRequiredValues()))
        {
            yield return item;
        }
        VariableContext.InEval = prev;
    }
}