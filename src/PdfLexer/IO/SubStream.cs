using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    // adapted from
    // from https://social.msdn.microsoft.com/Forums/vstudio/en-US/c409b63b-37df-40ca-9322-458ffe06ea48/how-to-access-part-of-a-filestream-or-memorystream?forum=netfxbcl
    internal class SubStream : Stream
    {
        private Stream baseStream;
        private long length;
        private long position;
        private long subOffset;
        private bool disposeMain;

        public SubStream(Stream baseStream, long offset, long length, bool disposeMain)
        {
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            if (!baseStream.CanRead) throw new ArgumentException("can't read base stream");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset");

            this.baseStream = baseStream;
            this.length = length;
            this.disposeMain = disposeMain;
            this.subOffset = offset;

            if (baseStream.CanSeek)
            {
                baseStream.Seek(offset, SeekOrigin.Begin);
            } else
            {
                throw new NotImplementedException("substream on unseekable");
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = length - position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;
            int read = baseStream.Read(buffer, offset, count);
            position += read;
            return read;
        }
        private void CheckDisposed()
        {
            if (baseStream == null) throw new ObjectDisposedException(GetType().Name);
        }
        public override long Length
        {
            get { return length; }
        }
        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override bool CanSeek
        {
            get { return baseStream.CanSeek; }
        }
        public override long Position
        {
            get
            {
                return position;
            }
            set { throw new NotSupportedException(); }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                offset += subOffset;
            }
            var result = baseStream.Seek(offset, origin);
            position = result - subOffset;
            return position;
            
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Flush()
        {
            baseStream.Flush();
        }
        protected override void Dispose(bool disposing)
        {
            // no op -> we reuse these
        }

        public void ActuallyDispose(bool disposing)
        {
            base.Dispose(disposing);
            baseStream = null;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Reset(long offset, long length)
        {
            subOffset = offset;
            this.length = length;
            position = 0;
            baseStream.Seek(offset, SeekOrigin.Begin);
        }
    }
}
