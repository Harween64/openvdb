# Plan de Portage: Fonctionnalités Topology

**Date**: 2025-11-09  
**Contexte**: Portage C++ → C# .NET 9  
**Basé sur**: RAPPORT_TOPOLOGY.md

---

## 1. Vue d'ensemble du plan

### 1.1 Objectifs

Porter les **35-40 méthodes topology** de C++ vers C# pour débloquer:
- Lot 6 (IO) - chargement différé de fichiers VDB
- Lot 7 (Tools) - opérations CSG et masquage
- Lot 8 (Points) - fonctionnalités avancées

### 1.2 Stratégie

**Approche bottom-up**: Commencer par les nœuds feuilles, remonter vers Grid.

**Phases**:
1. Compléter les structures de nœuds
2. Implémenter comparaison topology (`hasSameTopology`)
3. Implémenter constructeurs topology copy
4. Implémenter opérations booléennes
5. Implémenter I/O topology

### 1.3 Métriques de suivi

- [ ] **40 méthodes** topology à porter
- [ ] **5 fichiers** principaux à modifier
- [ ] **Tests unitaires** pour chaque méthode
- [ ] **Durée estimée**: 4-7 semaines

---

## 2. Audit détaillé: Ce qui a été porté

### 2.1 Structures de base ✅ PORTÉ

| Fichier C++ | Fichier C# | Status | Méthodes de base |
|-------------|------------|--------|------------------|
| `Math/Coord.h` | `Math/Coord.cs` | ✅ COMPLET | Coordonnées 3D |
| `Math/BBox.h` | `Math/BBox.cs` | ✅ COMPLET | Boîtes englobantes |
| `Metadata/Metadata.h` | `Metadata/Metadata.cs` | ✅ COMPLET | Métadonnées |

### 2.2 Structures de nœuds ⚠️ PARTIEL

| Fichier C++ | Fichier C# | Lignes | Méthodes portées | Méthodes topology manquantes |
|-------------|------------|--------|------------------|----------------------------|
| `tree/LeafNode.h` | `Tree/LeafNode.cs` | ~150 | getValue, setValue basiques | topologyUnion, topologyIntersection, topologyDifference, hasSameTopology, readTopology, writeTopology, constructeurs TopologyCopy |
| `tree/InternalNode.h` | `Tree/InternalNode.cs` | ~150 | getValue, setValue basiques | (idem) |
| `tree/RootNode.h` | `Tree/RootNode.cs` | 159 | GetValue, SetValue, Clear | (idem) |
| `tree/Tree.h` | `Tree/Tree.cs` | ~120 | Type, ValueType, Copy | (idem) |
| `Grid.h` | `Grid/Grid.cs` | 206 | Metadata uniquement | (idem) |

### 2.3 Structures utilitaires ❌ NON PORTÉ

| Fonctionnalité | Status | Nécessaire pour |
|----------------|--------|-----------------|
| **Node iterators** | ❌ NON PORTÉ | Traversal pour topology operations |
| **Value iterators** | ❌ NON PORTÉ | Itération sur voxels actifs |
| **ValueAccessor complet** | ⚠️ STUB | Accès optimisé (pas critique pour topology) |
| **I/O Stream complet** | ⚠️ STUB | readTopology/writeTopology |

---

## 3. Plan de portage par phase

### PHASE 1: Compléter les structures de nœuds (2 semaines)

#### 3.1.1 LeafNode.cs

**Fichier**: `dotnet/OpenVDB.Core/Tree/LeafNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/LeafNode.h`

**Méthodes à ajouter**:

| Méthode | Signature C++ (ligne ~) | Signature C# proposée | Priorité |
|---------|--------------------------|----------------------|----------|
| `fill` | `void fill(const ValueType& value, bool active = true)` (ligne ~400) | `void Fill(TValue value, bool active = true)` | Haute |
| `isConstant` | `bool isConstant(ValueType& constValue, bool& state, Real tolerance = 0) const` | `bool IsConstant(out TValue constValue, out bool state, double tolerance = 0.0)` | Haute |
| `memUsage` | `Index64 memUsage() const` | `long MemUsage()` | Moyenne |
| `getOrigin` | `Coord getOrigin() const` | `Coord GetOrigin()` | Haute |
| `isEmpty` | `bool isEmpty() const` | `bool IsEmpty()` | Haute |
| `onVoxelCount` | `Index64 onVoxelCount() const` | `long OnVoxelCount()` | Haute |
| `offVoxelCount` | `Index64 offVoxelCount() const` | `long OffVoxelCount()` | Moyenne |

**Durée estimée**: 3 jours

#### 3.1.2 InternalNode.cs

**Fichier**: `dotnet/OpenVDB.Core/Tree/InternalNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/InternalNode.h`

**Méthodes à ajouter** (similaires à LeafNode):
- `Fill`
- `IsConstant`
- `MemUsage`
- `GetOrigin`
- `IsEmpty`
- Gestion des tiles (active/inactive tiles)
- Child management (getChild, setChild, etc.)

**Durée estimée**: 4 jours

#### 3.1.3 RootNode.cs

**Fichier**: `dotnet/OpenVDB.Core/Tree/RootNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/RootNode.h`

**Méthodes à ajouter**:
- `GetChild` / `SetChild` (accès aux enfants)
- `IsTile` / `IsTileOn` / `IsTileOff`
- `AddTile` / `AddChild`
- `StealChild` (pour move semantics)
- Itérateurs de base sur la table de hash

