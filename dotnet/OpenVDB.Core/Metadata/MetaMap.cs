// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// MetaMap.cs - C# port of MetaMap.h and MetaMap.cc
//
// This file defines a container that maps names (strings) to metadata values.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVDB.Metadata
{
    /// <summary>
    /// Container that maps names (strings) to values of arbitrary types.
    /// </summary>
    public class MetaMap
    {
        private readonly Dictionary<string, Metadata> _metadata = new();

        /// <summary>
        /// Initializes a new instance of the MetaMap class.
        /// </summary>
        public MetaMap()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public MetaMap(MetaMap other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var kvp in other._metadata)
            {
                _metadata[kvp.Key] = kvp.Value.Copy();
            }
        }

        /// <summary>
        /// Return a copy of this map whose fields are shared with this map.
        /// </summary>
        public MetaMap CopyMeta()
        {
            var copy = new MetaMap();
            foreach (var kvp in _metadata)
            {
                copy._metadata[kvp.Key] = kvp.Value;
            }
            return copy;
        }

        /// <summary>
        /// Return a deep copy of this map that shares no data with this map.
        /// </summary>
        public MetaMap DeepCopyMeta()
        {
            return new MetaMap(this);
        }

        /// <summary>
        /// Assign a deep copy of another map to this map.
        /// </summary>
        public void Assign(MetaMap other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _metadata.Clear();
            foreach (var kvp in other._metadata)
            {
                _metadata[kvp.Key] = kvp.Value.Copy();
            }
        }

        /// <summary>
        /// Insert a new metadata field or overwrite the value of an existing field.
        /// </summary>
        /// <param name="name">The name of the metadata field.</param>
        /// <param name="value">The metadata value.</param>
        /// <exception cref="ArgumentException">If the field name is empty.</exception>
        /// <exception cref="TypeException">
        /// If a field with the given name already exists but has a different type.
        /// </exception>
        public void InsertMeta(string name, Metadata value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Field name cannot be null or empty", nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_metadata.TryGetValue(name, out var existing))
            {
                if (existing.TypeName != value.TypeName)
                {
                    throw new TypeException(
                        $"Cannot replace metadata '{name}' of type {existing.TypeName} with type {value.TypeName}");
                }
            }

            _metadata[name] = value.Copy();
        }

        /// <summary>
        /// Deep copy all of the metadata fields from the given map into this map.
        /// </summary>
        /// <exception cref="TypeException">
        /// If any field in the given map has the same name as but a different value type
        /// than one of this map's fields.
        /// </exception>
        public void InsertMeta(MetaMap other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var kvp in other._metadata)
            {
                InsertMeta(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Remove the given metadata field if it exists.
        /// </summary>
        public void RemoveMeta(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            _metadata.Remove(name);
        }

        /// <summary>
        /// Return the metadata with the given name, or null if no such field exists.
        /// </summary>
        public Metadata? this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                return _metadata.TryGetValue(name, out var metadata) ? metadata : null;
            }
        }

        /// <summary>
        /// Return a pointer to a TypedMetadata object of type T and with the given name.
        /// If no such field exists or if there is a type mismatch, return null.
        /// </summary>
        public TypedMetadata<T>? GetMetadata<T>(string name) where T : notnull
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return this[name] as TypedMetadata<T>;
        }

        /// <summary>
        /// Return a reference to the value of type T stored in the given metadata field.
        /// </summary>
        /// <exception cref="LookupException">If no field with the given name exists.</exception>
        /// <exception cref="TypeException">If the given field is not of type T.</exception>
        public T MetaValue<T>(string name) where T : notnull
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Field name cannot be null or empty", nameof(name));

            var metadata = this[name];
            if (metadata == null)
                throw new LookupException($"No metadata field with name '{name}'");

            if (metadata is not TypedMetadata<T> typed)
                throw new TypeException($"Metadata field '{name}' is of type {metadata.TypeName}, not {typeof(T).Name}");

            return typed.Value;
        }

        /// <summary>
        /// Set the value of a metadata field.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The field name.</param>
        /// <param name="value">The value to set.</param>
        public void SetMetaValue<T>(string name, T value) where T : notnull
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Field name cannot be null or empty", nameof(name));

            InsertMeta(name, new TypedMetadata<T>(value));
        }

        /// <summary>
        /// Gets an enumerable of all metadata entries.
        /// </summary>
        public IEnumerable<KeyValuePair<string, Metadata>> Metadata => _metadata;

        /// <summary>
        /// Gets the number of metadata fields.
        /// </summary>
        public int MetaCount => _metadata.Count;

        /// <summary>
        /// Clear all metadata fields.
        /// </summary>
        public void ClearMetadata()
        {
            _metadata.Clear();
        }

        /// <summary>
        /// Return a string describing this metadata map.
        /// </summary>
        /// <param name="indent">Prefix each line with this string.</param>
        public string AsString(string indent = "")
        {
            if (_metadata.Count == 0)
                return $"{indent}(no metadata)";

            var sb = new StringBuilder();
            foreach (var kvp in _metadata.OrderBy(kv => kv.Key))
            {
                sb.AppendLine($"{indent}{kvp.Key}: {kvp.Value.AsString()}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Returns a string representation of this metadata map.
        /// </summary>
        public override string ToString() => AsString();

        /// <summary>
        /// Determines whether this map is equivalent to another map.
        /// </summary>
        public bool Equals(MetaMap? other)
        {
            if (other == null) return false;
            if (_metadata.Count != other._metadata.Count) return false;

            foreach (var kvp in _metadata)
            {
                if (!other._metadata.TryGetValue(kvp.Key, out var otherMetadata))
                    return false;

                if (!kvp.Value.Equals(otherMetadata))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj) => obj is MetaMap other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var kvp in _metadata.OrderBy(kv => kv.Key))
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(MetaMap? left, MetaMap? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(MetaMap? left, MetaMap? right) => !(left == right);

        // Serialization will be implemented in Lot 6: IO
        // public void ReadMeta(BinaryReader reader);
        // public void WriteMeta(BinaryWriter writer);
    }
}
