# OpenVDB AI Coding Agent Instructions

## Project Overview
OpenVDB is a hierarchical sparse volumetric data structure library for efficient storage and manipulation of sparse volumes (level sets, fog volumes, point clouds). The project consists of multiple components: core library, NanoVDB (GPU-friendly), OpenVDB AX (expression language), and DCC plugins (Houdini/Maya).

## Architecture Fundamentals

### Core Data Structures
- **Grid**: Top-level container combining a Tree + Transform + Metadata. Use `Grid<TreeType>` pattern.
- **Tree**: Hierarchical B-tree structure (`Tree4<ValueType, 5, 4, 3>` = root + 2 internal levels + leaf nodes).
  - Standard configuration: 32³ upper internal, 16³ lower internal, 8³ leaf nodes
  - Access via `FloatTree`, `DoubleTree`, `Vec3STree`, etc. (defined in `openvdb/openvdb.h`)
- **Nodes**: `RootNode` → `InternalNode<N>` → `LeafNode<ValueType, Log2Dim>`
- **ValueAccessor**: Critical for performance—caches node pointers to accelerate spatially coherent access patterns. Always use for repeated getValue/setValue operations.

### Namespace Structure
All code lives under `openvdb::OPENVDB_VERSION_NAME` via `OPENVDB_USE_VERSION_NAMESPACE` macro. This enables ABI versioning for library compatibility.

### Component Boundaries
- **openvdb/openvdb/**: Core library (Grid, Tree, io, math, tools, points)
- **nanovdb/nanovdb/**: GPU-optimized read-only grids (self-contained, minimal dependencies)
- **openvdb_ax/**: JIT-compiled expression language for grid manipulation (requires LLVM)
- **openvdb_houdini/**, **openvdb_maya/**: DCC plugin integrations
- **openvdb_cmd/**: Command-line tools (vdb_print, vdb_render, vdb_tool, etc.)

## Build System (CMake)

### Critical Build Commands
```bash
# Standard build
mkdir build && cd build
cmake .. -DOPENVDB_BUILD_CORE=ON -DUSE_BLOSC=ON
cmake --build . --parallel 4 --config Release --target install

# Component flags (use in cmake configure)
-DOPENVDB_BUILD_CORE=ON          # Core library
-DOPENVDB_BUILD_BINARIES=ON      # vdb_print, etc.
-DOPENVDB_BUILD_PYTHON_MODULE=ON # Python bindings
-DOPENVDB_BUILD_AX=ON            # OpenVDB AX
-DOPENVDB_BUILD_NANOVDB=ON       # NanoVDB
-DOPENVDB_BUILD_UNITTESTS=ON     # Unit tests
```

### Dependency Management
- **Required**: CMake 3.24+, C++17 compiler, TBB 2020.3+
- **Highly Recommended**: Blosc 1.17.0+ (compression), Imath 3.2+ (half float)
- **AX Only**: LLVM 15.0+ (avoid 17 on Linux with Python)
- **Python**: Python 3.10+, nanobind 2.0+
- Use `find_package()` mechanism—see `cmake/FindBlosc.cmake`, `cmake/FindTBB.cmake` for patterns
- Control via `XXX_ROOT`, `XXX_INCLUDEDIR`, `XXX_LIBRARYDIR` variables
- Set `DISABLE_CMAKE_SEARCH_PATHS=ON` to strictly use provided paths

### Windows-Specific
- Use vcpkg for dependencies: `vcpkg install tbb:x64-windows blosc:x64-windows boost-iostreams:x64-windows`
- Toolchain: `-DCMAKE_TOOLCHAIN_FILE=<vcpkg>/scripts/buildsystems/vcpkg.cmake -DVCPKG_TARGET_TRIPLET=x64-windows`
- Explicit instantiation disabled by default on Windows (linker OOM issues)

## Coding Conventions

### Naming (Critical—Enforced)
- **Classes/Structs**: `PascalCase` (e.g., `Grid`, `TreeIterator`, `AffineMap`)
- **Methods**: `camelCase` (e.g., `getAccessor()`, `setValue()`, `gridType()`)
- **Member variables**: `mVariableName` (always prefix with `m`)
- **Local variables/args**: `camelCase` (e.g., `ijk`, `offset`, `value`)
- **Constants**: `UPPER_SNAKE_CASE` with library prefix if global (e.g., `OPENVDB_LEVEL_SET_HALF_WIDTH`)
- **Namespaces**: lowercase (e.g., `tools`, `math`, `internal`)

### File Organization
- Header guards: `OPENVDB_<PATH>_<NAME>_HAS_BEEN_INCLUDED`
- Headers: `.h`, Source: `.cc`
- Include order: corresponding header, local headers, library headers, system headers
- Use double quotes `""` for same-directory includes, angle brackets `<>` for external
- Forward declare when possible; avoid transitive includes

### Style Rules
- **Indentation**: 4 spaces (never tabs)
- **Line length**: Max 100 characters
- **Bracing**: K&R style
- **Comments**: Use `//` style, even for multi-line
- Always call `openvdb::initialize()` before using grids for serialization/PointDataGrid attributes

## Testing

### Running Tests
```bash
# Build with tests enabled
cmake .. -DOPENVDB_BUILD_UNITTESTS=ON -DOPENVDB_BUILD_AX_UNITTESTS=ON
cmake --build . --target install

# Run tests (from build directory)
ctest -V  # or run individual test executables
./openvdb/unittest/vdb_test
./openvdb_ax/unittest/vdb_ax_test
```

### Test Framework
- Uses GoogleTest (gtest) for C++ unit tests
- Python tests use unittest framework with numpy
- CI script: `ci/build.sh --components=core,test` (see `ci/build.sh` for component aliases)

## Common Patterns

### Creating Grids
```cpp
// Typed grid creation
FloatGrid::Ptr grid = FloatGrid::create(/*background=*/0.0f);
// Or use factory
auto grid = createGrid<FloatGrid>(0.0f);

