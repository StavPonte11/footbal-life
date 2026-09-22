import os, sys, json, urllib.request, urllib.error, subprocess

def get_token():
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GITHUB_PERSONAL_ACCESS_TOKEN")
    if token: return token
    try:
        proc = subprocess.run(["git","credential","fill"], input="protocol=https\nhost=github.com\n",
                              text=True, capture_output=True, check=True)
        for line in proc.stdout.splitlines():
            if line.startswith("password="): return line.split("=",1)[1].strip()
    except: pass
    return None

def get_repo():
    try:
        proc = subprocess.run(["git","remote","get-url","origin"], text=True, capture_output=True, check=True)
        url = proc.stdout.strip()
        if "github.com" in url:
            parts = url.replace(":","/").split("github.com/")[-1].replace(".git","").split("/")
            if len(parts) >= 2: return f"{parts[0]}/{parts[1]}"
    except: pass
    return "StavPonte11/footbal-life"

token = get_token()
repo = get_repo()
if not token:
    print("Error: Could not obtain GitHub token.", file=sys.stderr); sys.exit(1)

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

issues = [
    {
        "title": "[P2-009] Home Screen (Daily Hub): date, form, energy, next match, primary actions",
        "labels": ["milestone-2.3", "phase-2", "unity-ui", "complexity-L"],
        "body": """## User Story
As a player, after signing my contract I arrive at the **Daily Hub** — the main screen I see every day of my career. I want to see today's key info at a glance and decide my next action.

## Acceptance Criteria
- [ ] Date header: Season N · Week N · Day (Mon/Tue/Wed/Thu/Fri/Sat/Sun)
- [ ] Player identity bar: name, position, club, overall rating badge
- [ ] Three stat meters: Energy, Form, Morale (with colour-coded fills)
- [ ] Manager Trust bar
- [ ] Finance row: weekly wage · bank balance
- [ ] Next Match card: opponent, competition, home/away, countdown days (hidden if no match this week)
- [ ] Status / news ticker: shows latest `OnStatusLog` or life event notification
- [ ] Four primary action buttons: Train · Rest · Match (disabled when no match) · View Career
- [ ] UI binds to `SimulationBridge.OnDayAdvanced` and refreshes all widgets reactively

## Architecture Notes
- View: `CareerHubView.uxml`
- Controller: `CareerHubController.cs` (no game logic — purely presentation/binding)
- Coordinator: integrated into `PlayerCreationCoordinator` → loads this screen after sign-contract
- Layer: Unity/UI only — reads from `SimulationBridge.CurrentSave` snapshot
"""
    },
    {
        "title": "[P2-010] Training Selection UI: categories, fatigue cost, XP preview",
        "labels": ["milestone-2.3", "phase-2", "unity-ui", "complexity-M"],
        "body": """## User Story
As a player, I want to open a Training panel showing available training categories with their energy cost and expected attribute gain. I pick one and confirm to apply it.

## Acceptance Criteria
- [ ] Modal/overlay panel `TrainingView.uxml` with title "Training Session"
- [ ] 4 training category cards: **Physical** (Pace/Stamina), **Technical** (Shooting/Dribbling), **Tactical** (Passing/Vision), **Goalkeeping** (GK only or reduced impact)
- [ ] Each card shows: category name, icon emoji, energy cost (intensity slider: Low 12/Med 24/High 36), expected attribute gain preview
- [ ] Confirm button calls `SimulationBridge.SelectWeeklyTraining(category, intensity)`
- [ ] Cancel/back button returns to Daily Hub without change
- [ ] If energy < 12, all options show "Too Fatigued" warning and confirm is disabled

## Architecture Notes
- View: `TrainingView.uxml`
- Controller: `TrainingController.cs`
- Opened from Daily Hub "Train" action button via panel swap or overlay
"""
    },
    {
        "title": "[P2-011] Rest & Recovery action panel",
        "labels": ["milestone-2.3", "phase-2", "unity-ui", "complexity-S"],
        "body": """## User Story
As a player, I want to spend a day resting so that my energy recovers, especially before a match.

## Acceptance Criteria
- [ ] Rest modal/overlay `RestView.uxml` with 2 options:
  - **Light Rest** — free, +20 Energy, no morale change
  - **Physio Session** — costs £150, +35 Energy, +5 Morale
- [ ] Calls `SimulationBridge.PerformRest("Light")` or `SimulationBridge.PerformRest("Physio")`
- [ ] Physio disabled if `BankBalance < 150`
- [ ] After confirming, view returns to Daily Hub with updated stats

## Architecture Notes
- View: `RestView.uxml`
- Controller: `RestController.cs`
"""
    },
    {
        "title": "[P2-012] Advance Day button: trigger simulation tick, refresh Daily Hub",
        "labels": ["milestone-2.3", "phase-2", "unity", "complexity-M"],
        "body": """## User Story
As a player, I want to click "Advance Day" to move time forward. The UI should update to show the new date, changed stats, and any triggered events (life event, match opportunity).

## Acceptance Criteria
- [ ] "Advance Day" button on Daily Hub calls `SimulationBridge.AdvanceDay()`
- [ ] All stat widgets refresh via `OnDayAdvanced` event binding
- [ ] If `OnMatchOpportunity` fires → show "Match Day!" banner and enable Match action button
- [ ] If `OnLifeEventOccurred` fires → show life event modal with choices (stubbed for now: just log the choice)
- [ ] Day label cycles Mon → Tue → ... → Sun → Mon (with week counter incrementing)
- [ ] Visual feedback: brief pulse/flash animation on the changed stat meters

## Architecture Notes
- Logic in `CareerHubController.cs` — subscribes to all `SimulationBridge` events
- Auto-scrolls status ticker to newest entry
- Life event dilemma panel: `LifeEventView.uxml` (minimal stub: title + description + choice buttons)
"""
    }
]

created = []
for issue in issues:
    payload = json.dumps({
        "title": issue["title"],
        "body": issue["body"],
        "labels": issue["labels"]
    }).encode("utf-8")
    req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues", data=payload, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            res = json.loads(resp.read().decode("utf-8"))
            print(f"Created #{res['number']}: {res['title']}")
            created.append(res['number'])
    except urllib.error.HTTPError as e:
        print(f"Error: {e.read().decode('utf-8')}")

print(f"\nCreated issues: {created}")
