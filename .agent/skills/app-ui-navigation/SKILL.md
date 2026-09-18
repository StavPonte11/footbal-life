---
name: app-ui-navigation
description: Use when building multi-screen navigation, routing, modal flows, and visual navigation bars (AppBar, Drawer, BottomNavBar) using Unity App UI.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# App UI Navigation System

Deep guidance on configuring declarative navigation in Unity using App UI's `NavGraph`, `NavHost`, and navigation controllers.

## Core Concepts

1. **NavHost**: The container element hosting the current destination screen.
2. **NavGraph**: The route definition map defining destinations, parameters, transitions, and nested subgraphs.
3. **NavController**: The programmatic controller used to push, pop, or replace routes:
   ```csharp
   navController.Navigate("match_details", new RouteParams { { "matchId", 42 } });
   navController.PopBackStack();
   ```
4. **Visual Controllers**:
   - `AppBar`: Top toolbar with title, navigation back-button, and context action slots.
   - `BottomNavBar`: Mobile/tabbed bottom bar linked to root navigation destinations.
   - `Drawer`: Side navigation drawer for menus and secondary routes.
   - `NavigationRail`: Compact side rail for desktop / widescreen layouts.

## Implementation Pattern

```xml
<!-- In MainLayout.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:app="Unity.AppUI.UI">
    <app:NavHost name="main-nav-host" initialRoute="home">
        <!-- Sub-screens injected dynamically by NavController -->
    </app:NavHost>
</ui:UXML>
```

```csharp
public class MainNavigationController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private NavController navController;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        var navHost = root.Q<NavHost>("main-nav-host");
        navController = navHost.navController;

        navController.RegisterDestination("home", () => new HomeScreenView());
        navController.RegisterDestination("settings", () => new SettingsScreenView());
        navController.RegisterDestination("match", (args) => new MatchScreenView(args));
    }
}
```
