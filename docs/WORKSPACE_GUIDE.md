# Football Life — Workspace & AI Tooling Guide

This workspace is specifically optimized for game development with Google Antigravity, Unity 6, and modern AI coding agents. It brings together Model Context Protocol (MCP) servers, official Unity skills, design tooling, 3D pipelines, narrative tooling, and telemetry.

---

## 1. Quick Start / Verification

Run the automated workspace validation script from PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File .\tools\setup-workspace.ps1
```
This verifies your Unity installation, Git LFS hooks, Python/uvx runtime, native GitHub MCP binary, and all 44 installed agent skills.

---

## 2. MCP Servers Setup & Configuration

The workspace includes configured MCP servers defined in [.agents/plugins/unity-game-dev-kit/mcp_config.json](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/.agents/plugins/unity-game-dev-kit/mcp_config.json) and [tools/mcp/mcp_config.template.json](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/tools/mcp/mcp_config.template.json).

### A. Unity MCP Server
Bridges Antigravity directly to your running Unity Editor, allowing the agent to create GameObjects, inspect hierarchies, run PlayMode/EditMode tests, and capture the scene viewport.

1. **Unity Package**: In Unity (Window > Package Manager > Add package from git URL):
   ```
   https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
   ```
   *(Or via OpenUPM: `openupm add com.coplaydev.unity-mcp`)*
2. **Server Execution**: Runs via `uvx`:
   ```bash
   uvx --from mcpforunityserver mcp-for-unity --transport stdio
   ```
3. **Editor Menu**: Open `Window > MCP for Unity > Configure All Detected Clients`.
4. **Android / Google Antigravity Bridge Alternative**:
   - Follow [Android Unity MCP Guide](https://developer.android.com/games/engines/unity/unity-mcp-antigravity).
   - In Unity: **Integrations > Gemini > Configure**.

### B. GitHub MCP Server
A native Windows binary (`github-mcp-server.exe`) is pre-installed in [tools/bin/github-mcp-server.exe](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/tools/bin/github-mcp-server.exe).
- **Capabilities**: Issue triage, automated PR code review, commit history analysis, repository search, and GitHub Actions status.
- **Authentication**: Set your personal access token in your environment:
  ```powershell
  [System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', 'ghp_your_token_here', 'User')
  ```

### C. Context7 MCP Server
Integrated via `@upstash/context7-mcp`.
- **Capabilities**: Instant, hallucination-free documentation retrieval for Unity 6 APIs, C# 9+ language specs, and NuGet/UPM packages.
- **Skill**: Use the `context7` skill whenever you need precise method signatures for Unity packages.

### D. Figma MCP Server
Integrated via `figma-developer-mcp`.
- **Capabilities**: Reads Figma frames, extracts design tokens (colors, font sizes, margins), and converts UI wireframes directly into Unity UI Toolkit (UXML/USS).
- **Authentication**: Set your token:
  ```powershell
  [System.Environment]::SetEnvironmentVariable('FIGMA_ACCESS_TOKEN', 'figd_your_token_here', 'User')
  ```
- **Skill**: See `figma-to-unity-ui`.

### E. Blender MCP Server
Integrated via `blend-ai` / `blender-ai-mcp`.
- **Capabilities**: 164 tools for 3D modeling, UV unwrapping, material graph authoring, rigging, and FBX export directly to `Assets/Art/Models/`.
- **Addon**: Install the Blender addon from `HoldMyBeer-gg/blend-ai` or `PatrykIti/blender-ai-mcp`. In Blender N-panel: click **Start Server** (default port: `9876`).
- **Skill**: See `blender-unity-pipeline`.

---

## 3. Installed Agent Skills Catalog (44 Skills)

All skills reside in [.agents/skills/](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/.agents/skills/) and are loaded progressively by Antigravity:

### Official Unity Technologies Skills
- `new-unity-project` — Guided flow to scaffold a blank Unity game project from concept to packages.
- `unity-cli` — Bootstrap projects, install editors, execute builds, run headless tests.
- `unity-package-management` — Headless and C# PackageManager package installation.
- `physics-3d-collision` — 3D physics, collision matrices, raycasts, and layer optimization.
- `initialize-ai-navigation` — Unity NavMesh, agents, obstacles, and AI navigation.
- `audio-setup-mixers` & `optimize-audio` — Audio mixers, snapshots, sound compression.
- `ui`, `ui-uitk`, `ui-ugui`, `ui-imgui` — Comprehensive UI system authoring.
- `validate-urp-render-graph-renderer-feature` — Modern Unity 6 URP Render Graph features.
- `shader-graph-create-custom-node` — Custom HLSL nodes in Shader Graph.
- `setup-multiplayer-services` & `setup-vivox-voice-chat` — Unity Gaming Services multiplayer.
- `build-live-game`, `implement-in-app-purchases`, `levelplay-unity-integration`, `localization`.
- `2d-pixel-perfect`, `tilemap-palette-create`, `manage-sprite-atlas`, `sprite-editor`.

### Unity App UI Skills
- `app-ui` — Core App UI components, layout, styling, and design system.
- `app-ui-navigation` — Declarative routing with `NavHost`, `NavGraph`, and navigation bars.
- `app-ui-mvvm` — Model-View-ViewModel, `ObservableProperty`, and `RelayCommand`.
- `app-ui-theming` — USS variables, themes, light/dark mode switching.
- `app-ui-redux` — Predictable state management with Redux slices and reducers.

### Game Dev Pipeline & Specialized Skills
- `unity-mcp` — Control and query the Unity Editor live.
- `context7` — Live documentation lookup for Unity and C#.
- `figma-to-unity-ui` — Figma design token and layout conversion to UXML/USS.
- `blender-unity-pipeline` — 3D modeling, rigging, and FBX export to Unity.
- `asset-animation-pipeline` — Asset importers, blend trees, humanoid rigs, textures, audio.
- `llm-game-integration` — In-game OpenAI/Gemini REST client, dynamic match commentary, NPC dialogue.
- `narrative-engine` — Branching story trees, Ink/Yarn Spinner, ScriptableObject dialogue graphs.
- `game-telemetry` — Analytics taxonomy, session tracking, performance metrics.

---

## 4. Architectural Rules & Best Practices

All rules are active under [.agents/rules/](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/.agents/rules/) and summarized in [AGENTS.md](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/AGENTS.md):
1. **Zero Garbage Collection in Hot Paths**: No allocations in `Update()`, `FixedUpdate()`, or `LateUpdate()`. Pre-cache components in `Awake()`. Use `ObjectPool<T>`.
2. **Hierarchy Discipline**: Maintain `[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, and `[UI]` in every scene.
3. **ScriptableObject Data Separation**: Decouple logic from balance data.
4. **Git LFS**: Track all 3D models (`.fbx`, `.blend`), textures (`.png`, `.tga`), audio (`.wav`, `.mp3`), and video via [.gitattributes](file:///C:/Users/User/Desktop/Stav/projects/footbal-life/.gitattributes).
