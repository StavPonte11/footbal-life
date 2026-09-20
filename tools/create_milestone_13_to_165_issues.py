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

# Ensure required labels exist
required_labels = [
    ("complexity:L", "ededed", "Large complexity task"),
    ("milestone:1.2.5", "ededed", "Milestone 1.2.5 - Gap Fix Stories"),
    ("milestone:1.3", "ededed", "Milestone 1.3 - Training System"),
    ("milestone:1.4", "ededed", "Milestone 1.4 - Fatigue & Recovery"),
    ("milestone:1.5", "ededed", "Milestone 1.5 - Match Simulation v1"),
    ("milestone:1.6", "ededed", "Milestone 1.6 - Career Progression"),
    ("milestone:1.6.5", "ededed", "Milestone 1.6.5 - Validation Gate")
]

for label_name, color, desc in required_labels:
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/labels",
        data=json.dumps({"name": label_name, "color": color, "description": desc}).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            print(f"  [Label Created] {label_name}")
    except urllib.error.HTTPError as e:
        if e.code == 422:
            # Already exists
            pass
        else:
            print(f"  [Label Notice] {label_name}: HTTP {e.code}")

issues = [
    # --- MILESTONE 1.2.5: GAP FIX STORIES ---
    {
        "title": "[P1-GAP-1] SimulationRandom Deterministic RNG",
        "labels": ["phase-1", "milestone:1.2.5", "simulation", "complexity:S"],
        "body": """## Goal
Implement a deterministic, seeded random number generator (`SimulationRandom`) in `FootballLife.Simulation` so that all stochastic simulation outcomes are reproducible given the same seed, enabling determinism testing and replay.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `SimulationRandom` class in `FootballLife.Simulation` namespace
- [ ] Constructor: `SimulationRandom(int seed)` — wraps `System.Random(seed)`
- [ ] `float NextFloat(float min, float max)` — uniform float in `[min, max)`
- [ ] `int NextInt(int min, int maxExclusive)` — uniform int in `[min, max)`
- [ ] `bool NextBool(float probability)` — returns true with probability `probability` in `[0, 1]`
- [ ] `float NextGaussian(float mean, float stddev)` — Box-Muller transform, stddev clamped to >= 0
- [ ] `T Pick<T>(IReadOnlyList<T> list)` — picks a random element, throws if list is empty
- [ ] No allocations after construction (internal state is two `double` fields for Gaussian)
- [ ] Determinism test: `SimulationRandom(42).NextFloat(0,1)` called 1000 times produces identical sequence on every run
- [ ] Zero `UnityEngine` references

## Branch
`feature/p1-gap1-simulation-random`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-GAP-2] PlayerFactory Career Initialization",
        "labels": ["phase-1", "milestone:1.2.5", "simulation", "complexity:M"],
        "body": """## Goal
Create `PlayerFactory` in `FootballLife.Simulation` to produce a complete, internally-consistent starting `WorldState` for a new player's career so that downstream systems have a valid initial state.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `PlayerFactory` class in `FootballLife.Simulation` namespace
- [ ] Static method: `PlayerFactory.CreateCareer(WorldState world, PlayerCreationArgs args, SimulationRandom rng)` -> `WorldState`
- [ ] `PlayerCreationArgs` record: `string Name`, `string Nationality`, `DateOnly DateOfBirth`, `Position PrimaryPosition`, `Foot PreferredFoot`, `Guid StartingClubId`, `byte StartingAbilityBase` [40–60]
- [ ] Created player gets: generated `Guid Id`, all abilities initialized from `StartingAbilityBase +- rng.NextGaussian(0, 5)` weighted by `PositionWeightMap`, clamped [1, 100]
- [ ] Created player's `PlayerCareerState` initialized with `StartingClubId`, `SquadStatus.Academy`, `ManagerTrust = 30`, salary derived from realistic youth wage range [£150–£500/week]
- [ ] A matching `Contract` created (1-year duration starting from season start date), added to `WorldState.Contracts`
- [ ] `PlayerPotential` created with `PotentialRating = StartingAbilityBase + rng.NextInt(10, 35)` clamped [50, 99], added to `WorldState.Potentials` (requires Gap 3)
- [ ] Returned `WorldState` has the new player in `Players`, `Abilities`, `States`, `CareerStates`, `Potentials`, `Contracts`
- [ ] Club's `SquadPlayerIds` updated to include new player
- [ ] Unit test: `PlayerFactory_CreatesPlayer_WithValidBoundedAbilities()`
- [ ] Unit test: `PlayerFactory_HigherBase_ProducesHigherInitialAbilities_OnAverage()` (statistical over 100 seeds)
- [ ] Unit test: `PlayerFactory_CreatedWorldState_HasPlayerInAllFacets()`
- [ ] Determinism test: same seed + same args -> identical player

## Branch
`feature/p1-gap2-player-factory`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-GAP-3] PlayerPotential in WorldState",
        "labels": ["phase-1", "milestone:1.2.5", "domain", "simulation", "complexity:S"],
        "body": """## Goal
Integrate `PlayerPotential` as a first-class citizen in `WorldState` so that potential is always loadable, accessible, and testable alongside all other player facets.

## Layer
- [x] Domain (`FootballLife.Domain`)
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `WorldState` gains `IReadOnlyDictionary<Guid, PlayerPotential> Potentials { get; init; }`
- [ ] `WorldState` constructor updated — `potentials` parameter added (after `contracts`), non-null enforced
- [ ] `WorldState.CreateEmpty()` returns empty `Potentials` dictionary
- [ ] `WorldState.WithPlayerPotential(Guid playerId, PlayerPotential potential)` -> `WorldState` added
- [ ] `WorldState.GetPotential(Guid playerId)` -> `PlayerPotential` (throws `KeyNotFoundException` if missing)
- [ ] `WorldState.WithPlayer(...)` overload updated to accept optional `PlayerPotential? potential = null`
- [ ] `WorldDataLoader` updated: if a player's potential data exists in JSON, load it; otherwise skip
- [ ] Unit test: `WorldState_WithPotential_RoundTrips()`
- [ ] All existing `WorldStateTests` still pass

## Branch
`feature/p1-gap3-potential-in-worldstate`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-GAP-4] FixtureGenerator Round-Robin League Schedule",
        "labels": ["phase-1", "milestone:1.2.5", "simulation", "complexity:M"],
        "body": """## Goal
Implement a deterministic round-robin `FixtureGenerator` in `FootballLife.Simulation` that produces a realistic league schedule where every club plays every other club twice (home and away).

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `FixtureGenerator` static class in `FootballLife.Simulation` namespace
- [ ] `FixtureGenerator.Generate(League league, WorldState world, int year, SimulationRandom rng)` -> `Season`
- [ ] Uses round-robin algorithm (Berger tables or circle method) — every club plays every other club exactly twice (home + away)
- [ ] Total matchdays = `(clubCount - 1) * 2` — for 20-club league: 38 matchdays
- [ ] Each `Matchday` contains a list of `ScheduledMatch` records: `DateOnly Date`, `Guid HomeClubId`, `Guid AwayClubId`, `Guid LeagueId`
- [ ] Fixture dates start from `season.StartDate`, spaced weekly (7 days per matchday)
- [ ] Home/Away assignment is randomized but seeded — same `rng` seed -> same schedule
- [ ] Generated `Season.Table` initialized with all clubs at 0 points, 0 GD
- [ ] No club has a home match in two consecutive matchdays (minimum alternation check)
- [ ] Unit test: `FixtureGenerator_ProducesCorrectMatchdayCount_ForTwentyClubLeague()`
- [ ] Unit test: `FixtureGenerator_EachClubPlaysTwice_AgainstEveryOtherClub()`
- [ ] Unit test: `FixtureGenerator_HomeAwayBalance_IsEqualPerClub()`
- [ ] Determinism test: same seed -> same fixture list

## Branch
`feature/p1-gap4-fixture-generator`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },

    # --- MILESTONE 1.3: TRAINING SYSTEM ---
    {
        "title": "[P1-018] Training Session Domain Model",
        "labels": ["phase-1", "milestone:1.3", "domain", "complexity:S"],
        "body": """## Goal
Create `TrainingSession` and `TrainingResult` domain models in `FootballLife.Domain` describing training blocks and defining the data contract between training and fatigue systems.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `TrainingSession` record in `FootballLife.Domain`: `TrainingType Type`, `TrainingIntensity Intensity`, `int DurationMinutes` [15–120], `DateOnly Date`
- [ ] `TrainingType` enum: `Technical`, `Physical`, `Mental`, `PositionSpecific`, `Recovery`, `Gym`
- [ ] `TrainingIntensity` enum: `Light`, `Moderate`, `Hard`, `Maximum`
- [ ] `DurationMinutes` validated: [15, 120] — out of range throws `ArgumentOutOfRangeException`
- [ ] `TrainingResult` record in `FootballLife.Domain`: `IReadOnlyDictionary<AttributeName, float> XpGained`, `float FatigueCost`, `float InjuryRisk`
- [ ] `TrainingResult.TotalXpGained` computed property: sum of all attribute XP values
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `TrainingSession_Duration_OutOfRange_Throws()`
- [ ] Unit test: `TrainingResult_TotalXp_SumsAllAttributes()`

## Branch
`feature/p1-018-training-session-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-019] TrainingSystem — XP Calculation",
        "labels": ["phase-1", "milestone:1.3", "simulation", "complexity:M"],
        "body": """## Goal
Implement `TrainingSystem.CalculateXP` to award XP to relevant attributes based on session type and intensity, factoring in position relevance, motivation, and fatigue.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `TrainingSystem.CalculateXP(TrainingSession session, Position position, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` -> `TrainingResult`
- [ ] XP formula: `BaseXP * IntensityMultiplier * PositionRelevance * StateModifier * rng_variance`
  - `BaseXP`: `Technical=3.0`, `Physical=3.0`, `Mental=2.0`, `PositionSpecific=4.0`, `Recovery=0`, `Gym=2.5` (per attribute)
  - `IntensityMultiplier`: `Light=0.5`, `Moderate=1.0`, `Hard=1.5`, `Maximum=2.0`
  - `PositionRelevance`: `PositionWeightMap.GetWeight(position, attribute)` — ranges [0.0–1.0]
  - `StateModifier`: `Motivation / 100f` — low motivation reduces absorption (min 0.3, never zero)
  - `rng_variance`: `rng.NextGaussian(1.0f, 0.1f)` — ±10% natural variance, clamped [0.7, 1.3]
- [ ] Only attributes relevant to the training type receive XP (e.g. Technical -> Passing, Shooting, Dribbling, Crossing, FirstTouch; NOT Stamina or Strength)
- [ ] `PositionSpecific` training type awards XP weighted by the 3 highest-weighted attributes for the player's position
- [ ] `Recovery` type always returns `TrainingResult` with all XP=0, `FatigueCost=-15f` (negative = recovery), `InjuryRisk=0`
- [ ] Fatigue cost: `BaseFatigueCost * IntensityMultiplier`; `BaseFatigueCost`: `Technical=8`, `Physical=12`, `Mental=4`, `PositionSpecific=10`, `Gym=10` (per 60 min); scaled by `DurationMinutes / 60f`
- [ ] Injury risk: `0f` if `Fatigue < 40`; linear interpolation from 0% at Fatigue=40 to 8% at Fatigue=100, multiplied by `Maximum` intensity having 2x base risk
- [ ] No LINQ allocations in the hot path — pre-allocated dictionary/accumulator
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `TrainingSystem_Technical_AwardsPassingXP_NotStamina()`
- [ ] Unit test: `TrainingSystem_Recovery_AwardsZeroXP_AndRestoresFatigue()`
- [ ] Unit test: `TrainingSystem_MaximumIntensity_AtHighFatigue_RaisesInjuryRisk()`
- [ ] Unit test: `TrainingSystem_LowMotivation_ReducesXpGained()`
- [ ] Unit test: `TrainingSystem_PositionSpecific_Striker_AwardsShootingAndPace()`
- [ ] Determinism test: same seed + same inputs -> same `TrainingResult`

## Branch
`feature/p1-019-training-xp`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-020] TrainingSystem — Weekly Diminishing Returns",
        "labels": ["phase-1", "milestone:1.3", "simulation", "complexity:M"],
        "body": """## Goal
Implement weekly diminishing returns on repeated training types within `TrainingSystem` to incentivize training variety and prevent single-attribute spamming.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `TrainingSystem.CalculateWeekXP(IReadOnlyList<TrainingSession> weekSessions, Position position, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` -> `IReadOnlyList<TrainingResult>`
- [ ] Sessions processed in order. For each repeated `TrainingType` within same week: XP reduced by 30% per previous repetition of same type (session 1: 100%, session 2: 70%, session 3: 49%, ...)
- [ ] Diminishing returns apply within a single week only — resets next week
- [ ] `Recovery` sessions are exempt from diminishing returns
- [ ] Maximum 3 training sessions per week (4th session throws `InvalidOperationException`)
- [ ] Unit test: `TrainingSystem_TwoTechnicalSessions_SecondHas70PercentXP()`
- [ ] Unit test: `TrainingSystem_MixedTypes_NoDiminishingBetweenDifferentTypes()`
- [ ] Unit test: `TrainingSystem_FourSessions_ThrowsInvalidOperation()`

## Branch
`feature/p1-020-diminishing-returns`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-021] Unit Tests — Training System",
        "labels": ["phase-1", "milestone:1.3", "testing", "complexity:M"],
        "body": """## Goal
