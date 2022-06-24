using PdfLexer.Filters;
using PdfLexer.IO;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PdfLexer
{
    /// <summary>
    /// Pdf stream object
    /// </summary>
    public class PdfStream : PdfObject
    {
        // TODO support external content?
        public PdfStream(PdfDictionary dictionary, PdfStreamContents contents)
        {
            Dictionary = dictionary;
            Contents = contents;
        }
        /// <summary>
        /// Dictionary portion of the Stream object.
        /// </summary>
        public PdfDictionary Dictionary { get; }
        /// <summary>
        /// Stream portion of the Pdf Object
        /// </summary>
        public PdfStreamContents Contents { get; set; } // TODO clean decoded on set
        public bool IsIndirect => true;
        public override PdfObjectType Type => PdfObjectType.StreamObj;
        public override bool IsModified => Dictionary.IsModified; // TODO STREAM SUPPORT
        public byte[] DecodedData { get; private set; }
        public byte[] GetDecodedData(ParsingContext ctx)
        {
            if (DecodedData != null)
            {
                return DecodedData;
            }
            var stream = GetDecodedStream(ctx);
            if (stream is MemoryStream ms)
            {
                DecodedData = ms.ToArray();
            }
            else
            {
                if (stream.CanSeek)
                {
                    DecodedData = new byte[stream.Length];
                    int pos = 0;
                    int read;
                    while ((read = stream.Read(DecodedData, pos, (int)stream.Length - pos)) > 0)
                    {
                        pos += read;
                    }
                } else
                {
                    var copy = ParsingContext.StreamManager.GetStream();
                    stream.CopyTo(copy);
                    DecodedData = copy.ToArray(); // this may not make sense... ToArray allocates anyway, why use stream manager?
                }

            }

            return DecodedData;
        }
        public Stream GetDecodedStream(ParsingContext ctx)
        {
            if (DecodedData != null)
            {
                return new MemoryStream(DecodedData);
            }
            if (!Dictionary.TryGetValue(PdfName.Filter, out var obj))
            {
                return Contents.GetData();
            }

            var source = Contents.GetData();
            if (source.Length != Contents.Length)
            {
                source = new SubStream(source, 0, Contents.Length);
            }

            obj = obj.Resolve();
            if (obj.Type == PdfObjectType.ArrayObj)
            {
                var arr = obj.GetValue<PdfArray>();
                foreach (var f in arr)
                {
                    var filter = f.GetValue<PdfName>();
                    source = DecodeSingle(filter, source);
                }
                return source;
            }
            else
            {
                var filter = obj.GetValue<PdfName>();
                return DecodeSingle(filter, source);
            }

            Stream DecodeSingle(PdfName filterName, Stream input)
            {
                var decode = ctx.GetDecoder(filterName);
                return decode.Decode(input, Dictionary.GetOptionalValue<PdfDictionary>(PdfName.DecodeParms));
            }
        }


        // /Length required
        // /Filter /DecodeParms -> if filters
        // /F, /FFilter, /FDecodeParms -> external file
    }

    /// <summary>
    /// Contents of a Pdf stream.
    /// </summary>
    public abstract class PdfStreamContents
    {
        /// <summary>
        /// Reads data of the stream.
        /// WARNING: may return data past stream end.
        /// </summary>
        /// <param name="destination"></param>
        public abstract Stream GetData();
        /// <summary>
        /// Copies contents to the provided stream.
        /// </summary>
        /// <param name="destination"></param>
        public abstract void CopyRawContents(Stream destination);
        /// <summary>
        /// Length of the stream (compressed, if applicable).
        /// </summary>
        public abstract int Length { get; }
    }

    /// <summary>
    /// Contents of a Pdf stream.
    /// </summary>
    public class PdfExistingStreamContents : PdfStreamContents
    {
        internal IPdfDataSource Source { get; }
        internal long Offset { get; }
        public PdfExistingStreamContents(IPdfDataSource source, long offset, int length)
        {
            Source = source;
            Offset = offset;
            Length = length;
        }

        public override int Length { get; }
        /// <summary>
        /// Copies contents to the provided stream.
        /// </summary>
        /// <param name="destination"></param>
        public override void CopyRawContents(Stream destination)
        {
            Source.CopyData(Offset, Length, destination);
        }

        public override Stream GetData() => Source.GetDataAsStream(Offset, Length);
    }

    /// <summary>
    /// Contents of a Pdf stream.
    /// </summary>
    public class PdfByteArrayStreamContents : PdfStreamContents
    {
        public byte[] Contents;
        public PdfByteArrayStreamContents(byte[] contents)
        {
            Contents = contents;
        }

        /// <summary>
        /// Length of the stream (compressed, if applicable).
        /// </summary>
        public override int Length => Contents?.Length ?? 0;

        /// <summary>
        /// Copies contents to the provided stream.
        /// </summary>
        /// <param name="destination"></param>
        public override void CopyRawContents(Stream destination)
        {
            destination.Write(Contents);
        }

        public override Stream GetData() => new MemoryStream(Contents);
    }
}
