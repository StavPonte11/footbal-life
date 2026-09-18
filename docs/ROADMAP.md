# Football Life — Product Roadmap
> **Living document.** Last updated by Architect Agent, September 2026.
> Built from: GAME_DESIGN.md, ARCHITECTURE.md, SIMULATION_DESIGN.md, MATCH_ENGINE.md, UI_UX.md

---

## Vision Statement

> **You don't manage a football club. You live the life of a footballer.**

Football Life is a mobile-first, career + life simulation where players inhabit a single professional footballer across a complete career arc — from youth academy to retirement. Inspired by New Star Soccer, expanded with modern 3D situations, systemic life simulation, and emergent storytelling. Every career is uniquely shaped by decisions, relationships, luck, and the player's own choices.

---

## Architecture Principles (Non-Negotiable)

```
DOMAIN (Pure C#) → SIMULATION (Pure C# Deterministic) → UNITY (Presentation Only)
```

- Simulation is the single source of truth.
- Unity presents; it never owns authoritative state.
- All randomness seeded via `SimulationRandom(seed)` — fully deterministic.
- No `UnityEngine` references in `FootballLife.Domain` or `FootballLife.Simulation`.
- Zero GC allocations in simulation tick loops.
- Every system independently testable via `dotnet test`.
- Large-scale balance validated via: `career-simulator --careers 10000 --seasons 20`

---

## Phase 0 — Foundation ✅ (Complete)

**Goal:** Establish the scaffold that all future work builds on.

| Task | Status | Notes |
|---|---|---|
| Repository structure created | ✅ | Committed to GitHub |
| C# solution scaffolded (`FootballLife.slnx`) | ✅ | netstandard2.1 + net8.0 |
| `FootballLife.Domain` project created | ✅ | Empty scaffold |
| `FootballLife.Simulation` project created | ✅ | Empty scaffold |
| `FootballLife.Simulation.Tests` project created | ✅ | xUnit, 1 placeholder test passing |
| `FootballLife.CareerSimulator` CLI project created | ✅ | Entry point stubbed |
| `.gitignore` + `.gitattributes` + Git LFS configured | ✅ | |
| Agent rules + skills installed (`.agents/`) | ✅ | 53 skills |
| GitHub MCP tooling + workflow CLI | ✅ | `tools/gh-workflow.ps1` |
| Issue/PR/CR GitHub templates | ✅ | `.github/` |
| Multi-agent workflow defined | ✅ | 4 specialized agents |

---

## Phase 1 — Core Simulation Engine
**Goal:** Build the pure C# simulation that can run headless and produce believable, deterministic football careers.
**Exit Criteria:** `career-simulator --careers 1000 --seasons 5` produces statistically valid career distributions. All tests pass.

### Milestone 1.1 — Player Domain Model
**Priority: CRITICAL FIRST STEP**

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-001 | Player identity model (name, nationality, DOB, foot, position) | Domain | S |
| #P1-002 | Player Abilities model (15 attributes: pace, finishing, passing, etc.) | Domain | M |
| #P1-003 | Player State model (fatigue, confidence, form, happiness, motivation, morale, fitness) | Domain | M |
| #P1-004 | Player Career State (club, squad status, manager trust, reputation, salary, market value) | Domain | M |
| #P1-005 | Player Potential model (probabilistic development ceiling by age curve) | Domain | M |
| #P1-006 | Position taxonomy (GK, CB, FB, DM, CM, AM, LW/RW, ST) + position weight maps | Domain | S |
| #P1-007 | PlayerAttributes → position relevance matrix | Domain | M |
| #P1-008 | Domain invariants & value bounds validation | Domain | S |
| #P1-009 | Unit tests: Player model invariants, bounds, serialization | Tests | M |

### Milestone 1.2 — World Domain Model

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-010 | Club model (reputation, finances, squad quality, league, facilities, tactical identity) | Domain | M |
| #P1-011 | Manager model (tactics, trust, preferences, tolerance, formation) | Domain | S |
| #P1-012 | League & Competition model | Domain | S |
| #P1-013 | Contract model (salary, length, role, bonuses, clauses, expectations) | Domain | M |
| #P1-014 | Season model (weeks, match calendar, league table) | Domain | M |
| #P1-015 | WorldState container (clubs, players, leagues, season) | Domain | M |
| #P1-016 | Static data schema: `clubs.json`, `leagues.json`, `positions.json` | Data | M |
| #P1-017 | Data loader + content validation (duplicate IDs, missing refs, impossible values) | Simulation | M |