Build a comprehensive xUnit test suite for `TrainingSystem` validating edge cases, failure modes, high-fatigue injury risk, motivation extremes, and deterministic repeatability.

## Layer
- [x] Tests (`FootballLife.Simulation.Tests`)

## Acceptance Criteria
- [ ] Test: `TrainingSystem_Fatigue100_MaximumIntensity_ProducesHighInjuryRisk()` (injury risk > 10%)
- [ ] Test: `TrainingSystem_Fatigue0_AnyIntensity_ZeroInjuryRisk()`
- [ ] Test: `TrainingSystem_Gym_AwardsStrengthAndStamina_NotTechnical()`
- [ ] Test: `TrainingSystem_StatisticalBalance_Over1000Seeds_MeanXpWithin10PercentOfExpected()`
- [ ] Test: `TrainingSystem_DeterminismTest_SameSeedSameResult()`

## Branch
`feature/p1-021-training-tests`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },

    # --- MILESTONE 1.4: FATIGUE & RECOVERY SYSTEM ---
    {
        "title": "[P1-024] FatigueSystem — Core Accumulation",
        "labels": ["phase-1", "milestone:1.4", "simulation", "complexity:M"],
        "body": """## Goal
Implement `FatigueSystem` in `FootballLife.Simulation` to accumulate fatigue from training and matches, and recover via rest and sleep.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `FatigueSystem` class in `FootballLife.Simulation` with all static methods (stateless)
- [ ] `FatigueSystem.ApplyTraining(PlayerState state, TrainingResult result)` -> `PlayerState` with updated `Fatigue` (Fatigue += result.FatigueCost, clamped [0f, 100f])
- [ ] `FatigueSystem.ApplyMatch(PlayerState state, int minutesPlayed, float matchIntensity)` -> `PlayerState` (full 90 mins at 1.0 adds 25 fatigue, proportional for subs, 80+ stamina reduces fatigue gain by 15%)
- [ ] `FatigueSystem.ApplyRest(PlayerState state, int hoursSlept)` -> `PlayerState` (8h: -12 fatigue, each hour above 8: -1.5 capped at 10h=-15; below 6h: no recovery + adds 3 fatigue)
- [ ] `FatigueSystem.ApplyDayTick(PlayerState state)` -> `PlayerState` (passive daily recovery -3 fatigue; no recovery if Fitness < 40)
- [ ] All returned `PlayerState` instances are new immutable records
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `FatigueSystem_FullMatch_AddsExpectedFatigue()`
- [ ] Unit test: `FatigueSystem_HighStamina_ReducesMatchFatigueCost()`
- [ ] Unit test: `FatigueSystem_PoorSleep_AddsFatigue()`
- [ ] Unit test: `FatigueSystem_RecoveryTraining_ReducesFatigue()`
- [ ] Unit test: `FatigueSystem_FatigueClampsAtHundred_NeverExceeds()`

## Branch
`feature/p1-024-fatigue-system`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-025] FatigueSystem — Effect on Performance",
        "labels": ["phase-1", "milestone:1.4", "simulation", "complexity:M"],
        "body": """## Goal
Implement fatigue-adjusted effective ability calculation and composite form modifier in `FatigueSystem` to reflect performance decay under exhaustion.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `FatigueSystem.ComputeEffectiveAbility(PlayerAbilities abilities, PlayerState state, AttributeName attribute)` -> `float` in [0, 100]
- [ ] Formula: `effectiveAbility = baseAbility * (1 - fatigueImpact * attributeSensitivity)`
  - `fatigueImpact = max(0, (state.Fatigue - 30f) / 70f)` — no penalty below 30 fatigue
  - `attributeSensitivity`: Physical = 1.0x, Technical = 0.6x, Mental = 0.4x
- [ ] Result clamped to [0f, baseAbility] — fatigue never boosts an attribute
- [ ] `FatigueSystem.ComputeFormModifier(PlayerState state)` -> `float` in [0.7, 1.2]
  - Combines Form, Confidence, Motivation: `0.7 + (Form/100f * 0.2) + (Confidence/100f * 0.15) + (Motivation/100f * 0.15)`, clamped [0.7, 1.2]
- [ ] Unit test: `FatigueSystem_Fatigue80_PhysicalAttribute_HasSignificantPenalty()`
- [ ] Unit test: `FatigueSystem_Fatigue80_ComposureAttribute_HasMinorPenalty()`
- [ ] Unit test: `FatigueSystem_LowFatigue_NoEffectOnAbility()`
- [ ] Unit test: `FatigueSystem_FormModifier_HighAll_ProducesMaxModifier()`

## Branch
`feature/p1-025-fatigue-effects`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-026] Unit Tests — Fatigue System",
        "labels": ["phase-1", "milestone:1.4", "testing", "complexity:S"],
        "body": """## Goal
Build integration and boundary tests ensuring the fatigue pipeline operates correctly across simulated training, rest, and match weeks.

## Layer
- [x] Tests (`FootballLife.Simulation.Tests`)

## Acceptance Criteria
- [ ] Test: `FatigueSystem_WeekSimulation_TrainMondayMatchSaturday_ExpectedFatigueAtWeekEnd()` (expected range [45–65])
- [ ] Test: `FatigueSystem_RestWeek_NoTrainingNoMatch_FatigueFallsBelow20()`
- [ ] Test: `FatigueSystem_Overtraining_ThreeSessions_MatchDay_ExtremelyHighFatigue()`

## Branch
`feature/p1-026-fatigue-tests`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },

    # --- MILESTONE 1.5: MATCH SIMULATION V1 ---
    {
        "title": "[P1-028] Match Domain Model",
        "labels": ["phase-1", "milestone:1.5", "domain", "complexity:M"],
        "body": """## Goal
