# Product Guidelines

## Prose Style
Documentation and all user-facing communication should be **Technical and Direct**. Prioritize clarity, precision, and efficiency. Information should be conveyed in a professional tone that respects the technical expertise of developers working with the PDF specification.

## Brand Messaging and Core Values
The library's identity is built upon **Reliability and Speed**. All communication should emphasize that `pdflexer` is:
- **Exceptionally Fast:** Performance is a top-tier feature, not an afterthought.
- **Predictable and Stable:** The API and parsing logic must be robust and dependable for high-volume production environments.

## Documentation & Formatting
- **Code-First Approach:** Documentation should lead with clear, reproducible code snippets. Practical examples are the primary means of instruction.
- **Specification Alignment:** Terminology must strictly adhere to the official PDF specification (ISO 32000). Use standard names for objects and structures to ensure clarity for users familiar with the spec. The exception is when the library allows non-compliant documents to be parsed and manipulated. In that case, the documentation should explicitly state that the library allows more lenient parsing than the specification describes.
- **Structured for Efficiency:** Use headers, bullet points, and tables to make information easily scannable. Avoid long blocks of narrative prose.
