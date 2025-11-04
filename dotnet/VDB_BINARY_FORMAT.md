# OpenVDB Binary File Format Specification

## Overview

This document describes the binary file format used by OpenVDB for serializing volumetric data structures to disk. The format is designed to efficiently store sparse volumetric data while supporting features like compression, instancing, and partial loading.

## File Structure

An OpenVDB file (`.vdb`) consists of the following sequential sections:

```
[File Header]
[File Metadata]
[Grid Count]
[Grid Descriptors] (repeated for each grid)
[Grid Data] (repeated for each grid)
```

## Data Types

All multi-byte numeric values are stored in **little-endian** format unless otherwise specified.

- **int8**: 1-byte signed integer
- **int32**: 4-byte signed integer  
- **int64**: 8-byte signed integer
- **uint32**: 4-byte unsigned integer
- **uint64**: 8-byte unsigned integer
- **float**: 4-byte IEEE-754 floating point
- **double**: 8-byte IEEE-754 floating point
- **string**: Variable-length UTF-8 encoded string (see String Format below)
- **char**: 1-byte character

### String Format

Strings are encoded as:
1. **int32** length (number of bytes, not characters)
2. **bytes** UTF-8 encoded string data (no null terminator)

Empty strings are represented as length=0 with no following bytes.

## File Header

The file header is a fixed-size structure that identifies the file as a VDB file and provides versioning information.

| Offset | Size | Type | Field | Description |
|--------|------|------|-------|-------------|
| 0x00 | 8 | int64 | Magic Number | Always `0x56444220` ("VDB " in ASCII with extra byte) |
| 0x08 | 4 | uint32 | File Format Version | Current: 224, Minimum supported: 222 |
| 0x0C | 4 | uint32 | Library Major Version | OpenVDB library major version (e.g., 11) |
| 0x10 | 4 | uint32 | Library Minor Version | OpenVDB library minor version (e.g., 0) |
| 0x14 | 1 | char | Has Grid Offsets | 1 if file supports random access, 0 otherwise |
| 0x15 | 36 | char[36] | UUID | Fixed-length ASCII UUID string (32 hex digits + 4 hyphens) |

**Total Header Size:** 51 bytes (0x33)

### Magic Number Details

The magic number `0x56444220` corresponds to the ASCII string "VDB " (with a space), which allows quick identification of VDB files. This value must be present for the file to be recognized as valid.

### UUID Format

The UUID is a 36-character ASCII string in the form:
```
XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
```
where each X is a hexadecimal digit (0-9, A-F). The UUID is randomly generated when writing and used to detect file changes.

### File Format Version Notes

- **Version 222**: Introduced node mask compression
- **Version 223**: Introduced Blosc compression and PointIndexGrid support  
- **Version 224**: Introduced multi-pass I/O (current)

Files with version < 222 are not supported and will throw an error when reading.

## File Metadata

After the header, the file contains a metadata section that stores file-level key-value pairs.

The metadata is serialized using the MetaMap format:

1. **int32** metadata count (number of key-value pairs)
2. For each metadata entry:
   - **string** key name
   - **string** type name (e.g., "string", "int32", "vec3s")
   - **bytes** value data (format depends on type)

### Common Metadata Types

- **string**: string value
- **int32**: int32 value
- **int64**: int64 value  
- **float**: float value
- **double**: double value
- **vec3s**: 3 float values (x, y, z)
- **vec3d**: 3 double values (x, y, z)
- **vec3i**: 3 int32 values (x, y, z)

## Grid Count

After file metadata:

| Size | Type | Field | Description |
|------|------|-------|-------------|
| 4 | int32 | Grid Count | Number of grids in the file |

## Grid Descriptors

For each grid, a descriptor is written that contains metadata about the grid's location and type. Grid descriptors allow for random access and partial loading.

Each Grid Descriptor contains:

### Grid Descriptor Header

1. **string** Unique Name - The grid's name (may include instance suffix)
2. **string** Grid Type - Type name with optional `_HalfFloat` suffix
3. **string** Instance Parent Name - Name of parent grid if instancing (empty otherwise)

### Grid Descriptor Positions

After the header, three file offsets are stored:

| Size | Type | Field | Description |
|------|------|-------|-------------|
| 8 | int64 | Grid Position | File offset to start of grid data |
| 8 | int64 | Block Position | File offset to tree blocks (internal/leaf nodes) |
| 8 | int64 | End Position | File offset to end of grid data |

**Total descriptor size:** Variable (depends on string lengths) + 24 bytes for positions

### Grid Type Format

The grid type string identifies the type of values stored in the grid, e.g.:
- `Tree_float_5_4_3` - Float tree with 5-4-3 node configuration
- `Tree_double_5_4_3` - Double tree
- `Tree_vec3s_5_4_3` - Vec3f tree

If the `_HalfFloat` suffix is present, float values are quantized to 16-bit half precision.

### Instance Parent Name

If non-empty, this grid shares its tree data with another grid (the instance parent). This allows multiple grids to reference the same voxel data without duplication.

## Grid Data

Each grid's data follows its descriptor. The grid data structure is:

```
[Grid Metadata]
[Transform]
[Tree Structure]
```

### Grid Metadata

Similar to file metadata, but specific to this grid:

1. **int32** metadata count
2. For each entry:
   - **string** key
   - **string** type
   - **bytes** value

