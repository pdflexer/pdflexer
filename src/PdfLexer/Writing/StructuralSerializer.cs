using PdfLexer.DOM;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Writing;

internal class StructuralSerializer
{
    public StructuralSerializer()
    {
    }

    public (PdfDictionary Root, Dictionary<StructureNode, PdfIndirectRef> Map) ConvertToPdf(StructureNode rootNode)
    {
        // 1. Flatten the tree and create dictionaries for all nodes
        var items = new List<(StructureNode Node, PdfDictionary Dict, PdfIndirectRef Ref)>();
        
        CollectNodes(rootNode, items);

        var nodeMap = items.ToDictionary(x => x.Node, x => x.Ref);
        var map = items.ToDictionary(x => x.Node, x => (x.Dict, x.Ref));

        // 2. Populate dictionaries and link hierarchy
        foreach (var (node, dict, ir) in items)
        {
            dict[PdfName.TYPE] = PdfName.StructElem;
            dict[PdfName.S] = (PdfName)node.Type;
            
            if (!string.IsNullOrEmpty(node.ID))
            {
                dict[PdfName.ID] = new PdfString(node.ID);
            }
            
            if (!string.IsNullOrEmpty(node.Title))
            {
                dict[PdfName.T] = new PdfString(node.Title);
            }

            if (!string.IsNullOrEmpty(node.AlternateText))
            {
                dict[PdfName.ALT] = new PdfString(node.AlternateText);
            }

            if (!string.IsNullOrEmpty(node.Language))
            {
                dict[PdfName.Lang] = new PdfString(node.Language);
            }

            if (node.Attributes.Count > 0)
            {
                if (node.Attributes.Count == 1)
                {
                    dict[PdfName.A] = node.Attributes[0];
                }
                else
                {
                    dict[PdfName.A] = new PdfArray(node.Attributes.Cast<IPdfObject>().ToList());
                }
            }

            // Kids (/K)
            var kids = new PdfArray();
            
            // Add child nodes
            foreach (var child in node.Children)
            {
                if (map.TryGetValue(child, out var childInfo))
                {
                    kids.Add(childInfo.Ref);
                    childInfo.Dict[PdfName.Parent] = ir;
                }
            }

            // Add content items (MCIDs)
            var pages = node.ContentItems.Select(x => x.Page).Distinct().ToList();
            if (pages.Count == 1)
            {
                dict[PdfName.Pg] = pages[0].NativeObject.Indirect();
                foreach (var item in node.ContentItems)
                {
                    kids.Add(new PdfIntNumber(item.MCID));
                }
            }
            else
            {
                foreach (var item in node.ContentItems)
                {
                    var mcr = new PdfDictionary();
                    mcr[PdfName.TYPE] = PdfName.MCR;
                    mcr[PdfName.Pg] = item.Page.NativeObject.Indirect();
                    mcr[PdfName.MCID] = new PdfIntNumber(item.MCID);
                    kids.Add(mcr);
                }
            }

            if (kids.Count > 0)
            {
                if (kids.Count == 1)
                {
                    dict[PdfName.K] = kids[0];
                }
                else
                {
                    dict[PdfName.K] = kids;
                }
            }
        }

        // 3. Create StructTreeRoot
        var root = new PdfDictionary();
        root[PdfName.TYPE] = PdfName.StructTreeRoot;
        
        if (map.TryGetValue(rootNode, out var rootInfo))
        {
            root[PdfName.K] = rootInfo.Ref;
        }
        else if (rootNode.Children.Count > 0)
        {
            var rootKids = new PdfArray();
            foreach (var child in rootNode.Children)
            {
                if (map.TryGetValue(child, out var childInfo))
                {
                    rootKids.Add(childInfo.Ref);
                }
            }
            root[PdfName.K] = rootKids;
        }

        // 4. Build ParentTree
        root[PdfName.ParentTree] = BuildParentTree(items);

        return (root, nodeMap);
    }

    private void CollectNodes(StructureNode node, List<(StructureNode Node, PdfDictionary Dict, PdfIndirectRef Ref)> collected)
    {
        var dict = new PdfDictionary();
        var ir = PdfIndirectRef.Create(dict);
        collected.Add((node, dict, ir));
        foreach (var child in node.Children)
        {
            CollectNodes(child, collected);
        }
    }

    private IPdfObject BuildParentTree(List<(StructureNode Node, PdfDictionary Dict, PdfIndirectRef Ref)> items)
    {
        // Mapping: Page -> List of (MCID, StructElemRef)
        var pageMap = new Dictionary<PdfPage, SortedDictionary<int, IPdfObject>>();

        foreach (var (node, dict, ir) in items)
        {
            foreach (var content in node.ContentItems)
            {
                if (!pageMap.TryGetValue(content.Page, out var mcidMap))
                {
                    mcidMap = new SortedDictionary<int, IPdfObject>();
                    pageMap[content.Page] = mcidMap;
                }
                mcidMap[content.MCID] = ir;
            }
        }

        // Create the Number Tree
        // Each page gets a StructParents index
        var nums = new PdfArray();
        int pageIndex = 0;
        foreach (var entry in pageMap)
        {
            var page = entry.Key;
            var mcids = entry.Value;

            page.NativeObject[PdfName.StructParents] = new PdfIntNumber(pageIndex);

            // The value in ParentTree for this page is an array of StructElem refs, 
            // indexed by MCID.
            var mcidArray = new PdfArray();
            int maxMcid = mcids.Keys.Max();
            for (int i = 0; i <= maxMcid; i++)
            {
                if (mcids.TryGetValue(i, out var refObj))
                {
                    mcidArray.Add(refObj);
                }
                else
                {
                    mcidArray.Add(PdfNull.Value);
                }
            }

            nums.Add(new PdfIntNumber(pageIndex));
            nums.Add(PdfIndirectRef.Create(mcidArray));
            pageIndex++;
        }

        var parentTree = new PdfDictionary();
        parentTree[PdfName.Nums] = nums;
        return PdfIndirectRef.Create(parentTree);
    }
}