// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Metadata.cs - C# port of Metadata.h and Metadata.cc
//
// This file defines the metadata system for storing typed key-value data in grids.

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVDB.Metadata
{
    /// <summary>
    /// Base class for storing metadata information in a grid.
    /// </summary>
    public abstract class Metadata
    {
        /// <summary>
        /// Return the type name of the metadata.
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Return a copy of the metadata.
        /// </summary>
        public abstract Metadata Copy();

        /// <summary>
        /// Copy the given metadata into this metadata.
        /// </summary>
        public abstract void CopyFrom(Metadata other);

        /// <summary>
        /// Return a textual representation of this metadata.
        /// </summary>
        public abstract string AsString();

        /// <summary>
        /// Return the boolean representation of this metadata.
        /// </summary>
        /// <remarks>
        /// Empty strings and zero values evaluate to false; most other values evaluate to true.
        /// </remarks>
        public abstract bool AsBool();

        /// <summary>
        /// Return the size of this metadata in bytes.
        /// </summary>
        public abstract uint Size { get; }

        /// <summary>
        /// Returns a string representation of this metadata.
        /// </summary>
        public override string ToString() => AsString();

        /// <summary>
        /// Determines whether the specified metadata is equal to the current metadata.
        /// </summary>
        public abstract bool Equals(Metadata? other);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj) => obj is Metadata other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode() => TypeName.GetHashCode();

        /// <summary>
        /// Read this metadata from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader to read from.</param>
        public void Read(BinaryReader reader)
        {
            // Read the size of the value data
            uint numBytes = reader.ReadUInt32();
            ReadValue(reader, numBytes);
        }

        /// <summary>
        /// Write this metadata to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer to write to.</param>
        public void Write(BinaryWriter writer)
        {
            // Write the size of the value data
            // For simplicity, we'll write 0 as a placeholder and implementations can override
            writer.Write((uint)Size);
            WriteValue(writer);
        }

        /// <summary>
        /// Read the metadata value from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader to read from.</param>
        /// <param name="numBytes">The number of bytes to read.</param>
        protected abstract void ReadValue(BinaryReader reader, uint numBytes);

        /// <summary>
        /// Write the metadata value to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer to write to.</param>
        protected abstract void WriteValue(BinaryWriter writer);

        // ========================================================================
        // FACTORY AND REGISTRY
        // ========================================================================

        private static readonly Dictionary<string, Func<Metadata>> _registry = new();
        private static readonly object _registryLock = new();

        /// <summary>
        /// Create new metadata of the given type.
        /// </summary>
        /// <param name="typeName">The type name of the metadata to create.</param>
        /// <returns>A new metadata instance, or null if the type is not registered.</returns>
        public static Metadata? CreateMetadata(string typeName)
        {
            lock (_registryLock)
            {
                if (_registry.TryGetValue(typeName, out var factory))
                {
                    return factory();
                }
            }
            return null;
        }

        /// <summary>
        /// Return true if the given type is known by the metadata type registry.
        /// </summary>
        public static bool IsRegisteredType(string typeName)
        {
            lock (_registryLock)
            {
                return _registry.ContainsKey(typeName);
            }
        }

        /// <summary>
        /// Clear out the metadata registry.
        /// </summary>
        public static void ClearRegistry()
        {
            lock (_registryLock)
            {
                _registry.Clear();
            }
        }

        /// <summary>
        /// Register the given metadata type along with a factory function.
        /// </summary>
        public static void RegisterType(string typeName, Func<Metadata> createMetadata)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            if (createMetadata == null)
                throw new ArgumentNullException(nameof(createMetadata));

            lock (_registryLock)
            {
                _registry[typeName] = createMetadata;
            }
        }

        /// <summary>
        /// Unregister the given metadata type.
        /// </summary>
        public static void UnregisterType(string typeName)
        {
            lock (_registryLock)
            {
                _registry.Remove(typeName);
            }
        }
    }

    /// <summary>
    /// Subclass to hold raw data of an unregistered type.
    /// </summary>
    public sealed class UnknownMetadata : Metadata
    {
        private readonly string _typeName;
        private byte[] _bytes;

        /// <summary>
        /// Initializes a new instance of the UnknownMetadata class.
        /// </summary>
        public UnknownMetadata(string typeName = "<unknown>")
        {
            _typeName = typeName ?? "<unknown>";
            _bytes = Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the type name of this metadata.
        /// </summary>
        public override string TypeName => _typeName;

        /// <summary>
        /// Gets the size of this metadata.
        /// </summary>
        public override uint Size => (uint)_bytes.Length;

        /// <summary>
        /// Gets or sets the raw bytes of this metadata.
        /// </summary>
        public byte[] Bytes
        {
            get => _bytes;
            set => _bytes = value ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Returns a copy of this metadata.
        /// </summary>
        public override Metadata Copy()
        {
            var copy = new UnknownMetadata(_typeName);
            copy._bytes = new byte[_bytes.Length];
            Array.Copy(_bytes, copy._bytes, _bytes.Length);
            return copy;
        }

        /// <summary>
        /// Copies the given metadata into this metadata.
        /// </summary>
        public override void CopyFrom(Metadata other)
        {
            if (other is UnknownMetadata unknown)
            {
                _bytes = new byte[unknown._bytes.Length];
                Array.Copy(unknown._bytes, _bytes, unknown._bytes.Length);
            }
        }

        /// <summary>
        /// Returns a string representation of this metadata.
        /// </summary>
        public override string AsString() => _bytes.Length == 0 ? "" : "<binary data>";

        /// <summary>
        /// Returns the boolean representation of this metadata.
        /// </summary>
        public override bool AsBool() => _bytes.Length > 0;

        /// <summary>
        /// Determines whether this metadata equals another.
        /// </summary>
        public override bool Equals(Metadata? other)
        {
            if (other is not UnknownMetadata unknown) return false;
            if (_typeName != unknown._typeName) return false;
            if (_bytes.Length != unknown._bytes.Length) return false;
            
            for (int i = 0; i < _bytes.Length; i++)
            {
                if (_bytes[i] != unknown._bytes[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Read the metadata value from a binary reader.
        /// </summary>
        protected override void ReadValue(BinaryReader reader, uint numBytes)
        {
            _bytes = reader.ReadBytes((int)numBytes);
        }

        /// <summary>
        /// Write the metadata value to a binary writer.
        /// </summary>
        protected override void WriteValue(BinaryWriter writer)
        {
            writer.Write(_bytes);
        }
    }

    /// <summary>
    /// Templated metadata class to hold specific types.
    /// </summary>
    /// <typeparam name="T">The value type to store.</typeparam>
    public class TypedMetadata<T> : Metadata where T : notnull
    {
        private T _value;

        /// <summary>
        /// Initializes a new instance with the default value.
        /// </summary>
        public TypedMetadata()
        {
            _value = default(T)!;
        }

        /// <summary>
        /// Initializes a new instance with the specified value.
        /// </summary>
        public TypedMetadata(T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public TypedMetadata(TypedMetadata<T> other)
        {
            _value = other._value;
        }

        /// <summary>
        /// Gets the type name of this metadata.
        /// </summary>
        public override string TypeName => StaticTypeName();

        /// <summary>
        /// Gets the size of this metadata.
        /// </summary>
        public override uint Size => (uint)System.Runtime.InteropServices.Marshal.SizeOf<T>();

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value
        {
            get => _value;
            set => _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns the static type name for this metadata type.
        /// </summary>
        public static string StaticTypeName() => typeof(T).FullName ?? typeof(T).Name;

        /// <summary>
        /// Returns a copy of this metadata.
        /// </summary>
        public override Metadata Copy() => new TypedMetadata<T>(_value);

        /// <summary>
        /// Copies the given metadata into this metadata.
        /// </summary>
        public override void CopyFrom(Metadata other)
        {
            if (other is TypedMetadata<T> typed)
            {
                _value = typed._value;
            }
            else
            {
                throw new TypeException($"Cannot copy from {other.TypeName} to {TypeName}");
            }
        }

        /// <summary>
        /// Returns a string representation of this metadata.
        /// </summary>
        public override string AsString() => _value?.ToString() ?? "";

        /// <summary>
        /// Returns the boolean representation of this metadata.
        /// </summary>
        public override bool AsBool()
        {
            // Special handling for common types
            if (_value is bool b) return b;
            if (_value is string s) return !string.IsNullOrEmpty(s);
            if (_value is int i) return i != 0;
            if (_value is long l) return l != 0;
            if (_value is float f) return System.Math.Abs(f) > float.Epsilon;
            if (_value is double d) return System.Math.Abs(d) > double.Epsilon;
            
            // For other types, return true if not default
            return !EqualityComparer<T>.Default.Equals(_value, default(T)!);
        }

        /// <summary>
        /// Determines whether this metadata equals another.
        /// </summary>
        public override bool Equals(Metadata? other)
        {
            if (other is not TypedMetadata<T> typed) return false;
            return EqualityComparer<T>.Default.Equals(_value, typed._value);
        }

        /// <summary>
        /// Read the metadata value from a binary reader.
        /// </summary>
        protected override void ReadValue(BinaryReader reader, uint numBytes)
        {
            // Simple implementation for common types
            // TODO: This needs to be extended for complex types
            if (typeof(T) == typeof(bool))
            {
                _value = (T)(object)reader.ReadBoolean();
            }
            else if (typeof(T) == typeof(int))
            {
                _value = (T)(object)reader.ReadInt32();
            }
            else if (typeof(T) == typeof(long))
            {
                _value = (T)(object)reader.ReadInt64();
            }
            else if (typeof(T) == typeof(float))
            {
                _value = (T)(object)reader.ReadSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                _value = (T)(object)reader.ReadDouble();
            }
            else if (typeof(T) == typeof(string))
            {
                var bytes = reader.ReadBytes((int)numBytes);
                _value = (T)(object)System.Text.Encoding.UTF8.GetString(bytes);
            }
            else
            {
                // For other types, try reading raw bytes
                var bytes = reader.ReadBytes((int)numBytes);
                // This is a simplified approach - real implementation would need proper serialization
                throw new NotImplementedException($"Reading metadata of type {typeof(T)} is not yet implemented");
            }
        }

        /// <summary>
        /// Write the metadata value to a binary writer.
        /// </summary>
        protected override void WriteValue(BinaryWriter writer)
        {
            // Simple implementation for common types
            if (typeof(T) == typeof(bool))
            {
                writer.Write((bool)(object)_value!);
            }
            else if (typeof(T) == typeof(int))
            {
                writer.Write((int)(object)_value!);
            }
            else if (typeof(T) == typeof(long))
            {
                writer.Write((long)(object)_value!);
            }
            else if (typeof(T) == typeof(float))
            {
                writer.Write((float)(object)_value!);
            }
            else if (typeof(T) == typeof(double))
            {
                writer.Write((double)(object)_value!);
            }
            else if (typeof(T) == typeof(string))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes((string)(object)_value!);
                writer.Write(bytes);
            }
            else
            {
                // For other types, would need proper serialization
                throw new NotImplementedException($"Writing metadata of type {typeof(T)} is not yet implemented");
            }
        }

        /// <summary>
        /// Creates a new metadata instance of this type.
        /// </summary>
        public static Metadata CreateMetadata() => new TypedMetadata<T>();

        /// <summary>
        /// Registers this metadata type.
        /// </summary>
        public static void RegisterType()
        {
            Metadata.RegisterType(StaticTypeName(), CreateMetadata);
        }

        /// <summary>
        /// Unregisters this metadata type.
        /// </summary>
        public static void UnregisterType()
        {
            Metadata.UnregisterType(StaticTypeName());
        }

        /// <summary>
        /// Returns true if this metadata type is registered.
        /// </summary>
        public static bool IsRegisteredType()
        {
            return Metadata.IsRegisteredType(StaticTypeName());
        }
    }

    // ========================================================================
    // COMMON METADATA TYPES
    // ========================================================================

    /// <summary>Boolean metadata.</summary>
    public sealed class BoolMetadata : TypedMetadata<bool>
    {
        /// <summary>Creates a new BoolMetadata.</summary>
        public BoolMetadata() : base() { }
        /// <summary>Creates a new BoolMetadata with the specified value.</summary>
        public BoolMetadata(bool value) : base(value) { }
    }

    /// <summary>Double-precision floating point metadata.</summary>
    public sealed class DoubleMetadata : TypedMetadata<double>
    {
        /// <summary>Creates a new DoubleMetadata.</summary>
        public DoubleMetadata() : base() { }
        /// <summary>Creates a new DoubleMetadata with the specified value.</summary>
        public DoubleMetadata(double value) : base(value) { }
    }

    /// <summary>Single-precision floating point metadata.</summary>
    public sealed class FloatMetadata : TypedMetadata<float>
    {
        /// <summary>Creates a new FloatMetadata.</summary>
        public FloatMetadata() : base() { }
        /// <summary>Creates a new FloatMetadata with the specified value.</summary>
        public FloatMetadata(float value) : base(value) { }
    }

    /// <summary>32-bit integer metadata.</summary>
    public sealed class Int32Metadata : TypedMetadata<int>
    {
        /// <summary>Creates a new Int32Metadata.</summary>
        public Int32Metadata() : base() { }
        /// <summary>Creates a new Int32Metadata with the specified value.</summary>
        public Int32Metadata(int value) : base(value) { }
    }

    /// <summary>64-bit integer metadata.</summary>
    public sealed class Int64Metadata : TypedMetadata<long>
    {
        /// <summary>Creates a new Int64Metadata.</summary>
        public Int64Metadata() : base() { }
        /// <summary>Creates a new Int64Metadata with the specified value.</summary>
        public Int64Metadata(long value) : base(value) { }
    }

    /// <summary>String metadata.</summary>
    public sealed class StringMetadata : TypedMetadata<string>
    {
        /// <summary>Creates a new StringMetadata.</summary>
        public StringMetadata() : base(string.Empty) { }
        /// <summary>Creates a new StringMetadata with the specified value.</summary>
        public StringMetadata(string value) : base(value ?? string.Empty) { }
    }
}
