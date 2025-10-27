# Plan d'action de portage C++ vers C# .NET 9

## Vue d'ensemble

Ce document définit le plan d'action complet pour porter la bibliothèque OpenVDB du C++ vers C# .NET 9. Le portage concerne **189 fichiers** répartis dans plusieurs modules fonctionnels.

> ⚠️ **Important** : Ce document est un plan d'action uniquement. Le portage effectif des fichiers sera réalisé ultérieurement en suivant ce plan.

---

## Structure du projet cible

### Organisation des dossiers

```
openvdb/
├── dotnet/                          # Nouveau dossier racine pour le portage .NET
│   ├── OpenVDB.sln                 # Solution principale
│   ├── OpenVDB.Core/               # Projet bibliothèque principale
│   │   ├── OpenVDB.Core.csproj
│   │   ├── Platform/               # Types de base et configuration
│   │   ├── Math/                   # Mathématiques vectorielles et transformations
│   │   ├── Tree/                   # Structure d'arbre hiérarchique
│   │   ├── Grid/                   # Grilles volumétriques
│   │   ├── IO/                     # Lecture/écriture de fichiers
│   │   ├── Metadata/               # Métadonnées
│   │   ├── Utils/                  # Utilitaires
│   │   └── Threading/              # Support multi-threading
│   ├── OpenVDB.Tools/              # Projet pour les outils de manipulation
│   │   └── OpenVDB.Tools.csproj
│   ├── OpenVDB.Points/             # Projet pour le système de points
│   │   └── OpenVDB.Points.csproj
│   └── README.md                   # Documentation du portage
```

### Configuration des projets .NET

#### OpenVDB.Core.csproj (Bibliothèque principale)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <RootNamespace>OpenVDB</RootNamespace>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

#### OpenVDB.Tools.csproj (Outils et algorithmes)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <RootNamespace>OpenVDB.Tools</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\OpenVDB.Core\OpenVDB.Core.csproj" />
  </ItemGroup>
</Project>
```

#### OpenVDB.Points.csproj (Système de points)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <RootNamespace>OpenVDB.Points</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\OpenVDB.Core\OpenVDB.Core.csproj" />
  </ItemGroup>
</Project>
```

---

## Thèmes du projet (par ordre de priorité)

### Priorité 1 : Fondations (Core Infrastructure)
- [ ] Types de base et configuration plateforme
- [ ] Structures mathématiques vectorielles
- [ ] Système de métadonnées
- [ ] Utilitaires et helpers

**Justification** : Ces composants sont des dépendances fondamentales pour tous les autres modules.

### Priorité 2 : Structure de données (Tree System)
- [ ] Nœuds de l'arbre (RootNode, InternalNode, LeafNode)
- [ ] Itérateurs d'arbre
- [ ] ValueAccessor pour accès optimisé
- [ ] Gestionnaires de nœuds

**Justification** : Le système d'arbre est le cœur de la bibliothèque OpenVDB.

### Priorité 3 : Grilles et transformations (Grid System)
- [ ] Interface Grid et GridBase
- [ ] Types de grilles spécialisées
- [ ] Transformations spatiales
- [ ] Factory patterns pour création de grilles

**Justification** : Les grilles utilisent le système d'arbre et constituent l'API principale.

### Priorité 4 : Entrées/Sorties (IO System)
- [ ] Lecture/écriture de fichiers VDB
- [ ] Compression/décompression
- [ ] Streaming et archives
- [ ] Descripteurs de grilles

**Justification** : Permet la persistance et l'interopérabilité des données.

### Priorité 5 : Outils de manipulation (Tools)
- [ ] Opérations de base (activation, clipping, composite)
- [ ] Filtres et opérateurs
- [ ] Level sets
- [ ] Conversions mesh/volume

**Justification** : Fournit les algorithmes de manipulation de volumes.

### Priorité 6 : Système de points (Points)
- [ ] Structures d'attributs
- [ ] Grilles de points (PointDataGrid)
- [ ] Opérations sur points
- [ ] Implémentations spécialisées

**Justification** : Module spécialisé pour la gestion de nuages de points.

---

## Fichiers à porter (189 fichiers)

### Lot 1 : Fondations - Configuration et types de base (14 fichiers)
**Module racine** : `openvdb/openvdb/`

