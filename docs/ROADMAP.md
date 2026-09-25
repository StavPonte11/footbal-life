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

**Progress (as of Sep 2026):**
- **PHASE 1 IS 100% COMPLETE & MERGED TO MAIN** ✅
- **All 83 issues (#1 through #57, #64 through #68, #70 through #75, #77 through #81, #83 through #88, #90 through #93) closed.**
- **All PRs (#11–#16, #25–#32, #58–#63, #69, #76, #82, #89, #94) merged to `main`.**
- **524 tests passing** (0 failures, 0 skipped, 0 warnings) in `FootballLife.Simulation.Tests`.
- **Phase 1 Exit Criteria fully met**: High-throughput multi-career simulation tested and verified across 1,000+ careers in under 17 seconds (61+ careers/sec) with statistically validated distributions.
- **Next Active Target:** Phase 2 — Unity Prototype (Milestone 2.1 — Unity Project Bootstrap: Issues #P2-001 through #P2-005).

### Milestone 1.1 — Player Domain Model ✅ (Complete)
*PRs #11–#16 merged | Issues #2–#10 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-001 (#2) | Player identity model (name, nationality, DOB, foot, position) | Domain | S | ✅ Merged (#11) |
| #P1-002 (#3) | Player Abilities model (15 attributes: pace, finishing, passing, etc.) | Domain | M | ✅ Merged (#12) |
| #P1-003 (#4) | Player State model (fatigue, confidence, form, happiness, motivation, morale, fitness) | Domain | M | ✅ Merged (#13) |
| #P1-004 (#5) | Player Career State (club, squad status, manager trust, reputation, salary, market value) | Domain | M | ✅ Merged (#14) |
| #P1-005 (#6) | Player Potential model (probabilistic development ceiling by age curve) | Domain | M | ✅ Merged (#15) |
| #P1-006 (#7) | Position taxonomy (GK, CB, FB, DM, CM, AM, LW, RW, ST) + position weight maps | Domain | S | ✅ Merged (#16) |
| #P1-007 (#8) | PlayerAttributes → position relevance matrix | Domain | M | ✅ Merged (#16) |
| #P1-008 (#9) | Domain invariants & value bounds validation | Domain | S | ✅ Merged (#16) |
| #P1-009 (#10) | Unit tests: Player model invariants, bounds, serialization | Tests | M | ✅ Merged (#16) |

### Milestone 1.2 — World Domain Model ✅ (Complete)
*PRs #25–#32 merged | Issues #17–#24 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-010 (#17) | Club model (reputation, finances, squad quality, league, facilities, tactical identity) | Domain | M | ✅ Merged (#25) |
| #P1-011 (#18) | Manager model (tactics, trust, preferences, tolerance, formation) | Domain | S | ✅ Merged (#26) |
| #P1-012 (#19) | League & Competition model | Domain | S | ✅ Merged (#27) |
| #P1-013 (#20) | Contract model (salary, length, role, bonuses, clauses, expectations) | Domain | M | ✅ Merged (#28) |
| #P1-014 (#21) | Season model (weeks, match calendar, league table) | Domain | M | ✅ Merged (#29) |
| #P1-015 (#22) | WorldState container (clubs, players, leagues, season) | Domain | M | ✅ Merged (#30) |
| #P1-016 (#23) | Static data schema: `clubs.json`, `leagues.json`, `positions.json` | Data | M | ✅ Merged (#31) |
| #P1-017 (#24) | Data loader + content validation (duplicate IDs, missing refs, impossible values) | Simulation | M | ✅ Merged (#32) |

> ⚠️ **Open decision — #P1-016 real-world naming:** the shipped `leagues.json` sample uses real competitions (Premier League, La Liga, Bundesliga, Serie A, Ligue 1). That's a reasonable placeholder for dev/test data, but it's a decision point, not a default: real club/league/player names sit inside licenses held by EA/FIFA and the leagues themselves in most major markets. Before content scales past sample data (i.e. before committing to the full "20 clubs / 5 leagues" set in the AC below), explicitly decide fictional-but-recognizable naming vs. pursuing a license, and record the decision here. Don't let this default-by-inertia into real names because that's what the sample data happened to use.

### Milestone 1.2.5 — Simulation Foundations (Gap Fixes) ✅ (Complete)
*PR #58 merged | Issues #33–#36 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-GAP-1 (#33) | `SimulationRandom`: deterministic seeded RNG (uniform, gaussian, pick) | Simulation | S | ✅ Merged (#58) |
| #P1-GAP-2 (#34) | `PlayerFactory`: career initialization with bounded abilities & contract | Simulation | M | ✅ Merged (#58) |
| #P1-GAP-3 (#35) | `PlayerPotential` in `WorldState` first-class container & data loader | Domain | S | ✅ Merged (#58) |
| #P1-GAP-4 (#36) | `FixtureGenerator`: round-robin league schedule & balanced home/away | Simulation | M | ✅ Merged (#58) |

### Milestone 1.3 — Training System ✅ (Complete)
*PR #59 merged | Issues #37–#40 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-018 (#37) | Training session model (type, intensity, duration, result) | Domain | S | ✅ Merged (#59) |
| #P1-019 (#38) | TrainingSystem: calculate XP gain per attribute from session & fatigue cost | Simulation | M | ✅ Merged (#59) |
| #P1-020 (#39) | TrainingSystem: weekly diminishing returns & max session limits | Simulation | M | ✅ Merged (#59) |
| #P1-021 (#40) | Unit tests: training XP, fatigue, diminishing returns, injury risk | Tests | M | ✅ Merged (#59) |

### Milestone 1.4 — Fatigue & Recovery System ✅ (Complete)
*PR #60 merged | Issues #41–#43 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-024 (#41) | FatigueSystem: tick-based fatigue accumulation (training, matches, sleep, daily tick) | Simulation | M | ✅ Merged (#60) |
| #P1-025 (#42) | FatigueSystem: performance degradation & form modifier | Simulation | M | ✅ Merged (#60) |
| #P1-026 (#43) | Unit tests: fatigue accumulation, performance penalties, multi-day cycles | Tests | S | ✅ Merged (#60) |

### Milestone 1.5 — Match Simulation (v1) ✅ (Complete)
*PR #61 merged | Issues #44–#49 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-028 (#44) | Match domain model (MatchState, MatchEvent, MatchResult, Situation) | Domain | M | ✅ Merged (#61) |
| #P1-029 (#45) | MatchSimulation: situation generator (position-aware tables, context modifiers) | Simulation | L | ✅ Merged (#61) |
| #P1-030 (#46) | MatchSimulation: action resolver (ability weights, fatigue impact, gaussian variance) | Simulation | L | ✅ Merged (#61) |
| #P1-031 (#47) | MatchSimulation: full match runner & 90-minute simulation loop | Simulation | M | ✅ Merged (#61) |
| #P1-032 (#48) | MatchSimulation: manager trust update & squad status expectations | Simulation | S | ✅ Merged (#61) |
| #P1-033 (#49) | Integration tests: match pipeline, determinism, statistical outcomes | Tests | M | ✅ Merged (#61) |

### Milestone 1.6 — Career Progression System ✅ (Complete)
*PR #62 merged | Issues #50–#53 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-038 (#50) | ProgressionSystem: attribute XP to ability gain (nonlinear, age curve, physical decline) | Simulation | L | ✅ Merged (#62) |
| #P1-039 (#51) | CareerSystem: playing status evaluation (composite score & squad status steps) | Simulation | M | ✅ Merged (#62) |
| #P1-040 (#52) | CareerSystem: end-of-season statistics aggregation (SeasonStats) | Simulation/Domain | M | ✅ Merged (#62) |
| #P1-041 (#53) | Integration tests: multi-season progression curves, career balance, determinism | Tests | L | ✅ Merged (#62) |

### 🛑 Milestone 1.6.5 — Validation Gate: Minimum Playable Loop ✅ (Complete)
*PR #63 merged | Issues #54–#57 closed | Validation Decision: GO*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-GATE-1 (#54) | Minimal console harness: 38-week interactive playable loop (`--interactive`) | CareerSimulator | S | ✅ Merged (#63) |
| #P1-GATE-2 (#55) | Human playtest validation: ST & MF career playthroughs | Quality | S | ✅ Completed |
| #P1-GATE-3 (#56) | Gate review documentation: [`docs/gate-review.md`](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/gate-review.md) formal evaluation | Docs/Quality | S | ✅ Completed |
| #P1-GATE-4 (#57) | Go/No-Go checkpoint: formal approval to proceed to Milestone 1.7 | Quality | S | ✅ Passed (GO) |

---

### Milestone 1.7 — Economy System ✅ (Complete)
*PR #69 merged | Issues #64–#68 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-044 (#64) | FinanceAccount model (balance, income entries, expense entries) | Domain | S | ✅ Merged (#69) |
| #P1-045 (#65) | EconomySystem: weekly salary credit | Simulation | S | ✅ Merged (#69) |
| #P1-046 (#66) | EconomySystem: match bonuses | Simulation | S | ✅ Merged (#69) |
| #P1-047 (#67) | EconomySystem: lifestyle expense deductions | Simulation | M | ✅ Merged (#69) |
| #P1-048 (#68) | Unit tests: economy transactions, balance invariants | Tests | M | ✅ Merged (#69) |

### Milestone 1.8 — Life Events System (v1) ✅ (Complete)
*PR #76 merged | Issues #70–#75 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-049 (#70) | LifeEvent domain model (id, conditions, weight, cooldown, choices, effects) | Domain | M | ✅ Merged (#76) |
| #P1-050 (#71) | LifeEventSystem: condition evaluator (age, salary, fatigue, happiness, trust) | Simulation | M | ✅ Merged (#76) |
| #P1-051 (#72) | LifeEventSystem: weighted random event selection | Simulation | M | ✅ Merged (#76) |
| #P1-052 (#73) | LifeEventSystem: effect applicator (mutates player state, relationships, finances) | Simulation | M | ✅ Merged (#76) |
| #P1-053 (#74) | Seed `events.json` with 20 core life events | Data | L | ✅ Merged (#76) |
| #P1-054 (#75) | Unit tests: event conditions, effect application | Tests | M | ✅ Merged (#76) |

### Milestone 1.9 — Basic Relationships ✅ (Complete)
*PR #82 merged | Issues #77–#81 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-055 (#77) | Relationship model (type, affinity, trust, recent interaction, shared history) | Domain | M | ✅ Merged (#82) |
| #P1-056 (#78) | RelationshipSystem: affinity decay from neglect | Simulation | S | ✅ Merged (#82) |
| #P1-057 (#79) | RelationshipSystem: interaction events affecting affinity | Simulation | M | ✅ Merged (#82) |
| #P1-058 (#80) | RelationshipSystem: club transfer impact on relationships | Simulation | M | ✅ Merged (#82) |
| #P1-059 (#81) | Unit tests: relationship dynamics | Tests | M | ✅ Merged (#82) |

### Milestone 1.10 — Transfer & Contract System ✅ (Complete)
*PR #89 merged | Issues #83–#88 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-060 (#83) | TransferOffer model (club, role, salary, contract, location, competition level) | Domain | M | ✅ Merged (#89) |
| #P1-061 (#84) | TransferSystem: generate transfer offers based on reputation + form + position need | Simulation | L | ✅ Merged (#89) |
| #P1-062 (#85) | TransferSystem: player transfer acceptance/rejection flow | Simulation | M | ✅ Merged (#89) |
| #P1-063 (#86) | ContractSystem: contract negotiation simulation | Simulation | L | ✅ Merged (#89) |
| #P1-064 (#87) | ContractSystem: contract expiry and renewal | Simulation | M | ✅ Merged (#89) |
| #P1-065 (#88) | Unit tests: transfer offer generation, contract scenarios | Tests | L | ✅ Merged (#89) |

### Milestone 1.11 — Career Simulator CLI (v1) ✅ (Complete)
*PR #94 merged | Issues #90–#93 closed*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P1-066 (#90) | CareerSimulator: wire Phase 1 systems end-to-end | CareerSimulator | L | ✅ Merged (#94) |
| #P1-067 (#91) | CareerSimulator: output per-career stats (peak overall, goals, transfers, retirement age) | CareerSimulator | M | ✅ Merged (#94) |
| #P1-068 (#92) | CareerSimulator: run 1000 careers, validate balance distributions | CareerSimulator | M | ✅ Merged (#94) |
| #P1-069 (#93) | Balance validation: no runaway progression, realistic peak rating 60–95 distribution | Tests | L | ✅ Merged (#94) |

---

## Phase 2 — Unity Prototype
**Goal:** First playable prototype on Unity 6 — players can create a footballer, train, play abstracted matches, and see a season summary.
**Exit Criteria:** Internal playable build completing one full season without crashes.

### Milestone 2.1 — Unity Project Bootstrap ✅ (Complete)
*Branch: `feature/milestone-2.1-unity-bootstrap` | Issues #95, #100, #97–#99*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-001 (#95) | Initialize Unity 6 project (URP, App UI, Input System, Asmdefs) | Unity | M | ✅ Complete |
| #P2-002 (#100) | Integration bridge: SimulationRuntime <-> Unity session | Unity | L | ✅ Complete |
| #P2-003 (#97) | Save/load system: persist career state to disk | Unity | L | ✅ Complete |
| #P2-004 (#98) | Scene architecture: Bootstrap, MainMenu, CareerHub, Match, Home | Unity | M | ✅ Complete |
| #P2-005 (#99) | App UI design system: dark theme, typography, component library | Unity | L | ✅ Complete |

### Milestone 2.2 — Player Creation Flow ✅ (Complete)
*Branch: `feature/milestone-2.2-player-creation` | Issues #102–#104*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-006 (#102) | Player creation screen: name, nationality, position, foot, appearance | Unity/UI | L | ✅ Complete |
| #P2-007 (#103) | Starting club selection screen | Unity/UI | M | ✅ Complete |
| #P2-008 (#104) | Career initialization: wire player creation to simulation | Unity | M | ✅ Complete |

### Milestone 2.3 — Home Screen (Daily Hub) ✅ (Complete)
*Branch: `feature/milestone-2.3-daily-hub` | Issues #106–#109*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-009 (#106) | Home screen: date, form, energy, next match, current events, primary actions | Unity/UI | L | ✅ Complete |
| #P2-010 (#107) | Training selection UI: categories, energy cost, XP preview | Unity/UI | M | ✅ Complete |
| #P2-011 (#108) | Rest/recovery action panel (Light Rest vs Physio) | Unity/UI | S | ✅ Complete |
| #P2-012 (#109) | Advance day: trigger simulation tick, event dispatch, UI refresh | Unity | M | ✅ Complete |

### Milestone 2.4 — Career Screen & Profile

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-013 (#115) | Career screen: club, squad status, manager trust, contract, stats | Unity/UI | M | ✅ Complete |
| #P2-014 (#116) | Profile screen: attributes separated from current state, development visual | Unity/UI | M | ✅ Complete |

### Milestone 2.5 — Match Preview & Basic Match ✅ (Complete)
*Branch: `feature/milestone-2.5-match-system` | Issues #118–#120*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-015 (#118) | Match preview screen: opponent, competition, role, manager expectations | Unity/UI | M | ✅ Complete |
| #P2-016 (#119) | Abstracted match screen: situation cards, player choices, outcome text | Unity/UI | L | ✅ Complete |
| #P2-017 (#120) | Post-match summary: rating, goal/assist, manager reaction, trust delta | Unity/UI | M | ✅ Complete |

### Milestone 2.6 — End-of-Season & Transfers ✅ (Complete)
*Branch: `feature/milestone-2.6-season-end-transfers` | Issues #122–#124*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-018 (#122) | Season summary screen: appearances, goals, assists, trophies, progression, financial review | Unity/UI | L | ✅ Complete |
| #P2-019 (#123) | Attribute growth visualization: OVR delta, physical/technical/mental changes, potential ceiling, age curve | Unity/UI | M | ✅ Complete |
| #P2-020 (#124) | Transfer window screen: current contract, suitor bids, renewal offers, accept/reject, season rollover | Unity/UI | L | ✅ Complete |

### 🛑 Milestone 2.7 — Prototype Playtest & Gate 2.1 ✅ (Complete)
*Branch: `feature/milestone-2.7-playtest-gate-2.1` | Issues #127–#129 | Validation Decision: GO*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P2-021 (#127) | End-to-end Unity playable loop harness: automated validation runner (5/5 stages pass) | Unity/Editor | M | ✅ Complete |
| #P2-022 (#128) | Full prototype UX, performance & zero-GC memory audit | Quality | S | ✅ Complete |
| #P2-023 (#129) | Gate 2.1 review documentation: [`docs/gate-2.1-review.md`](file:///Users/stavponte/Desktop/stav/projects/footbal-life/docs/gate-2.1-review.md) formal evaluation & GO approval | Docs/Quality | S | ✅ Complete |

---

## Phase 3 — Football Vertical Slice
**Goal:** Real-time 3D match experience for one position. The player inhabits their footballer on the pitch.
**Exit Criteria:** Complete a full playable 3D match as a striker with visible opponents, camera, animations, touch controls.

### Milestone 3.1 — 3D Pitch, Stadium & Ball Physics ✅ (Complete)
*Branch: `feature/milestone-3.1-pitch-stadium-physics` | Issues #131–#133*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P3-004 (#131) | Stadium environment: regulation pitch, line markings, 3D goalposts, net, perimeter boards & lighting | Unity/Art | M | ✅ Complete |
| #P3-002 (#132) | Ball physics: realistic trajectory, friction, aerodynamic Magnus effect & goal detection volume trigger | Unity/Physics | M | ✅ Complete |
| #P3-005 (#133) | Dynamic match camera rig: Broadcast, ActionAim, ShotTrack & Celebration modes with smooth tracking | Unity/Camera | M | ✅ Complete |

### Milestone 3.2 — Player Pawn, Animations & Teammates ✅ (Complete)
*Branch: `feature/milestone-3.2-player-and-pawns` | Issues #135–#136*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P3-001 (#135) | 3D player character: humanoid rig, basic animations (run, walk, kick, tackle, celebrate) | Unity/Art | M | ✅ Complete |
| #P3-003 (#136) | Simulated teammate and opponent pawns (situation-driven, active goalkeeper goal line tracking) | Unity | M | ✅ Complete |

### Milestone 3.3 — Touch Controls & Interactive Gameplay Situations ✅ (Complete)
*Branch: `feature/milestone-3.3-touch-controls-situations` | Issues #138–#141*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P3-006 (#138) | Touch control system: tap-to-target pass, swipe-to-shoot aim/power/spin gestures | Unity/Input | M | ✅ Complete |
| #P3-007 (#139) | Match situation presenter: 3D situation setup driven by simulation events (BoxFinish, 1v1, Cross, ThroughBall) | Unity | M | ✅ Complete |
| #P3-008 (#140) | Shooting mini-interaction: 3D aim trajectory arc, power scaling, and Magnus curve spin | Unity/Gameplay | M | ✅ Complete |
| #P3-009 (#141) | Passing mini-interaction: teammate target selection, delivery timing, and defender intercept evaluation | Unity/Gameplay | M | ✅ Complete |

### Milestone 3.4 — Match Presentation & In-Game HUD
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P3-010 | Goal celebration: camera, VFX, crowd audio | Unity/Presentation | M | ⏳ Planned |
| #P3-011 | Match HUD: score, time, stamina bar, contextual action buttons | Unity/UI | M | ⏳ Planned |

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

## Current Focus: Phase 3 — Football Vertical Slice (Milestone 3.1: 3D Match Experience)

> **PHASE 1 IS 100% COMPLETE & MERGED TO `main`** (Issues #1–#57, #64–#68, #70–#75, #77–#81, #83–#88, #90–#93 closed, 557 passing tests).
> **PHASE 2 IS 100% COMPLETE & MERGED TO `main`** (Milestones 2.1–2.6: Issues #95, #100, #97–#99, #102–#104, #106–#109, #115–#116, #118–#120, #122–#124 closed).
> **Next phase to implement:** Phase 3 — Football Vertical Slice (`#P3-001` → `#P3-011`: Humanoid rig, ball physics, 3D match gameplay, touch controls, goal celebrations).
> See [USER_STORIES.md](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/USER_STORIES.md) for full acceptance criteria.