Create `MatchState`, `MatchEvent`, `MatchEventType`, and `MatchResult` domain models in `FootballLife.Domain` for serialized, replayable, and inspectable match simulations.

## Layer
- [x] Domain (`FootballLife.Domain`)

## Acceptance Criteria
- [ ] `MatchState` record: `Guid HomeClubId`, `Guid AwayClubId`, `int HomeScore`, `int AwayScore`, `int Minute` [0–120], `bool IsFinished`, `IReadOnlyList<MatchEvent> EventLog`
- [ ] `MatchEvent` record: `int Minute`, `MatchEventType Type`, `Guid PlayerId`, `string Description`
- [ ] `MatchEventType` enum: `Goal`, `Assist`, `YellowCard`, `RedCard`, `Substitution`, `Miss`, `Save`, `TackleWon`, `Foul`, `PenaltyScored`, `PenaltyMissed`
- [ ] `MatchState.AddEvent(MatchEvent e)` -> new `MatchState` with event appended
- [ ] `MatchState.WithGoal(int minute, Guid scorerId, Guid? assisterId, bool isHome)` -> new `MatchState` with score updated and events appended
- [ ] `MatchResult` record (post-match summary): `int HomeScore`, `int AwayScore`, `IReadOnlyList<MatchEvent> Events`, `float PlayerRating` [1.0–10.0], `int PlayerMinutesPlayed`, `bool PlayerScored`, `bool PlayerAssisted`
- [ ] `MatchResult.IsWin(bool playerWasHome)`, `IsDraw()`, `IsLoss(bool playerWasHome)` computed properties
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `MatchState_ScoreUpdates_Correctly_AfterGoal()`
- [ ] Unit test: `MatchResult_IsWin_ReturnsCorrectly_ForHomeAndAway()`

