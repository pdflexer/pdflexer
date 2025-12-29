# Technology Stack

## Core Technologies
- **Programming Language:** C# 11.0
- **Runtime Environment:** .NET 8.0 / .NET 10.0
- **Project Structure:** MSBuild (.csproj) based .NET Solution

## Key Libraries & Dependencies
- **Memory & Performance:**
    - `DotNext.Unsafe`: Used for low-level memory operations and unsafe optimizations.
    - `Microsoft.IO.RecyclableMemoryStream`: Provides a pooling mechanism for `MemoryStream` objects to reduce GC pressure.
    - `System.IO.Pipelines`: Employed for efficient, asynchronous I/O and stream processing.
    - `System.Numerics.Vectors`: Utilized for SIMD-accelerated operations.
- **Image Processing:**
    - `SixLabors.ImageSharp`: Extends the library with image extraction and manipulation capabilities.

## Development & Testing
- **Testing Framework:** `xUnit`
- **Build Tooling:** PowerShell (`create-module.ps1`) for module packaging.
