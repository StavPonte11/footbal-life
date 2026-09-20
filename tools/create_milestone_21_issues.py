import os
import sys
import json
import urllib.request
import urllib.error
import subprocess
import time

def get_token():
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GITHUB_PERSONAL_ACCESS_TOKEN")
    if token:
        return token
    try:
        proc = subprocess.run(
            ["git", "credential", "fill"],
            input="protocol=https\nhost=github.com\n",
            text=True,
            capture_output=True,
            check=True
        )
        for line in proc.stdout.splitlines():
            if line.startswith("password="):
                return line.split("=", 1)[1].strip()
    except Exception:
        pass
    return None

def get_repo():
    try:
        proc = subprocess.run(
            ["git", "remote", "get-url", "origin"],
            text=True,
            capture_output=True,
            check=True
        )
        url = proc.stdout.strip()
        if "github.com" in url:
            parts = url.replace(":", "/").split("github.com/")[-1].replace(".git", "").split("/")
            if len(parts) >= 2:
                return f"{parts[0]}/{parts[1]}"
    except Exception:
        pass
    return "StavPonte11/footbal-life"

token = get_token()
repo = get_repo()

if not token:
    print("Error: Could not obtain GitHub token.", file=sys.stderr)
    sys.exit(1)

print(f"Target Repository: {repo}")

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

# Ensure labels exist
required_labels = [
    {"name": "milestone:2.1", "color": "ededed", "description": "Milestone 2.1 - Unity Project Bootstrap"},
    {"name": "phase-2", "color": "1d76db", "description": "Phase 2 - Unity Prototype"},
    {"name": "layer:unity", "color": "5319e7", "description": "Unity Engine & Core Systems"},
    {"name": "layer:unity-ui", "color": "006b75", "description": "UI Toolkit / App UI Presentation"},
    {"name": "type:feature", "color": "a2eeef", "description": "New feature"}
]

for lbl in required_labels:
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/labels",
        data=json.dumps(lbl).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            pass
    except urllib.error.HTTPError as e:
        # 422 usually means label already exists
        if e.code != 422:
            print(f"Note creating label {lbl['name']}: {e}")
    except Exception:
        pass