## Branch
`feature/p1-028-match-domain-model`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-029] MatchSimulation — Situation Generator",
        "labels": ["phase-1", "milestone:1.5", "simulation", "complexity:L"],
        "body": """## Goal
Implement `MatchSituationGenerator` in `FootballLife.Simulation` to produce position-authentic match situations (shots, tackles, passes) tailored to the player's role and match context.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `MatchSituationGenerator` static class in `FootballLife.Simulation`
- [ ] `GenerateSituation(MatchState state, Position playerPosition, PlayerState playerState, SimulationRandom rng)` -> `MatchSituation?` (null = no player interaction this minute)
- [ ] `MatchSituation` record: `SituationType Type`, `float OpponentPressure` [0–10], `float ExpectedDifficulty` [0–1], `float PositionalAdvantage` [-1 to 1], `ActionChoice[] AvailableChoices`
- [ ] `ActionChoice` record: `MatchAction Action`, `float RiskLevel` [0–1], `float ExpectedValue` [0–1]
- [ ] Situation frequency: player receives 0–3 meaningful situations per 90-minute match; average ~2 for outfield, ~1.5 for GK
- [ ] Mandatory position-specific situation tables:
  - ST: `RunningInBehind` (40%), `ReceivingInBox` (30%), `LongShot` (15%), `Pressing` (10%), `AerialChallenge` (5%)
  - LW/RW: `ReceiveBall1v1` (35%), `Cross` (25%), `CutInside` (20%), `Pressing` (15%), `Tackle` (5%)
  - CM: `ReceiveUnderPressure` (30%), `ThroughBall` (25%), `LongPass` (20%), `Pressing` (15%), `Tackle` (10%)
  - DM: `Interception` (30%), `Tackle` (30%), `DistributionPass` (25%), `PositioningRun` (15%)
  - CB: `Tackle` (35%), `AerialChallenge` (25%), `Interception` (20%), `BuildUpPass` (20%)
  - GK: `Save` (50%), `ClaimCross` (25%), `Distribution` (25%)
- [ ] Context multipliers: Losing by 2+ (+20% attacking freq, -10% defending); Winning 85+ (-30% attacking); Player fatigue > 70 (+2 OpponentPressure)
- [ ] Zero LINQ, no allocations in hot path — static readonly tables
- [ ] Determinism test: same match state + same seed -> same situation
- [ ] Unit test: `SituationGenerator_Striker_ReceivesGoalScoringOpportunity()`
- [ ] Unit test: `SituationGenerator_Defender_ReceivesTackleAndInterceptionSituations()`
- [ ] Unit test: `SituationGenerator_Losing2Plus_AttackingPositions_GetHigherFrequency()`
- [ ] Unit test: `SituationGenerator_HighFatigue_IncreasesOpponentPressure()`

## Branch
`feature/p1-029-situation-generator`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-030] MatchSimulation — Action Resolver",
        "labels": ["phase-1", "milestone:1.5", "simulation", "complexity:L"],
        "body": """## Goal