**Durée estimée**: 3 jours

#### 3.1.4 Tree.cs

**Fichier**: `dotnet/OpenVDB.Core/Tree/Tree.cs`  
**Référence C++**: `openvdb/openvdb/tree/Tree.h`

**Méthodes à ajouter**:
- `Root()` property (accès au nœud racine)
- `IsEmpty()`
- `MemUsage()`
- `GetValue(Coord)` / `SetValue(Coord, TValue)`
- `Fill(BBox, TValue, bool active)`

**Durée estimée**: 2 jours

**Total Phase 1**: **12 jours** (2.4 semaines)

---

### PHASE 2: Implémentation hasSameTopology (1 semaine)

#### 3.2.1 LeafNode.hasSameTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/LeafNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/LeafNode.h` (ligne ~1200-1250)

```csharp
/// <summary>
/// Return true if the given node (which may have a different ValueType) has 
/// the same active value topology as this node.
/// </summary>
public bool HasSameTopology<TOtherValue>(LeafNode<TOtherValue, Log2Dim> other) 
    where TOtherValue : struct
{
    // Compare active value masks
    return _valueMask.Equals(other.ValueMask);
}
```

**Durée**: 0.5 jour (simple, compare uniquement les masques)

#### 3.2.2 InternalNode.hasSameTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/InternalNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/InternalNode.h` (ligne ~1800-1850)

```csharp
/// <summary>
/// Return true if the given node has the same active value topology as this node.
/// </summary>
public bool HasSameTopology<TOtherChild, TOtherValue>(
    InternalNode<TOtherChild, TOtherValue, Log2Dim> other)
    where TOtherChild : class
    where TOtherValue : struct
{
    // 1. Compare child masks
    if (!_childMask.Equals(other.ChildMask)) return false;
    
    // 2. Compare value masks (for tiles)
    if (!_valueMask.Equals(other.ValueMask)) return false;
    
    // 3. Recursively compare children
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (_childMask.IsOn(i)) {
            if (!GetChild(i).HasSameTopology(other.GetChild(i))) {
                return false;
            }
        }
    }
    
    return true;
}
```

**Durée**: 1 jour (récursif, plus complexe)

#### 3.2.3 RootNode.hasSameTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/RootNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/RootNode.h` (ligne ~1401-1435)

```csharp
/// <summary>
/// Return true if the given tree has the same active value topology as this tree.
/// </summary>
public bool HasSameTopology<TOtherChild, TOtherValue>(
    RootNode<TOtherChild, TOtherValue> other)
    where TOtherChild : class
    where TOtherValue : struct
{
    // 1. Check background values activity
    // (Background values themselves don't matter, only if they're active)
    
    // 2. Compare table sizes
    if (_table.Count != other.TableSize) return false;
    
    // 3. Iterate over this root's table
    foreach (var entry in _table) {
        var coord = entry.Key;
        var thisNode = entry.Value;
        
        // Check if other has same coordinate
        if (!other.HasKey(coord)) return false;
        
        var otherNode = other.GetNode(coord);
        
        // Both must be tiles or both must be children
        if (thisNode.IsTile != otherNode.IsTile) return false;
        
        // If tiles, compare activity
        if (thisNode.IsTile) {
            if (thisNode.IsActive != otherNode.IsActive) return false;
        } else {
            // If children, recursively compare
            if (!thisNode.Child.HasSameTopology(otherNode.Child)) {
                return false;
            }
        }
    }
    
    return true;
}
```

**Durée**: 1.5 jours

#### 3.2.4 Tree.hasSameTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/Tree.cs`  
**Référence C++**: `openvdb/openvdb/tree/Tree.h` (ligne ~1230-1240)

```csharp
/// <summary>
/// Return true if the given tree has the same active value topology as this tree,
/// whether or not it has the same ValueType.
/// </summary>
public bool HasSameTopology<TOtherRoot>(Tree<TOtherRoot> other)
    where TOtherRoot : class
{
    return _root != null && other.Root() != null && 
           _root.HasSameTopology(other.Root());
}
```

**Durée**: 0.5 jour

#### 3.2.5 Grid.hasSameTopology (optionnel)

**Note**: Pas présent dans C++ Grid.h, mais utile pour API cohérente.

```csharp
public bool HasSameTopology<TOtherTree>(Grid<TOtherTree> other)
    where TOtherTree : class
{
    return _tree.HasSameTopology(other.Tree);
}
```

**Durée**: 0.5 jour

**Total Phase 2**: **4 jours** (0.8 semaine)

---

### PHASE 3: Topology copy constructors (1.5 semaines)

#### 3.3.1 Concept TopologyCopy

En C++, `TopologyCopy` est un tag struct vide:
```cpp
struct TopologyCopy {};
```

En C#, on peut utiliser un enum ou une classe marker:

```csharp
namespace OpenVDB.Tree
{
    /// <summary>
    /// Marker type for topology copy constructors
    /// </summary>
    public enum CopyMode
    {
        DeepCopy,      // Copie complète (valeurs + structure)
        TopologyCopy   // Copie uniquement structure
    }
}
```

**Durée**: 0.5 jour

#### 3.3.2 LeafNode topology copy constructor

**Référence C++**: `openvdb/openvdb/tree/LeafNode.h` (ligne ~250-280)

