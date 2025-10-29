// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// TypeList.cs - C# port of TypeList.h
//
// This file provides a TypeList mechanism for working with heterogeneous type collections.
// In C++, TypeList uses template metaprogramming. In C#, we use a combination of
// generic types and runtime type information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVDB
{
    /// <summary>
    /// A compile-time list of types that can be queried and manipulated.
    /// </summary>
    /// <remarks>
    /// In C++, TypeList is implemented using variadic templates and compile-time
    /// metaprogramming. In C#, we use a runtime representation with Type[] and
    /// provide similar query capabilities.
    /// 
    /// Note: Some compile-time operations from the C++ version are not directly
    /// translatable to C# and are provided as runtime operations instead.
    /// </remarks>
    public sealed class TypeList
    {
        private readonly Type[] _types;

        /// <summary>
        /// Initializes a new instance of the TypeList class.
        /// </summary>
        /// <param name="types">The types in the list.</param>
        public TypeList(params Type[] types)
        {
            _types = types ?? Array.Empty<Type>();
        }

        /// <summary>
        /// Gets the number of types in this list.
        /// </summary>
        public int Count => _types.Length;

        /// <summary>
        /// Gets the type at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the type to get.</param>
        /// <returns>The type at the specified index, or null if the index is out of range.</returns>
        public Type? Get(int index)
        {
            return index >= 0 && index < _types.Length ? _types[index] : null;
        }

        /// <summary>
        /// Gets the index of the first occurrence of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to locate in the list.</typeparam>
        /// <returns>
        /// The zero-based index of the first occurrence of the type within the list,
        /// or -1 if not found.
        /// </returns>
        public int IndexOf<T>() => IndexOf(typeof(T));

        /// <summary>
        /// Gets the index of the first occurrence of the specified type.
        /// </summary>
        /// <param name="type">The type to locate in the list.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of the type within the list,
        /// or -1 if not found.
        /// </returns>
        public int IndexOf(Type type)
        {
            for (int i = 0; i < _types.Length; i++)
            {
                if (_types[i] == type)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the list contains the specified type.
        /// </summary>
        /// <typeparam name="T">The type to locate in the list.</typeparam>
        /// <returns>true if the type is found in the list; otherwise, false.</returns>
        public bool Contains<T>() => Contains(typeof(T));

        /// <summary>
        /// Determines whether the list contains the specified type.
        /// </summary>
        /// <param name="type">The type to locate in the list.</param>
        /// <returns>true if the type is found in the list; otherwise, false.</returns>
        public bool Contains(Type type) => IndexOf(type) >= 0;

        /// <summary>
        /// Returns a new TypeList with the specified types appended.
        /// </summary>
        /// <param name="other">The types to append.</param>
        /// <returns>A new TypeList containing all types from this list and the specified types.</returns>
        public TypeList Append(TypeList other)
        {
            if (other == null || other._types.Length == 0)
                return this;

            var combined = new Type[_types.Length + other._types.Length];
            Array.Copy(_types, 0, combined, 0, _types.Length);
            Array.Copy(other._types, 0, combined, _types.Length, other._types.Length);
            return new TypeList(combined);
        }

        /// <summary>
        /// Returns a new TypeList with the specified type appended.
        /// </summary>
        /// <typeparam name="T">The type to append.</typeparam>
        /// <returns>A new TypeList containing all types from this list and the specified type.</returns>
        public TypeList Append<T>()
        {
            var combined = new Type[_types.Length + 1];
            Array.Copy(_types, combined, _types.Length);
            combined[_types.Length] = typeof(T);
            return new TypeList(combined);
        }

        /// <summary>
        /// Executes an action for each type in the list.
        /// </summary>
        /// <param name="action">The action to execute for each type.</param>
        public void ForEach(Action<Type> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var type in _types)
            {
                action(type);
            }
        }

        /// <summary>
        /// Gets all types in this list.
        /// </summary>
        /// <returns>An array containing all types in the list.</returns>
        public Type[] GetTypes()
        {
            var result = new Type[_types.Length];
            Array.Copy(_types, result, _types.Length);
            return result;
        }

        /// <summary>
        /// Returns a string representation of this TypeList.
        /// </summary>
        public override string ToString()
        {
            if (_types.Length == 0)
                return "TypeList<>";

            return $"TypeList<{string.Join(", ", _types.Select(t => t.Name))}>";
        }
    }

    /// <summary>
    /// Factory methods for creating TypeLists.
    /// </summary>
    public static class TypeListFactory
    {
        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <typeparam name="T1">First type.</typeparam>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create<T1>() =>
            new(typeof(T1));

        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <typeparam name="T1">First type.</typeparam>
        /// <typeparam name="T2">Second type.</typeparam>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create<T1, T2>() =>
            new(typeof(T1), typeof(T2));

        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <typeparam name="T1">First type.</typeparam>
        /// <typeparam name="T2">Second type.</typeparam>
        /// <typeparam name="T3">Third type.</typeparam>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create<T1, T2, T3>() =>
            new(typeof(T1), typeof(T2), typeof(T3));

        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <typeparam name="T1">First type.</typeparam>
        /// <typeparam name="T2">Second type.</typeparam>
        /// <typeparam name="T3">Third type.</typeparam>
        /// <typeparam name="T4">Fourth type.</typeparam>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create<T1, T2, T3, T4>() =>
            new(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <typeparam name="T1">First type.</typeparam>
        /// <typeparam name="T2">Second type.</typeparam>
        /// <typeparam name="T3">Third type.</typeparam>
        /// <typeparam name="T4">Fourth type.</typeparam>
        /// <typeparam name="T5">Fifth type.</typeparam>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create<T1, T2, T3, T4, T5>() =>
            new(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        /// <summary>
        /// Creates a TypeList from the specified types.
        /// </summary>
        /// <param name="types">The types to include in the list.</param>
        /// <returns>A new TypeList containing the specified types.</returns>
        public static TypeList Create(params Type[] types) => new(types);
    }
}
