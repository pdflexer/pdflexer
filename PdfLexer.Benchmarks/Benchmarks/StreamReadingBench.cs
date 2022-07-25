using BenchmarkDotNet.Attributes;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace PdfLexer.Benchmarks.Benchmarks
{

    /// <summary>
    /// Looked at using string to look up common number values but the utf8 parser is faster than
    /// just getting ascii.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class StreamReadingBench
    {
        public static List<string> data = new List<string>
        {
            "<</BaseFont/USPSBarCode1/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 195 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 195 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0]/FontDescriptor 16 0 R/Subtype/TrueType>>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        ",
            "<</BaseFont/ArialMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 277 277 354 556 556 889 666 190 333 333 389 583 277 333 277 277 556 556 556 556 556 556 556 556 556 556 277 277 583 583 583 556 1015 666 666 722 722 666 610 777 722 277 500 666 556 833 722 777 666 777 722 666 610 722 666 943 666 666 610 277 277 277 469 556 333 556 556 500 556 556 277 556 556 222 222 500 222 833 556 556 556 556 333 500 277 556 500 722 500 500 500 333 259 333 583 0 556 0 222 556 333 1000 556 556 333 1000 666 333 1000 0 610 0 0 222 222 333 333 350 556 1000 333 1000 500 333 943 0 500 666 277 333 556 556 556 556 259 556 333 736 370 556 583 333 736 552 399 548 333 333 333 576 537 333 333 333 365 556 833 833 833 610 666 666 666 666 666 666 1000 722 666 666 666 666 277 277 277 277 722 722 777 777 777 777 777 583 777 722 722 722 722 666 666 610 556 556 556 556 556 556 889 500 556 556 556 556 277 277 277 277 556 556 556 556 556 556 556 548 610 556 556 556 556 500 556 500]/FontDescriptor 17 0 R/Subtype/TrueType>>",
            "<</BaseFont/CourierNewPS-BoldMT/FirstChar 1/Type/Font/Encoding/WinAnsiEncoding/LastChar 255/Widths [0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 0 600 0 600 600 600 600 600 600 600 600 600 600 600 0 600 0 0 600 600 600 600 600 600 600 600 600 600 600 600 0 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600 600]/FontDescriptor 18 0 R/Subtype/TrueType>>",
            "<</Key (Test \\\\(Test) Test )/Key2 (\\216\\217)/Key3 (Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLine Test \\\rNextLineTest \\\rNextLineTest \\\rNextLine)>>",
            "<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19189 19 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>",
            "<</Type/XObject/DecodeParms<</K -1/Columns 20/Rows 20>>/Subtype/Image/Width 20/ColorSpace/DeviceGray/Filter/CCITTFaxDecode/BitsPerComponent 1/Length 115/Height 20>>",
            "<</FormType 1/Subtype/Form/BBox[0 0 612 792]/Resources<</XObject<</Xf19186 20 0 R>>/ProcSet[/PDF/Text/ImageB/ImageC/ImageI]>>/Type/XObject/Filter/FlateDecode/Length 53/Matrix[1 0 0 1 0 0]>>"
        };
        private List<byte[]> samples = new List<byte[]>();
        private List<MemoryStream> streams = new List<MemoryStream>();
        private ParsingContext ctx = new ParsingContext();
        private StreamPipeReaderOptions opts512 =  new StreamPipeReaderOptions(bufferSize: 512, leaveOpen: true);
        private StreamPipeReaderOptions opts32 = new StreamPipeReaderOptions(bufferSize: 32, leaveOpen: true);
        private char[] chars = new char[10];
        public StreamReadingBench()
        {
            foreach (var item in data)
            {
                var bytes = Encoding.ASCII.GetBytes(item);
                var padded = new byte[bytes.Length+2000];
                Array.Copy(bytes, 0, padded, 0, bytes.Length);
                samples.Add(padded);
                streams.Add(new MemoryStream(bytes));
            }
        }

        [Benchmark(Baseline = true)]
        public int StreamPipe32()
        {
 
            var count = 0;
            foreach (var stream in streams)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var reader = PipeReader.Create(stream, opts32);
                var scanner = new PipeScanner(ctx, reader);
                scanner.Peek();
                var obj = (PdfDictionary)scanner.GetCurrentObject();
                count += obj.Count;
            }
            return count;
        }

        [Benchmark]
        public int StreamPipe512()
        {

            var count = 0;
            foreach (var stream in streams)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var reader = PipeReader.Create(stream, opts512);
                var scanner = new PipeScanner(ctx, reader);
                scanner.Peek();
                var obj = (PdfDictionary)scanner.GetCurrentObject();
                count += obj.Count;
            }
            return count;
        }

        // [Benchmark]
        // public int StreamReader()
        // {
        // 
        //     var count = 0;
        //     foreach (var stream in streams)
        //     {
        //         stream.Seek(0, SeekOrigin.Begin);
        //         using var reader = new StreamReader(stream);
        //         var scanner = new StreamScanner(ctx, reader);
        //         scanner.Peek();
        //         var obj = (PdfDictionary)scanner.GetCurrentObject();
        //         count += obj.Count;
        //     }
        //     return count;
        // }

        [Benchmark]
        public int Span()
        {
            var count = 0;
            foreach (var dat in samples)
            {
                var scanner = new Scanner(ctx, dat);
                scanner.Peek();
                var obj = (PdfDictionary)scanner.GetCurrentObject();
                count += obj.Count;
            }
            return count;
        }

        [Benchmark]
        public int StreamCopySpan()
        {
            var count = 0;
            foreach (var str in streams)
            {
                str.Seek(0, SeekOrigin.Begin);
                var buff = ArrayPool<byte>.Shared.Rent(1500);
                int total = 0;
                int read;
                while ((read = str.Read(buff, total, 1500 - total)) > 0)
                {
                    total += read;
                }
                var scanner = new Scanner(ctx, buff);
                scanner.Peek();
                var obj = (PdfDictionary)scanner.GetCurrentObject();
                count += obj.Count;
                ArrayPool<byte>.Shared.Return(buff);
            }
            return count;
        }

        // [Benchmark]
        public long SequenceRead()
        {
            long count = 0;
            foreach (var str in streams)
            {
                str.Seek(0, SeekOrigin.Begin);
                using var rdr = new StreamReaderTest(str);
                while (!rdr.IsCompleted)
                {
                    rdr.Read();
                }
                count += rdr.CurrentSequence.Length;
            }
            return count;
        }

        // [Benchmark]
        public long StreamRead()
        {
            long count = 0;
            foreach (var str in streams)
            {
                var ms = new MemoryStream();
                str.CopyTo(ms);
                var all = ms.ToArray();
                count += all.Length;
            }
            return count;
        }
    }

    internal class StreamReaderTest : IDisposable
    {
        private int bufferSize = 512;
        private Stream _stream;
        private MemorySegment _first;
        private int _start;
        private MemorySegment _last;
        private ReadOnlySequence<byte> _current;
        private int _totalRead;
        // private int _totalRead;

        public StreamReaderTest(Stream stream, int bufferSize=512)
        {
            _stream = stream;
            this.bufferSize = bufferSize;
        }

        public bool IsCompleted { get; private set; }
        public ReadOnlySequence<byte> CurrentSequence { get => _current; }
        public ReadOnlySequence<byte> Read()
        {
            var buff = ArrayPool<byte>.Shared.Rent(bufferSize);
            _totalRead = 0;
            int read;
            while ((read = _stream.Read(buff, _totalRead, buff.Length - _totalRead)) > 0)
            {
                _totalRead += read;
            }
            if (_totalRead == 0) { throw new ApplicationException("Read called and no data obtained"); }
            if (_totalRead < buff.Length)
            {
                IsCompleted = true;
            }

            if (_current.Length == 0)
            {
                _first = new MemorySegment(buff);
                _last = _first;
                return _current = new ReadOnlySequence<byte>(_first, _start, _last, _totalRead);
            } else
            {
                _last = _last.Append(buff);
                return _current = new ReadOnlySequence<byte>(_first, _start, _last, _totalRead);
            }
        }

        private ReadOnlySequence<byte> Advance(int count)
        {
            var currTotal = _first.Memory.Length - _start;
            while (count > currTotal)
            {
                count -= currTotal;
                var prev = _first;
                var nxt = _first.Next;
                if (nxt == null)
                {
                    throw new NotSupportedException("Attempted to advance beyond current sequence");
                }
                _first = (MemorySegment)nxt;
                _start = 0;
                currTotal = _first.Memory.Length;
                prev.Dispose();
            }
            return _current = new ReadOnlySequence<byte>(_first, count, _last, _totalRead);
        }

        public ReadOnlySequence<byte> AdvanceTo(SequencePosition pos)
        {
            var os =_current.GetOffset(pos);
            return Advance((int)os);
        }

        public void Dispose()
        {
            while (true)
            {
                var nxt = _first.Next;
                if (nxt == null) 
                {
                    return;
                }
                _first.Dispose();
                _first = (MemorySegment)nxt;
            }
        }
    }

    internal class MemorySegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        private byte[] ByteArray { get; set; }
        public MemorySegment(byte[] memory)
        {
            ByteArray = memory;
            Memory = memory;
        }

        public MemorySegment Append(byte[] memory)
        {
            var segment = new MemorySegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;

            return segment;
        }

        public void Dispose()
        {
            if (ByteArray != null) { ArrayPool<byte>.Shared.Return(ByteArray); }
            ByteArray = null;
        }
    }
}
