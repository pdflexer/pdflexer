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
}
