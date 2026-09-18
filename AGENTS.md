# Football Life — Agent Constitution & Master Instructions

## 1. Project Overview

**Football Life** is a mobile-first football career and life simulation game inspired by New Star Soccer, expanded with:
- Modern 3D football gameplay situations
- Dynamic player position selection
- In-depth career progression & training
- Transfers, contract negotiations, finances
- Personal relationships, home lifestyle, fame, and media
- Emergent, long-term world simulation

> **Core Fantasy**: You don't manage a football club. You live the life of a footballer. The game creates emergent stories rather than following a predefined script.

---

## 2. Master Architecture: Two Strictly Separated Layers

```text
       ┌──────────────┐
       │    DOMAIN    │ (Pure C# models: Player, Club, Match, Contract, Item)
       └──────┬───────┘
              │ (Domain state)
       ┌──────▼───────┐
       │  SIMULATION  │ (Pure C# systems: Training, MatchSimulation, Transfers, LifeEvents)
       └──────┬───────┘
              │ (State snapshots & events)
       ┌──────▼───────┐
       │    UNITY     │ (Presentation: URP, UI Toolkit/App UI, 3D GameObjects, Physics)
       └──────────────┘
```

- **Domain**: Pure C# domain models (no `UnityEngine` dependencies).
- **Simulation**: Pure C# systems that modify domain state deterministically via `SimulationRandom(seed)`. Must be completely executable and testable without launching Unity.
- **Unity**: Presentation and interaction layer (GameObjects, UXML/USS, Input, Audio, VFX, Cameras). Translates user touch input into simulation actions and renders simulation state.
- **Dependency Rule**: `Domain` ↑ `Simulation` ↑ `Unity`. Never allow Domain or Simulation to depend on Unity.

---

## 3. Tooling & MCP Architecture

| Tool / MCP Server | Transport / Runtime | Purpose in Workspace |
|---|---|---|
| **Unity MCP** | `uvx --from mcpforunityserver mcp-for-unity --transport stdio` | Two-way bridge to active Unity Editor: create GameObjects, inspect scenes, run playmode tests, capture viewport. |
| **GitHub MCP** | Native binary (`tools/bin/github-mcp-server.exe`) | Issue tracking, automated PR review, commit analysis, GitHub Actions monitoring. |
| **Context7** | `npx -y @upstash/context7-mcp` | Up-to-date documentation lookup for Unity 6 APIs, C# 9+ language specs, and NuGet/UPM packages. |
| **Figma MCP** | `npx -y figma-developer-mcp` | Extracts Figma frames, design tokens, and converts layouts directly to Unity UI Toolkit (UXML/USS). |
| **Blender MCP** | `blend-ai` / `blender-ai-mcp` | 164 tools for 3D modeling, UVs, rigging, and automated FBX export directly into `Assets/Art/Models/`. |
| **Asset Pipeline** | Custom Importers & Skills | Automated FBX postprocessors, Humanoid retargeting, ASTC texture compression, audio streaming presets. |
| **OpenAI / LLM** | In-Engine REST Client | Generates narrative presentation (press conferences, match commentary) driven strictly by simulation state. |
| **Narrative** | Ink / Yarn / Dialogue Graph | Branching dialogue trees, moral choices, contract negotiations, locker-room talks. |
| **Telemetry** | Event Taxonomy & Manager | Session tracking, match telemetry, performance metrics, and privacy-compliant event batching. |

---

## 4. Installed Agent Skills (53 Skills Available)

All skills reside in [.agents/skills/](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/.agents/skills/) and are progressively loaded on demand:

### Core Game Design Skills (from Project Specs)
- `agent-development-workflow` — Task planning, small vs medium/large tasks, architecture preservation.
- `simulation-development` — Pure C# simulation systems, determinism, career simulation.
- `match-engine` — 3D match situations, opportunity generation, single-player agency vs simulation.
- `game-balance` — Economy curves, attribute scaling, progression pacing, difficulty balance.
- `game-content` — Life events, moral dilemmas, relationships, items, and narrative content.
- `ui-ux-development` — Mobile-first UI/UX flows, HUD layouts, touch interactions.
- `unity-gameplay-development` — Unity 3D gameplay, camera rigs, touch controls, physics presentation.
- `game-testing` — Multi-career simulation testing (`career-simulator --careers 10000`), deterministic test suites.
- `game-performance` — Zero GC allocations in hot paths, memory budgeting, mobile frame targets.

