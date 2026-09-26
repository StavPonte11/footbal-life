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

### Milestone 3.4 — Match Presentation & In-Game HUD ✅ (Complete)
*Branch: `feature/milestone-3.4-match-presentation-hud` | Issues #143–#144*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P3-010 (#143) | Goal celebration: camera celebration orbit, VFX, dynamic goal banner overlay with shot speed | Unity/Presentation | M | ✅ Complete |
| #P3-011 (#144) | Match HUD: live scoreline, match clock, stamina bar, and contextual action buttons | Unity/UI | M | ✅ Complete |

---

## Phase 4 — Life Vertical Slice
**Goal:** The personal life layer becomes a meaningful gameplay system, not a cosmetic accessory.
**Exit Criteria:** Relationships, home, finances, and phone create real consequences on the footballer's career.

### Milestone 4.1: 3D Home Apartment Environment & Lifestyle Progression
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P4-001 (#146) | Home environment: interactive 3D apartment with bed, gym, phone, door hotspots and camera rig | Unity/Art | L | ✅ Complete |
| #P4-002 (#147) | Home progression: 5 apartment tiers (Modest to Superstar), sleep recovery, home gym workout, and real-time reskin | Unity/Simulation | L | ✅ Complete |

### Milestone 4.2: Smartphone OS & Social Layer
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P4-003 (#149) | Phone UI: messages (WhatsApp), news feed (Football Daily), agent, manager, social (FootyGram) | Unity/UI | M | ✅ Complete |
| #P4-004 (#150) | Relationship hub: partner, family, teammates displayed as people with affinity/trust meters & social actions | Unity/UI | M | ✅ Complete |

### Milestone 4.3: Life Events, Finances & Lifestyle Shop
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P4-005 (#152) | Life event system: immersive choice-cards with character context | Unity/UI | M | ✅ Complete |
| #P4-006 (#153) | Finances screen: balance, income, expenses, lifestyle tier | Unity/UI | M | ✅ Complete |
| #P4-007 (#154) | Lifestyle item shop: optional purchases that affect state and home visuals | Unity | M | ✅ Complete |

### Milestone 4.4: Social Activities & Dynamic Media Reports ✅ (Complete)
*Branch: `feature/milestone-4.4-social-activities-press-media` | Issues #156, #157*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P4-008 (#156) | Social activity: go out events that cost fatigue, gain happiness & outing catalog | Simulation/Unity | S | ✅ Complete |
| #P4-009 (#157) | Media section: dynamic match reports, transfer rumors, interactive press conferences | Domain/Simulation/Unity | L | ✅ Complete |

---

## Phase 5 — Career World
**Goal:** A living football world that evolves independently of the player's club.

### Milestone 5.1: World Simulation, Multi-Tier League Hierarchy & Dynamic Transfer Market ✅ (Complete)
*Branch: `feature/milestone-5.1-world-simulation-transfers` | Issues #159, #160, #161*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P5-001 (#159) | World simulation: NPC player development/aging/decline, squad replenishment, league progression | Simulation | L | ✅ Complete |
| #P5-002 (#160) | Transfer market: multi-club bidding wars, transfer requests & Transfer Market UI | Domain/Simulation/UI | L | ✅ Complete |
| #P5-003 (#161) | Multi-tier league ecosystem: division prestige, wage scaling, promotion & relegation | Domain/Simulation/UI | M | ✅ Complete |

### Milestone 5.2: International Football & Continental Tournaments (Completed)
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P5-004 | National team system: eligibility, call-ups, international career | Simulation | L | ✅ Complete |
| #P5-005 | Continental competitions: Champions Cup, continental qualification & fixtures | Domain/Simulation/UI | L | ✅ Complete |
| #P5-006 | Manager change system: new manager hiring, style & trust reset | Simulation | M | ✅ Complete |

### Milestone 5.3: Endorsements, Career Longevity & Legacy (Completed)
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P5-007 (#167) | Sponsorship system: reputation-gated endorsement deals & commercial perks | Simulation/UI | M | ✅ Complete |
| #P5-008 (#168) | Retirement arc: late-career physical decline, contract winds down, retirement choice | Simulation/UI | M | ✅ Complete |
| #P5-009 (#169) | Legacy system: hall of fame, career grade, post-retirement summary | Domain/Simulation/UI | M | ✅ Complete |

---

## Phase 6 — Polish, Balance & Release
**Goal:** Mobile-ready release build with analytics, monetization hooks, and content depth.

### Milestone 6.1: Career Simulation Balance & Content Expansion ✅ (Complete)
*Branch: `feature/milestone-6.1-balance-content-expansion` | Issues #171, #172*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P6-001 (#171) | 10,000-career simulation balance pass | Simulation/Balance | L | ✅ Complete |
| #P6-004 (#172) | Content expansion: 100+ life events, 50+ clubs, 10+ leagues | Content/Data | L | ✅ Complete |

> 🔴 **Escalated — real club/league naming (originally flagged at Milestone 1.2, #P1-016):** this was flagged as an open decision when the sample data was 5 leagues / handful of clubs. It appears to have been left unresolved and carried forward through content expansion — 50+ clubs and 10+ leagues are now built on real, licensed names (Premier League, La Liga, etc.) per `leagues.json`. This is no longer a "decide before scaling" item; it's now a blocking legal question before any external user — beta or public — can touch a build, since distributing real club/league identity without a license is the exposure, not just shipping it. Resolve this explicitly in Phase 7 (Milestone 7.5 below) before Milestone 7.8's beta program: either commit to fictional-but-recognizable renaming across all 50+ clubs/10+ leagues (a content-and-art-asset-touching change, not a quick find-replace, since crests/kits reference real identities too) or pursue licensing. Don't let beta testers be the ones who discover this wasn't decided.

### Milestone 6.2: Onboarding Flow & Localization
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P6-007 (#174) | Onboarding flow: first-session tutorial & character creation | Domain/Sim/UI | M | ✅ Complete |
| #P6-005 (#175) | Localization: EN primary, 4 additional languages (ES, DE, FR, IT) | Domain/Sim/UI | M | ✅ Complete |

### Milestone 6.3: Analytics, Cloud Save & Monetization Hooks
| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P6-003 (#177) | Analytics: session events, match telemetry, funnel tracking (UGS/PostHog) | Domain/Sim/Telemetry | M | ✅ Complete |
| #P6-006 (#178) | Cloud save via multi-slot sync & conflict resolution | Domain/Sim/Unity | M | ✅ Complete |
| #P6-008 (#179) | Monetization hooks: cosmetic items, career rewind tokens & catalog | Domain/Sim/Unity | M | ✅ Complete |

### Milestone 6.4: Mobile Performance & Release Validation ✅ (Complete)
*Branch: `feature/milestone-6.4-mobile-performance-release` | Issue #181*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P6-002 (#181) | Mobile performance pass: 60fps, <200MB RAM, zero-GC hot path, battery optimization & release validation | Domain/Sim/Unity/Perf | L | ✅ Complete |

---

## Phase 7 — Pre-Launch Polish & Real-User Readiness
**Goal:** Close the gap between "the systems are complete" and "a stranger can pick this up, understand it, and want to keep playing" — before any beta or public test.
**Why this phase exists:** Phases 1–6 validated that the game *works* (deterministic simulation, 647+ tests, 60fps/<200MB targets, all systems wired). None of that establishes that it's *fun to a stranger* or *safe to hand to one*. The only two human-judgment checkpoints in the whole roadmap so far are Gate 1.6.5 (simulation feel, console-only) and Gate 2.7 (Unity prototype, automated + audit). Everything from Phase 3 onward — the entire 3D match experience, the full life sim, world sim, monetization, onboarding, localization — shipped without a further human playtest gate. Phase 7 exists to close that gap before Milestone 7.8 puts the build in front of people who didn't build it.

### Milestone 7.0 — 🛑 Validation Gate: End-to-End Human Playtest ✅ (Complete)
*Branch: `feature/milestone-7.0-validation-gate` | Issues #183, #184, #185 | Review: [docs/gate-7.0-review.md](file:///Users/stavponte/Desktop/stav/projects/footbal-life/docs/gate-7.0-review.md)*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-000a (#183) | 3–5 people outside the build's author(s) play a full career session (creation → several seasons → at least one transfer, one life event, one retirement path if time allows), current build, no guidance given | Quality | M | ✅ Complete |
| #P7-000b (#184) | Structured debrief: where did they get confused, bored, stuck, or quit? What did they *think* a screen/button did vs. what it did? | Quality | S | ✅ Complete |
| #P7-000c (#185) | Gate review doc (`docs/gate-7.0-review.md`) — same format as prior gates — GO/NO-GO plus a ranked friction list that becomes the real Phase 7 backlog | Docs/Quality | S | ✅ Complete |

### Milestone 7.1 — Character Art & Rigging
*Merged (PR #198, Issues #195–#197)*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-101 (#195) | Base Humanoid Mesh & Rig (Outfield & GK with Standard Bone Hierarchy & AvatarBuilder Humanoid Avatar) | Unity/Art | L | ✅ Merged (#198) |
| #P7-102 (#196) | Modular Face, Hair & Skin Tone Variation System (6 hairstyles, 5 skin tones, facial hair, deterministic visual identity) | Unity/Art | M | ✅ Merged (#198) |
| #P7-103 (#197) | Kit & Appearance System Compatibility Pass (Procedural club-color kit schemes & jersey squad numbers) | Unity/Art | S | ✅ Merged (#198) |

### Milestone 7.2 — Animation System
*Source: kicks/dives currently rotate bones via code (`transform.localRotation = Quaternion.Euler(...)`) in `PlayerPawnController.cs` / `GoalkeeperController.cs`.*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-201 | Mecanim Animator Controller + Locomotion BlendTree (Idle ↔ Jog ↔ Sprint by velocity), replacing procedural sway | Unity/Animation | M | Not started |
| #P7-202 | Action clip set: power shot, finesse curl, header, sliding tackle, diving save (mocap or curated asset-store library, not code rotation) | Unity/Animation | L | Not started |
| #P7-203 | Celebration clip set: knee slide, fist pump, crowd wave | Unity/Animation | S | Not started |
| #P7-204 | Retire the procedural rotation code paths once clip-driven equivalents are verified — don't run both indefinitely | Unity/Animation | S | Not started |

### Milestone 7.3 — Audio & Sound Design
*Merged (PR #194, Issues #189–#193)*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-301 (#189) | AudioMixer & AudioSettings routing: SFX / Ambience / UI / Music buses with logarithmic dB conversions, mute toggles, and volume persistence | Unity/Audio | M | ✅ Merged (#194) |
| #P7-302 (#190) | Match SFX: kick impact (power-scaled), post/crossbar metallic clang, net ripple swoosh, referee whistle (start/goal/fulltime) | Unity/Audio | M | ✅ Merged (#194) |
| #P7-303 (#191) | Crowd ambience: idle stadium murmur loop, reactive goal celebration roar, woodwork/near-miss/save gasp | Unity/Audio | M | ✅ Merged (#194) |
| #P7-304 (#192) | UI SFX: button clicks, screen tab switches, wage-day coin chime, life event alert bell | Unity/Audio | S | ✅ Merged (#194) |
| #P7-305 (#193) | Menu/background music track management with procedural synthesis and automatic ducking during match highlights | Unity/Audio | M | ✅ Merged (#194) |

### Milestone 7.4 — Stadium Atmosphere & Visual Polish
*Source: current stadium is pitch + floodlights + ad boards; no stands, no crowd.*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-401 | Stadium grandstands: tiered seating geometry, dugouts, roof, scaled by club reputation tier (a lower-league ground shouldn't look identical to a title contender's) | Unity/Art | L | Not started |
| #P7-402 | Crowd system: GPU-instanced or billboard spectators, reacting to goals/near-misses — profile against the <200MB RAM / 60fps targets before committing to a technique | Unity/Art/Perf | L | Not started |
| #P7-403 | Match VFX pass: goal-net ripple, turf dust on tackles/slides, ball trail refinement beyond the current simple renderer | Unity/VFX | M | Not started |

### Milestone 7.5 — Content, Iconography & IP Resolution
*#P7-501 merged (PR #188, Issue #187)*

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-501 (#187) | **Resolve real vs. fictional club/league naming** (Safe Fictional Renaming Pass: 11 leagues & 66 clubs + stadiums converted in data & code) | Content/Legal | L | ✅ Merged (#188) |
| #P7-502 | Custom vector icon set for lifestyle shop items, replacing emoji placeholders (🚗👟⚽) in `tokens.uss`/UXML | UI/Art | M | Not started |
| #P7-503 | Club crest & league emblem illustrations for all clubs/leagues (unblocked by #P7-501) | UI/Art | L | Not started |
| #P7-504 | Player card portrait system, newspaper front-page illustration templates for media/press-conference screens | UI/Art | M | Not started |

### Milestone 7.6 — UX Flow, Onboarding & Accessibility Audit

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-601 | Re-test the onboarding tutorial (#P6-007) specifically against Gate 7.0 findings — a tutorial validated only by its own authors is a common source of first-session drop-off | UX | M | Not started |
| #P7-602 | Empty-state, loading-state, and error-state pass across every screen (e.g. no transfer offers yet, save failed, network unavailable for cloud save) — currently undocumented anywhere in the roadmap | UI/UX | M | Not started |
| #P7-603 | Accessibility pass: text scaling, color-blind-safe check on the emerald/gold token palette (green/gold confusion is a common deuteranopia failure mode), touch-target sizing audit on real devices, not just the 390×844 reference frame | UI/UX | M | Not started |
| #P7-604 | One-handed / thumb-reachability review of primary actions on larger modern phone sizes (reference frame is 390×844; verify on 6.7"+ devices) | UI/UX | S | Not started |

### Milestone 7.7 — Device Matrix, Crash Reporting & Live Monitoring

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-701 | Real-device performance validation: the 60fps/<200MB RAM targets from #P6-002 were profiler-asserted — verify on 2–3 actual low/mid-tier Android devices, not simulator/high-end-only | Unity/Perf | M | Not started |
| #P7-702 | Crash & exception reporting integration (Crashlytics/Sentry or equivalent) — without this, bugs found by beta testers in Milestone 7.8 vanish without a trace | Unity/Telemetry | S | Not started |
| #P7-703 | Save-corruption and cloud-save-conflict manual test pass — #P6-006 claims "conflict resolution" but this is exactly the kind of edge case that only surfaces under real, messy usage (killed mid-write, two devices, offline-then-sync) | Unity/QA | M | Not started |

### Milestone 7.8 — Compliance & Monetization Review

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-801 | Review "career rewind tokens" (#P6-008) against Apple/Google store policy on chance-based or pay-to-undo mechanics, and against loot-box disclosure law in markets that regulate it (Belgium, and age-rating questionnaires elsewhere) — confirm what's being sold is deterministic (a rewind, not a randomized reward) and that store listings reflect that accurately | Legal/Compliance | M | Not started |
| #P7-802 | Privacy policy + data-disclosure pass for analytics (#P6-003) and cloud save (#P6-006) — required for store submission regardless of team size, and stricter if the game's audience skews toward minors | Legal/Compliance | S | Not started |
| #P7-803 | Age rating questionnaire prep (Apple App Store / Google Play / IARC) — do this before Milestone 7.9's beta, since the answers depend on final monetization and content decisions from #P7-501/#P7-801 | Legal/Compliance | S | Not started |

### Milestone 7.9 — Closed Beta Program
**Only opens once Milestones 7.0–7.8 have a GO.**

| Issue | User Story | Layer | Complexity | Status |
|---|---|---|---|---|
| #P7-901 | Recruit a closed beta cohort (target size TBD) genuinely outside the project — not friends who already know the vision | Product | S | Not started |
| #P7-902 | In-app feedback/bug-report mechanism (don't rely on beta testers finding you elsewhere) | Unity/UI | S | Not started |
| #P7-903 | Define beta success metrics up front: D1/D7 retention, median session length, career-completion rate (creation → at least one season end), crash-free session rate — decide the bar for "ready for wider release" before the data arrives, not after | Product | S | Not started |
| #P7-904 | Beta retrospective + go/no-go for public release, informed by #P7-903's metrics | Product | M | Not started |

---

| Code | Meaning | Estimated Days |
|---|---|---|
| S | Small — focused change, 1-2 files | 0.5–1 |
| M | Medium — 2-5 files, clear scope | 1–3 |
| L | Large — multiple systems, requires planning | 3–7 |
| XL | Extra Large — multi-sprint, requires decomposition | 7+ |

---

## Current Focus: Phase 7, Milestone 7.2 — Animation System

All systems across Phases 1–6 and Milestones 7.0, 7.5, 7.3, and 7.1 are implemented, tested, and validated:
- **Phase 1**: Foundations & Core Loop (Domain Models, Engine, Match Situations, Training, Weekly Loop)
- **Phase 2**: Unity Prototype (Player Creation, Daily Hub, Career Screen, Abstracted Match, Season End)
- **Phase 3**: 3D Match Experience (Pitch, Stadium, Cameras, Ball Physics, Input Controls, Visual Feedback)
- **Phase 4**: UI/UX & Life Layer (Home, Phone/Social, Life Events, Finances, Lifestyle Shop)
- **Phase 5**: Career World (World Simulation, Transfers, International Football, Legacy)
- **Phase 6**: Polish, Balance & Release (10,000-Career Balance, Content Expansion, Onboarding, Localization, Telemetry, Cloud Save, Monetization, Mobile Performance)
- **Phase 7 Progress**:
  - ✅ **Milestone 7.0**: Gate Review & Playtest debrief (`docs/gate-7.0-review.md`, PR #186).
  - ✅ **Milestone 7.5 (#P7-501)**: Safe Fictional Renaming Pass & IP Resolution (11 leagues, 66 clubs, PR #188).
  - ✅ **Milestone 7.3**: Audio & Sound Design (Mixers, Kicks, Whistle, Crowd Roar, UI feedback, PR #194).
  - ✅ **Milestone 7.1**: Character Art & Rigging (Base Humanoid Rig & Avatar, Stylized Meshes, Hair Styles, Kit & Numbers, PR #198).
  - ⏳ **Milestone 7.2**: Animation System (Mecanim Animator Controller, Locomotion BlendTrees, Action & Celebration Clips).

> **CURRENT FOCUS:** Phase 7 — Milestone 7.2 Animation System (`#P7-201` – `#P7-204`).
> Sequence authorized: 7.5 (Done) $\rightarrow$ 7.3 (Done) $\rightarrow$ 7.1 (Done) $\rightarrow$ 7.2 $\rightarrow$ 7.4 $\rightarrow$ 7.6 $\rightarrow$ 7.7 $\rightarrow$ 7.8 $\rightarrow$ 7.9.
> See [USER_STORIES.md](file:///c:/Users/User/Desktop/Stav/projects/footbal-life/docs/USER_STORIES.md) for full acceptance criteria on Phases 1–6.