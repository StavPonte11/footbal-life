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
    "User-Agent": "FootballLife-ArchitectAgent",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

issues = [
    {
        "title": "[P1-010] Club Domain Model",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:M"],
        "body": """## Goal
Create the `Club` domain record and financial value objects in `FootballLife.Domain`.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `Club` record: `Guid Id`, `string Name`, `string ShortName`, `Guid LeagueId`, `int ReputationRating` [1–100], `ClubFinances Finances`, `int FacilityRating` [1–5], `TacticalIdentity TacticalStyle`
- [ ] `ClubFinances` value object: `decimal WeeklyWageBudget` (>= 0), `decimal TransferBudget` (>= 0)
- [ ] `TacticalIdentity` enum: `Possession`, `Counter`, `HighPress`, `Direct`
- [ ] Validate invariant bounds at construction (`ArgumentOutOfRangeException` on violations)
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: valid creation, negative budget throws, reputation bounds validation

## Branch
`feature/p1-010-club-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-011] Manager Domain Model",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:S"],
        "body": """## Goal
Create the `Manager` domain model to represent club managers whose tactical preferences and trust dynamics shape squad selection.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `Manager` record: `Guid Id`, `string Name`, `Formation PreferredFormation`, `TacticalIdentity TacticalStyle`, `float TrustDecayRate` [0.0–1.0], `float ToleranceThreshold` [0.0–1.0]
- [ ] `Formation` enum: `F442`, `F433`, `F352`, `F4231`, `F532`
- [ ] Trust decay calculation helper for squad status changes
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: manager creation, bounds checking on decay/tolerance rates

## Branch
`feature/p1-011-manager-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-012] League & Competition Domain Model",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:S"],
        "body": """## Goal
Create `League` and `Competition` domain models to represent organized football league tiers and tournament structures.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `League` record: `Guid Id`, `string Name`, `string CountryCode`, `int Tier` [1–5], `int ClubCount`, `int MatchdaysPerSeason`
- [ ] `Competition` record: `Guid Id`, `string Name`, `CompetitionFormat Format`
- [ ] `CompetitionFormat` enum: `League`, `Cup`, `GroupStage`
- [ ] Validate Tier in [1, 5] and ClubCount > 0
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: league creation, tier bounds, competition formats

## Branch
`feature/p1-012-league-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-013] Contract Domain Model",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:M"],
        "body": """## Goal
Create the `Contract` model representing the binding agreement between player and club with wage, role, bonuses, and duration.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `Contract` record: `Guid PlayerId`, `Guid ClubId`, `decimal WeeklySalary` (>= 0), `DateOnly StartDate`, `DateOnly EndDate`, `SquadRole ContractRole`, `ContractBonuses Bonuses`
- [ ] `ContractBonuses` value object: `decimal GoalBonus` (>= 0), `decimal AssistBonus` (>= 0), `decimal AppearanceBonus` (>= 0)
- [ ] `SquadRole` enum: `Starter`, `Rotation`, `Squad`, `Academy`
- [ ] Duration check: `EndDate > StartDate` (throws `ArgumentException`)
- [ ] Method: `bool IsExpired(DateOnly currentDate)`
- [ ] Method: `int RemainingDays(DateOnly currentDate)`
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: contract validity, expiration checks, bonus calculations

## Branch
`feature/p1-013-contract-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-014] Season & League Table Domain Model",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:M"],
        "body": """## Goal
Create `Season`, `Matchday`, `Fixture`, and `LeagueTable` models to structure calendar time and standings.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `Season` record: `int Year`, `DateOnly StartDate`, `DateOnly EndDate`, `IReadOnlyList<Matchday> Matchdays`
- [ ] `Matchday` record: `int WeekNumber`, `DateOnly Date`, `IReadOnlyList<FixtureId> Fixtures`
- [ ] `FixtureId` struct: `Guid HomeClubId`, `Guid AwayClubId`, `DateOnly Date`
- [ ] `LeagueTable` record containing sorted `IReadOnlyList<LeagueTableRow>`
- [ ] `LeagueTableRow` record: `Guid ClubId`, `int Played`, `int Won`, `int Drawn`, `int Lost`, `int GoalsFor`, `int GoalsAgainst`, `int Points`, `int GoalDifference`
- [ ] Standard points computation: `Won * 3 + Drawn`
- [ ] Sorting order: Points DESC -> GoalDifference DESC -> GoalsFor DESC
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: table sorting, points calculation, fixture association