Implement zero-allocation `ActionResolver` in `FootballLife.Simulation` to resolve chosen match actions using real abilities, form modifier, and situation context.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `ActionResolver` static class in `FootballLife.Simulation`
- [ ] `Resolve(MatchAction action, MatchSituation situation, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` -> `ActionOutcome`
- [ ] `ActionOutcome` struct (zero allocation): `bool Success`, `OutcomeType Type`, `float XpContribution`, `float ConfidenceDelta`, `float ManagerTrustDelta`
- [ ] Resolution formula:
  - `baseScore = weightedAbilityScore(action, abilities)` via `AttributeRelevanceMatrix`
  - `stateScore = FatigueSystem.ComputeFormModifier(state)`
  - `situationModifier = 1 - (situation.OpponentPressure / 10f * 0.3f)`
  - `roll = rng.NextGaussian(baseScore * stateScore * situationModifier, stddev)`
  - `success = roll > successThreshold(action)`
- [ ] `stddev` varies by action risk: safe actions (short pass) stddev=5, risky actions (long shot, through ball) stddev=15
- [ ] Success thresholds per action type (base vs 50-rated opponent):
  - `ShortPass`: 68, `LongPass`: 55, `Cross`: 55, `Dribble`: 60, `ThroughBall`: 48
  - `Shot_Close`: 62, `Shot_Long`: 38, `Header`: 55
  - `Tackle`: 60, `Interception`: 55, `AerialChallenge`: 60
  - `GoalkeeperSave`: 55
