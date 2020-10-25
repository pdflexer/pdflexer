using System;
using System.Collections.Generic;
using System.Text;
using PdfLexer.Objects;
using PdfLexer.Objects.Lazy;
using PdfLexer.Objects.Nested;
using PdfLexer.Objects.Parsers;

namespace PdfLexer
{
/// <summary>
    /// Parser for locating byte span of an indirect object. If data besides the raw bytes from
    /// and object is needed, other methods may be more efficient that directly parse the data into
    /// a useable form.
    /// Note: span returned does not include object header (eg. 12 0 obj) or trailer (endobj)
    /// </summary>
    public ref struct IndirectLookup
    {
        private ReadOnlySpan<byte> _data;
        private int _maxDepth;
        private Dictionary<int, XRefEntry> _lookup;
        public IndirectLookup(ReadOnlySpan<byte> data, Dictionary<int, XRefEntry> lookupTable, int maxDepth)
        {
            _data = data;
            _lookup = lookupTable;
            _maxDepth = maxDepth;
        }

        public PdfLazyDictionary GetLazyPdfDictionary(PdfLazyObject obj)
        {
            if (obj.Type == PdfObjectType.IndirectRefObj)
            {
                var ir = IndirectParser.ParseIndirectReference(_data, obj.Offset);
                return DictionaryParser.ParseLazyDictionary(GetIndirectObjectData(ir.ObjectNumber, out var _), 0);
            } else if (obj.Type == PdfObjectType.DictionaryObj)
            {
                return DictionaryParser.ParseLazyDictionary(_data, obj.Offset);
            } else
            {
                throw new ApplicationException("");
            }
        }

        public ReadOnlySpan<byte> GetIndirectObjectData(int objectNumber, out PdfObjectType type, int depth=0)
        {
            if (depth > _maxDepth)
            {
                throw new ApplicationException("Recursive parsing depth max reached.");
            }
            if (!_lookup.ContainsKey(objectNumber))
            {
                throw new ApplicationException("Unknown object number.");
            }
            var entry = _lookup[objectNumber];

            // rename? carry over from static method this was copied from
            var byteOffset = (int)entry.Offset;
            var bytes = _data;

            // skip the two numbers
            for (var n = 0; n < 2; n++)
            {
                byteOffset = CommonUtil.FindNextToken(bytes, out PdfTokenType nType, byteOffset);
                if (nType != PdfTokenType.NumericObj || byteOffset == -1)
                {
                    throw new ApplicationException("Object start was not correct, number not found.");
                }
                byteOffset += NumberParser.CountNumberBytes(bytes.Slice(byteOffset));
            }

            byteOffset = CommonUtil.SkipWhiteSpaces(bytes, byteOffset);
            if (byteOffset == -1 || !bytes.Slice(byteOffset, 3).SequenceEqual(IndirectSequences.obj))
            {
                throw new ApplicationException("Object start was not correct, object not found.");
            }
            byteOffset += 3; //obj
            byteOffset = CommonUtil.FindNextToken(bytes, out PdfTokenType tokenTYpe, byteOffset);
            int bytesUsed = 0;
            int objStart = byteOffset;
            switch (tokenTYpe)
            {
                case PdfTokenType.ArrayStart:
                    type = PdfObjectType.ArrayObj;
                    bytesUsed = NestedUtils.CountNestedBytes(bytes.Slice(byteOffset));
                    break;
                case PdfTokenType.DictionaryStart:
                    type = PdfObjectType.DictionaryObj;
                    bytesUsed = NestedUtils.CountNestedBytes(bytes.Slice(byteOffset));
                    break;
                case PdfTokenType.BooleanObj:
                    type = PdfObjectType.BooleanObj;
                    BoolParser.GetBool(bytes.Slice(byteOffset), out bool result, out bytesUsed);
                    break;
                case PdfTokenType.NameObj:
                    type = PdfObjectType.NameObj;
                    bytesUsed = NameParser.CountNameBytes(bytes, byteOffset);
                    break;
                case PdfTokenType.NullObj:
                    type = PdfObjectType.NullObj;
                    bytesUsed = 4;
                    break;
                case PdfTokenType.NumericObj:
                    type = PdfObjectType.NumericObj;
                    bytesUsed = NumberParser.CountNumberBytes(bytes.Slice(byteOffset));
                    break;
                case PdfTokenType.StringObj:
                    type = PdfObjectType.StringObj;
                    StringParser.GetString(bytes.Slice(byteOffset), out ReadOnlySpan<byte> data); //TODO just count
                    bytesUsed = data.Length;
                    break;
                default:
                    throw new ApplicationException("Invalid token found when reading object.");
            }
            //objBytes = bytes.Slice(byteOffset, bytesUsed);
            byteOffset += bytesUsed;
            byteOffset = CommonUtil.SkipWhiteSpaces(bytes, byteOffset);
            if (byteOffset == -1)
            {
                throw new ApplicationException("Object end was not correct, endobj not found.");
            }
            var next = bytes.Slice(byteOffset, 6);
            if (next.SequenceEqual(IndirectSequences.endobj))
            {
                return bytes.Slice(objStart, byteOffset - objStart);
            }
            else if (next.SequenceEqual(IndirectSequences.stream))
            {
                // TODO move length lookup to own method
                var nested = new NestedParser(bytes.Slice(objStart));
                while (nested.Read())
                {
                    if (nested.GetCurrentTokenSpan().SequenceEqual(IndirectSequences.length))
                    {
                        var read = nested.Read();
                        if (!read)
                        {
                            throw new ApplicationException("Unable to find length value in stream dictionary.");
                        }
                        int length = 0;
                        if (nested.ObjectType == PdfObjectType.IndirectRefObj)
                        {
                            length = GetLengthFromIndirectRef(nested.GetCurrentTokenSpan(), depth);
                        } else if (nested.ObjectType == PdfObjectType.NumericObj)
                        {
                            length = (int)NumberParser.ParseUInt64(nested.GetCurrentTokenSpan());
                        } else
                        {
                            throw new ApplicationException("Stream length not numeric or indirect ref.");
                        }
                        
                        byteOffset += 6; // stream
                        if (bytes[byteOffset] == (byte)'\n') // \n
                        {
                            byteOffset += 1;
                        }
                        else if (bytes[byteOffset + 1] == (byte)'\n') // \r\n
                        {
                            byteOffset += 2;
                        }
                        else
                        {
                            throw new ApplicationException("Stream start not followed by correct eol sequence.");
                        }
                        byteOffset += (int)length;
                        byteOffset = CommonUtil.SkipWhiteSpaces(bytes, byteOffset);
                        if (!(bytes.Slice(byteOffset, 9).SequenceEqual(IndirectSequences.endstream)))
                        {
                            throw new ApplicationException("Stream bytes not followed by endstream, encountered: " +
                                Encoding.ASCII.GetString(bytes.Slice(byteOffset, 9)));
                        }
                        byteOffset += 9;
                        byteOffset = CommonUtil.SkipWhiteSpaces(bytes, byteOffset);
                        if (!(bytes.Slice(byteOffset, 6).SequenceEqual(IndirectSequences.endobj)))
                        {
                            throw new ApplicationException("Stream not followed by endobj");
                        }
                        return bytes.Slice(objStart, byteOffset - objStart);
                    }
                }
                throw new ApplicationException("Unable to find length value in stream dictionary.");
            }
            else
            {
                throw new ApplicationException("Object end was not correct, endobj not found.");
            }
        }

        private int GetLengthFromIndirectRef(ReadOnlySpan<byte> indirectRefData, int depth = 0)
        {
            var iRef = IndirectParser.ParseIndirectReference(indirectRefData, 0);
            var data = GetIndirectObjectData(iRef.ObjectNumber, out var _, depth++);
            return NumberParser.ParseInt(data);
        }
    }
}