## Branch
`feature/p1-014-season-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-015] WorldState Container",
        "labels": ["phase-1", "milestone:1.2", "domain", "complexity:M"],
        "body": """## Goal
Create an immutable `WorldState` snapshot container aggregating all active world entities.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `WorldState` record holding:
  - `IReadOnlyDictionary<Guid, Club> Clubs`
  - `IReadOnlyDictionary<Guid, Player> Players`
  - `IReadOnlyDictionary<Guid, PlayerAbilities> Abilities`
  - `IReadOnlyDictionary<Guid, PlayerState> States`
  - `IReadOnlyDictionary<Guid, PlayerCareerState> CareerStates`
  - `IReadOnlyDictionary<Guid, League> Leagues`
  - `Season CurrentSeason`
- [ ] `With*` immutable mutation methods returning a new snapshot
- [ ] Lookup methods: `GetPlayer(Guid id)`, `GetClub(Guid id)`, `GetLeague(Guid id)` throwing clear `KeyNotFoundException`
- [ ] Zero `UnityEngine` dependencies
- [ ] Unit tests: immutable snapshot creation, mutation returns new instance, lookup exceptions

## Branch
`feature/p1-015-world-state`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-016] Static Data Schema (clubs.json, leagues.json)",
        "labels": ["phase-1", "milestone:1.2", "data", "complexity:M"],
        "body": """## Goal
Define structured JSON data schemas and seed files for clubs and leagues in `content/data/`.

## Layer
- [x] Data (`content/data/`)

## Acceptance Criteria
- [ ] `content/data/leagues.json`: seed data for 5 major European leagues (England, Spain, Germany, Italy, France)
- [ ] `content/data/clubs.json`: seed data for top clubs linked to `leagueId`
- [ ] All `id` fields are valid, non-colliding GUIDs
- [ ] Valid budgets, facility ratings [1–5], tactical styles matching `TacticalIdentity`
- [ ] JSON schema files in `content/data/schema/` for automated validation

## Branch
`feature/p1-016-static-data-schema`
"""
    },
    {
        "title": "[P1-017] World Data Loader and Content Validation",
        "labels": ["phase-1", "milestone:1.2", "simulation", "complexity:M"],
        "body": """## Goal
Implement `WorldDataLoader` in `FootballLife.Simulation` to parse, validate, and construct a valid `WorldState` from JSON data.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `WorldDataLoader.Load(string clubsJson, string leaguesJson)` method using `System.Text.Json`
- [ ] Content validation rules:
  - No duplicate GUIDs across clubs, leagues, players
  - Every club's `LeagueId` must exist in `Leagues`
  - All numerical values within domain invariant bounds
- [ ] Return `WorldDataLoadResult` with `WorldState` on success, or list of `ValidationError` on failure
- [ ] Unit tests: valid load, duplicate ID detected, missing league reference detected, invalid numerical value reported

## Branch
`feature/p1-017-world-data-loader`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    }
]

print(f"Creating {len(issues)} issues for Milestone 1.2...")
created_count = 0

for item in issues:
    payload = json.dumps({
        "title": item["title"],
        "body": item["body"],
        "labels": item["labels"]
    }).encode("utf-8")

    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues",
        data=payload,
        headers=headers,
        method="POST"
    )

    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            print(f"  [OK] Created #{data['number']}: {data['title']} -> {data['html_url']}")
            created_count += 1
            time.sleep(0.5)
    except urllib.error.HTTPError as e:
        err_body = e.read().decode("utf-8")
        print(f"  [FAIL] HTTP {e.code} for '{item['title']}': {err_body}", file=sys.stderr)
    except Exception as e:
        print(f"  [FAIL] Error for '{item['title']}': {e}", file=sys.stderr)

print(f"\nCompleted! Created {created_count}/{len(issues)} Milestone 1.2 issues.")
