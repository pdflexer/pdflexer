# Initial Concept

User Input: What do you want to build?

# Product Vision
pdflexer is a high-performance .NET library designed for efficient PDF parsing and modification. It prioritizes direct access to native PDF objects and provides a mutable model for page contents, enabling developers to build powerful PDF manipulation tools with industry-leading speed.

# Target Audience
- .NET Developers building specialized PDF manipulation tools.
- High-performance systems requiring rapid processing of large volumes of PDF data.
- Applications needing low-level access to internal PDF structures and metadata.

# Core Goals
- **Superior Performance:** Provide significantly faster execution for common PDF tasks such as splitting, merging, and text extraction compared to existing .NET alternatives.
- **Mutable Content Model:** Enable developers to directly modify, move, or delete existing text and graphics on a PDF page through an intuitive object model.

# Key Features & Differentiators
- **Native PDF Object Access:** High-level abstractions (like `PdfPage`) are designed as thin wrappers around native PDF dictionaries, allowing for raw access when the higher-level API doesn't cover a specific use case.
- **Performance-First Architecture:** Built from the ground up using modern .NET performance features, including `Span<T>`, `ArrayPool<T>`, and Generic Math, to minimize memory allocations and maximize processing throughput.