using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EFunction : INode
{
    public static INode Create(string type, string contents)
    {
        var parts = SplitWithFns(contents).Select(x => new EGroup(x)).ToList();
        switch (type)
        {
            case "IsPDFVersion":
                return new EFunc_IsPDFVersion(parts);
            case "Eval":
                return new EFunc_Eval(parts);
            case "SinceVersion":
                return new EFunc_SinceVersion(parts);
            case "BeforeVersion":
                return new EFunc_BeforeVersion(parts);
            case "Extension":
                return new EFunc_Extension(parts);
            case "PageProperty":
                return new EFunc_PageProperty(parts);
            case "ArrayLength":
                return new EFunc_ArrayLength(parts);
            case "Deprecated":
                return new EFunc_Deprecated(parts);
            case "NumberOfPages":
                return new EFunc_NumPages(parts);
            case "FileSize":
                return new EFunc_FileSize(parts);
            case "IsPresent":
                return new EFunc_IsPresent(parts);
            case "Not":
                return new EFunc_Not(parts);
            case "RectHeight":
                return new EFunc_RectHeight(parts);
            case "IsRequired":
                return new EFunc_IsRequired(parts);
            case "BitClear":
                return new EFunc_BitClear(parts);
            case "BitsClear":
                return new EFunc_BitsClear(parts);
            case "BitSet":
                return new EFunc_BitSet(parts);
            case "BitsSet":
                return new EFunc_BitsSet(parts);
            default:
                return new EFunction(type, contents);
        }
    }
    public string Type;
    public EFunction(string type, string contents)
    {
        Type = type;
        Parts = SplitWithFns(contents).Select(x => new EGroup(x)).ToList();
    }

    public List<EGroup> Parts { get; }

    public List<INode> Children { get => Parts.Cast<INode>().ToList(); }

    public void Write(StringBuilder sw)
    {
        sw.Append(Type);
        sw.Append('(');
        if (Children.SelectMany(x=>x.GetRequiredValues()).Any())
        {
            for (var i = 0; i < Parts.Count; i++)
            {
                Parts[i].Write(sw);
                if (i < Parts.Count - 1)
                {
                    sw.Append(", ");
                }
            }
        } else
        {
            sw.Append("obj");
        }

        sw.Append(')');
    }

    private static List<string> SplitWithFns(string text)
    {
        var parts = new List<string>();
        int d = 0;
        int l = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '(')
            {
                d++;
            }
            else if (c == ')')
            {
                d--;
            }
            else if (d == 0 && c == ',')
            {
                parts.Add(text.Substring(l, i - l));
                l = i + 1;
            }
        }
        parts.Add(text.Substring(l, text.Length - l));
        return parts;
    }
}

internal abstract class EFunBase : INode
{
    public List<EGroup> Inputs { get; }
    public List<INode> Children { get => Inputs.Cast<INode>().ToList(); }
    public EFunBase(List<EGroup> inputs)
    {
        Inputs = inputs;
    }

    public abstract void Write(StringBuilder sb);
}




internal class EFunc_IsPDFVersion : EFunBase
{
    public EFunc_IsPDFVersion(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"(ctx.Version == ");
        using (var es = new EvalScope())
        {
            Inputs[0].Write(sb);
        }
        if (Inputs.Count > 1)
        {
            sb.Append(" && ");
            Inputs[1].Write(sb);
        }
        sb.Append(")");
    }
}

internal class EFunc_NumPages : EFunBase
{
    public EFunc_NumPages(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"ctx.NumberOfPages");
    }
}

internal class EFunc_FileSize : EFunBase
{
    public EFunc_FileSize(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"ctx.FileSize");
    }
}

internal class EFunc_Deprecated : EFunBase
{
    public EFunc_Deprecated(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        using (var es = new EvalScope())
        {
            sb.Append($"(ctx.Version < ");
            Inputs[0].Write(sb);
        }

        if (Inputs.Count > 1)
        {
            sb.Append(" && ");
            Inputs[1].Write(sb);
        }
        sb.Append($")");
    }
}



internal class EFunc_RequiredValue : EFunBase
{
    public EFunc_RequiredValue(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"(ctx.Version == ");
        using (var es = new EvalScope())
        {
            Inputs[0].Write(sb);
        }
        sb.Append(" && ");
        Inputs[1].Write(sb);
        sb.Append(")");
    }
}

internal class EFunc_Extension : EFunBase, INode
{
    public EFunc_Extension(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        sb.Append($"(ctx.Extensions.Contains(");
        using (var es = new EvalScope())
        {
            Inputs[0].Write(sb);
        }
        sb.Append(")");
        if (Inputs.Count > 1)
        {
            sb.Append(" && ");
            Inputs[1].Write(sb);
        }
        sb.Append(")");
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        if (Inputs.Count > 1)
        {
            foreach (var item in ((INode)Inputs[1]).GetRequiredValues())
            {
                yield return item;
            }
        }
    }
}


internal class EFunc_SinceVersion : EFunBase, INode
{
    public EFunc_SinceVersion(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        if (Inputs.Count == 1)
        {
            using var es = new EvalScope();
            sb.Append("ctx.Version >= ");
            Inputs[0].Write(sb);
        } else
        {
            if ((Inputs[1] as INode).IsSingleValue())
            {
                using (var es = new EvalScope())
                {
                    sb.Append("(ctx.Version >= ");
                    Inputs[0].Write(sb);
                }
                sb.Append(" && ");
                Inputs[1].Write(sb);
                sb.Append(")");
            } else
            {
                using (var es = new EvalScope())
                {
                    sb.Append("(ctx.Version < ");
                    Inputs[0].Write(sb);
                    sb.Append(" || ");
                }

                Inputs[1].Write(sb);
                sb.Append(")");
            }
        }
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        if (Inputs.Count > 1)
        {
            foreach (var item in ((INode)Inputs[1]).GetRequiredValues())
            {
                yield return item;
            }
        }
    }
}

internal class EFunc_BeforeVersion : EFunBase, INode
{
    public EFunc_BeforeVersion(List<EGroup> inputs) : base(inputs) { }
    public override void Write(StringBuilder sb)
    {
        if (Inputs.Count == 1)
        {
            using var es = new EvalScope();
            sb.Append("ctx.Version < ");
            Inputs[0].Write(sb);
        }
        else
        {
            if ((Inputs[1] as INode).IsSingleValue())
            {
                using (var es = new EvalScope())
                {
                    sb.Append("(ctx.Version > ");
                    Inputs[0].Write(sb);
                }
                sb.Append(" && ");
                Inputs[1].Write(sb);
                sb.Append(")");
            }
            else
            {
                using (var es = new EvalScope())
                {
                    sb.Append("(ctx.Version >= ");
                    Inputs[0].Write(sb);
                    sb.Append(" || ");
                }

                Inputs[1].Write(sb);
                sb.Append(")");
            }
        }
    }

    IEnumerable<string> INode.GetRequiredValues()
    {
        if (Inputs.Count > 1)
        {
            foreach (var item in ((INode)Inputs[1]).GetRequiredValues())
            {
                yield return item;
            }
        }
    }
}