issues = [
    {
        "title": "[P2-001] Initialize Unity 6 project & assembly definition setup",
        "labels": ["layer:unity", "complexity:M", "milestone:2.1", "phase-2", "type:feature"],
        "body": """## User Story
> As a gameplay programmer, I want a clean Unity 6 URP project with modular assembly definitions (`FootballLife.Unity.Core`, `FootballLife.Unity.UI`, `FootballLife.Unity.Editor`) linked to `FootballLife.Domain` and `FootballLife.Simulation` so that Unity presentation code compiles cleanly with strict separation from domain logic.

## Description
Set up the core assembly structure and project configuration for Unity 6:
- Verify and configure Universal Render Pipeline (URP), New Input System (`com.unity.inputsystem`), and UI Toolkit (`com.unity.modules.uielements`).
- Create modular assembly definitions (`.asmdef`):
  - `FootballLife.Unity.Core`: MonoBehaviours, simulation bridges, save/load, scene management, audio, input.
  - `FootballLife.Unity.UI`: UI Toolkit / App UI view controllers, USS theme definitions, visual components.
  - `FootballLife.Unity.Editor`: Custom inspectors, build helpers, and MCP automation tooling.
- Establish clean reference linkage to `FootballLife.Domain` and `FootballLife.Simulation` (compiled netstandard2.1 libraries placed in `Assets/Plugins/FootballLife/`) preserving the strict architectural rule: `Domain` ↑ `Simulation` ↑ `Unity`. Never allow Domain or Simulation to depend on Unity.
- Verify zero compilation errors or namespace conflicts in Unity.

## Acceptance Criteria
- [ ] Assembly definitions created: `FootballLife.Unity.Core.asmdef`, `FootballLife.Unity.UI.asmdef`, `FootballLife.Unity.Editor.asmdef`
- [ ] `FootballLife.Domain.dll` and `FootballLife.Simulation.dll` compiled and linked cleanly in `Assets/Plugins/FootballLife/`
- [ ] Unity 6 URP and New Input System active and configured
- [ ] Clean compilation with zero errors
"""
    },
    {
        "title": "[P2-002] Integration bridge: SimulationRuntime <-> Unity session",
        "labels": ["layer:unity", "complexity:L", "milestone:2.1", "phase-2", "type:feature"],
        "body": """## User Story
> As a gameplay programmer, I want a `SimulationBridge` MonoBehaviour that hosts the pure C# `CareerSimulationEngine` / `SimulationRuntime` and translates Unity time / UI actions into deterministic simulation ticks and dispatches C# events for presentation updates.

## Description
Build the foundational runtime adapter between the pure C# simulation engine and Unity's presentation layer:
- Implement `SimulationBridge` MonoBehaviour in `FootballLife.Unity.Core`:
  - Instantiates or receives `CareerSimulationEngine` and active career state (`CareerSaveData` / `PlayerProfile`).
  - Thread-safe and frame-safe event dispatcher firing Unity-facing events:
    - `OnDayAdvanced(SimulationDaySnapshot snapshot)`
    - `OnWeekAdvanced(SimulationWeekSnapshot snapshot)`
    - `OnSeasonAdvanced(SeasonRecord record)`
    - `OnMatchOpportunity(MatchOpportunity opportunity)`
    - `OnLifeEventOccurred(LifeEventInstance evt)`
  - Public interaction API:
    - `void AdvanceDay()`
    - `void SelectWeeklyRegimen(TrainingRegimen regimen)`
    - `void PerformRest(RestAction rest)`
    - `void RespondToEvent(LifeEventResponse response)`
- Adhere strictly to performance rules: zero GC allocations in frame update loops (`Update()`, `FixedUpdate()`).

## Acceptance Criteria
- [ ] `SimulationBridge` component created in `FootballLife.Unity.Core`
- [ ] Clean event dispatching for day, week, season, match, and life events
- [ ] Deterministic tick execution powered by pure C# `CareerSimulationEngine`
- [ ] Unit/Integration tests asserting bridge methods and event triggers
"""
    },
    {
        "title": "[P2-003] Save/load system: persist career state to disk",
        "labels": ["layer:unity", "complexity:L", "milestone:2.1", "phase-2", "type:feature"],
        "body": """## User Story
> As a player, I want my career state to be persisted to disk automatically and safely so that I can resume my career anytime across game sessions without data loss or corruption.

## Description
Implement a robust, atomic save/load manager in `FootballLife.Unity.Core`:
- Target location: `Application.persistentDataPath/saves/`
- Data contract: `CareerSaveData` record serializing player state, club state, season schedule, history, finances, relationships, and RNG state.
- Safety & Reliability:
  - Atomic file writing (write to `.tmp` file, flush, then atomic replace) to prevent corruption during unexpected app termination or power loss.
  - Backup retention (`.bak` file kept on each successful overwrite).
  - Version header (`SaveVersion`, `CreatedAt`, `GameVersion`) for backwards compatibility.
- Multi-slot support:
  - `SaveCareer(int slotIndex, CareerSaveData data)`
  - `CareerSaveData LoadCareer(int slotIndex)`
  - `IReadOnlyList<SaveSlotSummary> GetSaveSlots()`
  - `void DeleteCareer(int slotIndex)`

## Acceptance Criteria
- [ ] `SaveLoadManager` service implemented with atomic write guarantees
- [ ] Complete serialization/deserialization of career state
- [ ] Multi-slot management (slots 1–3 + auto-save slot)
- [ ] Tests verifying save, load, corruption resistance, and schema versioning
"""
    },
    {
        "title": "[P2-004] Scene architecture: Bootstrap, MainMenu, CareerHub, Match, Home",
        "labels": ["layer:unity", "complexity:M", "milestone:2.1", "phase-2", "type:feature"],
        "body": """## User Story
> As a player and developer, I want a structured scene architecture (`Bootstrap`, `MainMenu`, `CareerHub`, `Match`) with additive loading and a central `SceneFlowManager` so that transitions between screens are smooth, fast, and maintain persistent session state.

## Description
Establish scene workflow and lifecycle management:
- Standard scene hierarchy on all scenes:
  - `[MANAGERS]`
  - `[ENVIRONMENT]`
  - `[ENTITIES]`
  - `[CAMERAS]`
  - `[UI]`
- Scenes configured:
  - `Bootstrap.unity`: Game startup, services initialization, loads `MainMenu`.
  - `MainMenu.unity`: New game, load career, settings, credits.
  - `CareerHub.unity`: Main career loop screen (home, training, transfer, lifestyle).
  - `Match.unity`: 3D match situations and presentation.
- Implement `SceneFlowManager`:
  - Persistent singleton across scene transitions (`DontDestroyOnLoad`).
  - Additive asynchronous scene loading / unloading.
  - Screen transitions with smooth fade-to-black and loading progress.

## Acceptance Criteria
- [ ] Scenes created: `Bootstrap`, `MainMenu`, `CareerHub`, `Match`
- [ ] Standard root hierarchy applied to each scene
- [ ] `SceneFlowManager` implemented with asynchronous transition methods
- [ ] Build Settings updated with scene list
"""
    },
    {
        "title": "[P2-005] App UI design system: dark theme, typography, component library",
        "labels": ["layer:unity-ui", "complexity:L", "milestone:2.1", "phase-2", "type:feature"],
        "body": """## User Story
> As a mobile player, I want a premium sports-lifestyle dark UI theme with consistent typography, custom USS design tokens, and reusable component styles (stat meters, cards, badges, buttons) so that the game looks and feels like a modern mobile football career app.

## Description
Create a modern, sports-lifestyle UI Toolkit & App UI design system:
- USS Design Tokens (`theme-dark.uss`, `tokens.uss`):
  - Colors: Surface default (`#121826`), Surface elevated (`#1E293B`), Primary accent (`#10B981` pitch green), Secondary accent (`#F59E0B` gold), Danger (`#EF4444`), Text primary (`#F8FAFC`), Text secondary (`#94A3B8`).
  - Metrics: Radii (`8px`, `12px`, `16px`), Spacing (`4px`, `8px`, `16px`, `24px`, `32px`), Elevation / shadow variables.
- Reusable UI Components & Visual Styles:
  - `.btn-primary`, `.btn-secondary`, `.btn-ghost`
  - `.card-container`, `.card-header`, `.card-elevated`
  - `.stat-meter` (progress bar for Energy, Form, Morale, Stamina with color grading)
  - `.badge-status` (position tags, match result badges, transfer status)
  - `.app-header` and `.bottom-nav-bar` navigation containers
- Showcase / preview layout (`DesignSystemPreview.uxml`) demonstrating the complete design system in action.

## Acceptance Criteria
- [ ] USS token stylesheets (`theme-dark.uss`, `tokens.uss`, `components.uss`) created in `Assets/UI/Styles/`
- [ ] Reusable styles for buttons, cards, stat meters, and badges implemented
- [ ] Mobile-first responsive layout rules with proper safe-area padding
- [ ] `DesignSystemPreview.uxml` demonstrating components
"""
    }
]

url = f"https://api.github.com/repos/{repo}/issues"

for issue in issues:
    data = json.dumps(issue).encode("utf-8")
    req = urllib.request.Request(url, data=data, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            res_body = json.loads(resp.read().decode("utf-8"))
            print(f"Created Issue #{res_body['number']}: {res_body['title']}")
    except urllib.error.HTTPError as e:
        print(f"Failed to create issue '{issue['title']}': HTTP {e.code} - {e.read().decode('utf-8')}", file=sys.stderr)
    except Exception as e:
        print(f"Error creating issue '{issue['title']}': {e}", file=sys.stderr)
    time.sleep(1)
