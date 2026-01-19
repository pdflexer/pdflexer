using System.Collections;

namespace PdfLexer.DOM;

public class PdfOutlineItem : IEnumerable<PdfOutlineItem>
{
    private readonly PdfDictionary _dictionary;
    private PdfOutlineItem? _parent;
    private PdfOutlineItem? _prev;
    private PdfOutlineItem? _next;
    private PdfOutlineItem? _first;
    private PdfOutlineItem? _last;
    private List<PdfOutlineItem>? _children;

    public PdfOutlineItem()
    {
        _dictionary = new PdfDictionary();
    }

    public PdfOutlineItem(PdfDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public PdfDictionary GetPdfObject() => _dictionary;

    public string? Title
    {
        get => _dictionary.Get<PdfString>(PdfName.Title)?.Value;
        set
        {
            if (value == null)
            {
                _dictionary.Remove(PdfName.Title);
            }
            else
            {
                _dictionary[PdfName.Title] = new PdfString(value);
            }

        }
    }


    public PdfOutlineItem? Parent
    {
        get
        {
            if (_parent != null) return _parent;
            var p = _dictionary.Get<PdfDictionary>(PdfName.Parent);
            if (p == null) return null;
            return _parent = new PdfOutlineItem(p);
        }
        internal set
        {
            _dictionary[PdfName.Parent] = value._dictionary;
            _parent = value;
        }
    }

    public PdfOutlineItem? Prev
    {
        get
        {
            if (_prev != null) return _prev;
            var p = _dictionary.Get<PdfDictionary>(PdfName.Prev);
            if (p == null) return null;
            return _prev = new PdfOutlineItem(p);
        }
        internal set
        {
            _dictionary[PdfName.Prev] = value._dictionary;
            _prev = value;
        }
    }

    public PdfOutlineItem? Next
    {
        get
        {
            if (_next != null) return _next;
            var p = _dictionary.Get<PdfDictionary>(PdfName.Next);
            if (p == null) return null;
            return _next = new PdfOutlineItem(p);
        }
        internal set
        {
            _dictionary[PdfName.Next] = value._dictionary;
            _next = value;
        }
    }

    public PdfOutlineItem? First
    {
        get
        {
            if (_first != null) return _first;
            var p = _dictionary.Get<PdfDictionary>(PdfName.First);
            if (p == null) return null;
            return _first = new PdfOutlineItem(p);
        }
        internal set
        {
            _dictionary[PdfName.First] = value._dictionary;
            _first = value;
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
        internal set
        {
            _dictionary[PdfName.Last] = value._dictionary;
            _last = value;
        }
    }

    public int? Count
    {
        get => _dictionary.Get<PdfIntNumber>(PdfName.Count)?.Value;
        set => _dictionary[PdfName.Count] = new PdfIntNumber(value ?? 0);
    }

    public IPdfObject? Dest => _dictionary.Get(PdfName.Dest);
    public IPdfObject? Action => _dictionary.Get(PdfName.A);

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
        child.Parent = this;
        Children.Add(child);
        if (Last != null)
        {
            Last.Next = child;
            child.Prev = Last;
        }
        Last = child;
        if (First == null)
        {
            First = child;
        }
        Count = Children.Count;
    }

    public IEnumerator<PdfOutlineItem> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}