```csharp
/// <summary>
/// Topology copy constructor from a leaf node of a different value type.
/// </summary>
public LeafNode(LeafNode<TOtherValue, Log2Dim> other, 
                TValue background, 
                CopyMode mode = CopyMode.TopologyCopy)
    where TOtherValue : struct
{
    if (mode != CopyMode.TopologyCopy) {
        throw new ArgumentException("Only TopologyCopy mode supported");
    }
    
    _origin = other.Origin;
    _valueMask = new NodeMask(other.ValueMask); // Copy activity mask
    
    // Initialize all values to background
    _buffer = new TValue[SIZE];
    for (int i = 0; i < SIZE; ++i) {
        _buffer[i] = background;
    }
}

/// <summary>
/// Topology copy constructor with separate inactive/active values.
/// </summary>
public LeafNode(LeafNode<TOtherValue, Log2Dim> other,
                TValue inactiveValue,
                TValue activeValue,
                CopyMode mode = CopyMode.TopologyCopy)
    where TOtherValue : struct
{
    if (mode != CopyMode.TopologyCopy) {
        throw new ArgumentException("Only TopologyCopy mode supported");
    }
    
    _origin = other.Origin;
    _valueMask = new NodeMask(other.ValueMask);
    
    _buffer = new TValue[SIZE];
    for (int i = 0; i < SIZE; ++i) {
        _buffer[i] = _valueMask.IsOn(i) ? activeValue : inactiveValue;
    }
}
```

**Durée**: 1 jour

#### 3.3.3 InternalNode topology copy constructor

**Référence C++**: `openvdb/openvdb/tree/InternalNode.h` (ligne ~180-220)

```csharp
/// <summary>
/// Topology copy constructor from an internal node of different type.
/// </summary>
public InternalNode(InternalNode<TOtherChild, TOtherValue, Log2Dim> other,
                    TValue background,
                    CopyMode mode = CopyMode.TopologyCopy)
    where TOtherChild : class
    where TOtherValue : struct
{
    if (mode != CopyMode.TopologyCopy) {
        throw new ArgumentException("Only TopologyCopy mode supported");
    }
    
    _origin = other.Origin;
    _childMask = new NodeMask(other.ChildMask);
    _valueMask = new NodeMask(other.ValueMask);
    
    // Initialize unions
    _table = new UnionType[NUM_VALUES];
    
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (_childMask.IsOn(i)) {
            // Recursively create child with topology copy
            _table[i] = new UnionType {
                Child = new TChild(other.GetChild(i), background, mode)
            };
        } else {
            // Create tile with background value
            _table[i] = new UnionType {
                Tile = new Tile { Value = background, Active = _valueMask.IsOn(i) }
            };
        }
    }
}
```

**Durée**: 2 jours (plus complexe, récursif)

#### 3.3.4 RootNode topology copy constructor

**Référence C++**: `openvdb/openvdb/tree/RootNode.h` (ligne ~1092-1145)

```csharp
/// <summary>
/// Topology copy constructor from a root node of different type.
/// </summary>
public RootNode(RootNode<TOtherChild, TOtherValue> other,
                TValue background,
                CopyMode mode = CopyMode.TopologyCopy)
    where TOtherChild : class
    where TOtherValue : struct
{
    if (mode != CopyMode.TopologyCopy) {
        throw new ArgumentException("Only TopologyCopy mode supported");
    }
    
    _background = background;
    _table = new Dictionary<Coord, NodeUnion<TValue, TChild>>();
    
    // Iterate over other's table
    foreach (var entry in other.Table) {
        var coord = entry.Key;
        var otherNode = entry.Value;
        
        if (otherNode.IsTile) {
            // Copy tile with new background value
            _table[coord] = new NodeUnion<TValue, TChild> {
                Value = background,
                IsActive = otherNode.IsActive
            };
        } else {
            // Recursively create child with topology copy
            _table[coord] = new NodeUnion<TValue, TChild> {
                Child = new TChild(otherNode.Child, background, mode)
            };
        }
    }
}
```

**Durée**: 2 jours

#### 3.3.5 Tree topology copy constructor

**Référence C++**: `openvdb/openvdb/tree/Tree.h` (ligne ~258-280)

```csharp
/// <summary>
/// Topology copy constructor from a tree of different type.
/// </summary>
public Tree(Tree<TOtherRoot> other,
            TValue background,
            CopyMode mode = CopyMode.TopologyCopy)
    where TOtherRoot : class
{
    if (mode != CopyMode.TopologyCopy) {
        throw new ArgumentException("Only TopologyCopy mode supported");
    }
    
    if (other.Root() != null) {
        _root = new TRoot(other.Root(), background, mode);
    } else {
        _root = new TRoot(background);
    }
}
```

**Durée**: 0.5 jour

**Total Phase 3**: **6 jours** (1.2 semaines)

---

### PHASE 4: Opérations booléennes topology (2 semaines)

#### 3.4.1 LeafNode topology operations

**Fichier**: `dotnet/OpenVDB.Core/Tree/LeafNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/LeafNode.h` (lignes ~1800-1900)

##### topologyUnion

```csharp
/// <summary>
/// Union this node's active values with the other node's active values.
/// The value type of the other node can be different.
/// </summary>
public void TopologyUnion<TOtherValue>(LeafNode<TOtherValue, Log2Dim> other)
    where TOtherValue : struct
{
    // Union of active value masks
    _valueMask |= other.ValueMask;
}
```

##### topologyIntersection

```csharp
/// <summary>
/// Intersect this node's active values with the other node's active values.
/// </summary>
public void TopologyIntersection<TOtherValue>(LeafNode<TOtherValue, Log2Dim> other)
    where TOtherValue : struct
{
    // Intersection of active value masks
    _valueMask &= other.ValueMask;
}
```

