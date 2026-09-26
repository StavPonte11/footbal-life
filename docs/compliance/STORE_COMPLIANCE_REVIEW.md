# Football Life — Store Policy & Monetization Compliance Review

**Document Version:** 1.0 (Phase 7 / Milestone 7.8 / #P7-801)  
**Applicable Jurisdictions:** Global (Apple App Store, Google Play Store, European Union, United States, United Kingdom, Japan, Australia)  
**Audited Systems:** In-App Purchases, Career Rewind Tokens, Cosmetic Customization, Scouting Intel Pass

---

## 1. Executive Summary & Determinism Declaration

Football Life strictly adheres to fair monetization practices. **The game contains zero chance-based, randomized, or loot-box mechanics.** Every in-app purchase offers a fixed, transparent, and immediate utility or cosmetic entitlement.

```text
┌────────────────────────────────────────────────────────────────────────┐
│                        MONETIZATION AUDIT MATRIX                       │
├──────────────────────────┬─────────────────┬───────────┬───────────────┤
│ Offering Type            │ Deterministic?  │ Loot Box? │ Gambling Law? │
├──────────────────────────┼─────────────────┼───────────┼───────────────┤
│ Cosmetic Kit/Boots/Penth.│ Yes (100% fixed)│ No        │ Exempt        │
│ Career Rewind Tokens     │ Yes (100% fixed)│ No        │ Exempt        │
│ Scout Intel Pass         │ Yes (100% fixed)│ No        │ Exempt        │
└──────────────────────────┴─────────────────┴───────────┴───────────────┘
```

All offerings are deterministic digital goods. Because there is no probability-driven item drop, no odds disclosure or loot-box warning is required under Apple Guideline 3.1.1 or Google Play Loot Box Policy.

---

## 2. Career Rewind Tokens Legal Analysis

### 2.1 Mechanic Definition
A **Career Rewind Token** (`token.career_rewind_1x`, `token.career_rewind_3x`, `token.career_rewind_10x`) allows the player to revert their active career state to the previous match situation or decision checkpoint. It functions identically to an "undo" or "continue" credit in classic arcade and simulation games.

### 2.2 Apple App Store Guidelines Review
- **Guideline 3.1.1 (In-App Purchase):** Consumable items that provide extra lives, continues, or retries are standard consumable IAPs. Career Rewind Tokens are consumed upon use and depleted from the user's inventory.
- **Guideline 3.1.1 Loot Box Provision:** *"Apps offering 'loot boxes' or other mechanisms that provide randomized virtual goods for purchase must disclose the odds of receiving each type of item to customers prior to purchase."*  
  **Finding:** Rewind tokens do **not** provide randomized virtual goods; they revert deterministic state. Guideline 3.1.1 odds disclosure does not apply.
- **Guideline 5.3.4 (Gaming, Gambling, and Lotteries):** No real money or prizes can be won; tokens cannot be transferred or cashed out.

### 2.3 Google Play Store Policies Review
- **Google Play Monetization & Payments Policy:** Consumable utility items (e.g. game continues) are explicitly recognized.
- **Google Play Loot Box Policy:** *"Apps and games offering mechanisms to receive randomized virtual items from a purchase (including but not limited to 'loot boxes') must clearly disclose the odds of receiving those items in advance."*  
  **Finding:** Fully compliant. No randomized mechanics exist.

---

## 3. European Anti-Gambling Legislation Review

### 3.1 Belgian Gaming Commission (Kansspelcommissie)
Under Article 4/1 of the Belgian Gaming Act, a mechanic constitutes illegal gambling if it satisfies four cumulative criteria:
1. Game of chance.
2. Incurring an economic stake/wager.
3. Opportunity to win or lose.
4. Loss or gain of in-game value with perceived economic value.

**Evaluation:** Career Rewind Tokens and Cosmetics involve **zero chance**. The player receives exactly what they purchased without random variance. Therefore, Football Life is **fully authorized for distribution in Belgium** without geoblocking or feature disabling.

### 3.2 Dutch Gaming Authority (Kansspelautoriteit)
Under the Dutch Betting and Gaming Act (Wet op de kansspelen):
- Paid chance mechanics with transferable items are classified as illegal games of chance.
- Paid chance mechanics with non-transferable items are heavily scrutinized.

**Evaluation:** Because items in Football Life are non-random and non-transferable (bound strictly to local/cloud career saves), the game is fully compliant with Dutch gaming law.

### 3.3 UK Advertising Standards Authority (ASA) & PEGI
- In-game purchases are transparently disclosed in all marketing copy and store metadata: *"Contains In-App Purchases"*.
- The game will receive the PEGI "In-Game Purchases" content descriptor. It does **not** trigger the "Includes Random Items" descriptor.

---

## 4. Refund & Consumer Protection Policy

- **Non-Consumable Goods (Cosmetics & Passes):** Handled via platform restore transactions (`RestorePurchases`). Reinstalling the game or migrating devices recovers all owned cosmetic identifiers.
- **Consumable Tokens (Rewind Tokens):** Tracked in cloud save sync; consumed atomically upon use. In case of purchase drop, receipt validation reconciles unconsumed tokens.
- **Cooling-Off Period & Disclosures:** EU statutory consumer withdrawal rights are managed directly through Apple App Store and Google Play billing infrastructure.

---

## 5. Automated Verification in CI/CD

Catalog compliance is verified programmatically via `StoreComplianceValidator.AuditCatalog()` and unit tests in `StoreComplianceTests.cs`. Any pull request introducing an item without `IsDeterministic = true` fails compilation and test execution.
