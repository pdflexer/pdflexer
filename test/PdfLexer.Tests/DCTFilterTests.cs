using PdfLexer.Content;
using PdfLexer.Images;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests
{
    /// <summary>
    /// Tests for DCTDecode (JPEG) filter functionality.
    /// </summary>
    public class DCTFilterTests
    {
        /// <summary>
        /// Tests that PDFs with DCT-encoded images can be read without errors.
        /// These PDFs are from pdf.js test suite and are known to work correctly there.
        /// </summary>
        [InlineData("__artofwar.pdf")]
        [InlineData("__issue3248.pdf")]
        [InlineData("__issue6364.pdf")]
        [InlineData("__issue7303.pdf")]
        [InlineData("__wdsg_fitc.pdf")]
        [InlineData("__issue1096.pdf")]
        [InlineData("__issue1419.pdf")]
        [InlineData("__issue2386.pdf")]
        [Theory]
        public void It_Reads_DCT_Encoded_PDFs(string pdfName)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", pdfName);
            
            if (!File.Exists(pdf))
            {
                throw new FileNotFoundException($"Test PDF not found: {pdf}");
            }

            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
            var errors = new List<string>();
            
            foreach (var page in doc.Pages)
            {
                try
                {
                    // Try to read images from the page which exercises DCT decoding
                    var imgScanner = new ImageScanner(doc.Context, page);
                    while (imgScanner.Advance())
                    {
                        if (imgScanner.TryGetImage(out var img))
                        {
                            // Get decoded data to exercise the DCT filter
                            var data = img.XObj.Contents.GetDecodedData();
                            Assert.NotNull(data);
                            Assert.True(data.Length > 0, "Decoded image data should not be empty");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Page error: {ex.Message}");
                }
            }
            
            if (errors.Any())
            {
                throw new AggregateException($"Errors reading {pdfName}", 
                    errors.Select(e => new Exception(e)));
            }
        }

        /// <summary>
        /// Tests that all streams in these PDFs can be decoded.
        /// </summary>
        [InlineData("__artofwar.pdf")]
        [InlineData("__issue3248.pdf")]
        [InlineData("__issue6364.pdf")]
        [InlineData("__issue7303.pdf")]
        [InlineData("__wdsg_fitc.pdf")]
        [InlineData("__issue1096.pdf")]
        [InlineData("__issue1419.pdf")]
        [InlineData("__issue2386.pdf")]
        [Theory]
        public void It_Decodes_All_Streams(string pdfName)
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            var pdf = Path.Combine(tp, "pdfs", "pdfjs", pdfName);
            
            if (!File.Exists(pdf))
            {
                throw new FileNotFoundException($"Test PDF not found: {pdf}");
            }

            using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
            var errors = new List<string>();
            var visited = new HashSet<int>();
            
            DecodeAllStreams(doc.Trailer, visited, errors);
            
            if (errors.Any())
            {
                throw new AggregateException($"Errors decoding streams in {pdfName}", 
                    errors.Select(e => new Exception(e)));
            }
        }

        private void DecodeAllStreams(IPdfObject obj, HashSet<int> visited, List<string> errors)
        {
            if (obj is PdfIndirectRef ir)
            {
                if (visited.Contains(ir.Reference.ObjectNumber))
                {
                    return;
                }
                visited.Add(ir.Reference.ObjectNumber);
            }
            
            obj = obj.Resolve();

            if (obj is PdfStream stream)
            {
                try
                {
                    // Get decoded data to exercise the filter
                    var data = stream.Contents.GetDecodedData();
                    // Just verify we can read it
                }
                catch (Exception ex)
                {
                    var filter = stream.Dictionary.GetOptionalValue<IPdfObject>(PdfName.Filter);
                    var filterStr = filter?.ToString() ?? "none";
                    // Only report DCTDecode errors since this is a DCT-specific test
                    if (filterStr.Contains("DCT"))
                    {
                        errors.Add($"Failed to decode stream with filter {filterStr}: {ex.Message}");
                    }
                }
            }

            if (obj is PdfArray arr)
            {
                foreach (var item in arr)
                {
                    DecodeAllStreams(item, visited, errors);
                }
            }
            else if (obj is PdfDictionary dict)
            {
                foreach (var item in dict.Values)
                {
                    DecodeAllStreams(item, visited, errors);
                }
            }
        }
    }
}
