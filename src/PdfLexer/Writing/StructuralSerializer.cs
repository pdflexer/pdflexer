using PdfLexer.DOM;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Writing;

internal class StructuralSerializer
{
    private readonly Dictionary<PdfPage, PdfIndirectRef>? _pageMap;
    private readonly Dictionary<PdfDictionary, PdfDictionary>? _annotationMap;

    public StructuralSerializer(Dictionary<PdfPage, PdfIndirectRef>? pageMap = null, Dictionary<PdfDictionary, PdfDictionary>? annotationMap = null)
    {
        _pageMap = pageMap;
        _annotationMap = annotationMap;
    }

    private PdfIndirectRef GetPageRef(PdfPage page)
    {
        if (_pageMap != null && _pageMap.TryGetValue(page, out var ir))
        {
            return ir;
        }
        return page.NativeObject.Indirect();
    }

    public (PdfDictionary Root, Dictionary<StructureNode, PdfIndirectRef> Map) ConvertToPdf(StructureNode rootNode)
    {
        var structureRoot = EnsureStructureRoot(rootNode);
        var items = new List<(StructureNode Node, PdfDictionary Dict, PdfIndirectRef Ref)>();
        CollectNodes(rootNode, items);

        var nodeMap = items.ToDictionary(x => x.Node, x => x.Ref);
        var map = items.ToDictionary(x => x.Node, x => (x.Dict, x.Ref));
        var namespaceMap = CreateNamespaceMap(structureRoot);

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

            if (!string.IsNullOrEmpty(node.Alt))
            {
                dict[PdfName.Alt] = new PdfString(node.Alt);
            }

            if (!string.IsNullOrEmpty(node.ActualText))
            {
                dict[PdfName.ActualText] = new PdfString(node.ActualText);
            }

            if (!string.IsNullOrEmpty(node.Expansion))
            {
                dict[PdfName.E] = new PdfString(node.Expansion);
            }

            if (!string.IsNullOrEmpty(node.Language))
            {
                dict[PdfName.Lang] = new PdfString(node.Language);
            }

            if (node.Classes.Count == 1)
            {
                dict[PdfName.Class] = (PdfName)node.Classes[0];
            }
            else if (node.Classes.Count > 1)
            {
                dict[PdfName.Class] = new PdfArray(node.Classes.Select(x => (IPdfObject)(PdfName)x).ToList());
            }

            if (node.Namespace != null && namespaceMap.TryGetValue(node.Namespace, out var nsRef))
            {
                dict[PdfName.NS] = nsRef;
            }

            if (node.References.Count > 0)
            {
                var refs = new PdfArray();
                foreach (var refId in node.References)
                {
                    if (structureRoot.IdMap.TryGetValue(refId, out var targetNode) && nodeMap.TryGetValue(targetNode, out var targetRef))
                    {
                        refs.Add(targetRef);
                    }
                }

                if (refs.Count > 0)
                {
                    dict[PdfName.Ref] = refs;
                }
            }

            var attributes = BuildAttributes(node, map, structureRoot);
            if (attributes.Count == 1)
            {
                dict[PdfName.A] = attributes[0];
            }
            else if (attributes.Count > 1)
            {
                dict[PdfName.A] = new PdfArray(attributes.Cast<IPdfObject>().ToList());
            }

            var kids = new PdfArray();
            foreach (var child in node.Children)
            {
                if (map.TryGetValue(child, out var childInfo))
                {
                    kids.Add(childInfo.Ref);
                    childInfo.Dict[PdfName.P] = ir;
                }
            }

            var pages = node.ContentItems.Select(x => x.Page).Distinct().ToList();
            if (pages.Count == 1)
            {
                dict[PdfName.Pg] = GetPageRef(pages[0]);
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
                    mcr[PdfName.Pg] = GetPageRef(item.Page);
                    mcr[PdfName.MCID] = new PdfIntNumber(item.MCID);
                    kids.Add(mcr);
                }
            }

            foreach (var item in node.XObjectContentItems)
            {
                var mcr = new PdfDictionary();
                mcr[PdfName.TYPE] = PdfName.MCR;
                mcr[PdfName.Stm] = item.Form.NativeObject.Indirect();
                mcr[PdfName.MCID] = new PdfIntNumber(item.MCID);
                kids.Add(mcr);
            }

            foreach (var objRef in node.ObjectReferences)
            {
                ApplyAnnotationAccessibility(objRef, nodeMap);
                kids.Add(BuildObjectReference(objRef));
            }

