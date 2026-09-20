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

print(f"Target Repository: {repo}")

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

issues = [
    {
        "title": "[P1-066] CareerSimulator: End-to-End Multi-Season Career Runner",
        "labels": ["layer:career-simulator", "complexity:L", "milestone:1.11", "type:feature"],
        "body": """## User Story
> As a game designer and developer, I want a headless career runner that wires together all Phase 1 simulation systems into a multi-season career lifecycle so that player careers can simulate continuously from rookie debut to retirement.

## Description
Wire together:
- Player generation with dynamic attributes, potential, and starting club
- Multi-season progression with annual calendar cycles (pre-season, regular season fixtures, mid-season & summer transfer windows, end-of-season review)
- Weekly loops integrating:
  - Weekly wages and lifestyle expenses via `EconomySystem`
  - Weekly training regimens, XP gain, and fatigue accumulation via `TrainingSystem`, `ProgressionSystem`, `FatigueSystem`
  - Matchday simulation via `MatchSimulator` and `ManagerTrustSystem`
  - Match bonus distribution via `EconomySystem.ApplyMatchBonuses`
  - Narrative life events via `LifeEventSystem`
  - Relationship decay and interaction updates via `RelationshipSystem`
  - Transfer and contract status evaluation via `ContractSystem` and `TransferSystem`
  - Annual aging, skill progression/decline curves, and retirement evaluation based on age, physical abilities, or free agent stagnation.

## Acceptance Criteria
- [ ] Implement `CareerRunner` executing complete multi-season careers (up to 20 seasons or retirement)
- [ ] Seamless integration of all Phase 1 domain and simulation systems
- [ ] Retirement triggers: age threshold (>= 35 with declining physicals or >= 38 hard ceiling) or prolonged free agency (> 1 full season unattached)
- [ ] Headless execution completes without memory leaks or unhandled exceptions
- [ ] Strict determinism: same seed produces identical career trajectory
"""
    },
    {
        "title": "[P1-067] CareerSimulator: Statistical Metrics & Per-Career Export (CSV/JSON)",
        "labels": ["layer:career-simulator", "complexity:M", "milestone:1.11", "type:feature"],
        "body": """## User Story
> As a game designer, I want detailed per-career tracking and export options (CSV and JSON) so that I can inspect individual player trajectories, peak ratings, financial health, and transfer histories.

## Description
Track per career:
- `Seed`, `PlayerId`, `Name`, `Position`, `StartingOverall`, `PeakOverall`, `PeakAge`, `RetirementAge`, `SeasonsPlayed`
- `TotalAppearances`, `TotalGoals`, `TotalAssists`, `AverageRating`
- `TotalEarnings`, `FinalBalance`, `PeakWeeklySalary`, `BankruptcyOccurred`
- `ClubsPlayedForCount`, `TransferCount`, `TrophiesWon` (if applicable)

Support command-line export flags:
- `--csv <filepath>` writes a structured CSV of all simulated careers
- `--json <filepath>` writes full structured JSON with seasonal breakdowns
- Formatted console table for single-career or sample inspections

## Acceptance Criteria
- [ ] Define `CareerStatistics` and `SeasonStatistics` domain/simulation records
- [ ] Export to CSV via `--csv <path>` matching header schema
- [ ] Export to JSON via `--json <path>`
- [ ] Summary console output showing per-career highlights in interactive or verbose mode
"""
    },
    {
        "title": "[P1-068] CareerSimulator: Bulk Multi-Career Execution & Aggregate Distributions",
        "labels": ["layer:career-simulator", "complexity:M", "milestone:1.11", "type:feature"],
        "body": """## User Story
> As a game designer, I want to run bulk simulations of 1,000 to 10,000 careers and compute aggregate statistical distributions so that I can validate system balance, economy health, and progression pacing across the player population.

## Description
Support bulk simulation CLI parameters:
- `--careers <N>` (e.g., 100, 1000, 10000)
- `--seasons <N>` (max seasons per career, default 20)
- `--seed <S>` (master seed for deterministic pseudo-random sequences)
- `--threads <N>` / parallel execution option for fast bulk processing
- `--quiet` mode to suppress per-career console logs

Calculate aggregate distribution metrics across all careers:
- Mean, Median (p50), 10th percentile (p10), 90th percentile (p90), Min, Max, Standard Deviation
- Metrics: Peak Overall, Retirement Age, Career Length, Career Goals, Career Assists, Career Earnings, Total Transfers, In-Debt %
- ASCII / console histogram bins for Peak Overall and Retirement Age distributions

## Acceptance Criteria
- [ ] CLI flags `--careers`, `--seasons`, `--seed`, `--parallel`, `--quiet` supported
- [ ] Aggregate statistical report computed with mean, p10, p50, p90, min, max
- [ ] High-throughput simulation capability (simulating 1,000 careers completes in seconds)
- [ ] Distribution histograms rendered cleanly to console
"""
    },
    {
        "title": "[P1-069] Balance Validation & Distribution Assertions (Automated Test Suite)",
        "labels": ["layer:tests", "complexity:L", "milestone:1.11", "type:testing"],
        "body": """## User Story
> As a development team, I want automated balance assertion tests in the test suite so that any regression in XP curves, match difficulty, finances, transfers, or aging immediately fails CI.

## Description
Build automated balance validation test fixtures in `FootballLife.Simulation.Tests`:
- **Peak Overall Distribution**: Mean peak overall between 70.0 and 78.0; p10 >= 60; p90 <= 88; max <= 99; no players reaching 90+ before age 24.
- **Career Length & Aging**: Mean retirement age between 33 and 38; career length mean 12–18 seasons.
- **Transfers**: Mean transfer count between 1.5 and 5.0 across career.
- **Finances**: In-debt rate at retirement < 10%; maximum wage scaling corresponds with tier.
- **Determinism**: Assert that simulating 50 careers with seed 42 produces identical statistical hashes and values across repeated runs.

## Acceptance Criteria
- [ ] `CareerSimulatorBalanceTests.cs` created in `FootballLife.Simulation.Tests`
- [ ] Balance tests execute deterministically and assert on statistical bounds
- [ ] Strict determinism test asserting byte-for-byte identical output given identical seed
- [ ] All tests pass in `dotnet test` with zero warnings or failures
"""
    }
]

url = f"https://api.github.com/repos/{repo}/issues"

for issue in issues:
    data = json.dumps(issue).encode("utf-8")
    req = urllib.request.Request(url, data=data, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            res_body = json.loads(resp.read().decode("utf-8"))
            print(f"Created Issue #{res_body['number']}: {res_body['title']}")
    except urllib.error.HTTPError as e:
        print(f"Failed to create issue '{issue['title']}': HTTP {e.code} - {e.read().decode('utf-8')}", file=sys.stderr)
    except Exception as e:
        print(f"Error creating issue '{issue['title']}': {e}", file=sys.stderr)
    time.sleep(1)