##### topologyDifference

```csharp
/// <summary>
/// Difference this node's active values with the other node's active values.
/// </summary>
public void TopologyDifference<TOtherValue>(LeafNode<TOtherValue, Log2Dim> other)
    where TOtherValue : struct
{
    // Difference of active value masks
    _valueMask -= other.ValueMask; // Assumes NodeMask has -= operator
}
```

**Durée**: 1 jour (simple, opérations sur masques)

#### 3.4.2 InternalNode topology operations

**Fichier**: `dotnet/OpenVDB.Core/Tree/InternalNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/InternalNode.h` (lignes ~2200-2400)

**Complexité**: Moyenne-Haute (gestion tiles + children récursifs)

##### topologyUnion

```csharp
/// <summary>
/// Union this node's set of active values with the other node's active values.
/// </summary>
public void TopologyUnion<TOtherChild, TOtherValue>(
    InternalNode<TOtherChild, TOtherValue, Log2Dim> other,
    bool preserveTiles = false)
    where TOtherChild : class
    where TOtherValue : struct
{
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (other.ChildMask.IsOn(i)) {
            // Other has a child at this position
            if (_childMask.IsOn(i)) {
                // Both have children - recurse
                GetChild(i).TopologyUnion(other.GetChild(i), preserveTiles);
            } else if (!preserveTiles || !_valueMask.IsOn(i)) {
                // Replace tile with child (topology copy from other)
                SetChild(i, new TChild(other.GetChild(i), GetValue(i), CopyMode.TopologyCopy));
            }
            // else: preserveTiles is true and this has active tile - keep it
        } else {
            // Other has a tile
            if (other.ValueMask.IsOn(i)) {
                // Other has active tile
                if (!_childMask.IsOn(i)) {
                    // This also has tile - union activity
                    _valueMask.SetOn(i);
                } else {
                    // This has child, other has active tile
                    // Mark all voxels in child as active
                    GetChild(i).Fill(_background, true);
                }
            }
            // else: Other has inactive tile - no change needed
        }
    }
}
```

##### topologyIntersection

```csharp
/// <summary>
/// Intersect this node's set of active values with the other node's active values.
/// </summary>
public void TopologyIntersection<TOtherChild, TOtherValue>(
    InternalNode<TOtherChild, TOtherValue, Log2Dim> other,
    TValue background)
    where TOtherChild : class
    where TOtherValue : struct
{
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (other.ChildMask.IsOn(i)) {
            // Other has a child
            if (_childMask.IsOn(i)) {
                // Both have children - recurse
                GetChild(i).TopologyIntersection(other.GetChild(i), background);
            } else if (_valueMask.IsOn(i)) {
                // This has active tile, other has child
                // Replace tile with child (topology copy)
                SetChild(i, new TChild(other.GetChild(i), GetValue(i), CopyMode.TopologyCopy));
            } else {
                // This has inactive tile - intersection is empty (delete child if any)
                // Already a tile, just ensure it's inactive
                _valueMask.SetOff(i);
            }
        } else {
            // Other has a tile
            if (!other.ValueMask.IsOn(i)) {
                // Other has inactive tile - intersection is empty
                if (_childMask.IsOn(i)) {
                    // Delete child
                    DeleteChild(i, background);
                } else {
                    // Mark tile as inactive
                    _valueMask.SetOff(i);
                }
            } else {
                // Other has active tile - keep this node's state
                // No change needed
            }
        }
    }
}
```

##### topologyDifference

```csharp
/// <summary>
/// Difference this node's set of active values with the other node's active values.
/// </summary>
public void TopologyDifference<TOtherChild, TOtherValue>(
    InternalNode<TOtherChild, TOtherValue, Log2Dim> other,
    TValue background)
    where TOtherChild : class
    where TOtherValue : struct
{
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (other.ChildMask.IsOn(i)) {
            // Other has a child
            if (_childMask.IsOn(i)) {
                // Both have children - recurse
                GetChild(i).TopologyDifference(other.GetChild(i), background);
            } else if (_valueMask.IsOn(i)) {
                // This has active tile, other has child with some active values
                // Result: active tile minus other's active voxels
                // Replace tile with child, then subtract
                var child = new TChild(other.GetChild(i), GetValue(i), CopyMode.TopologyCopy);
                child.Fill(GetValue(i), true); // Fill all as active
                child.TopologyDifference(other.GetChild(i), background);
                SetChild(i, child);
            }
            // else: This has inactive tile - difference is empty, no change
        } else {
            // Other has a tile
            if (other.ValueMask.IsOn(i)) {
                // Other has active tile - subtract all
                if (_childMask.IsOn(i)) {
                    DeleteChild(i, background);
                }
                _valueMask.SetOff(i);
                SetValue(i, background);
            }
            // else: Other has inactive tile - no change
        }
    }
}
```

**Durée**: 3 jours (complexe, beaucoup de cas)

#### 3.4.3 RootNode topology operations

**Fichier**: `dotnet/OpenVDB.Core/Tree/RootNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/RootNode.h` (lignes ~3247-3330)

##### topologyUnion

