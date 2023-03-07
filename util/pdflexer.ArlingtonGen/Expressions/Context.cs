using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;


internal enum VariableHandling
{
    MustBeObj,
    MustBeVal,
    Either
}

internal enum NonEvalType
{
    Name,
    String,
    Number
}

internal static class VariableContext
{
    public static Row? Context { get; set; }
    public static bool InEval { get; set; }
    public static NonEvalType CurrentType { get; set; }
    public static VariableHandling Handling { get; set; } = VariableHandling.Either;
    public static Dictionary<string,string> Vars { get; set; } = new Dictionary<string,string>();
    public static string VarName { get; set; } = "val";
    public static string VarSub { get; set; } = "val";
    public static Action<StringBuilder, EValue, Action<StringBuilder>> ValWrapper { get; } = (sb, v, core) => {
        var prev = InEval;
        InEval = false;
        sb.Append($"{VarSub} == ");
        core(sb);
        InEval = prev;
    };
    public static Action<StringBuilder, EValue, Action<StringBuilder>> GetFromObj { get; } = (sb, v, core) => {
        sb.Append($"obj.Get(");
        core(sb);
        sb.Append($")");
    };
    public static Action<StringBuilder, EValue, Action<StringBuilder>> EvalWrapper { get; } = (sb, v, core) => {
        var prev = InEval;
        InEval = true;
        core(sb);
        InEval = prev;
    };
    public static Action<StringBuilder, EValue, Action<StringBuilder>> Wrapper { get; set; } = ValWrapper;
}


internal class EvalScope : IDisposable
{
    public EvalScope(Action<StringBuilder, EValue, Action<StringBuilder>>? val = null)
    {
        Orig = VariableContext.Wrapper;
        VariableContext.Wrapper = val ?? VariableContext.EvalWrapper;
    }

    public Action<StringBuilder, EValue, Action<StringBuilder>> Orig { get; }

    public void Dispose()
    {
        VariableContext.Wrapper = Orig;
    }
}

internal class VarScope : IDisposable
{
    public VarScope(VariableHandling val)
    {
        Orig = VariableContext.Handling;
        VariableContext.Handling = val;
    }

    public VariableHandling Orig { get; }

    public void Dispose()
    {
        VariableContext.Handling = Orig;
    }
}