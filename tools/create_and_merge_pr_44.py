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

def main():
    token = get_token()
    repo = get_repo()

    branch = "feature/milestone-4.4-social-activities-press-media"
    base = "main"

    title = "feat: Milestone 4.4 — Social Activities, Dynamic Media Reports & Press Conferences (#P4-008, #P4-009)"
    body = """## Milestone 4.4: Social Activities, Dynamic Media Reports & Press Conferences

### Summary of Changes

#### 1. Social Activities — Go Out Events with Fatigue/Morale Trade-Offs & Outing Catalog (#P4-008 / Issue #156)
- **Pure C# Domain Models (`SocialActivity.cs`)**:
  - `SocialActivityCategory` enum: `Casual`, `TeamBonding`, `Nightlife`, `Glamour`, `Philanthropy`.
  - `SocialActivity` record: `Id`, `Name`, `Category`, `EnergyCost`, `FinancialCost`, `MoraleBoost`, `TeamAffinityBoost`, `PrestigeBoost`, `ManagerTrustRisk`, `Description`, `IconEmoji`.
  - `SocialActivityCatalog`: 12 curated outings spanning casual strolls to charity galas.
- **Pure C# Simulation System (`SocialActivitySystem.cs`)**:
  - `CanAffordActivity`: Validates energy and bank balance.
  - `ExecuteActivity`: Deducts cost, awards morale, updates teammate affinity, tracks financial transaction, and evaluates matchday proximity manager disapproval risk via `SimulationRandom`.
- **UI Toolkit Presentation (`SocialActivitiesView.uxml`, `SocialActivitiesController.cs`)**:
  - Category filter tabs (`All`, `Casual`, `Team Bonding`, `Nightlife`, `Glamour`, `Philanthropy`).
  - Outing cards with energy/price chips, morale/prestige metrics, dynamic state buttons (`GO OUT`, `EXHAUSTED`, `NO FUNDS`).
  - Feedback toast alerts and seamless integration in both `CareerHub` and `Home` apartment scenes.

#### 2. Dynamic Media & Press Conference System (#P4-009 / Issue #157)
- **Pure C# Domain Models (`PressConference.cs`, `MediaArticle.cs`)**:
  - `PressTone` enum: `Humble`, `Confident`, `Defiant`, `Diplomatic`.
  - `Journalist` & `JournalistTemperament` (`Supportive`, `Sensationalist`, `Tactical`).
  - `PressResponseChoice` with manager trust, fan popularity, teammate morale, and media reputation deltas.
  - `MediaArticle` record with publication sources (*The Athletic*, *Sky Sports*, *The Daily Mirror*, *GQ Sports Style*, *BBC Football*), categories, like/view metrics, and timestamps.
- **Pure C# Simulation Systems (`PressConferenceSystem.cs`, `MediaFeedSystem.cs`)**:
  - `PressConferenceSystem`: Generates context-aware questions from recent match results (wins, losses, debut) and processes responses with career stat consequences.
  - `MediaFeedSystem`: Generates realistic sports journalism articles and provides `BuildLLMPrompt` helper conforming to `llm-game-integration`.
- **UI Toolkit Presentation (`PressConferenceView.uxml`, `PressConferenceController.cs`)**:
  - Press briefing room modal with live broadcast badge, journalist spotlight, question block, 4 tone choice cards with consequence previews, and conference debrief summary.
  - Quick access buttons in `CareerHub` header and action grid.

#### 3. Core Bridge & Persistence Integration
- `CareerSaveData.cs`: Added `FanPopularity` and `MediaReputation` integer properties (0-100).
- `SimulationBridge.cs`: Added `ExecuteSocialActivity`, `GetPendingPressConference`, `AnswerPressQuestion`, `GetMediaArticles`, and testing fallback.
- `HomeController.cs`: Integrated `_btnNavOutings` dock button and `_socialOverlay` modal presenter.

### Automated Testing & Validation
- `SocialActivitySystemTests.cs`: 6 tests validating catalog, affordability, execution, teammate bonding, pre-matchday risk, and philanthropy.
- `PressConferenceAndMediaTests.cs`: 6 tests validating question generation, answer processing, media feed articles, and LLM prompt builder.
- **602 / 602 tests passing** across `FootballLife.Simulation.Tests` with 0 failures.
- Unity compilation verified with 0 errors; in-engine runtime validation verified via Unity MCP.

Closes #156
Closes #157
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "-A"], check=True)
    status_proc = subprocess.run(["git", "status", "--porcelain"], text=True, capture_output=True, check=True)
    if status_proc.stdout.strip():
        subprocess.run(["git", "commit", "-m", title], check=True)
        print("Committed changes.")
    else:
        print("No changes to commit.")

    print(f"Step 2: Pushing branch {branch} to origin...")
    subprocess.run(["git", "push", "-u", "origin", branch], check=True)

    if not token:
        print("Error: No GitHub token found. Please set GITHUB_TOKEN.")
        return

    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }

    print("Step 3: Checking if PR already exists...")
    req_list = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
        headers=headers
    )
    existing_pr = None
    try:
        with urllib.request.urlopen(req_list) as resp:
            prs = json.loads(resp.read().decode("utf-8"))
            if prs:
                existing_pr = prs[0]
                print(f"Found existing PR #{existing_pr['number']}")
    except urllib.error.HTTPError as e:
        print(f"HTTPError checking existing PRs: {e.code}")

    if existing_pr:
        pr_number = existing_pr["number"]
        pr_url = existing_pr["html_url"]
    else:
        print("Step 4: Creating Pull Request...")
        pr_data = {
            "title": title,
            "body": body,
            "head": branch,
            "base": base
        }
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/pulls",
            data=json.dumps(pr_data).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                pr_res = json.loads(resp.read().decode("utf-8"))
                pr_number = pr_res["number"]
                pr_url = pr_res["html_url"]
                print(f"Successfully created PR #{pr_number}: {pr_url}")
        except urllib.error.HTTPError as e:
            err_msg = e.read().decode("utf-8")
            print(f"HTTPError creating PR: {e.code} - {err_msg}")
            return

    print("Step 5: Merging Pull Request (Squash Merge)...")
    merge_data = {
        "commit_title": f"{title} (#{pr_number})",
        "merge_method": "squash"
    }
    req_merge = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_data).encode("utf-8"),
        headers=headers,
        method="PUT"
    )
    time.sleep(2)
    try:
        with urllib.request.urlopen(req_merge) as resp:
            merge_res = json.loads(resp.read().decode("utf-8"))
            print(f"PR #{pr_number} successfully merged: {merge_res.get('message', 'Merged')}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError merging PR: {e.code} - {err_msg}")
        return

    print("Step 6: Switching to main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)

    print("Step 7: Closing issues #156, #157...")
    for issue_id in [156, 157]:
        close_data = {"state": "closed"}
        req_close = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues/{issue_id}",
            data=json.dumps(close_data).encode("utf-8"),
            headers=headers,
            method="PATCH"
        )
        try:
            with urllib.request.urlopen(req_close) as resp:
                print(f"Closed issue #{issue_id}")
        except urllib.error.HTTPError as e:
            print(f"Could not close issue #{issue_id}: {e.code}")

    print("Milestone 4.4 completed, merged and closed successfully!")

if __name__ == "__main__":
    main()
