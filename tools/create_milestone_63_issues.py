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
            "title": "#P6-003: Analytics & Telemetry Pipeline — Session Events, Match Telemetry & Funnel Tracking",
            "body": """## Summary
Implement a high-performance, privacy-compliant event taxonomy, session tracking, match telemetry, and onboarding funnel analytics pipeline (UGS Analytics / PostHog compatible).

### Requirements
1. **Domain & Simulation Layer (`FootballLife.Domain` & `FootballLife.Simulation`)**:
   - `TelemetryEvent` record and `TelemetryEventType` standard taxonomy (`session_start`, `session_end`, `tutorial_step`, `match_started`, `match_ended`, `goal_scored`, `economy_transaction`, `career_milestone`).
   - `TelemetryService` with batching queue, flush thresholds, and GDPR privacy opt-out toggle.
2. **Unity Presentation Layer (`FootballLife.Unity`)**:
   - `TelemetryBridge` hook into lifecycle events, scene changes, match outcomes, and shop purchases.
3. **Automated Testing**:
   - Unit tests in `FootballLife.Simulation.Tests` verifying event dispatch, queuing, batch flushing, and opt-out suppression.
""",
            "labels": ["telemetry", "simulation", "phase-6", "milestone-6.3", "complexity:M"]
        },
        {
            "title": "#P6-006: Cloud Save Architecture — Multi-Slot Sync & Conflict Resolution",
            "body": """## Summary
Implement a robust cloud save synchronization architecture supporting cross-device progression, automatic conflict resolution (KeepNewest, KeepLocal, KeepCloud), and offline fallback.

### Requirements
1. **Domain & Simulation Layer (`FootballLife.Domain` & `FootballLife.Simulation`)**:
   - `ICloudSaveProvider` abstraction and `CloudSaveMetadata` record.
   - `CloudSaveSyncService` managing hash checks, conflict detection, and merge strategies.
   - `EmulatedCloudSaveProvider` for offline testing and deterministic headless validation.
2. **Unity Presentation Layer (`FootballLife.Unity`)**:
   - Integration with `SaveLoadManager` and `SimulationBridge` for auto-syncing upon career milestones.
3. **Automated Testing**:
   - Unit tests covering cloud backup, restoration, timestamp conflict resolution, and offline resilience.
""",
            "labels": ["simulation", "ui", "phase-6", "milestone-6.3", "complexity:M"]
        },
        {
            "title": "#P6-008: Monetization Hooks — Cosmetics Store, Career Rewind Tokens & IAP Bridge",
            "body": """## Summary
Implement clean monetization hooks and entitlement services for cosmetic items (boots, kit trims, luxury cosmetics) and gameplay convenience tokens (Career Rewind Tokens).

### Requirements
1. **Domain & Simulation Layer (`FootballLife.Domain` & `FootballLife.Simulation`)**:
   - `MonetizationProduct` record, `ProductCategory` (Cosmetic, Convenience, Pass), and catalog data.
   - `MonetizationService` handling purchase validation, token consumption, and inventory management.
   - Persistence in `CareerSaveData` (`OwnedCosmetics`, `CareerRewindTokens`).
2. **Unity Presentation Layer (`FootballLife.Unity`)**:
   - Store view integration in `LifestyleShop` / Settings.
   - Rewind confirmation flow after match loss or major injury.
3. **Automated Testing**:
   - Unit tests covering product granting, token consumption, rewind execution, and save persistence.
""",
            "labels": ["domain", "simulation", "ui", "phase-6", "milestone-6.3", "complexity:M"]
        }
    ]

    created_issues = {}

    for item in issues_to_create:
        print(f"Creating issue: {item['title']}...")
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(item).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created_issues[item['title']] = num
                print(f"Created #{num}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"HTTPError: {e.code} {e.read().decode('utf-8')}")

    with open("tools/milestone_63_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

if __name__ == "__main__":
    main()
