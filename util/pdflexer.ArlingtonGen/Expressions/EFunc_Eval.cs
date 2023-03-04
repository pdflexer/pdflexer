using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;


internal class EFunc_Eval : EFunBase
{
    public EFunc_Eval(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            foreach (var dep in Inputs)
            {
                dep.Write(sb);
            }
        }
    }
}