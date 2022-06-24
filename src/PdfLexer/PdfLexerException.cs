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

    public class PdfLexerObjectMismatchException : PdfLexerException
    {
        public PdfLexerObjectMismatchException(string message) : base(message)
        {

        }
    }

    public class PdfLexerTokenMismatchException : PdfLexerException
    {
        public PdfLexerTokenMismatchException(string message) : base(message)
        {

        }
    }
}