### Milestone 1.3 — Training System

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-018 | Training session model (type, intensity, duration) | Domain | S |
| #P1-019 | TrainingSystem: calculate XP gain per attribute from session | Simulation | M |
| #P1-020 | TrainingSystem: fatigue cost per session type | Simulation | S |
| #P1-021 | TrainingSystem: diminishing returns on repeated same-type training | Simulation | M |
| #P1-022 | TrainingSystem: injury risk calculation based on fatigue × intensity | Simulation | M |
| #P1-023 | Unit tests: training XP, fatigue, diminishing returns, injury risk | Tests | M |

### Milestone 1.4 — Fatigue & Recovery System

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-024 | FatigueSystem: tick-based fatigue accumulation (training, matches, travel) | Simulation | M |
| #P1-025 | RecoverySystem: sleep/rest reduces fatigue per day | Simulation | S |
| #P1-026 | FatigueSystem: fatigue affects form, performance, mood | Simulation | M |
| #P1-027 | Unit tests: fatigue accumulation, recovery curves | Tests | M |

### Milestone 1.5 — Match Simulation (v1)

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-028 | Match domain model (score, time, events, situation log) | Domain | M |
| #P1-029 | MatchSimulation: situation generator (position-aware, tactical context) | Simulation | L |
| #P1-030 | MatchSimulation: action resolver (pass, shoot, dribble, defend) | Simulation | L |
| #P1-031 | MatchSimulation: performance score calculator | Simulation | M |
| #P1-032 | MatchSimulation: stamina drain during match | Simulation | S |
| #P1-033 | MatchSimulation: match rating calculation (position-aware) | Simulation | M |
| #P1-034 | MatchSimulation: manager trust delta from match | Simulation | M |
| #P1-035 | MatchSimulation: confidence delta from goals/assists/errors | Simulation | S |
| #P1-036 | Determinism test: identical seed produces identical match outcome | Tests | M |
| #P1-037 | Unit tests: action resolution outcomes, match rating | Tests | L |

### Milestone 1.6 — Career Progression System

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-038 | ProgressionSystem: attribute XP to ability gain (nonlinear, age-gated) | Simulation | L |
| #P1-039 | ProgressionSystem: age-based development curve (peak 24-28, decline post-30) | Simulation | M |
| #P1-040 | ProgressionSystem: playing time contribution to development | Simulation | M |
| #P1-041 | CareerSystem: playing status evaluation (Academy→Reserve→Bench→Rotation→Starter→Key) | Simulation | M |
| #P1-042 | CareerSystem: end-of-season statistics aggregation | Simulation | M |
| #P1-043 | Unit tests: progression curves, playing status, season stats | Tests | L |

### Milestone 1.7 — Economy System

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-044 | FinanceAccount model (balance, income entries, expense entries) | Domain | S |
| #P1-045 | EconomySystem: weekly salary credit | Simulation | S |
| #P1-046 | EconomySystem: match bonuses | Simulation | S |
| #P1-047 | EconomySystem: lifestyle expense deductions | Simulation | M |
| #P1-048 | Unit tests: economy transactions, balance invariants | Tests | M |

### Milestone 1.8 — Life Events System (v1)

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-049 | LifeEvent domain model (id, conditions, weight, cooldown, choices, effects) | Domain | M |
| #P1-050 | LifeEventSystem: condition evaluator (age, salary, fatigue, happiness, trust) | Simulation | M |
| #P1-051 | LifeEventSystem: weighted random event selection | Simulation | M |
| #P1-052 | LifeEventSystem: effect applicator (mutates player state, relationships, finances) | Simulation | M |
| #P1-053 | Seed `events.json` with 20 core life events | Data | L |
| #P1-054 | Unit tests: event conditions, effect application | Tests | M |

### Milestone 1.9 — Basic Relationships

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-055 | Relationship model (type, affinity, trust, recent interaction, shared history) | Domain | M |
| #P1-056 | RelationshipSystem: affinity decay from neglect | Simulation | S |
| #P1-057 | RelationshipSystem: interaction events affecting affinity | Simulation | M |
| #P1-058 | RelationshipSystem: club transfer impact on relationships | Simulation | M |
| #P1-059 | Unit tests: relationship dynamics | Tests | M |

