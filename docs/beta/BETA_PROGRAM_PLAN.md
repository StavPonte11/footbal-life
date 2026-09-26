# Football Life — Closed Beta Program Plan & Cohort Strategy

**Document Version:** 1.0 (Phase 7 / Milestone 7.9 / #P7-901)  
**Target Duration:** 4 Weeks (28 Days)  
**Distribution Platforms:** Apple TestFlight (iOS) & Google Play Console Internal/Closed Testing (Android)  
**Target Cohort Size:** 150 – 250 External Testers

---

## 1. Program Philosophy: True External Testers

As articulated in the Phase 7 roadmap (#P7-901), **friends and internal team members who already know the vision must not constitute the validation cohort.** When friends test, they instinctively excuse confusing onboarding, forgive missing feedback cues, and play according to developer instructions.

To ensure authentic data, the closed beta cohort must consist of genuine external mobile gamers who have never interacted with the engineering team.

```text
┌────────────────────────────────────────────────────────────────────────┐
│                      BETA COHORT DISTRIBUTION (250 MAX)                │
├──────────────────────┬─────────────┬─────────────┬─────────────────────┤
│ Cohort Name          │ Target Size │ Channel     │ Gamer Profile       │
├──────────────────────┼─────────────┼─────────────┼─────────────────────┤
│ External Pioneers    │ 100         │ TestFlight  │ New Star Soccer &   │
│                      │             │ & Play Store│ Retro Goal fans     │
│ Mobile Gamers        │ 75          │ Play Store  │ Casual / midcore    │
│                      │             │             │ mobile sports gamers│
│ Football Sim Fans    │ 50          │ TestFlight  │ FM / Career mode    │
│                      │             │             │ hardcore tacticians │
│ Community VIPs       │ 25          │ Direct VIP  │ Content creators /  │
│                      │             │             │ football moderators │
└──────────────────────┴─────────────┴─────────────┴─────────────────────┘
```

---

## 2. Recruitment & Qualification Channels

### 2.1 External Acquisition Funnel
1. **Targeted Reddit & Discord Outreach:**
   - Communities: `r/iosgaming`, `r/androidgaming`, `r/footballmanagergames`, `r/fifacareers`.
   - Clear value proposition: *"Seeking 100 footballers to live an emergent 20-year career. No gacha, no pay-to-win, pure deterministic progression."*
2. **Pre-Screening Questionnaire:**
   - Device model & OS version (to ensure coverage across Low, Mid, and High performance tiers per #P7-701).
   - Primary mobile gaming habits (sessions per day, typical session length).
   - Familiarity with career simulation games.
3. **Invitation Pass Delivery:**
   - Qualified candidates receive a personalized invitation code (e.g. `PIONEER-2026`, `MOBILE-KICKOFF`) redeemed directly inside the game or via TestFlight public link.

---

## 3. Four-Week Phased Test Schedule

```text
┌────────────────────────────────────────────────────────────────────────┐
│ Week 1: FTUE & Onboarding (Days 1–7)                                   │
│ • Focus: Player creation, tutorial clarity, Day 1 retention (D1)       │
│ • Milestone: First 5 career weeks completed by 80% of cohort           │
├────────────────────────────────────────────────────────────────────────┤
│ Week 2: Core Loop & Match Situations (Days 8–14)                       │
│ • Focus: 3D match situation feel, difficulty curve, training choices   │
│ • Milestone: Season 1 mid-point (Week 19) reached                      │
├────────────────────────────────────────────────────────────────────────┤
│ Week 3: Mid-Career, Life Layer & Transfers (Days 15–21)                │
│ • Focus: Lifestyle shop, relationships, contract renewal, transfers    │
│ • Milestone: First season completed (Season 1 end ceremony)            │
├────────────────────────────────────────────────────────────────────────┤
│ Week 4: Multi-Season Progression & Retrospective (Days 22–28)          │
│ • Focus: National team call-ups, career rewind token utility, longevity│
│ • Milestone: Post-beta survey & Go/No-Go release gate evaluation       │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 4. In-App Feedback Integration (#P7-902)

Testers must not be forced to leave the game to file reports. A floating bug icon and a dedicated **"Send Feedback"** button in the Settings menu invoke the `BetaFeedbackService`:
- Direct submission of category: `Bug`, `MatchEngine`, `CareerProgression`, `UIUX`, `BalanceEconomy`, or `GeneralSuggestion`.
- Automatic attachment of the last 30 diagnostic breadcrumbs and device hardware profile.
- Offline persistence ensuring feedback filed in airplanes or poor connectivity is cached and dispatched on next launch.
