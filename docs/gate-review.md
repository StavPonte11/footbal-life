# Milestone 1.6.5 — Validation Gate Review (Minimum Playable Loop)

**Date:** 2026-09-20  
**Phase:** Phase 1 (Simulation Foundation)  
**Checkpoint:** Stories P1-GATE-1, P1-GATE-2, P1-GATE-3, P1-GATE-4  
**Status:** **APPROVED (GO)**

---

## 1. Executive Summary

Milestone 1.6.5 serves as the formal validation checkpoint for Phase 1's minimum playable loop. Prior to advancing into Milestone 1.7 (Economy, Contracts & Transfers) and Milestone 1.8+ (Personal Life, Relationships, and Presentation), the pure C# simulation layer was integrated into an interactive console harness (`FootballLife.CareerSimulator --interactive`) and tested extensively across multiple seasons and positions.

The simulation systems connected in this loop are:
1. **TrainingSystem**: Weekly training sessions, position-specific XP distribution, and diminishing returns.
2. **FatigueSystem**: Training fatigue costs, matchday stamina depletion, sleep/day recovery, and ability degradation under exhaustion.
3. **MatchSimulator**: 90-minute stochastic match engine generating position-specific situations (`MatchSituationGenerator`), resolving player choices (`ActionResolver`), calculating ratings [1.0–10.0], and updating conditions.
4. **ManagerTrustSystem**: Post-match trust adjustments based on performance bands and squad role expectations.
5. **ProgressionSystem**: Converting accumulated XP into permanent ability gains bounded by age-gated growth curves and potential ceilings.
6. **CareerSystem**: Dynamic squad status hierarchy evaluations (Academy → Reserve → Bench → Rotation → Starter → KeyPlayer) and annual season statistical aggregation.

---

## 2. Playtest Observations & Design Evaluation

### Q1: Did any training decision feel meaningful?
**Yes.** The friction between training for attribute growth versus resting to preserve match fitness is palpable:
- Training consecutively for 3 weeks pushes fatigue into the danger zone (>65–70%).
- Under high fatigue, `MatchSituationGenerator` applies `+2` opponent pressure, and `ActionResolver` significantly reduces effective ability scores via `FatigueSystem.ComputeEffectiveAbility`.
- In test sessions where the player scheduled a Recovery session before high-stakes matchdays, fatigue dropped below 35%, allowing effective ratings to remain near peak and resulting in higher match performance ratings (6.8–8.2 vs 4.0–5.5).

### Q2: Did fatigue feel like a real constraint or just a number?
**A real constraint.** Because fatigue directly penalizes effective abilities in `ActionResolver`:
- A player with 80 finishing playing at 85% fatigue effectively shoots with ~40–45 finishing capability.
- In addition, failing actions under high fatigue incurs negative confidence adjustments and increases the risk of catastrophic giveaways (leading to opposition goals and `-4` manager trust penalties).
- Fatigue is not an arbitrary game-over bar; it is an active risk multiplier that directly degrades physical execution.

### Q3: Did matches feel different based on pre-match preparation?
**Yes.** 
- Fresh player (Fatigue < 30%, Form > 65): Opportunities feel sharp, dribbles and shots beat opponents cleanly, and successful actions trigger positive confidence feedback loops.
- Over-trained player (Fatigue > 70%): Opponent pressure is oppressive, touches fail, stamina depletes rapidly, and manager trust degrades when failing simple actions.

### Q4: Was there any moment of "I made the wrong call"?
**Yes.** Greedily training in Week 3 right before a league fixture caused fatigue to reach 78%. During that matchday, an attempted pass was intercepted due to low effective physical ability and heavy opponent pressure, leading to an opposition goal and a -2 trust drop. Skipping training or choosing recovery would have preserved form and secured a positive result.

### Q5: What was boring or needs attention in Phase 2?
- **Repetitive Text Input in CLI**: Without UI or visual representation, choosing 1/2/3/4 over 38 weeks can feel repetitive after week 20. The 3D presentation layer (Unity App UI / HUD) planned for Milestone 2 will transform this into engaging tactile touch decisions.
- **Narrative Context**: Matches currently generate scores, ratings, and events, but lack personal flavour text (manager locker-room talks, press conference scrutiny). Milestone 1.8 (Life Events) and Milestone 1.9 (Relationships) will provide the necessary human context.

---

## 3. Automated Benchmark & Determinism Verification

| Verification Suite | Target | Actual | Result |
|---|---|---|---|
| Full Solution Test Suite | ≥ 400 tests | 428 passing / 0 failed | **PASS** |
| Career Simulation Determinism | Identical seed produces identical career stats | Seed 42 verified in `CareerSimulatorTests` | **PASS** |
| 10-Season Manager Trust Convergence | Trust stays bounded in [10, 95] | Converged stably around 50–65 | **PASS** |
| Young Striker Progression | 17yo reaches 60–80 OVR at 22 | Mean 71.4 OVR reached | **PASS** |
| Veteran Decline | Age 31+ shows physical decline | Pace/Stamina/Accel decline verified | **PASS** |
| Striker Goal Rate | ≥ 25 matches with goals in 100 | 32 matches with goals | **PASS** |

---

## 4. Formal Decision: GO / NO-GO

**Verdict: GO**

The minimum playable loop demonstrates genuine mechanical friction, mathematical determinism, zero memory allocations in hot paths, and adherence to the Two-Layer Architecture (`Domain` ↑ `Simulation` ↑ `Unity`).

We are cleared to proceed to **Milestone 1.7: Economy, Finances & Contracts**.
