namespace PdfLexer.Parsers.Nested;

internal enum ParseState
{
    None,
    ReadArray,
    ReadDict
}

internal struct ObjParseState
{
    public ParseState State { get; set; }
    public PdfName CurrentKey { get; set; }
    public PdfDictionary Dict { get; set; }
    public PdfArray Array { get; set; }
    public List<IPdfObject> Bag { get; set; }

    public bool IsParsing()
    {
        return State != ParseState.None;
    }

    public PdfArray GetArrayFromBag(PdfDocument? doc)
    {
        var arr = new PdfArray(new List<IPdfObject>(Bag.Count));
        for (var i=0;i<Bag.Count;i++)
        {
            var item = Bag[i];
            if (item is PdfNumber num 
                && i + 2 < Bag.Count
                && Bag[i+1] is PdfIntNumber num2
                && Bag[i+2] is IndirectRefToken)
            {
                if (doc == null) { throw new PdfLexerException("Indirect reference found without source doc."); }
                arr.Add(new ExistingIndirectRef(doc, (long)num, num2.Value));
                i+=2;
            } else
            {
                arr.Add(item);
            }
        }
        return arr;
    }

    public PdfDictionary GetDictionaryFromBag(ParsingContext ctx, PdfDocument? doc)
    {
        var dict = new PdfDictionary(Bag.Count / 2);
        bool key = true;
        PdfName? name = null;
        for (var i=0;i<Bag.Count;i++)
        {
            var item = Bag[i];
            if (key)
            {
                if (item is PdfName nm)
                {
                    name = nm;
                } else
                {
                    ctx.Error($"Pdf dictionary was malformed, expected PdfName for key, got {item.Type}");
                    continue;
                }
            } else if (name != null)
            {
                if (item is PdfNumber num 
                    && i + 2 < Bag.Count
                    && Bag[i+1] is PdfIntNumber num2
                    && Bag[i+2] is IndirectRefToken)
                {

                    if (doc == null) { throw new PdfLexerException("Indirect reference found without source doc."); }
                    dict[name] = new ExistingIndirectRef(doc, (long)num, num2.Value);
                    i+=2;
                } else
                {
                    dict[name] = item;
                }
                
            }
            key = !key;
        }
        dict.DictionaryModified = false;
        return dict;
    }
}
