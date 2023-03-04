using pdflexer.ArlingtonGen.Expressions;
using System.Text;

namespace pdflexer.ArlingtonGen;

internal class Exp
{
    internal static List<INode> Tokenize(string text)
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
                parts.Add(new EGroup(text.Substring(s, i - s)));
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
        return parts;
    }

    private static List<char> terms = new List<char>
    {
        ' ', '(', '<', '>', '!', '='
    };
}
