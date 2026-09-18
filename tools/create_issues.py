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
        "title": "[P1-001] Player Identity Domain Model",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Create the foundational `Player` domain record in `FootballLife.Domain`.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `Player` record: `Guid Id`, `string Name`, `string Nationality`, `DateOnly DateOfBirth`, `Foot PreferredFoot`, `Position PrimaryPosition`, `Position? SecondaryPosition`
- [ ] `Foot` enum: `Left`, `Right`, `Both`
- [ ] `Position` enum: `GK`, `CB`, `FB`, `DM`, `CM`, `AM`, `LW`, `RW`, `ST`
- [ ] Stable `Guid Id` never changes across save/load
- [ ] Zero `UnityEngine` references in Domain assembly
- [ ] Test: `Player_CreatedWithValidData_HasStableId()`
- [ ] Test: `Player_DateOfBirth_CanComputeAgeAtGivenDate()`

## Branch
`feature/p1-001-player-identity`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-002] Player Abilities Model (15 Attributes)",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Create `PlayerAbilities` with typed byte attributes [0–100] for all 15 football skills.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `PlayerAbilities` immutable record with `byte` values [0–100]:
  - Physical: `Pace`, `Acceleration`, `Stamina`, `Strength`, `Agility`
  - Technical: `Passing`, `Shooting`, `Dribbling`, `Crossing`, `FirstTouch`, `Tackling`
  - Mental: `Vision`, `Composure`, `Positioning`, `DecisionMaking`
- [ ] All values clamped to [0, 100] at construction — no silent out-of-bounds
- [ ] Immutable: mutations via `with` expression produce a new instance
- [ ] Zero `UnityEngine` references
- [ ] Test: `PlayerAbilities_CannotExceedMaximumBounds()`
- [ ] Test: `PlayerAbilities_CannotBeBelowZero()`
- [ ] Test: `PlayerAbilities_WithExpression_ProducesNewInstance()`

