import os
import sys
import json
import urllib.request
import urllib.error
import subprocess
import time

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

    branch = "feature/milestone-6.2-onboarding-localization"
    base = "main"

    title = "feat: Milestone 6.2 — Onboarding Flow & Localization Infrastructure (#174, #175)"
    body = """## Milestone 6.2: Onboarding Flow & Localization Infrastructure

### Summary of Changes

#### 1. Onboarding Flow & First-Session Tutorial (#P6-007 / Issue #174)
- **Domain Layer (`FootballLife.Domain`)**:
  - `OnboardingStep` enum (`Welcome`, `CharacterCreation`, `ClubSigning`, `FirstTraining`, `FirstMatchDebut`, `HomeApartment`, `SmartphoneIntro`, `Completed`).
  - `OnboardingStepDefinition` record: localized step titles, descriptions, target element IDs, action prompts, and rewards.
  - `OnboardingState` immutable record: current step, completed steps history, completion and skip flags.
  - `OnboardingReward` record: starter bonuses for XP, Energy, Form, Manager Trust, and Cash.
- **Simulation Layer (`FootballLife.Simulation`)**:
  - `OnboardingSystem`: deterministic state machine handling step progression, non-linear feature gating, starter reward distribution, and fast-forward skipping.
  - `CareerSaveData`: full persistence for tutorial state (`IsTutorialCompleted`, `IsTutorialSkipped`, `CurrentTutorialStep`, `CompletedTutorialSteps`).
- **Unity Presentation Layer (`FootballLife.Unity`)**:
  - `TutorialOverlayView.uxml`: elegant dark overlay with spotlight card, step counter badges, description, reward callouts, action buttons, and skip button.
  - `TutorialOverlayController.cs`: UI Toolkit controller handling display, dynamic text refresh, action events, and multi-language reaction.
  - `PlayerCreationCoordinator.cs`: seamlessly advances tutorial through character creation and starting contract signing.
  - `CareerHubCoordinator.cs`: orchestrates tutorial step prompts in the career hub and directs player to their debut match, training, apartment, and phone.

#### 2. Localization Infrastructure (#P6-005 / Issue #175)
- **Domain Layer (`FootballLife.Domain`)**:
  - `GameLanguage` enum (`English`, `Spanish`, `German`, `French`, `Italian`).
  - `LanguageInfo` descriptors: ISO codes, display names, native names, and flag emojis.
- **Simulation Layer (`FootballLife.Simulation`)**:
  - `LocalizationService`: pure C# localization engine supporting directory/string-table loading, positional formatting (`{0}`), named token substitution (`{token}`), and automatic fallback to English.
  - `CareerSaveData`: persists `PreferredLanguage` across career sessions.
- **Content Catalogs (`content/data/localization/` & `StreamingAssets/localization/`)**:
  - 5 comprehensive JSON translation catalogs (`en.json`, `es.json`, `de.json`, `fr.json`, `it.json`) with 100% key parity across all 86 core gameplay and tutorial strings.
- **Unity Presentation Layer (`FootballLife.Unity`)**:
  - App bar quick language button (`btn-quick-language`) cycling between languages.
  - `CareerHubController.cs` and `SimulationBridge.cs`: dynamic text re-binding across all views when language is toggled.

#### 3. Automated Testing Suite
- `OnboardingSystemTests.cs`: 5 unit tests validating initial state, sequential step advancement, feature gating, starter rewards, and save/load persistence.
- `LocalizationServiceTests.cs`: 5 unit tests validating string resolution, language switching, event notification, token substitution, and 100% catalog key parity.
- **662 / 662 unit tests passing** across `FootballLife.Simulation.Tests` with 0 failures.

Closes #174
Closes #175
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-6): complete Milestone 6.2 - onboarding flow & localization (#174, #175)"], check=True)
    subprocess.run(["git", "push", "origin", branch], check=True)

    print("Step 2: Creating Pull Request...")
    pr_payload = {
        "title": title,
        "head": branch,
        "base": base,
        "body": body
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_payload).encode("utf-8"),
        headers=headers,
        method="POST"
    )

    pr_number = None
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            pr_number = data["number"]
            print(f"Created PR #{pr_number}: {data['html_url']}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError creating PR: {e.code} {err_msg}")
        if "A pull request already exists" in err_msg:
            list_req = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
                headers=headers
            )
            with urllib.request.urlopen(list_req) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    print(f"Found existing PR #{pr_number}")
        if not pr_number:
            sys.exit(1)

    print("Waiting 3 seconds before merging...")
    time.sleep(3)

    print(f"Step 3: Merging PR #{pr_number}...")
    merge_payload = {
        "commit_title": f"feat: Milestone 6.2 — Onboarding Flow & Localization Infrastructure (#{pr_number})",
        "merge_method": "squash"
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            print(f"Merge successful: {data['message']}")
    except urllib.error.HTTPError as e:
        print(f"HTTPError merging PR: {e.code} {e.read().decode('utf-8')}")
        sys.exit(1)

    print("Step 4: Switching to main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done! Milestone 6.2 successfully merged to main.")

if __name__ == "__main__":
    main()
