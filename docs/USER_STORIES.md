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

---

## MILESTONE 1.1 — Player Domain Model

### Story P1-001: Player Identity
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

### Story P1-002: Player Abilities
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

### Story P1-003: Player State
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

### Story P1-004: Player Career State
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

### Story P1-005: Player Potential
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

### Story P1-006: Position Taxonomy
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

### Story P1-007: Attribute Position Relevance
**Branch:** `feature/p1-007-attribute-relevance-matrix`
**Label:** `domain`, `phase-1`

> As a match simulation, I want to query which attributes are relevant for a given position in a given situation so that outcomes are position-authentic rather than globally generic.

**Acceptance Criteria:**
- [ ] `AttributeRelevance` record: position → list of weighted (`AttributeName`, `float Weight`) pairs
- [ ] `AttributeRelevanceMatrix` provides lookup by `Position` and optional `SituationType`
- [ ] Used by `MatchSimulation` action resolver (not hardcoded in match code)
- [ ] Unit test: `AttributeRelevance_Midfielder_PassingIsHighlyRelevant()`

---

### Stories P1-008 & P1-009: Domain Invariants & Tests
**AC:**
- [ ] All domain models enforce their invariants in constructors (no silent clamping without unit test)
- [ ] Domain assembly has zero warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- [ ] `dotnet test` reports 100% pass on all P1-001 through P1-007 tests

---

## MILESTONE 1.2 — World Domain Model

### Story P1-010: Club Model
**Branch:** `feature/p1-010-club-model`

> As a transfer system, I want a `Club` domain model encoding reputation, finances, and tactical identity so that transfer offers and career opportunities are grounded in club reality.

**AC:**
- [ ] `Club` record: `Guid Id`, `string Name`, `string Country`, `LeagueTier Tier`, `float Reputation` [0–100], `decimal AnnualBudget`, `ClubFacilities Facilities`, `TacticalIdentity Tactics`
- [ ] `LeagueTier` enum: `Top`, `Second`, `Third`, `Lower`, `NonLeague`
- [ ] `ClubFacilities` record: `TrainingQuality`, `MedicalQuality` [0–100] — affects player development rate
- [ ] Unit test: `Club_Reputation_BoundedToValidRange()`

---

### Story P1-014: Season Model
**Branch:** `feature/p1-014-season-model`

> As the simulation tick engine, I want a `Season` model defining the calendar, match schedule, and current week so that all time-based systems advance consistently.

**AC:**
- [ ] `Season` record: `int Year`, `int CurrentWeek`, `int TotalWeeks` (38–46 depending on cups), `IReadOnlyList<ScheduledMatch> Calendar`
- [ ] `ScheduledMatch`: `DateOnly Date`, `Guid HomeClubId`, `Guid AwayClubId`, `CompetitionType Competition`
- [ ] Season advances strictly forward — no `CurrentWeek` rollback
- [ ] Unit test: `Season_CurrentWeek_NeverExceedsTotalWeeks()`

---

### Story P1-016: Static Data Schema
**Branch:** `feature/p1-016-static-data-schema`

> As the simulation, I want canonical JSON schemas for clubs, leagues, and positions so that content is fully data-driven and no club or league is hardcoded in simulation logic.

**AC:**
- [ ] `clubs.json` with 20 clubs minimum (5 leagues, 4 clubs each), fields: `id`, `name`, `country`, `tier`, `reputation`, `annualBudget`, `facilities`
- [ ] `leagues.json` with 5 leagues, fields: `id`, `name`, `country`, `tier`, `clubs`
- [ ] `positions.json` with attribute weight maps per position
- [ ] Content validator checks: no duplicate IDs, no orphaned club references, valid ranges
- [ ] Unit test: `ContentValidator_DetectsDuplicateClubId()`
- [ ] Unit test: `ContentValidator_DetectsInvalidAttributeWeight()`

---

## MILESTONE 1.3 — Training System

### Story P1-019: Training XP Calculation
**Branch:** `feature/p1-019-training-xp`

> As a footballer, I want training sessions to award XP to relevant attributes based on session type and intensity so that training feels purposeful and strategically meaningful.

**AC:**
- [ ] `TrainingSystem.CalculateXP(TrainingSession session, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` returns `TrainingResult`
- [ ] `TrainingResult` contains: `IReadOnlyDictionary<AttributeName, float> XpGained`, `float FatigueCost`, `float InjuryRisk`
- [ ] Position-relevant attributes gain more XP for relevant training type
- [ ] Diminishing returns: repeated same-type sessions in same week reduce XP by 30% per repetition
- [ ] Zero `new` allocations inside the hot path (pre-allocated result struct)
- [ ] No `UnityEngine` reference
- [ ] Unit test: `TrainingSystem_Technical_AwardsPassingXP()`
- [ ] Unit test: `TrainingSystem_RepeatedSameType_ReducesXP()`
- [ ] Unit test: `TrainingSystem_HighFatigue_IncreasesInjuryRisk()`
- [ ] Determinism test: same seed → same XP distribution

