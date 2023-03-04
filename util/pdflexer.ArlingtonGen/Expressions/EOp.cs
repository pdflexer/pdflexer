using System.Text;

namespace pdflexer.ArlingtonGen.Expressions;

internal class EOp : INode
{
    public string Text;
    public EOp(string text)
    {
        Text = text;
    }
    public List<INode> Children { get; } = new List<INode>();

    public bool IsComparator()
    {
        if (Text == "<=") { return true; }
        if (Text == ">=") { return true; }
        if (Text == "<") { return true; }
        if (Text == ">") { return true; }
        if (Text == "==") { return true; }
        if (Text == "!=") { return true; }
        if (Text == "||") { return true; }
        if (Text == "&&") { return true; }
        return false;
    }

    public void Write(StringBuilder sw)
    {
        sw.Append(Text);
    }

    public IEnumerable<string> GetRequiredValues() { yield break; }
}
