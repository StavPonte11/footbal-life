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
            ["gh", "auth", "token"],
            text=True,
            capture_output=True,
            check=True
        )
        t = proc.stdout.strip()
        if t:
            return t
    except Exception:
        pass
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

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-PRCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

pr_title = "feat(m2.3): Daily Hub, Training, Rest & Advance Day (#106 #107 #108 #109)"
pr_body = """## Milestone 2.3 — Home Screen (Daily Hub)

Delivers **Milestone 2.3: Daily Hub**, bringing the central career loop to life with player vitals, training regimens, rest & recovery, life event dilemmas, and calendar advancement.

### Presentation Layer (Unity App UI & UI Toolkit)
- **`CareerHubView.uxml` + `CareerHubController.cs`**:
  - Date header with Season / Week / Day of week
  - Player identity bar with OVR badge and dynamic initials
  - 4 colour-coded stat meters: Energy (cyan), Form (green), Morale (yellow), Manager Trust (purple)
  - Finance indicators (weekly wage and current bank balance)
  - Next match card preview with home/away tag and competition details
  - Real-time status ticker updating on daily/weekly simulation events
  - Primary action buttons: Train, Rest, Match, View Career, and Advance Day
- **`TrainingView.uxml` + `TrainingController.cs`**:
  - 4 training category cards: Physical, Technical, Tactical, Goalkeeping
  - 3-level intensity selector (Low 12, Med 24, High 36 energy cost)
  - Fatigue threshold guard preventing training when energy < 12
  - Binds directly to `SimulationBridge.SelectWeeklyTraining`
- **`RestView.uxml` + `RestController.cs`**:
  - Light Rest (+20 Energy, free) vs Physio Session (+35 Energy, +5 Morale, £150)
  - Financial affordability check disabling Physio when funds are insufficient
  - Binds directly to `SimulationBridge.PerformRest`
- **`LifeEventView.uxml` + `LifeEventController.cs`**:
  - Modal dilemma overlay displaying event category, title, description, and dynamic choice buttons
- **`CareerHubCoordinator.cs`**:
  - Single coordinator orchestrating the base hub screen and modular overlays
- **`SceneSetupHelper.cs`**:
  - Automates full scene setup across Bootstrap, MainMenu, CareerHub, and Match scenes

### Platform & Dependency Fixes
- **Unity 6 Compatibility**:
  - Pinned `System.Text.Json` to 9.0.0 in `FootballLife.Domain.csproj` and `FootballLife.Simulation.csproj`
  - Shipped `Portable.System.DateTimeOnly.dll` (9.0.2) and `System.Text.Json.dll` (9.0.0) in `Assets/Plugins/FootballLife/`
  - Rebuilt Domain and Simulation DLLs

### Docs
- `ROADMAP.md` + `USER_STORIES.md` — Milestone 2.3 marked ✅ Complete, focus shifted to Milestone 2.4

### Verification
- ✅ Unity 6 batchmode compilation succeeded with zero compiler errors
- ✅ All 4 scenes registered and wired in Build Settings
- ✅ Closes issues #106, #107, #108, #109
"""

# 1. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-2.3-daily-hub",
    "base": "main",
    "body": pr_body
}).encode('utf-8')

req = urllib.request.Request(f"https://api.github.com/repos/{repo}/pulls", data=pr_payload, headers=headers, method="POST")
pr_num = None
try:
    with urllib.request.urlopen(req) as resp:
        res = json.loads(resp.read().decode('utf-8'))
        pr_num = res["number"]
        print(f"Created PR #{pr_num}: {res['html_url']}")
except urllib.error.HTTPError as e:
    err_msg = e.read().decode('utf-8')
    print(f"Error creating PR: {err_msg}")
    sys.exit(1)

time.sleep(2)

# 2. Merge PR (squash)
merge_payload = json.dumps({
    "merge_method": "squash",
    "commit_title": f"{pr_title} (#{pr_num})",
    "commit_message": f"Squash merge PR #{pr_num} into main."
}).encode('utf-8')

merge_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/pulls/{pr_num}/merge", data=merge_payload, headers=headers, method="PUT")
try:
    with urllib.request.urlopen(merge_req) as resp:
        res = json.loads(resp.read().decode('utf-8'))
        print(f"Merged PR #{pr_num}: {res.get('message', 'Merged successfully')}")
except urllib.error.HTTPError as e:
    err_msg = e.read().decode('utf-8')
    print(f"Error merging PR #{pr_num}: {err_msg}")

time.sleep(2)

# 3. Close issues explicitly if not closed automatically
issue_numbers = [106, 107, 108, 109]
for num in issue_numbers:
    issue_payload = json.dumps({"state": "closed"}).encode('utf-8')
    issue_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues/{num}", data=issue_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(issue_req) as resp:
            print(f"Closed Issue #{num}")
    except Exception as e:
        print(f"Note on Issue #{num}: {e}")
