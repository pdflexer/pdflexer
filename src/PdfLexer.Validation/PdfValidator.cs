using PdfLexer.Parsers.Structure;
using System.Runtime.CompilerServices;

namespace PdfLexer.Validation;

/// <summary>
/// Validates a PDF against the pdf associations arlington PDF model
/// </summary>
public class PdfValidator
{
    public PdfValidator(PdfDocument doc, long fileSize)
    {
        Document = doc;
        FileSize = fileSize;
        Current = null!;
    }
    internal PdfDictionary Trailer { get => Document.Trailer; }
    internal decimal Version { get => Document.PdfVersion; }
    internal long FileSize { get; set; }
    internal int NumberOfPages { get => Document.Pages.Count; }
    internal PdfDocument Document { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Extensions { get; set; } = new List<string>();
    public List<string> Passed { get; } = new List<string>();

    public void Run()
    {
        // Run<APM_FileTrailer, PdfDictionary>(new CallStack(), Trailer, null);
    }

    internal void Fail<T>(string msg) where T : ISpecification => Fail(T.Name, msg);

    internal void Fail(string type, string msg)
    {
        Errors.Add($"[{type}] " + msg + "\n(" + string.Join(" -> ", Current.Stack) + ")");
    }

    internal PdfDictionary? GetPage(IPdfObject? lookup)
    {
        if (lookup == null)
        {
            return null;
        }
        if (lookup is PdfIntNumber num)
        {

        }
        return null;
    }

    internal PdfValidator Clone()
    {
        return new PdfValidator(Document, FileSize)
        {
            Extensions = Extensions,
        };
    }

    internal bool Register<T>(IPdfObject obj) where T : ISpecification
    {
        if (!RunList.TryGetValue(obj, out var list))
        {
            list = new List<string>() { T.Name };
            RunList.Add(obj, list);
            return false;
        }

        if (!list.Contains(T.Name))
        {
            list.Add(T.Name);
            return false;
        }

        return true;
    }
    private ConditionalWeakTable<IPdfObject, List<string>> RunList = new ConditionalWeakTable<IPdfObject, List<string>>();
    private CallStack Current;
    internal void CallStack(XRef xr)
    {
        Current.Add(xr.ToString());
    }
    internal void CallStack(string key)
    {
        Current.Add(key);
    }
    internal void SetCallStack<T>(IPdfObject obj) where T : ISpecification
    {
        Current.Add<T>();
    }


    internal void Run<T, TT>(CallStack stack, TT obj, IPdfObject? parent) where T : ISpecification<TT> where TT : IPdfObject
    {
        var l = stack.Stack.Count;
        Current = stack;
        var grp = T.RuleGroup();
        if (grp && Register<T>(obj)) { return; }
        if (grp)
        {
            SetCallStack<T>(obj);
            stack.Add(obj);
        }
        if (!T.AppliesTo(Version, Extensions))
        {
            return;
        }

        var cnt = Errors.Count;
        T.Validate(this, stack, obj, parent);
        if (cnt == Errors.Count)
        {
            Passed.Add(string.Join(" -> ", stack.Stack));
        }
        if (stack.Stack.Count > l)
        {
            stack.Stack.RemoveRange(l, stack.Stack.Count - l);
        }
        if (grp)
        {
            stack.PopObj();
        }
    }

    internal void ValidatedLinkedDecodeParams(IPdfObject? obj, CallStack stack)
    {

    }

    internal void ValidateNameTree(IPdfObject? obj, Action<PdfString, IPdfObject>? action=null, bool root = true)
    {
        var boc = Current.Objects.Count;
        var bsc = Current.Stack.Count;
        if (obj == null)
        {
            Fail("NameTree", "Name tree did not exist");
            return;
        }
        if (!(obj is PdfDictionary dict))
        {
            Fail("NameTree", "Name tree was not a dictionary, was " + obj.Type);
            return;
        }

        var kids = dict.Get<PdfArray>(PdfName.Kids);
        var names = dict.Get<PdfArray>(PdfName.Kids);
        var limits = dict.Get<PdfArray>(PdfName.Limits);

        if (root && names == null && kids == null)
        {
            Fail("NameTree", "Name tree root did not have /Names or /Kids specified.");
        }

        if (kids != null && names != null)
        {
            Fail("NameTree", "Name tree specified both /Names and /Kids.");
        }

        if (kids != null)
        {
            CallStack("Kids");
            for (var i = 0; i < kids.Count; i++)
            {
                Reset(bsc+1, boc);
                CallStack(i.ToString());
                var c = kids[i];
                if (c.Type != PdfObjectType.IndirectRefObj)
                {
                    Fail("NameTree", "Name tree specified both /Names and /Kids.");
                }
                var ir = (PdfIndirectRef)c;
                CallStack(ir.Reference);
                var co = ir.GetObject();
                Current.Add(co);
                ValidateNameTree(co, action, false);
            }
        }

        Reset(bsc, boc);


        if (names != null)
        {
            if (limits == null)
            {
                Fail("NameTree", "Name tree with values did not have /Limits specified.");
            }

            CallStack("Names");

            // ensure sort
            string? prev = null;
            bool fail = false;
            for (var i = 0; i<names.Count; i++)
            {
                Reset(bsc+1, boc);
                CallStack((i/2).ToString());
                var k = names[i].Resolve();
                i++;
                if (i >= names.Count)
                {
                    Fail("NameTree", "Name tree with values did not have pairs.");
                    continue;
                }
                var v = names[i].Resolve();

                if (!(k is PdfString str))
                {
                    Fail("NameTree", "Name tree key was not a string, got " + k.Type);
                    continue;
                }

                if (prev != null && string.Compare(prev, str.Value) > -1)
                {
                    fail = true;
                    
                }

                action?.Invoke(str, v);
 
                // set min and max
                prev = str.Value;
            }
            if (fail)
            {
                Reset(bsc + 1, boc);
                Fail("NameTree", "Name tree keys were not in lexical order");
            }
        }

        void Reset(int sc, int oc)
        {
            if (Current.Stack.Count > sc)
            {
                Current.Stack.RemoveRange(sc, Current.Stack.Count - sc);
            }
            if (Current.Objects.Count > oc)
            {
                Current.Objects.RemoveRange(oc, Current.Objects.Count - oc);
            }
        }
    }

    internal (T? Obj, bool WasIndirect) GetOptional<T, TSpec>(PdfDictionary obj, string name, IndirectRequirement req) where T : class, IPdfObject where TSpec : ISpecification
    {
        var (val, wasIr) = GetOptional<TSpec>(obj, name, req);
        if (val == null) { return (default(T), wasIr); }
        if (val is T typed)
        {
            return (typed, wasIr);
        }
        Fail<TSpec>(name + $" entry is was of type {val.GetPdfObjType()} not {typeof(T).Name}");
        return (default(T), wasIr);
    }

    internal (IPdfObject? Obj, bool WasIndirect) GetOptional<TSpec>(PdfDictionary obj, string name, IndirectRequirement req) where TSpec : ISpecification
    {
        CallStack(name);
        if (!obj.TryGetValue(name, out var val))
        {
            return (null, false);
        }

        var ir = false;

        if (req == IndirectRequirement.MustBeIndirect)
        {
            if (val.Type != PdfObjectType.IndirectRefObj)
            {
                CallStack(name);
                Fail<TSpec>(name + " must be indirect reference");
            }
            else
            {
                ir = true;
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
                val = val.Resolve();
            }
        }
        else if (req == IndirectRequirement.MustBeDirect)
        {
            if (val.Type == PdfObjectType.IndirectRefObj)
            {
                ir = true;
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
                Fail<TSpec>(name + " must not be indirect reference");
                val = val.Resolve();
            }
        }
        else
        {
            ir = val.Type == PdfObjectType.IndirectRefObj;
            if (ir)
            {
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
            }
            val = val.Resolve();
        }
        return (val, ir);
    }

    internal (T? Obj, bool WasIndirect) GetRequired<T, TSpec>(PdfDictionary obj, string name, IndirectRequirement req) where T : class, IPdfObject where TSpec : ISpecification
    {
        var (val, wasIr) = GetOptional<TSpec>(obj, name, req);
        if (val == null)
        {
            Fail<TSpec>(name + " entry is missing");
            return (default(T), wasIr);
        }
        if (val is T typed)
        {
            return (typed, wasIr);
        }
        Fail<TSpec>(name + $" entry is was of type {val.GetPdfObjType()} not {typeof(T).Name}");
        return (default(T), wasIr);
    }

    internal (IPdfObject? Obj, bool WasIndirect) GetOptional<TSpec>(PdfArray obj, int i, IndirectRequirement req) where TSpec : ISpecification
    {
        CallStack(i.ToString());

        var val = obj.Get(i);
        if (val == null)
        {
            return (null, false);
        }

        var ir = false;

        if (req == IndirectRequirement.MustBeIndirect)
        {
            if (val.Type != PdfObjectType.IndirectRefObj)
            {
                Fail<TSpec>(i.ToString() + " must be indirect reference");
            }
            else
            {
                ir = true;
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
                val = val.Resolve();
            }
        }
        else if (req == IndirectRequirement.MustBeDirect)
        {
            if (val.Type == PdfObjectType.IndirectRefObj)
            {
                ir = true;
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
                Fail<TSpec>(i.ToString() + " must not be indirect reference");
                val = val.Resolve();
            }
        }
        else
        {
            ir = val.Type == PdfObjectType.IndirectRefObj;
            if (ir)
            {
                var irObj = (PdfIndirectRef)val;
                CallStack(irObj.Reference);
            }
            val = val.Resolve();
        }
        return (val, ir);

    }
    internal (T? Obj, bool WasIndirect) GetOptional<T, TSpec>(PdfArray obj, int i, IndirectRequirement req) where T : class, IPdfObject where TSpec : ISpecification
    {
        var (val, wasIr) = GetOptional<TSpec>(obj, i, req);
        if (val == null) { return (default(T), wasIr); }
        if (val is T typed)
        {
            return (typed, wasIr);
        }
        Fail<TSpec>(i.ToString() + $" entry is was of type {val.GetPdfObjType()} not {typeof(T).Name}");
        return (default(T), wasIr);

    }

    internal (T? Obj, bool WasIndirect) GetRequired<T, TSpec>(PdfArray obj, int i, IndirectRequirement req) where T : class, IPdfObject where TSpec : ISpecification
    {
        var (val, wasIr) = GetOptional<TSpec>(obj, i, req);
        if (val == null)
        {
            Fail<TSpec>(i.ToString() + " entry is missing");
            return (default(T), wasIr);
        }
        if (val is T typed)
        {
            return (typed, wasIr);
        }
        Fail<TSpec>(i.ToString() + $" entry is was of type {val.GetPdfObjType()} not {typeof(T).Name}");
        return (default(T), wasIr);
    }
}
