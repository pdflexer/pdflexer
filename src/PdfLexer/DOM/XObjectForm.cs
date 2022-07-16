using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class XObjectForm
    {
        public PdfDictionary Dictionary { get; }
        public XObjectForm(PdfDictionary dict)
        {
            Dictionary = dict;
        }

        // Type -> XObject (optional)
        // Subtype => Form
        // FormType => 1 (optional)
        // BBox req => PdfRectangle clipping box
        // Matrix opt => transformation matrix
        // Resources => same as page
        // Group opt
        // Ref opt
        // Metadata opt
        // PieceInfo opt
        // LastModified opt
        // StructParent req if structural
        // StructParents req if structural
        // OPT opt
        // OC opt
        // Name req in pdf v1 -> not recommended
        // + stream items
    }
}
