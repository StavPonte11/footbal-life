import os
import sys
import json
import urllib.request
import urllib.error
import subprocess

def get_token():
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GITHUB_PERSONAL_ACCESS_TOKEN")
    if token:
        return token
    try:
        proc = subprocess.run(
            ["gh", "auth", "token"],
            text=True,
            capture_output=True,
            check=True
        )
        t = proc.stdout.strip()
        if t:
            return t
    except Exception:
        pass
    try:
        proc = subprocess.run(
            ["git", "credential", "fill"],
            input="protocol=https\nhost=github.com\n",
            text=True,
            capture_output=True,
            check=True
        )
        for line in proc.stdout.splitlines():
            if line.startswith("password="):
                return line.split("=", 1)[1].strip()
    except Exception:
        pass
    return None

def get_repo():
    try:
        proc = subprocess.run(
            ["git", "remote", "get-url", "origin"],
            text=True,
            capture_output=True,
            check=True
        )
        url = proc.stdout.strip()
        if "github.com" in url:
            parts = url.replace(":", "/").split("github.com/")[-1].replace(".git", "").split("/")
            if len(parts) >= 2:
                return f"{parts[0]}/{parts[1]}"
    except Exception:
        pass
    return "StavPonte11/footbal-life"

def main():
    token = get_token()
    repo = get_repo()

    if not token:
        print("Error: No GitHub token found.")
        sys.exit(1)

    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }

    issues_to_create = [
        {
            "id": "#P7-801",
            "title": "#P7-801 Store Policy & Loot Box Compliance Review for Career Rewind Tokens",
            "body": """### Summary
Audit career rewind tokens and all monetization catalog items against Apple App Store, Google Play, and international consumer protection standards (e.g. Belgian Gaming Commission, Dutch Kansspelautoriteit, and UK ASA):
- Verify 100% determinism: rewind tokens and cosmetics are transparent, fixed utility purchases with zero chance-based or randomized outcome mechanics.
- In-engine `StoreComplianceValidator` and `ProductComplianceInfo` models confirming catalog validity.
- Comprehensive compliance and disclosure documentation (`docs/compliance/STORE_COMPLIANCE_REVIEW.md`).

### Acceptance Criteria
- Domain model for product compliance, determinism flags, and odds disclosures.
- Simulation validator confirming no loot-box or chance mechanics exist in the catalog.
- Complete store policy audit documentation for App Store & Google Play submission.
""",
            "labels": ["compliance", "monetization", "phase-7", "milestone-7.8"]
        },
        {
            "id": "#P7-802",
            "title": "#P7-802 Privacy Policy Framework, Consent Management (GDPR/CCPA/COPPA) & Data Disclosure",
            "body": """### Summary
Implement a complete privacy compliance and consent management framework covering telemetry, crash diagnostics, and cloud save:
- Privacy consent state tracking per user (`Analytics`, `CrashReporting`, `CloudSave`).
- Granular consent controls with full opt-in/opt-out, right-to-be-forgotten (data deletion), and data export capabilities.
- Minor age gating enforcing strict COPPA and GDPR-K protections (no tracking below regional age of digital consent).
- Integration with TelemetryManager and CrashDiagnosticService to suppress dispatches when consent is withheld.
- Legal privacy policy documentation (`docs/compliance/PRIVACY_POLICY.md`).

### Acceptance Criteria
- `PrivacyConsentService` and `PrivacyConsentState` domain/simulation architecture.
- Gating in telemetry and crash reporting pipelines based on active consent.
- Age gating logic blocking tracking for minors.
- Full unit test coverage for consent grant/revocation/export.
""",
            "labels": ["compliance", "privacy", "phase-7", "milestone-7.8"]
        },
        {
            "id": "#P7-803",
            "title": "#P7-803 Store Age Rating Questionnaire & Data Safety Section Declarations",
            "body": """### Summary
Prepare and validate accurate, synchronized answers for store age-rating questionnaires and platform data disclosures:
- Apple App Store Age Rating Questionnaire (ensuring 4+ or 9+ rating without gambling, violence, or user-generated content flags).
- Google Play Content Rating & IARC Questionnaire (PEGI 3 / ESRB Everyone).
- Google Play Data Safety Section declarations matching actual client payload signatures (ephemeral device IDs, crash traces, anonymous gameplay metrics).
- In-engine `AgeRatingProfile` domain model and verification suite.
- Comprehensive store questionnaire reference document (`docs/compliance/AGE_RATING_QUESTIONNAIRES.md`).

### Acceptance Criteria
- Verified questionnaire responses documented for Apple, Google Play, and IARC.
- In-engine age rating models and store profile validation.
- Unit tests verifying data safety declarations against telemetry event schemas.
""",
            "labels": ["compliance", "legal", "phase-7", "milestone-7.8"]
        }
    ]

    created_issues = {}

    for item in issues_to_create:
        print(f"Creating issue: {item['title']}...")
        payload = {
            "title": item["title"],
            "body": item["body"],
            "labels": item["labels"]
        }
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(payload).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created_issues[item["id"]] = num
                print(f"Created Issue #{num}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"Failed to create issue: {e.code} - {e.read().decode('utf-8')}")

    with open("tools/milestone_78_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.8 issues recorded in tools/milestone_78_issues.json")

if __name__ == "__main__":
    main()
