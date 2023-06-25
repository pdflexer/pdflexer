using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Content.Model;

/// <summary>
/// Type of content the content group represents
/// </summary>
public enum ContentType
{
    Text,
    Paths,
    Image,
    Form,
    Shading,
    // MarkedPoint
}