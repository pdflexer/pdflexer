using System.Collections;

namespace PdfLexer.DOM;

public class PdfOutlineRoot : IEnumerable<PdfOutlineItem>
{
    private readonly PdfDictionary _dictionary;
    private PdfOutlineItem? _first;
    private PdfOutlineItem? _last;
    private List<PdfOutlineItem>? _children;

    public PdfOutlineRoot()
    {
        _dictionary = new PdfDictionary();
    }
    
    public PdfOutlineRoot(PdfDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public PdfDictionary GetPdfObject() => _dictionary;

    public PdfOutlineItem? First
    {
        get
        {
            if (_first != null) return _first;
            var p = _dictionary.Get<PdfDictionary>(PdfName.First);
            if (p == null) return null;
            return _first = new PdfOutlineItem(p);
        }
    }

    public PdfOutlineItem? Last
    {
        get
        {
            if (_last != null) return _last;
            var p = _dictionary.Get<PdfDictionary>(PdfName.Last);
            if (p == null) return null;
            return _last = new PdfOutlineItem(p);
        }
    }
    
    public int? Count
    {
        get => _dictionary.Get<PdfIntNumber>(PdfName.Count)?.Value;
        set => _dictionary[PdfName.Count] = new PdfIntNumber(value ?? 0);
    }
    
    public List<PdfOutlineItem> Children
    {
        get
        {
            if (_children != null) return _children;
            _children = new List<PdfOutlineItem>();
            var current = First;
            while (current != null)
            {
                _children.Add(current);
                current = current.Next;
            }
            return _children;
        }
    }

    public void Add(PdfOutlineItem child)
    {
        Children.Add(child);
        if (Last != null)
        {
            Last.Next = child;
            child.Prev = Last;
        }
        _last = child;
        if (First == null)
        {
            _first = child;
        }
        if (_dictionary.ContainsKey(PdfName.First))
        {
            _dictionary.Remove(PdfName.First);
        }
        if (_dictionary.ContainsKey(PdfName.Last))
        {
            _dictionary.Remove(PdfName.Last);
        }
        Count = Children.Count;
    }

    public IEnumerator<PdfOutlineItem> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}