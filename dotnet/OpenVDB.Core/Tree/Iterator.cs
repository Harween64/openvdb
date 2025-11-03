// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Iterator.cs - C# port of Iterator.h
//
// Base classes for iterators over internal and leaf nodes

using System;
using System.Runtime.CompilerServices;
using OpenVDB.Math;
using OpenVDB.Utils;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Base class for iterators over internal and leaf nodes
    /// </summary>
    /// <typeparam name="TMaskIter">Mask iterator type</typeparam>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <remarks>
    /// This class is typically not instantiated directly, since it doesn't provide methods
    /// to dereference the iterator. Those methods (operator*, setValue(), etc.) are
    /// implemented in the sparse and dense iterator subclasses.
    /// </remarks>
    public class IteratorBase<TMaskIter, TNode>
        where TMaskIter : class
        where TNode : class
    {
        protected TNode? _parentNode;
        protected TMaskIter? _maskIter;

        /// <summary>
        /// Default constructor
        /// </summary>
        public IteratorBase()
        {
            _parentNode = null;
            _maskIter = null;
        }

        /// <summary>
        /// Constructor with mask iterator and parent node
        /// </summary>
        public IteratorBase(TMaskIter maskIter, TNode parent)
        {
            _parentNode = parent;
            _maskIter = maskIter;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public IteratorBase(IteratorBase<TMaskIter, TNode> other)
        {
            _parentNode = other._parentNode;
            _maskIter = other._maskIter;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public bool Equals(IteratorBase<TMaskIter, TNode> other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return ReferenceEquals(_parentNode, other._parentNode) && 
                   ReferenceEquals(_maskIter, other._maskIter);
        }

        /// <summary>
        /// Return a pointer to the node (if any) over which this iterator is iterating
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TNode? GetParentNode() => _parentNode;

        /// <summary>
        /// Return a reference to the node over which this iterator is iterating
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no parent node</exception>
        public TNode Parent()
        {
            if (_parentNode == null)
                throw new InvalidOperationException("Iterator references a null node");
            return _parentNode;
        }

        /// <summary>
        /// Return this iterator's position as an index into the parent node's table
        /// </summary>
        /// <remarks>
        /// This method should be overridden by specific iterator implementations
        /// to delegate to the mask iterator's Offset() method
        /// </remarks>
        public virtual int Offset()
        {
            // Default implementation - should be overridden
            return 0;
        }

        /// <summary>
        /// Identical to Offset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Pos() => Offset();

        /// <summary>
        /// Return true if this iterator is not yet exhausted
        /// </summary>
        /// <remarks>
        /// This method should be overridden by specific iterator implementations
        /// to delegate to the mask iterator's Test() method
        /// </remarks>
        public virtual bool Test()
        {
            // Default implementation - should be overridden
            return false;
        }

        /// <summary>
        /// Advance to the next item in the parent node's table
        /// </summary>
        /// <remarks>
        /// This method should be overridden by specific iterator implementations
        /// to delegate to the mask iterator's Next() method
        /// </remarks>
        public virtual bool Next()
        {
            // Default implementation - should be overridden
            return false;
        }

        /// <summary>
        /// Advance to the next item in the parent node's table
        /// </summary>
        /// <remarks>
        /// This method should be overridden by specific iterator implementations
        /// to delegate to the mask iterator's Increment() method
        /// </remarks>
        public virtual void Increment()
        {
            // Default implementation - should be overridden
        }

        /// <summary>
        /// Advance n items in the parent node's table
        /// </summary>
        /// <remarks>
        /// This method should be overridden by specific iterator implementations
        /// to delegate to the mask iterator's Increment(n) method
        /// </remarks>
        public virtual void Increment(int n)
        {
            // Default implementation - should be overridden
        }

        /// <summary>
        /// Get the coordinates of the item to which this iterator is pointing
        /// </summary>
        /// <remarks>
        /// This is a placeholder. Actual implementation requires the parent node
        /// to have an OffsetToGlobalCoord method.
        /// </remarks>
        public virtual Coord GetCoord()
        {
            // Default implementation - should be overridden
            return new Coord(0, 0, 0);
        }
    }

    /// <summary>
    /// Base class for sparse iterators over internal and leaf nodes
    /// </summary>
    /// <typeparam name="TMaskIter">Mask iterator type</typeparam>
    /// <typeparam name="TIter">Iterator implementation type (CRTP pattern)</typeparam>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TItem">Item value type</typeparam>
    public class SparseIteratorBase<TMaskIter, TIter, TNode, TItem> : IteratorBase<TMaskIter, TNode>
        where TMaskIter : class
        where TIter : SparseIteratorBase<TMaskIter, TIter, TNode, TItem>
        where TNode : class
    {
        /// <summary>
        /// Indicates this is a sparse iterator
        /// </summary>
        public const bool IsSparseIterator = true;

        /// <summary>
        /// Indicates this is not a dense iterator
        /// </summary>
        public const bool IsDenseIterator = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SparseIteratorBase() : base()
        {
        }

        /// <summary>
        /// Constructor with mask iterator and parent node
        /// </summary>
        public SparseIteratorBase(TMaskIter maskIter, TNode parent) : base(maskIter, parent)
        {
        }

        /// <summary>
        /// Return the item to which this iterator is pointing
        /// </summary>
        /// <remarks>
        /// This method uses static polymorphism via the CRTP pattern.
        /// Subclasses must implement GetItem.
        /// </remarks>
        public virtual TItem GetValue()
        {
            // This should be overridden by derived classes
            throw new NotImplementedException("GetValue() must be implemented by derived class");
        }

        /// <summary>
        /// Set the value of the item to which this iterator is pointing
        /// </summary>
        /// <remarks>
        /// Not valid for const iterators.
        /// This method uses static polymorphism via the CRTP pattern.
        /// Subclasses must implement SetItem.
        /// </remarks>
        public virtual void SetValue(TItem value)
        {
            // This should be overridden by derived classes
            throw new NotImplementedException("SetValue() must be implemented by derived class");
        }
    }

    /// <summary>
    /// Base class for dense iterators over internal and leaf nodes
    /// </summary>
    /// <typeparam name="TMaskIter">Mask iterator type</typeparam>
    /// <typeparam name="TIter">Iterator implementation type (CRTP pattern)</typeparam>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TItem">Item value type</typeparam>
    public class DenseIteratorBase<TMaskIter, TIter, TNode, TItem> : IteratorBase<TMaskIter, TNode>
        where TMaskIter : class
        where TIter : DenseIteratorBase<TMaskIter, TIter, TNode, TItem>
        where TNode : class
    {
        /// <summary>
        /// Indicates this is not a sparse iterator
        /// </summary>
        public const bool IsSparseIterator = false;

        /// <summary>
        /// Indicates this is a dense iterator
        /// </summary>
        public const bool IsDenseIterator = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DenseIteratorBase() : base()
        {
        }

        /// <summary>
        /// Constructor with mask iterator and parent node
        /// </summary>
        public DenseIteratorBase(TMaskIter maskIter, TNode parent) : base(maskIter, parent)
        {
        }

        /// <summary>
        /// Return the item to which this iterator is pointing
        /// </summary>
        /// <remarks>
        /// This method uses static polymorphism via the CRTP pattern.
        /// Subclasses must implement GetItem.
        /// </remarks>
        public virtual TItem GetValue()
        {
            // This should be overridden by derived classes
            throw new NotImplementedException("GetValue() must be implemented by derived class");
        }

        /// <summary>
        /// Set the value of the item to which this iterator is pointing
        /// </summary>
        /// <remarks>
        /// Not valid for const iterators.
        /// This method uses static polymorphism via the CRTP pattern.
        /// Subclasses must implement SetItem.
        /// </remarks>
        public virtual void SetValue(TItem value)
        {
            // This should be overridden by derived classes
            throw new NotImplementedException("SetValue() must be implemented by derived class");
        }
    }
}
