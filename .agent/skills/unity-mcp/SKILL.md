---
name: unity-mcp
description: Use when connecting to or controlling the Unity Editor via Model Context Protocol (MCP). Inspect GameObjects, execute Editor scripts, run tests, profile, and automate Unity workflows.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Unity MCP Integration Guide

This skill guides the AI agent and developer through controlling and querying the active Unity Editor via the Model Context Protocol (MCP).

## Two Unity MCP Ecosystems Supported

### 1. CoplayDev Unity MCP (`mcpforunityserver`)
- **Package**: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` (or OpenUPM `com.coplaydev.unity-mcp`)
- **Server command**: `uvx --from mcpforunityserver mcp-for-unity --transport stdio`
- **Capabilities**:
  - `create_game_object`, `delete_game_object`, `find_game_objects`
  - `add_component`, `get_components`, `modify_component`
  - `execute_csharp_script`, `run_menu_item`
  - `manage_scene`, `capture_scene_view`, `inspect_hierarchy`
  - `run_play_mode_tests`, `run_edit_mode_tests`
  - Profiler inspection and build execution

### 2. Unity Editor Gemini / Antigravity Native Integration
As documented in Google Android Developer guidance (`developer.android.com/games/engines/unity/unity-mcp-antigravity`):
1. In Unity Editor: Open **Integrations > Gemini**.
2. Click **Configure**, ensure the status is **Configured** (green) and **Unity Bridge** is **Running**.
3. Select desired tools under the **Tools** tab.
4. Copy the generated configuration snippet into `~/.gemini/config/mcp_config.json`.
5. In Antigravity: Open the Agent window > `...` > **MCP Servers > Manage MCP Servers** > Click **Refresh**.

## Editor Safety & Workflow Guidelines

- Always ensure Unity is in Edit Mode unless explicitly instructed to enter Play Mode.
- When generating GameObjects, parent them under logical root managers (e.g. `[MANAGERS]`, `[ENVIRONMENT]`, `[UI]`).
- Check compile errors in the Console before executing Editor operations or test runs.
- Use `AssetDatabase.SaveAssets()` and `AssetDatabase.Refresh()` after creating or modifying assets programmatically.
