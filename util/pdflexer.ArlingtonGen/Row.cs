using PdfLexer;

namespace pdflexer.ArlingtonGen;

internal record Row(string Key, string Type, string SinveVersion, string DeprecatedIn, string Required,
    string IndirectReference, string Inheritable, string DefaultValue, string PossibleValues,
    string SpecialCase, string Link, string Notes);

internal static class Maps
{
    internal static Dictionary<string, PdfObjectType> typemap = new Dictionary<string, PdfObjectType>
    {
        {"array", PdfObjectType.ArrayObj },
        {"bitmask", PdfObjectType.NumericObj },
        {"boolean", PdfObjectType.BooleanObj },
        {"date", PdfObjectType.StringObj },
        {"dictionary", PdfObjectType.DictionaryObj },
        {"integer", PdfObjectType.NumericObj },
        {"matrix", PdfObjectType.ArrayObj },
        {"name", PdfObjectType.NameObj },
        {"name-tree", PdfObjectType.DictionaryObj },
        {"null", PdfObjectType.NullObj },
        {"number", PdfObjectType.ArrayObj },
        {"number-tree", PdfObjectType.NumericObj },
        {"rectangle", PdfObjectType.ArrayObj },
        {"string", PdfObjectType.StringObj },
        {"string-ascii", PdfObjectType.StringObj },
        {"string-byte", PdfObjectType.StringObj },
        {"string-text", PdfObjectType.StringObj },
    };

    internal static Dictionary<string, string> typeDomMap = new Dictionary<string, string>
    {
        {"array", "PdfArray" },
        {"bitmask", "PdfIntNumber" },
        {"boolean", "PdfBoolean" },
        {"date", "PdfDate" },
        {"dictionary", "PdfDictionary" },
        {"integer", "PdfIntNumber" },
        {"matrix", "Matrix" },
        {"name", "PdfName" },
        {"name-tree", "NameTree" },
        {"null", "PdfNull" },
        {"number", "PdfNumber" },
        {"number-tree", "NumberTree" },
        {"rectangle", "Rectangle" },
        {"string", "PdfString" },
        {"string-ascii", "AsciiString" },
        {"string-byte", "ByteString" },
        {"string-text", "TextString" },
    };

    internal static Dictionary<PdfObjectType, string> typeRawnames = new Dictionary<PdfObjectType, string>
    {
        {PdfObjectType.ArrayObj,  "PdfArray" },
        {PdfObjectType.NumericObj, "PdfNumber" },
        {PdfObjectType.BooleanObj, "PdfBoolean" },
        {PdfObjectType.StringObj, "PdfString" },
        {PdfObjectType.DictionaryObj, "PdfDictionary" },
        {PdfObjectType.NumericObj, "PdfNumber" },
        {PdfObjectType.ArrayObj, "PdfArray" },
        {PdfObjectType.NameObj, "PdfName" },
        {PdfObjectType.DictionaryObj, "PdfDictionary" },
        {PdfObjectType.NullObj, "PdfNull" },
    };
}

internal static class Util
{

    internal static string CreateDictProperty(Row row)
    {
        if (string.IsNullOrWhiteSpace(row.Link) && string.IsNullOrWhiteSpace(row.Link))
        {

        }

        var type = Maps.typeDomMap[row.Type];
        var valsettxt = "value";
        if (row.IndirectReference == "TRUE")
        {
            valsettxt += ".Indirectly()";
        }

        var gettxt = $"return NativeObject.Get<{type}>(PdfName.{row.Key})";
        if (!string.IsNullOrEmpty(row.DefaultValue))
        {
            if (row.DefaultValue.StartsWith("@"))
            {
                var fallback = row.DefaultValue.TrimStart('@');
                gettxt = $"return NativeObject.GetWithFallbackKey<{type}>(PdfName.{row.Key}, PdfName.{fallback})";
            } else
            {
                gettxt = $"return NativeObject.Get<{type}>(PdfName.{row.Key}) ?? {row.DefaultValue}";
            }
        }

        
        var data = $@"
public {type} {row.Key} {{
    get {{ {gettxt}; }}
    set {{ NativeObject[PdfName.{row.Key}] = {valsettxt}; }}
}}
";
        return data;
    }
}
