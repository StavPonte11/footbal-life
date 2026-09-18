---
name: context7
description: Use when looking up up-to-date documentation, API signatures, package guides, or code examples for Unity packages, C#, NuGet libraries, or external frameworks via Context7.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Context7 Documentation Retrieval

Guidance for using Context7 to fetch authoritative, version-accurate documentation for libraries, Unity packages, and C# APIs without hallucinating obsolete methods.

## Overview

Context7 indexes official documentation across thousands of ecosystems. When writing code involving:
- Unity 6 APIs (Render Graph, UI Toolkit runtime binding, Input System, Multiplayer Services)
- Third-party packages (UniTask, DOTween, Inkle Ink, Yarn Spinner, Odin, Netcode for GameObjects)
- C# standard libraries and NuGet packages

Use Context7 to look up exact namespaces, signatures, and patterns before writing boilerplate.

## Tool Invocation Patterns

When Context7 MCP is active:
- **`search_docs`**: Query library documentation by keyword or concept.
  - Query example: `"unity 6 render graph pass data execute"`
  - Query example: `"cuttlefish / unitask cancellation token source"`
- **`get_doc`**: Retrieve full documentation page or API reference for a specific symbol.

## CLI & Fallback Usage

If executing queries directly via command-line or REST:
```bash
# Query Context7 via npx
npx -y @upstash/context7-mcp search "Unity UI Toolkit data binding"
```
Or query direct documentation URLs if offline.
