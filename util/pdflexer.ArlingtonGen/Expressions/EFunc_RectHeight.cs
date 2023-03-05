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
        var prev = VariableContext.InEval;
        VariableContext.InEval = true;
        foreach (var item in Inputs.Cast<INode>().SelectMany(x=> x.GetRequiredValues()))
        {
            yield return item;
        }
        VariableContext.InEval = prev;
    }
}