using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_IsRequired : EFunBase, INode
{
    public EFunc_IsRequired(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            foreach (var dep in Children)
            {
                dep.Write(sb);
            }
        }
    }
}
