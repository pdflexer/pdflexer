using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;


internal class EFunc_Eval : EFunBase
{
    public EFunc_Eval(List<INode> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        VariableContext.InEval = true;
        using var es = new EvalScope();
        using var ev = new VarScope(VariableHandling.Either);
        foreach (var dep in Children)
        {
            dep.Write(sb);
        }
        VariableContext.InEval = false;
    }
}