## Branch
`feature/p1-002-player-abilities`
"""
    },
    {
        "title": "[P1-003] Player State Model (Temporary Condition)",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Create `PlayerState` capturing temporary condition (fatigue, form, confidence, etc.) — strictly separated from permanent `PlayerAbilities`.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `PlayerState` immutable record with `float` values [0f–100f]: `Fatigue`, `Confidence`, `Form`, `Happiness`, `Motivation`, `Morale`, `Fitness`
- [ ] All values clamped to [0f, 100f] — invariant enforced in constructor
- [ ] Completely separate type from `PlayerAbilities` — different lifetime, different mutation path
- [ ] Zero `UnityEngine` references
- [ ] Test: `PlayerState_Fatigue_ClampedToValidRange()`
- [ ] Test: `PlayerState_Form_DecaysTowardsNeutral_IsSupported()`

## Branch
`feature/p1-003-player-state`
"""
    },
    {
        "title": "[P1-004] Player Career State (Club, Status, Trust, Finances)",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Create `PlayerCareerState` tracking the footballer's current career context — club, squad status, manager trust, salary, market value, reputation.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `PlayerCareerState` record: `Guid ClubId`, `SquadStatus Status`, `float ManagerTrust` [0–100], `decimal WeeklySalary` (>=0), `decimal MarketValue` (>=0), `float Reputation` [0–100]
- [ ] `SquadStatus` enum: `Academy`, `Reserve`, `Bench`, `Rotation`, `Starter`, `KeyPlayer`
- [ ] `ManagerTrust` bounded [0, 100] — constructor rejects out-of-range
- [ ] `MarketValue` >= 0 invariant — cannot be negative
- [ ] Zero `UnityEngine` references
- [ ] Test: `PlayerCareerState_ManagerTrust_ClampsToValidRange()`
- [ ] Test: `PlayerCareerState_MarketValue_CannotBeNegative()`

## Branch
`feature/p1-004-player-career-state`
"""
    },
    {
        "title": "[P1-005] Player Potential Model (Probabilistic Ceiling)",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Create `PlayerPotential` encoding a probabilistic development ceiling — bounded but not perfectly predictable — so careers have variance.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `PlayerPotential` record: `byte PotentialRating` [50–99], `PotentialRange Range` enum (`Low`, `Medium`, `High`, `Elite`)
- [ ] `PotentialRange` defines the stochastic band width used by `ProgressionSystem`
- [ ] `RealizationProbability(int age)` returns decreasing float [0–1] after age 28
- [ ] Zero `UnityEngine` references
- [ ] Test: `PlayerPotential_RealizationProbability_DecreasesAfterPeakAge()`
- [ ] Test: `PlayerPotential_Rating_BoundedBetween50And99()`

## Branch
`feature/p1-005-player-potential`
"""
    },
    {
        "title": "[P1-006] Position Taxonomy and Attribute Weight Maps",
        "labels": ["phase-1", "domain", "data"],
        "body": """## Goal
Define a position taxonomy and data-driven attribute weight maps so all systems are position-aware without hardcoded magic numbers.

## Layer
- [x] Domain (`FootballLife.Domain`)
- [x] Data (`positions.json`)

## Acceptance Criteria
- [ ] `PositionWeightMap` static class: `float GetWeight(Position position, AttributeName attribute)` returns [0.0–1.0]
- [ ] Weights loaded from `content/data/positions.json` — NOT hardcoded in C#
- [ ] Striker weights: `Shooting` >= 0.9, `Pace` >= 0.8, `Tackling` <= 0.2
- [ ] Defender weights: `Tackling` >= 0.9, `Positioning` >= 0.85, `Shooting` <= 0.15
- [ ] Goalkeeper weights: `Positioning` >= 0.9, `Composure` >= 0.85
- [ ] Zero `UnityEngine` references
- [ ] Test: `PositionWeightMap_Striker_HasHighShootingWeight()`
- [ ] Test: `PositionWeightMap_Goalkeeper_HasHighPositioningWeight()`
- [ ] Test: `PositionWeightMap_AllPositions_WeightsSumToOne()`

## Branch
`feature/p1-006-position-taxonomy`
"""
    },
    {
        "title": "[P1-007] Attribute Relevance Matrix (Position x Situation)",
        "labels": ["phase-1", "domain"],
        "body": """## Goal
Build an `AttributeRelevanceMatrix` so that match simulation queries which attributes matter in a given position and situation — no hardcoded logic in the match engine.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `AttributeRelevance` record: `Position` -> list of `(AttributeName, float Weight)` pairs
- [ ] `AttributeRelevanceMatrix.Get(Position, SituationType?)` returns relevant weighted attributes
- [ ] Used by `MatchSimulation` action resolver (injected, not directly referenced)
- [ ] Zero `UnityEngine` references
- [ ] Test: `AttributeRelevance_Midfielder_PassingIsHighlyRelevant()`
- [ ] Test: `AttributeRelevance_Defender_TacklingIsHighlyRelevant()`

## Branch
`feature/p1-007-attribute-relevance-matrix`
"""
    },
    {
        "title": "[P1-008] Domain Invariants and Build Quality Gates",
        "labels": ["phase-1", "domain", "quality"],
        "body": """## Goal
Enforce that all domain models validate their invariants at construction and that the Domain assembly compiles with zero warnings.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `FootballLife.Domain.csproj`
- [ ] `<Nullable>enable</Nullable>` in all Domain projects
- [ ] All domain model constructors throw `ArgumentOutOfRangeException` for invalid bounds — no silent clamping without a test
- [ ] `dotnet build simulation/FootballLife.slnx` reports 0 warnings, 0 errors
- [ ] All invariant tests pass: `dotnet test simulation/FootballLife.slnx`

## Branch
`feature/p1-008-domain-invariants`
"""
    },
    {
        "title": "[P1-009] Player Domain Model - Unit Test Coverage (Milestone 1.1 Gate)",
        "labels": ["phase-1", "testing"],
        "body": """## Goal
Write comprehensive xUnit tests for all P1-001 through P1-007 domain models to gate Milestone 1.1 completion.

## Layer
- [x] Tests (`FootballLife.Simulation.Tests`)

## Acceptance Criteria
- [ ] Test class per domain model: `PlayerTests`, `PlayerAbilitiesTests`, `PlayerStateTests`, `PlayerCareerStateTests`, `PlayerPotentialTests`, `PositionWeightMapTests`, `AttributeRelevanceMatrixTests`
- [ ] Each test class covers: happy path, boundary values, invariant violations
- [ ] 100% of domain model public surface covered by at least one test
- [ ] `dotnet test` output: Failed: 0, Passed: >=30, Skipped: 0
- [ ] No tests that simply pass with empty assertions

## Branch
`feature/p1-009-player-domain-tests`

## Verification
`dotnet test simulation/FootballLife.slnx --verbosity normal`
"""
    }
]

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

print(f"\nDone! Successfully created {created_count}/{len(issues)} issues.")
