// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// NodeUnion.cs - C# port of NodeUnion.h
//
// Controls access to either the child node pointer or the value for a
// particular element of a root or internal node.

using System;
using System.Runtime.InteropServices;

namespace OpenVDB.Tree
{
    /// <summary>
    /// NodeUnion controls access to either a child node pointer or a value.
    /// For space efficiency, these are stored in a single field when the value type
    /// is a value type (struct).
    /// </summary>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TChild">The child node type</typeparam>
    /// <remarks>
    /// In C++, this uses a union for trivially copyable types. In C#, we use
    /// a single object reference field and type checking to determine which
    /// is currently stored.
    /// </remarks>
    public class NodeUnion<TValue, TChild> 
        where TValue : struct
        where TChild : class
    {
        private TChild? _child;
        private TValue _value;
        private bool _isChild;
        
        /// <summary>
        /// Default constructor - initializes with null child
        /// </summary>
        public NodeUnion()
        {
            _child = null;
            _value = default(TValue);
            _isChild = false;
        }
        
        /// <summary>
        /// Get the child node pointer
        /// </summary>
        /// <returns>The child node or null</returns>
        public TChild? GetChild() => _child;
        
        /// <summary>
        /// Set the child node pointer
        /// </summary>
        /// <param name="child">The child node to set</param>
        public void SetChild(TChild? child)
        {
            _child = child;
            _isChild = true;
        }
        
        /// <summary>
        /// Get the value
        /// </summary>
        /// <returns>The current value</returns>
        public ref TValue GetValue() => ref _value;
        
        /// <summary>
        /// Set the value
        /// </summary>
        /// <param name="val">The value to set</param>
        public void SetValue(TValue val)
        {
            _value = val;
            _isChild = false;
        }
        
        /// <summary>
        /// Check if this union currently holds a child node
        /// </summary>
        /// <returns>True if holding a child node</returns>
        public bool IsChild() => _isChild;
    }
}
