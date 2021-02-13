using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Objects
{
    public class PdfDocument : IDisposable
    {
        private Stream _stream;
        private bool _disposeStream;

        public PdfDocument(Stream stream, bool disposeStream = false)
        {
            _stream = stream;
            _disposeStream = disposeStream;
        }

        public void Initialize()
        {

        }
        public void InitializeAsync()
        {

        }

        public void Dispose()
        {
            if (_disposeStream)
            {
                _stream?.Dispose();
            }
        }
    }
}
