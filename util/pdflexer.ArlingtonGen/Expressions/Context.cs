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
    Number,
    Integer
}

internal static class VariableContext
{
    public static void InitType(string type)
    {
        InEval = false;
        CurrentType = type switch
        {
            "name" => NonEvalType.Name,
            "number" => NonEvalType.Number,
            "integer" => NonEvalType.Integer,
            "bitmask" => NonEvalType.Integer,
            "string" => NonEvalType.String,
            "string-text" => NonEvalType.String,
            "string-byte" => NonEvalType.String,
            "string-ascii" => NonEvalType.String,
            _ => NonEvalType.String,
        };
        VarSub = type switch
        {
            "string" => "val.Value",
            "string-text" => "val.Value",
            "string-byte" => "val.Value",
            "string-ascii" => "val.Value",
            _ => "val"
        };
        Wrapper = ValWrapper;
    }
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

internal class VarType : IDisposable
{
    public VarType(NonEvalType val)
    {
        Orig = VariableContext.CurrentType;
        VariableContext.CurrentType = val;
    }

    public NonEvalType Orig { get; }

    public void Dispose()
    {
        VariableContext.CurrentType = Orig;
    }
}