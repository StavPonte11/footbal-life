# Football Life — User Stories & Acceptance Criteria
> **Phase 1: Core Simulation Engine — Milestones 1.1 through 1.11**
> Authored by Architect Agent | September 2026

---

## How to Read This Document

Each user story follows the format:
```
As a [stakeholder], I want [capability] so that [benefit].
```

**Layers:** Domain (D) | Simulation (S) | Tests (T) | Data (Data)
**Branch convention:** `feature/p1-NNN-<slug>`
**Acceptance Criteria (AC)** must all pass before a PR may be merged.

> **Status (Sep 2026):** **PHASE 1 IS 100% COMPLETE AND MERGED TO `main`** (Stories P1-001 through P1-069, P1-GAP-1 through P1-GAP-4, and P1-GATE-1 through P1-GATE-4). All 83 related GitHub issues (#1–#57, #64–#68, #70–#75, #77–#81, #83–#88, #90–#93) and PRs (#11–#16, #25–#32, #58–#63, #69, #76, #82, #89, #94) are merged and closed. Build is green with **524 unit & integration tests passing** (0 failures, 0 warnings).
>
> **Validation Gate 1.6.5 PASSED (GO)**: The minimum playable loop was playtested via the interactive console harness (`FootballLife.CareerSimulator --interactive`) and formal evaluation documented in [`docs/gate-review.md`](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/gate-review.md).
>
> **Current Active Milestone:** Phase 2, Milestone 2.1 — Unity Project Bootstrap (Stories P2-001 through P2-005).

---

## MILESTONE 1.1 — Player Domain Model ✅ Complete

### Story P1-001: Player Identity ✅
**Branch:** `feature/p1-001-player-identity`
**Label:** `domain`, `phase-1`

> As a simulation system, I want a `Player` domain model representing a footballer's identity so that all other systems have a stable reference to reason about the person.

**Acceptance Criteria:**
- [ ] `Player` record in `FootballLife.Domain` with: `Guid Id`, `string Name`, `string Nationality`, `DateOnly DateOfBirth`, `Foot PreferredFoot`, `Position PrimaryPosition`, `Position? SecondaryPosition`
- [ ] `Foot` enum: `Left`, `Right`, `Both`
- [ ] `Position` enum: `GK`, `CB`, `FB`, `DM`, `CM`, `AM`, `LW`, `RW`, `ST`
- [ ] Player has a stable `Guid Id` that never changes across save/load
- [ ] No `UnityEngine` namespace imported anywhere in Domain
- [ ] Unit test: `Player_CreatedWithValidData_HasStableId()`
- [ ] Unit test: `Player_DateOfBirth_CanComputeAgeAtGivenDate()`

---

### Story P1-002: Player Abilities ✅
**Branch:** `feature/p1-002-player-abilities`
**Label:** `domain`, `phase-1`

> As a training system, I want a `PlayerAbilities` model with 15 typed attributes so that training effects and match performance can be calculated precisely per attribute.

**Acceptance Criteria:**
- [ ] `PlayerAbilities` record with `byte` values (0–100) for:
  - Physical: `Pace`, `Acceleration`, `Stamina`, `Strength`, `Agility`
  - Technical: `Passing`, `Shooting`, `Dribbling`, `Crossing`, `FirstTouch`, `Tackling`
  - Mental: `Vision`, `Composure`, `Positioning`, `DecisionMaking`
- [ ] All attribute values clamped to [0, 100] at construction — cannot be out of bounds
- [ ] `PlayerAbilities` is immutable; mutations produce a new instance (`with` expression)
- [ ] Unit test: `PlayerAbilities_CannotExceedMaximumBounds()`
- [ ] Unit test: `PlayerAbilities_CannotBeBelowZero()`
- [ ] Unit test: `PlayerAbilities_IsImmutable_WithExpressionProducesNewInstance()`

---

### Story P1-003: Player State ✅
**Branch:** `feature/p1-003-player-state`
**Label:** `domain`, `phase-1`

> As a simulation tick, I want a `PlayerState` model capturing temporary condition so that performance, recovery, and motivation systems operate on current condition rather than permanent ability.

**Acceptance Criteria:**
- [ ] `PlayerState` record with `float` values [0f–100f]: `Fatigue`, `Confidence`, `Form`, `Happiness`, `Motivation`, `Morale`, `Fitness`
- [ ] All values clamped to [0, 100] — invariant enforced at construction
- [ ] `PlayerState` is clearly separate from `PlayerAbilities` (different type, different lifetime)
- [ ] Unit test: `PlayerState_Fatigue_ClampedToValidRange()`
- [ ] Unit test: `PlayerState_Form_DecaysTowardsNeutral_WhenNoMatchesPlayed()`

---

### Story P1-004: Player Career State ✅
**Branch:** `feature/p1-004-player-career-state`
**Label:** `domain`, `phase-1`

> As a career system, I want a `PlayerCareerState` model tracking club membership, squad status, manager trust, and finances so that progression systems always know the player's current career context.

**Acceptance Criteria:**
- [ ] `PlayerCareerState` record with: `Guid ClubId`, `SquadStatus Status`, `float ManagerTrust`, `decimal WeeklySalary`, `decimal MarketValue`, `float Reputation`
- [ ] `SquadStatus` enum: `Academy`, `Reserve`, `Bench`, `Rotation`, `Starter`, `KeyPlayer`
- [ ] `ManagerTrust` bounded [0, 100]
- [ ] `MarketValue` ≥ 0 invariant
- [ ] Unit test: `PlayerCareerState_ManagerTrust_ClampsToValidRange()`
- [ ] Unit test: `PlayerCareerState_MarketValue_CannotBeNegative()`

---

### Story P1-005: Player Potential ✅
**Branch:** `feature/p1-005-player-potential`
**Label:** `domain`, `phase-1`

> As a progression system, I want a `PlayerPotential` model encoding a probabilistic development ceiling so that player growth is bounded but not perfectly predictable.

**Acceptance Criteria:**
- [ ] `PlayerPotential` record with `byte PotentialRating` [50–99] and `PotentialRange Range` (Low, Medium, High, Elite)
- [ ] `PotentialRange` widens or narrows the stochastic band used by `ProgressionSystem`
- [ ] `RealizationProbability(age)` method returns decreasing probability after age 28
- [ ] Unit test: `PlayerPotential_RealizationProbability_DecreasesAfterPeak()`
- [ ] Unit test: `PlayerPotential_RatingBounded_Between50And99()`

---

### Story P1-006: Position Taxonomy ✅
**Branch:** `feature/p1-006-position-taxonomy`
**Label:** `domain`, `phase-1`

> As all systems, I want a well-defined position taxonomy with weight maps so that training, match evaluation, and progression are position-aware rather than generic.

**Acceptance Criteria:**
- [ ] `PositionWeightMap` static class providing `float GetWeight(Position position, AttributeName attribute)` [0.0–1.0]
- [ ] Strikers weight Shooting, Finishing, Pace high; Tackling low
- [ ] Defenders weight Tackling, Positioning, Strength high; Shooting low
- [ ] Weights are data-driven (loaded from `positions.json` not hardcoded magic numbers)
- [ ] Unit test: `PositionWeightMap_Striker_HasHighShootingWeight()`
- [ ] Unit test: `PositionWeightMap_Goalkeeper_HasHighPositioningWeight()`

---

### Story P1-007: Attribute Position Relevance ✅
**Branch:** `feature/p1-007-attribute-relevance-matrix`
**Label:** `domain`, `phase-1`

> As a match simulation, I want to query which attributes are relevant for a given position in a given situation so that outcomes are position-authentic rather than globally generic.

**Acceptance Criteria:**
- [ ] `AttributeRelevance` record: position → list of weighted (`AttributeName`, `float Weight`) pairs
- [ ] `AttributeRelevanceMatrix` provides lookup by `Position` and optional `SituationType`
- [ ] Used by `MatchSimulation` action resolver (not hardcoded in match code)
- [ ] Unit test: `AttributeRelevance_Midfielder_PassingIsHighlyRelevant()`

---

### Stories P1-008 & P1-009: Domain Invariants & Tests ✅
**AC:**
- [ ] All domain models enforce their invariants in constructors (no silent clamping without unit test)
- [ ] Domain assembly has zero warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- [ ] `dotnet test` reports 100% pass on all P1-001 through P1-007 tests

---

## MILESTONE 1.2 — World Domain Model ✅ Complete

### Story P1-010: Club Model ✅
**Branch:** `feature/p1-010-club-model`

> As a transfer system, I want a `Club` domain model encoding reputation, finances, and tactical identity so that transfer offers and career opportunities are grounded in club reality.

**AC:**
- [ ] `Club` record: `Guid Id`, `string Name`, `string Country`, `LeagueTier Tier`, `float Reputation` [0–100], `decimal AnnualBudget`, `ClubFacilities Facilities`, `TacticalIdentity Tactics`
- [ ] `LeagueTier` enum: `Top`, `Second`, `Third`, `Lower`, `NonLeague`
- [ ] `ClubFacilities` record: `TrainingQuality`, `MedicalQuality` [0–100] — affects player development rate
- [ ] Unit test: `Club_Reputation_BoundedToValidRange()`

---

### Story P1-014: Season Model ✅
**Branch:** `feature/p1-014-season-model`

> As the simulation tick engine, I want a `Season` model defining the calendar, match schedule, and current week so that all time-based systems advance consistently.

**AC:**
- [ ] `Season` record: `int Year`, `int CurrentWeek`, `int TotalWeeks` (38–46 depending on cups), `IReadOnlyList<ScheduledMatch> Calendar`
- [ ] `ScheduledMatch`: `DateOnly Date`, `Guid HomeClubId`, `Guid AwayClubId`, `CompetitionType Competition`
- [ ] Season advances strictly forward — no `CurrentWeek` rollback
- [ ] Unit test: `Season_CurrentWeek_NeverExceedsTotalWeeks()`

---

### Story P1-016: Static Data Schema ✅
**Branch:** `feature/p1-016-static-data-schema`

> As the simulation, I want canonical JSON schemas for clubs, leagues, and positions so that content is fully data-driven and no club or league is hardcoded in simulation logic.

**AC:**
- [ ] `clubs.json` with 20 clubs minimum (5 leagues, 4 clubs each), fields: `id`, `name`, `country`, `tier`, `reputation`, `annualBudget`, `facilities`
- [ ] `leagues.json` with 5 leagues, fields: `id`, `name`, `country`, `tier`, `clubs`
- [ ] `positions.json` with attribute weight maps per position
- [ ] Content validator checks: no duplicate IDs, no orphaned club references, valid ranges
- [ ] Unit test: `ContentValidator_DetectsDuplicateClubId()`
- [ ] Unit test: `ContentValidator_DetectsInvalidAttributeWeight()`

> **Implementation note:** the shipped `leagues.json` diverges from this AC's field list — `countryCode` instead of `country`, and `clubCount` / `matchdaysPerSeason` / `promotionSlots` / `relegationSlots` instead of an inline `clubs` array. That's a reasonable normalization (clubs presumably reference their league by `leagueId` rather than leagues embedding a club list — avoids the two ever going out of sync), but the AC text above should be updated to match what was actually built, and it's worth double-checking that `clubs.json`'s content validator actually enforces "every club's `leagueId` resolves to a real league" now that the relationship runs that direction. Currently only the 5 top-tier (tier 1) leagues are populated, all with `promotionSlots: 0`, which is correct for tier-1 but means promotion/relegation logic between tiers 1↔2 has no data to exercise yet — fine for now, just a gap to remember once Tier 2 leagues are added. Also carries the real-league-naming flag noted in ROADMAP.md's Milestone 1.2 section — this is where that decision actually needs to land before `clubs.json` scales past sample data.

---

---

## MILESTONE 1.2.5 — Simulation Foundations (Gap Fixes) ✅ Complete
*PR #58 merged | Issues #33–#36 closed*

These are critical simulation foundations identified and implemented to enable deterministic testing, valid initial state, and complete schedules.

### Story P1-GAP-1: SimulationRandom ✅
**Branch:** `feature/p1-gap1-simulation-random`
**Labels:** `phase-1`, `milestone:1.2.5`, `simulation`, `complexity:S`

> As every simulation system, I need a deterministic, seeded random number generator so that all stochastic outcomes are reproducible given the same seed, enabling determinism testing and replay.

**Acceptance Criteria:**
- [ ] `SimulationRandom` class in `FootballLife.Simulation` namespace
- [ ] Constructor: `SimulationRandom(int seed)` — wraps `System.Random(seed)`
- [ ] `float NextFloat(float min, float max)` — uniform float in [min, max)
- [ ] `int NextInt(int min, int maxExclusive)` — uniform int in [min, max)
- [ ] `bool NextBool(float probability)` — returns true with probability `probability` ∈ [0,1]
- [ ] `float NextGaussian(float mean, float stddev)` — Box-Muller transform, stddev clamped to ≥ 0
- [ ] `T Pick<T>(IReadOnlyList<T> list)` — picks a random element, throws if list is empty
- [ ] No allocations after construction (internal state is two `double` fields for Gaussian)
- [ ] Determinism test: `SimulationRandom(42).NextFloat(0,1)` called 1000 times produces identical sequence on every run
- [ ] No `UnityEngine` reference

---

### Story P1-GAP-2: PlayerFactory ✅
**Branch:** `feature/p1-gap2-player-factory`
**Labels:** `phase-1`, `milestone:1.2.5`, `simulation`, `complexity:M`

> As the career initialization system, I need a factory that produces a complete, internally-consistent starting WorldState for a new player's career so that all downstream systems have a valid state to operate on.

**Acceptance Criteria:**
- [ ] `PlayerFactory` class in `FootballLife.Simulation` namespace
- [ ] Static method: `PlayerFactory.CreateCareer(WorldState world, PlayerCreationArgs args, SimulationRandom rng)` → `WorldState`
- [ ] `PlayerCreationArgs` record: `string Name`, `string Nationality`, `DateOnly DateOfBirth`, `Position PrimaryPosition`, `Foot PreferredFoot`, `Guid StartingClubId`, `byte StartingAbilityBase` [40–60]
- [ ] Created player gets: generated `Guid Id`, all abilities initialized from `StartingAbilityBase ± rng.NextGaussian(0, 5)` weighted by `PositionWeightMap`, clamped [1,100]
- [ ] Created player's `PlayerCareerState` initialized with `StartingClubId`, `SquadStatus.Academy`, `ManagerTrust=30`, salary derived from a realistic youth wage range [£150–£500/week]
- [ ] A matching `Contract` created (1-year duration starting from season start date), added to `WorldState.Contracts`
- [ ] `PlayerPotential` created with `PotentialRating = StartingAbilityBase + rng.NextInt(10, 35)` clamped [50,99], added to `WorldState.Potentials`
- [ ] Returned `WorldState` has the new player in `Players`, `Abilities`, `States`, `CareerStates`, `Potentials`, `Contracts`
- [ ] Club's `SquadPlayerIds` updated to include new player
- [ ] Unit test: `PlayerFactory_CreatesPlayer_WithValidBoundedAbilities()`
- [ ] Unit test: `PlayerFactory_HigherBase_ProducesHigherInitialAbilities_OnAverage()` (statistical over 100 seeds)
- [ ] Unit test: `PlayerFactory_CreatedWorldState_HasPlayerInAllFacets()`
- [ ] Determinism test: same seed + same args → identical player

---

### Story P1-GAP-3: PlayerPotential in WorldState ✅
**Branch:** `feature/p1-gap3-potential-in-worldstate`
**Labels:** `phase-1`, `milestone:1.2.5`, `domain`, `simulation`, `complexity:S`

> As the progression and factory systems, I need `PlayerPotential` to be a first-class citizen in `WorldState` so that potential is always loadable, accessible, and testable alongside other player facets.

**Acceptance Criteria:**
- [ ] `WorldState` gains `IReadOnlyDictionary<Guid, PlayerPotential> Potentials { get; init; }`
- [ ] `WorldState` constructor updated — `potentials` parameter added (after `contracts`), non-null enforced
- [ ] `WorldState.CreateEmpty()` returns empty `Potentials` dictionary
- [ ] `WorldState.WithPlayerPotential(Guid playerId, PlayerPotential potential)` → `WorldState` added
- [ ] `WorldState.GetPotential(Guid playerId)` → `PlayerPotential` (throws `KeyNotFoundException` if missing)
- [ ] `WorldState.WithPlayer(...)` overload updated to accept optional `PlayerPotential? potential = null`
- [ ] `WorldDataLoader` updated: if a player's potential data exists in JSON, load it; otherwise skip
- [ ] Unit test: `WorldState_WithPotential_RoundTrips()`
- [ ] All existing `WorldStateTests` still pass

---

### Story P1-GAP-4: FixtureGenerator ✅
**Branch:** `feature/p1-gap4-fixture-generator`
**Labels:** `phase-1`, `milestone:1.2.5`, `simulation`, `complexity:M`

> As the match simulation system, I need a deterministic fixture generator that produces a realistic round-robin league schedule so that every club in a league plays every other club twice (home and away) across the season.

**Acceptance Criteria:**
- [ ] `FixtureGenerator` static class in `FootballLife.Simulation` namespace
- [ ] `FixtureGenerator.Generate(League league, WorldState world, int year, SimulationRandom rng)` → `Season`
- [ ] Uses round-robin algorithm (Berger tables or circle method) — every club plays every other club exactly twice (home + away)
- [ ] Total matchdays = `(clubCount - 1) * 2` — for 20-club league: 38 matchdays
- [ ] Each `Matchday` contains a list of `ScheduledMatch` records: `DateOnly Date`, `Guid HomeClubId`, `Guid AwayClubId`, `Guid LeagueId`
- [ ] Fixture dates start from `season.StartDate`, spaced weekly (7 days per matchday)
- [ ] Home/Away assignment is randomized but seeded — same `rng` seed → same schedule
- [ ] Generated `Season.Table` initialized with all clubs at 0 points, 0 GD
- [ ] No club has a home match in two consecutive matchdays (minimum alternation check)
- [ ] Unit test: `FixtureGenerator_ProducesCorrectMatchdayCount_ForTwentyClubLeague()`
- [ ] Unit test: `FixtureGenerator_EachClubPlaysTwice_AgainstEveryOtherClub()`
- [ ] Unit test: `FixtureGenerator_HomeAwayBalance_IsEqualPerClub()`
- [ ] Determinism test: same seed → same fixture list

---

## MILESTONE 1.3 — Training System ✅ Complete
*PR #59 merged | Issues #37–#40 closed*

### Story P1-018: Training Session Domain Model ✅
**Branch:** `feature/p1-018-training-session-model`
**Labels:** `phase-1`, `milestone:1.3`, `domain`, `complexity:S`

> As the training system, I need a `TrainingSession` domain model describing a single training block so that the simulation knows what type of training, at what intensity, the player performed.

**Acceptance Criteria:**
- [ ] `TrainingSession` record in `FootballLife.Domain`: `TrainingType Type`, `TrainingIntensity Intensity`, `int DurationMinutes` [15–120], `DateOnly Date`
- [ ] `TrainingType` enum: `Technical`, `Physical`, `Mental`, `PositionSpecific`, `Recovery`, `Gym`
- [ ] `TrainingIntensity` enum: `Light`, `Moderate`, `Hard`, `Maximum`
- [ ] `DurationMinutes` validated: [15, 120] — shorter than 15 is warmup, longer than 120 is unrealistic for a focused session
- [ ] `TrainingResult` record in `FootballLife.Domain`: `IReadOnlyDictionary<AttributeName, float> XpGained`, `float FatigueCost`, `float InjuryRisk` — explicit protocol between TrainingSystem and FatigueSystem
- [ ] `TrainingResult.TotalXpGained` computed property: sum of all attribute XP values
- [ ] No `UnityEngine` reference
- [ ] Unit test: `TrainingSession_Duration_OutOfRange_Throws()`
- [ ] Unit test: `TrainingResult_TotalXp_SumsAllAttributes()`

---

### Story P1-019: TrainingSystem — XP Calculation ✅
**Branch:** `feature/p1-019-training-xp`
**Labels:** `phase-1`, `milestone:1.3`, `simulation`, `complexity:M`

> As a footballer, I want training sessions to award XP to relevant attributes based on session type and intensity so that training feels purposeful — my chosen focus actually improves the right attributes.

**Acceptance Criteria:**
- [ ] `TrainingSystem.CalculateXP(TrainingSession session, Position position, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` → `TrainingResult`
- [ ] XP formula: `BaseXP * IntensityMultiplier * PositionRelevance * StateModifier * rng_variance`
  - `BaseXP`: `Technical=3.0`, `Physical=3.0`, `Mental=2.0`, `PositionSpecific=4.0`, `Recovery=0`, `Gym=2.5` (per attribute)
  - `IntensityMultiplier`: `Light=0.5`, `Moderate=1.0`, `Hard=1.5`, `Maximum=2.0`
  - `PositionRelevance`: `PositionWeightMap.GetWeight(position, attribute)` — ranges [0.0–1.0]
  - `StateModifier`: `Motivation / 100f` — low motivation means poor absorption (min 0.3, never zero)
  - `rng_variance`: `rng.NextGaussian(1.0f, 0.1f)` — ±10% natural variance, clamped [0.7, 1.3]
- [ ] Only attributes relevant to the training type receive XP (e.g. Technical training → Passing, Shooting, Dribbling, Crossing, FirstTouch; NOT Stamina or Strength)
- [ ] `PositionSpecific` training type awards XP weighted by the **3 highest-weighted attributes** for the player's position
- [ ] `Recovery` type always returns `TrainingResult` with all XP=0, `FatigueCost=-15f` (negative = recovery), `InjuryRisk=0`
- [ ] Fatigue cost: `BaseFatigueCost * IntensityMultiplier`; `BaseFatigueCost`: `Technical=8`, `Physical=12`, `Mental=4`, `PositionSpecific=10`, `Gym=10` (per 60 min); scaled by `DurationMinutes / 60f`
- [ ] Injury risk: `0f` if `Fatigue < 40`; linear interpolation from 0% at Fatigue=40 to 8% at Fatigue=100, multiplied by `Maximum` intensity having 2× base risk
- [ ] No LINQ allocations in the hot path — pre-allocated dictionary or fixed-size struct accumulator
- [ ] No `UnityEngine` reference
- [ ] Unit test: `TrainingSystem_Technical_AwardsPassingXP_NotStamina()`
- [ ] Unit test: `TrainingSystem_Recovery_AwardsZeroXP_AndRestoresFatigue()`
- [ ] Unit test: `TrainingSystem_MaximumIntensity_AtHighFatigue_RaisesInjuryRisk()`
- [ ] Unit test: `TrainingSystem_LowMotivation_ReducesXpGained()`
- [ ] Unit test: `TrainingSystem_PositionSpecific_Striker_AwardsShootingAndPace()`
- [ ] Determinism test: same seed + same inputs → same `TrainingResult`

---

### Story P1-020: TrainingSystem — Weekly Diminishing Returns ✅
**Branch:** `feature/p1-020-diminishing-returns`
**Labels:** `phase-1`, `milestone:1.3`, `simulation`, `complexity:M`

> As a footballer, I want repeated training of the same type in a week to produce diminishing returns so that training variety is strategically rewarded and spamming one session type is not optimal.

**Acceptance Criteria:**
- [ ] `TrainingSystem.CalculateWeekXP(IReadOnlyList<TrainingSession> weekSessions, Position position, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` → `IReadOnlyList<TrainingResult>`
- [ ] Sessions are processed in order. For each repeated `TrainingType` within the same week: XP reduced by `30%` per previous repetition of the same type (session 1: 100%, session 2: 70%, session 3: 49%, ...)
- [ ] Diminishing returns apply **within a single week only** — next week resets
- [ ] `Recovery` sessions are exempt from diminishing returns (they don't generate XP)
- [ ] Maximum 3 training sessions per week (4th session in same week throws `InvalidOperationException` with descriptive message)
- [ ] Unit test: `TrainingSystem_TwoTechnicalSessions_SecondHas70PercentXP()`
- [ ] Unit test: `TrainingSystem_MixedTypes_NoDiminishingBetweenDifferentTypes()`
- [ ] Unit test: `TrainingSystem_FourSessions_ThrowsInvalidOperation()`

---

### Story P1-021: Unit Tests — Training System ✅
**Branch:** `feature/p1-021-training-tests`
**Labels:** `phase-1`, `milestone:1.3`, `testing`, `complexity:M`

> As a developer, I want comprehensive training system tests that cover edge cases and failure modes, including high-fatigue injury risk, motivation extremes, and determinism under seeded randomness.

**Acceptance Criteria:**
- [ ] Test: `TrainingSystem_Fatigue100_MaximumIntensity_ProducesHighInjuryRisk()` (injury risk > 10%)
- [ ] Test: `TrainingSystem_Fatigue0_AnyIntensity_ZeroInjuryRisk()`
- [ ] Test: `TrainingSystem_Gym_AwardsStrengthAndStamina_NotTechnical()`
- [ ] Test: `TrainingSystem_StatisticalBalance_Over1000Seeds_MeanXpWithin10PercentOfExpected()`
- [ ] Test: `TrainingSystem_DeterminismTest_SameSeedSameResult()`

---

## MILESTONE 1.4 — Fatigue & Recovery System ✅ Complete
*PR #60 merged | Issues #41–#43 closed*

### Story P1-024: FatigueSystem — Core Accumulation ✅
**Branch:** `feature/p1-024-fatigue-system`
**Labels:** `phase-1`, `milestone:1.4`, `simulation`, `complexity:M`

> As a footballer, I want fatigue to accumulate realistically from training, matches, and travel so that I must manage my energy as a genuine resource across a week.

**Acceptance Criteria:**
- [ ] `FatigueSystem` class in `FootballLife.Simulation` with all static methods (stateless)
- [ ] `FatigueSystem.ApplyTraining(PlayerState state, TrainingResult result)` → `PlayerState` with updated `Fatigue` (Fatigue += result.FatigueCost, clamped [0f, 100f])
- [ ] `FatigueSystem.ApplyMatch(PlayerState state, int minutesPlayed, float matchIntensity)` → `PlayerState` (full 90 mins at 1.0 adds 25 fatigue, proportional for subs, 80+ stamina reduces fatigue gain by 15%)
- [ ] `FatigueSystem.ApplyRest(PlayerState state, int hoursSlept)` → `PlayerState` (8h: -12 fatigue, each hour above 8: -1.5 capped at 10h=-15; below 6h: no recovery + adds 3 fatigue)
- [ ] `FatigueSystem.ApplyDayTick(PlayerState state)` → `PlayerState` (passive daily recovery: -3 fatigue; no recovery if Fitness < 40)
- [ ] All returned `PlayerState` instances are new immutable records
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `FatigueSystem_FullMatch_AddsExpectedFatigue()`
- [ ] Unit test: `FatigueSystem_HighStamina_ReducesMatchFatigueCost()`
- [ ] Unit test: `FatigueSystem_PoorSleep_AddsFatigue()`
- [ ] Unit test: `FatigueSystem_RecoveryTraining_ReducesFatigue()`
- [ ] Unit test: `FatigueSystem_FatigueClampsAtHundred_NeverExceeds()`

---

### Story P1-025: FatigueSystem — Effect on Performance ✅
**Branch:** `feature/p1-025-fatigue-effects`
**Labels:** `phase-1`, `milestone:1.4`, `simulation`, `complexity:M`

> As the match simulation, I need a way to compute a fatigue-adjusted effective ability rating so that fatigued players genuinely perform worse, creating real stakes around rest decisions.

**Acceptance Criteria:**
- [ ] `FatigueSystem.ComputeEffectiveAbility(PlayerAbilities abilities, PlayerState state, AttributeName attribute)` → `float` in [0, 100]
- [ ] Formula: `effectiveAbility = baseAbility * (1 - fatigueImpact * attributeSensitivity)`
  - `fatigueImpact = max(0, (state.Fatigue - 30f) / 70f)` — no penalty below 30 fatigue
  - `attributeSensitivity`: Physical = 1.0×, Technical = 0.6×, Mental = 0.4×
- [ ] Result clamped to [0f, baseAbility] — fatigue never boosts an attribute
- [ ] `FatigueSystem.ComputeFormModifier(PlayerState state)` → `float` in [0.7, 1.2]
  - Combines Form, Confidence, Motivation: `0.7 + (Form/100f * 0.2) + (Confidence/100f * 0.15) + (Motivation/100f * 0.15)`, clamped [0.7, 1.2]
- [ ] Unit test: `FatigueSystem_Fatigue80_PhysicalAttribute_HasSignificantPenalty()`
- [ ] Unit test: `FatigueSystem_Fatigue80_ComposureAttribute_HasMinorPenalty()`
- [ ] Unit test: `FatigueSystem_LowFatigue_NoEffectOnAbility()`
- [ ] Unit test: `FatigueSystem_FormModifier_HighAll_ProducesMaxModifier()`

---

### Story P1-026: Unit Tests — Fatigue System ✅
**Branch:** `feature/p1-026-fatigue-tests`
**Labels:** `phase-1`, `milestone:1.4`, `testing`, `complexity:S`

> Integration tests ensuring the fatigue pipeline is coherent across a simulated week.

**Acceptance Criteria:**
- [ ] Test: `FatigueSystem_WeekSimulation_TrainMondayMatchSaturday_ExpectedFatigueAtWeekEnd()` (expected range [45–65])
- [ ] Test: `FatigueSystem_RestWeek_NoTrainingNoMatch_FatigueFallsBelow20()`
- [ ] Test: `FatigueSystem_Overtraining_ThreeSessions_MatchDay_ExtremelyHighFatigue()`

---

## MILESTONE 1.5 — Match Simulation v1 ✅ Complete
*PR #61 merged | Issues #44–#49 closed*

### Story P1-028: Match Domain Model ✅
**Branch:** `feature/p1-028-match-domain-model`
**Labels:** `phase-1`, `milestone:1.5`, `domain`, `complexity:M`

> As the match simulation engine, I need a `MatchState` domain model tracking score, time, and event log so that a match can be serialized, debugged, and replayed from any point.

**Acceptance Criteria:**
- [ ] `MatchState` record: `Guid HomeClubId`, `Guid AwayClubId`, `int HomeScore`, `int AwayScore`, `int Minute` [0–120], `bool IsFinished`, `IReadOnlyList<MatchEvent> EventLog`
- [ ] `MatchEvent` record: `int Minute`, `MatchEventType Type`, `Guid PlayerId`, `string Description`
- [ ] `MatchEventType` enum: `Goal`, `Assist`, `YellowCard`, `RedCard`, `Substitution`, `Miss`, `Save`, `TackleWon`, `Foul`, `PenaltyScored`, `PenaltyMissed`
- [ ] `MatchState.AddEvent(MatchEvent e)` → new `MatchState` with event appended
- [ ] `MatchState.WithGoal(int minute, Guid scorerId, Guid? assisterId, bool isHome)` → new `MatchState` with score updated and events appended
- [ ] `MatchResult` record (post-match summary): `int HomeScore`, `int AwayScore`, `IReadOnlyList<MatchEvent> Events`, `float PlayerRating` [1.0–10.0], `int PlayerMinutesPlayed`, `bool PlayerScored`, `bool PlayerAssisted`
- [ ] `MatchResult.IsWin(bool playerWasHome)`, `IsDraw()`, `IsLoss(bool playerWasHome)` computed properties
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `MatchState_ScoreUpdates_Correctly_AfterGoal()`
- [ ] Unit test: `MatchResult_IsWin_ReturnsCorrectly_ForHomeAndAway()`

---

### Story P1-029: MatchSimulation — Situation Generator ✅
**Branch:** `feature/p1-029-situation-generator`
**Labels:** `phase-1`, `milestone:1.5`, `simulation`, `complexity:L`

> As a footballer, I want the match simulation to generate position-appropriate situations so that I interact with authentic scenarios for my role, not generic football events.

**Acceptance Criteria:**
- [ ] `MatchSituationGenerator` static class in `FootballLife.Simulation`
- [ ] `GenerateSituation(MatchState state, Position playerPosition, PlayerState playerState, SimulationRandom rng)` → `MatchSituation?` (null = no player interaction this minute)
- [ ] `MatchSituation` record: `SituationType Type`, `float OpponentPressure` [0–10], `float ExpectedDifficulty` [0–1], `float PositionalAdvantage` [-1 to 1], `ActionChoice[] AvailableChoices`
- [ ] `ActionChoice` record: `MatchAction Action`, `float RiskLevel` [0–1], `float ExpectedValue` [0–1]
- [ ] Situation frequency: player receives 0–3 meaningful situations per 90-minute match; average ~2 for outfield, ~1.5 for GK
- [ ] **Position-specific situation tables (mandatory):**
  - ST: `RunningInBehind` (40%), `ReceivingInBox` (30%), `LongShot` (15%), `Pressing` (10%), `AerialChallenge` (5%)
  - LW/RW: `ReceiveBall1v1` (35%), `Cross` (25%), `CutInside` (20%), `Pressing` (15%), `Tackle` (5%)
  - CM: `ReceiveUnderPressure` (30%), `ThroughBall` (25%), `LongPass` (20%), `Pressing` (15%), `Tackle` (10%)
  - DM: `Interception` (30%), `Tackle` (30%), `DistributionPass` (25%), `PositioningRun` (15%)
  - CB: `Tackle` (35%), `AerialChallenge` (25%), `Interception` (20%), `BuildUpPass` (20%)
  - GK: `Save` (50%), `ClaimCross` (25%), `Distribution` (25%)
- [ ] Context multipliers: Losing by 2+ (+20% attacking freq, -10% defending); Winning 85+ (-30% attacking); Player fatigue > 70 (+2 OpponentPressure)
- [ ] No LINQ, no allocations in hot path — static readonly tables
- [ ] Determinism test: same match state + same seed → same situation
- [ ] Unit test: `SituationGenerator_Striker_ReceivesGoalScoringOpportunity()`
- [ ] Unit test: `SituationGenerator_Defender_ReceivesTackleAndInterceptionSituations()`
- [ ] Unit test: `SituationGenerator_Losing2Plus_AttackingPositions_GetHigherFrequency()`
- [ ] Unit test: `SituationGenerator_HighFatigue_IncreasesOpponentPressure()`

---

### Story P1-030: MatchSimulation — Action Resolver ✅
**Branch:** `feature/p1-030-action-resolver`
**Labels:** `phase-1`, `milestone:1.5`, `simulation`, `complexity:L`

> As a footballer, I want my chosen action to be resolved using my real abilities and match context so that skill matters, fatigue hurts, and outcomes have appropriate, position-authentic uncertainty.

**Acceptance Criteria:**
- [ ] `ActionResolver` static class in `FootballLife.Simulation`
- [ ] `Resolve(MatchAction action, MatchSituation situation, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` → `ActionOutcome`
- [ ] `ActionOutcome` struct (zero allocation): `bool Success`, `OutcomeType Type`, `float XpContribution`, `float ConfidenceDelta`, `float ManagerTrustDelta`
- [ ] Resolution formula:
  ```
  baseScore = weightedAbilityScore(action, abilities)   // using AttributeRelevanceMatrix
  stateScore = FatigueSystem.ComputeFormModifier(state)
  situationModifier = 1 - (situation.OpponentPressure / 10f * 0.3f)
  roll = rng.NextGaussian(baseScore * stateScore * situationModifier, stddev)
  success = roll > successThreshold(action)
  ```
- [ ] `stddev` varies by action risk: safe actions (short pass) stddev=5, risky actions (long shot, through ball) stddev=15
- [ ] Success thresholds per action type (base, vs 50-rated opponent):
  - `ShortPass`: 68, `LongPass`: 55, `Cross`: 55, `Dribble`: 60, `ThroughBall`: 48
  - `Shot_Close`: 62, `Shot_Long`: 38, `Header`: 55
  - `Tackle`: 60, `Interception`: 55, `AerialChallenge`: 60
  - `GoalkeeperSave`: 55
- [ ] On success: `XpContribution = situation.ExpectedDifficulty * 10f`; `ConfidenceDelta = +rng.NextFloat(1f, 3f)`
- [ ] On failure: `ConfidenceDelta = -rng.NextFloat(0.5f, 2f)` scaled by failure margin
- [ ] Goals award `ManagerTrustDelta = +5f`; errors leading to opposition goal = `-4f`; all other actions ±0-2f
- [ ] No `new` allocations — `ActionOutcome` is a struct
- [ ] Determinism test: identical inputs → identical `ActionOutcome`
- [ ] Unit test: `ActionResolver_HighFinishing_IncreasesGoalProbability()` (statistical over 1000 runs)
- [ ] Unit test: `ActionResolver_HighFatigue_ReducesSuccessRate()`
- [ ] Unit test: `ActionResolver_RiskyAction_HasHigherVariance_ThanSafeAction()`
- [ ] Unit test: `ActionResolver_Goal_IncreasesConfidenceAndTrust()`

---

### Story P1-031: MatchSimulation — Full Match Runner ✅
**Branch:** `feature/p1-031-match-runner`
**Labels:** `phase-1`, `milestone:1.5`, `simulation`, `complexity:M`

> As the career simulator, I need a full match runner that simulates an entire 90-minute match, producing a result, a performance score, and updated player state so that I can advance the season.

**Acceptance Criteria:**
- [ ] `MatchSimulator.Simulate(ScheduledMatch fixture, Guid playerClubId, PlayerAbilities abilities, PlayerState state, Position position, SimulationRandom rng)` → `(MatchResult result, PlayerState updatedState)`
- [ ] Internal loop: 90 ticks (one per minute). Each tick: 10% chance of generating a situation; player interacts only if situation generated
- [ ] When no player situation: background simulation advances match momentum — opposing team can score based on `opponentQuality vs playerClubQuality` delta
- [ ] Match result rating [1.0–10.0] formula: Base 5.0, +1.5 per goal, +0.75 per assist, +0.5 per key action, -1.0 per serious error; position-adjusted
- [ ] Post-match `PlayerState` updated: Fatigue += 25, Form updated toward match rating, Confidence updated from ConfidenceDelta accumulation
- [ ] Unit test: `MatchSimulator_Striker_CanScoreGoal()`
- [ ] Unit test: `MatchSimulator_90MinutesSimulated_ProducesValidRating()`
- [ ] Determinism test: same seed → same final score, same player rating

---

### Story P1-032: MatchSimulation — Manager Trust Update ✅
**Branch:** `feature/p1-032-manager-trust-update`
**Labels:** `phase-1`, `milestone:1.5`, `simulation`, `complexity:S`

> As a career simulation, I want manager trust to change after each match based on performance so that a sustained run of form (or poor performances) has visible career consequences.

**Acceptance Criteria:**
- [ ] `ManagerTrustSystem.ApplyMatchResult(PlayerCareerState careerState, MatchResult result, SquadStatus squadExpectation)` → `PlayerCareerState`
- [ ] Trust delta formula:
  - Rating 8.0+: `+4f`
  - Rating 6.5–7.9: `+1.5f`
  - Rating 5.0–6.4: `0.0f` (neutral)
  - Rating 3.5–4.9: `-2f`
  - Rating below 3.5: `-5f`
- [ ] Expectation modifier: if player is listed as `KeyPlayer` but delivers rating < 5.0: delta × 1.5
- [ ] Trust clamped to [0f, 100f] via `PlayerCareerState.ClampTrust()`
- [ ] Unit test: `ManagerTrustSystem_ExcellentPerformance_IncreasesTrust()`
- [ ] Unit test: `ManagerTrustSystem_PoorPerformance_DecreasesTrust()`
- [ ] Unit test: `ManagerTrustSystem_KeyPlayerExpectations_AmplifyPenalty()`
- [ ] Unit test: `ManagerTrustSystem_TrustNeverExceeds100()`

---

### Story P1-033: Integration Tests — Match System ✅
**Branch:** `feature/p1-033-match-integration-tests`
**Labels:** `phase-1`, `milestone:1.5`, `testing`, `complexity:M`

> Complete integration testing of the match pipeline from situation generation through outcome and state updates.

**Acceptance Criteria:**
- [ ] Test: `MatchPipeline_Striker_ScoresSomeGoals_Over100Games()` — statistical: in 100 simulated matches, striker with 80 finishing scores > 0 goals in at least 30 matches
- [ ] Test: `MatchPipeline_HighFatigue_ReducesPerformanceRating_Statistically()`
- [ ] Test: `MatchPipeline_FatigueIncreases_AfterEachMatch()`
- [ ] Test: `MatchPipeline_DeterminismTest_SameSeedProducesSameEntireMatchLog()`
- [ ] Test: `MatchPipeline_TenSeasonSimulation_TrustConverges_NotRunaway()` — run 380 matches, verify ManagerTrust stays in [10, 95] range for average players

---

## MILESTONE 1.6 — Career Progression System ✅ Complete
*PR #62 merged | Issues #50–#53 closed*

### Story P1-038: ProgressionSystem — XP to Ability Gain ✅
**Branch:** `feature/p1-038-progression-xp-to-ability`
**Labels:** `phase-1`, `milestone:1.6`, `simulation`, `complexity:L`

> As a footballer, I want accumulated XP to translate into ability improvements using an age-gated, nonlinear formula so that young players develop quickly and older players develop slowly, reflecting real-world football physiology.

**Acceptance Criteria:**
- [ ] `ProgressionSystem.ApplyXpGains(Guid playerId, WorldState world, DateOnly currentDate, SimulationRandom rng)` → `PlayerAbilities`
- [ ] XP is accumulated externally; this method converts pending XP to ability points
- [ ] Requires `WorldState.GetPotential(playerId)` to be resolvable
- [ ] **Age-gated development curve:**
  - Age 15–20: `developmentRate = 1.4` (fast development)
  - Age 21–24: `developmentRate = 1.1` (still growing)
  - Age 25–28: `developmentRate = 0.8` (mature, slow gains)
  - Age 29–31: `developmentRate = 0.4` (minimal development)
  - Age 32+: `developmentRate = 0.1` (near-zero; physical decline begins)
- [ ] **Potential ceiling:** ability gain stops at `PlayerPotential.PotentialRating`. XP is consumed but produces 0 gain once ceiling reached.
- [ ] `PotentialRange.Elite` adds ±5 ceiling randomness
- [ ] Physical attributes (`Pace`, `Acceleration`, `Stamina`) decline at age 31+: `-0.5f/year` independent of XP
- [ ] Gains are fractional (stored as float internally, truncated to byte only at serialization)
- [ ] No `new` allocations in the computation path
- [ ] Unit test: `ProgressionSystem_YoungPlayer_GainsFasterThanVeteran()`
- [ ] Unit test: `ProgressionSystem_AbilityCappedAtPotentialRating()`
- [ ] Unit test: `ProgressionSystem_PhysicalDecline_BeginsAt31()`
- [ ] Determinism test: same seed → same gains

---

### Story P1-039: CareerSystem — Playing Status Evaluation ✅
**Branch:** `feature/p1-039-playing-status-evaluation`
**Labels:** `phase-1`, `milestone:1.6`, `simulation`, `complexity:M`

> As a footballer, I want my squad status to be re-evaluated at the start of each season (and mid-season after a form run) based on my ability, manager trust, and position competition so that promotions and demotions feel earned and consequential.

**Acceptance Criteria:**
- [ ] `CareerSystem.EvaluateSquadStatus(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Club club, WorldState world)` → `SquadStatus`
- [ ] Factors considered:
  - `ManagerTrust` (40% weight)
  - `Reputation` relative to club reputation (30% weight)
  - Position competition within club (30% weight) — count of players in `Club.SquadPlayerIds` sharing position with higher overall ability
- [ ] Status thresholds (composite score 0–100):
  - 0–15: `Academy`, 16–30: `Reserve`, 31–45: `Bench`, 46–65: `Rotation`, 66–80: `Starter`, 81–100: `KeyPlayer`
- [ ] Status can only change by **one level** per evaluation cycle
- [ ] Unit test: `CareerSystem_HighTrustHighReputation_EvaluatesAsStarter()`
- [ ] Unit test: `CareerSystem_LowTrustDespiteHighAbility_EvaluatesBelowAbilityExpectation()`
- [ ] Unit test: `CareerSystem_StatusOnlyChangesOneLevel_PerEvaluation()`

---

### Story P1-040: CareerSystem — End-of-Season Statistics ✅
**Branch:** `feature/p1-040-season-stats`
**Labels:** `phase-1`, `milestone:1.6`, `domain`, `simulation`, `complexity:M`

> As a footballer, I want comprehensive season statistics aggregated at the end of each season so that I can see my career arc and the career simulator can validate balance distributions.

**Acceptance Criteria:**
- [ ] `SeasonStats` record in `FootballLife.Domain`: `int Appearances`, `int Goals`, `int Assists`, `float AverageRating`, `int YellowCards`, `int RedCards`, `decimal WageEarned`, `SquadStatus FinalStatus`, `float AttributeGrowthAverage`
- [ ] `CareerSystem.AggregateSeasonStats(IReadOnlyList<MatchResult> results, PlayerCareerState finalCareerState, decimal weeklyWage, int weeksInSeason)` → `SeasonStats`
- [ ] `AverageRating`: mean of all `result.PlayerRating` values for matches where `MinutesPlayed > 0`
- [ ] `AttributeGrowthAverage`: mean of all attribute deltas from start of season to end
- [ ] `WageEarned`: `weeklyWage * weeksInSeason`
- [ ] Unit test: `CareerSystem_SeasonStats_CorrectGoalAndAssistCounts()`
- [ ] Unit test: `CareerSystem_AverageRating_ExcludesDidNotPlayMatches()`

---

### Story P1-041: Integration Tests — Progression & Career ✅
**Branch:** `feature/p1-041-progression-tests`
**Labels:** `phase-1`, `milestone:1.6`, `testing`, `complexity:L`

> Multi-season integration tests validating that development curves and career trajectories are statistically valid.

**Acceptance Criteria:**
- [ ] Test: `Progression_FiveSeasonCareer_YoungStriker_ReachesExpectedAbilityRange()` (17yo starting at 50 overall reaches 60–80 at 22)
- [ ] Test: `Progression_VeteranPlayer_Age34_MinimalAbilityGain_PhysicalDeclineVisible()`
- [ ] Test: `CareerSimulator_100Careers_PeakOverall_Mean70to80_NoneReach99Before24()`
- [ ] Test: `CareerSimulator_DeterministicSeed42_IdenticalCareerStats()`

---

## 🛑 MILESTONE 1.6.5 — Validation Gate (Minimum Playable Loop) ✅ Complete
*PR #63 merged | Issues #54–#57 closed | Validation Decision: GO*

> **This was a validation checkpoint, not a feature milestone.** The playable loop was built, exercised, and formally approved.
> **Documentation:** [`docs/gate-review.md`](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/gate-review.md)

### Story P1-GATE-1: Minimal Console Harness ✅
**Branch:** `feature/p1-gate1-console-harness`
**Labels:** `phase-1`, `milestone:1.6.5`, `simulation`, `complexity:S`

> As a game designer, I want a minimal interactive console mode that lets me manually play 10+ in-game weeks of the core loop so I can evaluate whether the simulation produces meaningful decisions.

**Acceptance Criteria:**
- [ ] `dotnet run --project simulation/FootballLife.CareerSimulator -- --interactive` launches an interactive text loop
- [ ] Each loop iteration presents: current week, fatigue, form, next match (if any), training choices
- [ ] Player types a choice: `1` (Technical), `2` (Physical), `3` (Recovery), `4` (Skip ahead to match)
- [ ] On match week: simulate match, print result, rating, trust delta
- [ ] On non-match week: simulate training, print XP gained per attribute, new fatigue
- [ ] Each week prints player state: `Fatigue: X | Form: X | Confidence: X | Trust: X`
- [ ] Session ends after 38 weeks (one season) or player types `q`
- [ ] Uses real (non-mocked) Training/Fatigue/MatchSimulation/Progression systems — no stubs
- [ ] Starting player: age 19, position ST, abilities all 55, at "Tier 2" club

---

### Story P1-GATE-2: Human Playtest Validation ✅
**Branch:** `feature/p1-gate2-playtest`
**Labels:** `phase-1`, `milestone:1.6.5`, `quality`, `complexity:S`

> Run the console harness manually for 15–20 real minutes across 2–3 different player positions to observe emergent behavior and decision friction.

**Acceptance Criteria:**
- [ ] Run interactive console harness for 15–20 minutes with Striker position
- [ ] Run interactive console harness with Midfielder or Defender position
- [ ] Document raw playtest observations (decision trade-offs, progression pacing, fatigue tension)

---

### Story P1-GATE-3: Human Validation - Gate Review Documentation ✅
**Branch:** `docs/gate-review`
**Labels:** `phase-1`, `milestone:1.6.5`, `documentation`, `quality`, `complexity:S`

> Document honest review answers in `docs/gate-review.md` evaluating decision significance, fatigue friction, and match stakes to inform the Go/No-Go gate decision.

**Acceptance Criteria:**
- [ ] Create `docs/gate-review.md` answering:
  - Did any training decision feel meaningful? (e.g., "I rested instead of training and performed better")
  - Did fatigue feel like a real constraint or just a number?
  - Did matches feel different based on pre-match preparation?
  - Was there any moment of "I made the wrong call"?
  - What was boring?
- [ ] Commit `docs/gate-review.md` to repository

---

### Story P1-GATE-4: Go/No-Go Decision Checkpoint ✅
**Branch:** `checkpoint/p1-gate4-decision`
**Labels:** `phase-1`, `milestone:1.6.5`, `quality`, `complexity:S`

> Conduct a formal Go/No-Go evaluation before proceeding to Milestone 1.7 (Economy). Identify and tune specific simulation parameters if adjustments are required.

**Acceptance Criteria:**
- [ ] Evaluate playtest observations and gate review results
- [ ] If GO: Approve progression to Milestone 1.7
- [ ] If NO-GO: Identify specific simulation tuning parameters (ActionResolver thresholds, XP scales, fatigue rates) and create tuning PR before opening Milestone 1.7 issues

---

## Implementation Note: System Interface Protocol

The following handoffs between systems must be respected:

```
TrainingSystem.CalculateXP() 
    → TrainingResult.FatigueCost 
    → FatigueSystem.ApplyTraining()
    → PlayerState (updated)

FatigueSystem.ComputeEffectiveAbility()
    ↑ consumed by
    → ActionResolver.Resolve()
    → ActionOutcome

ActionOutcome.XpContribution
    → accumulated externally per week
    → ProgressionSystem.ApplyXpGains()
    → PlayerAbilities (updated)

MatchResult
    → ManagerTrustSystem.ApplyMatchResult()
    → PlayerCareerState (updated trust)
    → CareerSystem.EvaluateSquadStatus()
    → SquadStatus (updated)
```

---

## MILESTONE 1.7 — Economy System ✅ Complete

### Story P1-044: FinanceAccount Domain Model ✅
**Branch:** `feature/p1-044-finance-account`
**Labels:** `phase-1`, `milestone:1.7`, `domain`, `complexity:S`
**Status:** Merged in PR #69 (Issue #64)

> As a footballer, I want a `FinanceAccount` domain model tracking my financial balance and transaction history so that I can manage my earnings, expenses, and savings over time.

**Acceptance Criteria:**
- [x] `FinanceAccount` record in `FootballLife.Domain`: `decimal Balance`, `IReadOnlyList<FinanceTransaction> History`
- [x] `FinanceTransaction` record: `Guid Id`, `DateOnly Date`, `TransactionType Type`, `decimal Amount`, `string Description`
- [x] `TransactionType` enum: `Salary`, `MatchBonus`, `LifestyleExpense`, `Fine`, `Investment`, `TransferBonus`
- [x] `FinanceAccount.WithTransaction(FinanceTransaction tx)` → returns new immutable `FinanceAccount` with updated `Balance` (`Balance + tx.Amount`) and transaction appended to `History`
- [x] `FinanceAccount.Create(decimal initialBalance = 0)` factory method
- [x] Unit test: `FinanceAccount_InitialBalance_IsZero_ByDefault()`
- [x] Unit test: `FinanceAccount_PositiveTransaction_IncreasesBalance()`
- [x] Unit test: `FinanceAccount_NegativeTransaction_DecreasesBalance()`
- [x] Unit test: `FinanceAccount_HistoryPreservesChronologicalOrder()`

---

### Story P1-045: EconomySystem — Weekly Salary Credit ✅
**Branch:** `feature/p1-045-salary-credit`
**Labels:** `phase-1`, `milestone:1.7`, `simulation`, `complexity:S`
**Status:** Merged in PR #69 (Issue #65)

> As a footballer, I want my weekly contracted salary automatically credited to my bank account each week so that I have funds to spend on lifestyle and investments.

**Acceptance Criteria:**
- [x] `EconomySystem` static class in `FootballLife.Simulation`
- [x] `EconomySystem.ApplyWeeklySalary(FinanceAccount account, decimal weeklySalary, DateOnly date)` → `FinanceAccount`
- [x] Creates a `Salary` transaction with amount = `weeklySalary`
- [x] Enforces non-negative salary parameter (`weeklySalary >= 0`)
- [x] Unit test: `EconomySystem_WeeklySalary_CreditsContractedAmount()`
- [x] Unit test: `EconomySystem_ZeroSalary_StillCreatesTransactionRecord()`

---

### Story P1-046: EconomySystem — Match Bonuses ✅
**Branch:** `feature/p1-046-match-bonuses`
**Labels:** `phase-1`, `milestone:1.7`, `simulation`, `complexity:S`
**Status:** Merged in PR #69 (Issue #66)

> As a footballer, I want my performance match bonuses (appearance, goal, assist, clean sheet) credited after matches so that strong on-pitch performance directly rewards my personal wealth.

**Acceptance Criteria:**
- [x] `EconomySystem.ApplyMatchBonuses(FinanceAccount account, ContractBonuses bonuses, MatchResult result, Position position, DateOnly date)` → `FinanceAccount`
- [x] Evaluates bonuses earned:
  - Appearance: `result.PlayerMinutesPlayed > 0 ? bonuses.AppearanceBonus : 0`
  - Goals: `result.PlayerScored ? (goalsCount * bonuses.GoalBonus) : 0`
  - Assists: `result.PlayerAssisted ? (assistsCount * bonuses.AssistBonus) : 0`
  - Clean Sheet: `(position == GK || position == CB || position == FB) && result.OpponentScore == 0 && result.PlayerMinutesPlayed >= 60 ? bonuses.CleanSheetBonus : 0`
- [x] Aggregates non-zero bonuses into transaction with itemized breakdown in `Description`
- [x] Unit test: `EconomySystem_MatchBonus_CalculatesGoalAndAppearanceCorrectly()`
- [x] Unit test: `EconomySystem_DefenderCleanSheet_AwardsBonusWhenEligible()`
- [x] Unit test: `EconomySystem_DidNotPlay_ZeroBonusEarned()`

---

### Story P1-047: EconomySystem — Lifestyle Expense Deductions ✅
**Branch:** `feature/p1-047-lifestyle-expenses`
**Labels:** `phase-1`, `milestone:1.7`, `simulation`, `complexity:M`
**Status:** Merged in PR #69 (Issue #67)

> As a footballer, I want weekly lifestyle expenses deducted according to my living tier so that higher luxury requires maintaining high earnings to avoid debt.

**Acceptance Criteria:**
- [x] `LifestyleTier` enum in `FootballLife.Domain`: `Modest` (£150/wk), `Comfortable` (£500/wk), `Luxurious` (£2,000/wk), `Extravagant` (£8,000/wk), `Superstar` (£25,000/wk)
- [x] `EconomySystem.ApplyLifestyleExpenses(FinanceAccount account, LifestyleTier tier, DateOnly date)` → `FinanceAccount`
- [x] Deducts standard weekly cost as negative amount transaction of type `LifestyleExpense`
- [x] If balance goes negative: sets mood/happiness penalty modifier (returned or recorded via event)
- [x] Unit test: `EconomySystem_LifestyleExpenses_DeductsAccurateTierCost()`
- [x] Unit test: `EconomySystem_Overdraft_AllowsNegativeBalance_WithEventFlag()`

---

### Story P1-048: Unit & Integration Tests — Economy System ✅
**Branch:** `feature/p1-048-economy-tests`
**Labels:** `phase-1`, `milestone:1.7`, `testing`, `complexity:M`
**Status:** Merged in PR #69 (Issue #68)

> Full test suite verifying economy transactions, multi-season financial stability, and zero-allocation compliance.

**Acceptance Criteria:**
- [x] Test: `EconomySystem_FullSeasonAccumulation_38Weeks_NetWorthMatchesFormula()`
- [x] Test: `EconomySystem_DeterministicFinancialTrajectory_SameContractSamePerformance()`
- [x] Test: `EconomySystem_DebtPenalties_TriggerCorrectlyWhenBalanceDepleted()`

---

## MILESTONE 1.8 — Life Events System (v1) ✅ Complete

### Story P1-049: LifeEvent Domain Model ✅
**Branch:** `feature/p1-049-life-event-model`
**Labels:** `phase-1`, `milestone:1.8`, `domain`, `complexity:M`
**Status:** Merged in PR #76 (Issue #70)

> As the narrative engine, I want a `LifeEvent` domain model encoding conditions, choices, and consequences so that personal life dilemmas are data-driven and systemic.

**Acceptance Criteria:**
- [x] `LifeEvent` record in `FootballLife.Domain`: `string Id`, `string Title`, `string Description`, `EventCategory Category`, `IReadOnlyList<EventChoice> Choices`, `EventPreconditions Conditions`, `int Weight`, `int CooldownWeeks`
- [x] `EventChoice` record: `string Id`, `string Text`, `IReadOnlyList<EventEffect> Effects`
- [x] `EventEffect` record: `EffectTarget Target`, `float Delta`, `string Description`
- [x] Unit test: `LifeEvent_Serialization_RoundTripsFromJson()`

---

### Story P1-050: LifeEventSystem — Condition Evaluator ✅
**Branch:** `feature/p1-050-event-conditions`
**Labels:** `phase-1`, `milestone:1.8`, `simulation`, `complexity:M`
**Status:** Merged in PR #76 (Issue #71)

> As the life simulation, I want conditions evaluated against current player state and career context so that only plausible events trigger.

**Acceptance Criteria:**
- [x] `LifeEventSystem.EvaluateConditions(LifeEvent ev, Player player, PlayerState state, PlayerCareerState career, FinanceAccount finance, WorldState world)` → `bool`
- [x] Checks age min/max, fatigue min/max, salary bounds, marital/relationship status, manager trust
- [x] Unit test: `LifeEventSystem_HighSalaryEvent_DoesNotTriggerForYouthPlayer()`

---

### Story P1-051: LifeEventSystem — Weighted Selection ✅
**Branch:** `feature/p1-051-event-selection`
**Labels:** `phase-1`, `milestone:1.8`, `simulation`, `complexity:M`
**Status:** Merged in PR #76 (Issue #72)

> As the life simulation, I want probabilistic event selection weighted by current player situation so that narrative pacing is dynamic yet predictable with seed.

**Acceptance Criteria:**
- [x] `LifeEventSystem.SelectWeeklyEvent(IReadOnlyList<LifeEvent> pool, Player player, WorldState world, SimulationRandom rng)` → `LifeEvent?`
- [x] Evaluates cooldowns, filters eligible events, rolls weighted random
- [x] Determinism test: same seed → identical chosen event sequence

---

### Story P1-052: LifeEventSystem — Effect Applicator ✅
**Branch:** `feature/p1-052-event-effects`
**Labels:** `phase-1`, `milestone:1.8`, `simulation`, `complexity:M`
**Status:** Merged in PR #76 (Issue #73)

> As the simulation, I want chosen event effects applied to mutate player state, relationships, or finances deterministically.

**Acceptance Criteria:**
- [x] `LifeEventSystem.ApplyChoice(EventChoice choice, Player player, WorldState world)` → `WorldState`
- [x] Mutates relevant state (Fatigue, Happiness, Confidence, Trust, Finances)
- [x] Unit test: `LifeEventSystem_ChoiceReducesFatigue_AndIncreasesHappiness()`

---

### Story P1-053: Seed Data (`events.json`) ✅
**Branch:** `feature/p1-053-seed-events`
**Labels:** `phase-1`, `milestone:1.8`, `data`, `complexity:L`
**Status:** Merged in PR #76 (Issue #74)

> Canonical data file containing 20 core life events covering press dilemmas, sponsor offers, social nights out, family requests, and training controversies.

**Acceptance Criteria:**
- [x] `content/data/events.json` with 20 diverse, balanced life events
- [x] Schema validation tests verify all event IDs, choices, and effects

---

### Story P1-054: Unit & Integration Tests — Life Events System ✅
**Branch:** `feature/p1-054-life-events-tests`
**Labels:** `phase-1`, `milestone:1.8`, `testing`, `complexity:M`
**Status:** Merged in PR #76 (Issue #75)

> Comprehensive tests ensuring life events fire reliably, observe cooldowns, and mutate state without corrupting invariants.

**Acceptance Criteria:**
- [x] Test: `LifeEventSystem_CooldownObserved_CannotFireSameEventConsecutively()`
- [x] Test: `LifeEventSystem_StatisticalSpread_Over1000Weeks()`

---

## MILESTONE 1.9 — Relationships ✅ Complete

### Story P1-055: Relationship Model ✅
**Branch:** `feature/milestone-1.9-relationships`
**Status:** Merged in PR #82 (Issue #77)

> As a life simulation, I want relationship entities with emotional context so that the game presents people as people rather than numeric bars.

**AC:**
- [x] `Relationship` record: `Guid Id`, `Guid PersonId`, `RelationshipType Type`, `float Affinity` [0–100], `float Trust` [0–100], `DateOnly LastInteraction`, `IReadOnlyList<string> SharedHistory`
- [x] `RelationshipType` enum: `Partner`, `Parent`, `Sibling`, `Friend`, `Teammate`, `Manager`, `Agent`
- [x] Affinity decays 0.5 per week without interaction (but never below 20 for family)
- [x] Unit test: `Relationship_Affinity_DecaysFromNeglect()`
- [x] Unit test: `Relationship_FamilyAffinity_HasMinimumFloor()`

### Story P1-056: Affinity Decay from Neglect ✅
**Branch:** `feature/milestone-1.9-relationships`
**Status:** Merged in PR #82 (Issue #78)

> As a player, I want relationships to naturally cool down when neglected so that maintaining friendships and family ties requires deliberate effort and time investment.

**AC:**
- [x] `RelationshipSystem.ApplyWeeklyDecay(WorldState world)` evaluates weeks since `LastInteraction`
- [x] Weekly decay rate: default 0.5 affinity points per uncontacted week
- [x] Family members (`Parent`, `Sibling`) maintain a baseline affinity floor of 20.0 regardless of time passed
- [x] Non-family members can decay down to 0.0 affinity
- [x] Unit test: `RelationshipSystem_WeeklyDecay_ReducesAffinity()`
- [x] Unit test: `RelationshipSystem_FamilyDecay_RespectsFloor()`

### Story P1-057: Interaction Events Affecting Affinity ✅
**Branch:** `feature/milestone-1.9-relationships`
**Status:** Merged in PR #82 (Issue #79)

> As a player, I want positive and negative interactions (dinners, gifts, arguments, praise) to directly mutate relationship affinity and trust so that my choices have social consequences.

**AC:**
- [x] `RelationshipSystem.RecordInteraction(WorldState world, Guid relationshipId, float affinityDelta, float trustDelta, string context, DateOnly date)`
- [x] Appends context note to `SharedHistory` log
- [x] Updates `LastInteraction` date
- [x] Clamps `Affinity` and `Trust` between 0.0 and 100.0
- [x] Integration with `LifeEventEffect`: life events can directly target relationship affinity/trust
- [x] Unit test: `RelationshipSystem_RecordInteraction_UpdatesAffinityAndHistory()`

### Story P1-058: Club Transfer Impact on Relationships ✅
**Branch:** `feature/milestone-1.9-relationships`
**Status:** Merged in PR #82 (Issue #80)

> As a player, I want moving clubs to impact my social circle — leaving teammates behind, straining distant relationships, and opening opportunities for new bonds.

**AC:**
- [x] `RelationshipSystem.ApplyClubTransfer(WorldState world, Club oldClub, Club newClub)`
- [x] High-affinity teammates (>= 75) transition to long-distance friends with trust bonus
- [x] Low/average teammates (< 75) experience social distance decay
- [x] Old manager relationship cools (-10 affinity, -5 trust)
- [x] Partner relationship reflects relocation strain if affinity is low (< 50) or support if high (>= 50)
- [x] Unit test: `RelationshipSystem_ClubTransfer_ImpactsTeammatesAndManager()`

### Story P1-059: Unit & Integration Tests — Relationship Dynamics ✅
**Branch:** `feature/milestone-1.9-relationships`
**Status:** Merged in PR #82 (Issue #81)

> As a game engineer, I want comprehensive test coverage across all relationship mechanics and edge cases so that relationship dynamics remain balanced and regression-free.

**AC:**
- [x] Test suite covering `Relationship` domain invariants, clamping, and immutability
- [x] Test suite covering weekly decay, multi-week elapsed time, and family floor protection
- [x] Test suite covering positive/negative interactions, history log appending, and date updates
- [x] Test suite covering club transfer relocation scenarios
- [x] Headless and interactive runner integration verified (16 dedicated tests, 489 total tests passing)

---

## MILESTONE 1.10 — Transfer & Contract System ✅ Complete

### Story P1-060: TransferOffer Domain Model ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #83)

> As a simulation system, I want a structured `TransferOffer` domain model representing official transfer and contractual proposals between clubs and players so that career movements are legally and financially well-defined.

**AC:**
- [x] `TransferOffer` immutable record in `FootballLife.Domain` (`Id`, `PlayerId`, `OfferingClubId`, `OfferedRole`, `OfferedWage`, `TransferFee`, `ContractYears`, `ReleaseClause`, `SigningBonus`, `OfferDate`, `ExpiryDate`, `Status`)
- [x] `TransferOfferStatus` enum: `Pending`, `Accepted`, `Rejected`, `Expired`, `Withdrawn`
- [x] Factory method `TransferOffer.Create(...)` and transition `WithStatus(TransferOfferStatus status)`
- [x] Invariant validation: positive wage (> 0), non-negative financials, valid date ordering, contract years in [1, 5]
- [x] `WorldState` integration with `TransferOffers` dictionary, lookup helpers, and mutation methods
- [x] Unit test: `TransferOffer_ConstructsAndValidatesParameters()`

### Story P1-061: Transfer Offer Generation ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #84)

> As a footballer, I want transfer offers to appear when my reputation, form, and position need align with a club's requirements so that transfer opportunities feel earned and believable.

**AC:**
- [x] `TransferSystem.GenerateOffers(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Contract currentContract, WorldState world, SimulationRandom rng, DateOnly currentDate)`
- [x] Suitor clubs filtered by matching `LeagueTier` ± 1 relative to player ability/reputation (elite clubs for top performers)
- [x] Current club strictly excluded from suitor bids
- [x] Wage calculation scaled exponentially by ability, tier multiplier, and squad role
- [x] Minimum 1 escape offer guaranteed during transfer windows if player `ManagerTrust` < 30
- [x] Maximum 3 simultaneous active offers
- [x] Determinism test: same seed + same world → same offers
- [x] Unit test: `GenerateOffers_GeneratesSuitorOffersExcludingCurrentClub()`
- [x] Unit test: `GenerateOffers_LowManagerTrust_GuaranteesEscapeOffer()`

### Story P1-062: Transfer Acceptance & Rejection Flow ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #85)

> As a player, I want to accept or reject transfer offers, seamlessly moving to the new club, registering my new contract, receiving my signing bonus, and adjusting my relationships.

**AC:**
- [x] `TransferSystem.AcceptOffer(TransferOffer offer, WorldState world, DateOnly transferDate)`
- [x] Creates and registers new `Contract` in `WorldState`
- [x] Updates `PlayerCareerState` with new `ClubId`, `WeeklySalary`, `SquadStatus`, and role-based initial `ManagerTrust`
- [x] Updates `Club` squad rosters: removes player from old club, adds to new club
- [x] Credits `SigningBonus` directly to player's `FinanceAccount` as `TransactionType.TransferBonus`
- [x] Invokes `RelationshipSystem.ApplyClubTransfer` for teammate distancing and manager cool-down
- [x] Updates accepted offer status and automatically marks other pending offers as `Withdrawn`
- [x] `TransferSystem.RejectOffer(TransferOffer offer)` sets status to `Rejected`
- [x] Unit test: `AcceptOffer_TransfersRosterContractAndFinancials()`
- [x] Unit test: `AcceptOffer_WithdrawnOtherPendingOffers()`

### Story P1-063: Contract Negotiation Simulation ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #86)

> As a footballer and agent, I want to negotiate wage, role, and duration terms with prospective or existing clubs so that I can maximize my earnings and secure playing time.

**AC:**
- [x] `ContractSystem.NegotiateTerms(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Club club, decimal demandedWage, int demandedYears, WorldState world, SimulationRandom rng)`
- [x] `ContractNegotiationResult` record tracking outcome (`Accepted`, `CounterOffer`, `WalkedAway`), agreed wage, years, and bonus
- [x] Club wage budget and tolerance curves based on club reputation and player overall
- [x] Agent relationship leverage: high agent affinity provides up to +15% wage acceptance tolerance
- [x] Counter-offer logic when demands are within compromise band (1.10x to 1.30x)
- [x] Walk-away logic when demands exceed club ceiling (> 1.30x)
- [x] Unit test: `NegotiateTerms_ReasonableDemand_Accepted()`
- [x] Unit test: `NegotiateTerms_SlightlyElevatedDemand_Counters()`
- [x] Unit test: `NegotiateTerms_HighAgentAffinity_IncreasesWageAcceptanceThreshold()`

### Story P1-064: Contract Expiry, Renewal & Free Agency ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #87)

> As a footballer, I want my contract to progress toward expiry, allowing renewal negotiations, pre-contract Bosman moves, or free agency release when a contract runs out.

**AC:**
- [x] `ContractSystem.EvaluateContractStatus(Contract contract, DateOnly currentDate)` returns `ContractExpiryStatus` (`Active`, `BosmanEligible`, `Expired`)
- [x] Bosman eligibility triggered when remaining contract duration <= 6 months (<= 182 days)
- [x] `ContractSystem.OfferRenewal(...)` generates renewal offer if manager trust >= 30, refuses if trust < 30
- [x] `ContractSystem.HandleContractExpiry(Player player, Contract contract, WorldState world, DateOnly currentDate)`
- [x] Expired players transition to Free Agent (`ClubId = Guid.Empty`, `WeeklySalary = 0m`, removed from squad roster)
- [x] Free agents remain eligible for transfer offers with zero transfer fee
- [x] Unit test: `EvaluateContractStatus_SixMonthsOrLess_ReturnsBosmanEligible()`
- [x] Unit test: `HandleContractExpiry_ExpiredContract_ReleasesPlayerToFreeAgency()`

### Story P1-065: Unit & Integration Tests — Transfer Scenarios & Contract Lifecycles ✅
**Branch:** `feature/milestone-1.10-transfers`
**Status:** Merged in PR #89 (Issue #88)

> Comprehensive unit and integration test suite validating transfer offer generation, contract negotiations, free agency, and career stability.

**AC:**
- [x] Test suite covering `TransferOffer` domain construction, bounds checking, and immutability
- [x] Test suite covering `TransferSystem` suitor matching, escape offers, determinism, and roster execution
- [x] Test suite covering `ContractSystem` negotiation bargaining curves, agent leverage, Bosman timelines, and free agency
- [x] CLI and headless runner integration verified (24 dedicated tests, 513 total tests passing)

---

## MILESTONE 1.11 — Career Simulator CLI ✅ Complete
*PR #94 merged | Issues #90–#93 closed*

### Story P1-066: End-to-End Multi-Season Career Runner ✅
**Branch:** `feature/milestone-1.11-career-simulator`
**Status:** Merged in PR #94 (Issue #90)

> As a game designer and developer, I want a headless career runner that wires together all Phase 1 simulation systems into a multi-season career lifecycle so that player careers can simulate continuously from rookie debut to retirement.

**AC:**
- [x] `CareerSimulationEngine` wires all Phase 1 domain models and systems:
  - Weekly wages and lifestyle expenses via `EconomySystem`
  - Training sessions, XP progression, and fatigue accumulation via `TrainingSystem`, `ProgressionSystem`, `FatigueSystem`
  - Matchday simulation via `MatchSimulator` and `ManagerTrustSystem`
  - Match bonus distribution via `EconomySystem.ApplyMatchBonuses`
  - Social dynamics and relationship decay via `RelationshipSystem`
  - Transfer and contract status evaluation via `ContractSystem` and `TransferSystem`
  - Natural aging, skill progression/decline curves, and retirement evaluation
- [x] Multi-season careers run up to 20 seasons or retirement
- [x] Retirement criteria: age ceiling (38), age >= 35 with physical decline, or prolonged free agency
- [x] Deterministic execution: identical seed produces identical career trajectory

### Story P1-067: Statistical Metrics & Per-Career Export (CSV/JSON) ✅
**Branch:** `feature/milestone-1.11-career-simulator`
**Status:** Merged in PR #94 (Issue #91)

> As a game designer, I want detailed per-career tracking and export options (CSV and JSON) so that I can inspect individual player trajectories, peak ratings, financial health, and transfer histories.

**AC:**
- [x] Define `CareerStatistics` and `SeasonRecord` immutable records
- [x] Track `Seed`, `PlayerId`, `Name`, `Position`, `StartingOverall`, `PeakOverall`, `PeakAge`, `RetirementAge`, `SeasonsPlayed`, `TotalAppearances`, `TotalGoals`, `TotalAssists`, `AverageRating`, `TotalEarnings`, `FinalBalance`, `PeakWeeklySalary`, `BankruptcyOccurred`, `TransferCount`
- [x] Export to CSV via `--csv <path>` matching schema header
- [x] Export to JSON via `--json <path>`

### Story P1-068: Bulk Multi-Career Execution & Aggregate Distributions ✅
**Branch:** `feature/milestone-1.11-career-simulator`
**Status:** Merged in PR #94 (Issue #92)

> As a game designer, I want to run bulk simulations of 1,000 to 10,000 careers and compute aggregate statistical distributions so that I can validate system balance, economy health, and progression pacing across the player population.

**AC:**
- [x] CLI flags supported: `--careers`, `--seasons`, `--seed`, `--parallel`, `--quiet`, `--csv`, `--json`
- [x] `BulkSimulationRunner` high-throughput orchestrator executing sequential and parallel batch runs
- [x] `AggregateReport` calculating Mean, Median (P50), P10, P90, Min, Max, StdDev, and bankruptcy rate
- [x] ASCII histograms generated for Peak Overall and Retirement Age distributions
- [x] High-speed performance: 1,000 careers completed in ~16s (60+ careers/sec)

### Story P1-069: Balance Validation & Distribution Assertions ✅
**Branch:** `feature/milestone-1.11-career-simulator`
**Status:** Merged in PR #94 (Issue #93)

> As a development team, I want automated balance assertion tests in the test suite so that any regression in XP curves, match difficulty, finances, transfers, or aging immediately fails CI.

**AC:**
- [x] `CareerSimulatorBalanceTests.cs` and `CareerSimulatorTests.cs` test fixtures in `FootballLife.Simulation.Tests`
- [x] Peak overall distribution validated: mean between 65.0 and 75.0, P10 >= 58, P90 <= 85, no player reaching 90+ overall before age 24
- [x] Career longevity validated: mean retirement age 33–38, mean seasons 12–20
- [x] Transfer frequency validated: mean 2.0–6.0 transfers across full career
- [x] Financial sustainability: bankruptcy rate < 10% (observed 0.0%)
- [x] Strict bit-exact determinism assertion test

---

## Phase 1 Issue Creation Priority

```
COMPLETED & MERGED TO MAIN (PHASE 1 - 100% COMPLETE):
  Milestone 1.1:    P1-001 → P1-009   (Player Domain Model)       ✅ Merged (PRs #11–#16, Issues #2–#10)
  Milestone 1.2:    P1-010 → P1-017   (World Domain Model)        ✅ Merged (PRs #25–#32, Issues #17–#24)
  Milestone 1.2.5:  P1-GAP-1 → P1-GAP-4 (Simulation Foundations)    ✅ Merged (PR #58, Issues #33–#36)
  Milestone 1.3:    P1-018 → P1-021   (Training System)           ✅ Merged (PR #59, Issues #37–#40)
  Milestone 1.4:    P1-024 → P1-026   (Fatigue System)            ✅ Merged (PR #60, Issues #41–#43)
  Milestone 1.5:    P1-028 → P1-033   (Match Simulation v1)       ✅ Merged (PR #61, Issues #44–#49)
  Milestone 1.6:    P1-038 → P1-041   (Progression + Career)      ✅ Merged (PR #62, Issues #50–#53)
  Milestone 1.6.5:  P1-GATE-1 → P1-GATE-4 (Validation Gate: GO)   ✅ Merged (PR #63, Issues #54–#57)
  Milestone 1.7:    P1-044 → P1-048   (Economy System)            ✅ Merged (PR #69, Issues #64–#68)
  Milestone 1.8:    P1-049 → P1-054   (Life Events System v1)     ✅ Merged (PR #76, Issues #70–#75)
  Milestone 1.9:    P1-055 → P1-059   (Relationships)             ✅ Merged (PR #82, Issues #77–#81)
  Milestone 1.10:   P1-060 → P1-065   (Transfer & Contract System) ✅ Merged (PR #89, Issues #83–#88)
  Milestone 1.11:   P1-066 → P1-069   (Career Simulator & 10k Balance) ✅ Merged (PR #94, Issues #90–#93)

---

## PHASE 2 — UNITY PROTOTYPE

## MILESTONE 2.1 — Unity Project Bootstrap ✅ Complete
*Branch: `feature/milestone-2.1-unity-bootstrap` | Issues #95, #100, #97–#99*

### Story P2-001: Initialize Unity 6 Project & Assembly Definition Setup ✅
**Branch:** `feature/milestone-2.1-unity-bootstrap`
**Status:** Completed (Issue #95)

> As a gameplay programmer, I want a clean Unity 6 URP project with modular assembly definitions (`FootballLife.Unity.Core`, `FootballLife.Unity.UI`, `FootballLife.Unity.Editor`) linked to `FootballLife.Domain` and `FootballLife.Simulation` so that Unity presentation code compiles cleanly with strict separation from domain logic.

**Acceptance Criteria:**
- [x] Assembly definitions created: `FootballLife.Unity.Core.asmdef`, `FootballLife.Unity.UI.asmdef`, `FootballLife.Unity.Editor.asmdef`
- [x] `FootballLife.Domain.dll` and `FootballLife.Simulation.dll` compiled and linked cleanly in `Assets/Plugins/FootballLife/`
- [x] Unity 6 URP and New Input System active and configured
- [x] Clean compilation with zero errors

---

### Story P2-002: Integration Bridge: SimulationRuntime <-> Unity Session ✅
**Branch:** `feature/milestone-2.1-unity-bootstrap`
**Status:** Completed (Issue #100)

> As a gameplay programmer, I want a `SimulationBridge` MonoBehaviour that hosts the pure C# `CareerSimulationEngine` / `SimulationRuntime` and translates Unity time / UI actions into deterministic simulation ticks and dispatches C# events for presentation updates.

**Acceptance Criteria:**
- [x] `SimulationBridge` component created in `FootballLife.Unity.Core`
- [x] Clean event dispatching for day, week, season, match, and life events (`OnDayAdvanced`, `OnWeekAdvanced`, `OnSeasonAdvanced`, `OnMatchOpportunity`, `OnLifeEventOccurred`)
- [x] Deterministic tick execution powered by pure C# `CareerSimulationEngine`
- [x] Zero GC allocations in frame update loops (`Update()`, `FixedUpdate()`)

---

### Story P2-003: Save/Load System: Persist Career State to Disk ✅
**Branch:** `feature/milestone-2.1-unity-bootstrap`
**Status:** Completed (Issue #97)

> As a player, I want my career state to be persisted to disk automatically and safely so that I can resume my career anytime across game sessions without data loss or corruption.

**Acceptance Criteria:**
- [x] `SaveLoadManager` service implemented with atomic write guarantees (temp file + replace)
- [x] Complete serialization/deserialization of career state (`CareerSaveData`)
- [x] Multi-slot management (slots 1–3 + auto-save slot)
- [x] Tests verifying save, load, corruption resistance, and schema versioning

---

### Story P2-004: Scene Architecture: Bootstrap, MainMenu, CareerHub, Match, Home ✅
**Branch:** `feature/milestone-2.1-unity-bootstrap`
**Status:** Completed (Issue #98)

> As a player and developer, I want a structured scene architecture (`Bootstrap`, `MainMenu`, `CareerHub`, `Match`) with additive loading and a central `SceneFlowManager` so that transitions between screens are smooth, fast, and maintain persistent session state.

**Acceptance Criteria:**
- [x] Scenes created/configured: `Bootstrap`, `MainMenu`, `CareerHub`, `Match`
- [x] Standard root hierarchy applied to each scene (`[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, `[UI]`)
- [x] `SceneFlowManager` implemented with asynchronous transition methods and persistent lifecycle
- [x] Build Settings updated with scene list

---

### Story P2-005: App UI Design System: Dark Theme, Typography, Component Library ✅
**Branch:** `feature/milestone-2.1-unity-bootstrap`
**Status:** Completed (Issue #99)

> As a mobile player, I want a premium sports-lifestyle dark UI theme with consistent typography, custom USS design tokens, and reusable component styles (stat meters, cards, badges, buttons) so that the game looks and feels like a modern mobile football career app.

**Acceptance Criteria:**
- [x] USS token stylesheets (`theme-dark.uss`, `tokens.uss`, `components.uss`) created in `Assets/UI/Styles/`
- [x] Reusable styles for buttons, cards, stat meters, and badges implemented
- [x] Mobile-first responsive layout rules with proper safe-area padding
- [x] `DesignSystemPreview.uxml` demonstrating components

---

## MILESTONE 2.2 — Player Creation Flow ✅ Complete
*Branch: `feature/milestone-2.2-player-creation` | Issues #102–#104*

### Story P2-006: Player Creation Screen: Name, Nationality, Position, Foot, Appearance ✅
**Branch:** `feature/milestone-2.2-player-creation`
**Status:** Complete (Issue #102)

> As a player, I want an intuitive and stylish player creation screen where I can configure my footballer's identity (name, nationality, preferred position, preferred foot, and appearance preset) and preview starting baseline attributes so that I can establish my footballer persona.

**Acceptance Criteria:**
- [x] `PlayerCreationView.uxml` created using dark sports theme tokens
- [x] `PlayerCreationController.cs` managing interactive UI events and input validation
- [x] Live attribute preview updates dynamically when position changes
- [x] Random name generator utility supporting multiple nationalities (`RandomNameGenerator.cs`)
- [x] Clean navigation flow to club selection screen

---

### Story P2-007: Starting Club Selection Screen ✅
**Branch:** `feature/milestone-2.2-player-creation`
**Status:** Complete (Issue #103)

> As a player, I want to choose my starting rookie club from multiple realistic starter offers so that I can select my entry point into the professional football world based on club status, wage offer, and competition tier.

**Acceptance Criteria:**
- [x] `ClubSelectionView.uxml` created with interactive offer cards
- [x] `ClubSelectionController.cs` populating dynamic club offer metadata
- [x] Card selection states and visual highlighting with border colors and badges
- [x] Confirmed club choice proceeds to career initialization

---

### Story P2-008: Career Initialization: Wire Player Creation to Simulation ✅
**Branch:** `feature/milestone-2.2-player-creation`
**Status:** Complete (Issue #104)

> As a player, I want my created player and chosen starting club to initialize a new simulation session via `SimulationBridge` and save automatically so that I smoothly transition into my first week at the club in the CareerHub.

**Acceptance Criteria:**
- [x] End-to-end wiring from creation UI -> `SimulationBridge.StartNewCareer` -> `CareerHub`
- [x] Immediate auto-save generated upon career start
- [x] Starting condition initialized (Energy 100, Form 70, Morale 75, Manager Trust 50)
- [x] Scene transition to `CareerHub` completes smoothly

---

## MILESTONE 2.3 — Home Screen (Daily Hub) ✅ Complete
*Branch: `feature/milestone-2.3-daily-hub` | Issues #106–#109*

### Story P2-009: Home Screen (Daily Hub): Date, Form, Energy, Next Match, Primary Actions ✅
**Branch:** `feature/milestone-2.3-daily-hub`
**Status:** Complete (Issue #106)

> As a player, after signing my contract I arrive at the Daily Hub — the main screen I see every day of my career. I want to see today's key info at a glance and decide my next action.

**Acceptance Criteria:**
- [x] Date header: Season N · Week N · Day (Mon/Tue/Wed/Thu/Fri/Sat/Sun)
- [x] Player identity bar: name, position, club, overall rating badge
- [x] Three stat meters: Energy, Form, Morale (with colour-coded fills)
- [x] Manager Trust bar
- [x] Finance row: weekly wage · bank balance
- [x] Next Match card: opponent, competition, home/away, countdown days (hidden if no match this week)
- [x] Status / news ticker: shows latest `OnStatusLog` or life event notification
- [x] Four primary action buttons: Train · Rest · Match (disabled when no match) · View Career
- [x] UI binds to `SimulationBridge.OnDayAdvanced` and refreshes all widgets reactively

---

### Story P2-010: Training Selection UI: Categories, Fatigue Cost, XP Preview ✅
**Branch:** `feature/milestone-2.3-daily-hub`
**Status:** Complete (Issue #107)

> As a player, I want to open a Training panel showing available training categories with their energy cost and expected attribute gain. I pick one and confirm to apply it.

**Acceptance Criteria:**
- [x] Modal/overlay panel `TrainingView.uxml` with title "Training Session"
- [x] 4 training category cards: Physical, Technical, Tactical, Goalkeeping
- [x] Each card shows: category name, icon emoji, energy cost (intensity slider: Low 12 / Med 24 / High 36), expected attribute gain preview
- [x] Confirm button calls `SimulationBridge.SelectWeeklyTraining(category, intensity)`
- [x] Cancel/back button returns to Daily Hub without change
- [x] If energy < 12, all options show "Too Fatigued" warning and confirm is disabled

---

### Story P2-011: Rest & Recovery Action Panel ✅
**Branch:** `feature/milestone-2.3-daily-hub`
**Status:** Complete (Issue #108)

> As a player, I want to spend a day resting so that my energy recovers, especially before a match.

**Acceptance Criteria:**
- [x] Rest modal/overlay `RestView.uxml` with 2 options:
  - **Light Rest** — free, +20 Energy, no morale change
  - **Physio Session** — costs £150, +35 Energy, +5 Morale
- [x] Calls `SimulationBridge.PerformRest("Light")` or `SimulationBridge.PerformRest("Physio")`
- [x] Physio disabled if `BankBalance < 150`
- [x] After confirming, view returns to Daily Hub with updated stats

---

### Story P2-012: Advance Day Button: Trigger Simulation Tick, Refresh Daily Hub ✅
**Branch:** `feature/milestone-2.3-daily-hub`
**Status:** Complete (Issue #109)

> As a player, I want to click "Advance Day" to move time forward. The UI should update to show the new date, changed stats, and any triggered events (life event, match opportunity).

**Acceptance Criteria:**
- [x] "Advance Day" button on Daily Hub calls `SimulationBridge.AdvanceDay()`
- [x] All stat widgets refresh via `OnDayAdvanced` event binding
- [x] If `OnMatchOpportunity` fires → show "Match Day!" banner and enable Match action button
- [x] If `OnLifeEventOccurred` fires → show life event modal with choices (`LifeEventView.uxml` + `LifeEventController.cs`)
- [x] Day label cycles Mon → Tue → ... → Sun → Mon (with week counter incrementing)
- [x] Status ticker updates to newest entry

---

### Milestone 2.4 — Career Screen & Player Profile

#### #P2-013 — Career Screen: Club, Contract & Reputation Overview
**Issue:** #115 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player, I want to open my Career Overview to inspect my current club, squad role, manager trust, contract terms, weekly wage, market value, and career statistics.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`CareerView.uxml`) using App UI dark sports design tokens
- [x] Club & Squad Role Card: Club name, position, preferred foot, squad role badge, division, season/week
- [x] Manager Trust Card: Trust score (0-100), trust status label, visual progress fill
- [x] Contract & Finances Card: Weekly wage, contract expiry year, estimated market value, lifestyle tier
- [x] Career Statistics Card: Total appearances, goals, assists, average rating
- [x] Pure presentation bound to `CareerSaveData` via `CareerController.cs`
- [x] Seamless navigation between Daily Hub, Career Overview, and Player Profile

#### #P2-014 — Player Profile Screen: Attributes & Condition
**Issue:** #116 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player, I want to view my Player Profile to inspect my core abilities separated strictly from temporary condition vitals.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`ProfileView.uxml`) using App UI dark sports design tokens
- [x] Strict architectural separation between long-term permanent abilities and temporary dynamic condition
- [x] Condition Card: Energy, Form, Morale, Manager Trust meters (0-100%)
- [x] 15 core attributes organized into 3 category cards:
  - Physical: Pace, Acceleration, Stamina, Strength, Agility
  - Technical: Shooting/Finishing, Passing, Dribbling, First Touch, Crossing, Tackling
  - Mental: Vision, Composure, Positioning, Decision Making
- [x] Pure presentation bound to `CareerSaveData` via `ProfileController.cs`
- [x] Interactive tab switching between Career Overview and Player Profile views

### Milestone 2.5: Match Preview & Basic Match

#### #P2-015 — Match Preview Screen
**Issue:** #118 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player on match day, I want to see a tactical match preview showing opponent details, competition context, my squad role, and manager expectations before kicking off.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`MatchPreviewView.uxml`) using App UI dark sports design tokens
- [x] Opponent Card: Home vs Away club names, badge initials, competition fixture label, venue badge
- [x] Player Context Card: Starting role badge, primary position, preferred foot, energy %, form %
- [x] Manager Tactical Briefing Card: Concrete match objective (e.g. win by +1 goal) and tactical instruction
- [x] Action Buttons: "Kick Off Match" (`btn-kickoff`) and "Back / Cancel" (`btn-preview-back`)
- [x] Pure presentation bound to `MatchOpportunitySnapshot` and `CareerSaveData` via `MatchPreviewController.cs`

#### #P2-016 — Abstracted Match Screen
**Issue:** #119 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player during a match, I want an interactive match situation interface with a live scoreboard, match clock, situation cards, tactical choices, and simulation-resolved outcomes.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`MatchGameView.uxml`) using App UI dark sports design tokens
- [x] Live Scoreboard: Home/away club names, live scores, match clock (e.g. 18'), commentary ticker
- [x] Situation Card: Dynamic situation title, narrative text, pressure badge, advantage badge
- [x] 3 Player Action Choices: Contextual action buttons with risk indicators
- [x] Simulation Resolution: Pure simulation outcome computation via `ActionResolver` and `SimulationRandom`
- [x] Resolution Card: Outcome badge, narrative breakdown, match rating delta, confidence delta, "Next Moment" button
- [x] Live Player Stats: Real-time match rating, goals, and assists updated dynamically

#### #P2-017 — Post-Match Summary Screen
**Issue:** #120 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player after the final whistle, I want a post-match breakdown displaying the full-time result, my match rating, goals and assists, manager reaction, attribute/vital deltas, and automated progress saving.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`MatchPostView.uxml`) using App UI dark sports design tokens
- [x] Final Result Card: Match outcome tag (VICTORY 🏆, DRAW ⚖️, DEFEAT), final scoreline, competition label
- [x] Individual Performance Card: Large match rating (e.g. 8.10 ★), goals, assists, key actions, errors
- [x] Manager Dressing Room Reaction: Context-sensitive manager quote based on rating and scoreline
- [x] Attribute & Vital Deltas: Manager trust delta (+/-), player form delta (+/-), match energy expenditure (-25)
- [x] Simulation & Persistence: Invokes `SimulationBridge.RecordMatchResult`, updates career stats (`TotalAppearances`, `TotalGoals`), auto-saves career state, returns cleanly to `CareerHub`

#### #P2-018 — Season Summary Screen
**Issue:** #122 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a footballer at the conclusion of a 38-week season, I want an end-of-season summary screen displaying final league standings, individual accomplishments, trophies/honors, and financial year-in-review so that I feel the culmination and achievements of my season's effort.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`SeasonSummaryView.uxml`) using App UI dark theme design tokens
- [x] Campaign Achievements Card: Final league position, club & division badge, trophies won (Championship trophy, promotion medals, top scorer award)
- [x] Player Performance Recap: Season appearances, total goals, total assists, average match rating, season honors & awards
- [x] Financial Year-in-Review: 38-week salary credited, total lifestyle expenses deducted, net annual savings calculated
- [x] Navigation: "View Attribute Growth →" button and "Return to Hub" fallback button

#### #P2-019 — Attribute Growth Visualization
**Issue:** #123 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a developing footballer entering the off-season, I want to review my annual attribute progress, OVR delta, developmental phase, and potential ceiling so that I understand how training and match performance have developed my player.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`AttributeGrowthView.uxml`) with dark sports card styling
- [x] OVR Progression Header: Starting OVR, ending OVR, large green +delta badge (e.g. +3 OVR)
- [x] Development Curve Feedback: Age display, developmental phase badge (e.g. Rapid Youth Development 2.0x vs Peak Plateau), potential ceiling rating
- [x] Attribute Breakdown Categories: Physical attributes (Pace, Stamina, Strength), Technical attributes (Finishing, Passing, Dribbling), Mental attributes (Vision, Positioning, Composure)
- [x] Navigation: "Proceed to Transfers →" button and "← Back to Summary" button

#### #P2-020 — Transfer Window & Contract Renewal Screen
**Issue:** #124 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a footballer during the summer transfer window, I want to review my current contract status, consider contract extension offers from my current club, and review formal bids from interested suitors with wage/bonus incentives so that I can decide where to play next season.

**Acceptance Criteria:**
- [x] UI Toolkit layout (`TransferWindowView.uxml`) with mobile-first card list
- [x] Current Contract Card: Current club, weekly wage, expiry year, estimated market value, squad role
- [x] Contract Extension Offer: Current club renewal proposal with wage increase (+50%) and loyalty signing bonus
- [x] Suitor Transfer Bids: External club offers (e.g. Southport Athletic, Bristol Rovers) with higher division, higher wages, signing bonuses, and squad role expectations
- [x] Contract Signing & Rollover: "Accept & Sign Contract" credits signing bonus to bank balance, updates wage and club in `SimulationBridge`, and "Start Next Season" advances season counter to `Season++`, resets week to `Week 1`, and restores player energy to 100

#### #P2-021 — End-to-End Unity Playable Loop Harness
**Issue:** #127 | **Layer:** Unity/Editor | **Status:** ✅ Complete

> As a developer and player, I want an automated end-to-end integration playtest harness that steps through the complete Phase 2 Unity player journey (Bootstrap -> Player Creation -> Daily Hub -> Matchday -> Season Finale & Transfers) and validates state preservation, UI transitions, and deterministic simulation synchronization.

**Acceptance Criteria:**
- [x] Automated end-to-end playtest runner class in `FootballLife.Unity.Editor` (`PrototypePlaytestRunner.cs`)
- [x] Stage 1 (Assets): Validates Build Settings (4 registered scenes) and presence of all 14 UXML views
- [x] Stage 2 (Creation): Validates Player Creation & Club Selection data flow into `SimulationBridge` (Marcus Vance, ST, Northfield Town, OVR 60, £500/wk)
- [x] Stage 3 (Hub): Validates Career Hub daily actions (Training consumes energy/boosts form, Physio rest recovers energy, Save Slot 0 roundtrip)
- [x] Stage 4 (Matchday): Validates Match flow (`MatchOpportunitySnapshot`, deterministic resolution, 8.4 rating, 2 goals, manager trust delta +6, match fatigue -25 energy)
- [x] Stage 5 (Off-Season): Validates Off-Season flow (Season summary stats, attribute growth OVR +3/youth curve, transfer acceptance to Southport Athletic with £1,500 signing bonus, season rollover to Season 2, Week 1)
- [x] Batch execution support via Unity MCP / command line with zero errors (5/5 stages pass)

#### #P2-022 — Full Prototype UX, Performance & Zero-GC Memory Audit
**Issue:** #128 | **Layer:** Quality/Performance | **Status:** ✅ Complete

> As a player on mobile devices, I want the entire prototype UI to adhere strictly to mobile-first touch ergonomics, App UI dark theme design tokens, 60 FPS performance, and zero GC allocations in hot paths.

**Acceptance Criteria:**
- [x] All 4 scenes (`Bootstrap.unity`, `MainMenu.unity`, `CareerHub.unity`, `Match.unity`) verified for standard hierarchy `[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, `[UI]`
- [x] All UI Toolkit layouts verified for dark theme sports styling (`tokens.uss`, `theme-dark.uss`, `components.uss`) and mobile responsiveness (1080x1920 reference)
- [x] Hot paths (`Update`, `FixedUpdate`) verified for zero GC allocations (event-driven subscriptions used across all controllers)
- [x] Clean editor preview fallback verified for every scene when launched directly in Unity Editor

#### #P2-023 — Gate 2.1 Formal Review Documentation & Go/No-Go Checkpoint
**Issue:** #129 | **Layer:** Docs/Quality | **Status:** ✅ Complete

> As project stakeholders, we want a comprehensive Gate 2.1 review document (`docs/gate-2.1-review.md`) formally auditing Phase 2 against architectural principles, design questions, deterministic benchmarks, and providing the Go/No-Go decision to proceed to Phase 3 (Football Vertical Slice).

**Acceptance Criteria:**
- [x] Formal review document created at `docs/gate-2.1-review.md`
- [x] Executive summary covering Milestones 2.1–2.6
- [x] Design evaluation answering key questions (core fantasy, touch ergonomics, loop engagement, season progression)
- [x] Automated verification & test benchmarks (557 pure C# tests + Unity playmode validation 5/5 stages pass)
- [x] Architecture compliance audit (Separation of layers, determinism, data-driven content, zero GC in hot paths)
- [x] Formal Go/No-Go approval recommendation for Phase 3: APPROVED (GO)

---

---

## Phase 3 — Football Vertical Slice

### Milestone 3.1 — 3D Pitch, Stadium & Ball Physics

#### #P3-004 — Stadium Environment: Regulation Pitch, Line Markings, 3D Goalposts & Lighting
**Issue:** #131 | **Layer:** Unity/Art | **Status:** ✅ Complete

> As a player in a 3D match, I want to see a regulation football pitch with grass turf, official FIFA pitch markings, 3D goalposts with net enclosure, perimeter advertising boards, and realistic stadium floodlighting.

**Acceptance Criteria:**
- [x] Regulation attacking half pitch (68m width x 55m length) with rich green grass turf shader and realistic friction
- [x] Regulation pitch markings: touchlines, goal line, penalty area (40.32m x 16.5m), 6-yard box (18.32m x 5.5m), and penalty spot (11m)
- [x] Official regulation 3D goalposts: 7.32m wide x 2.44m high cylindrical posts, crossbar, and 2.0m depth net enclosure
- [x] Realistic metal post PhysicMaterial (bounciness 0.85, high rebound combine)
- [x] Stadium perimeter advertising boards along touchlines and behind goal
- [x] Dual stadium floodlight setup (key directional sun + soft fill light)
- [x] Procedural automated generation via `PitchBuilder.cs` inside standard hierarchy `[ENVIRONMENT]`

#### #P3-002 — Realistic Ball Physics: Trajectory, Magnus Effect Spin & Goal Detection
**Issue:** #132 | **Layer:** Unity/Physics | **Status:** ✅ Complete

> As a player taking shots or passes, I want the ball to respond with authentic football physics including regulation mass, bounciness, aerodynamic Magnus effect curve, dynamic trail renderer, and automated goal detection inside the net.

**Acceptance Criteria:**
- [x] Ball physics model in `BallController.cs` with regulation FIFA size 5 mass (0.43kg), radius (0.11m), and PhysicMaterial (0.68 bounciness)
- [x] Continuous dynamic collision detection preventing high-velocity tunneling through posts or net
- [x] Aerodynamic Magnus effect calculation in `FixedUpdate` applying cross-product curve force from ball spin
- [x] Visual dynamic `TrailRenderer` indicating shot speed and curve during flight
- [x] 3D trigger volume (`GoalTrigger.cs`) inside the net dispatching `GoalScoredEvent` with entry position, speed (km/h), and timestamp
- [x] Debounce cooldown preventing duplicate goal trigger events

#### #P3-005 — Dynamic Match Camera Rig: Broadcast, ActionAim & ShotTrack Modes
**Issue:** #133 | **Layer:** Unity/Camera | **Status:** ✅ Complete

> As a player experiencing different match phases, I want the camera to dynamically transition between broadcast overview, behind-the-ball aiming, and dramatic ball-tracking follow views with smooth damping.

**Acceptance Criteria:**
- [x] Multi-mode camera rig in `MatchCameraRig.cs` supporting 4 distinct modes:
  - `Broadcast`: Elevated tactical view showing attacking pitch, player, and open space
  - `ActionAim`: Over-the-shoulder behind-the-ball view facing the opponent goal for shot aiming
  - `ShotTrack`: Dynamic zoomed follow cam tracking behind the ball flight towards the net
  - `Celebration`: Low-angle dramatic framing for goals and celebrations
- [x] Smooth position tracking via `Vector3.SmoothDamp` and rotational damping with zero GC allocations in `LateUpdate`
- [x] Dynamic field-of-view tightening during shots for cinematic drama (55° default to 45° shot track)
- [x] Integrated into standard `[CAMERAS]` root in `Match.unity` scene

### Milestone 3.2 — Player Pawn, Animations & Teammates

#### #P3-001 — 3D Player Character: Humanoid Pawn, Rig & Locomotion/Kicking Presentation
**Issue:** #135 | **Layer:** Unity/Art, Unity/Gameplay | **Status:** ✅ Complete

> As a player in a 3D match situation, I want to see my footballer represented as an articulated 3D humanoid pawn with clean team kit colors, responsive locomotion (idle, run/jog), aiming stance, and dynamic kicking animations.

**Acceptance Criteria:**
- [x] Articulated 3D humanoid character pawn (torso, hips, head/face/hair, shoulders/arms, hips/legs, boots with studs) via `HumanoidPawnBuilder.cs`
- [x] Team kit customization scheme (`PawnKitScheme`): Home Outfield (Navy/Cyan/White), Away Outfield (Crimson/Navy), Goalkeeper (Fluorescent Lime/Black)
- [x] Procedural articulation controller (`PlayerPawnController.cs`) supporting `Idle`, `Jog`, `Run`, `PrepKick`, `Kick`, `Tackle`, `Celebrate`
- [x] Smooth sinusoidal run cycles (alternating leg swing, arm counter-swing, and vertical bounce)
- [x] Kick impact synchronization: forward leg strike through apex dispatches `OnKickImpact` event, launching the ball with `BallController.Kick()`
- [x] Zero GC allocations in `Update()` / `LateUpdate()` loops

#### #P3-003 — Situation-Driven Teammate & Opponent Pawns
**Issue:** #136 | **Layer:** Unity/Gameplay, Unity/Presentation | **Status:** ✅ Complete

> As a player taking part in a match situation, I want to see teammate and opponent pawns arranged dynamically on the pitch matching the attacking/defending tactical context, including an active goalkeeper tracking the goal line.

**Acceptance Criteria:**
- [x] `MatchPawn.cs` component holding identity (PlayerName, KitNumber, Role: UserStriker, Teammate, Defender, Goalkeeper; Team: Home, Away) and visual selection ring
- [x] `GoalkeeperController.cs` on goal line ($Z = 34.8\text{ m}$) with crouched ready stance, continuous lateral tracking along goal line following ball $X$ coordinate (clamped to $[-3.2\text{ m}, +3.2\text{ m}]$)
- [x] Goalkeeper dynamic reaction: detects incoming high-speed shots and initiates horizontal diving save or jump save postures
- [x] `SituationPawnPresenter.cs`: arranges 5 situation pawns under `[ENTITIES]/Pawns` (User Striker at $Z=13.5\text{ m}$, Supporting Teammate at $Z=18.5\text{ m}$, Opponent CB1 at $Z=22.5\text{ m}$, Opponent CB2 at $Z=23.5\text{ m}$, Opponent GK at $Z=34.8\text{ m}$)
- [x] Target linking to `MatchCameraRig` for smooth multi-mode camera tracking

### Milestone 3.3 — Touch Controls & Interactive Gameplay Situations

#### #P3-006 — Touch Control System: Tap-to-Target & Swipe-to-Kick
**Issue:** #138 | **Layer:** Unity/Input | **Status:** ✅ Complete

> As a mobile player, I want responsive touch controls (tap on teammate to pass, swipe/flick trajectory toward goal to shoot) with visual gesture feedback.

**Acceptance Criteria:**
- [x] Input system handling touch gestures and mouse fallback via `UnityEngine.InputSystem.Pointer.current` (`TouchGestureController.cs`)
- [x] Tap detection (< 0.25s duration, < 15px drift) for selecting teammates or passing lanes
- [x] Drag/swipe detection measuring direction, swipe displacement distance (power), and curvature (Magnus spin)
- [x] Visual gesture event dispatching (`OnAimStarted`, `OnAimUpdated`, `OnAimReleased`, `OnTapDetected`)
- [x] Zero GC allocations in input processing loop

#### #P3-007 — Match Situation Presenter: 3D Situation Setup from Simulation Events
**Issue:** #139 | **Layer:** Unity/Gameplay | **Status:** ✅ Complete

> As a player during match play, I want 3D situations to be dynamically constructed from simulation opportunities (ReceivingInBox, OneOnOne, Cross, ThroughBall).

**Acceptance Criteria:**
- [x] `SituationPawnPresenter.ApplySituationPreset` translating domain `SituationType` into spatial pitch coordinates
- [x] Distinct situation setups:
  - `ReceivingInBox`: Central box finish at $Z=13.5\text{ m}$ with defenders jockeying
  - `OneOnOne`: Striker breakaway at $Z=18.0\text{ m}$ with goalkeeper rushing out
  - `Cross`: Teammate on right wing at $Z=24\text{ m}, X=16\text{ m}$ crossing to striker at $Z=25\text{ m}$
  - `ThroughBall`: Ball rolling into space at $Z=17.5\text{ m}$ with striker sprinting from deep
- [x] Automatic camera alignment and target binding via `MatchCameraRig`

#### #P3-008 — Shooting Mini-Interaction: Aim Trajectory & Power System
**Issue:** #140 | **Layer:** Unity/Gameplay | **Status:** ✅ Complete

> As a striker shooting at goal, I want an aim and power mechanic where swipe trajectory, length, and curve directly determine the ball launch velocity and Magnus spin.

**Acceptance Criteria:**
- [x] Real-time 3D ballistic trajectory preview arc (`AimTrajectoryRenderer.cs`) with gravity, drag, and Magnus curve prediction
- [x] Target ground/net reticle updating dynamically during swipe drag
- [x] Swipe power scaling launch velocity ($18\text{ m/s}$ to $30\text{ m/s}$) with vertical lift angle
- [x] Gesture curvature deriving Magnus spin rate ($-10\text{ rad/s}$ to $+10\text{ rad/s}$) for dipping and curling shots
- [x] Striker windup and kick synchronization with camera tracking switch to `ShotTrack` and `Celebration`

#### #P3-009 — Passing Mini-Interaction: Teammate Target & Delivery Timing
**Issue:** #141 | **Layer:** Unity/Gameplay | **Status:** ✅ Complete

> As a player with passing options, I want to tap a supporting teammate to trigger a grounded or lobbed pass, with intercept checks from marking defenders.

**Acceptance Criteria:**
- [x] Teammate tap detection via screen raycast and screen-space proximity touch targeting (`PassingInteraction.cs`)
- [x] Calibrated ground pass velocity calculation leading teammate's stride
- [x] Defender interception evaluation calculating proximity of opponent center-backs to the passing ray
- [x] Successful pass receipt: teammate traps ball and turns to face the opponent goal

---

#### #P3-010 — Goal Celebration: Camera Celebration Orbit & Dynamic Banner VFX
**Issue:** #143 | **Layer:** Unity/Presentation | **Status:** ✅ Complete

> As a player scoring a goal, I want an exciting celebration presentation featuring dynamic camera orbit tracking, goal audio/particle feedback, and a celebratory overlay banner displaying scorer name and shot speed in km/h.

**Acceptance Criteria:**
- [x] Detection of goal entry event via `GoalTrigger.OnGoalScored` with precise launch velocity derivation
- [x] Smooth camera transition to `MatchCameraRig.CameraMode.Celebration` orbiting the scoring player
- [x] Dynamic celebration card (`card-goal-celebration` in `MatchHUDView.uxml`) displaying:
  - ⚽ GOAL! header
  - Scorer name and minute (`Marcus Vance 68'`)
  - Shot speed metric (`⚡ Shot Speed: xx.x km/h`)
  - Live updated scoreline badge (`Home X - Y Away`)
- [x] Interactive action buttons to continue/reset play or proceed to full-time match post summary

#### #P3-011 — Match In-Game HUD: Scoreboard, Clock, Stamina Bar & Contextual Buttons
**Issue:** #144 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a player during interactive 3D gameplay, I want a non-intrusive broadcast-style HUD showing the live scoreline, match clock, stamina bar, and quick-action touch controls.

**Acceptance Criteria:**
- [x] UI Toolkit transparent HUD overlay (`MatchHUDView.uxml`) using theme tokens and `picking-mode="Ignore"` for non-blocking pitch interaction
- [x] Live scoreboard displaying Home & Away club names, current score, and match minute
- [x] Dynamic stamina bar visualizer with percentage text label reflecting player fatigue
- [x] Action hint label and touch-friendly quick action buttons (`btn-action-shoot`, `btn-action-pass`, `btn-action-reset`)
- [x] Clean integration with `MatchCoordinator` to seamlessly transition between Pre-Match Preview, 3D Gameplay HUD, and Post-Match Summary
- [x] Zero GC allocations during update cycles

---

## Phase 3 Issue Status Overview

```
COMPLETED IN PHASE 3:
  Milestone 3.1:    P3-004, P3-002, P3-005 (3D Pitch, Stadium & Ball Physics)  ✅ Complete (Issues #131–#133)
  Milestone 3.2:    P3-001, P3-003         (3D Player Character & Pawns)       ✅ Complete (Issues #135–#136)
  Milestone 3.3:    P3-006 → P3-009        (Touch Controls & Situations)       ✅ Complete (Issues #138–#141)
  Milestone 3.4:    P3-010 → P3-011        (Goal Celebrations & Match HUD)     ✅ Complete (Issues #143–#144)

PHASE 3 COMPLETE! Vertical slice football match gameplay, pawns, physics, touch controls, and match presentation operational.
```

---

# Phase 4: Life Vertical Slice

### Milestone 4.1: 3D Home Apartment Environment & Lifestyle Progression

#### #P4-001 — Interactive 3D Home Apartment Environment
**Issue:** #146 | **Layer:** Unity/Art | **Status:** ✅ Complete

> As a footballer, I want an interactive 3D home apartment environment with dedicated hotspots for Bed (sleep), Gym (workout), Phone (smartphone OS), and Door (exit to club) so that my off-pitch life feels tangible and immersive.

**Acceptance Criteria:**
- [x] Dedicated Unity scene `Home.unity` configured with standard 5-root hierarchy (`[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, `[UI]`) and registered in Editor Build Settings.
- [x] Procedural 3D apartment environment generator (`ApartmentBuilder.cs`) constructing modern architectural interior (flooring, perimeter walls, ceiling, panoramic window overlooking city skyline with night lights, warm interior illumination).
- [x] 3D interactive zones (`HomeInteractionZone.cs`) with colliders and camera focus anchors:
  - 🛏️ **Bed Zone**: Rest and fatigue recovery hotspot
  - 🏋️ **Gym Zone**: Dumbbells and workout mat for conditioning
  - 📱 **Lounge/Phone Zone**: Modern sofa and smartphone coffee table
  - 🚪 **Door Zone**: Apartment entrance returning to Career Hub
- [x] Smooth camera rig (`HomeCameraRig.cs`) supporting ambient overview drift and smooth framed transitions to individual zones upon tap or button select.
- [x] Touch and raycast interaction controller (`HomeInteractionController.cs`) translating taps to camera transitions and interaction event dispatch.
- [x] UI Toolkit lifestyle HUD (`HomeHUDView.uxml` and `HomeController.cs`) providing vitals indicators (energy bar, bank balance, property name, tier badge), quick-dock shortcuts, and feedback toasts.

#### #P4-002 — Home Progression & Lifestyle Apartment Tiers
**Issue:** #147 | **Layer:** Unity/Simulation | **Status:** ✅ Complete

> As a footballer rising through divisions and earning higher wages, I want to upgrade my home across 5 distinct lifestyle tiers (Modest to Superstar) so that my living quarters reflect my career success and boost physical recovery and workout gains.

**Acceptance Criteria:**
- [x] Pure C# domain model (`HomeProperty.cs`) defining 5 distinct property tiers:
  - Tier 0: `Modest Studio` (£0 purchase, £80/wk upkeep, 1.00x rest, 1.00x gym)
  - Tier 1: `Comfortable Townhome` (£35,000 purchase, £250/wk upkeep, 1.15x rest, 1.10x gym)
  - Tier 2: `Luxurious Penthouse` (£180,000 purchase, £750/wk upkeep, 1.35x rest, 1.25x gym)
  - Tier 3: `Extravagant Villa` (£850,000 purchase, £2,400/wk upkeep, 1.60x rest, 1.45x gym)
  - Tier 4: `Superstar Estate` (£3,500,000 purchase, £7,500/wk upkeep, 2.00x rest, 1.75x gym)
- [x] Pure C# simulation system (`HomeSystem.cs`) providing deterministic upgrade validation (`CanAffordUpgrade`), transaction execution (`UpgradeHome`), sleep recovery calculations (`CalculateSleepRecovery`), and home workout execution (`ExecuteHomeWorkout`).
- [x] Real-time 3D environment re-theming (`ApartmentBuilder.RebuildForTier`) dynamically altering materials, wood finishes, lighting warmth, and luxury accents upon tier upgrade.
- [x] Full UI modal in `HomeHUDView.uxml` with target property specifications, purchase cost, weekly upkeep requirements, rest bonus, and physical gym multiplier.
- [x] 12 comprehensive unit tests in `HomeSystemTests.cs` validating economy curves, downscaling, sleep mathematics, and workout stamina/strength gains.

---

### Milestone 4.2: Smartphone OS & Social Layer

#### #P4-003 — Smartphone OS UI: Messages, Social Feed & Journalism News
**Issue:** #149 | **Layer:** Unity/UI | **Status:** ✅ Complete

> As a footballer, I want a smartphone OS overlay accessible from my home lounge or career hub featuring messaging (WhatsApp style), social media (FootyGram), and sports journalism (Football Daily) so that I stay connected with teammates, fans, and the football world.

**Acceptance Criteria:**
- [x] Smartphone overlay UI (`PhoneOSView.uxml` and `PhoneOSController.cs`) with realistic chassis, status bar (carrier, 5G, time, battery), dynamic island, app screen viewport, and bottom dock navigation.
- [x] Four primary mobile apps:
  - 💬 **WhatsApp (`Messages`)**: Conversation threads with contacts, incoming message notifications, dialogue choices with branching replies, and immediate stat impacts.
  - 📸 **FootyGram (`Social`)**: Social feed cards with user handle, photo preview, caption, verified badge, comment counts, and interactive like toggle with dynamic counter.
  - 👥 **Relationship Hub (`Contacts`)**: People-centric list displaying key contacts with affinity and trust meters, shared history, and contextual action buttons.
  - 📰 **Football Daily (`News`)**: Journalism cards with breaking transfer rumors, tactical match previews, player spotlights, and category badges.
- [x] Pure C# Domain models (`PhoneMessage.cs`, `SocialPost.cs`) and Simulation system (`PhoneSystem.cs`) generating messages, social posts, news articles, and deterministic dialogue replies.
- [x] Bidirectional interaction with `CareerHub` and `Home` scenes via dedicated quick-access phone buttons and phone table 3D hotspot.

#### #P4-004 — Relationship Hub: People-Centric Bonds, Trust & Social Actions
**Issue:** #150 | **Layer:** Unity/Simulation | **Status:** ✅ Complete

> As a footballer, I want deep individual relationships with my Manager, Teammate, Partner, and Agent, where I can spend energy and money on social actions to increase affinity and trust, influencing my starting lineup status, morale, and commercial opportunities.

**Acceptance Criteria:**
- [x] Pure C# Domain model `SocialActionType` (`CallCatchUp`, `SendGift`, `DinnerHangOut`, `TalkTactics`) and Simulation system extensions in `RelationshipSystem.cs`.
- [x] Deterministic execution logic in `RelationshipSystem.ExecuteSocialAction`:
  - `CallCatchUp`: Low energy cost (-5), moderate affinity boost (+4).
  - `SendGift`: Monetary cost (-£200), high affinity boost (+8.5).
  - `DinnerHangOut`: High energy cost (-15), monetary cost (-£80), large affinity (+12) & morale (+8) boost.
  - `TalkTactics`: Manager-specific action boosting Manager Trust (+6.5) at modest energy cost (-8).
- [x] UI Toolkit integration in `PhoneOSController.cs` rendering interactive buttons ("Call", "Gift", "Dinner", "Tactics") on each contact card with live feedback toasts, energy/balance checks, and stat bar updates.
- [x] 11 comprehensive automated tests in `SocialSystemTests.cs` validating all actions, cost constraints, balance checks, and stat updates (580/580 tests passing).

---

### Milestone 4.3: Life Events, Finances & Lifestyle Shop

#### #P4-005 — Life Event System: Immersive Choice-Cards with Character Context
**Issue:** #152 | **Layer:** Unity/UI, Simulation | **Status:** ✅ Complete

> As a footballer, I want random and contextual life events and moral dilemmas presented as rich choice-cards featuring character portraits, background lore, and transparent trade-offs so that my personal decisions have meaningful consequences on my career, finances, and relationships.

**Acceptance Criteria:**
- [x] Immersive choice-card UI (`LifeEventView.uxml` and `LifeEventController.cs`) featuring event character avatar, author name, category badge, atmospheric description block, and dynamic action cards.
- [x] Choice cards displaying visual stat impact chips (+/- Money, Energy, Morale, Trust) before confirmation.
- [x] Integration with `SimulationBridge.Instance.ResolveLifeEventChoice(choice)` to deterministically apply attribute, financial, and relational consequences.
- [x] Default curated dilemmas (`Night Out Before Matchday`, `Sponsorship Controversy`, `Family Request`) providing high-stakes narrative choices.
- [x] Seamless invocation from both `CareerHubController` and `HomeController`.

#### #P4-006 — Finances Screen: Balance, Cash Flow, Weekly Income/Expenses & Lifestyle Tier
**Issue:** #153 | **Layer:** Unity/UI, Simulation | **Status:** ✅ Complete

> As a footballer, I want a comprehensive personal finance dashboard showing my bank balance, net weekly cash flow, detailed breakdown of income and expenses (wages, taxes, agent fees, housing upkeep), and lifestyle spending tier so that I can manage my wealth sustainably.

**Acceptance Criteria:**
- [x] Pure C# domain model `FinanceBreakdown` calculating gross weekly wage, match bonuses, sponsorships, tax withholding (35%), agent commission (5%), property upkeep, lifestyle tier expenses, net cash flow, and estimated net worth.
- [x] Interactive Finances UI (`FinancesView.uxml` and `FinancesController.cs`) with 4 top-level metric cards (`Bank Balance`, `Net Weekly Cash Flow`, `Estimated Net Worth`, `Prestige Rating`).
- [x] Collapsible/tabulated Income Breakdown (gross wage, bonuses, sponsors) and Expenses Breakdown (taxes, agent fees, property upkeep, item upkeep, living tier).
- [x] Living Standard Selector allowing toggling between `Modest`, `Comfortable`, `Luxury`, and `Excessive` tiers with real-time recalculation of cash flow.
- [x] Chronological transaction ledger with color-coded credit/debit indicators, dates, and category tags.

#### #P4-007 — Lifestyle Item Shop: Vehicles, Fashion, Tech & Wellness Upgrades
**Issue:** #154 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer, I want a luxury lifestyle boutique where I can purchase high-end items across Vehicles, Fashion, Tech, and Wellness categories that grant persistent morale, prestige, and recovery perks to reflect my rising status.

**Acceptance Criteria:**
- [x] Pure C# Domain models (`LifestyleItem.cs`, `LifestyleCategory`, `LifestyleCatalog`) featuring 16 luxury items across `Vehicles`, `Fashion`, `Tech`, and `Wellness` with unique price, weekly upkeep, morale perk, rest recovery bonus, and prestige score.
- [x] Pure C# Simulation system (`LifestyleShopSystem.cs`) providing `CanAffordItem`, `PurchaseItem` with duplicate prevention and transaction recording, `CalculateTotalItemUpkeep`, and `CalculateTotalPerks`.
- [x] Lifestyle Shop UI (`LifestyleShopView.uxml` and `LifestyleShopController.cs`) with category filter tabs, luxury item cards, owned state badges, perk indicators, live wallet balance, and purchase toast feedback.
- [x] Wellness gear integration with `HomeSystem.CalculateSleepRecovery` providing rest multipliers (e.g. Cryo Chamber, Espresso Machine, Hyperbaric Pod) directly buffing bed recovery.
- [x] 8 comprehensive automated unit tests in `LifestyleShopSystemTests.cs` and `FinancesBreakdownTests.cs` (590/590 passing tests).

---

### Milestone 4.4: Social Activities, Dynamic Media Reports & Press Conferences

#### #P4-008 — Social Activities: Go Out Events with Fatigue/Morale Trade-Offs & Outing Catalog
**Issue:** #156 | **Layer:** Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer, I want to participate in social outings and leisure activities (fine dining, concerts, charity galas, nightlife, coffee strolls) that provide morale, prestige, and team affinity boosts at the cost of energy, money, and manager disapproval risk if too close to matchday.

**Acceptance Criteria:**
- [x] Pure C# Domain models (`SocialActivity.cs`, `SocialActivityCategory`, `SocialActivityCatalog`) with 12 curated outings across `Casual`, `TeamBonding`, `Nightlife`, `Glamour`, and `Philanthropy` categories.
- [x] Pure C# Simulation system (`SocialActivitySystem.cs`) implementing `CanAffordActivity`, `ExecuteActivity` with deterministic energy/balance deduction, morale/teammate affinity boosts, and near-matchday manager disapproval risk roll using `SimulationRandom`.
- [x] Social Activities UI (`SocialActivitiesView.uxml` and `SocialActivitiesController.cs`) with category filter tabs, energy/wallet chips, interactive outing cards, dynamic state badges (`GO OUT`, `EXHAUSTED`, `NO FUNDS`), and feedback toasts.
- [x] Integration with both `CareerHub` (`CareerHubView.uxml`, `CareerHubController.cs`, `CareerHubCoordinator.cs`) and `Home` (`HomeHUDView.uxml`, `HomeController.cs`) scenes with seamless modal presentation and vitals refreshing.
- [x] 6 automated unit tests in `SocialActivitySystemTests.cs` validating catalog, affordability, execution, teammate bonding, pre-matchday risk, and philanthropy.

#### #P4-009 — Dynamic Media & Press Conference System: Match Reports, Rumors & Interviews
**Issue:** #157 | **Layer:** Domain, Simulation, Unity/UI, LLM Hook | **Status:** ✅ Complete

> As a footballer, I want dynamic sports journalism media feeds covering my performances and transfer rumors, and interactive post-match press conferences where my dialogue choices impact manager trust, fan popularity, teammate morale, and media reputation.

**Acceptance Criteria:**
- [x] Pure C# Domain models (`PressConference.cs`, `PressTone`, `Journalist`, `PressResponseChoice`, `PressQuestion`, `MediaArticle.cs`) capturing tone variations (`Humble`, `Confident`, `Defiant`, `Diplomatic`) and journalist temperaments (`Supportive`, `Sensationalist`, `Tactical`).
- [x] Pure C# Simulation systems:
  - `PressConferenceSystem.cs`: Generates context-aware questions based on recent match results (wins, losses, debut) and answers choices modifying `ManagerTrust`, `FanPopularity`, `TeammateMorale`, and `MediaReputation`.
  - `MediaFeedSystem.cs`: Generates dynamic journalism articles from publications (*The Athletic*, *Sky Sports*, *The Daily Mirror*, *GQ Sports Style*, *BBC Football*) with engagement metrics (likes, views) and an LLM prompt builder conforming to `llm-game-integration`.
- [x] Interactive Press Conference UI (`PressConferenceView.uxml` and `PressConferenceController.cs`) with live broadcast badge, journalist spotlight, question block, 4 tone choice cards with consequence previews, and conference debrief summary.
- [x] CareerHub integration with dedicated header button (`btn-quick-press`), action button (`btn-press-briefing`), and modal orchestration via `CareerHubCoordinator.cs`.
- [x] 6 automated unit tests in `PressConferenceAndMediaTests.cs` (602/602 total passing tests across the entire solution).

---

## Phase 4 Issue Status Overview

```
COMPLETED IN PHASE 4:
  Milestone 4.1:    P4-001 (#146), P4-002 (#147)  (3D Home Apartment & Lifestyle Progression)  ✅ Complete
  Milestone 4.2:    P4-003 (#149), P4-004 (#150)  (Smartphone OS UI & Relationship Hub)        ✅ Complete
  Milestone 4.3:    P4-005 (#152), P4-006 (#153), P4-007 (#154) (Life Events, Finances & Shop) ✅ Complete
  Milestone 4.4:    P4-008 (#156), P4-009 (#157)  (Social Activities & Press Conferences)      ✅ Complete
```

---

## MILESTONE 5.1 — World Simulation, Multi-Tier League Hierarchy & Dynamic Transfer Market ✅ Complete

### Story P5-001: World Simulation — NPC Player Development, Aging/Decline, AI Squad Replenishment & League Progression ✅
**Issue:** #159 | **Layer:** Domain, Simulation | **Status:** ✅ Complete

> As a simulation engine, I want living world progression where NPC players develop, age, and decline realistically, clubs replenish aging squads with fresh talent, and leagues simulate seasonal standings so that the football world evolves dynamically around the player.

**Acceptance Criteria:**
- [x] Pure C# Domain models for world player progression and season resolution (`LeagueSeasonResolution.cs`, `ClubSeasonOutcome`, `WorldSeasonResolution`).
- [x] Pure C# Simulation system `WorldSimulationSystem.cs` implementing `AgeAndDevelopNpcPlayers` with age curves: youth growth (<21), prime maturation (21-28), peak plateau (29-30), physical decline (31-34), and retirement at 36+ or severe decline.
- [x] Squad replenishment in `WorldSimulationSystem.ReplenishSquads` generating balanced squads of at least 18 players with position coverage across GK, DEF, MID, FWD.
- [x] League season resolution `ResolveLeagueSeason` ranking clubs deterministically, evaluating champion, continental spots, and promotion/relegation between divisions.
- [x] Automated unit test suite `WorldSimulationSystemTests.cs` (5 tests) passing with 0 failures.

---

### Story P5-002: Transfer Window & Market — Multi-Club Bidding Wars, Player Transfer Requests & Market UI ✅
**Issue:** #160 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer, I want a dynamic transfer market where multiple clubs bid for my signature with competitive wage offers and promised squad roles, and where I can submit formal transfer requests to force a move to a bigger club.

**Acceptance Criteria:**
- [x] Pure C# Domain models for player transfer listing and bidding wars (`TransferListing.cs`, `TransferBiddingWar.cs`, `ClubBid`, `PlayerTransferStatus`).
- [x] Pure C# Simulation system `TransferMarketSystem.cs` providing `GenerateBiddingWar` (2-4 competitive club bids with tier-scaled wages, signing bonuses, and squad roles), `RequestTransferListing` (manager trust and squad status evaluation), and `AcceptBid` (atomic contract, signing bonus, and club transition).
- [x] Interactive UI Toolkit Transfer Market interface (`TransferMarketView.uxml`, `TransferMarketController.cs`) with player market valuation badge, multi-club bid cards with wage comparisons and accept buttons, tab navigation (`Active Bids`, `League Pyramid`), and transfer request button with toast feedback.
- [x] Integrated into `CareerHubView.uxml` with quick header button (`btn-quick-market`), action grid button (`btn-transfer-market`), and modal coordinator handling in `CareerHubCoordinator.cs`.
- [x] Automated unit test suite `TransferMarketSystemTests.cs` (5 tests) passing with 0 failures.

---

### Story P5-003: Multi-Tier League Ecosystem — Division Prestige, Wage Scaling, Promotion & Relegation ✅
**Issue:** #161 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a player climbing the football ladder, I want a multi-tier league ecosystem with distinct divisional prestige, authentic wage bands, and functioning promotion and relegation so that moving up divisions feels rewarding and transformative.

**Acceptance Criteria:**
- [x] Pure C# Domain models (`LeagueTierConfig.cs`, `LeagueTier`, `LeagueTierProfile`) defining a 4-tier English football pyramid (`Tier1_Premier`, `Tier2_Championship`, `Tier3_LeagueOne`, `Tier4_LeagueTwo`) with distinct prestige ratings (55 to 95), weekly wage ranges (£500 up to £250,000), promotion/relegation spots, continental spots, and trophies.
- [x] Pure C# simulation logic in `WorldSimulationSystem.ResolveLeagueSeason` handling inter-division swaps with `clubsAlreadyMoved` guard preventing multi-tier cascades in a single season.
- [x] Contract wage generation dynamically scaled based on target club's league tier and player overall rating.
- [x] Presentation of multi-tier league pyramid cards in `TransferMarketView.uxml` with tier badge, prestige rating, wage band, and promotional spot indicators.
- [x] Fully verified with unit tests and headless career test suites (612/612 total passing tests).

---

---

## MILESTONE 5.2 — International Football, Continental Tournaments & Manager Changes ✅ Complete

### Story P5-004: National Team System — Eligibility, Call-Ups, International Tournaments & Career Caps ✅
**Issue:** #P5-004 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer, I want to earn call-ups for my national team based on my form, rating, and positional need, compete in international fixtures, accumulate caps and goals, and experience the physical fatigue and prestige rewards of representing my country.

**Acceptance Criteria:**
- [x] Pure C# Domain models for international football (`NationalTeam.cs`, `InternationalCallUp.cs`, `InternationalCareer.cs`, `InternationalFixture.cs`, `InternationalTier`).
- [x] Pure C# Simulation system `InternationalSystem.cs` implementing nationality eligibility matching, national squad selection by rating/form with positional quotas (3 GK, 7 DEF, 7 MID, 6 FWD), dynamic call-up invitations, and international match simulation with caps, goals, and assists.
- [x] Physical and psychological feedback: fatigue increase (+18 to +25), confidence boost on victory (+6 to +10), and career reputation gain (+3 to +6).
- [x] Persistence in `CareerSaveData.cs` tracking `InternationalCaps`, `InternationalGoals`, `InternationalAssists`, and `IsRetiredFromInternational`.
- [x] UI Toolkit presentation in `ProfileView.uxml` with an International Football card displaying national team badge, caps, goals, assists, active call-up status, and action button.
- [x] Automated unit test suite `InternationalSystemTests.cs` (7 tests) passing with 0 failures.

---

### Story P5-005: Continental Competitions — Champions Cup, Qualification, Group Stage & Knockouts ✅
**Issue:** #P5-005 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As an elite club footballer, I want my club to qualify for the Champions Cup continental tournament based on league finish, compete in a 32-team group stage and 2-legged knockout bracket, and win prestigious continental glory and massive prize money.

**Acceptance Criteria:**
- [x] Pure C# Domain models for continental tournaments (`ContinentalCompetition.cs`, `ContinentalFixture.cs`, `ContinentalGroupStanding.cs`, `ContinentalStage`).
- [x] Pure C# Simulation system `ContinentalCompetitionSystem.cs` managing 32-club tournament setup with 8 groups of 4 (with same-league avoidance where possible), home/away round-robin group stage resolution (6 matchdays), and 2-legged knockout bracket (RO16, Quarter-Finals, Semi-Finals, and Final).
- [x] Knockout aggregate resolution with away goals / extra time and penalty shootouts if aggregate scores are tied.
- [x] Financial rewards: £50,000,000 champion prize money distributed to the winning club's budget and prize money for runner-up (£30M) and semi-finalists (£15M).
- [x] Full UI Toolkit screen `ContinentalView.uxml` and controller `ContinentalViewController.cs` presenting Groups A-H standings, knockout bracket tree, player's club status, and interactive match simulation button.
- [x] Integrated into `CareerHubView.uxml` with quick header button (`btn-quick-continental`), action grid button (`btn-continental`), and overlay coordinator handling in `CareerHubCoordinator.cs`.
- [x] Automated unit test suite `ContinentalCompetitionSystemTests.cs` (5 tests) passing with 0 failures.

---

### Story P5-006: Manager Change System — Manager Sacking, Hiring & Tactical Trust Reset ✅
**Issue:** #P5-006 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer, I want clubs to hold managers accountable with sackings when performance falls below expectations, appoint new managers with distinct tactical identities, and reset my manager trust so I must prove myself again to the new boss.

**Acceptance Criteria:**
- [x] Pure C# Domain models for managerial appointments and sackings (`ManagerChangeEvent.cs`, `ManagerReason`).
- [x] Pure C# Simulation system `ManagerChangeSystem.cs` evaluating seasonal performance against board expectations and patience, calculating sacking probability, and appointing a new manager from tier-appropriate names, tactical identities (Attacking, Possessional, Counter, Direct, Defensive), and trust tolerances.
- [x] Dynamic player impact: player manager trust resets to neutral (50.0) upon a managerial change, requiring the player to earn starting status through training and performances.
- [x] Real-time event propagation via `SimulationBridge.OnManagerChanged` updating player identity, hub header, and manager trust indicators.
- [x] Manager change history logged in `WorldState.ManagerChangeHistory`.
- [x] Automated unit test suite `ManagerChangeSystemTests.cs` (6 tests) passing with 0 failures.

---

---

## MILESTONE 5.3 — Endorsements, Career Longevity & Legacy ✅ Complete

### Story P5-007: Sponsorship System — Reputation-Gated Endorsement Deals & Commercial Perks ✅
**Issue:** #167 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a footballer gaining fame and reputation, I want to sign commercial endorsement deals and sponsorships (boot deals, brand ambassador roles, luxury endorsements) that provide recurring commercial income and unique perks, balanced by commitments and reputation prerequisites.

**Acceptance Criteria:**
- [x] Pure C# Domain models for endorsements (`SponsorshipDeal.cs`, `SponsorshipTier`, `SponsorshipType`, `ActiveSponsorship`).
- [x] Pure C# Simulation system `SponsorshipSystem.cs` providing:
  - Reputation-gated offer catalog across 4 tiers (Local, Regional, National, Global) and 5 categories (Boots, Apparel, Beverage, Luxury, Tech).
  - Slot limit enforcement (maximum 3 concurrent active sponsorships).
  - Weekly commercial payout calculation and expiration tracking.
  - Perk metrics: energy recovery bonus and weekly fame multipliers.
  - Brand termination triggers when reputation crashes (-15 below requirement) or form collapses (< 15).
- [x] Interactive UI Toolkit presentation (`SponsorshipView.uxml`, `SponsorshipViewController.cs`) with active deals overview, available offers, and contract signing buttons.
- [x] Integrated into weekly advancement in `SimulationBridge.cs` crediting payouts and energy boosts.
- [x] Automated unit test suite `SponsorshipSystemTests.cs` (7 tests) passing with 0 failures.

---

### Story P5-008: Retirement Arc — Late-Career Physical Decline, Contract Wind-Downs & Retirement Choice ✅
**Issue:** #168 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a veteran footballer entering my mid-to-late 30s, I want realistic late-career dynamics including physical attribute decline, shorter contract terms, transitioning squad roles, and the agency to decide when to hang up my boots on my own terms.

**Acceptance Criteria:**
- [x] Pure C# Domain models for career retirement (`RetirementDecision.cs`, `RetirementReason`, `PostPlayingRole`).
- [x] Pure C# Simulation system `RetirementSystem.cs` implementing:
  - Retirement eligibility gating (age 32+ minimum).
  - Age-scaled physical attribute decay: pace, acceleration, stamina, and agility degrade progressively with age, while mental and technical attributes remain resilient.
  - Contract wind-down scaling restricting veteran contract lengths to 1-2 years.
  - Voluntary retirement choices at season end with reason and post-playing role selection (Manager, Academy Coach, TV Pundit, Club Ambassador, Private Life).
  - Formatted public farewell statements.
- [x] State persistence in `CareerSaveData.cs` and `WorldState.PlayerRetirement`.
- [x] Automated unit test suite `RetirementSystemTests.cs` (6 tests) passing with 0 failures.

---

### Story P5-009: Legacy System — Hall of Fame, Career Score Grade & Post-Retirement Summary ✅
**Issue:** #169 | **Layer:** Domain, Simulation, Unity/UI | **Status:** ✅ Complete

> As a retired player, I want a comprehensive retrospective evaluating my entire career, calculating a career legacy grade (from Journeyman to Legend to GOAT), inducting me into the Hall of Fame, and memorializing my trophies, records, and lifetime earnings.

**Acceptance Criteria:**
- [x] Pure C# Domain models for career legacy (`CareerLegacy.cs`, `LegacyGrade`, `HallOfFameEntry`, `CareerTrophyRecord`).
- [x] Pure C# Simulation logic in `LegacySystem.cs` calculating:
  - Comprehensive lifetime career score based on appearances, goals, assists, clean sheets, trophies, international caps/goals, peak rating, and lifetime wealth.
  - Legacy Grade categorization (`Underachiever`, `Journeyman`, `CultHero`, `Icon`, `Legend`, `GOAT`).
  - Hall of Fame eligibility checks and commemorative plaque generation.
- [x] UI Toolkit Presentation screen (`LegacyView.uxml`, `LegacyViewController.cs`) featuring hero grade card, lifetime statistics grid, Hall of Fame plaque presentation, and retirement announcement options.
- [x] Integrated into `CareerHubView.uxml`, `CareerHubController.cs`, and `CareerHubCoordinator.cs`.
- [x] Automated unit test suite `LegacySystemTests.cs` (4 tests) passing with 0 failures.

---

## Phase 5 Issue Status Overview

```
PHASE 5 COMPLETE (Career World):
  Milestone 5.1:    P5-001 (#159), P5-002 (#160), P5-003 (#161) (World Sim, League Hierarchy & Transfers) ✅ Complete
  Milestone 5.2:    P5-004 (#163), P5-005 (#164), P5-006 (#165) (International Football & Continental Tournaments) ✅ Complete
  Milestone 5.3:    P5-007 (#167), P5-008 (#168), P5-009 (#169) (Endorsements, Career Longevity & Legacy) ✅ Complete
```

---

# PHASE 6: Polish, Balance & Release

## MILESTONE 6.1 — Career Simulation Balance Pass & Content Expansion

### Story P6-001: 10,000-Career Simulation Balance Pass ✅
**Issue:** #171 | **Layer:** Simulation, Balance, Tests | **Status:** ✅ Complete

> As a game designer and player, I want the multi-season career simulation to be balanced across 10,000 simulated careers so that progression curves, retirement ages, transfer frequencies, financial earnings, and legacy outcomes mirror authentic professional football without runaway feedback loops or economic distortions.

**Acceptance Criteria:**
- [x] Incorporate Phase 5 systems into `CareerSimulationEngine`:
  - `RetirementSystem`: Age 32+ physical decline curve and retirement evaluation.
  - `SponsorshipSystem`: Commercial deals and earnings based on player reputation.
  - `LegacySystem`: Lifetime career scoring, legacy grades, and Hall of Fame eligibility.
- [x] Update `CareerStatistics`, `AggregateReport`, and CLI output to track:
  - Legacy Grade distributions (GOAT, Legend, Icon, Cult Hero, Journeyman, Underachiever).
  - Hall of Fame induction rate.
  - Commercial earnings vs wage earnings.
- [x] Run full 10,000-career simulation pass with `--careers 10000 --parallel`.
- [x] Validate distribution targets:
  - Peak Overall: Mean between 66-72, Elite 85+ achieved by 1-5% of players, Max <= 94.
  - Career Length: Mean 15-18 seasons, Retirement age ~33-36.
  - Bankruptcy Rate: < 1.0%.
  - Legacy Grades: GOAT (<1%), Legend (2-5%), Icon (10-15%), Cult Hero (20-30%), Journeyman (40-50%), Underachiever (5-15%).
  - Hall of Fame Induction Rate: 3-7%.
- [x] Automated balance unit tests in `FootballLife.Simulation.Tests` (`BalancePassTests.cs`).

---

### Story P6-004: Content Expansion — 100+ Life Events, 50+ Clubs, 10+ Leagues ✅
**Issue:** #172 | **Layer:** Content, Data, Simulation, Tests | **Status:** ✅ Complete

> As a footballer, I want rich, deep, and varied static content across world football so that every career feels distinct with authentic clubs across multiple European divisions, diverse leagues, and over 100 emergent life events and moral choices.

**Acceptance Criteria:**
- [x] Expand `content/data/leagues.json` to 11 leagues across England, Spain, Germany, Italy, and France across Tier 1, Tier 2, and Tier 3.
- [x] Expand `content/data/clubs.json` to 66 clubs with diverse reputations, facility ratings, budgets, tactical identities, and stadiums.
- [x] Expand `content/data/events.json` to 103 unique, narrative-rich life events across all categories (Media, Locker Room, Commercial, Personal/Family, Training, Lifestyle).
- [x] Verify all content adheres to schemas in `content/data/schema/` with zero missing references or invalid fields.
- [x] Content validation unit tests verifying 100+ events, 50+ clubs, and 10+ leagues load cleanly through `WorldDataLoader` and `LifeEventDataLoader` (`ContentExpansionTests.cs`).