### Milestone 1.10 — Transfer & Contract System

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-060 | TransferOffer model (club, role, salary, contract, location, competition level) | Domain | M |
| #P1-061 | TransferSystem: generate transfer offers based on reputation + form + position need | Simulation | L |
| #P1-062 | TransferSystem: player transfer acceptance/rejection flow | Simulation | M |
| #P1-063 | ContractSystem: contract negotiation simulation | Simulation | L |
| #P1-064 | ContractSystem: contract expiry and renewal | Simulation | M |
| #P1-065 | Unit tests: transfer offer generation, contract scenarios | Tests | L |

### Milestone 1.11 — Career Simulator CLI (v1)

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P1-066 | CareerSimulator: wire Phase 1 systems end-to-end | CareerSimulator | L |
| #P1-067 | CareerSimulator: output per-career stats (peak overall, goals, transfers, retirement age) | CareerSimulator | M |
| #P1-068 | CareerSimulator: run 1000 careers, validate balance distributions | CareerSimulator | M |
| #P1-069 | Balance validation: no runaway progression, realistic peak rating 60–95 distribution | Tests | L |

---

## Phase 2 — Unity Prototype
**Goal:** First playable prototype on Unity 6 — players can create a footballer, train, play abstracted matches, and see a season summary.
**Exit Criteria:** Internal playable build completing one full season without crashes.

### Milestone 2.1 — Unity Project Bootstrap

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-001 | Initialize Unity 6 project (URP, App UI, Input System) | Unity | M |
| #P2-002 | Integration bridge: SimulationRuntime ↔ Unity session | Unity | L |
| #P2-003 | Save/load system: persist career state to disk | Unity | L |
| #P2-004 | Scene architecture: Bootstrap, MainMenu, CareerHub, Match, Home | Unity | M |
| #P2-005 | App UI design system: dark theme, typography, component library | Unity | L |

### Milestone 2.2 — Player Creation Flow

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-006 | Player creation screen: name, nationality, position, foot, appearance | Unity/UI | L |
| #P2-007 | Starting club selection screen | Unity/UI | M |
| #P2-008 | Career initialization: wire player creation to simulation | Unity | M |

### Milestone 2.3 — Home Screen (Daily Hub)

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-009 | Home screen: date, form, fatigue, next match, current events, primary actions | Unity/UI | L |
| #P2-010 | Training selection UI: categories, fatigue cost, expected XP | Unity/UI | M |
| #P2-011 | Rest/recovery action | Unity/UI | S |
| #P2-012 | Advance day: trigger simulation tick, update UI | Unity | M |

### Milestone 2.4 — Career Screen

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-013 | Career screen: club, squad status, manager trust, contract, stats | Unity/UI | M |
| #P2-014 | Profile screen: attributes separated from current state, development visual | Unity/UI | M |

### Milestone 2.5 — Match Preview & Basic Match

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-015 | Match preview screen: opponent, competition, role, manager expectations | Unity/UI | M |
| #P2-016 | Abstracted match screen: situation cards, player choices, outcome text | Unity/UI | L |
| #P2-017 | Post-match summary: rating, goal/assist, manager reaction, trust delta | Unity/UI | M |

### Milestone 2.6 — End-of-Season Screen

| Issue | User Story | Layer | Complexity |
|---|---|---|---|
| #P2-018 | Season summary screen: appearances, goals, assists, trophies, progression | Unity/UI | L |
| #P2-019 | Attribute growth visualization | Unity/UI | M |
| #P2-020 | Transfer window screen: display offers, accept/reject flow | Unity/UI | L |

---

## Phase 3 — Football Vertical Slice
**Goal:** Real-time 3D match experience for one position. The player inhabits their footballer on the pitch.
**Exit Criteria:** Complete a full playable 3D match as a striker with visible opponents, camera, animations, touch controls.

