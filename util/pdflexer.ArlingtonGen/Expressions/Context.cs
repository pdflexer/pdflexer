using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;



internal static class VariableContext
{
    public static Row? Context { get; set; }
    public static bool InEval { get; set; }
    public static Dictionary<string,string> Vars { get; set; } = new Dictionary<string,string>();
    public static string VarName { get; set; } = "val";
    public static string VarSub { get; set; } = "val";
    public static Action<StringBuilder, Action<StringBuilder>> ValWrapper { get; } = (sb, core) => {
        var prev = InEval;
        InEval = false;
        sb.Append($"{VarSub} == ");
        core(sb);
        InEval = prev;
    };
    public static Action<StringBuilder, Action<StringBuilder>> GetFromObj { get; } = (sb, core) => {
        sb.Append($"obj.Get(");
        core(sb);
        sb.Append($")");
    };
    public static Action<StringBuilder, Action<StringBuilder>> EvalWrapper { get; } = (sb, core) => {
        var prev = InEval;
        InEval = true;
        core(sb);
        InEval = prev;
    };
    public static Action<StringBuilder, Action<StringBuilder>> Wrapper { get; set; } = ValWrapper;
}


public class EvalScope : IDisposable
{
    public EvalScope(Action<StringBuilder, Action<StringBuilder>>? val = null)
    {
        Orig = VariableContext.Wrapper;
        VariableContext.Wrapper = val ?? VariableContext.EvalWrapper;
    }

    public Action<StringBuilder, Action<StringBuilder>> Orig { get; }

    public void Dispose()
    {
        VariableContext.Wrapper = Orig;
    }
}