---

## MILESTONE 1.5 — Match Simulation

### Story P1-029: Situation Generator
**Branch:** `feature/p1-029-match-situation-generator`

> As a footballer, I want the match simulation to generate position-appropriate situations during the match so that my interactions feel authentic to my role on the pitch.

**AC:**
- [ ] `MatchSituationGenerator.Generate(MatchState state, PlayerPosition position, SimulationRandom rng)` returns `MatchSituation?` (null = no interaction this tick)
- [ ] `MatchSituation` includes: `SituationType`, `OpponentPressure` [0–10], `AvailableActions[]`, `ExpectedValue`, `PositionContext`
- [ ] Striker receives: Shooting, RunBehindDefense, PositioningRun situations
- [ ] Midfielder receives: ReceivingUnderPressure, ThroughBall, Pressing situations
- [ ] Defender receives: Tackle, Interception, AerialChallenge, BuildUp situations
- [ ] Situation frequency influenced by match context (scoreline, time, possession)
- [ ] Determinism test: same seed, same match state → same situation sequence
- [ ] Unit test: `SituationGenerator_Striker_ReceivesShootingSituations()`
- [ ] Unit test: `SituationGenerator_WinningAt85Min_ReducesAttackingFrequency()`

---

### Story P1-030: Action Resolver
**Branch:** `feature/p1-030-action-resolver`

> As a footballer, I want my chosen action to be resolved using my abilities, current state, and the situation's context so that skill matters but outcomes have appropriate uncertainty.

**AC:**
- [ ] `ActionResolver.Resolve(MatchAction action, MatchSituation situation, PlayerAbilities abilities, PlayerState state, SimulationRandom rng)` returns `ActionOutcome`
- [ ] `ActionOutcome`: `bool Success`, `OutcomeType Type` (Goal, Save, Miss, Tackle, Foul, etc.), `float XpContribution`, `float ConfidenceDelta`
- [ ] Formula weights: relevant ability (60%) + current state (20%) + situation difficulty (10%) + randomness (10%)
- [ ] No literal randomness — all via injected `SimulationRandom`
- [ ] Risky actions (through ball, long shot) have higher variance than safe actions
- [ ] Zero allocations in resolve path (struct return)
- [ ] Determinism test: identical inputs → identical outcome
- [ ] Unit test: `ActionResolver_HighFinishing_IncreasesGoalProbability()`
- [ ] Unit test: `ActionResolver_HighFatigue_ReducesSuccess()`
- [ ] Unit test: `ActionResolver_RiskyAction_HasHigherVariance()`

---

## MILESTONE 1.9 — Relationships

### Story P1-055: Relationship Model
**Branch:** `feature/p1-055-relationship-model`

> As a life simulation, I want relationship entities with emotional context so that the game presents people as people rather than numeric bars.

**AC:**
- [ ] `Relationship` record: `Guid Id`, `Guid PersonId`, `RelationshipType Type`, `float Affinity` [0–100], `float Trust` [0–100], `DateOnly LastInteraction`, `IReadOnlyList<string> SharedHistory`
- [ ] `RelationshipType` enum: `Partner`, `Parent`, `Sibling`, `Friend`, `Teammate`, `Manager`, `Agent`
- [ ] Affinity decays 0.5 per week without interaction (but never below 20 for family)
- [ ] Unit test: `Relationship_Affinity_DecaysFromNeglect()`
- [ ] Unit test: `Relationship_FamilyAffinity_HasMinimumFloor()`

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

The Architect Agent will create GitHub Issues in this sequence:

```
Week 1:  P1-001 → P1-009   (Player Domain Model + Tests)
Week 2:  P1-010 → P1-017  (World Domain Model + Data)
Week 3:  P1-018 → P1-027  (Training + Fatigue Systems)
Week 4:  P1-028 → P1-037  (Match Simulation v1)
Week 5:  P1-038 → P1-051  (Progression + Economy + Life Events)
Week 6:  P1-052 → P1-069  (Relationships + Transfers + Career Sim)
```

Each issue will be created via:
```powershell
.\tools\gh-workflow.ps1 issue-create -Title "[P1-NNN] <title>" -Body "<acceptance criteria>" -Labels "phase-1,domain"
```