// Level set grids (narrow-band signed distance fields)
auto ls = createLevelSet<FloatGrid>(/*voxelSize=*/0.1, /*halfWidth=*/3.0);
```

### Efficient Tree Access
```cpp
FloatGrid::Accessor acc = grid->getAccessor();
for (Coord ijk : activeVoxels) {
    float value = acc.getValue(ijk);  // Cached access—much faster than grid->tree().getValue(ijk)
    acc.setValue(ijk, newValue);
}
```

### Tool Usage
All processing tools are in `openvdb/tools/`. Examples:
- `tools::Filter<Grid>`: Gaussian, mean, median filters
- `tools::LevelSetSphere`: Generate sphere level sets
- `tools::meshToVolume()`: Mesh → VDB conversion
- `tools::volumeToMesh()`: VDB → mesh conversion

## Key Files for Understanding
- `openvdb/openvdb/openvdb.h`: Main API entry point, common type definitions
- `openvdb/openvdb/Grid.h`: Grid interface and factory methods
- `openvdb/openvdb/tree/Tree.h`: Tree implementation (read comments on memory layout)
- `openvdb/openvdb/tree/ValueAccessor.h`: Accelerated access patterns
- `CMakeLists.txt`: Root build configuration—comprehensive option documentation
- `doc/codingstyle.txt`: Complete style guide (authoritative)
- `doc/build.txt`: Deep-dive on build system and dependency resolution

## Known Gotchas
- **Thread safety**: ValueAccessors are NOT thread-safe—use one per thread
- **Delayed loading**: Core library supports lazy loading via `DelayedLoadMetadata` (see `openvdb/io/`)
- **Explicit instantiation**: When `USE_EXPLICIT_INSTANTIATION=ON`, templates are pre-instantiated for common types (faster linking, slower core build)
- **Blosc compatibility**: Versions 1.5.3-1.16.x have compatibility issues—use 1.17.0+
- **LLVM 17 + Python**: Known symbol conflicts on Linux—use LLVM 18+

## DCC Integration
When building against Houdini/Maya, use `USE_HOUDINI=ON` or `USE_MAYA=ON`. These automatically configure TBB/Blosc from DCC installations. See `cmake/OpenVDBHoudiniSetup.cmake` and `cmake/OpenVDBMayaSetup.cmake`.

## License & Contributions
- Apache License 2.0 (migrated from MPL 2.0)
- All commits require DCO sign-off: `git commit --signoff`
- CLA required (managed via EasyCLA)
- See `CONTRIBUTING.md` for full guidelines
