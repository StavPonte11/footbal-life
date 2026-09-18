---
trigger: always_on
---

# Unity 6 Architecture & Engine Standards

1. **Unity Version**: Target **Unity 6 (6000.2.0f1+)**. Utilize the latest engine features:
   - **Universal Render Pipeline (URP)**: Use Render Graph for custom render passes and renderer features.
   - **New Input System (`com.unity.inputsystem`)**: Use Input Action Assets and player-centric event callbacks instead of legacy `Input.GetKey`.
   - **UI Toolkit**: Prioritize UI Toolkit (`com.unity.dt.app-ui` / `UnityEngine.UIElements`) for in-game HUDs and menus over legacy IMGUI.
2. **Component & Hierarchy Structure**:
   - Every scene must have top-level organizer GameObjects:
     - `[MANAGERS]` - Persistent singletons, telemetry, audio, game state.
     - `[ENVIRONMENT]` - Pitch, stadium, lights, props, static geometry.
     - `[ENTITIES]` - Players, ball, referees, interactive dynamic objects.
     - `[CAMERAS]` - Virtual cameras, cinematic rigs, main camera.
     - `[UI]` - UIDocuments, event system, floating world-space HUDs.
3. **ScriptableObject Architecture**:
   - Keep game balance data (player stats, kit designs, tactics configurations, tournament schedules) in ScriptableObjects rather than hardcoding in MonoBehaviours.
   - Use event-driven ScriptableObjects (GameEvents) to decouple managers without tight coupling.
