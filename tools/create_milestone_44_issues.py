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

token = get_token()
repo = get_repo()

if not token:
    print("Error: Could not retrieve GitHub token.")
    sys.exit(1)

headers = {
    "Authorization": f"Bearer {token}",
    "Accept": "application/vnd.github+json",
    "X-GitHub-Api-Version": "2022-11-28"
}

issues = [
    {
        "title": "#P4-008: Social Activities — Go Out Events with Fatigue/Morale Trade-Offs & Outing Catalog",
        "body": """## User Story (#P4-008)
> As a footballer, I want to participate in various social outings and night life activities (e.g., Casual Restaurant, VIP Nightclub, Team Bowling, Charity Gala, Concert) that consume energy and money in exchange for morale boosts, relational bonding, and public fame, so that my lifestyle choices have genuine physical and emotional consequences on matchday readiness.

### Scope & Architecture
- **Pure C# Domain Layer (`FootballLife.Domain`)**:
  - `SocialActivityCategory` enum: `Casual`, `Nightlife`, `TeamBonding`, `LuxuryGlamour`, `Charity`.
  - `SocialActivity` record: `Id`, `Name`, `Category`, `EnergyCost`, `FinancialCost`, `MoraleBoost`, `PrestigeBoost`, `TeamAffinityBoost`, `ManagerTrustRisk`, `Description`, `IconEmoji`.
  - `SocialActivityCatalog`: Curated list of distinct activities with progressive prestige/cost tiers.
- **Pure C# Simulation Layer (`FootballLife.Simulation`)**:
  - `SocialActivitySystem`:
    - `CanAffordActivity(save, activity)`
    - `ExecuteActivity(save, activity, date)` returning `SocialActivityResult` (mutates bank balance, energy/fatigue, morale, team affinity, with random risk of manager disapproval if late before matchday).
- **Automated Unit Tests**:
  - Comprehensive unit tests in `SocialActivitySystemTests.cs` verifying costs, state deltas, minimum balance checks, and risk calculation.
- **Unity Presentation Layer (`Unity/UI`)**:
  - Social Outings modal (`SocialActivitiesView.uxml` and `SocialActivitiesController.cs`).
  - Integrated into both `HomeHUDView` (Lounge hotspot / dock) and `CareerHubView`.
""",
        "labels": ["phase-4", "simulation", "ui"]
    },
    {
        "title": "#P4-009: Dynamic Media & Press Conference System — Match Reports, Transfer Rumors & Interview Press Conferences",
        "body": """## User Story (#P4-009)
> As a footballer, I want to face journalists in post-match and pre-match press conferences with branching response choices, and read dynamic media match reports and transfer rumors with LLM/procedural narrative flavor, so that my public image, manager trust, fan reputation, and team chemistry react to what I say.

### Scope & Architecture
- **Pure C# Domain Layer (`FootballLife.Domain`)**:
  - `PressConference`: Pre-match / post-match / transfer rumor interview context.
  - `PressQuestion`: Journalist identity, publication (e.g. *The Daily Pitch*, *Sky Sports Weekly*), tone (`Aggressive`, `Supportive`, `Provocative`), question text, and list of responses.
  - `PressResponseChoice`: Response text (e.g. `Humble`, `Confident`, `Defiant`, `Diplomatic`), stat impact deltas (`ManagerTrust`, `FanPopularity`, `TeammateMorale`, `MediaReputation`).
  - `MediaArticle`: Rich news article model with headline, body, author, timestamp, category badge, and thumbnail.
- **Pure C# Simulation Layer (`FootballLife.Simulation`)**:
  - `PressConferenceSystem`: Generates contextual press conferences based on recent match performance, transfer rumors, or milestone events.
  - `MediaFeedSystem`: Procedurally generates detailed match reports, transfer gossip, and editorial commentary with deterministic fallback templates.
  - LLM Integration Hook: Optional dynamic flavor commentary via `LLMCommentaryService` following `llm-game-integration` guidelines with zero-latency offline fallbacks.
- **Automated Unit Tests**:
  - Comprehensive unit tests in `PressConferenceSystemTests.cs` and `MediaFeedSystemTests.cs`.
- **Unity Presentation Layer (`Unity/UI`)**:
  - `PressConferenceView.uxml` and `PressConferenceController.cs`: Press room backdrop, reporter microphone, journalist badges, typewriter question prompt, and response choice cards with impact previews.
  - `MediaFeedView.uxml` / updated Phone OS News tab: Tabbed sports media hub with headline stories, interview recaps, and rumor mills.
""",
        "labels": ["phase-4", "simulation", "ui", "llm"]
    }
]

for item in issues:
    data = json.dumps({
        "title": item["title"],
        "body": item["body"],
        "labels": item["labels"]
    }).encode("utf-8")
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues",
        data=data,
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            res = json.loads(resp.read().decode("utf-8"))
            print(f"Created Issue #{res['number']}: {res['title']} -> {res['html_url']}")
    except urllib.error.HTTPError as e:
        print(f"HTTPError {e.code}: {e.read().decode('utf-8')}")

