using PdfLexer.DOM;
using PdfLexer.Serializers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Pipelines;

namespace PdfLexer.Content;


/// <summary>
/// Performs deduplicate on pdf resources using a two level approach
/// * first level is matched on computationally easy item (eg. stream length for images)
/// * second level is a deep compare of object (very expensive) but only hit for objects matching first level
/// </summary>
internal class Deduplication
{
    private readonly PdfDocument _doc;
    private readonly TreeHasher _hash;

    public Deduplication(PdfDocument doc)
    {
        _doc = doc;
        _hash = new TreeHasher();
    }

    public void RunDeduplication()
    {
        foreach (var page in _doc.Pages)
        {
            seen = new HashSet<PdfDictionary>();
            HandleResources(page.Resources);
        }
    }

    private HashSet<PdfDictionary> seen = null!;
    private void HandleResources(PdfDictionary resources)
    {
        if (seen.Contains(resources)) return;
        seen.Add(resources);

        // for now just xobj / font
        // PdfName.XObject
        // PdfName.Font
        // PdfName.ColorSpace
        // PdfName.ExtGState
        // PdfName.Shading
        // PdfName.Pattern
        // Properties

        if (resources.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj, false))
        {
            HandleXObjs(xobj);
        }

        if (resources.TryGetValue<PdfDictionary>(PdfName.Font, out var font, false))
        {
            HandleFonts(font);
        }
    }

    private void HandleXObjs(PdfDictionary resources)
    {
        foreach (var (k,v) in resources.ToList())
        {
            if (v.Resolve() is PdfStream xobj && xobj.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var name, false))
            {
                switch (name.Value)
                {
                    case "Form":
                        HandleXObjForm(resources, k, xobj);
                        break;
                    case "Image":
                        HandleXObjImg(resources, k, xobj);
                        break;
                }
            }
        }
    }

    private void HandleFonts(PdfDictionary resources)
    {
        foreach (var (k, v) in resources.ToList())
        {
            if (v.Resolve() is PdfDictionary font)
            {
                HandleFont(resources, k, font);
            }
        }
    }

    private Dictionary<int, List<CachableItem>> fonts = new();
    private void HandleFont(PdfDictionary parent, PdfName key, PdfDictionary font)
    {
        if (!font.TryGetValue<PdfName>(PdfName.BaseFont, out var name, false))
        {
            return;
        }

        var l = name.GetHashCode();

        var item = new CachableItem
        {
            Item = font,
            Parent = parent,
            Key = key,
            Length = l
        };

        EvaluateCaching(fonts, item);
    }

    private Dictionary<int, List<CachableItem>> images = new();
    private void HandleXObjImg(PdfDictionary parent, PdfName key, PdfStream image)
    {
        var img = (XObjImage)image;
        var length = img.Contents.Length;
        if (length == 0) { return; }
        var item = new CachableItem
        {
            Item = image,
            Parent = parent,
            Key = key,
            Length = length
        };

        EvaluateCaching(images, item);
    }

    private Dictionary<int, List<CachableItem>> forms = new();
    private void HandleXObjForm(PdfDictionary parent, PdfName key, PdfStream form)
    {
        var frm = (XObjForm)form;
        var length = frm.Contents?.Length ?? 0;
        if (length == 0) { return; }
        if (frm.Resources != null )
        {
            if (frm.Resources.Count > 0)
            {
                unchecked
                {
                    length ^= frm.Resources.Count;
                }
            }
            
            if (frm.Resources.TryGetValue<PdfDictionary>(PdfName.XObject, out var dict) && dict.Count > 0)
            {
                unchecked
                {
                    length ^= dict.Count;
                }
            }
        }

        var item = new CachableItem
        {
            Item = form,
            Parent = parent,
            Key = key,
            Length = length
        };

        if (EvaluateCaching(forms, item))
        {
            return;
        }

        if (frm.Resources != null)
        {
            HandleResources(frm.Resources);
        }
    }

    private Dictionary<IPdfObject, PdfIndirectRef?> evaluated = new Dictionary<IPdfObject, PdfIndirectRef?>();

    private bool EvaluateCaching(Dictionary<int, List<CachableItem>> cache, CachableItem current)
    {
        if (evaluated.TryGetValue(current.Item, out var ir))
        {
            if (ir != null)
            {
                current.Parent[current.Key] = ir;
            }
            return true;
        }

        if (!cache.TryGetValue(current.Length, out var items))
        {
            items = new List<CachableItem> { current };
            cache[current.Length] = items;
            evaluated[current.Item] = null;
            return false;
        }


        foreach (var cached in items)
        {
            if (cached.Hash == null)
            {
                cached.Hash = _hash.GetHash(cached.Item);
            }
        }

        current.Hash = _hash.GetHash(current.Item);
        var match = items.FirstOrDefault(x=> x.Hash.Equals(current.Hash));
        if (match == null)
        {
            items.Add(current);
            evaluated[current.Item] = null;
            return false;
        }

        ir = match.Item.Indirect();
        current.Parent[current.Key] = ir;
        evaluated[current.Item] = ir;
        return true;
    }




    internal class CachableItem
    {
        public required IPdfObject Item { get; set; }
        public required PdfDictionary Parent { get; set; }
        public required PdfName Key { get; set; }
        public required int Length { get; set; } // first level deduping
        public PdfStreamHash? Hash { get; set; } // deep compare
    }

}
