using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_RequiredValue : EFunBase, INode
{
    public EFunc_RequiredValue(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"(");
        using (var es = new EvalScope())
        {
            Children[0].Write(sb);
        }
        sb.Append(" && !(");
        Children[1].Write(sb);
        sb.Append("))");
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        return Children[0].GetRequiredValues();
    }
}