```csharp
/// <summary>
/// Union this root node's set of active values with the other root node's active values.
/// </summary>
public void TopologyUnion<TOtherChild, TOtherValue>(
    RootNode<TOtherChild, TOtherValue> other,
    bool preserveTiles = false)
    where TOtherChild : class
    where TOtherValue : struct
{
    foreach (var entry in other.Table) {
        var coord = entry.Key;
        var otherNode = entry.Value;
        
        if (_table.ContainsKey(coord)) {
            // This root also has an entry at this coordinate
            var thisNode = _table[coord];
            
            if (thisNode.IsChild && otherNode.IsChild) {
                // Both are children - recurse
                thisNode.Child.TopologyUnion(otherNode.Child, preserveTiles);
            } else if (!thisNode.IsChild && otherNode.IsChild) {
                // This is tile, other is child
                if (!preserveTiles || !thisNode.IsActive) {
                    // Replace tile with child (topology copy)
                    _table[coord] = new NodeUnion<TValue, TChild> {
                        Child = new TChild(otherNode.Child, thisNode.Value, CopyMode.TopologyCopy)
                    };
                }
            } else if (thisNode.IsChild && !otherNode.IsChild && otherNode.IsActive) {
                // This is child, other is active tile - activate all in child
                thisNode.Child.Fill(_background, true);
            } else {
                // Both tiles or this child + other inactive tile
                // Union tile activity
                if (otherNode.IsActive) {
                    _table[coord] = new NodeUnion<TValue, TChild> {
                        Value = thisNode.IsChild ? _background : thisNode.Value,
                        IsActive = true
                    };
                }
            }
        } else {
            // This root doesn't have an entry - add from other
            if (otherNode.IsChild) {
                _table[coord] = new NodeUnion<TValue, TChild> {
                    Child = new TChild(otherNode.Child, _background, CopyMode.TopologyCopy)
                };
            } else {
                _table[coord] = new NodeUnion<TValue, TChild> {
                    Value = _background,
                    IsActive = otherNode.IsActive
                };
            }
        }
    }
}
```

##### topologyIntersection

```csharp
/// <summary>
/// Intersect this root node's set of active values with the other root node's active values.
/// </summary>
public void TopologyIntersection<TOtherChild, TOtherValue>(
    RootNode<TOtherChild, TOtherValue> other)
    where TOtherChild : class
    where TOtherValue : struct
{
    var coordsToRemove = new List<Coord>();
    
    foreach (var entry in _table) {
        var coord = entry.Key;
        var thisNode = entry.Value;
        
        if (!other.Table.ContainsKey(coord)) {
            // Other doesn't have this coord - intersection is empty
            coordsToRemove.Add(coord);
        } else {
            var otherNode = other.Table[coord];
            
            if (thisNode.IsChild && otherNode.IsChild) {
                // Both children - recurse
                thisNode.Child.TopologyIntersection(otherNode.Child, _background);
            } else if (!thisNode.IsChild && !otherNode.IsChild) {
                // Both tiles - intersection of activity
                if (!otherNode.IsActive) {
                    coordsToRemove.Add(coord); // Inactive tile intersection = empty
                } else {
                    // Other is active - keep this node's activity
                    // No change needed
                }
            } else if (thisNode.IsChild && !otherNode.IsChild) {
                // This child, other tile
                if (!otherNode.IsActive) {
                    coordsToRemove.Add(coord); // Intersection with inactive tile = empty
                }
                // else: other is active tile - keep this child as is
            } else {
                // This tile, other child
                if (thisNode.IsActive) {
                    // Replace active tile with child topology copy
                    _table[coord] = new NodeUnion<TValue, TChild> {
                        Child = new TChild(otherNode.Child, thisNode.Value, CopyMode.TopologyCopy)
                    };
                } else {
                    // Inactive tile - intersection is empty
                    coordsToRemove.Add(coord);
                }
            }
        }
    }
    
    // Remove entries
    foreach (var coord in coordsToRemove) {
        _table.Remove(coord);
    }
}
```

##### topologyDifference

```csharp
/// <summary>
/// Difference this root node's set of active values with the other root node's active values.
/// </summary>
public void TopologyDifference<TOtherChild, TOtherValue>(
    RootNode<TOtherChild, TOtherValue> other)
    where TOtherChild : class
    where TOtherValue : struct
{
    var coordsToRemove = new List<Coord>();
    
    foreach (var entry in _table) {
        var coord = entry.Key;
        var thisNode = entry.Value;
        
        if (!other.Table.ContainsKey(coord)) {
            // Other doesn't have this coord - keep this node as is
            continue;
        }
        
        var otherNode = other.Table[coord];
        
        if (thisNode.IsChild && otherNode.IsChild) {
            // Both children - recurse
            thisNode.Child.TopologyDifference(otherNode.Child, _background);
        } else if (!thisNode.IsChild && !otherNode.IsChild) {
            // Both tiles
            if (otherNode.IsActive) {
                // Subtract active tile - result is empty
                coordsToRemove.Add(coord);
            }
            // else: subtract inactive tile - no change
        } else if (thisNode.IsChild && !otherNode.IsChild) {
            // This child, other tile
            if (otherNode.IsActive) {
                // Subtract active tile - result is empty
                coordsToRemove.Add(coord);
            }
            // else: subtract inactive tile - no change
        } else {
            // This tile, other child
            if (thisNode.IsActive) {
                // Active tile minus other's active voxels
                var child = new TChild(otherNode.Child, thisNode.Value, CopyMode.TopologyCopy);
                child.Fill(thisNode.Value, true); // Fill all active
                child.TopologyDifference(otherNode.Child, _background);
                _table[coord] = new NodeUnion<TValue, TChild> {
                    Child = child
                };
            }
            // else: inactive tile - difference is empty
        }
    }
    
    // Remove entries
    foreach (var coord in coordsToRemove) {
        _table.Remove(coord);
    }
}
```

