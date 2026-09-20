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

> **Status (Sep 2026):** Milestones 1.1 through 1.9 are **COMPLETE and MERGED to `main`** (Stories P1-001 through P1-059, P1-GAP-1 through P1-GAP-4, and P1-GATE-1 through P1-GATE-4). All 73 related GitHub issues (#1–#57, #64–#68, #70–#75, #77–#81) and PRs (#11–#16, #25–#32, #58–#63, #69, #76, #82) are closed. Build is green with **489 unit & integration tests passing** (0 failures, 0 warnings).
>
> **Validation Gate 1.6.5 PASSED (GO)**: The minimum playable loop was playtested via the interactive console harness (`FootballLife.CareerSimulator --interactive`) and formal evaluation documented in [`docs/gate-review.md`](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/gate-review.md).
>
> **Current Active Milestone:** Milestone 1.10 — Transfer & Contract System (Stories P1-060 through P1-065).

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

## MILESTONE 1.10 — Transfer System

### Story P1-061: Transfer Offer Generation
**Branch:** `feature/p1-061-transfer-offer-generation`

> As a footballer, I want transfer offers to appear when my reputation, form, and position need align with a club's requirements so that transfer opportunities feel earned and believable.

**AC:**
- [ ] `TransferSystem.GenerateOffers(Player player, WorldState world, SimulationRandom rng)` returns `IReadOnlyList<TransferOffer>`
- [ ] Offers only generated from clubs with matching `LeagueTier` ± 1 relative to player reputation
- [ ] Offer salary computed as: `BaseWage * ReputationMultiplier * LeaguePremium`
- [ ] Minimum 1 offer per transfer window if player `ManagerTrust` < 30 (forced to move)
- [ ] Maximum 3 simultaneous offers
- [ ] Determinism test: same seed + same world → same offers
- [ ] Unit test: `TransferSystem_HighReputation_AttractsTopClubOffers()`
- [ ] Unit test: `TransferSystem_LowTrust_GeneratesAtLeastOneEscapeOffer()`

---

## MILESTONE 1.11 — Career Simulator CLI

### Story P1-066: Headless Career Simulation
**Branch:** `feature/p1-066-career-simulator`

> As a game designer, I want to run 1000+ full football careers headlessly so that I can validate balance, detect progression anomalies, and tune systems before Unity integration.

**AC:**
- [ ] `dotnet run --project simulation/FootballLife.CareerSimulator -- --careers 1000 --seasons 10` completes without exceptions
- [ ] Outputs per-career CSV: `seed, peak_overall, goals, assists, transfers, retirement_age, max_salary`
- [ ] Outputs aggregate stats: mean, p10, p50, p90 for each metric
- [ ] Peak overall distribution: 60–99 range, mean ~74, no careers reaching 99 before age 24
- [ ] Transfer count: mean 2–4 over 15 seasons
- [ ] Determinism: `--seed 42` always produces identical output
- [ ] Unit test: `CareerSimulator_DeterministicSeed_ProducesIdenticalResults()`

---

## Phase 1 Issue Creation Priority

```
COMPLETED & MERGED TO MAIN:
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

CURRENT ACTIVE TARGET:
  Milestone 1.10:   P1-060 → P1-065   (Transfer & Contract System) 🚀 Ready to Start

UPCOMING:
  Milestone 1.11:   P1-066 → P1-069   (Career Simulator CLI & 10k Career Balance)
```

