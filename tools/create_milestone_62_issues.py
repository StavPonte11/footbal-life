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
            "title": "#P6-007: Onboarding Flow — First-Session Tutorial & Character Creation",
            "body": """## Summary
Implement a polished, seamless First-Time User Experience (FTUE) and onboarding tutorial flow guiding new players from initial character creation through their first training session, debut match, phone setup, and home apartment.

### Requirements
1. **Domain & Simulation Layer (`FootballLife.Domain` & `FootballLife.Simulation`)**:
   - `OnboardingStep` enum and `OnboardingState` immutable model.
   - `OnboardingSystem` tracking tutorial progression, feature unlocking, and starter rewards.
   - Persistence in `CareerSaveData` (`IsTutorialCompleted`, `CurrentTutorialStep`, `CompletedSteps`).
   - Non-linear skip option for veteran players.
2. **Unity Presentation Layer (`FootballLife.Unity`)**:
   - `TutorialOverlayView.uxml` and `TutorialOverlayController.cs` providing contextual callouts, animated spotlights, and instructional tooltips.
   - Seamless integration with `PlayerCreationCoordinator`, `CareerHubCoordinator`, and `MatchCoordinator`.
3. **Automated Testing**:
   - Unit tests in `FootballLife.Simulation.Tests` verifying step progression, reward granting, state persistence, and skipping.
""",
            "labels": ["ui", "simulation", "phase-6", "milestone-6.2", "complexity:M"]
        },
        {
            "title": "#P6-005: Localization Infrastructure — Multi-Language Support (EN, ES, DE, FR, IT)",
            "body": """## Summary
Implement high-performance, pure C# and UI Toolkit localization infrastructure supporting English (primary) alongside 4 major European football languages (Spanish, German, French, Italian).

### Requirements
1. **Domain & Simulation Layer (`FootballLife.Domain` & `FootballLife.Simulation`)**:
   - `GameLanguage` enum and language catalog.
   - Pure C# `LocalizationService` supporting string table lookups, pluralization, token replacement, and fallback to English.
   - Data-driven translation catalogs in `content/data/localization/` (`en.json`, `es.json`, `de.json`, `fr.json`, `it.json`).
2. **Unity Presentation Layer (`FootballLife.Unity`)**:
   - `LocalizationManager` bridge for UI Toolkit visual elements and runtime language switching.
   - Language selector dropdown/dialog in UI.
   - Persisted language preference in settings/player profile.
3. **Automated Testing**:
   - Unit tests verifying key completeness across all language catalogs, token syntax validation, and fallback mechanisms.
""",
            "labels": ["domain", "simulation", "ui", "phase-6", "milestone-6.2", "complexity:M"]
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

    with open("tools/milestone_62_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

if __name__ == "__main__":
    main()