**Durée**: 3 jours

#### 3.4.4 Tree topology operations

**Fichier**: `dotnet/OpenVDB.Core/Tree/Tree.cs`  
**Référence C++**: `openvdb/openvdb/tree/Tree.h` (lignes ~1363-1380)

```csharp
/// <summary>
/// Union this tree's set of active values with the active values of the other tree.
/// </summary>
public void TopologyUnion<TOtherRoot>(Tree<TOtherRoot> other, bool preserveTiles = false)
    where TOtherRoot : class
{
    if (_root != null && other.Root() != null) {
        _root.TopologyUnion(other.Root(), preserveTiles);
    }
}

/// <summary>
/// Intersect this tree's set of active values with the active values of the other tree.
/// </summary>
public void TopologyIntersection<TOtherRoot>(Tree<TOtherRoot> other)
    where TOtherRoot : class
{
    if (_root != null && other.Root() != null) {
        _root.TopologyIntersection(other.Root());
    }
}

/// <summary>
/// Difference this tree's set of active values with the active values of the other tree.
/// </summary>
public void TopologyDifference<TOtherRoot>(Tree<TOtherRoot> other)
    where TOtherRoot : class
{
    if (_root != null && other.Root() != null) {
        _root.TopologyDifference(other.Root());
    }
}
```

**Durée**: 0.5 jour

#### 3.4.5 Grid topology operations

**Fichier**: `dotnet/OpenVDB.Core/Grid/Grid.cs`  
**Référence C++**: `openvdb/openvdb/Grid.h` (lignes ~1554-1577)

```csharp
/// <summary>
/// Union this grid's set of active values with the active values of the other grid.
/// </summary>
public void TopologyUnion<TOtherTree>(Grid<TOtherTree> other)
    where TOtherTree : class
{
    _tree.TopologyUnion(other.Tree());
}

/// <summary>
/// Intersect this grid's set of active values with the active values of the other grid.
/// </summary>
public void TopologyIntersection<TOtherTree>(Grid<TOtherTree> other)
    where TOtherTree : class
{
    _tree.TopologyIntersection(other.Tree());
}

/// <summary>
/// Difference this grid's set of active values with the active values of the other grid.
/// </summary>
public void TopologyDifference<TOtherTree>(Grid<TOtherTree> other)
    where TOtherTree : class
{
    _tree.TopologyDifference(other.Tree());
}
```

**Durée**: 0.5 jour

**Total Phase 4**: **8 jours** (1.6 semaines)

---

### PHASE 5: I/O topology (2 semaines)

**Note**: C'est la partie la plus complexe car elle nécessite le système I/O complet.

#### 3.5.1 Prérequis: Compléter IO/Stream.cs

**Fichier**: `dotnet/OpenVDB.Core/IO/Stream.cs`  
**Status actuel**: Stub

**Méthodes nécessaires**:
- `WriteInt32`, `ReadInt32`
- `WriteInt64`, `ReadInt64`
- `WriteFloat`, `ReadFloat`
- `WriteHalf`, `ReadHalf` (conversion float ↔ half)
- `WriteCoord`, `ReadCoord`
- `WriteBool`, `ReadBool`

**Durée**: 2 jours

#### 3.5.2 LeafNode.readTopology / writeTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/LeafNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/LeafNode.h` (lignes ~2200-2280)

```csharp
/// <summary>
/// Write this node's topology to an output stream.
/// </summary>
public void WriteTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Write origin
    stream.WriteCoord(_origin);
    
    // 2. Write value mask (active voxel mask)
    _valueMask.Write(stream);
    
    // 3. NO values written (topology only)
}

/// <summary>
/// Read this node's topology from an input stream.
/// </summary>
public void ReadTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Read origin
    _origin = stream.ReadCoord();
    
    // 2. Read value mask
    _valueMask.Read(stream);
    
    // 3. Initialize buffer with background value (values will be loaded later)
    _buffer = new TValue[SIZE];
    for (int i = 0; i < SIZE; ++i) {
        _buffer[i] = default(TValue); // or pass background
    }
}
```

**Durée**: 1 jour

#### 3.5.3 InternalNode.readTopology / writeTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/InternalNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/InternalNode.h` (lignes ~2600-2700)

```csharp
/// <summary>
/// Write this node's topology to an output stream.
/// </summary>
public void WriteTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Write origin
    stream.WriteCoord(_origin);
    
    // 2. Write child mask
    _childMask.Write(stream);
    
    // 3. Write value mask (for tiles)
    _valueMask.Write(stream);
    
    // 4. Recursively write children's topology
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (_childMask.IsOn(i)) {
            GetChild(i).WriteTopology(stream, saveFloatAsHalf);
        }
    }
    
    // 5. NO tile values written (topology only)
}

/// <summary>
/// Read this node's topology from an input stream.
/// </summary>
public void ReadTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Read origin
    _origin = stream.ReadCoord();
    
    // 2. Read child mask
    _childMask.Read(stream);
    
    // 3. Read value mask
    _valueMask.Read(stream);
    
    // 4. Initialize table
    _table = new UnionType[NUM_VALUES];
    
    for (int i = 0; i < NUM_VALUES; ++i) {
        if (_childMask.IsOn(i)) {
            // Recursively read child topology
            var child = new TChild();
            child.ReadTopology(stream, saveFloatAsHalf);
            _table[i] = new UnionType { Child = child };
        } else {
            // Initialize tile with default value
            _table[i] = new UnionType {
                Tile = new Tile { Value = default(TValue), Active = _valueMask.IsOn(i) }
            };
        }
    }
}
```

