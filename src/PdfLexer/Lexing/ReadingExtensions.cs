using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.Lexing
{
    internal static class ReadingExtensions
    {
        public static IPdfObject ReadWrappedFromStream(this IPdfDataSource source, XRefEntry xref)
        {
            if (source.Disposed) { throw new ObjectDisposedException("Attempted to get object from disposed data source."); }
            // TODO
            // quick path if xref offsets known
            var stream = source.GetStream(xref.Offset);
            var reader = source.Context.Options.CreateReader(stream);
            var scanner = new PipeScanner(source.Context, reader);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            xref.KnownObjType = scanner.Peek();
            xref.KnownObjStart = (int)scanner.GetStartOffset();
            source.Context.CurrentOffset = xref.Offset + xref.KnownObjStart;
            var obj = scanner.GetCurrentObject();
            xref.KnownObjLength = (int)scanner.GetOffset() - xref.KnownObjStart;
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                scanner.SkipCurrent();
                xref.KnownStreamStart = (int)scanner.GetOffset();
            }
            else
            {
                source.Context.Error($"XRef obj did not end with endobj: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
                return obj;
            }

            if (!(obj is PdfDictionary dict))
            {
                source.Context.Error($"Obj followed by startstream was {obj.Type} instead of dictionary ({xref.Reference.ObjectNumber} {xref.Reference.Generation})");
                xref.KnownStreamStart = 0;
                return obj;
            }

            if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
            {
                source.Context.Error("Pdf dictionary followed by start stream token did not contain /Length.");
                streamLength = PdfCommonNumbers.Zero;
            }

            var contents = new PdfXRefStreamContents(source, xref, streamLength);
            contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
            contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
            return new PdfStream(dict, contents);
        }

        public static void WriteWrappedFromStream(this IPdfDataSource source, XRefEntry xref, Stream output)
        {
            if (source.Disposed) { throw new ObjectDisposedException("Attempted to get object data from disposed data source."); }
            // TODO
            // quick path if xref offsets known
            var stream = source.GetStream(xref.Offset);
            var reader = source.Context.Options.CreateReader(stream);
            var scanner = new PipeScanner(source.Context, reader);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            xref.KnownObjType = scanner.Peek();
            xref.KnownObjStart = (int)scanner.GetStartOffset();

            var dat = scanner.GetAndSkipCurrentData();
            foreach (var seg in dat)
            {
                output.Write(seg.Span);
            }

            xref.KnownObjLength = (int)scanner.GetOffset() - xref.KnownObjStart;
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                scanner.SkipCurrent();
                xref.KnownStreamStart = (int)scanner.GetOffset();
            }
            else
            {
                source.Context.Error($"XRef obj did not end with endobj: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
            }
            var obj = source.Context.GetPdfItem(PdfObjectType.DictionaryObj, in dat);

            reader.Complete();

            if (!(obj is PdfDictionary dict))
            {
                source.Context.Error($"Obj followed by startstream was {obj.Type} instead of dictionary ({xref.Reference.ObjectNumber} {xref.Reference.Generation})");
                xref.KnownStreamStart = 0;
                return;
            }

            if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
            {
                source.Context.Error("Pdf dictionary followed by start stream token did not contain /Length.");
                streamLength = PdfCommonNumbers.Zero;
            }

            PdfName filterName = CommonUtil.GetFirstFilter(dict);

            output.Write(IndirectSequences.stream);
            output.WriteByte((byte)'\n');
            using var so = source.Context.GetStreamOfContents(xref, filterName, streamLength);
            so.CopyTo(output);
            output.WriteByte((byte)'\n');
            output.Write(IndirectSequences.endstream);
        }

        internal static IPdfObject GetWrappedFromSpan(this IPdfDataSource source, XRefEntry xref)
        {
            if (source.Disposed) { throw new ObjectDisposedException("Attempted to get object from disposed data source."); }
            // TODO
            // quick path if xref offsets known
            // set xref offsets
            source.GetData(xref.Offset, -1, out var data);
            var scanner = new Scanner(source.Context, data, 0);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            var obj = scanner.GetCurrentObject();
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                if (!(obj is PdfDictionary dict))
                {
                    source.Context.Error($"Pdf dictionary followed by startstream was {obj.Type} instead of dictionary.");
                    return obj;
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    source.Context.Error("Pdf dictionary followed by start stream token did not contain /Length.");
                    streamLength = PdfCommonNumbers.Zero;
                }

                var startPos = scanner.Position + scanner.CurrentLength;
                scanner.SkipCurrent(); // start stream
                scanner.Advance(streamLength);
                var endstream = scanner.Peek();
                if (endstream != PdfTokenType.EndStream)
                {
                    source.Context.Error("Endstream not found at end of stream when parsing indirect object.");
                    if (endstream == PdfTokenType.EOS)
                    {
                        scanner.Position = data.Length - Math.Min(data.Length, 100);
                    }
                    if (scanner.TryFindEndStream())
                    {
                        source.Context.Error("Found endstream in contents, using repaired length.");
                        streamLength = new PdfIntNumber(scanner.Position - startPos);
                        dict[PdfName.Length] = streamLength;
                    }
                }
                var contents = new PdfExistingStreamContents(source, xref.Offset + startPos, streamLength);
                contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                var stream = new PdfStream(dict, contents);

                return stream;
            }
            source.Context.Error("Indirect object not followed by endobj token: " + CommonUtil.GetDataErrorInfo(data, scanner.Position));
            return obj;
        }

        internal static void UnwrapAndCopyFromSpan(this IPdfDataSource source, XRefEntry xref, WritingContext wtx)
        {
            if (source.Disposed) { throw new ObjectDisposedException("Attempted to get object data from disposed data source."); }
            // TODO
            // quick path if xref offsets known
            // set xref offsets
            if (source.Context.IsEncrypted)
            {
                throw new NotSupportedException("Copying raw data from encrypted PDF is not supported.");
            }
            source.GetData(xref.Offset, xref.MaxLength, out var data);
            var scanner = new Scanner(source.Context, data, 0);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            CopyRawObjFromSpan(source, ref scanner, wtx);
        }

        internal static void CopyRawObjFromSpan(this IPdfDataSource source, ref Scanner scanner, WritingContext wtx)
        {
            var objLength = scanner.SkipObject();
            var objStart = scanner.Position - objLength;
            var type = scanner.Peek();
            if (type == PdfTokenType.EndObj)
            {
                wtx.Stream.Write(scanner.Data.Slice(objStart, objLength));
                return;
            }
            else if (type == PdfTokenType.StartStream)
            {
                // TODO can we not parse dict here?
                scanner.SkipCurrent(); // startstream
                var startPos = scanner.Position;
                var existing = source.Context.Options.Eagerness;
                source.Context.Options.Eagerness = Eagerness.Lazy;
                var obj = source.Context.GetKnownPdfItem(PdfObjectType.DictionaryObj, scanner.Data, objStart, objLength);
                if (!(obj is PdfDictionary dict))
                {
                    throw CommonUtil.DisplayDataErrorException(scanner.Data, scanner.Position, "Indirect object followed by start stream token but was not dictionary");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }

                scanner.Advance(streamLength);
                var eosByLength = scanner.Position;
                var endstream = scanner.Peek();
                if (endstream == PdfTokenType.EndStream)
                {
                    scanner.SkipCurrent();
                    wtx.Stream.Write(scanner.Data.Slice(objStart, scanner.Position - objStart));
                }
                else
                {
                    source.Context.Error("Endstream not found at end of stream when parsing when copying data.");
                    if (endstream == PdfTokenType.EOS)
                    {
                        scanner.Position = scanner.Data.Length - Math.Min(scanner.Data.Length, 100);
                    }
                    if (!scanner.TryFindEndStream())
                    {
                        source.Context.Error("Unable to find endstream in contents, writing provided length.");
                        // no way to repair this.. simply write existing data length
                        wtx.Stream.Write(scanner.Data.Slice(objStart, eosByLength - objStart));
                        wtx.Stream.WriteByte((byte)'\n');
                        wtx.Stream.Write(IndirectSequences.endstream);
                        source.Context.Options.Eagerness = existing;
                        return;
                    }

                    source.Context.Error("Found endstream in contents, using repaired length.");
                    streamLength = new PdfIntNumber(scanner.Position - startPos);
                    dict[PdfName.Length] = streamLength;
                    var contents = new PdfByteArrayStreamContents(scanner.Data.Slice(startPos, scanner.Position - startPos).ToArray());
                    var stream = new PdfStream(dict, contents);
                    contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                    contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                    wtx.SerializeObject(stream, true);
                }
                source.Context.Options.Eagerness = existing;
                return;
            }
        }
    }
}
