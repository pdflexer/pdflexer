namespace PdfLexer;

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

public class PdfLexerPasswordException : PdfLexerException
{
    public PdfLexerPasswordException(string message) : base(message)
    {

    }
}

public class PdfAccessibilitySetupException : PdfLexerException
{
    public PdfAccessibilitySetupException(string message) : base(message)
    {

    }
}

public class PdfAccessibilityTextException : PdfLexerException
{
    public PdfAccessibilityTextException(string message) : base(message)
    {

    }
}

public class PdfAccessibilityConformanceException : PdfLexerException
{
    public PdfAccessibilityConformanceException(string message) : base(message)
    {

    }
}
