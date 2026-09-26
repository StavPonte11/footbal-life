# Football Life — Store Age Rating Questionnaires & Data Safety Declarations

**Document Version:** 1.0 (Phase 7 / Milestone 7.8 / #P7-803)  
**Applicability:** Apple App Store (Connect), Google Play Console, IARC (International Age Rating Coalition)  
**Target Ratings:** Apple 4+, PEGI 3 (In-Game Purchases), ESRB Everyone (In-Game Purchases), USK 0 (In-Game-Käufe)

---

## 1. Executive Summary & Classification Matrix

Because Football Life contains **no violence, no profanity, no crude humor, and no chance-based gambling mechanics**, it qualifies for the most inclusive, general-audience rating across all global regulatory bodies.

```text
┌─────────────────────────────────┬──────────────────────┬───────────────────────────────────┐
│ Store / Regulatory Body         │ Target Rating Tier   │ Mandated Content Descriptors      │
├─────────────────────────────────┼──────────────────────┼───────────────────────────────────┤
│ Apple App Store                 │ 4+                   │ In-App Purchases                  │
│ PEGI (Europe / IARC)            │ PEGI 3               │ In-Game Purchases                 │
│ ESRB (North America / IARC)     │ Everyone (E)         │ In-Game Purchases                 │
│ USK (Germany / IARC)            │ USK 0                │ In-Game-Käufe                     │
│ ACB (Australia / IARC)          │ General (G)          │ Very Mild In-Game Purchases       │
│ ClassInd (Brazil / IARC)        │ Livre (L)            │ Compras no jogo                   │
└─────────────────────────────────┴──────────────────────┴───────────────────────────────────┘
```

---

## 2. Apple App Store Age Rating Questionnaire

### 2.1 Content Descriptions
- **Cartoon or Fantasy Violence:** None
- **Realistic Violence:** None
- **Profanity or Crude Humor:** None
- **Mature/Suggestive Themes:** None
- **Horror/Fear Themes:** None
- **Medical/Treatment Information:** None
- **Alcohol, Tobacco, or Drug Use/References:** None
- **Simulated Gambling:** None *(Note: Rewind tokens and store purchases are deterministic continues, not casino/wagering games)*
- **Sexual Content or Nudity:** None
- **Graphic Sexual Content and Nudity:** None

### 2.2 Feature Questions
- **Unrestricted Web Access:** No *(Game does not include an open embedded web browser)*
- **Gambling and Contests:** No
- **User-to-User Communication:** No *(Single-player offline career simulation; no open chat rooms)*
- **Advertising / Commercial Content:** No third-party behavioral ads

**Resulting Rating:** **4+** (Appropriate for ages 4 and older).

---

## 3. Google Play & IARC Rating Questionnaire

### 3.1 Violence & Sensitive Content
1. **Does the game contain violence?**  
   *Answer:* **No.** Football matches simulate standard sports gameplay (tackles, goals, celebrations) with zero gore, blood, or unsanctioned physical violence.
2. **Does the game contain fear/horror?**  
   *Answer:* **No.**
3. **Does the game contain sexuality or nudity?**  
   *Answer:* **No.**
4. **Does the game contain gambling or simulated gambling?**  
   *Answer:* **No.** The game does not allow wagering or chance-based betting.
5. **Does the game contain crude humor or profanity?**  
   *Answer:* **No.**
6. **Does the game allow users to interact or exchange content through voice or text?**  
   *Answer:* **No.**
7. **Does the game share user location with others?**  
   *Answer:* **No.**
8. **Does the game allow users to purchase digital goods?**  
   *Answer:* **Yes.** (Career Rewind Tokens, Cosmetic Customization, Scouting Intel Pass).

**Resulting Rating:**  
- **IARC:** PEGI 3 / ESRB Everyone / USK 0 / ACB G / ClassInd L.

---

## 4. Google Play Data Safety Section Declarations

Google Play requires developers to complete the Data Safety questionnaire to disclose data collection practices.

### 4.1 Data Collection Overview
- **Does your app collect or share any of the required user data types?**  
  *Answer:* **Yes.** (App performance, crash logs, and anonymous in-game analytics).
- **Is all of the user data collected by your app encrypted in transit?**  
  *Answer:* **Yes.** (HTTPS / TLS 1.3 protocol enforced).
- **Do you provide a way for users to request that their data be deleted?**  
  *Answer:* **Yes.** In-game "Erase My Data" button purges local diagnostic dumps and revokes analytics.

### 4.2 Detailed Data Type Disclosures

#### A. Diagnostics (App Info and Performance)
- **Data Element:** Crash logs & Diagnostics (stack traces, memory allocation budget).
- **Collected?** Yes (optional, player can opt out).
- **Shared with third parties?** No.
- **Purposes:** App functionality, Analytics / Bug fixing.
- **Linked to user identity?** **No.** (Anonymous device instance ID only).

#### B. App Activity (Analytics)
- **Data Element:** App interactions (match situations played, career weeks advanced).
- **Collected?** Yes (optional, player can opt out).
- **Shared with third parties?** No.
- **Purposes:** Analytics, Game balance tuning.
- **Linked to user identity?** **No.**

#### C. Personal Information / Financial / Location
- **Name, Email, User IDs:** **Not collected.**
- **Purchase History:** Managed directly by Google Play Billing; Football Life only receives local transaction validation tokens without credit card or billing details.
- **Location:** **Not collected.**
- **Photos, Videos, Audio:** **Not collected.**
- **Contacts:** **Not collected.**

---

## 5. Summary of Regulatory Sign-Off

The monetization and telemetry architecture engineered across Milestones 6.3, 7.7, and 7.8 directly backs these declarations. Any change in telemetry schemas or monetization models is validated by `StoreComplianceValidator.AuditCatalog()` and `StoreComplianceTests.cs` before public deployment.
