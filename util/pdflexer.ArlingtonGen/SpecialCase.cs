namespace pdflexer.ArlingtonGen;

internal class SpecialCase
{
    Row Row;

    public GenBase Gen { get; }

    public SpecialCase(GenBase gen, Row row)
    {
        Row = row;
        Gen = gen;
    }

    public string GetSpecialCase()
    {
        return GetSpecialCaseInt(Row.SpecialCase.Trim().TrimStart('[').TrimEnd(']'), Row.Type);
    }

    public string GetSpecialCase(string type)
    {
        var types = Row.Type.Split(';').ToList();
        var sc = Row.SpecialCase.Split(";");
        var special = sc.Length == 1 ? sc[0] : sc[types.IndexOf(type)];
        special = special.Trim().TrimStart('[').TrimEnd(']');
        return GetSpecialCaseInt(special, type);
    }

    private string GetSpecialCaseInt(string value, string type)
    {
        if (string.IsNullOrEmpty(value) || value == "")
        {
            return "// no special cases";
        }
        if (value.Contains("fn:Ignore"))
        {
            return "// special case is an fn:Ignore, not pertinent to validation";
        }
        if (value.StartsWith("fn:IsMeaningful"))
        {
            return "// special case is an fn:IsMeaningful, not pertinent to validation";
        }

        if (value.StartsWith("fn:Eval"))
        {
            var exp = new Exp(value);
            var vars = exp.GetRequiredValues().Distinct();
            var defs = string.Join('\n', vars.Select(v => Gen.GetSetter(v, type, "val")));
            return $$"""
{{defs}}
if (!({{exp.GetText()}})) 
{
    ctx.Fail<APM_{{Gen.RootName}}_{{Gen.Key}}>($"Value failed special case check: {{value}}");
}
""";
        }

        if (value.StartsWith("fn:Not")) // doesn't really make sense but seems to only be used with IsPresent
        {
            var exp = new Exp(value[7..^1]);
            var vars = exp.GetRequiredValues().Distinct();
            var defs = string.Join('\n', vars.Select(v => Gen.GetSetter(v, type, "val")));
            return $$"""
{{defs}}
if ({{exp.GetText()}}) 
{
    ctx.Fail<APM_{{Gen.RootName}}_{{Gen.Key}}>($"Value failed special case check: {{value}}");
}
""";
        }

        return "// TODO special case: " + value;
    }
}
