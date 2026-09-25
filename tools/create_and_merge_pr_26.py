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

branch_name = "feature/milestone-2.6-season-end-transfers"

# 1. Stage and commit git changes
print("Staging files and committing...")
subprocess.run(["git", "add", "."], check=True)
commit_msg = "feat(m2.6): End-of-Season Summary, Attribute Growth Visualization & Transfer Window (#122, #123, #124)"
subprocess.run(["git", "commit", "-m", commit_msg], check=True)

# 2. Push branch
print(f"Pushing {branch_name} to origin...")
subprocess.run(["git", "push", "-u", "origin", branch_name], check=True)

pr_title = "feat(m2.6): End-of-Season Summary, Attribute Growth Visualization & Transfer Window (#122, #123, #124)"
pr_body = """## Milestone 2.6 — End-of-Season & Transfers

Delivers **Milestone 2.6: End-of-Season & Transfers**, completing the full annual cycle from the 38-week season finale recap, through annual attribute progression and developmental curve feedback, to the summer transfer window featuring contract extension negotiations, suitor transfer bids, contract signing, and season rollover.

---

### Key Features & Architecture

#### 1. Season Summary Screen (#122 / #P2-018)
- **`SeasonSummaryView.uxml` + `SeasonSummaryController.cs`**:
  - **Campaign Achievements Card**: Displays final league standing (Champions & Promoted, Automatic Promotion, or Playoff Contender), club badge, and silver/gold trophy or medal achievement.
  - **Player Performance Card**: Aggregated 38-week record with total appearances, goals, assists, average match rating (e.g. 7.45 ★), and season honors (Golden Boot, Top Goalscorer, Breakthrough Prospect).
  - **Financial Year in Review Card**: 38-week salary credited, annual lifestyle expenses deducted, and net annual savings calculated.
  - **Flow Navigation**: Smooth transitions to Attribute Growth or direct return to Career Hub.

#### 2. Attribute Growth Visualization (#123 / #P2-019)
- **`AttributeGrowthView.uxml` + `AttributeGrowthController.cs`**:
  - **OVR Progression Card**: Start-of-season OVR vs End-of-season OVR with a vibrant green `+delta` badge.
  - **Development Curve Feedback**: Displays player age, age-gated developmental phase (e.g. `Rapid Youth Development (2.0x Multiplier)` vs `Peak Plateau`), and projected career potential ceiling.
  - **Categorized Attribute Breakdowns**:
    - *Physical*: Pace (+2), Stamina (+2), Strength (+1).
    - *Technical*: Finishing (+3), Passing (+2), Dribbling (+2).
    - *Mental*: Vision (+1), Positioning (+2), Composure (+1).
  - **Navigation**: "Proceed to Transfers →" and "← Back to Summary" back-navigation.

#### 3. Transfer Window & Contract Renewal Screen (#124 / #P2-020)
- **`TransferWindowView.uxml` + `TransferWindowController.cs`**:
  - **Current Contract Overview Card**: Current club, weekly wage, contract expiration year, estimated market valuation, and squad role (e.g. FIRST TEAM STARTER / PROSPECT).
  - **Club Extension Proposal**: Renewal offer from current club with +50% wage increase and loyalty signing bonus.
  - **Suitor Transfer Bids**: Multi-club bids (e.g. Southport Athletic in Division 3, Bristol Rovers) offering promotion, higher wages, signing bonuses, and squad roles.
  - **Contract Acceptance & Season Rollover**:
    - "Accept & Sign Contract": Credits signing bonus to bank balance, updates wage, club name, and contract length in `SimulationBridge`.
    - "Start Next Season": Advances season counter (`Season++`), resets week to `Week 1`, restores energy to 100, and returns to `CareerHub`.

#### 4. Architecture & Seamless Integration
- **Simulation Bridge**: Added `SeasonSummarySnapshot`, `AttributeGrowthSnapshot`, and `TransferOfferSnapshot` DTOs, plus deterministic data generation methods `GetSeasonSummaryData()`, `GetAttributeGrowthData()`, `GetTransferOffers()`, `AcceptTransferOffer()`, and `AdvanceToNextSeason()`.
- **Career Hub Coordinator & Daily Hub**: Added a dedicated `btn-offseason` button to `CareerHubView.uxml`, instantiated off-season overlays in `CareerHubCoordinator`, and wired bidirectional navigation.
- **Scene Setup Automation**: Updated `SceneSetupHelper.cs` to automatically assign off-season UXML assets to `CareerHubCoordinator` during automated setup.

---

### Verification & Testing
1. **Automated Unit Tests**:
   - `dotnet test simulation/FootballLife.Simulation.Tests`: All **557 tests pass** with 0 failures.
2. **Unity Compilation**:
   - Zero compilation errors across all assemblies (`FootballLife.Domain`, `FootballLife.Simulation`, `FootballLife.Unity.Core`, `FootballLife.Unity.UI`, `FootballLife.Unity.Editor`).
3. **Play Mode Verification via Unity MCP**:
   - Verified `CareerHubCoordinator.ShowSeasonSummary()` renders season achievements, badges, stats, and financials.
   - Verified `CareerHubCoordinator.ShowAttributeGrowth()` displays OVR deltas, potential ceiling, and youth development multiplier.
   - Verified `CareerHubCoordinator.ShowTransferWindow()` renders contract overview, renewal offer, and suitor bids.
   - Verified `AcceptTransferOffer()` transfers player to Southport Athletic, updates wage to £1,100/wk, and credits signing bonus.
   - Verified `AdvanceToNextSeason()` increments season to Season 2, Week 1, and resets energy to 100.

Closes #122
Closes #123
Closes #124
"""

pr_data = {
    "title": pr_title,
    "head": branch_name,
    "base": "main",
    "body": pr_body
}

print(f"Creating PR on {repo}...")
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
    "commit_title": f"feat(m2.6): End-of-Season Summary, Attribute Growth Visualization & Transfer Window (#{pr_number})",
    "commit_message": f"Delivers Milestone 2.6 (Issues #122, #123, #124).\n\nCloses #122\nCloses #123\nCloses #124",
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

# 6. Ensure issues #122, #123, #124 are closed
for issue_id in [122, 123, 124]:
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

print("\n🚀 Milestone 2.6 successfully delivered and merged into main!")
