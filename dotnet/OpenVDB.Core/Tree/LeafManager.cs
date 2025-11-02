// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// LeafManager.cs - C# port of LeafManager.h
//
// A LeafManager manages a linear array of pointers to a given tree's leaf nodes,
// as well as optional auxiliary buffers that can be swapped with the leaf nodes'
// voxel data buffers.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Manages a linear array of pointers to a tree's leaf nodes
    /// </summary>
    /// <typeparam name="TTree">The tree type</typeparam>
    /// <remarks>
    /// The leaf array is useful for multithreaded computations over leaf voxels
    /// in a tree with static topology but varying voxel values. The auxiliary
    /// buffers are convenient for temporal integration. Efficient methods are
    /// provided for multithreaded swapping and synching (copying the contents)
    /// of these buffers.
    /// 
    /// Note: Buffer index 0 denotes a leaf node's internal voxel data buffer.
    /// Any auxiliary buffers are indexed starting from one.
    /// </remarks>
    public class LeafManager<TTree>
        where TTree : TreeBase
    {
        private TTree _tree;
        private List<object> _leafNodes;
        private int _auxBuffersPerLeaf;
        private List<object>? _auxBuffers;

        /// <summary>
        /// The tree type
        /// </summary>
        public TTree Tree => _tree;

        /// <summary>
        /// Number of leaf nodes being managed
        /// </summary>
        public int LeafCount => _leafNodes.Count;

        /// <summary>
        /// Number of auxiliary buffers per leaf
        /// </summary>
        public int AuxBuffersPerLeaf => _auxBuffersPerLeaf;

        /// <summary>
        /// Constructor from a tree reference and an auxiliary buffer count
        /// </summary>
        /// <param name="tree">The tree whose leaf nodes to manage</param>
        /// <param name="auxBuffersPerLeaf">Number of auxiliary buffers per leaf (default 0)</param>
        /// <param name="serial">If true, build the leaf array serially (default false)</param>
        public LeafManager(TTree tree, int auxBuffersPerLeaf = 0, bool serial = false)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _leafNodes = new List<object>();
            _auxBuffersPerLeaf = auxBuffersPerLeaf;
            _auxBuffers = null;
            
            Rebuild(serial);
        }

        /// <summary>
        /// Rebuild the leaf array from the tree
        /// </summary>
        /// <param name="serial">If true, build serially instead of in parallel</param>
        public void Rebuild(bool serial = false)
        {
            _leafNodes.Clear();
            
            // TODO: Traverse tree and collect all leaf nodes
            // This requires the tree to have a leaf iteration mechanism
            
            // Allocate auxiliary buffers if needed
            if (_auxBuffersPerLeaf > 0)
            {
                int totalBuffers = _leafNodes.Count * _auxBuffersPerLeaf;
                _auxBuffers = new List<object>(totalBuffers);
                for (int i = 0; i < totalBuffers; i++)
                {
                    // TODO: Create actual buffer objects of appropriate type
                    _auxBuffers.Add(new object());
                }
            }
        }

        /// <summary>
        /// Get a leaf node by index
        /// </summary>
        /// <param name="index">The leaf index</param>
        /// <returns>The leaf node at that index</returns>
        public object GetLeaf(int index)
        {
            if (index < 0 || index >= _leafNodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _leafNodes[index];
        }

        /// <summary>
        /// Get an auxiliary buffer
        /// </summary>
        /// <param name="leafIndex">The leaf index</param>
        /// <param name="bufferIndex">The buffer index (0 = leaf's own buffer, 1+ = auxiliary)</param>
        /// <returns>The buffer</returns>
        public object GetBuffer(int leafIndex, int bufferIndex)
        {
            if (bufferIndex == 0)
            {
                // Return the leaf node's own buffer
                return GetLeaf(leafIndex);
            }
            else if (bufferIndex > 0 && bufferIndex <= _auxBuffersPerLeaf)
            {
                if (_auxBuffers == null)
                    throw new InvalidOperationException("No auxiliary buffers allocated");
                
                int auxIndex = leafIndex * _auxBuffersPerLeaf + (bufferIndex - 1);
                if (auxIndex >= _auxBuffers.Count)
                    throw new ArgumentOutOfRangeException(nameof(bufferIndex));
                
                return _auxBuffers[auxIndex];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(bufferIndex));
            }
        }

        /// <summary>
        /// Swap a leaf node's buffer with an auxiliary buffer
        /// </summary>
        /// <param name="leafIndex">The leaf index</param>
        /// <param name="auxBufferIndex">The auxiliary buffer index (must be >= 1)</param>
        public void SwapLeafBuffer(int leafIndex, int auxBufferIndex)
        {
            if (auxBufferIndex < 1 || auxBufferIndex > _auxBuffersPerLeaf)
                throw new ArgumentOutOfRangeException(nameof(auxBufferIndex));
            
            // TODO: Implement actual buffer swapping
            // This requires the leaf nodes to have a Swap method
        }

        /// <summary>
        /// Swap all leaf buffers with auxiliary buffers
        /// </summary>
        /// <param name="auxBufferIndex">The auxiliary buffer index (must be >= 1)</param>
        /// <param name="serial">If true, swap serially instead of in parallel</param>
        public void SwapAllLeafBuffers(int auxBufferIndex, bool serial = false)
        {
            if (auxBufferIndex < 1 || auxBufferIndex > _auxBuffersPerLeaf)
                throw new ArgumentOutOfRangeException(nameof(auxBufferIndex));
            
            if (serial)
            {
                for (int i = 0; i < _leafNodes.Count; i++)
                {
                    SwapLeafBuffer(i, auxBufferIndex);
                }
            }
            else
            {
                Parallel.For(0, _leafNodes.Count, i =>
                {
                    SwapLeafBuffer(i, auxBufferIndex);
                });
            }
        }

        /// <summary>
        /// Remove all auxiliary buffers
        /// </summary>
        public void RemoveAuxBuffers()
        {
            _auxBuffers = null;
            _auxBuffersPerLeaf = 0;
        }

        /// <summary>
        /// Add auxiliary buffers
        /// </summary>
        /// <param name="count">Number of auxiliary buffers to add per leaf</param>
        public void AddAuxBuffers(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            
            int newTotal = _auxBuffersPerLeaf + count;
            int newBufferCount = _leafNodes.Count * newTotal;
            
            if (_auxBuffers == null)
            {
                _auxBuffers = new List<object>(newBufferCount);
            }
            
            // Add new buffers
            for (int i = _auxBuffers.Count; i < newBufferCount; i++)
            {
                _auxBuffers.Add(new object());
            }
            
            _auxBuffersPerLeaf = newTotal;
        }

        /// <summary>
        /// Get a range for iterating over leaves
        /// </summary>
        /// <param name="grainSize">The grain size for parallel iteration</param>
        /// <returns>A leaf range</returns>
        public LeafRange GetLeafRange(int grainSize = 1)
        {
            return new LeafRange(0, _leafNodes.Count, this, grainSize);
        }

        /// <summary>
        /// Range for iterating over leaf nodes
        /// </summary>
        public class LeafRange
        {
            private readonly int _begin;
            private readonly int _end;
            private readonly int _grainSize;
            private readonly LeafManager<TTree> _manager;

            /// <summary>
            /// Constructor
            /// </summary>
            public LeafRange(int begin, int end, LeafManager<TTree> manager, int grainSize = 1)
            {
                _begin = begin;
                _end = end;
                _manager = manager;
                _grainSize = grainSize;
            }

            /// <summary>
            /// Beginning of the range
            /// </summary>
            public int Begin => _begin;

            /// <summary>
            /// End of the range
            /// </summary>
            public int End => _end;

            /// <summary>
            /// Size of the range
            /// </summary>
            public int Size => _end - _begin;

            /// <summary>
            /// Grain size for parallel iteration
            /// </summary>
            public int GrainSize => _grainSize;

            /// <summary>
            /// The leaf manager
            /// </summary>
            public LeafManager<TTree> Manager => _manager;

            /// <summary>
            /// Check if the range is empty
            /// </summary>
            public bool IsEmpty => _begin >= _end;

            /// <summary>
            /// Get an enumerator for iterating over leaf indices
            /// </summary>
            public IEnumerable<int> Indices()
            {
                for (int i = _begin; i < _end; i++)
                {
                    yield return i;
                }
            }

            /// <summary>
            /// Execute an action for each leaf in the range
            /// </summary>
            /// <param name="action">The action to execute</param>
            /// <param name="parallel">If true, execute in parallel</param>
            public void ForEach(Action<int> action, bool parallel = false)
            {
                if (parallel)
                {
                    Parallel.For(_begin, _end, action);
                }
                else
                {
                    for (int i = _begin; i < _end; i++)
                    {
                        action(i);
                    }
                }
            }
        }
    }
}