            if (kids.Count == 1)
            {
                dict[PdfName.K] = kids[0];
            }
            else if (kids.Count > 1)
            {
                dict[PdfName.K] = kids;
            }
        }

        var root = new PdfDictionary();
        root[PdfName.TYPE] = PdfName.StructTreeRoot;
        var rootRef = PdfIndirectRef.Create(root);

        if (map.TryGetValue(rootNode, out var rootInfo))
        {
            root[PdfName.K] = rootInfo.Ref;
            rootInfo.Dict[PdfName.P] = rootRef;
        }
        else if (rootNode.Children.Count > 0)
        {
            var rootKids = new PdfArray();
            foreach (var child in rootNode.Children)
            {
                if (map.TryGetValue(child, out var childInfo))
                {
                    childInfo.Dict[PdfName.P] = rootRef;
                    rootKids.Add(childInfo.Ref);
                }
            }

            if (rootKids.Count == 1)
            {
                root[PdfName.K] = rootKids[0];
            }
            else if (rootKids.Count > 1)
            {
                root[PdfName.K] = rootKids;
            }
        }

        var idTree = BuildIdTree(structureRoot, nodeMap);
        if (idTree != null)
        {
            root[PdfName.IDTree] = idTree;
        }

        var roleMap = BuildRoleMap(structureRoot);
        if (roleMap != null)
        {
            root[PdfName.RoleMap] = roleMap;
        }

        var classMap = BuildClassMap(structureRoot);
        if (classMap != null)
        {
            root[PdfName.ClassMap] = classMap;
        }

        if (namespaceMap.Count > 0)
        {
            root[PdfName.Namespaces] = new PdfArray(namespaceMap.Values.Cast<IPdfObject>().ToList());
        }

        root[PdfName.ParentTree] = BuildParentTree(items);

        return (root, nodeMap);
    }

    private static List<PdfDictionary> BuildAttributes(
        StructureNode node,
        Dictionary<StructureNode, (PdfDictionary Dict, PdfIndirectRef Ref)> map,
        StructureRoot structureRoot)
    {
        var attributes = new List<PdfDictionary>(node.Attributes);

        if (node.Scope.HasValue || node.Headers.Count > 0 || !string.IsNullOrEmpty(node.Summary))
        {
            var table = new PdfDictionary
            {
                [PdfName.O] = PdfName.Table
            };

            if (node.Scope.HasValue)
            {
                table[PdfName.Scope] = node.Scope.Value switch
                {
                    StructureScope.Row => PdfName.Row,
                    StructureScope.Column => PdfName.Column,
                    _ => PdfName.Both
                };
            }

            if (node.Headers.Count > 0)
            {
                var headers = new PdfArray();
                foreach (var headerId in node.Headers)
                {
                    if (structureRoot.IdMap.TryGetValue(headerId, out var headerNode) && map.TryGetValue(headerNode, out var headerRef))
                    {
                        headers.Add(headerRef.Ref);
                    }
                }

                if (headers.Count > 0)
                {
                    table[PdfName.Headers] = headers;
                }
            }

            if (!string.IsNullOrEmpty(node.Summary))
            {
                table[PdfName.Summary] = new PdfString(node.Summary);
            }

            attributes.Add(table);
        }

        if (node.ListNumbering.HasValue)
        {
            var list = new PdfDictionary
            {
                [PdfName.O] = new PdfName("List"),
                [PdfName.ListNumbering] = node.ListNumbering.Value switch
                {
                    StructureListNumbering.Decimal => PdfName.Decimal,
                    StructureListNumbering.LowerRoman => PdfName.LowerRoman,
                    StructureListNumbering.UpperRoman => PdfName.UpperRoman,
                    StructureListNumbering.LowerAlpha => PdfName.LowerAlpha,
                    StructureListNumbering.UpperAlpha => PdfName.UpperAlpha,
                    StructureListNumbering.Disc => PdfName.Disc,
                    StructureListNumbering.Circle => PdfName.Circle,
                    StructureListNumbering.Square => PdfName.Square,
                    _ => PdfName.None
                }
            };
            attributes.Add(list);
        }

        return attributes;
    }

    private static PdfIndirectRef? BuildIdTree(StructureRoot structureRoot, Dictionary<StructureNode, PdfIndirectRef> nodeMap)
    {
        if (structureRoot.IdMap.Count == 0)
        {
            return null;
        }

        var names = new PdfArray();
        foreach (var entry in structureRoot.IdMap.OrderBy(x => x.Key, System.StringComparer.Ordinal))
        {
            if (!nodeMap.TryGetValue(entry.Value, out var nodeRef))
            {
                continue;
            }

            names.Add(new PdfString(entry.Key));
            names.Add(nodeRef);
        }

        if (names.Count == 0)
        {
            return null;
        }

        var tree = new PdfDictionary
        {
            [PdfName.Names] = names
        };
        return PdfIndirectRef.Create(tree);
    }

    private static PdfDictionary? BuildRoleMap(StructureRoot structureRoot)
    {
        if (structureRoot.RoleMap.Count == 0)
        {
            return null;
        }

        var roleMap = new PdfDictionary();
        foreach (var entry in structureRoot.RoleMap)
        {
            roleMap[(PdfName)entry.Key] = (PdfName)entry.Value;
        }

        return roleMap;
    }

    private static PdfDictionary? BuildClassMap(StructureRoot structureRoot)
    {
        if (structureRoot.ClassMap.Count == 0)
        {
            return null;
        }

        var classMap = new PdfDictionary();
        foreach (var entry in structureRoot.ClassMap)
        {
            classMap[(PdfName)entry.Key] = entry.Value;
        }

        return classMap;
    }

    private static Dictionary<StructureNamespace, PdfIndirectRef> CreateNamespaceMap(StructureRoot structureRoot)
    {
        var nsMap = new Dictionary<StructureNamespace, PdfIndirectRef>();
        foreach (var ns in structureRoot.Namespaces)
        {
            var nsDict = new PdfDictionary
            {
                [PdfName.TYPE] = new PdfName("Namespace"),
                [PdfName.NS] = new PdfString(ns.Uri)
            };
            nsMap[ns] = PdfIndirectRef.Create(nsDict);
        }

        return nsMap;
    }

    private static StructureRoot EnsureStructureRoot(StructureNode rootNode)
    {
        if (rootNode.Root != null)
        {
            return rootNode.Root;
        }

        var structureRoot = new StructureRoot();
        structureRoot.AttachSubtree(rootNode);
        return structureRoot;
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
        var pageMap = new Dictionary<PdfPage, SortedDictionary<int, IPdfObject>>();
        var xobjectMap = new Dictionary<XObjForm, SortedDictionary<int, IPdfObject>>();
        var directEntries = new SortedDictionary<int, IPdfObject>();
        var participantPages = new HashSet<PdfPage>();

        foreach (var (node, _, ir) in items)
        {
            foreach (var content in node.ContentItems)
            {
                if (!pageMap.TryGetValue(content.Page, out var mcidMap))
                {
                    mcidMap = new SortedDictionary<int, IPdfObject>();
                    pageMap[content.Page] = mcidMap;
                }
                mcidMap[content.MCID] = ir;
                participantPages.Add(content.Page);
            }

            foreach (var content in node.XObjectContentItems)
            {
                if (!xobjectMap.TryGetValue(content.Form, out var mcidMap))
                {
                    mcidMap = new SortedDictionary<int, IPdfObject>();
                    xobjectMap[content.Form] = mcidMap;
                }
                mcidMap[content.MCID] = ir;
            }

            foreach (var objRef in node.ObjectReferences)
            {
                directEntries[objRef.StructParentIndex] = ir;
                foreach (var page in objRef.Pages)
                {
                    participantPages.Add(page);
                }
                if (objRef.Object.Resolve() is PdfDictionary objDict)
                {
                    if (_annotationMap != null && _annotationMap.TryGetValue(objDict, out var mapped))
                    {
                        objDict = mapped;
                    }
                    objDict[PdfName.StructParent] = new PdfIntNumber(objRef.StructParentIndex);
                }
            }

            foreach (var xObjectRef in node.XObjectReferences)
            {
                directEntries[xObjectRef.StructParentsIndex] = ir;
                foreach (var page in xObjectRef.Pages)
                {
                    participantPages.Add(page);
                }
                if (xObjectRef.XObject.Resolve() is PdfDictionary xObjectDict)
                {
                    xObjectDict[PdfName.StructParents] = new PdfIntNumber(xObjectRef.StructParentsIndex);
                }
            }
        }

        var nums = new PdfArray();
        var usedIndexes = new HashSet<int>(directEntries.Keys);
        var nextPageIndex = 0;

        foreach (var entry in pageMap)
        {
            while (usedIndexes.Contains(nextPageIndex))
            {
                nextPageIndex++;
            }

            var page = entry.Key;
            var mcids = entry.Value;

            if (_pageMap != null && _pageMap.TryGetValue(page, out var ir))
            {
                if (ir.GetObject() is PdfDictionary pageDict)
                {
                    pageDict[PdfName.StructParents] = new PdfIntNumber(nextPageIndex);
                }
            }
            else
            {
                page.NativeObject[PdfName.StructParents] = new PdfIntNumber(nextPageIndex);
            }

            var mcidArray = new PdfArray();
            var maxMcid = mcids.Keys.Max();
            for (var i = 0; i <= maxMcid; i++)
            {
                mcidArray.Add(mcids.TryGetValue(i, out var refObj) ? refObj : PdfNull.Value);
            }

            directEntries[nextPageIndex] = mcidArray;
            usedIndexes.Add(nextPageIndex);
            nextPageIndex++;
        }

        foreach (var entry in xobjectMap)
        {
            var form = entry.Key;
            var mcids = entry.Value;
            var index = (int?)form.StructParents ?? -1;
            if (index < 0)
            {
                while (usedIndexes.Contains(nextPageIndex))
                {
                    nextPageIndex++;
                }
                index = nextPageIndex++;
                form.StructParents = new PdfIntNumber(index);
            }

            var mcidArray = new PdfArray();
            var maxMcid = mcids.Keys.Max();
            for (var i = 0; i <= maxMcid; i++)
            {
                mcidArray.Add(mcids.TryGetValue(i, out var refObj) ? refObj : PdfNull.Value);
            }

            directEntries[index] = mcidArray;
            usedIndexes.Add(index);
        }

        foreach (var page in participantPages)
        {
            if (pageMap.ContainsKey(page))
            {
                continue;
            }

            while (usedIndexes.Contains(nextPageIndex))
            {
                nextPageIndex++;
            }

            if (_pageMap != null && _pageMap.TryGetValue(page, out var ir))
            {
                if (ir.GetObject() is PdfDictionary pageDict)
                {
                    pageDict[PdfName.StructParents] = new PdfIntNumber(nextPageIndex);
                }
            }
            else
            {
                page.NativeObject[PdfName.StructParents] = new PdfIntNumber(nextPageIndex);
            }

            directEntries[nextPageIndex] = new PdfArray();
            usedIndexes.Add(nextPageIndex);
            nextPageIndex++;
        }

        foreach (var entry in directEntries)
        {
            nums.Add(new PdfIntNumber(entry.Key));
            nums.Add(entry.Value);
        }

        var parentTree = new PdfDictionary();
        parentTree[PdfName.Nums] = nums;
        return PdfIndirectRef.Create(parentTree);
    }

    private PdfDictionary BuildObjectReference(StructureObjectReference objRef)
    {
        var objr = new PdfDictionary
        {
            [PdfName.TYPE] = PdfName.OBJR,
            [PdfName.Obj] = GetSerializedObjectRef(objRef.Object)
        };

        if (objRef.Pages.Count == 1)
        {
            objr[PdfName.Pg] = GetPageRef(objRef.Pages[0]);
        }

        return objr;
    }

    private void ApplyAnnotationAccessibility(
        StructureObjectReference objRef,
        Dictionary<StructureNode, PdfIndirectRef> nodeMap)
    {
        var annotation = GetSerializedObjectDictionary(objRef.Object);
        if (annotation == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(objRef.AnnotationContents))
        {
            annotation[PdfName.Contents] = new PdfString(objRef.AnnotationContents);
        }

        if (annotation.Get<PdfName>(PdfName.Subtype) != PdfName.Link ||
            objRef.StructureDestinationTarget == null)
        {
            return;
        }

        if (!nodeMap.TryGetValue(objRef.StructureDestinationTarget, out var targetRef))
        {
            throw new PdfAccessibilityConformanceException(
                "A structure-targeted link references a destination node that was not serialized into the structure tree.");
        }

        annotation[PdfName.Dest] = BuildStructureDestination(objRef, targetRef);
        annotation.Remove(PdfName.A);
    }

    private static PdfArray BuildStructureDestination(StructureObjectReference objRef, PdfIndirectRef targetRef)
    {
        var template = objRef.StructureDestinationTemplate;
        if (template == null || template.Count == 0)
        {
            return new PdfArray { targetRef, PdfName.Fit };
        }

        var destination = new PdfArray { targetRef };
        for (var i = 1; i < template.Count; i++)
        {
            destination.Add(template[i]);
        }

        return destination;
    }

    private IPdfObject GetSerializedObjectRef(IPdfObject obj)
    {
        if (GetSerializedObjectDictionary(obj) is PdfDictionary dict)
        {
            return dict.Indirect();
        }

        return obj is PdfIndirectRef ir ? ir : obj.Indirect();
    }

    private PdfDictionary? GetSerializedObjectDictionary(IPdfObject obj)
    {
        if (_annotationMap != null && obj.Resolve() is PdfDictionary dict && _annotationMap.TryGetValue(dict, out var mapped))
        {
            return mapped;
        }

        return obj.Resolve().Type == PdfObjectType.DictionaryObj
            ? obj.Resolve().GetAs<PdfDictionary>()
            : null;
    }
}
