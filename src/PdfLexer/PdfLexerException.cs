using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfLexerException : Exception
    {
        public PdfLexerException()
        {

        }

        public PdfLexerException(string message) : base(message)
        {

        }

        public PdfLexerException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

    public class PdfTokenMismatchException : PdfLexerException
    {
        public PdfTokenMismatchException(string message) : base(message)
        {

        }
    }
}
