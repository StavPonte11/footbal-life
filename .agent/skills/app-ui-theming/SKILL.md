---
name: app-ui-theming
description: Use when styling Unity UI Toolkit interfaces with App UI theming, custom color palettes, dark/light mode switching, scale factors, and USS variables.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# App UI Theming & Styling

Expert guidance on configuring themes, design tokens, scale contexts, and CSS/USS variables in Unity App UI.

## Theme Architecture

App UI uses hierarchical themes based on standard design tokens:
- **Color Palettes**: Primary, Secondary, Neutral, Success, Warning, Danger, Background, Surface.
- **Elevation / Shadows**: Depth layers for modals, cards, popovers, and floating toolbars.
- **Scale Contexts**: Responsive typography and component padding for mobile, tablet, and 4K desktop screens.

## Declaring Custom Theme Variables

Create a custom theme `.uss` file overriding standard variables:
```css
:root {
    /* Brand Accent Colors */
    --app-color-primary: #10B981;
    --app-color-primary-hover: #059669;
    --app-color-accent: #F59E0B;

    /* Pitch / Sports HUD Dark Theme */
    --unity-colors-surface-default: #1E293B;
    --unity-colors-surface-elevated: #334155;
    --unity-colors-text-primary: #F8FAFC;
    --unity-colors-text-secondary: #94A3B8;

    /* Geometry & Spacing */
    --unity-metrics-radius-card: 12px;
    --unity-metrics-spacing-edge: 16px;
}
```

## Runtime Theme & Dark Mode Switching

```csharp
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class ThemeController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    public void SetDarkMode(bool enabled)
    {
        var root = uiDocument.rootVisualElement;
        if (enabled)
        {
            root.AddToClassList("theme-dark");
            root.RemoveFromClassList("theme-light");
        }
        else
        {
            root.AddToClassList("theme-light");
            root.RemoveFromClassList("theme-dark");
        }
    }
}
```