### Official Unity Technologies Skills (31 Skills)
- `new-unity-project`, `unity-cli`, `unity-package-management`, `build-live-game`
- `physics-3d-collision`, `initialize-ai-navigation`, `audio-setup-mixers`, `optimize-audio`
- `ui` router, `ui-uitk`, `ui-ugui`, `ui-imgui`
- `validate-urp-render-graph-renderer-feature`, `shader-graph-create-custom-node`, `urp-postprocessing`
- `setup-multiplayer-services`, `setup-vivox-voice-chat`, `implement-in-app-purchases`, `levelplay-unity-integration`
- `2d-pixel-perfect`, `tilemap-palette-create`, `manage-sprite-atlas`, `sprite-editor`, etc.

### Unity App UI Skills (5 Skills)
- `app-ui` — Core App UI components, layout, styling, and design system.
- `app-ui-navigation` — Declarative routing with `NavHost`, `NavGraph`, and navigation bars.
- `app-ui-mvvm` — Model-View-ViewModel, `ObservableProperty`, and `RelayCommand`.
- `app-ui-theming` — USS variables, themes, light/dark mode switching.
- `app-ui-redux` — Redux unidirectional state flows.

### Advanced Pipeline & Tooling Skills (8 Skills)
- `unity-mcp` — Live Unity Editor automation and scene inspection.
- `context7` — Live documentation lookup for Unity and C#.
- `figma-to-unity-ui` — Figma design token and layout conversion to UXML/USS.
- `blender-unity-pipeline` — 3D modeling, rigging, and FBX export to Unity.
- `asset-animation-pipeline` — Asset importers, blend trees, humanoid rigs, textures, audio.
- `llm-game-integration` — In-game OpenAI/Gemini REST client, dynamic match commentary, NPC dialogue.
- `narrative-engine` — Branching story trees, Ink/Yarn Spinner, ScriptableObject dialogue graphs.
- `game-telemetry` — Analytics taxonomy, session tracking, performance metrics.

---

## 5. Development Principles & Non-Negotiables

1. **Determinism**: Simulation systems must be deterministic given identical initial state, inputs, and `SimulationRandom(seed)`. No global uncontrolled randomness.
2. **Simulation Before Presentation**: Always implement the domain model, simulation system, and tests *before* writing Unity UI or 3D visuals.
3. **No God Objects**: Never build monolithic `GameManager` or `CareerManager` classes. Prefer small, decoupled systems (`TrainingSystem`, `TransferSystem`, `ContractSystem`).
4. **No Hard-Coded Content**: All clubs, player attributes, items, and events must be data-driven.
5. **Player Model**: Strictly separate long-term **Abilities** (finishing, pace, stamina, vision) from temporary **State** (confidence, fatigue, happiness, form, morale).
6. **LLM Authority Boundary**: LLMs may generate flavour text or commentary *from* simulation events, but must **never** make authoritative game mechanic decisions (e.g. deciding if a transfer happens).
7. **Mobile First**: Design for touch, short sessions, battery efficiency, fast loading, and automatic saves.
8. **Zero GC in Hot Paths**: No allocations (`new`, LINQ, string ops) in `Update()` / `FixedUpdate()`. Use `ObjectPool<T>` and hash lookups (`Animator.StringToHash`).

---

## 6. Development Workflow (Step-by-Step)

1. **Understand**: Read `AGENTS.md`, `ROADMAP.md`, relevant skills, and inspect existing code.
2. **Plan**: For non-trivial tasks, state Goal, Current Architecture, Proposed Changes, Files Affected, Tests, and Risks.
3. **Implement**: Smallest coherent change respecting architectural boundaries.
4. **Test & Validate**: Run unit tests, simulation validations, and verify Unity compilation/console.
5. **Report**: Concisely describe what changed, why, tests executed, and recommended next steps.
