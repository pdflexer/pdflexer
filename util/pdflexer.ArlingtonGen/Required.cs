using pdflexer.ArlingtonGen.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace pdflexer.ArlingtonGen;

internal class Required
{
	Row Row;

    public Required(Row row)
	{
        Row = row;
	}

	public bool IsSimple()
	{
		switch (Row.Required)
        {
            case "FALSE":
            case "TRUE":
                return true;
            default:
                return false;
        }

	}

    public bool GetSimple()
    {
        switch (Row.Required)
        {
            case "FALSE":
                return false;
            case "TRUE":
                return true;
            default:
                throw new ApplicationException("Unknown smple req " + Row.Required);
        }
    }

    public string GetComplex()
    {
        var sb = new StringBuilder();
        using var a = new EvalScope();
        var parts = Exp.Tokenize(Row.Required);
        foreach (var c in parts[0].Children)
        {
            c.Write(sb);
        }
        return sb.ToString(); ;
    }

    public List<string> GetComplexVars()
    {
        var parts = Exp.Tokenize(Row.Required);
        return parts[0].GetRequiredValues().ToList();
    }

}
