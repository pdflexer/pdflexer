# pdflexer
pdflexer is a PDF parsing library. It is focused on efficient parsing and modification of PDF files and is mainly targeted for users familiar with the pdf spec. It is generally very fast at what it does (eg. splitting / merging shows multiple times better performance than alternatives). The parsing logic was implemented from scratch but some higher level functionality (eg. filters) were ported from the [pdf.js](https://github.com/mozilla/pdf.js) project.

pdflexer differs from existing .net libraries in that it:
* Is primarly designed for PDF modification (not just reading). Any object / page read from a PDF can be modified and written to others PDFs.
* Has lazy parsing features which allow objects to be parsed on demand increasing performance in many cases.
* Designed for direct access to the native PDF objects types. Any higher level objects are simple wrappers areound the native pdf object types (eg `PdfPage` is a wrapper around a `PdfDictionary`. The `PdfDictionary` can be directly modified for features not implemented on `PdfPage`)
* Attempts to be performant / efficient. Not a ton of effort has been put in here but it is a goal to keep this in mind.

### State of library
| Feature         | WIP | Alpha | Beta | Release |
| ---------       | ----| ----- | ---- | ----    |
| [Document access](docs/basics.md) |  |     |      |  :heavy_check_mark:  |
| [Stream based access](docs/basics.md#stream-based-access) |  |     |  :heavy_check_mark:  |    |
| [General modification](docs/basics.md#modifying-documents) <br> (non page content) |  |     |  :heavy_check_mark:  |    |
| [Merging / splitting](docs/merge_split.md) |  | |:heavy_check_mark: |  |
| [Streaming writer](docs/streaming_writer.md)  | |  |:heavy_check_mark: |  |
| [Resource dedup](docs/streaming_writer.md#resource-deduplication) | | :heavy_check_mark:  | |  |
| [Page content access](docs/page_content.md) | :heavy_check_mark: |   | |  |
| [Text extraction](docs/text_extraction.md) | |  | |  |
| Image extraction | |  | |  |
| Page content modification | |  | |  |

### Major Gaps
- [ ] PDF encryption support
- [ ] Filter support (deflate, ascii85, asciihex completed)
- [ ] Content stream access (partially complete)
- [ ] Higher level utility functions (eg. image / text extraction), currently all APIs are through the raw PdfObjects requiring extensive knowledge of the PDF spec to use pdflexer for most tasks.
- [ ] Public API cleanup / documentation. Lots of classes / properties exposed that will likely be internalized.
- [ ] Documentation / examples