#### Sous-lot 1A : Configuration plateforme (4 fichiers)
- [ ] `Platform.h` → `dotnet/OpenVDB.Core/Platform/Platform.cs`
- [ ] `Platform.cc` → *(Intégrer dans Platform.cs)*
- [ ] `PlatformConfig.h` → `dotnet/OpenVDB.Core/Platform/PlatformConfig.cs`
- [ ] `Exceptions.h` → `dotnet/OpenVDB.Core/Platform/Exceptions.cs`

#### Sous-lot 1B : Types fondamentaux (5 fichiers)
- [ ] `Types.h` → `dotnet/OpenVDB.Core/Types.cs`
- [ ] `TypeList.h` → `dotnet/OpenVDB.Core/TypeList.cs`
- [ ] `openvdb.h` → `dotnet/OpenVDB.Core/OpenVDB.cs`
- [ ] `openvdb.cc` → *(Intégrer dans OpenVDB.cs)*

#### Sous-lot 1C : Métadonnées (5 fichiers)
- [ ] `Metadata.h` → `dotnet/OpenVDB.Core/Metadata/Metadata.cs`
- [ ] `Metadata.cc` → *(Intégrer dans Metadata.cs)*
- [ ] `MetaMap.h` → `dotnet/OpenVDB.Core/Metadata/MetaMap.cs`
- [ ] `MetaMap.cc` → *(Intégrer dans MetaMap.cs)*
- [ ] `Grid.h` → `dotnet/OpenVDB.Core/Grid/Grid.cs` *(partiellement)*
- [ ] `Grid.cc` → *(Intégrer dans Grid.cs)*

---

### Lot 2 : Mathématiques (28 fichiers)
**Module** : `openvdb/openvdb/math/`

#### Sous-lot 2A : Vecteurs et matrices de base (12 fichiers)
- [ ] `Vec2.h` → `dotnet/OpenVDB.Core/Math/Vec2.cs`
- [ ] `Vec3.h` → `dotnet/OpenVDB.Core/Math/Vec3.cs`
- [ ] `Vec4.h` → `dotnet/OpenVDB.Core/Math/Vec4.cs`
- [ ] `Tuple.h` → `dotnet/OpenVDB.Core/Math/Tuple.cs`
- [ ] `Mat.h` → `dotnet/OpenVDB.Core/Math/Mat.cs`
- [ ] `Mat3.h` → `dotnet/OpenVDB.Core/Math/Mat3.cs`
- [ ] `Mat4.h` → `dotnet/OpenVDB.Core/Math/Mat4.cs`
- [ ] `Quat.h` → `dotnet/OpenVDB.Core/Math/Quat.cs`
- [ ] `Ray.h` → `dotnet/OpenVDB.Core/Math/Ray.cs`
- [ ] `BBox.h` → `dotnet/OpenVDB.Core/Math/BBox.cs`
- [ ] `Coord.h` → `dotnet/OpenVDB.Core/Math/Coord.cs`
- [ ] `Math.h` → `dotnet/OpenVDB.Core/Math/Math.cs`

#### Sous-lot 2B : Opérations mathématiques (8 fichiers)
- [ ] `Operators.h` → `dotnet/OpenVDB.Core/Math/Operators.cs`
- [ ] `Stats.h` → `dotnet/OpenVDB.Core/Math/Stats.cs`
- [ ] `FiniteDifference.h` → `dotnet/OpenVDB.Core/Math/FiniteDifference.cs`
- [ ] `Stencils.h` → `dotnet/OpenVDB.Core/Math/Stencils.cs`
- [ ] `ConjGradient.h` → `dotnet/OpenVDB.Core/Math/ConjGradient.cs`
- [ ] `DDA.h` → `dotnet/OpenVDB.Core/Math/DDA.cs`
- [ ] `Proximity.h` → `dotnet/OpenVDB.Core/Math/Proximity.cs`
- [ ] `Proximity.cc` → *(Intégrer dans Proximity.cs)*

#### Sous-lot 2C : Transformations et types spécialisés (8 fichiers)
- [ ] `Transform.h` → `dotnet/OpenVDB.Core/Math/Transform.cs`
- [ ] `Transform.cc` → *(Intégrer dans Transform.cs)*
- [ ] `Maps.h` → `dotnet/OpenVDB.Core/Math/Maps.cs`
- [ ] `Maps.cc` → *(Intégrer dans Maps.cs)*
- [ ] `Half.h` → `dotnet/OpenVDB.Core/Math/Half.cs`
- [ ] `Half.cc` → *(Intégrer dans Half.cs)*
- [ ] `QuantizedUnitVec.h` → `dotnet/OpenVDB.Core/Math/QuantizedUnitVec.cs`
- [ ] `QuantizedUnitVec.cc` → *(Intégrer dans QuantizedUnitVec.cs)*

