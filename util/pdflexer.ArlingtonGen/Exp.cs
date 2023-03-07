using pdflexer.ArlingtonGen.Expressions;
using System.Text;

namespace pdflexer.ArlingtonGen;

internal class Exp : INode
{
    public Exp(string text)
    {
        var n = Tokenize(text);
        Children = new List<INode> { };
        if (n != null)
        {
            Children.Add(n);
        }
    }
    internal static INode? Tokenize(string text)
    {
        var parts = new List<INode>();
        int i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == ' ') { i++; continue; }
            var n = text.Substring(i);
            if (n.StartsWith("fn:"))
            {
                var m = GenBase.AnyFuncRegex.Match(n);
                if (!m.Success)
                {
                    m = GenBase.FuncRegex.Match(n);
                }

                parts.Add(EFunction.Create(m.Groups[1].Value, m.Groups[2].Value));
                i += m.Value.Length;
                continue;
            }
            if (n.StartsWith("* ")) { parts.Add(new EOp("*")); i += 2; continue; }
            if (n.StartsWith("+")) { parts.Add(new EOp("+")); i += 1; continue; }
            if (n.StartsWith("- ")) { parts.Add(new EOp("-")); i += 2; continue; }
            if (n.StartsWith("<=")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith(">=")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith("<")) { parts.Add(new EOp(n.Substring(0, 1))); i += 1; continue; }
            if (n.StartsWith(">")) { parts.Add(new EOp(n.Substring(0, 1))); i += 1; continue; }
            if (n.StartsWith("==")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith("!=")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith("||")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith("&&")) { parts.Add(new EOp(n.Substring(0, 2))); i += 2; continue; }
            if (n.StartsWith("mod")) { parts.Add(new EOp("%")); i += 3; continue; }
            if (c == '(')
            {
                var d = 1;
                i++;
                var s = i;
                for (; i < text.Length; i++)
                {
                    var cc = text[i];
                    if (cc == '(')
                    {
                        d++;
                    }
                    else if (cc == ')')
                    {
                        d--;
                    }
                    if (d == 0)
                    {
                        break;
                    }
                }
                parts.Add(new Exp(text.Substring(s, i - s)));
                i++;
                continue;
            }

            var vs = i;
            bool matched = false;
            for (; i < text.Length; i++)
            {
                var cc = text[i];
                if (terms.Contains(cc))
                {
                    // if (cc == '*' && i > 0 && text[i-1] == ':')
                    // {
                    //     continue;
                    // }
                    parts.Add(new EValue(text.Substring(vs, i - vs)));
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                parts.Add(new EValue(text.Substring(vs, i - vs)));
            }

        }
        if (parts.Count == 1)
        {
            return parts[0];
        } else if (parts.Count == 0)
        {
            return null;
        }
        return new EGroup(parts);
    }

    public void Write(StringBuilder sb)
    {
        foreach (var c in Children)
        {
            c.Write(sb);
        }
    }

    private static List<char> terms = new List<char>
    {
        ' ', '(', '<', '>', '!', '=', '+',
    };

    public List<INode> Children { get; }


    public IEnumerable<string> GetRequiredValues()
    {
        foreach (var part in Children)
        {
            foreach (var dep in part.GetRequiredValues())
            {
                if (!VariableContext.Vars.ContainsKey(dep))
                {
                    yield return dep;
                }
            }
        }
    }

    public IEnumerable<INode> GetAll()
    {
        foreach (var part in Children)
        {
            yield return part;

            foreach (var dep in part.Descendants())
            {
                yield return dep;
            }
        }
    }


    public string GetText()
    {
        var sb = new StringBuilder();
        Write(sb);
        return sb.ToString();
    }
}