- [ ] On success: `XpContribution = situation.ExpectedDifficulty * 10f`, `ConfidenceDelta = +rng.NextFloat(1f, 3f)`
- [ ] On failure: `ConfidenceDelta = -rng.NextFloat(0.5f, 2f)` scaled by failure margin
- [ ] Goals award `ManagerTrustDelta = +5f`, errors leading to opposition goal = `-4f`, all other actions +-0–2f
- [ ] Zero `new` allocations in resolution path
- [ ] Determinism test: identical inputs -> identical `ActionOutcome`
- [ ] Unit test: `ActionResolver_HighFinishing_IncreasesGoalProbability()` (statistical over 1000 runs)
- [ ] Unit test: `ActionResolver_HighFatigue_ReducesSuccessRate()`
- [ ] Unit test: `ActionResolver_RiskyAction_HasHigherVariance_ThanSafeAction()`
- [ ] Unit test: `ActionResolver_Goal_IncreasesConfidenceAndTrust()`

## Branch
`feature/p1-030-action-resolver`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-031] MatchSimulation — Full Match Runner",
        "labels": ["phase-1", "milestone:1.5", "simulation", "complexity:M"],
        "body": """## Goal
Implement `MatchSimulator.Simulate` in `FootballLife.Simulation` to run an entire 90-minute match, managing minute ticks, background team scoring, player rating, and state updates.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `MatchSimulator.Simulate(ScheduledMatch fixture, Guid playerClubId, PlayerAbilities abilities, PlayerState state, Position position, SimulationRandom rng)` -> `(MatchResult result, PlayerState updatedState)`
- [ ] Internal loop: 90 ticks (one per minute). Each tick: 10% chance of generating situation; player interacts only if generated
- [ ] Background simulation when no player situation: team momentum scoring based on `opponentQuality vs playerClubQuality` delta
- [ ] Match result rating [1.0–10.0] formula: Base 5.0, +1.5 per goal, +0.75 per assist, +0.5 per key action, -1.0 per serious error; position-adjusted
- [ ] Post-match `PlayerState` updated: Fatigue += 25, Form updated toward rating, Confidence updated
- [ ] Unit test: `MatchSimulator_Striker_CanScoreGoal()`
- [ ] Unit test: `MatchSimulator_90MinutesSimulated_ProducesValidRating()`
- [ ] Determinism test: same seed -> same final score, same player rating

## Branch
`feature/p1-031-match-runner`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-032] MatchSimulation — Manager Trust Update",
        "labels": ["phase-1", "milestone:1.5", "simulation", "complexity:S"],
        "body": """## Goal
Implement `ManagerTrustSystem.ApplyMatchResult` in `FootballLife.Simulation` to update manager trust after each match based on performance and squad expectation.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `ManagerTrustSystem.ApplyMatchResult(PlayerCareerState careerState, MatchResult result, SquadStatus squadExpectation)` -> `PlayerCareerState`
- [ ] Trust delta formula:
  - Rating 8.0+: `+4f`
  - Rating 6.5–7.9: `+1.5f`
  - Rating 5.0–6.4: `0.0f` (neutral)
  - Rating 3.5–4.9: `-2f`
  - Rating below 3.5: `-5f`
- [ ] Expectation modifier: if player is `KeyPlayer` but delivers rating < 5.0: delta x 1.5
- [ ] Trust clamped to [0f, 100f] via `PlayerCareerState.ClampTrust()`
- [ ] Unit test: `ManagerTrustSystem_ExcellentPerformance_IncreasesTrust()`
- [ ] Unit test: `ManagerTrustSystem_PoorPerformance_DecreasesTrust()`
- [ ] Unit test: `ManagerTrustSystem_KeyPlayerExpectations_AmplifyPenalty()`
- [ ] Unit test: `ManagerTrustSystem_TrustNeverExceeds100()`

## Branch
`feature/p1-032-manager-trust-update`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-033] Integration Tests — Match System",
        "labels": ["phase-1", "milestone:1.5", "testing", "complexity:M"],
        "body": """## Goal
Build complete end-to-end integration tests for the match simulation pipeline covering situation generation, action resolution, match results, and multi-season trust convergence.

## Layer
- [x] Tests (`FootballLife.Simulation.Tests`)

## Acceptance Criteria
- [ ] Test: `MatchPipeline_Striker_ScoresSomeGoals_Over100Games()` — statistical: striker with 80 finishing scores in >=30 of 100 matches
- [ ] Test: `MatchPipeline_HighFatigue_ReducesPerformanceRating_Statistically()`
- [ ] Test: `MatchPipeline_FatigueIncreases_AfterEachMatch()`
- [ ] Test: `MatchPipeline_DeterminismTest_SameSeedProducesSameEntireMatchLog()`
- [ ] Test: `MatchPipeline_TenSeasonSimulation_TrustConverges_NotRunaway()` — 380 matches, trust stays within [10, 95] range for average player

## Branch
`feature/p1-033-match-integration-tests`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },

    # --- MILESTONE 1.6: CAREER PROGRESSION SYSTEM ---
    {
        "title": "[P1-038] ProgressionSystem — XP to Ability Gain",
        "labels": ["phase-1", "milestone:1.6", "simulation", "complexity:L"],
        "body": """## Goal
