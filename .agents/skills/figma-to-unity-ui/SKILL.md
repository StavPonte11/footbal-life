---
name: figma-to-unity-ui
description: Use when converting Figma designs, wireframes, and design tokens into Unity UI Toolkit (UXML/USS) or uGUI layouts using Figma MCP and modern UI practices.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Figma to Unity UI Pipeline

A streamlined workflow for extracting design tokens, component hierarchies, layout specifications, and assets from Figma via Figma MCP (`figma-developer-mcp`) and translating them cleanly into Unity UI Toolkit (UXML/USS) or uGUI.

## Workflow Steps

### 1. Inspect Figma File via MCP
- Call `get_figma_node` or `get_file_styles` to inspect frames, text styles, colors, and layout containers.
- Note Auto-Layout directions (Horizontal / Vertical) which translate 1:1 to flex-direction in UI Toolkit.

### 2. Extract Design Tokens
Map Figma color and typography styles to USS variables:
```css
/* Generated Design Tokens: ThemeTokens.uss */
:root {
    --color-bg-primary: #0F172A;
    --color-accent-green: #22C55E;
    --font-heading-size: 24px;
    --radius-button: 8px;
    --padding-container: 16px;
}
```

### 3. Translate Layout into UXML
- Auto-Layout Frame (Vertical) -> `<ui:VisualElement style="flex-direction: column;">`
- Auto-Layout Frame (Horizontal) -> `<ui:VisualElement style="flex-direction: row;">`
- Text Layer -> `<ui:Label text="..." class="heading-large" />`
- Button Frame -> `<ui:Button text="..." class="btn-primary" />`

### 4. Slice & Export Vector/Raster Assets
- Vector icons -> SVG or 9-slice Sprite pngs placed into `Assets/Art/UI/Sprites/`.
- Ensure sprites are imported with **Sprite (2D and UI)** texture type, Sprite Mode **Single** or **Multiple**, and appropriate mesh type.
