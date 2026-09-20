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

# Ensure milestone label exists
required_labels = [
    {"name": "milestone:2.2", "color": "ededed", "description": "Milestone 2.2 - Player Creation Flow"}
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
        if e.code != 422:
            print(f"Note creating label {lbl['name']}: {e}")
    except Exception:
        pass

issues = [
    {
        "title": "[P2-006] Player creation screen: name, nationality, position, foot, appearance",
        "labels": ["layer:unity-ui", "complexity:L", "milestone:2.2", "phase-2", "type:feature"],
        "body": """## User Story
> As a player, I want an intuitive and stylish player creation screen where I can configure my footballer's identity (name, nationality, preferred position, preferred foot, and appearance preset) and preview starting baseline attributes so that I can establish my footballer persona.

## Description
Create the player creation visual layout and controller in UI Toolkit:
- `PlayerCreationView.uxml` and styling using App UI design tokens:
  - Input field for First and Last Name (with a 'Randomize' button).
  - Nationality dropdown / selector (England, France, Germany, Spain, Italy, Brazil, Argentina, etc.).
  - Position chip selector (GK, CB, FB, DM, CM, AM, LW, RW, ST) with dynamic attribute weights.
  - Preferred foot selector (Left, Right, Both).
  - Kit Number selector (1–99).
  - Live preview panel: displays baseline starting attributes calculated for the chosen position (e.g., Strikers highlight Shooting/Pace; Midfielders highlight Passing/Vision; Defenders highlight Tackling/Strength).
- Implement `PlayerCreationController.cs` in `FootballLife.Unity.UI`:
  - Form validation (name cannot be blank, position must be selected).
  - Dispatches validated creation payload to the club selection step.

## Acceptance Criteria
- [ ] `PlayerCreationView.uxml` created using dark sports theme tokens
- [ ] `PlayerCreationController.cs` managing interactive UI events and input validation
- [ ] Live attribute preview updates dynamically when position changes
- [ ] Random name generator utility supporting multiple nationalities
- [ ] Clean navigation flow to club selection screen
"""
    },
    {
        "title": "[P2-007] Starting club selection screen",
        "labels": ["layer:unity-ui", "complexity:M", "milestone:2.2", "phase-2", "type:feature"],
        "body": """## User Story
> As a player, I want to choose my starting rookie club from multiple realistic starter offers so that I can select my entry point into the professional football world based on club status, wage offer, and competition tier.

## Description
Build the rookie club offer selection UI:
- `ClubSelectionView.uxml` featuring:
  - 3 starter club offer cards representing lower-tier/entry professional clubs (e.g., League Two / National League clubs: Northfield Town, Bristol Rovers, Harrogate Town).
  - Club card info: Badge/Colors, Club Name, Division, Stadium, Starting Weekly Wage (e.g. £450 - £600/wk), Squad Role ("Youth Prospect"), Contract Length (2 years), and Initial Manager Trust expectation.
  - "Sign Contract" action button with confirmation state.
- Implement `ClubSelectionController.cs` in `FootballLife.Unity.UI`:
  - Loads starter club data.
  - Handles card selection, active card highlighting, and contract signing event.

## Acceptance Criteria
- [ ] `ClubSelectionView.uxml` created with interactive offer cards
- [ ] `ClubSelectionController.cs` populating dynamic club offer metadata
- [ ] Card selection states and visual highlighting
- [ ] Confirmed club choice proceeds to career initialization
"""
    },
    {
        "title": "[P2-008] Career initialization: wire player creation to simulation",
        "labels": ["layer:unity", "complexity:M", "milestone:2.2", "phase-2", "type:feature"],
        "body": """## User Story
> As a player, I want my created player and chosen starting club to initialize a new simulation session via `SimulationBridge` and save automatically so that I smoothly transition into my first week at the club in the CareerHub.

## Description
Connect the UI creation flow to the core simulation engine:
- Implement `CareerInitializationFlow.cs` orchestrating:
  - Collecting created player data and selected club.
  - Invoking `SimulationBridge.Instance.StartNewCareer(...)`.
  - Creating initial domain records (`Player`, `PlayerAbilities`, `PlayerState`, `Contract`, `WorldState`).
  - Triggering initial auto-save to ensure state is immediately persisted.
  - Using `SceneFlowManager` to transition cleanly from creation flow into `CareerHub`.
- Automated test coverage asserting career initialization state and save persistence.

## Acceptance Criteria
- [ ] End-to-end wiring from creation UI -> `SimulationBridge.StartNewCareer` -> `CareerHub`
- [ ] Immediate auto-save generated upon career start
- [ ] Starting condition initialized (Energy 100, Form 70, Morale 75, Manager Trust 50)
- [ ] Scene transition to `CareerHub` completes smoothly
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