**Durée**: 2 jours

#### 3.5.4 RootNode.readTopology / writeTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/RootNode.cs`  
**Référence C++**: `openvdb/openvdb/tree/RootNode.h` (lignes ~2348-2430)

```csharp
/// <summary>
/// Write this root node's topology to an output stream.
/// </summary>
public bool WriteTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Write table size
    stream.WriteInt32(_table.Count);
    
    // 2. Write background value (might be needed for reconstruction)
    stream.WriteValue(_background, saveFloatAsHalf);
    
    // 3. Write each entry in table
    foreach (var entry in _table) {
        // Write coordinate
        stream.WriteCoord(entry.Key);
        
        // Write node type (tile vs child)
        stream.WriteBool(entry.Value.IsChild);
        
        if (entry.Value.IsChild) {
            // Write child topology recursively
            entry.Value.Child.WriteTopology(stream, saveFloatAsHalf);
        } else {
            // Write tile activity (not value - topology only)
            stream.WriteBool(entry.Value.IsActive);
        }
    }
    
    return true;
}

/// <summary>
/// Read this root node's topology from an input stream.
/// </summary>
public bool ReadTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // 1. Read table size
    int tableSize = stream.ReadInt32();
    
    // 2. Read background value
    _background = stream.ReadValue<TValue>(saveFloatAsHalf);
    
    // 3. Read each entry
    _table = new Dictionary<Coord, NodeUnion<TValue, TChild>>(tableSize);
    
    for (int i = 0; i < tableSize; ++i) {
        // Read coordinate
        var coord = stream.ReadCoord();
        
        // Read node type
        bool isChild = stream.ReadBool();
        
        if (isChild) {
            // Read child topology recursively
            var child = new TChild();
            child.ReadTopology(stream, saveFloatAsHalf);
            _table[coord] = new NodeUnion<TValue, TChild> { Child = child };
        } else {
            // Read tile activity
            bool isActive = stream.ReadBool();
            _table[coord] = new NodeUnion<TValue, TChild> {
                Value = _background,
                IsActive = isActive
            };
        }
    }
    
    return true;
}
```

**Durée**: 2 jours

#### 3.5.5 Tree.readTopology / writeTopology

**Fichier**: `dotnet/OpenVDB.Core/Tree/Tree.cs`  
**Référence C++**: `openvdb/openvdb/tree/Tree.h` (lignes ~1283-1297)

```csharp
/// <summary>
/// Write this tree's topology to an output stream.
/// </summary>
public void WriteTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // Write tree metadata (if any)
    // ... 
    
    // Write root topology
    if (_root != null) {
        _root.WriteTopology(stream, saveFloatAsHalf);
    }
}

/// <summary>
/// Read this tree's topology from an input stream.
/// </summary>
public void ReadTopology(Stream stream, bool saveFloatAsHalf = false)
{
    // Read tree metadata (if any)
    // ...
    
    // Read root topology
    if (_root == null) {
        _root = new TRoot();
    }
    _root.ReadTopology(stream, saveFloatAsHalf);
}
```

**Durée**: 1 jour

#### 3.5.6 Grid.readTopology / writeTopology

**Fichier**: `dotnet/OpenVDB.Core/Grid/Grid.cs`  
**Référence C++**: `openvdb/openvdb/Grid.h` (lignes ~1609-1622)

```csharp
/// <summary>
/// Write this grid's topology to an output stream.
/// This writes only the grid structure, not the actual data buffers.
/// </summary>
public void WriteTopology(Stream stream)
{
    _tree.WriteTopology(stream, SaveFloatAsHalf);
}

/// <summary>
/// Read this grid's topology from an input stream.
/// This reads only the grid structure, not the actual data buffers.
/// </summary>
public void ReadTopology(Stream stream)
{
    _tree.ReadTopology(stream, SaveFloatAsHalf);
}
```

**Durée**: 0.5 jour

**Total Phase 5**: **8.5 jours** (1.7 semaines)

---

## 4. Plan de tests

### 4.1 Tests unitaires par méthode

Chaque méthode topology doit avoir au minimum:

1. **Test de base**: Cas simple, 1-2 voxels actifs
2. **Test de compatibilité de types**: Tester avec types différents (float ↔ double)
3. **Test de cas limites**: 
   - Grilles vides
   - Grilles complètement remplies
   - Overlap partiel
4. **Test de performance**: Vérifier que les opérations sont en O(n) où n = voxels actifs

### 4.2 Tests d'intégration

1. **CSG complet**: Créer sphere ∪ box, sphere ∩ cylinder, etc.
2. **I/O round-trip**: Write topology → Read topology → Compare avec hasSameTopology
3. **Compatibilité avec fichiers C++**: Lire fichiers VDB générés par C++

### 4.3 Framework de tests

Utiliser **xUnit** avec structure:

```
dotnet/OpenVDB.Core.Tests/
├── Tree/
│   ├── LeafNodeTopologyTests.cs
│   ├── InternalNodeTopologyTests.cs
│   ├── RootNodeTopologyTests.cs
│   └── TreeTopologyTests.cs
├── Grid/
│   └── GridTopologyTests.cs
└── TestData/
    ├── simple_sphere.vdb
    ├── csg_union.vdb
    └── ...
```