Common grid metadata includes:
- `name` (string): Grid name
- `file_compression` (string): Compression settings
- `file_delayed_load` (varies): Delayed loading metadata
- `class` (string): Grid class (e.g., "level set", "fog volume")

### Transform

The transform maps between index space (integer voxel coordinates) and world space (real-world coordinates). The format depends on the transform type:

1. **string** Transform type name
2. **bytes** Transform-specific data

Common transform types:
- `LinearTransform`: 4x4 matrix
- `ScaleTranslateMap`: Scale vector + translation vector
- `UniformScaleTranslateMap`: Uniform scale float + translation vector

### Tree Structure

The tree structure stores the actual voxel data in a sparse hierarchical format. For a typical configuration (5-4-3):

```
[Root Node]
[Internal Nodes (Level 1)]
[Internal Nodes (Level 2)]
[Leaf Nodes]
```

#### Root Node

1. **value** Background value (type depends on grid type)
2. **int32** Child count
3. **int32** Tile count  
4. For each tile:
   - **int32[3]** Coordinate (x, y, z)
   - **value** Tile value
   - **byte** Active state
5. For each child:
   - **int32[3]** Coordinate (x, y, z)
   - Internal node data (see below)

#### Internal Nodes

1. **value** Background value (for this subtree)
2. **byte** Value mask indicator (for compression)
3. **byte** Child mask indicator (for compression)
4. Depending on compression:
   - Value mask data (compressed or uncompressed)
   - Child mask data (compressed or uncompressed)
   - Value buffer (compressed or uncompressed)
   - Child table (file offsets or inline data)

#### Leaf Nodes

1. **byte** Buffer compression info
2. **value mask** Active voxel mask (compressed or uncompressed)
3. **value buffer** Voxel values (compressed or uncompressed, may use inactive value compression)

## Compression

OpenVDB supports several compression schemes:

### Compression Flags

Stored per-grid in metadata, combinations of:
- `COMPRESS_NONE (0x0)`: No compression
- `COMPRESS_ZIP (0x1)`: ZLIB/Deflate compression  
- `COMPRESS_ACTIVE_MASK (0x2)`: Inactive value compression
- `COMPRESS_BLOSC (0x4)`: Blosc compression (faster than ZIP)

### Inactive Value Compression

When `COMPRESS_ACTIVE_MASK` is enabled, leaf nodes can be stored more efficiently by not storing inactive values that match the background or a common value. The compression metadata byte indicates the storage mode:

- **0**: No inactive values or all inactive = +background
- **1**: All inactive values = -background  
- **2**: All inactive values = single non-background value
- **3**: Mask selects between -background and +background
- **4**: Mask selects between background and one other value
- **5**: Mask selects between two non-background values
- **6**: More than 2 inactive values (no compression)

### Buffer Compression

When `COMPRESS_ZIP` or `COMPRESS_BLOSC` is enabled, node value buffers are compressed:

1. **int64** Compressed size
2. **int64** Uncompressed size  
3. **bytes** Compressed data

## Instancing

When multiple grids share the same tree structure, only one copy of the tree data is written. Other grids reference it via the Instance Parent Name in their descriptor. When reading:

1. The instance parent grid is read normally
2. Instance child grids point to the parent's tree via Instance Parent Name
3. The tree pointer is either shared (if instancing enabled) or copied

## Multi-Pass I/O

For grids requiring multi-pass I/O (version 224+), the tree is written in multiple passes:

1. **First pass**: Tree structure and topology
2. **Subsequent passes**: Voxel data in chunks

This is tracked via the `pass` and `leaf` counters in StreamMetadata during I/O operations.

## Delayed Loading

When delayed loading is enabled (and file has grid offsets):

1. Only grid metadata and descriptors are read initially
2. Tree data remains on disk
3. Data is loaded on-demand when accessed
4. File may be memory-mapped for efficient access

The `DelayedLoadMetadata` attached to grids tracks:
- Compression mask for each leaf
- Compressed size for each leaf (if compression enabled)

## Example File Layout

```
Offset  | Content
--------|----------------------------------------------------
0x0000  | Magic: 0x56444220
0x0008  | File Version: 224
0x000C  | Library Version: 11.0
0x0014  | Has Grid Offsets: 1
0x0015  | UUID: "A1B2C3D4-E5F6-..."
0x0033  | File Metadata: { "creator": "OpenVDB 11.0", ... }
0x????  | Grid Count: 2
0x????  | Grid 1 Descriptor
0x????  | Grid 2 Descriptor  
0x????  | Grid 1 Data
0x????  | Grid 2 Data
```

## Compatibility Notes

1. **Backward Compatibility**: Newer libraries can read older file versions (>= 222)
2. **Forward Compatibility**: Older libraries cannot read newer file versions
3. **Unknown Types**: Files containing grids of unknown type can still be read; unknown grids are skipped
4. **ABI Versioning**: The library version numbers help detect ABI mismatches

## References

This specification is based on the OpenVDB C++ implementation found in:
- `openvdb/io/Archive.cc` - Archive base class with header reading/writing
- `openvdb/io/File.cc` - File-based I/O implementation
- `openvdb/io/Stream.cc` - Stream-based I/O implementation
- `openvdb/io/GridDescriptor.cc` - Grid descriptor format
- `openvdb/version.h` - Version constants and magic numbers