---

### Lot 3 : Utilitaires (13 fichiers)
**Module** : `openvdb/openvdb/util/`

#### Sous-lot 3A : Utilitaires système (7 fichiers)
- [ ] `Assert.h` → `dotnet/OpenVDB.Core/Utils/Assert.cs`
- [ ] `Assert.cc` → *(Intégrer dans Assert.cs)*
- [ ] `Util.h` → `dotnet/OpenVDB.Core/Utils/Util.cs`
- [ ] `Name.h` → `dotnet/OpenVDB.Core/Utils/Name.cs`
- [ ] `logging.h` → `dotnet/OpenVDB.Core/Utils/Logging.cs`
- [ ] `CpuTimer.h` → `dotnet/OpenVDB.Core/Utils/CpuTimer.cs`
- [ ] `NullInterrupter.h` → `dotnet/OpenVDB.Core/Utils/NullInterrupter.cs`

#### Sous-lot 3B : Structures de données utilitaires (6 fichiers)
- [ ] `NodeMasks.h` → `dotnet/OpenVDB.Core/Utils/NodeMasks.cs`
- [ ] `PagedArray.h` → `dotnet/OpenVDB.Core/Utils/PagedArray.cs`
- [ ] `MapsUtil.h` → `dotnet/OpenVDB.Core/Utils/MapsUtil.cs`
- [ ] `ExplicitInstantiation.h` → *(Adapter pour C# génériques)*
- [ ] `Formats.h` → `dotnet/OpenVDB.Core/Utils/Formats.cs`
- [ ] `Formats.cc` → *(Intégrer dans Formats.cs)*

---

### Lot 4 : Threading (1 fichier)
**Module** : `openvdb/openvdb/thread/`

- [ ] `Threading.h` → `dotnet/OpenVDB.Core/Threading/Threading.cs`

---

### Lot 5 : Structure d'arbre (13 fichiers)
**Module** : `openvdb/openvdb/tree/`

#### Sous-lot 5A : Nœuds de base (6 fichiers)
- [ ] `Tree.h` → `dotnet/OpenVDB.Core/Tree/Tree.cs`
- [ ] `RootNode.h` → `dotnet/OpenVDB.Core/Tree/RootNode.cs`
- [ ] `InternalNode.h` → `dotnet/OpenVDB.Core/Tree/InternalNode.cs`
- [ ] `LeafNode.h` → `dotnet/OpenVDB.Core/Tree/LeafNode.cs`
- [ ] `LeafNodeBool.h` → `dotnet/OpenVDB.Core/Tree/LeafNodeBool.cs`
- [ ] `LeafNodeMask.h` → `dotnet/OpenVDB.Core/Tree/LeafNodeMask.cs`

#### Sous-lot 5B : Support et accesseurs (7 fichiers)
- [ ] `LeafBuffer.h` → `dotnet/OpenVDB.Core/Tree/LeafBuffer.cs`
- [ ] `NodeUnion.h` → `dotnet/OpenVDB.Core/Tree/NodeUnion.cs`
- [ ] `ValueAccessor.h` → `dotnet/OpenVDB.Core/Tree/ValueAccessor.cs`
- [ ] `Iterator.h` → `dotnet/OpenVDB.Core/Tree/Iterator.cs`
- [ ] `TreeIterator.h` → `dotnet/OpenVDB.Core/Tree/TreeIterator.cs`
- [ ] `LeafManager.h` → `dotnet/OpenVDB.Core/Tree/LeafManager.cs`
- [ ] `NodeManager.h` → `dotnet/OpenVDB.Core/Tree/NodeManager.cs`

---

### Lot 6 : Entrées/Sorties (17 fichiers)
**Module** : `openvdb/openvdb/io/`

#### Sous-lot 6A : Fichiers et streaming (8 fichiers)
- [ ] `io.h` → `dotnet/OpenVDB.Core/IO/IO.cs`
- [ ] `File.h` → `dotnet/OpenVDB.Core/IO/File.cs`
- [ ] `File.cc` → *(Intégrer dans File.cs)*
- [ ] `Stream.h` → `dotnet/OpenVDB.Core/IO/Stream.cs`
- [ ] `Stream.cc` → *(Intégrer dans Stream.cs)*
- [ ] `Archive.h` → `dotnet/OpenVDB.Core/IO/Archive.cs`
- [ ] `Archive.cc` → *(Intégrer dans Archive.cs)*
- [ ] `Queue.h` → `dotnet/OpenVDB.Core/IO/Queue.cs`
- [ ] `Queue.cc` → *(Intégrer dans Queue.cs)*

#### Sous-lot 6B : Compression et descripteurs (6 fichiers)
- [ ] `Compression.h` → `dotnet/OpenVDB.Core/IO/Compression.cs`
- [ ] `Compression.cc` → *(Intégrer dans Compression.cs)*
- [ ] `GridDescriptor.h` → `dotnet/OpenVDB.Core/IO/GridDescriptor.cs`
- [ ] `GridDescriptor.cc` → *(Intégrer dans GridDescriptor.cs)*
- [ ] `TempFile.h` → `dotnet/OpenVDB.Core/IO/TempFile.cs`
- [ ] `TempFile.cc` → *(Intégrer dans TempFile.cs)*

#### Sous-lot 6C : Chargement différé (3 fichiers)
- [ ] `DelayedLoadMetadata.h` → `dotnet/OpenVDB.Core/IO/DelayedLoadMetadata.cs`
- [ ] `DelayedLoadMetadata.cc` → *(Intégrer dans DelayedLoadMetadata.cs)*

---

### Lot 7 : Outils de manipulation (56 fichiers)
**Module** : `openvdb/openvdb/tools/`

#### Sous-lot 7A : Opérations de base (10 fichiers)
- [ ] `Activate.h` → `dotnet/OpenVDB.Tools/Activate.cs`
- [ ] `ChangeBackground.h` → `dotnet/OpenVDB.Tools/ChangeBackground.cs`
- [ ] `Clip.h` → `dotnet/OpenVDB.Tools/Clip.cs`
- [ ] `Composite.h` → `dotnet/OpenVDB.Tools/Composite.cs`
- [ ] `Count.h` → `dotnet/OpenVDB.Tools/Count.cs`
- [ ] `Dense.h` → `dotnet/OpenVDB.Tools/Dense.cs`
- [ ] `DenseSparseTools.h` → `dotnet/OpenVDB.Tools/DenseSparseTools.cs`
- [ ] `Diagnostics.h` → `dotnet/OpenVDB.Tools/Diagnostics.cs`
- [ ] `Mask.h` → `dotnet/OpenVDB.Tools/Mask.cs`
- [ ] `Merge.h` → `dotnet/OpenVDB.Tools/Merge.cs`

#### Sous-lot 7B : Filtres et opérateurs (8 fichiers)
- [ ] `Filter.h` → `dotnet/OpenVDB.Tools/Filter.cs`
- [ ] `GridOperators.h` → `dotnet/OpenVDB.Tools/GridOperators.cs`
- [ ] `Interpolation.h` → `dotnet/OpenVDB.Tools/Interpolation.cs`
- [ ] `Morphology.h` → `dotnet/OpenVDB.Tools/Morphology.cs`
- [ ] `Prune.h` → `dotnet/OpenVDB.Tools/Prune.cs`
- [ ] `Statistics.h` → `dotnet/OpenVDB.Tools/Statistics.cs`
- [ ] `ValueTransformer.h` → `dotnet/OpenVDB.Tools/ValueTransformer.cs`
- [ ] `VectorTransformer.h` → `dotnet/OpenVDB.Tools/VectorTransformer.cs`

#### Sous-lot 7C : Level Sets (13 fichiers)
- [ ] `LevelSetAdvect.h` → `dotnet/OpenVDB.Tools/LevelSetAdvect.cs`
- [ ] `LevelSetDilatedMesh.h` → `dotnet/OpenVDB.Tools/LevelSetDilatedMesh.cs`
- [ ] `LevelSetFilter.h` → `dotnet/OpenVDB.Tools/LevelSetFilter.cs`
- [ ] `LevelSetFracture.h` → `dotnet/OpenVDB.Tools/LevelSetFracture.cs`
- [ ] `LevelSetMeasure.h` → `dotnet/OpenVDB.Tools/LevelSetMeasure.cs`
- [ ] `LevelSetMorph.h` → `dotnet/OpenVDB.Tools/LevelSetMorph.cs`
- [ ] `LevelSetPlatonic.h` → `dotnet/OpenVDB.Tools/LevelSetPlatonic.cs`
- [ ] `LevelSetRebuild.h` → `dotnet/OpenVDB.Tools/LevelSetRebuild.cs`
- [ ] `LevelSetSphere.h` → `dotnet/OpenVDB.Tools/LevelSetSphere.cs`
- [ ] `LevelSetTracker.h` → `dotnet/OpenVDB.Tools/LevelSetTracker.cs`
- [ ] `LevelSetTubes.h` → `dotnet/OpenVDB.Tools/LevelSetTubes.cs`
- [ ] `LevelSetUtil.h` → `dotnet/OpenVDB.Tools/LevelSetUtil.cs`
- [ ] `TopologyToLevelSet.h` → `dotnet/OpenVDB.Tools/TopologyToLevelSet.cs`

#### Sous-lot 7D : Conversions et transformations (9 fichiers)
- [ ] `MeshToVolume.h` → `dotnet/OpenVDB.Tools/MeshToVolume.cs`
- [ ] `VolumeToMesh.h` → `dotnet/OpenVDB.Tools/VolumeToMesh.cs`
- [ ] `VolumeToSpheres.h` → `dotnet/OpenVDB.Tools/VolumeToSpheres.cs`
- [ ] `GridTransformer.h` → `dotnet/OpenVDB.Tools/GridTransformer.cs`
- [ ] `MultiResGrid.h` → `dotnet/OpenVDB.Tools/MultiResGrid.cs`
- [ ] `NodeVisitor.h` → `dotnet/OpenVDB.Tools/NodeVisitor.cs`
- [ ] `FindActiveValues.h` → `dotnet/OpenVDB.Tools/FindActiveValues.cs`
- [ ] `SignedFloodFill.h` → `dotnet/OpenVDB.Tools/SignedFloodFill.cs`
- [ ] `FastSweeping.h` → `dotnet/OpenVDB.Tools/FastSweeping.cs`

#### Sous-lot 7E : Particules et points (7 fichiers)
- [ ] `ParticleAtlas.h` → `dotnet/OpenVDB.Tools/ParticleAtlas.cs`
- [ ] `ParticlesToLevelSet.h` → `dotnet/OpenVDB.Tools/ParticlesToLevelSet.cs`
- [ ] `PointAdvect.h` → `dotnet/OpenVDB.Tools/PointAdvect.cs`
- [ ] `PointIndexGrid.h` → `dotnet/OpenVDB.Tools/PointIndexGrid.cs`
- [ ] `PointPartitioner.h` → `dotnet/OpenVDB.Tools/PointPartitioner.cs`
- [ ] `PointScatter.h` → `dotnet/OpenVDB.Tools/PointScatter.cs`
- [ ] `PointsToMask.h` → `dotnet/OpenVDB.Tools/PointsToMask.cs`

#### Sous-lot 7F : Algorithmes avancés (6 fichiers)
- [ ] `PoissonSolver.h` → `dotnet/OpenVDB.Tools/PoissonSolver.cs`
- [ ] `PotentialFlow.h` → `dotnet/OpenVDB.Tools/PotentialFlow.cs`
- [ ] `RayIntersector.h` → `dotnet/OpenVDB.Tools/RayIntersector.cs`
- [ ] `RayTracer.h` → `dotnet/OpenVDB.Tools/RayTracer.cs`
- [ ] `VelocityFields.h` → `dotnet/OpenVDB.Tools/VelocityFields.cs`
- [ ] `VolumeAdvect.h` → `dotnet/OpenVDB.Tools/VolumeAdvect.cs`

#### Sous-lot 7G : Implémentations (3 fichiers)
- [ ] `impl/ConvexVoxelizer.h` → `dotnet/OpenVDB.Tools/Impl/ConvexVoxelizer.cs`
- [ ] `impl/LevelSetDilatedMeshImpl.h` → `dotnet/OpenVDB.Tools/Impl/LevelSetDilatedMeshImpl.cs`
- [ ] `impl/LevelSetTubesImpl.h` → `dotnet/OpenVDB.Tools/Impl/LevelSetTubesImpl.cs`

---

### Lot 8 : Système de points (47 fichiers)
**Module** : `openvdb/openvdb/points/`

#### Sous-lot 8A : Attributs de base (9 fichiers)
- [ ] `AttributeArray.h` → `dotnet/OpenVDB.Points/AttributeArray.cs`
- [ ] `AttributeArray.cc` → *(Intégrer dans AttributeArray.cs)*
- [ ] `AttributeArrayString.h` → `dotnet/OpenVDB.Points/AttributeArrayString.cs`
- [ ] `AttributeArrayString.cc` → *(Intégrer dans AttributeArrayString.cs)*
- [ ] `AttributeGroup.h` → `dotnet/OpenVDB.Points/AttributeGroup.cs`
- [ ] `AttributeGroup.cc` → *(Intégrer dans AttributeGroup.cs)*
- [ ] `AttributeSet.h` → `dotnet/OpenVDB.Points/AttributeSet.cs`
- [ ] `AttributeSet.cc` → *(Intégrer dans AttributeSet.cs)*
- [ ] `points.cc` → `dotnet/OpenVDB.Points/Points.cs`

#### Sous-lot 8B : Grilles et filtres de points (4 fichiers)
- [ ] `PointDataGrid.h` → `dotnet/OpenVDB.Points/PointDataGrid.cs`
- [ ] `IndexFilter.h` → `dotnet/OpenVDB.Points/IndexFilter.cs`
- [ ] `IndexIterator.h` → `dotnet/OpenVDB.Points/IndexIterator.cs`
- [ ] `StreamCompression.h` → `dotnet/OpenVDB.Points/StreamCompression.cs`
- [ ] `StreamCompression.cc` → *(Intégrer dans StreamCompression.cs)*

#### Sous-lot 8C : Opérations sur points (12 fichiers)
- [ ] `PointAdvect.h` → `dotnet/OpenVDB.Points/PointAdvect.cs`
- [ ] `PointAttribute.h` → `dotnet/OpenVDB.Points/PointAttribute.cs`
- [ ] `PointConversion.h` → `dotnet/OpenVDB.Points/PointConversion.cs`
- [ ] `PointCount.h` → `dotnet/OpenVDB.Points/PointCount.cs`
- [ ] `PointDelete.h` → `dotnet/OpenVDB.Points/PointDelete.cs`
- [ ] `PointGroup.h` → `dotnet/OpenVDB.Points/PointGroup.cs`
- [ ] `PointMask.h` → `dotnet/OpenVDB.Points/PointMask.cs`
- [ ] `PointMove.h` → `dotnet/OpenVDB.Points/PointMove.cs`
- [ ] `PointReplicate.h` → `dotnet/OpenVDB.Points/PointReplicate.cs`
- [ ] `PointSample.h` → `dotnet/OpenVDB.Points/PointSample.cs`
- [ ] `PointScatter.h` → `dotnet/OpenVDB.Points/PointScatter.cs`
- [ ] `PointTransfer.h` → `dotnet/OpenVDB.Points/PointTransfer.cs`

#### Sous-lot 8D : Rastérisation et statistiques (6 fichiers)
- [ ] `PointRasterizeFrustum.h` → `dotnet/OpenVDB.Points/PointRasterizeFrustum.cs`
- [ ] `PointRasterizeSDF.h` → `dotnet/OpenVDB.Points/PointRasterizeSDF.cs`
- [ ] `PointRasterizeTrilinear.h` → `dotnet/OpenVDB.Points/PointRasterizeTrilinear.cs`
- [ ] `PointStatistics.h` → `dotnet/OpenVDB.Points/PointStatistics.cs`
- [ ] `PrincipalComponentAnalysis.h` → `dotnet/OpenVDB.Points/PrincipalComponentAnalysis.cs`

#### Sous-lot 8E : Implémentations (16 fichiers)
- [ ] `impl/PointAttributeImpl.h` → `dotnet/OpenVDB.Points/Impl/PointAttributeImpl.cs`
- [ ] `impl/PointConversionImpl.h` → `dotnet/OpenVDB.Points/Impl/PointConversionImpl.cs`
- [ ] `impl/PointCountImpl.h` → `dotnet/OpenVDB.Points/Impl/PointCountImpl.cs`
- [ ] `impl/PointDeleteImpl.h` → `dotnet/OpenVDB.Points/Impl/PointDeleteImpl.cs`
- [ ] `impl/PointGroupImpl.h` → `dotnet/OpenVDB.Points/Impl/PointGroupImpl.cs`
- [ ] `impl/PointMaskImpl.h` → `dotnet/OpenVDB.Points/Impl/PointMaskImpl.cs`
- [ ] `impl/PointMoveImpl.h` → `dotnet/OpenVDB.Points/Impl/PointMoveImpl.cs`
- [ ] `impl/PointRasterizeEllipsoidsSDFImpl.h` → `dotnet/OpenVDB.Points/Impl/PointRasterizeEllipsoidsSDFImpl.cs`
- [ ] `impl/PointRasterizeFrustumImpl.h` → `dotnet/OpenVDB.Points/Impl/PointRasterizeFrustumImpl.cs`
- [ ] `impl/PointRasterizeSDFImpl.h` → `dotnet/OpenVDB.Points/Impl/PointRasterizeSDFImpl.cs`
- [ ] `impl/PointRasterizeTrilinearImpl.h` → `dotnet/OpenVDB.Points/Impl/PointRasterizeTrilinearImpl.cs`
- [ ] `impl/PointReplicateImpl.h` → `dotnet/OpenVDB.Points/Impl/PointReplicateImpl.cs`
- [ ] `impl/PointSampleImpl.h` → `dotnet/OpenVDB.Points/Impl/PointSampleImpl.cs`
- [ ] `impl/PointScatterImpl.h` → `dotnet/OpenVDB.Points/Impl/PointScatterImpl.cs`
- [ ] `impl/PointStatisticsImpl.h` → `dotnet/OpenVDB.Points/Impl/PointStatisticsImpl.cs`
- [ ] `impl/PrincipalComponentAnalysisImpl.h` → `dotnet/OpenVDB.Points/Impl/PrincipalComponentAnalysisImpl.cs`

---

## Récapitulatif des lots

| Lot | Module | Fichiers | Priorité | Dépendances |
|-----|--------|----------|----------|-------------|
| **Lot 1** | Fondations | 14 | 🔴 Critique | Aucune |
| **Lot 2** | Math | 28 | 🔴 Critique | Lot 1 |
| **Lot 3** | Utilitaires | 13 | 🔴 Critique | Lot 1 |
| **Lot 4** | Threading | 1 | 🟡 Haute | Lot 1, 3 |
| **Lot 5** | Tree | 13 | 🔴 Critique | Lots 1-4 |
| **Lot 6** | IO | 17 | 🟡 Haute | Lots 1-5 |
| **Lot 7** | Tools | 56 | 🟢 Moyenne | Lots 1-6 |
| **Lot 8** | Points | 47 | 🟢 Moyenne | Lots 1-6 |
| **TOTAL** | - | **189** | - | - |

---

## Ordre de portage recommandé

### Phase 1 : Infrastructure de base (56 fichiers - ~3-4 semaines)
1. **Lot 1** : Fondations (14 fichiers)
2. **Lot 2** : Mathématiques (28 fichiers)
3. **Lot 3** : Utilitaires (13 fichiers)
4. **Lot 4** : Threading (1 fichier)

**Objectif** : Créer l'infrastructure de base réutilisable par tous les modules.

### Phase 2 : Cœur du système (30 fichiers - ~2-3 semaines)
5. **Lot 5** : Tree (13 fichiers)
6. **Lot 6** : IO (17 fichiers)

**Objectif** : Implémenter le système d'arbre hiérarchique et la persistance.

### Phase 3 : Fonctionnalités avancées (103 fichiers - ~5-6 semaines)
7. **Lot 7** : Tools (56 fichiers)
8. **Lot 8** : Points (47 fichiers)

**Objectif** : Ajouter les algorithmes de manipulation et le système de points.

---

## Points d'attention pour le portage

### Adaptations C++ → C#

#### 1. Templates C++ → Génériques C#
```cpp
// C++
template<typename T>
class Grid { }
```
```csharp
// C#
public class Grid<T> where T : struct { }
```

#### 2. Pointeurs intelligents → Types référence
```cpp
// C++
using GridPtr = std::shared_ptr<Grid>;
```
```csharp
// C#
// Les classes C# sont déjà des types référence
public class Grid { }
```

#### 3. Namespaces et versions
```cpp
// C++
namespace openvdb {
OPENVDB_USE_VERSION_NAMESPACE
namespace OPENVDB_VERSION_NAME { }
}
```
```csharp
// C#
namespace OpenVDB.V11_0_0 { }
// ou simplement
namespace OpenVDB { }
```

#### 4. Macros de préprocesseur → Attributs/Constantes
```cpp
// C++
#define OPENVDB_VERSION_NAME v11_0_0
```
```csharp
// C#
public static class Version {
    public const string Name = "v11_0_0";
}
```

#### 5. Unsafe code pour performance
```csharp
// C# avec code unsafe si nécessaire
public unsafe void ProcessBuffer(byte* buffer, int length) {
    // Manipulation directe de la mémoire
}
```

#### 6. TBB (Threading Building Blocks) → Task Parallel Library
```cpp
// C++
tbb::parallel_for(range, operation);
```
```csharp
// C#
Parallel.For(start, end, i => operation(i));
```

### Conventions de nommage C#

| Élément | Convention C++ | Convention C# .NET |
|---------|----------------|-------------------|
| Classes | `PascalCase` | `PascalCase` ✓ |
| Méthodes | `camelCase` | `PascalCase` |
| Membres privés | `mVariableName` | `_variableName` ou `variableName` |
| Propriétés | - | `PascalCase` |
| Constantes | `UPPER_SNAKE_CASE` | `PascalCase` |
| Namespaces | `lowercase` | `PascalCase` |

### Gestion de la mémoire

- **C++** : Gestion manuelle, RAII, smart pointers
- **C#** : Garbage Collector, `IDisposable` pour ressources non managées

```csharp
public class Resource : IDisposable {
    private IntPtr _nativeHandle;
    
    public void Dispose() {
        if (_nativeHandle != IntPtr.Zero) {
            // Libérer ressources natives
            _nativeHandle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }
}
```

### Interopérabilité (si nécessaire)

Pour certaines parties critiques en performance, envisager :
- **P/Invoke** pour appeler du code C++ natif
- **C++/CLI** comme pont
- **Source Generators** pour générer du code

---

## Validation et tests

### Stratégie de test par lot

Chaque lot doit inclure :
1. **Tests unitaires** (xUnit/NUnit)
2. **Tests d'intégration** entre modules
3. **Benchmarks de performance** (BenchmarkDotNet)
4. **Tests de compatibilité** avec fichiers VDB existants

### Exemple de structure de tests

```
dotnet/
├── OpenVDB.Core.Tests/
│   ├── Math/
│   │   ├── Vec3Tests.cs
│   │   └── TransformTests.cs
│   ├── Tree/
│   │   └── TreeTests.cs
│   └── IO/
│       └── FileTests.cs
├── OpenVDB.Tools.Tests/
└── OpenVDB.Points.Tests/
```

---

## Métriques de suivi

### Par lot
- [ ] Nombre de fichiers portés / total
- [ ] Couverture de code des tests (cible : >80%)
- [ ] Performance relative au C++ (cible : >70%)
- [ ] Documentation XML complète

### Globale
- [ ] **189/189 fichiers portés** (100%)
- [ ] Solution compilable sans warnings
- [ ] Tous les tests passent
- [ ] Package NuGet publié

---

## Dépendances .NET potentielles

### Packages recommandés

```xml
<ItemGroup>
  <!-- Mathématiques haute performance -->
  <PackageReference Include="System.Numerics.Vectors" Version="4.5.0" />
  
  <!-- Compression (équivalent Blosc) -->
  <PackageReference Include="K4os.Compression.LZ4" Version="1.3.6" />
  
  <!-- Tests -->
  <PackageReference Include="xunit" Version="2.6.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  
  <!-- Benchmarks -->
  <PackageReference Include="BenchmarkDotNet" Version="0.13.10" />
  
  <!-- Logging -->
  <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
</ItemGroup>
```

---

## Livrables attendus

### Structure finale

```
dotnet/
├── OpenVDB.sln
├── OpenVDB.Core/
│   ├── OpenVDB.Core.csproj
│   └── [189 fichiers .cs organisés par module]
├── OpenVDB.Tools/
│   └── OpenVDB.Tools.csproj
├── OpenVDB.Points/
│   └── OpenVDB.Points.csproj
├── OpenVDB.Core.Tests/
├── OpenVDB.Tools.Tests/
├── OpenVDB.Points.Tests/
├── README.md
└── PORTING_NOTES.md
```

### Documentation

- [ ] README.md du projet .NET
- [ ] Guide de migration C++ → C#
- [ ] Documentation API (XML docs)
- [ ] Exemples d'utilisation
- [ ] Notes de performance

---

## Annexes

### Fichiers exclus

Les fichiers suivants sont **exclus** du portage :
- Tous les fichiers dans `/openvdb/openvdb/python/`
- Tous les fichiers dans `/openvdb/openvdb/unittest/`

### Ressources

- [Documentation OpenVDB C++](https://www.openvdb.org/documentation/)
- [Guide de style C# Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)

---

## Conclusion

Ce plan d'action définit une approche structurée et progressive pour porter les **189 fichiers** C++ d'OpenVDB vers C# .NET 9. Le portage est organisé en **8 lots** sur **3 phases**, avec une priorité donnée aux fondations et au cœur du système avant les fonctionnalités avancées.

**Durée estimée totale** : 10-13 semaines

**Prochaine étape** : Validation du plan et début du portage avec le Lot 1 (Fondations).
