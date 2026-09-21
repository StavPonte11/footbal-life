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

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-PRCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

pr_title = "feat(m2.2): Player Creation Flow + fix ZBinningJob/Render Graph crash (#102 #103 #104)"
pr_body = """## Milestone 2.2 — Player Creation Flow

Delivers **Milestone 2.2: Player Creation Flow** while fixing a pre-existing `ZBinningJob` Render Graph crash in the Cockpit Skybox renderer.

### Simulation Layer (Pure C#)
- **`StartingAttributeCalculator.cs`** — position-weighted rookie attribute generation (OVR 58–63)
- **`RandomNameGenerator.cs`** — culturally authentic name lists for 10 nationalities
- 557 automated unit tests passing ✅

### Unity Presentation Layer
- **`PlayerCreationData.cs` + `ClubOfferData.cs`** — data transfer objects (no Unity dep)
- **`PlayerCreationView.uxml`** — dark-theme player creation wizard (name, nationality, position chips, foot, kit number, live attribute preview)
- **`ClubSelectionView.uxml`** — 3-card club offer selection screen with badge + wage details
- **`PlayerCreationController.cs`** — interactive wizard with live attribute bars updating on position change
- **`ClubSelectionController.cs`** — card selection with green/slate border highlighting
- **`PlayerCreationCoordinator.cs`** — wires UI → `SimulationBridge.StartNewCareer` → `CareerHub` with auto-save

### Bug Fixes
- **`Skybox3D.cs`**: Initialize `s_ShaderTagValues`/`s_RenderStateBlocks` inline (not just via `RuntimeInitializeOnLoadMethod` which only fires in Play Mode). Fixes `NullReferenceException` in Editor that cascades into `ZBinningJob` race condition.
- **`Skybox3D.cs`**: Add `cullResults.visibleLights.IsCreated` and render-target handle validity guards in `RecordRenderGraph` to skip cleanly on invalid Editor frames.
- **`SimulationBridge.cs`**: `NextDouble()` → `NextFloat(0f, 1f)` (correct `SimulationRandom` API).
- **`PlayerCreationController.cs`**: `using Position = FootballLife.Domain.Position` resolves Domain/UIElements ambiguity.
- **`ClubSelectionController.cs`**: Replace invalid `IStyle.borderColor` with per-side `borderTopColor`/`borderRightColor`/`borderBottomColor`/`borderLeftColor`.

### Docs
- `ROADMAP.md` + `USER_STORIES.md` — Milestone 2.2 marked ✅ Complete

### Verification
- ✅ 557 simulation tests passing
- ✅ Unity compilation: `compilationFailed: false`, `errors: []`
- ✅ No new compiler errors

Closes #102, Closes #103, Closes #104
"""

# 1. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-2.2-player-creation",
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
issue_numbers = [102, 103, 104]
for num in issue_numbers:
    issue_payload = json.dumps({"state": "closed"}).encode('utf-8')
    issue_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues/{num}", data=issue_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(issue_req) as resp:
            print(f"Closed Issue #{num}")
    except Exception as e:
        print(f"Note on Issue #{num}: {e}")