Implement `ProgressionSystem.ApplyXpGains` in `FootballLife.Simulation` to convert accumulated XP into attribute gains using an age-gated curve, bounded by player potential ceiling and aging physical decline.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `ProgressionSystem.ApplyXpGains(Guid playerId, WorldState world, DateOnly currentDate, SimulationRandom rng)` -> `PlayerAbilities`
- [ ] XP accumulated externally; this method converts pending XP to ability points
- [ ] Requires `WorldState.GetPotential(playerId)` to be resolvable
- [ ] Age-gated development curve:
  - Age 15–20: `developmentRate = 1.4` (fast development)
  - Age 21–24: `developmentRate = 1.1` (still growing)
  - Age 25–28: `developmentRate = 0.8` (mature, slow gains)
  - Age 29–31: `developmentRate = 0.4` (minimal development)
  - Age 32+: `developmentRate = 0.1` (near-zero; physical decline begins)
- [ ] Potential ceiling: ability gain stops at `PlayerPotential.PotentialRating`. XP is consumed but produces 0 gain once ceiling reached.
- [ ] `PotentialRange.Elite` adds ±5 ceiling randomness
- [ ] Physical attributes (`Pace`, `Acceleration`, `Stamina`) decline at age 31+: `-0.5f/year` independent of XP
- [ ] Gains are fractional (stored as float internally, truncated to byte only at serialization)
- [ ] No `new` allocations in computation path
- [ ] Unit test: `ProgressionSystem_YoungPlayer_GainsFasterThanVeteran()`
- [ ] Unit test: `ProgressionSystem_AbilityCappedAtPotentialRating()`
- [ ] Unit test: `ProgressionSystem_PhysicalDecline_BeginsAt31()`
- [ ] Determinism test: same seed -> same gains

## Branch
`feature/p1-038-progression-xp-to-ability`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-039] CareerSystem — Playing Status Evaluation",
        "labels": ["phase-1", "milestone:1.6", "simulation", "complexity:M"],
        "body": """## Goal
Implement `CareerSystem.EvaluateSquadStatus` in `FootballLife.Simulation` to re-evaluate squad status based on manager trust, relative reputation, and position competition.

## Layer
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `CareerSystem.EvaluateSquadStatus(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Club club, WorldState world)` -> `SquadStatus`
- [ ] Factors considered:
  - `ManagerTrust` (40% weight)
  - `Reputation` relative to club reputation (30% weight)
  - Position competition within club (30% weight) — count of players in `Club.SquadPlayerIds` sharing position with higher overall ability
- [ ] Status thresholds (composite score 0–100):
  - 0–15: `Academy`, 16–30: `Reserve`, 31–45: `Bench`, 46–65: `Rotation`, 66–80: `Starter`, 81–100: `KeyPlayer`
- [ ] Status can only change by one level per evaluation cycle
- [ ] Unit test: `CareerSystem_HighTrustHighReputation_EvaluatesAsStarter()`
- [ ] Unit test: `CareerSystem_LowTrustDespiteHighAbility_EvaluatesBelowAbilityExpectation()`
- [ ] Unit test: `CareerSystem_StatusOnlyChangesOneLevel_PerEvaluation()`

## Branch
`feature/p1-039-playing-status-evaluation`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-040] CareerSystem — End-of-Season Statistics",
        "labels": ["phase-1", "milestone:1.6", "domain", "simulation", "complexity:M"],
        "body": """## Goal
Create `SeasonStats` domain record in `FootballLife.Domain` and `CareerSystem.AggregateSeasonStats` in `FootballLife.Simulation` to aggregate comprehensive season statistics.

## Layer
- [x] Domain (`FootballLife.Domain`)
- [x] Simulation (`FootballLife.Simulation`)

## Acceptance Criteria
- [ ] `SeasonStats` record in `FootballLife.Domain`: `int Appearances`, `int Goals`, `int Assists`, `float AverageRating`, `int YellowCards`, `int RedCards`, `decimal WageEarned`, `SquadStatus FinalStatus`, `float AttributeGrowthAverage`
- [ ] `CareerSystem.AggregateSeasonStats(IReadOnlyList<MatchResult> results, PlayerCareerState finalCareerState, decimal weeklyWage, int weeksInSeason)` -> `SeasonStats`
- [ ] `AverageRating`: mean of all `result.PlayerRating` values for matches where `MinutesPlayed > 0`
- [ ] `AttributeGrowthAverage`: mean of all attribute deltas from start of season to end
- [ ] `WageEarned`: `weeklyWage * weeksInSeason`
- [ ] Unit test: `CareerSystem_SeasonStats_CorrectGoalAndAssistCounts()`
- [ ] Unit test: `CareerSystem_AverageRating_ExcludesDidNotPlayMatches()`

