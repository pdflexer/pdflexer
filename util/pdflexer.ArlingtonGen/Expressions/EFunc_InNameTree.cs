using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_InNameTree : EFunBase, INode
{
    public EFunc_InNameTree(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("InNameTree(");
        sb.Append(VariableContext.VarSub);
        sb.Append(",");
        using var vs = new VarScope(VariableHandling.MustBeObj);
        using var ev = new EvalScope();
        Children[0].Write(sb);
        sb.Append(")");
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        using var vs = new VarScope(VariableHandling.MustBeObj);
        return Children[0].GetRequiredValues();
    }
}