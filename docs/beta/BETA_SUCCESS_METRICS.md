# Football Life — Closed Beta Success Metrics & KPI Framework

**Document Version:** 1.0 (Phase 7 / Milestone 7.9 / #P7-903)  
**Status:** Pre-Approved Release Gate Criteria  
**Rule:** *Define the bar for "ready for wider release" before the data arrives, not after.*

---

## 1. Upfront Release Readiness Targets

To prevent moving goalposts after observing real player behavior, the engineering and product leads have committed to the following quantitative targets:

```text
┌──────────────────────────────────────┬─────────────┬─────────────┬─────────────┐
│ Key Performance Indicator (KPI)      │ Green (GO)  │ Amber (WARN)│ Red (NO-GO) │
├──────────────────────────────────────┼─────────────┼─────────────┼─────────────┤
│ Day 1 Retention (D1)                 │ >= 45.0%    │ 37.0–44.9%  │ < 37.0%     │
│ Day 7 Retention (D7)                 │ >= 20.0%    │ 15.0–19.9%  │ < 15.0%     │
│ Median Session Duration              │ 8.0–15.0 min│ 5.0–7.9 min │ < 5.0 min   │
│ Season 1 Completion Rate (38 Weeks)  │ >= 35.0%    │ 25.0–34.9%  │ < 25.0%     │
│ Crash-Free Session Rate              │ >= 99.5%    │ 98.5–99.4%  │ < 98.5%     │
│ Average Tester Satisfaction Rating   │ >= 4.0 / 5.0│ 3.5–3.9     │ < 3.5       │
│ Match Situation Success Rate         │ 55.0–70.0%  │ 45.0–54.9%  │ Out of band │
└──────────────────────────────────────┴─────────────┴─────────────┴─────────────┘
```

---

## 2. Metric Computation Formulas & Instrumentation

### 2.1 Day 1 & Day 7 Retention
- **Formula:**
  $$\text{D1 Retention} = \frac{\text{Unique users active on Day } (t_0 + 1)}{\text{Unique users enrolled on Day } t_0}$$
  $$\text{D7 Retention} = \frac{\text{Unique users active on Day } (t_0 + 7)}{\text{Unique users enrolled on Day } t_0}$$
- **Data Source:** Tracked automatically via `TelemetryEventType.SessionStart` filtered by anonymous `SessionId`.

### 2.2 Median Session Duration
- **Formula:** Calculated across all completed sessions in a 7-day rolling window (`SessionEnd - SessionStart`).
- **Target Rationale:** Mobile sports sims thrive in the 8–15 minute "coffee break" window. Sessions shorter than 5 minutes suggest lack of immersion; sessions longer than 30 minutes suggest slow UI pacing or fatigue.

### 2.3 Season 1 Career Completion Rate
- **Formula:**
  $$\text{Season 1 Completion} = \frac{\text{Careers advancing past Week 38 Year 1}}{\text{Total careers created}}$$
- **Significance:** In single-player career sims, reaching the first off-season award ceremony is the primary indicator of long-term commitment.

### 2.4 Crash-Free Session Rate
- **Formula:**
  $$\text{Crash-Free Rate} = 1.0 - \left(\frac{\text{Total Crash Reports}}{\text{Total Sessions}}\right)$$
- **Target Rationale:** 99.5% is the industry standard for tier-1 mobile games on Google Play and the App Store.

---

## 3. Automated In-Engine Scoring

All metrics are evaluated objectively by `BetaMetricsEvaluator.EvaluateSnapshot()`. The evaluator outputs a `BetaKpiReport` that feeds directly into the release gate decision matrix.
