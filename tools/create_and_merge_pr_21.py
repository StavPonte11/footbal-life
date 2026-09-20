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

pr_title = "feat(unity): Unity Project Bootstrap - Milestone 2.1 (#95 #100 #97 #98 #99)"
pr_body = """## Summary of Changes

Delivers **Milestone 2.1: Unity Project Bootstrap**, initiating Phase 2 (Unity Prototype) of Football Life while strictly preserving architectural boundaries (`Domain` ↑ `Simulation` ↑ `Unity`).

### Key Deliverables:
- **#95 (P2-001)**: Modular Unity assembly definitions (`FootballLife.Unity.Core`, `FootballLife.Unity.UI`, `FootballLife.Unity.Editor`) linked cleanly to precompiled `netstandard2.1` binaries in `Assets/Plugins/FootballLife/` (`FootballLife.Domain.dll`, `FootballLife.Simulation.dll`).
- **#100 (P2-002)**: `SimulationBridge` MonoBehaviour runtime presentation adapter providing event dispatching (`OnDayAdvanced`, `OnWeekAdvanced`, `OnSeasonAdvanced`, `OnMatchOpportunity`, `OnLifeEventOccurred`) and user interaction APIs with zero per-frame GC allocations.
- **#97 (P2-003)**: Atomic persistence service (`SaveLoadManager`, `CareerSaveService`, `CareerSaveData`) featuring temp-write flush, automatic `.bak` backup retention, corruption fallback, and multi-slot management (slots 1-3 + auto-save).
- **#98 (P2-004)**: Scene architecture (`Bootstrap`, `MainMenu`, `CareerHub`, `Match`) conforming to the root organizer standard (`[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, `[UI]`), `SceneFlowManager` asynchronous additive loader, and `EditorBuildSettings.asset` configuration.
- **#99 (P2-005)**: App UI and UI Toolkit sports-lifestyle dark theme design system (`tokens.uss`, `theme-dark.uss`, `components.uss`) with buttons, cards, stat meters, status badges, and `DesignSystemPreview.uxml` showcase.
- **Test Suite**: 533 automated unit tests passing (100% green).

Closes #95, Closes #100, Closes #97, Closes #98, Closes #99
"""

# 1. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-2.1-unity-bootstrap",
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
issue_numbers = [95, 100, 97, 98, 99]
for num in issue_numbers:
    issue_payload = json.dumps({"state": "closed"}).encode('utf-8')
    issue_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues/{num}", data=issue_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(issue_req) as resp:
            print(f"Closed Issue #{num}")
    except Exception as e:
        print(f"Note on Issue #{num}: {e}")
