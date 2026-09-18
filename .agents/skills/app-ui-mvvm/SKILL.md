---
name: app-ui-mvvm
description: Use when implementing MVVM (Model-View-ViewModel) architecture, reactive property binding, commands, and dependency injection with Unity App UI.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# App UI MVVM Architecture

Architecture guide for decoupling business logic from visual representation using MVVM patterns in Unity App UI.

## Components of App UI MVVM

1. **Model**: Pure C# domain data and game state (e.g. `PlayerProfile`, `TeamStats`, `MatchEvent`).
2. **ViewModel**: Exposes state for the view and executes user commands. Inherits from `ObservableObject`:
   - Uses `[ObservableProperty]` to notify the view when state changes.
   - Uses `[RelayCommand]` for actions bound to buttons/inputs.
3. **View**: The UXML/USS hierarchy and binding context. Automatically synchronizes with the ViewModel via data binding.

## ViewModel Example

```csharp
using Unity.AppUI.MVVM;
using System.Threading.Tasks;

public partial class TeamTacticsViewModel : ObservableObject
{
    [ObservableProperty]
    private string teamName = "FC Strikers";

    [ObservableProperty]
    private int teamRating = 84;

    [ObservableProperty]
    private bool isSimulating = false;

    [RelayCommand]
    private async Task StartSimulationAsync()
    {
        IsSimulating = true;
        await Task.Delay(1000);
        TeamRating += 1;
        IsSimulating = false;
    }
}
```

## Dependency Injection & AppBuilder

Configure service lifetimes (Singletons, Transients) using `UIToolkitAppBuilder`:
```csharp
public class AppStartup : MonoBehaviour
{
    private void Awake()
    {
        var app = new UIToolkitAppBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<ITeamRepository, LocalTeamRepository>();
                services.AddTransient<TeamTacticsViewModel>();
            })
            .Build();
    }
}
```
