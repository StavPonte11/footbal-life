# Football Life — Privacy Policy & Data Disclosure Framework

**Effective Date:** September 26, 2026  
**Policy Version:** 2026.1-v1.0 (#P7-802)  
**Applicability:** iOS (Apple App Store) & Android (Google Play Store)  
**Applicable Regulations:** EU GDPR / UK GDPR, California CCPA/CPRA, Children’s Online Privacy Protection Act (COPPA), Brazilian LGPD

---

## 1. Overview & Data Minimization Commitment

**Football Life** is a single-player career simulation game. We believe privacy is a fundamental human right. Our core design philosophy adheres strictly to **Data Minimization** and **Privacy by Design**:

- **No Personally Identifiable Information (PII):** We do not collect names, email addresses, phone numbers, contacts, physical addresses, or location data.
- **No Third-Party Advertising Trackers:** We do not embed ad mediation SDKs that track users across other apps or websites (zero IDFA / GAID behavioral profiling).
- **Zero Sale or Sharing of Data:** We never sell, rent, or trade player data to data brokers or advertising networks.

---

## 2. Categories of Data Collected & Purpose

```text
┌──────────────────────────┬─────────────────────────────┬─────────────────────────────────┐
│ Category                 │ Data Elements               │ Purpose & Legal Basis           │
├──────────────────────────┼─────────────────────────────┼─────────────────────────────────┤
│ Telemetry & Analytics    │ Anonymous Session ID, match │ Gameplay balance, tutorial      │
│ (Optional - Opt-In/Out)  │ situation outcomes, weekly  │ completion rates, feature usage.│
│                          │ progression milestones      │ Basis: Consent (GDPR Art. 6(1)a)│
├──────────────────────────┼─────────────────────────────┼─────────────────────────────────┤
│ Diagnostics & Crashes    │ Call stack traces, memory   │ Stability debugging, crash-free │
│ (Optional - Opt-In/Out)  │ budget (MB), battery level, │ session rate guarantee.         │
│                          │ recent non-PII breadcrumbs  │ Basis: Legitimate Interest /    │
│                          │                             │ Consent                         │
├──────────────────────────┼─────────────────────────────┼─────────────────────────────────┤
│ Cloud Save Synchronization│ Encrypted career save state │ Cross-device progression backup │
│ (Core Gameplay Feature)  │ payload, SHA-256 checksum   │ and save corruption recovery.   │
│                          │                             │ Basis: Contractual Necessity    │
└──────────────────────────┴─────────────────────────────┴─────────────────────────────────┘
```

---

## 3. Children's Privacy Protections (COPPA & GDPR-K)

Protecting children is paramount:
1. **Age Gating:** During initial launch and onboarding, players can specify their age tier.
2. **Under 13 (COPPA) & Under 16 (GDPR-K) Protections:**
   - Analytics collection is **permanently disabled and blocked** (`ConsentStatus.Denied`).
   - Personalized identifiers and crash analytics are suppressed.
   - Cloud save synchronization operates solely on encrypted gameplay state without external user profiling.

---

## 4. Player Privacy Controls & Statutory Rights

### 4.1 Granular In-Game Controls
Players can view and alter their consent preferences at any time in `Settings > Privacy & Data Rights`:
- **Analytics Toggle:** Opt-in or opt-out of gameplay telemetry.
- **Diagnostics Toggle:** Opt-in or opt-out of automated error reporting.

### 4.2 Right of Access & Data Portability (GDPR Art. 15 / 20)
Players can tap **"Export My Data"** in the game settings to generate a human-readable JSON archive containing:
- Career profile snapshot and stats.
- Entitlement records (purchased cosmetics and remaining rewind tokens).
- Current consent audit records.

### 4.3 Right to Erasure / "Be Forgotten" (GDPR Art. 17)
Players can tap **"Erase My Data"** in game settings. This immediately:
1. Purges all offline diagnostic breadcrumbs and stored crash dumps from disk.
2. Flushes and empties memory-buffered telemetry queues.
3. Sets all analytics consent states to permanently denied.
4. Resets cloud synchronization identifiers.

---

## 5. Security & Storage Architecture

- **Encryption in Transit:** All telemetry and cloud synchronization transactions utilize TLS 1.3 encryption.
- **Integrity Validation:** Save files and backups employ SHA-256 cryptographic checksums to guard against tampering and corruption.
- **Retention Period:** Telemetry and crash reports are automatically purged from analysis servers after 90 days.

---

## 6. Contact Information & Data Protection Officer

For privacy inquiries, rights enforcement, or questions regarding this policy, players may contact:
- **Email:** `privacy@football-life-game.com`
- **Data Protection Inquiries:** `dpo@football-life-game.com`
