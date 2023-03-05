using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal interface INode
{
    List<INode> Children { get; }
    void Write(StringBuilder sb);
    IEnumerable<string> GetRequiredValues()
    {
        foreach (var part in Children)
        {
            foreach (var dep in part.GetRequiredValues())
            {
                yield return dep;
            }
        }
    }

    bool IsSingleValue()
    {
        if (Children.Count != 1) { return false; }
        var val = Children[0];
        if (val is EValue)
        {
            return true;
        } else if (val is EGroup)
        {
            return val.IsSingleValue();
        }
        return false;
    }

    string GetText()
    {
        var sb = new StringBuilder();
        Write(sb);
        return sb.ToString();
    }
}
