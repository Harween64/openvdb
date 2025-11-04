// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// NodeManager.cs - C# port of NodeManager.h
//
// NodeManager produces linear arrays of all tree nodes allowing for
// efficient threading and bottom-up processing.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Manages linear arrays of all tree nodes for efficient threading
    /// </summary>
    /// <typeparam name="TTree">The tree type</typeparam>
    /// <remarks>
    /// A NodeManager can be constructed from a Tree or LeafManager.
    /// It produces linear arrays of all tree nodes to facilitate efficient
    /// threading and bottom-up processing.
    /// 
    /// This is a simplified C# implementation. The C++ version uses extensive
    /// template metaprogramming to handle multiple node levels efficiently.
    /// </remarks>
    public class NodeManager<TTree>
        where TTree : TreeBase
    {
        private TTree _tree;
        private List<object> _rootNodes;
        private List<object> _internalNodes;
        private List<object> _leafNodes;

        /// <summary>
        /// The tree being managed
        /// </summary>
        public TTree Tree => _tree;

        /// <summary>
        /// Number of root nodes (typically 1)
        /// </summary>
        public int RootNodeCount => _rootNodes.Count;

        /// <summary>
        /// Number of internal nodes at all levels
        /// </summary>
        public int InternalNodeCount => _internalNodes.Count;

        /// <summary>
        /// Number of leaf nodes
        /// </summary>
        public int LeafNodeCount => _leafNodes.Count;

        /// <summary>
        /// Total number of nodes at all levels
        /// </summary>
        public int TotalNodeCount => _rootNodes.Count + _internalNodes.Count + _leafNodes.Count;

        /// <summary>
        /// Constructor from a tree
        /// </summary>
        /// <param name="tree">The tree whose nodes to manage</param>
        /// <param name="serial">If true, build the arrays serially (default false)</param>
        public NodeManager(TTree tree, bool serial = false)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _rootNodes = new List<object>();
            _internalNodes = new List<object>();
            _leafNodes = new List<object>();
            
            Rebuild(serial);
        }

        /// <summary>
        /// Constructor from a LeafManager
        /// </summary>
        /// <param name="leafManager">The leaf manager</param>
        /// <param name="serial">If true, build the arrays serially (default false)</param>
        public NodeManager(LeafManager<TTree> leafManager, bool serial = false)
        {
            if (leafManager == null)
                throw new ArgumentNullException(nameof(leafManager));
            
            _tree = leafManager.Tree;
            _rootNodes = new List<object>();
            _internalNodes = new List<object>();
            _leafNodes = new List<object>();
            
            // Copy leaf nodes from the LeafManager
            for (int i = 0; i < leafManager.LeafCount; i++)
            {
                _leafNodes.Add(leafManager.GetLeaf(i));
            }
            
            // Build internal and root node arrays
            RebuildInternalNodes(serial);
        }

        /// <summary>
        /// Rebuild all node arrays from the tree
        /// </summary>
        /// <param name="serial">If true, build serially instead of in parallel</param>
        public void Rebuild(bool serial = false)
        {
            _rootNodes.Clear();
            _internalNodes.Clear();
            _leafNodes.Clear();
            
            // TODO: Traverse tree and collect all nodes
            // This requires the tree to have iteration mechanisms
            
            // For now, add a placeholder root node reference
            // Note: In a real implementation, we'd traverse the tree properly
            // TreeBase doesn't expose Root(), so this is commented out
            // var root = _tree.Root();
            // if (root != null)
            // {
            //     _rootNodes.Add(root);
            // }
        }

        /// <summary>
        /// Rebuild internal and root node arrays (leaf nodes already populated)
        /// </summary>
        /// <param name="serial">If true, build serially instead of in parallel</param>
        private void RebuildInternalNodes(bool serial = false)
        {
            _rootNodes.Clear();
            _internalNodes.Clear();
            
            // TODO: Traverse tree bottom-up from leaf nodes to build internal node arrays
        }

        /// <summary>
        /// Get a leaf node by index
        /// </summary>
        /// <param name="index">The leaf node index</param>
        /// <returns>The leaf node</returns>
        public object GetLeafNode(int index)
        {
            if (index < 0 || index >= _leafNodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _leafNodes[index];
        }

        /// <summary>
        /// Get an internal node by index
        /// </summary>
        /// <param name="index">The internal node index</param>
        /// <returns>The internal node</returns>
        public object GetInternalNode(int index)
        {
            if (index < 0 || index >= _internalNodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _internalNodes[index];
        }

        /// <summary>
        /// Get a root node by index
        /// </summary>
        /// <param name="index">The root node index (typically 0)</param>
        /// <returns>The root node</returns>
        public object GetRootNode(int index)
        {
            if (index < 0 || index >= _rootNodes.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _rootNodes[index];
        }

        /// <summary>
        /// Execute an operation on all leaf nodes
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="parallel">If true, execute in parallel</param>
        public void ForEachLeaf(Action<object> operation, bool parallel = false)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            
            if (parallel)
            {
                Parallel.ForEach(_leafNodes, operation);
            }
            else
            {
                foreach (var node in _leafNodes)
                {
                    operation(node);
                }
            }
        }

        /// <summary>
        /// Execute an operation on all internal nodes
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="parallel">If true, execute in parallel</param>
        public void ForEachInternal(Action<object> operation, bool parallel = false)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            
            if (parallel)
            {
                Parallel.ForEach(_internalNodes, operation);
            }
            else
            {
                foreach (var node in _internalNodes)
                {
                    operation(node);
                }
            }
        }

        /// <summary>
        /// Execute an operation on all nodes at all levels
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="parallel">If true, execute in parallel</param>
        public void ForEachNode(Action<object> operation, bool parallel = false)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            
            // Process in bottom-up order: leaves, internals, root
            ForEachLeaf(operation, parallel);
            ForEachInternal(operation, parallel);
            
            if (parallel)
            {
                Parallel.ForEach(_rootNodes, operation);
            }
            else
            {
                foreach (var node in _rootNodes)
                {
                    operation(node);
                }
            }
        }

        /// <summary>
        /// Get a range for iterating over leaf nodes
        /// </summary>
        /// <param name="grainSize">The grain size for parallel iteration</param>
        /// <returns>A node range for leaf nodes</returns>
        public NodeRange GetLeafRange(int grainSize = 1)
        {
            return new NodeRange(0, _leafNodes.Count, _leafNodes, grainSize);
        }

        /// <summary>
        /// Get a range for iterating over internal nodes
        /// </summary>
        /// <param name="grainSize">The grain size for parallel iteration</param>
        /// <returns>A node range for internal nodes</returns>
        public NodeRange GetInternalRange(int grainSize = 1)
        {
            return new NodeRange(0, _internalNodes.Count, _internalNodes, grainSize);
        }

        /// <summary>
        /// Range for iterating over nodes
        /// </summary>
        public class NodeRange
        {
            private readonly int _begin;
            private readonly int _end;
            private readonly int _grainSize;
            private readonly List<object> _nodes;

            /// <summary>
            /// Constructor
            /// </summary>
            public NodeRange(int begin, int end, List<object> nodes, int grainSize = 1)
            {
                _begin = begin;
                _end = end;
                _nodes = nodes;
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
            /// Check if the range is empty
            /// </summary>
            public bool IsEmpty => _begin >= _end;

            /// <summary>
            /// Get a node by index within the range
            /// </summary>
            public object GetNode(int index)
            {
                if (index < _begin || index >= _end)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _nodes[index];
            }

            /// <summary>
            /// Get an enumerator for iterating over node indices
            /// </summary>
            public IEnumerable<int> Indices()
            {
                for (int i = _begin; i < _end; i++)
                {
                    yield return i;
                }
            }

            /// <summary>
            /// Execute an action for each node in the range
            /// </summary>
            /// <param name="action">The action to execute</param>
            /// <param name="parallel">If true, execute in parallel</param>
            public void ForEach(Action<object> action, bool parallel = false)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                
                if (parallel)
                {
                    Parallel.For(_begin, _end, i => action(_nodes[i]));
                }
                else
                {
                    for (int i = _begin; i < _end; i++)
                    {
                        action(_nodes[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// DynamicNodeManager provides dynamic iteration over tree nodes
    /// </summary>
    /// <typeparam name="TTree">The tree type</typeparam>
    /// <remarks>
    /// Unlike NodeManager which creates cached node arrays, DynamicNodeManager
    /// traverses the tree dynamically. This is useful when the tree structure
    /// might change during iteration or when memory overhead of caching is undesirable.
    /// </remarks>
    public class DynamicNodeManager<TTree> where TTree : TreeBase
    {
        private readonly TTree _tree;
        private readonly int _maxDepth;

        /// <summary>
        /// The tree being managed
        /// </summary>
        public TTree Tree => _tree;

        /// <summary>
        /// Constructor from a tree
        /// </summary>
        /// <param name="tree">The tree to manage</param>
        /// <param name="maxDepth">Maximum depth to traverse (default is full tree depth)</param>
        public DynamicNodeManager(TTree tree, int maxDepth = -1)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _maxDepth = maxDepth < 0 ? tree.Depth : maxDepth;
        }

        /// <summary>
        /// Apply an operation to all nodes in a top-down traversal
        /// </summary>
        /// <typeparam name="TOperator">The operator type</typeparam>
        /// <param name="op">The operator to apply</param>
        /// <param name="threaded">Whether to use multi-threading</param>
        public void ForeachTopDown<TOperator>(TOperator op, bool threaded = true)
        {
            // TODO: Implement top-down traversal with operator
            // This requires full tree structure implementation
        }

        /// <summary>
        /// Apply an operation to all nodes in a bottom-up traversal
        /// </summary>
        /// <typeparam name="TOperator">The operator type</typeparam>
        /// <param name="op">The operator to apply</param>
        /// <param name="threaded">Whether to use multi-threading</param>
        public void ForeachBottomUp<TOperator>(TOperator op, bool threaded = true)
        {
            // TODO: Implement bottom-up traversal with operator
            // This requires full tree structure implementation
        }

        /// <summary>
        /// Reduce operation in a top-down traversal
        /// </summary>
        /// <typeparam name="TOperator">The operator type</typeparam>
        /// <param name="op">The operator to apply and reduce</param>
        /// <param name="threaded">Whether to use multi-threading</param>
        public void ReduceTopDown<TOperator>(TOperator op, bool threaded = true)
        {
            // TODO: Implement top-down reduction
            // This requires full tree structure implementation and parallel reduction support
        }

        /// <summary>
        /// Reduce operation in a bottom-up traversal
        /// </summary>
        /// <typeparam name="TOperator">The operator type</typeparam>
        /// <param name="op">The operator to apply and reduce</param>
        /// <param name="threaded">Whether to use multi-threading</param>
        public void ReduceBottomUp<TOperator>(TOperator op, bool threaded = true)
        {
            // TODO: Implement bottom-up reduction
            // This requires full tree structure implementation and parallel reduction support
        }
    }
}
