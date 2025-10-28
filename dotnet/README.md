# OpenVDB C# Port (.NET 9)

This directory contains the C# port of the OpenVDB library for .NET 9.

## Overview

OpenVDB is a hierarchical data structure and toolkit for efficient storage and manipulation of sparse volumetric data. This is a faithful port from the original C++ implementation to modern C#, leveraging .NET 9 features for performance and safety.

## Project Structure

```
dotnet/
├── OpenVDB.sln                 # Main solution file
├── OpenVDB.Core/               # Core library - foundational types and structures
│   ├── Platform/               # Platform configuration and utilities
│   ├── Math/                   # Mathematical types and operations (to be ported)
│   ├── Tree/                   # Hierarchical tree data structures (to be ported)
│   ├── Grid/                   # Volume grid types (to be ported)
│   ├── IO/                     # File I/O and serialization (to be ported)
│   ├── Metadata/               # Metadata support (to be ported)
│   ├── Utils/                  # Utility classes (to be ported)
│   └── Threading/              # Multi-threading support (to be ported)
├── OpenVDB.Tools/              # Algorithms and tools for volume manipulation
└── OpenVDB.Points/             # Point cloud data structures and operations
```

## Prerequisites

- **.NET 9 SDK** or later
- A C# compatible IDE (Visual Studio 2022, Visual Studio Code, JetBrains Rider, etc.)

## Building

### Using Command Line

```bash
# From the dotnet directory
dotnet build OpenVDB.sln

# Or build specific projects
dotnet build OpenVDB.Core/OpenVDB.Core.csproj
dotnet build OpenVDB.Tools/OpenVDB.Tools.csproj
dotnet build OpenVDB.Points/OpenVDB.Points.csproj
```

### Using Visual Studio

1. Open `OpenVDB.sln` in Visual Studio 2022 or later
2. Build the solution (Ctrl+Shift+B)

## Current Status

### ✅ Completed

- [x] **Solution structure** - Created .NET solution and project files
- [x] **OpenVDB.Core project** - Set up with .NET 9, nullable enabled, unsafe code support
- [x] **OpenVDB.Tools project stub** - Created for future algorithm implementations
- [x] **OpenVDB.Points project stub** - Created for future point cloud features
- [x] **Platform.cs** - Simplified port from Platform.h and Platform.cc
  - Version namespace compatibility (VersionName constant)
  - Interop helpers for C++ compatibility
  - Thread-safety documentation attributes
  - Platform initialization utilities

### 🚧 In Progress / To Do

According to `PLAN_PORTAGE_CSHARP.md`, the porting will proceed in phases:

#### Phase 1: Foundation (Lot 1-4)
- [ ] **Lot 1: Foundations** (14 files)
  - [x] Platform.h/cc → Platform.cs
  - [ ] PlatformConfig.h
  - [ ] Exceptions.h
  - [ ] Types.h
  - [ ] TypeList.h
  - [ ] openvdb.h/cc
  - [ ] Metadata files

- [ ] **Lot 2: Mathematics** (28 files)
  - Vec2, Vec3, Vec4
  - Matrices (Mat, Mat3, Mat4)
  - Quaternions
  - Ray, BBox, Coord
  - Transform and Maps

- [ ] **Lot 3: Utilities** (13 files)
  - Assert, Logging
  - NodeMasks
  - PagedArray
  - Formats

- [ ] **Lot 4: Threading** (1 file)
  - Threading.h → Threading.cs

#### Phase 2: Core System (Lot 5-6)
- [ ] **Lot 5: Tree Structure** (13 files)
  - Tree, RootNode, InternalNode, LeafNode
  - ValueAccessor
  - Iterators

- [ ] **Lot 6: I/O** (17 files)
  - File, Stream, Archive
  - Compression
  - GridDescriptor

#### Phase 3: Advanced Features (Lot 7-8)
- [ ] **Lot 7: Tools** (56 files)
- [ ] **Lot 8: Points** (47 files)

**Total: 189 files to port**

## Architecture Differences: C++ vs C#

### Templates → Generics
```cpp
// C++
template<typename T>
class Grid { };
```
```csharp
// C#
public class Grid<T> where T : struct { }
```

### Smart Pointers → Reference Types
```cpp
// C++
using GridPtr = std::shared_ptr<Grid>;
```
```csharp
// C# - Classes are already reference types
public class Grid { }
```

### Macros → Attributes/Constants
```cpp
// C++
#define OPENVDB_VERSION_NAME v11_0_0
```
```csharp
// C#
public const string VersionName = "v11_0_0";
```

### TBB → Task Parallel Library
```cpp
// C++
tbb::parallel_for(range, operation);
```
```csharp
// C#
Parallel.For(start, end, i => operation(i));
```

### Memory Management
- **C++**: Manual management, RAII, smart pointers
- **C#**: Garbage Collector, `IDisposable` for native resources

## Naming Conventions

Following .NET conventions:

| Element | C++ Convention | C# Convention |
|---------|----------------|---------------|
| Classes | PascalCase | PascalCase ✓ |
| Methods | camelCase | PascalCase |
| Members | mVariableName | _variableName |
| Properties | - | PascalCase |
| Constants | UPPER_SNAKE_CASE | PascalCase |
| Namespaces | lowercase | PascalCase |

## Performance Considerations

- `AllowUnsafeBlocks=true` enables pointer operations for performance-critical code
- `AggressiveInlining` attribute used for hot paths
- Spans and Memory<T> used for efficient buffer operations
- ValueTypes (structs) used where appropriate to reduce GC pressure

## Testing

Tests will be added using xUnit:

```
dotnet/
├── OpenVDB.Core.Tests/
├── OpenVDB.Tools.Tests/
└── OpenVDB.Points.Tests/
```

## Contributing

This port follows the porting plan defined in `PLAN_PORTAGE_CSHARP.md` at the repository root. Each file is ported methodically to preserve the original functionality while adapting to C# idioms.

## License

This port maintains the original Apache License 2.0 from the OpenVDB project.

## References

- [Original OpenVDB Documentation](https://www.openvdb.org/documentation/)
- [Porting Plan](../PLAN_PORTAGE_CSHARP.md)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
