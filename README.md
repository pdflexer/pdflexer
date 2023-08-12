# pdflexer

pdflexer is a PDF parsing library. It is focused on efficient parsing and modification of PDF files and is mainly targeted for users familiar with the pdf spec. It is generally very fast at what it does (eg. splitting / merging / text extract shows multiple times better performance than alternatives). The parsing logic was implemented from scratch but some higher level functionality (eg. filters) were ported from the [pdf.js](https://github.com/mozilla/pdf.js) project.

pdflexer differs from existing .net libraries in that it:

- Is primarly designed for PDF modification (not just reading). Any object / page read from a PDF can be modified and written to others PDFs.
- Mutable model for page contents. Move, delete, modify existing text and graphics on page (note: in active development)
- Has lazy parsing features which allow objects to be parsed on demand increasing performance in many cases.
- Modern .net features (nullable enabled, Span, ArrayPool, Generic math)
- Designed for direct access to the native PDF objects types. Any higher level objects are simple wrappers areound the native pdf object types (eg `PdfPage` is a wrapper around a `PdfDictionary`. The `PdfDictionary` can be directly modified for features not implemented on `PdfPage`)
- Attempts to be performant / efficient. Not a ton of effort has been put in here but it is a goal to keep this in mind.

### State of library

| Feature                                                                            | WIP                | Alpha              | Beta               | Release            |
| ---------------------------------------------------------------------------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| [Document access](docs/basics.md)                                                  |                    |                    |                    | :heavy_check_mark: |
| [General modification](docs/basics.md#modifying-documents) <br> (non page content) |                    |                    |                    | :heavy_check_mark: |
| [Merging / splitting](docs/merge_split.md)                                         |                    |                    |                    | :heavy_check_mark: |
| [Streaming writer](docs/streaming_writer.md)                                       |                    |                    |                    | :heavy_check_mark: |
| [Page content access](docs/page_content.md)                                        |                    |                    | :heavy_check_mark: |                    |
| [Text extraction](docs/text_extraction.md)                                         |                    |                    | :heavy_check_mark: |                    |
| [Image extraction](docs/image_extraction.md)                                       |                    |                    | :heavy_check_mark: |                    |
| [Resource dedup](docs/streaming_writer.md#resource-deduplication)                  |                    | :heavy_check_mark: |                    |                    |
| [Content creation ](docs/content_creation.md)                                      |                    | :heavy_check_mark: |                    |                    |
| [Content redaction ](docs/redaction.md)                                            |                    | :heavy_check_mark: |                    |                    |
| Mutable Content                                                                    | :heavy_check_mark: |                    |                    |                    |

- Release - API stable and few breaking changes are expected. Feature has significant test coverage and has been used in real use cases on a wide variety of pdfs
- Beta - API stable but some breaking changes are expected. Feature has some test coverage and has been used in some real use cased.
- Alpha - API unstable and breaking changes are expected. Feature generally functional but may lack test coverage and may not have any real use.
- WIP - API unstable and many breaking changes are expected. Feature may have significant bugs, may lack test coverage and may not have any real use.

### Major Gaps

- [ ] Filter support (ascii85, asciihex, ccitt, deflate, lzw, and run length completed)
- [ ] Public API cleanup / documentation. Lots of classes / properties exposed that will likely be internalized.
- [ ] Documentation / examples

### Examples

Some examples are available as polyglot notebooks in the /examples/ folder.

- [General pdf loading](examples/loading.ipynb)
- [PDF creation](examples/pdf-creation.ipynb)
- [Text Extraction](examples/text-extraction.ipynb)
- [Redaction](examples/redaction.ipynb)
- [Mutable Content](examples/mutable-content.ipynb)
