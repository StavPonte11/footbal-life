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
        proc = subprocess.run(["gh", "auth", "token"], text=True, capture_output=True, check=True)
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
        proc = subprocess.run(["git", "remote", "get-url", "origin"], text=True, capture_output=True, check=True)
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
            "key": "#P5-001",
            "title": "#P5-001: World simulation — NPC player development, aging/decline, AI squad replenishment & league progression",
            "body": """### User Story
As a footballer, I want other clubs and NPC players across the league ecosystem to age, develop, decline, and retire dynamically, and AI clubs to actively replenish their squads, so that the football universe evolves as a living world rather than remaining static.

### Acceptance Criteria
- [ ] Pure C# simulation in `WorldSimulationSystem.cs` implementing:
  - Youth development curves (<21 prospects develop rapidly based on potential).
  - Peak maintenance (24-29 maintain high performance).
  - Physical decline for veterans (31+ experience pace/stamina decay).
  - Natural retirement at 35+ or severe physical decline.
- [ ] AI Club squad replenishment ensuring clubs maintain balanced squads with starting XI and bench depth (youth promotions or free agent signings).
- [ ] Deterministic league progression and end-of-season table resolution based on squad quality and `SimulationRandom`.
- [ ] Zero GC allocations in hot simulation paths.
- [ ] Automated unit test suite verifying NPC aging, squad replenishment, and multi-season stability.
""",
            "labels": ["phase-5", "simulation", "milestone-5.1"]
        },
        {
            "key": "#P5-002",
            "title": "#P5-002: Transfer window — multi-club bidding, player-initiated transfer requests & Transfer Market UI",
            "body": """### User Story
As a footballer, I want an interactive transfer market during transfer windows where multiple interested clubs submit competitive bids (wage, signing bonus, squad role promise), and I can formally request a transfer if unhappy, so that my career trajectory has dynamic agency.

### Acceptance Criteria
- [ ] Pure C# domain model `TransferBiddingWar` and `TransferListing` representing concurrent club offers, wage bids, signing bonuses, and squad role promises.
- [ ] Pure C# simulation logic in `TransferMarketSystem.cs` supporting:
  - Multi-club competitive bidding wars during summer (Week 1) and winter (Week 20) transfer windows.
  - Player-initiated transfer requests (`RequestTransferList()`) with manager trust risk evaluation.
  - AI-to-AI transfer circulation across clubs.
- [ ] Modern UI Toolkit interface (`TransferMarketView.uxml` and `TransferMarketController.cs`):
  - Offers comparison cards (crest, tier badge, weekly salary, bonus, squad role).
  - Action buttons (`ACCEPT OFFER`, `REJECT OFFER`, `REQUEST TRANSFER`).
  - League pyramid overview tab showing current league standings and promotion/relegation zones.
- [ ] Integrated into `CareerHub` scene modal workflow and `SimulationBridge`.
- [ ] Comprehensive automated unit tests.
""",
            "labels": ["phase-5", "simulation", "ui", "milestone-5.1"]
        },
        {
            "key": "#P5-003",
            "title": "#P5-003: Multi-tier league ecosystem — division prestige, wage scaling, promotion & relegation",
            "body": """### User Story
As a footballer, I want a hierarchical multi-tier league system with distinct prestige levels, realistic wage bands, and annual promotion/relegation between divisions, so that climbing from lower tiers to the top flight feels momentous.

### Acceptance Criteria
- [ ] Pure C# Domain models (`LeagueTierConfig.cs`, `LeagueSeasonResolution.cs`) defining:
  - Tiers: Tier 1 (Premier), Tier 2 (Championship), Tier 3 (League One), Tier 4 (League Two).
  - Division prestige ratings (85+ for top flight down to 40 for bottom tier).
  - Wage scaling bands (min, average, max per tier).
  - Promotion and relegation quota rules (e.g., top 3 promoted, bottom 3 relegated).
- [ ] End-of-season resolution swapping clubs between league tiers in `WorldState` deterministically.
- [ ] Player contract/wage validation ensuring offers strictly respect tier financial realities.
- [ ] Unit tests verifying multi-tier promotion/relegation transitions and financial integrity.
""",
            "labels": ["phase-5", "domain", "simulation", "milestone-5.1"]
        }
    ]

    print(f"Creating {len(issues_to_create)} issues in {repo}...")
    for item in issues_to_create:
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
                res = json.loads(resp.read().decode("utf-8"))
                print(f"Created {item['key']}: #{res['number']} - {res['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"Error creating {item['key']}: {e.code} - {e.read().decode('utf-8')}")

if __name__ == "__main__":
    main()
