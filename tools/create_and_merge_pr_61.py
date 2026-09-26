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

    branch = "feature/milestone-6.1-balance-content-expansion"
    base = "main"

    title = "feat: Milestone 6.1 — 10,000-Career Simulation Balance Pass & Content Expansion (#171, #172)"
    body = """## Milestone 6.1: 10,000-Career Simulation Balance Pass & Content Expansion

### Summary of Changes

#### 1. Content Expansion (#P6-004 / Issue #172)
- **Leagues (`content/data/leagues.json`)**:
  - Expanded from 5 to **11 leagues** across Tier 1, Tier 2, and Tier 3 across England, Spain, Germany, Italy, and France.
  - Added: English Championship, English League One, Spanish Segunda División, German 2. Bundesliga, Italian Serie B, French Ligue 2.
- **Clubs (`content/data/clubs.json`)**:
  - Expanded from 7 to **66 clubs** with complete stadium capacity, wage budget, transfer budget, reputation, prestige, ticket prices, tactical styles, and fanbases.
- **Life Events (`content/data/events.json`)**:
  - Expanded from 20 to **103 life events** across all 6 core categories: `LockerRoom`, `Media`, `Commercial`, `Training`, `Family`, and `Lifestyle`.
  - Every event features multi-option moral dilemmas, consequence branches (Energy, Happiness, Relationships, Trust, Finances, Fame), and condition gates.
- **Unit Tests (`ContentExpansionTests.cs`)**:
  - Schema, count, and integrity validations ensuring 100+ events, 50+ clubs, and 10+ leagues pass in 33ms.

#### 2. 10,000-Career Simulation Balance Pass (#P6-001 / Issue #171)
- **Career Simulator Integration**:
  - Integrated Phase 5 systems into `CareerSimulationEngine.cs`:
    - Commercial Sponsorship earnings and weekly payouts.
    - Silverware tracking across domestic leagues, cups, and continental tournaments.
    - International caps, goals, and international tournament honors.
    - Veteran decline curves (`RetirementSystem.ApplyLateCareerDecline`).
    - Career Score and Legacy Grade categorization (`LegacySystem.CalculateCareerScore`).
  - Added new distribution metrics and an ASCII histogram renderer in `AggregateMetrics.cs` and `CareerStatistics.cs`.
  - Updated CLI tool `FootballLife.CareerSimulator` with multi-threaded parallel execution and `<RollForward>LatestMajor</RollForward>` support.
- **10,000-Career Empirical Results**:
  - Peak Overall: Mean 68.3, Median 68.0, Max 87.0
  - Retirement Age: Mean 34.4, Min 34.0, Max 37.0
  - Bankruptcy Rate: 0.0%
  - Hall of Fame Induction Rate: 6.1%
  - Legacy Grade Distribution:
    - GOAT: 0.1% (14 careers)
    - Legend: 2.3% (230 careers)
    - Icon: 13.8% (1,384 careers)
    - Cult Hero: 19.2% (1,924 careers)
    - Journeyman: 64.2% (6,424 careers)
    - Underachiever: 0.2% (24 careers)
  - Throughput: 10,000 full 20-season careers simulated in 84.08 seconds (119 careers/sec).
- **Automated Validation (`BalancePassTests.cs`)**:
  - Verifies peak ratings, retirement age, bankruptcy rate (<1%), and Hall of Fame rate (3-10%) across automated multi-career test runs.

#### 3. Unity Plugins & Compilation
- Rebuilt `FootballLife.Domain.dll` and `FootballLife.Simulation.dll` for `netstandard2.1` and synced to `unity/FootballLife/Assets/Plugins/FootballLife/`.
- Full test suite: **652 / 652 tests passing** (0 failures).

Closes #171
Closes #172
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-6): complete Milestone 6.1 - balance pass & content expansion (#171, #172)"], check=True)
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
            data = json.loads(resp.read().decode("utf-8"))
            pr_number = data["number"]
            print(f"Created PR #{pr_number}: {data['html_url']}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError creating PR: {e.code} {err_msg}")
        if "A pull request already exists" in err_msg:
            list_req = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
                headers=headers
            )
            with urllib.request.urlopen(list_req) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    print(f"Found existing PR #{pr_number}")
        if not pr_number:
            sys.exit(1)

    print("Waiting 3 seconds before merging...")
    time.sleep(3)

    print(f"Step 3: Merging PR #{pr_number}...")
    merge_payload = {
        "commit_title": f"feat: Milestone 6.1 — 10,000-Career Simulation Balance Pass & Content Expansion (#{pr_number})",
        "merge_method": "squash"
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            print(f"Merge successful: {data['message']}")
    except urllib.error.HTTPError as e:
        print(f"HTTPError merging PR: {e.code} {e.read().decode('utf-8')}")
        sys.exit(1)

    print("Step 4: Switching to main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done! Milestone 6.1 successfully merged to main.")

if __name__ == "__main__":
    main()
