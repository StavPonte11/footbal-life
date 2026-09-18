---
name: unity-development
description: Unity gameplay presentation, 3D controls, camera rigs, physics, and scene management for Football Life.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Unity Development Skill

## Role of Unity

Unity is the presentation and real-time gameplay environment.

It should consume the simulation rather than become the simulation.

## Unity owns

- GameObjects
- Scenes
- Prefabs
- Cameras
- Animators
- Input
- UI
- Audio
- VFX
- Rendering
- real-time physics

## Simulation owns

- player state
- career
- clubs
- matches
- relationships
- economy
- progression
- world state

## MonoBehaviours

Use MonoBehaviours as presentation/integration components.

Do not put business logic into giant MonoBehaviours.

## Architecture

Prefer:

```text
Simulation State
       ↓
Unity Adapter
       ↓
Presentation
```

rather than:

```text
Button
 ↓
GameManager
 ↓
random modifications everywhere
```

## Scenes

Keep scenes focused.

Avoid one enormous scene containing the entire game.

## Prefabs

Create reusable prefabs for:

- players
- UI components
- stadium elements
- interactive objects

## UI

Prefer reusable components.

Do not duplicate UI logic across screens.

## Performance

Do not optimize before profiling.

Avoid unnecessary:

- allocations in Update loops
- GetComponent calls in hot loops
- Find operations during gameplay
- expensive per-frame searches

Profile actual mobile hardware before major optimization work.