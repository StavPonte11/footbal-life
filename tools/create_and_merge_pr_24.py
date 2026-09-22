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

# 1. Stage and commit git changes
print("Staging files and committing...")
subprocess.run(["git", "add", "."], check=True)
commit_msg = "feat(m2.4): Career Screen & Player Profile Screen (#115 #116)"
subprocess.run(["git", "commit", "-m", commit_msg], check=True)

# 2. Push branch
print("Pushing feature/milestone-2.4-career-profile to origin...")
subprocess.run(["git", "push", "-u", "origin", "feature/milestone-2.4-career-profile"], check=True)

pr_title = "feat(m2.4): Career Screen & Player Profile Screen (#115 #116)"
pr_body = """## Milestone 2.4 — Career Screen & Player Profile Screen

Delivers **Milestone 2.4: Career Screen & Player Profile Screen**, introducing deep career tracking, contract terms, manager trust status, and the player attributes profile.

### Presentation Layer (Unity App UI & UI Toolkit)
- **`CareerView.uxml` + `CareerController.cs` (#115)**:
  - Club & Squad Role Card: Club name, position, preferred foot, squad role badge, division, season/week.
  - Manager Trust Card: 0-100 rating with visual progress bar and status feedback (Reserve / Rotation / First Team Starter).
  - Contract & Finances Card: Weekly wage, contract expiry year, estimated market value, and lifestyle tier.
  - Career Statistics Card: Total appearances, goals, assists, and average match rating.
  - Reputation & Squad Standing Card: Morale impact and transfer status.
  - Pure presentation layer bound cleanly to `SimulationBridge.CurrentSave` / `CareerSaveData`.
- **`ProfileView.uxml` + `ProfileController.cs` (#116)**:
  - Identity Header: Player avatar initials, name, nationality, age, foot, and OVR badge.
  - Strict Architectural Separation: Dynamic temporary condition (Energy, Form, Morale, Trust) displayed distinctly from permanent abilities.
  - 15 Core Attributes organized into 3 category cards with visual 0-100 meters:
    - Physical: Pace, Acceleration, Stamina, Strength, Agility.
    - Technical: Shooting/Finishing, Passing, Dribbling, First Touch, Crossing, Tackling.
    - Mental: Vision, Composure, Positioning, Decision Making.
- **`CareerHubCoordinator.cs` Integration**:
  - Wired `_careerViewAsset` and `_profileViewAsset` serialized references.
  - Seamless navigation between Daily Hub, Career Overview, and Player Profile with interactive top tab bar.
  - Back button (`← Hub`) cleanly returns to Daily Hub.
- **Scene Automation (`SceneSetupHelper.cs`)**:
  - `FullSceneSetup.SetupAllScenesBatchmode()` ensures all UXML assets are serialized and linked to `CareerHub.unity`.
- **Tooling (`tools/unity_mcp_client.py`)**:
  - Python MCP client for FastMCP HTTP bridge with session ID management.

### Docs
- `ROADMAP.md` + `USER_STORIES.md` — Milestone 2.4 marked ✅ Complete, focus shifted to Milestone 2.5 (Match Preview & Basic Match).

### Verification
- ✅ Unity 6 compilation verified with zero compiler errors.
- ✅ Live Unity Play Mode verification:
  - Career view elements bound to Marcus Vance at Northfield Town (£500/wk wage, 50/100 trust, £50,000 market value).
  - Profile view elements bound with 100% Energy, 70% Form, 75% Morale, 50% Trust, 62 Pace, 59 Shooting, 55 Vision, 60 Composure.
- ✅ Closes issues #115 and #116.
"""

# 3. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-2.4-career-profile",
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

# 4. Merge PR (squash)
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

# 5. Switch to main and pull
print("Switching back to main and pulling...")
subprocess.run(["git", "checkout", "main"], check=True)
subprocess.run(["git", "pull", "origin", "main"], check=True)

# 6. Close issues explicitly if not closed automatically
issue_numbers = [115, 116]
for num in issue_numbers:
    issue_payload = json.dumps({"state": "closed"}).encode('utf-8')
    issue_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues/{num}", data=issue_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(issue_req) as resp:
            print(f"Closed Issue #{num}")
    except Exception as e:
        print(f"Note on Issue #{num}: {e}")

print("Milestone 2.4 delivery completed!")
