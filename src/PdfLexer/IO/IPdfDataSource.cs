﻿using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    public interface IPdfDataSource
    {
        /// <summary>
        /// Total bytes contained in the PDF data source
        /// </summary>
        long TotalBytes { get; }
        /// <summary>
        /// Determines if the data source has section in memory
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        bool IsDataInMemory(long startPosition, int length);
        /// <summary>
        /// Returns data already in memory.
        /// </summary>
        /// <param name="startPosition">Starting position in data source for buffer</param>
        /// <param name="requiredBytes">Number of required bytes.</param>
        /// <param name="buffer">Filled buffer of at least required bytes size.</param>
        void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer);
        Stream GetDataAsStream(long startPosition, int desiredBytes);

        /// <summary>
        /// If the data source supports cloning for concurrent processing.
        /// </summary>
        bool SupportsCloning { get; }
        /// <summary>
        /// Clones the current data source for use in concurrent processing.
        /// If all operations on data source are thread safe, source can return itself.
        /// </summary>
        /// <returns></returns>
        IPdfDataSource Clone();
        /// <summary>
        /// Gets a readable stream.
        /// Reader MUST NOT dispose stream.
        /// </summary>
        /// <param name="startPosition">Position of stream in current data source</param>
        /// <returns>Stream</returns>
        Stream GetStream(long startPosition);
        /// <summary>
        /// Copies data from data source to stream.
        /// </summary>
        /// <param name="startPosition">Start position for copy.</param>
        /// <param name="requiredBytes">Number of bytes to copy</param>
        /// <param name="stream">Stream to copy to.</param>
        void CopyData(long startPosition, int requiredBytes, Stream stream);
        /// <summary>
        /// Associated parsing context.
        /// </summary>
        ParsingContext Context {get;}
    }
}
