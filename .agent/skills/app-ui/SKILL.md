---
name: app-ui
description: Use when building modern UI in Unity using App UI (com.unity.dt.app-ui) and UI Toolkit. Covers components, layout, styling, themes, MVVM, and UI architecture.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Unity App UI Expert

Expert guidance for building modern game and application interfaces using Unity's official **App UI** (`com.unity.dt.app-ui`) package and **UI Toolkit** in Unity 6+.

## Core Concepts

App UI builds on UI Toolkit to provide a comprehensive design system, modern architecture patterns, and responsive components:
- **Components**: Pre-built accessible UI widgets (Button, TextField, Dropdown, Modal, Drawer, Tabs, Slider, Toggle, Tooltip, Badge, Avatar, Icon).
- **MVVM & Data Binding**: Clean separation of UI layout (`.uxml`), styling (`.uss`), and business logic via ViewModels and ObservableProperties.
- **Theming & Design Tokens**: Design tokens mapped to USS variables for seamless light/dark mode and responsive scaling.
- **Navigation System**: Declarative routing with `NavGraph`, `NavHost`, and visual navigation controllers (`AppBar`, `Drawer`, `BottomNavBar`).

## Architecture & Best Practices

1. **Keep Layout in UXML**: Avoid constructing complex visual trees in C# code. Define structure in `.uxml` templates and bind values dynamically.
2. **Style with USS & Variables**: Utilize App UI's built-in CSS/USS variables for colors, typography, margins, and elevations rather than hardcoded pixel values:
   ```css
   .card {
       background-color: var(--unity-colors-surface-default);
       border-radius: var(--unity-metrics-radius-medium);
       padding: var(--unity-metrics-spacing-large);
   }
   ```
3. **Reactive Binding**: Use `ObservableProperty` and `RelayCommand` to bind UI elements to your game state without manual polling in `Update()`.
4. **Specialized Skills Router**:
   - For navigation graphs and screen routing, see `app-ui-navigation`.
   - For MVVM architecture and dependency injection, see `app-ui-mvvm`.
   - For styling, custom color palettes, and themes, see `app-ui-theming`.
   - For Redux-style unidirectional state flows, see `app-ui-redux`.
