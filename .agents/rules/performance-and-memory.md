---
trigger: always_on
---

# Performance & Memory Rules for Game Development

1. **Zero Garbage Collection (GC) in Hot Paths**:
   - **Never allocate in `Update()`, `FixedUpdate()`, or `LateUpdate()`**:
     - No `new List<T>()`, `new GameObject()`, or LINQ queries (`.Where()`, `.Select()`).
     - Cache strings and Animator parameters: use `Animator.StringToHash()` and `Shader.PropertyToID()`.
     - Cache component references in `Awake()` or `OnEnable()`. Never call `GetComponent<T>()` or `FindObjectOfType<T>()` in loop/frame updates.
2. **Object Pooling**:
   - Dynamic objects spawned frequently (e.g. particle effects, sound effects, projectiles, ball trail meshes, UI floating labels) must use Unity's built-in `UnityEngine.Pool.ObjectPool<T>`.
3. **Physics & Collision**:
   - Use `FixedUpdate()` strictly for physics interactions (Rigidbody forces, velocity manipulation).
   - Use non-alloc physics queries: `Physics.RaycastNonAlloc()`, `Physics.OverlapSphereNonAlloc()` with preallocated arrays.
   - Configure the Collision Matrix in Project Settings to disable checks between layers that never interact.
4. **Draw Calls & Batching**:
   - Combine static stadium/environment meshes or mark them **Static** for static batching / SRP Batcher.
   - Use GPU Instancing for stadium crowd, grass blades, and repeated assets.