## Branch
`feature/p1-040-season-stats`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },
    {
        "title": "[P1-041] Integration Tests — Progression & Career",
        "labels": ["phase-1", "milestone:1.6", "testing", "complexity:L"],
        "body": """## Goal
Build multi-season integration tests validating that development curves, veteran aging, and career trajectories match realistic football distributions.

## Layer
- [x] Tests (`FootballLife.Simulation.Tests`)

## Acceptance Criteria
- [ ] Test: `Progression_FiveSeasonCareer_YoungStriker_ReachesExpectedAbilityRange()` (17yo starting at 50 overall reaches 60–80 at 22)
- [ ] Test: `Progression_VeteranPlayer_Age34_MinimalAbilityGain_PhysicalDeclineVisible()`
- [ ] Test: `CareerSimulator_100Careers_PeakOverall_Mean70to80_NoneReach99Before24()`
- [ ] Test: `CareerSimulator_DeterministicSeed42_IdenticalCareerStats()`

## Branch
`feature/p1-041-progression-tests`

## Verification
`dotnet test simulation/FootballLife.slnx`
"""
    },

    # --- MILESTONE 1.6.5: VALIDATION GATE ---
    {
        "title": "[P1-GATE-1] Minimal Console Harness (Minimum Playable Loop)",
        "labels": ["phase-1", "milestone:1.6.5", "simulation", "complexity:S"],
        "body": """## Goal
Build an interactive text console harness in `FootballLife.CareerSimulator` that lets a designer manually play 10+ in-game weeks of the core loop (train -> rest -> match -> see progress).

## Layer
- [x] CareerSimulator (`FootballLife.CareerSimulator`)

## Acceptance Criteria
- [ ] `dotnet run --project simulation/FootballLife.CareerSimulator -- --interactive` launches interactive text loop
- [ ] Loop presents: current week, fatigue, form, next match (if any), training choices
- [ ] Player types a choice: `1` (Technical), `2` (Physical), `3` (Recovery), `4` (Skip ahead to match)
- [ ] On match week: simulate match, print result, rating, trust delta
- [ ] On non-match week: simulate training, print XP gained per attribute, new fatigue
- [ ] Each week prints player state: `Fatigue: X | Form: X | Confidence: X | Trust: X`
- [ ] Session ends after 38 weeks (one season) or player types `q`
- [ ] Uses real (non-mocked) Training/Fatigue/MatchSimulation/Progression systems — no stubs
- [ ] Starting player: age 19, position ST, abilities all 55, at Tier 2 club

## Branch
`feature/p1-gate1-console-harness`

## Verification
`dotnet run --project simulation/FootballLife.CareerSimulator -- --interactive`
"""
    },
    {
        "title": "[P1-GATE-2] Human Validation - Playtest Console Harness",
        "labels": ["phase-1", "milestone:1.6.5", "quality", "complexity:S"],
        "body": """## Goal
Execute manual playtesting using the interactive console harness for 15–20 real minutes across 2–3 different player positions to observe emergent behavior and decision friction.

## Layer
- [x] Quality (`QA / Playtest`)

## Acceptance Criteria
- [ ] Run interactive console harness for 15–20 minutes with Striker position
- [ ] Run interactive console harness with Midfielder or Defender position
- [ ] Document raw playtest observations (decision trade-offs, progression pacing, fatigue tension)

## Branch
`feature/p1-gate2-playtest`

## Verification
Manual playtest session recorded
"""
    },
    {
        "title": "[P1-GATE-3] Human Validation - Gate Review Documentation",
        "labels": ["phase-1", "milestone:1.6.5", "documentation", "quality", "complexity:S"],
        "body": """## Goal
Document honest review answers in `docs/gate-review.md` evaluating decision significance, fatigue friction, and match stakes to inform the Go/No-Go gate decision.

## Layer
- [x] Documentation (`docs/gate-review.md`)

## Acceptance Criteria
- [ ] Create `docs/gate-review.md` answering:
  - Did any training decision feel meaningful? (e.g., "I rested instead of training and performed better")
  - Did fatigue feel like a real constraint or just a number?
  - Did matches feel different based on pre-match preparation?
  - Was there any moment of "I made the wrong call"?
  - What was boring?
- [ ] Commit `docs/gate-review.md` to repository

## Branch
`docs/gate-review`

## Verification
`docs/gate-review.md` exists and answers all review questions
"""
    },
    {
        "title": "[P1-GATE-4] Go/No-Go Decision Checkpoint",
        "labels": ["phase-1", "milestone:1.6.5", "quality", "complexity:S"],
        "body": """## Goal
Conduct a formal Go/No-Go evaluation before proceeding to Milestone 1.7 (Economy). Identify and tune specific simulation parameters if adjustments are required.

## Layer
- [x] Quality (`Quality / Architecture`)

## Acceptance Criteria
- [ ] Evaluate playtest observations and gate review results
- [ ] If GO: Approve progression to Milestone 1.7
- [ ] If NO-GO: Identify specific simulation tuning parameters (ActionResolver thresholds, XP scales, fatigue rates) and create tuning PR before opening Milestone 1.7 issues

## Branch
`checkpoint/p1-gate4-decision`

## Verification
Documented Go/No-Go decision signed off
"""
    }
]

print(f"Total issues to process: {len(issues)}")
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

print(f"\nDone! Successfully created {created_count}/{len(issues)} issues on GitHub.")
