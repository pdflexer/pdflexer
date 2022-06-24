using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PdfLexer.Legacy
{
    [ExcludeFromCodeCoverage]
    public class NestedSkipper
    {
        internal int ArrayDepth = 0;
        internal int DictDepth = 0;
        internal int StringDepth = 0;
        public bool HadIndirectObjects {get; private set; }

        public bool TryScanToEndOfString(ref SequenceReader<byte> reader)
        {
            if (!reader.TryPeek(out byte b))
            {
                return false;
            }

            if (b == (byte)'(')
            {
                return  TryAdvancePastStringLiteral(ref reader);
            } else if (b == (byte)'<')
            {
                return reader.TryAdvanceTo((byte) '>', true);
            }

            throw new ApplicationException("Invalid string, first char not ( or <.");

        }

        private static byte[] stringItems = new byte[]
        {
            (byte) '\\', (byte) '(', (byte) ')'
        };
        private bool TryAdvancePastStringLiteral(ref SequenceReader<byte> reader)
        {
            while (reader.TryAdvanceToAny(stringItems, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {
                    case (byte) '\\':
                        if (!reader.TryRead(out var b2))
                        {
                            return false;
                        }

                        continue;
                    case (byte) '(':
                        StringDepth++;
                        continue;
                    case (byte) ')':
                        StringDepth--;
                        Debug.Assert(StringDepth >= 0, "StringDepth >= 0");
                        if (ArrayDepth == 0)
                        {
                            return true;
                        }

                        continue;
                }
            }

            return ArrayDepth == 0;
        }

        public bool TryScanToEndOfArray(ref SequenceReader<byte> reader)
        {
            if (DictDepth > 0)
            {
                if (!TryScanToEndOfDict(ref reader))
                {
                    return false;
                }
            }

            while (reader.TryAdvanceToAny(NestedUtil.arrayScanTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {
                    case (byte) 'R':
                    {
                        HadIndirectObjects = true;
                        continue;
                    }
                    case (byte) '<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '<')
                        {
                            // new dict
                            if (!reader.TryRead(out byte _))
                            {
                                reader.Rewind(1);
                                return false;
                            }

                            DictDepth++;
                            if (!TryScanToEndOfDict(ref reader))
                            {
                                return false;
                            }
                        }

                        // just hex string
                    }
                        continue;
                    case (byte) '(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
                    case (byte) '[':
                    {
                        ArrayDepth++;
                        if (!TryScanToEndOfArray(ref reader))
                        {
                            return false;
                        }
                        continue;
                    }
                    case (byte) ']':
                        ArrayDepth--;
                        Debug.Assert(ArrayDepth >= 0, "ArrayDepth >= 0");
                        if (ArrayDepth == 0)
                        {
                            return true;
                        }
                        continue;
                        
                }
            }

            return false;
        }

        public bool TryScanToEndOfDict(ref SequenceReader<byte> reader)
        {
            while (reader.TryAdvanceToAny(NestedUtil.dictScanTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {                    
                    case (byte) 'R':
                    {
                        HadIndirectObjects = true;
                        continue;
                    }
                    case (byte) '<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '<')
                        {
                            // new dict
                            if (!reader.TryRead(out byte _))
                            {
                                reader.Rewind(1);
                                return false;
                            }

                            DictDepth++;
                        }

                        // just hex string
                    }
                        continue;
                    case (byte) '>':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '>')
                        {
                            
                            if (!reader.TryRead(out byte _))
                            {
                                reader.Rewind(1);
                                return false;
                            }
                            // ended!
                            DictDepth--;
                            Debug.Assert(DictDepth >= 0, "DictDepth >= 0");
                            if (DictDepth == 0)
                            {
                                return true;
                            }
                        }
                        // just hex end
                    }
                        continue;
                    case (byte) '(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
                }
            }

            return false;
        }
    }
}
