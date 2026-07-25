using System.Runtime.CompilerServices;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;

namespace PdfLexer.Writing;

/// <summary>
/// Allocates marked-content identifiers in the namespace of the underlying
/// page or form stream rather than in the lifetime of an individual writer.
/// </summary>
internal static class McidAllocator
{
    private static readonly ConditionalWeakTable<IPdfObject, AllocationState> States = new();

    internal static void Prepare(PdfPage page, PageWriteMode mode)
    {
        var state = States.GetValue(page.NativeObject, _ => new AllocationState());
        lock (state)
        {
            if (mode == PageWriteMode.Replace)
            {
                if (state.Initialized && state.Next > 0)
                {
                    throw new PdfAccessibilityConformanceException(
                        "PageWriteMode.Replace cannot be used on a page after structure elements have already been bound to MCIDs on that page.");
                }
                state.Next = 0;
                state.Initialized = true;
                return;
            }

            if (!state.Initialized)
            {
                Refresh(state, FindHighestMcid(page.GetContentNodes()), "page");
            }
        }
    }

    internal static int Allocate(PdfPage page)
    {
        var state = States.GetValue(page.NativeObject, _ => new AllocationState());
        lock (state)
        {
            if (!state.Initialized)
            {
                Refresh(state, FindHighestMcid(page.GetContentNodes()), "page");
            }

            return AllocateNext(state, "page");
        }
    }

    internal static int Allocate(XObjForm form)
    {
        var state = States.GetValue(form.NativeObject, _ => new AllocationState());
        lock (state)
        {
            if (!state.Initialized)
            {
                Refresh(state, FindHighestMcid(form), "form XObject");
            }

            return AllocateNext(state, "form XObject");
        }
    }

    private static void Refresh(AllocationState state, int highestMcid, string owner)
    {
        var next = highestMcid < 0 ? 0 : GetSuccessor(highestMcid, owner);
        if (!state.Initialized || state.Next < next)
        {
            state.Next = next;
        }

        state.Initialized = true;
    }

    private static int AllocateNext(AllocationState state, string owner)
    {
        var allocated = state.Next;
        state.Next = GetSuccessor(allocated, owner);
        return allocated;
    }

    private static int GetSuccessor(int value, string owner)
    {
        if (value == int.MaxValue)
        {
            throw new PdfAccessibilityConformanceException(
                $"Cannot allocate another MCID for this {owner}; the existing MCID range is exhausted.");
        }

        return value + 1;
    }

    private static int FindHighestMcid(XObjForm form)
    {
        if (form.Contents == null || form.Contents.Length == 0)
        {
            return -1;
        }

        var parent = new PdfDictionary();
        if (form.Resources != null)
        {
            parent[PdfName.Resources] = form.Resources;
        }

        var parser = new ContentModelParser<double>(
            ParsingContext.Current,
            parent,
            form.NativeObject,
            new GfxState<double>());
        return FindHighestMcid(parser.Parse());
    }

    private static int FindHighestMcid(IEnumerable<IContentNode<double>> nodes)
    {
        var highest = -1;
        foreach (var node in nodes)
        {
            if (node is not MarkedContentGroup<double> marked)
            {
                // Form XObjects have their own MCID namespace and are deliberately
                // not traversed while scanning a page or an outer form.
                continue;
            }

            var value = marked.Tag.InlineProps?.Get<PdfNumber>(PdfName.MCID) ??
                        marked.Tag.PropList?.Get<PdfNumber>(PdfName.MCID);
            if (value != null)
            {
                highest = Math.Max(highest, (int)value);
            }

            highest = Math.Max(highest, FindHighestMcid(marked.Children));
        }

        return highest;
    }

    private sealed class AllocationState
    {
        internal bool Initialized { get; set; }
        internal int Next { get; set; }
    }
}
