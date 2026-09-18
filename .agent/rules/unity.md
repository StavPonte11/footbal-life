---
trigger: always_on
---

# Unity Engine & Presentation Rules

## 1. Engine Target
- Target **Unity 6 (6000.2.0f1+)**.
- Render Pipeline: **Universal Render Pipeline (URP)** with Render Graph.
- Input: **New Input System** (`com.unity.inputsystem`).
- UI: **App UI** (`com.unity.dt.app-ui`) and **UI Toolkit** (`UnityEngine.UIElements`).

## 2. Zero GC in Hot Paths
- **Never allocate in `Update()`, `FixedUpdate()`, or `LateUpdate()`**:
  - No `new List<T>()`, `new GameObject()`, LINQ (`.Where()`, `.Select()`), or string concatenation.
  - Pre-cache component references in `Awake()` or `OnEnable()`.
  - Cache hashes: `Animator.StringToHash()` and `Shader.PropertyToID()`.
- Use `UnityEngine.Pool.ObjectPool<T>` for dynamic objects (particles, popups, floating labels, match events).
- Physics: Non-alloc queries only (`Physics.RaycastNonAlloc`, `Physics.OverlapSphereNonAlloc`).

## 3. Scene Hierarchy Standards
Every scene must organize GameObjects into top-level root categories:
- `[MANAGERS]` — Persistent singletons, telemetry, audio, game state integration.
- `[ENVIRONMENT]` — Pitch, stadium, floodlights, props, static geometry.
- `[ENTITIES]` — Players, ball, referees, interactive dynamic objects.
- `[CAMERAS]` — Virtual cameras, cinematic rigs, main camera.
- `[UI]` — UIDocuments, event system, floating world-space HUDs.

## 4. Mobile First
- Design primarily for touch inputs, short sessions (5-10 minutes), fast loading, and battery efficiency.
- Support automatic background saves.