| Issue | User Story | Layer |
|---|---|---|
| #P3-001 | 3D player character: humanoid rig, basic animations (run, walk, kick, tackle) | Unity/Art |
| #P3-002 | Ball physics: realistic trajectory and collision | Unity/Physics |
| #P3-003 | Simulated teammate and opponent pawns (non-AI, situation-driven) | Unity |
| #P3-004 | Stadium environment: pitch, goals, crowd, lights | Unity/Art |
| #P3-005 | Dynamic camera rig: follow player, zoom on shots, match context | Unity/Camera |
| #P3-006 | Touch control system: tap-to-select situation, swipe-to-shoot | Unity/Input |
| #P3-007 | Match situation presenter: 3D situation setup from simulation events | Unity |
| #P3-008 | Shooting mini-interaction: aim + power system | Unity/Gameplay |
| #P3-009 | Passing mini-interaction: target select + timing | Unity/Gameplay |
| #P3-010 | Goal celebration: camera, VFX, crowd audio | Unity/Presentation |
| #P3-011 | Match HUD: score, time, stamina bar, contextual action buttons | Unity/UI |

---

## Phase 4 — Life Vertical Slice
**Goal:** The personal life layer becomes a meaningful gameplay system, not a cosmetic accessory.
**Exit Criteria:** Relationships, home, finances, and phone create real consequences on the footballer's career.

| Issue | User Story | Layer |
|---|---|---|
| #P4-001 | Home environment: interactive apartment with bed, gym, phone, door | Unity/Art |
| #P4-002 | Home progression: apartment tiers reflect salary/lifestyle | Unity |
| #P4-003 | Phone UI: messages, news feed, agent, manager, social | Unity/UI |
| #P4-004 | Relationship hub: partner, family, teammates displayed as people, not numbers | Unity/UI |
| #P4-005 | Life event system: immersive choice-cards with character context | Unity/UI |
| #P4-006 | Finances screen: balance, income, expenses, lifestyle tier | Unity/UI |
| #P4-007 | Lifestyle item shop: optional purchases that affect state and home visuals | Unity |
| #P4-008 | Social activity: go out events that cost fatigue, gain happiness | Unity |
| #P4-009 | Media section: AI-generated match reports, transfer rumors, interviews | Unity/LLM |

---

## Phase 5 — Career World
**Goal:** A living football world that evolves independently of the player's club.

| Issue | User Story | Layer |
|---|---|---|
| #P5-001 | World simulation: other players develop/decline/transfer each season | Simulation |
| #P5-002 | Transfer window: multiple clubs can bid, player can initiate interest | Simulation |
| #P5-003 | Multiple leagues: different prestige, salary ranges, competition quality | Domain |
| #P5-004 | National team system: eligibility, call-ups, international career | Simulation |
| #P5-005 | Manager change system: new manager may reset trust | Simulation |
| #P5-006 | Sponsorship system: reputation-gated endorsement income | Simulation |
| #P5-007 | Retirement arc: late-career decline, retirement decision | Simulation |
| #P5-008 | Legacy system: hall of fame, career grade, post-retirement summary | Domain |

---

## Phase 6 — Polish, Balance & Release
**Goal:** Mobile-ready release build with analytics, monetization hooks, and content depth.

| Issue | User Story | Layer |
|---|---|---|
| #P6-001 | 10,000-career simulation balance pass | Balance |
| #P6-002 | Mobile performance pass: 60fps, <200MB RAM, battery optimization | Performance |
| #P6-003 | Analytics: session events, match telemetry, funnel tracking (UGS/PostHog) | Telemetry |
| #P6-004 | Content expansion: 100+ life events, 50+ clubs, 10+ leagues | Content |
| #P6-005 | Localization: EN primary, 5 additional languages | Unity |
| #P6-006 | Cloud save via Unity Gaming Services | Unity |
| #P6-007 | Onboarding flow: first-session tutorial | Unity/UI |
| #P6-008 | Monetization hooks: cosmetic items, career replay | Unity |

---

## Complexity Key

| Code | Meaning | Estimated Days |
|---|---|---|
| S | Small — focused change, 1-2 files | 0.5–1 |
| M | Medium — 2-5 files, clear scope | 1–3 |
| L | Large — multiple systems, requires planning | 3–7 |
| XL | Extra Large — multi-sprint, requires decomposition | 7+ |

---

## Current Focus: Phase 1, Milestone 1.1

> **Next issue to create:** `#P1-001 — Player Identity Domain Model`
> See USER_STORIES.md for full acceptance criteria.