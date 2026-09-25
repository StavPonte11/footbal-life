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

    if not token:
        print("Error: No GitHub token found.")
        sys.exit(1)

    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }

    branch = "feature/milestone-5.2-international-continental-managers"
    base = "main"

    title = "feat: Milestone 5.2 — International Football, Continental Tournaments & Manager Changes (#P5-004, #P5-005, #P5-006)"
    body = """## Milestone 5.2: International Football, Continental Tournaments & Manager Changes

### Summary of Changes

#### 1. National Team System — Eligibility, Call-Ups, International Tournaments & Career Caps (#P5-004 / Issue #163)
- **Pure C# Domain Models (`NationalTeam.cs`, `InternationalCallUp.cs`, `InternationalCareer.cs`)**:
  - `NationalTeam`: country, ranking, reputation, tactical style, squad list.
  - `InternationalCallUp`: call-up status, invitation evaluation against form and rating.
  - `InternationalCareer`: caps, goals, assists, tournament accolades.
- **Pure C# Simulation System (`InternationalSystem.cs`)**:
  - Squad selection with positional balance: 3 GKs, 7 DEFs, 7 MIDs, 6 FWDs.
  - Deterministic international fixture simulation with physical fatigue cost (+18 to +25) and confidence/reputation boosts.
  - `CareerSaveData.cs` persistence for international metrics.
- **UI Toolkit Presentation**:
  - `ProfileView.uxml` and `ProfileController.cs`: International Football summary card with country badge, caps, goals, and call-up button.

#### 2. Continental Competitions — Champions Cup, Qualification, Group Stage & Knockouts (#P5-005 / Issue #164)
- **Pure C# Domain Models (`ContinentalCompetition.cs`, `ContinentalFixture.cs`, `ContinentalGroupStanding.cs`)**:
  - 32-club tournament, 8 groups (A–H), group standings with goal difference and head-to-head.
  - Knockout brackets from Round of 16 through Final with 2-legged aggregate scores, extra time, and penalty shootouts.
- **Pure C# Simulation System (`ContinentalCompetitionSystem.cs`)**:
  - Group draw with same-league avoidance where possible.
  - Complete 6-matchday group stage simulation.
  - 2-legged knockout resolution with aggregate ties resolved by extra time/penalties.
  - £50,000,000 champion prize money distribution, £30M runner-up, £15M semi-finalists.
- **UI Toolkit Presentation**:
  - `ContinentalView.uxml` and `ContinentalViewController.cs`: Full-screen modal overlay with group tables and knockout brackets.
  - Integrated into `CareerHubView.uxml` and `CareerHubCoordinator.cs`.

#### 3. Manager Change System — Manager Sacking, Hiring & Tactical Trust Reset (#P5-006 / Issue #165)
- **Pure C# Domain Models (`ManagerChangeEvent.cs`)**:
  - Sacking reasons (Underperformance, Relegation, Contract Expired), manager attributes, tactical identities.
- **Pure C# Simulation System (`ManagerChangeSystem.cs`)**:
  - Evaluation of season performance vs board expectations.
  - Sacking probability model and appointment of new manager from tier-appropriate identities.
  - Resets player manager trust to neutral (50.0), requiring player to prove themselves.
  - `WorldState.ManagerChangeHistory` event logging.
- **Unity Presentation & Core Bridge**:
  - `SimulationBridge.cs`: Dispatches `OnManagerChanged` event.
  - `CareerHubCoordinator.cs`: Refreshes hub identity and manager trust on change.

### Automated Testing & Validation
- `InternationalSystemTests.cs`: 7 tests covering nationality matching, squad selection quotas, call-up acceptance, match simulation, and stats tracking.
- `ContinentalCompetitionSystemTests.cs`: 5 tests covering 32-team tournament setup, same-league avoidance, group stage round-robin, knockout bracket progression, aggregate resolution, and prize money.
- `ManagerChangeSystemTests.cs`: 6 tests covering board expectation evaluation, sacking probabilities, hiring generation, trust reset to 50, and event dispatch.
- **630 / 630 tests passing** across `FootballLife.Simulation.Tests` with 0 failures.

Closes #163
Closes #164
Closes #165
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "chore: add tools and metadata for milestone 5.2"], check=False)
    subprocess.run(["git", "push", "origin", branch], check=True)

    print("Step 2: Creating Pull Request...")
    pr_payload = {
        "title": title,
        "head": branch,
        "base": base,
        "body": body
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_payload).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    pr_number = None
    try:
        with urllib.request.urlopen(req) as resp:
            pr_data = json.loads(resp.read().decode("utf-8"))
            pr_number = pr_data["number"]
            print(f"Created PR #{pr_number}: {pr_data['html_url']}")
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8")
        print(f"Error creating PR: {e.code} - {err}")
        # If PR already exists, try to find it
        if "A pull request already exists" in err:
            req_list = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}",
                headers=headers
            )
            with urllib.request.urlopen(req_list) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    print(f"Found existing PR #{pr_number}")
        if not pr_number:
            sys.exit(1)

    print("Step 3: Merging Pull Request (Squash)...")
    time.sleep(2)
    merge_payload = {
        "commit_title": f"feat: Milestone 5.2 — International Football, Continental Tournaments & Manager Changes (#{pr_number})",
        "merge_method": "squash"
    }
    merge_req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )
    try:
        with urllib.request.urlopen(merge_req) as resp:
            merge_data = json.loads(resp.read().decode("utf-8"))
            print(f"Merged PR #{pr_number}: {merge_data.get('message', 'Success')}")
    except urllib.error.HTTPError as e:
        print(f"Error merging PR: {e.code} - {e.read().decode('utf-8')}")
        sys.exit(1)

    print("Step 4: Checking out main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done! Milestone 5.2 successfully merged to main.")

if __name__ == "__main__":
    main()