**Durée totale tests**: **5 jours** (parallèle à l'implémentation)

---

## 5. Récapitulatif et timeline

### 5.1 Résumé des phases

| Phase | Description | Durée | Dépendances |
|-------|-------------|-------|-------------|
| **Phase 1** | Compléter structures de nœuds | 12 jours | Aucune |
| **Phase 2** | `hasSameTopology` | 4 jours | Phase 1 |
| **Phase 3** | Topology copy constructors | 6 jours | Phase 1 |
| **Phase 4** | Opérations booléennes | 8 jours | Phases 1-3 |
| **Phase 5** | I/O topology | 8.5 jours | Phases 1-4, IO/Stream |
| **Tests** | Tests unitaires et intégration | 5 jours | Parallèle |

**Durée totale séquentielle**: **38.5 jours** (~7.7 semaines)

Avec parallélisation (tests + certaines tâches):
**Durée optimisée**: **30-35 jours** (~6-7 semaines)

### 5.2 Timeline détaillée (7 semaines)

#### Semaine 1-2: Phase 1 - Structures de nœuds
- [ ] LeafNode méthodes de base (3j)
- [ ] InternalNode méthodes de base (4j)
- [ ] RootNode méthodes de base (3j)
- [ ] Tree méthodes de base (2j)

#### Semaine 3: Phase 2 + 3 début
- [ ] hasSameTopology tous niveaux (4j)
- [ ] TopologyCopy tag/enum (0.5j)

#### Semaine 4: Phase 3 fin
- [ ] Topology copy constructors (5.5j)

#### Semaine 5: Phase 4 - Opérations booléennes
- [ ] LeafNode topology ops (1j)
- [ ] InternalNode topology ops (3j)
- [ ] RootNode topology ops (3j)
- [ ] Tree + Grid wrappers (1j)

#### Semaine 6-7: Phase 5 - I/O topology
- [ ] Compléter IO/Stream (2j)
- [ ] LeafNode I/O (1j)
- [ ] InternalNode I/O (2j)
- [ ] RootNode I/O (2j)
- [ ] Tree + Grid I/O (1.5j)

#### En parallèle: Tests
- [ ] Tests unitaires (continu)
- [ ] Tests d'intégration (semaines 6-7)

---

## 6. Risques et atténuations

### 6.1 Risques identifiés

| Risque | Probabilité | Impact | Atténuation |
|--------|-------------|--------|-------------|
| **Complexité sous-estimée** | Moyenne | Haute | Buffer de 20% dans timeline |
| **Dépendances manquantes** (itérateurs, etc.) | Haute | Moyenne | Identifier tôt, porter si nécessaire |
| **Bugs de conversion C++ → C#** | Haute | Moyenne | Tests exhaustifs, validation avec C++ |
| **Performance insuffisante** | Faible | Moyenne | Profiling, optimisation si nécessaire |
| **Incompatibilité I/O avec C++** | Moyenne | Haute | Tests avec vrais fichiers VDB |

### 6.2 Plan de contingence

Si délais dépassés:

1. **Prioriser Phase 4** (opérations booléennes) - cas d'usage principal
2. **Reporter Phase 5** (I/O) à une version ultérieure
3. **Simplifier tests** - focus sur tests unitaires, moins d'intégration

---

## 7. Livrables attendus

### 7.1 Code

- [ ] **5 fichiers modifiés** avec méthodes topology:
  - `LeafNode.cs` (+500 lignes)
  - `InternalNode.cs` (+600 lignes)
  - `RootNode.cs` (+400 lignes)
  - `Tree.cs` (+200 lignes)
  - `Grid.cs` (+100 lignes)

- [ ] **1 nouveau fichier**: `CopyMode.cs` ou intégré dans `Types.cs`

- [ ] **IO/Stream.cs complété** (+300 lignes)

**Total**: ~2100 lignes de code

### 7.2 Tests

- [ ] **15 fichiers de tests** minimum
- [ ] **Coverage**: >80% sur méthodes topology
- [ ] **Tests d'intégration**: CSG, I/O round-trip

### 7.3 Documentation

- [ ] **XML docs** pour toutes les méthodes publiques
- [ ] **README topology** avec exemples d'usage
- [ ] **Migration guide** C++ → C# pour topology

---

## 8. Conclusion

### 8.1 Impact du portage

Après portage, le port C# OpenVDB aura:
- ✅ **Opérations CSG** complètes
- ✅ **Chargement différé** de fichiers VDB
- ✅ **Conversion de types** avec structure préservée
- ✅ **Compatibilité** avec outils existants (Lot 7)
- ✅ **I/O efficace** pour gros volumes

### 8.2 Déblocage des lots

| Lot | Status actuel | Après portage topology |
|-----|---------------|------------------------|
| **Lot 6 (IO)** | ⚠️ BLOQUÉ (readTopology manquant) | ✅ DÉBLOQUÉ |
| **Lot 7 (Tools)** | ⚠️ PARTIEL (opérations manquantes) | ✅ DÉBLOQUÉ |
| **Lot 8 (Points)** | ⚠️ PARTIEL | ✅ DÉBLOQUÉ |

### 8.3 Prochaines étapes

1. ✅ **Validation du plan** par l'équipe
2. ⏭️ **Démarrage Phase 1** (structures de nœuds)
3. ⏭️ **Revue hebdomadaire** pour ajustements

---

**Auteur**: GitHub Copilot  
**Version**: 1.0  
**À valider par**: Équipe projet OpenVDB C#
