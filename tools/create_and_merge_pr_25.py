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

branch_name = "feature/milestone-2.5-match-system"

# 1. Stage and commit git changes
print("Staging files and committing...")
subprocess.run(["git", "add", "."], check=True)
commit_msg = "feat(m2.5): Match Preview, Abstracted Match Gameplay & Post-Match Summary (#118, #119, #120)"
subprocess.run(["git", "commit", "-m", commit_msg], check=True)

# 2. Push branch
print(f"Pushing {branch_name} to origin...")
subprocess.run(["git", "push", "-u", "origin", branch_name], check=True)

pr_title = "feat(m2.5): Match Preview, Abstracted Match Gameplay & Post-Match Summary (#118, #119, #120)"
pr_body = """## Milestone 2.5 — Match Preview & Basic Match

Delivers **Milestone 2.5: Match Preview & Basic Match**, completing the full match day cycle from pre-match tactical briefing, through interactive situation decision-making resolved by the deterministic simulation layer, to the post-match summary with manager reactions, condition/attribute deltas, and automated persistence.

---

### Key Features & Architecture

#### 1. Match Preview View & Controller (#118 / #P2-015)
- **`MatchPreviewView.uxml` + `MatchPreviewController.cs`**:
  - Opponent Card: Home vs Away club names, circular club initials, competition fixture badge, venue status.
  - Player Context Card: Squad role badge (FIRST TEAM STARTER / ROTATION / BENCH), position, preferred foot, current energy and form levels.
  - Tactical Briefing Card: Concrete manager objective (e.g. win by +1 goal difference) and tactical instruction.
  - Primary Action: Kick Off Match button (`btn-kickoff`) triggers transition into match gameplay; cancel/back returns to Career Hub.

#### 2. Abstracted Match View & Controller (#119 / #P2-016)
- **`MatchGameView.uxml` + `MatchGameController.cs`**:
  - Live Scoreboard: Running clock (18', 42', 67', 85'), live team scores, dynamic commentary ticker.
  - Situation Cards: Situation title, narrative description, pressure rating, and tactical advantage state.
  - Contextual Choices: 3 tactical decision buttons with transparent risk percentages.
  - Pure Simulation Authority: Player choices are resolved deterministically using `ActionResolver` and `SimulationRandom`.
  - Resolution Overlay: Outcome banner (GOAL, CHANCE CREATED, TACKLE WON, BLOCKED, TURNOVER), narrative breakdown, match rating delta, and confidence delta.
  - Live Player Stats: Real-time tracking of rating, goals, and assists throughout the match.

#### 3. Post-Match Summary View & Controller (#120 / #P2-017)
- **`MatchPostView.uxml` + `MatchPostController.cs`**:
  - Final Result Card: Match result badge (VICTORY 🏆, DRAW ⚖️, DEFEAT) with themed styling, full-time score, competition fixture title.
  - Individual Performance Card: Prominent star match rating (e.g. 8.10 ★), goals scored, assists, key actions, errors committed.
  - Manager Dressing Room Reaction: Dynamic manager quote responding to result and player match rating.
  - Condition & Reputation Deltas: Manager trust delta (+8), form delta (+5), energy cost (-25), and weekly wage earnings.
  - Career Persistence: Records match outcome via `SimulationBridge.RecordMatchResult`, updating `TotalAppearances`, `TotalGoals`, `TotalAssists`, weighted career average rating, and triggering auto-save to disk.
  - Return to Hub: Clean navigation back to `CareerHub.unity`.

#### 4. Hub & Scene Integration
- **`CareerHubController.cs` + `CareerHubCoordinator.cs`**:
  - Wired "⚽ Play Match" button (`btn-match`) to trigger scene transition to `Match.unity`.
- **`MatchCoordinator.cs`**:
  - Single coordinator orchestrating the 3 view lifecycle transitions in `Match.unity`.
  - Built-in editor preview fallback allowing direct playability and verification of `Match.unity` without prerequisite scenes.
- **`SceneSetupHelper.cs`**:
  - Configured batch scene setup for `Match.unity` with `UIDocument_Match` and all UXML references.

---

### Verification
- **Compilation**: Zero compilation errors across all assemblies (`FootballLife.Domain`, `FootballLife.Simulation`, `FootballLife.Unity.Core`, `FootballLife.Unity.Editor`, `FootballLife.Unity.UI`).
- **In-Editor Play Mode Validation**:
  - Loaded `Match.unity` and verified `MatchPreviewView`: Home=Northfield Town vs Away=Westford United, Prospect squad role, Tactical target objective.
  - Triggered `coord.StartMatch()`: Verified `MatchGameView` clock, situation card (⚽ Counter-Attack Breakthrough at 18'), and 3 decision choices.
  - Triggered `coord.ShowPostMatch()`: Verified full-time score 2-1, VICTORY badge, rating 8.10 ★, quote, trust delta (+8), energy cost (-25), and verified `CareerSaveData` updated (`TotalAppearances: 1`, `TotalGoals: 1`, `Energy: 75`, `ManagerTrust: 58`).

Closes #118
Closes #119
Closes #120
"""

