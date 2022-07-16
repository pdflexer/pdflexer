using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfTrailer
    {
        public PdfDictionary Dictionary { get; }
        public PdfTrailer()
        {
            Dictionary = new PdfDictionary();
        }
        // Size -> total xrefs
        // Root xref
        // Encrypt (if encrypted)
        // Info (optional) xref
        // ID required if encrypted, two byte strings
    }

    public class PageTreeRoot
    {
        public PdfDictionary Dictionary { get; }
        public PageTreeRoot()
        {
            Dictionary = new PdfDictionary();
            Dictionary[PdfName.TypeName] = PdfName.Pages;
        }

        // Kids => array IR to children
        // Count => sum of all descendant pages
    }

    public class PageTreeNode
    {
        public PdfDictionary Dictionary { get; }
        public PageTreeNode()
        {
            Dictionary = new PdfDictionary();
            Dictionary[PdfName.TypeName] = PdfName.Pages;
        }

        public PageTreeNode(PdfDictionary dict)
        {
            Dictionary = dict;
        }

        public PdfArray Kids
        {
            get
            {
                var k = Dictionary.GetOrCreateValue<PdfArray>(PdfName.Kids);
                k.IndirectOnly = true;
                return k;
            } 
            set => Dictionary[PdfName.Kids] = value;
        }

        public static implicit operator PageTreeNode(PdfDictionary dict) => new PageTreeNode(dict);
        public static implicit operator PdfDictionary(PageTreeNode page) => page.Dictionary;

        // Parent => IR dict
        // Kids => array IR to children
        // Count => sum of all descendant pages
    }
}
