using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunc_Not : EFunBase
{
    public EFunc_Not(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append("!");
        using (var es = new EvalScope())
        {
            foreach (var dep in Children)
            {
                dep.Write(sb);
            }
        }
    }
}