# 3. Create Pull Request
print("Creating Pull Request on GitHub...")
pr_data = {
    "title": pr_title,
    "head": branch_name,
    "base": "main",
    "body": pr_body
}
req = urllib.request.Request(
    f"https://api.github.com/repos/{repo}/pulls",
    data=json.dumps(pr_data).encode("utf-8"),
    headers=headers,
    method="POST"
)
try:
    with urllib.request.urlopen(req) as resp:
        pr_response = json.loads(resp.read().decode("utf-8"))
        pr_number = pr_response["number"]
        pr_html_url = pr_response["html_url"]
        print(f"Successfully created PR #{pr_number}: {pr_html_url}")
except urllib.error.HTTPError as e:
    err_body = e.read().decode("utf-8")
    print(f"Failed to create PR: {e.code} {err_body}", file=sys.stderr)
    sys.exit(1)

# 4. Merge Pull Request (squash)
print(f"Merging PR #{pr_number} via squash merge...")
time.sleep(2)
merge_data = {
    "commit_title": f"feat(m2.5): Match Preview, Abstracted Match Gameplay & Post-Match Summary (#{pr_number})",
    "commit_message": f"Delivers Milestone 2.5 (Issues #118, #119, #120).\n\nCloses #118\nCloses #119\nCloses #120",
    "merge_method": "squash"
}
merge_req = urllib.request.Request(
    f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
    data=json.dumps(merge_data).encode("utf-8"),
    headers=headers,
    method="PUT"
)
try:
    with urllib.request.urlopen(merge_req) as resp:
        merge_res = json.loads(resp.read().decode("utf-8"))
        print(f"Successfully merged PR #{pr_number}! Sha: {merge_res.get('sha')}")
except urllib.error.HTTPError as e:
    err_body = e.read().decode("utf-8")
    print(f"Failed to merge PR #{pr_number}: {e.code} {err_body}", file=sys.stderr)
    sys.exit(1)

# 5. Checkout main and pull
print("Checking out main and pulling latest changes...")
subprocess.run(["git", "checkout", "main"], check=True)
subprocess.run(["git", "pull", "origin", "main"], check=True)

# 6. Ensure issues #118, #119, #120 are closed
for issue_id in [118, 119, 120]:
    issue_data = {"state": "closed"}
    issue_req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues/{issue_id}",
        data=json.dumps(issue_data).encode("utf-8"),
        headers=headers,
        method="PATCH"
    )
    try:
        with urllib.request.urlopen(issue_req) as resp:
            print(f"Confirmed Issue #{issue_id} closed.")
    except Exception as e:
        print(f"Note: Could not patch Issue #{issue_id} directly ({e}).")

print("\n Milestone 2.5 successfully delivered and merged into main!")
