---
name: blender-unity-pipeline
description: Use when creating, rigging, texturing, or exporting 3D models and animations in Blender for Unity using Blender MCP (blend-ai / blender-ai-mcp).
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Blender to Unity Asset Pipeline

Comprehensive workflow for authoring 3D meshes, armatures, UVs, and materials in Blender via Blender MCP (`blend-ai` / `blender-ai-mcp`) and exporting them for seamless import into Unity.

## Scale, Coordinate & Origin Standards

1. **Units**: Set Blender scene units to **Metric**, Unit Scale **1.0** (1 Blender unit = 1 Unity meter).
2. **Forward & Up Axes**:
   - Blender: Z-Up, -Y Forward.
   - Unity: Y-Up, +Z Forward.
   - During export (FBX), use:
     - **Forward**: `-Z Forward`
     - **Up**: `Y Up`
     - Apply Transform: Check **! Apply Transform** (or Experimental Apply Transform) to prevent -90 degree X rotation on import in Unity.
3. **Pivots & Origins**:
   - Characters: Origin at floor level between feet `(0, 0, 0)`.
   - Props / Stadium elements: Origin at base/center for easy snap alignment.

## Armature & Humanoid Rigging Rules

- Ensure root bone is named `Hips` or `Root`.
- Standard humanoid hierarchy:
  - `Hips` -> `Spine` -> `Spine1` -> `Chest` -> `Neck` -> `Head`
  - `Chest` -> `Shoulder.L/R` -> `UpperArm.L/R` -> `LowerArm.L/R` -> `Hand.L/R`
  - `Hips` -> `UpperLeg.L/R` -> `LowerLeg.L/R` -> `Foot.L/R` -> `Toes.L/R`
- Always apply all transforms (`Ctrl+A -> All Transforms`) on both mesh and armature before binding or exporting.

## Automated FBX Export Script

Save export scripts into `tools/blender/` or execute via Blender MCP:
```python
import bpy

def export_fbx_to_unity(filepath):
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_space_transform=True,
        object_types={'ARMATURE', 'MESH'},
        mesh_smooth_type='FACE',
        add_leaf_bones=False,
        primary_bone_axis='Y',
        secondary_bone_axis='X'
    )
```
Target directory for exported assets: `Assets/Art/Models/` or `Assets/Art/Characters/`.
