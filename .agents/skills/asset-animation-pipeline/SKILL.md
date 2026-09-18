---
name: asset-animation-pipeline
description: Use when configuring Unity asset import settings, Animator Controllers, BlendTrees, humanoid avatar retargeting, sprite atlases, and audio compression.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Asset & Animation Pipeline Tooling

Expert procedures for configuring asset importers, Animator State Machines, blend trees, audio settings, and texture atlasing in Unity.

## 1. 3D Model & Animation Import Automation

Implement `AssetPostprocessor` in `Assets/Editor/Pipeline/ModelImportPipeline.cs` to ensure consistent imports:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ModelPostprocessor : AssetPostprocessor
{
    private void OnPreprocessModel()
    {
        ModelImporter importer = (ModelImporter)assetImporter;
        if (assetPath.Contains("/Characters/"))
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        }
        else if (assetPath.Contains("/Environment/") || assetPath.Contains("/Props/"))
        {
            importer.animationType = ModelImporterAnimationType.None;
            importer.generateSecondaryUV = true; // Lightmap UVs
            importer.isReadable = false;
        }
    }
}
#endif
```

## 2. Animation Controllers & BlendTrees

- **Locomotion BlendTrees**: Use 2D Freeform Directional or Cartesian BlendTrees for 8-way movement (Idle, Walk, Jog, Sprint, Strafe).
- **Parameters**: Use hashes `Animator.StringToHash("Speed")` instead of strings in update loops to eliminate string allocation.
- **Transitions**:
  - For immediate response (e.g. Kick, Tackle, Jump): Set `Has Exit Time = false`, `Transition Duration = 0.1s`.
  - For natural cycles: Set `Has Exit Time = true`.
- **Root Motion**: Explicitly choose between Root Motion (`applyRootMotion = true`) or code-driven movement (`applyRootMotion = false`) per character type.

## 3. Texture & Audio Compression Presets

- **Textures**:
  - UI Sprites: Default format, clamp wrap mode, disable MipMaps, ASTC 6x6 or RGBA32.
  - 3D Textures (Albedo/Normal): Generate MipMaps, repeat wrap mode, ASTC 4x4 / BC7.
  - Normal Maps: Must check **Texture Type: Normal Map**.
- **Audio**:
  - Long music / Ambience (> 10s): **Vorbis** or AAC, Load Type: **Streaming**, Decompress on Load: false.
  - Short SFX (Whistle, Kick, Click) (< 3s): **PCM** or **ADPCM**, Load Type: **Decompress On Load**.
