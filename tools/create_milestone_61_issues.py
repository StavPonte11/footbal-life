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
        proc = subprocess.run(["git", "credential", "fill"], input="protocol=https\nhost=github.com\n", text=True, capture_output=True, check=True)
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
            "title": "#P6-001: 10,000-Career Simulation Balance Pass",
            "body": """## User Story #P6-001: 10,000-Career Simulation Balance Pass

> As a game designer and player, I want the multi-season career simulation to be balanced across 10,000 simulated careers so that progression curves, retirement ages, transfer frequencies, financial earnings, and legacy outcomes mirror authentic professional football without runaway feedback loops or economic distortions.

### Acceptance Criteria:
- [ ] Incorporate Phase 5 systems into `CareerSimulationEngine`:
  - `RetirementSystem`: Age 32+ physical decline curve and retirement evaluation.
  - `SponsorshipSystem`: Commercial deals and earnings based on player reputation.
  - `LegacySystem`: Lifetime career scoring, legacy grades, and Hall of Fame eligibility.
- [ ] Update `CareerStatistics`, `AggregateReport`, and CLI output to track:
  - Legacy Grade distributions (GOAT, Legend, Icon, Cult Hero, Journeyman, Underachiever).
  - Hall of Fame induction rate.
  - Commercial earnings vs wage earnings.
- [ ] Run full 10,000-career simulation pass with `--careers 10000 --parallel`.
- [ ] Validate distribution targets:
  - Peak Overall: Mean between 66-72, Elite 85+ achieved by 2-5% of players, Max <= 94.
  - Career Length: Mean 15-18 seasons, Retirement age ~33-36.
  - Bankruptcy Rate: < 1.0%.
  - Legacy Grades: GOAT (<1%), Legend (2-5%), Icon (10-15%), Cult Hero (20-30%), Journeyman (40-50%), Underachiever (5-15%).
  - Hall of Fame Induction Rate: 3-7%.
- [ ] Automated balance unit tests in `FootballLife.Simulation.Tests`.
""",
            "labels": ["phase-6", "simulation", "balance", "complexity:L"]
        },
        {
            "title": "#P6-004: Content Expansion — 100+ Life Events, 50+ Clubs, 10+ Leagues",
            "body": """## User Story #P6-004: Content Expansion — 100+ Life Events, 50+ Clubs, 10+ Leagues

> As a footballer, I want rich, deep, and varied static content across world football so that every career feels distinct with authentic clubs across multiple European divisions, diverse leagues, and over 100 emergent life events and moral choices.

### Acceptance Criteria:
- [ ] Expand `content/data/leagues.json` to 10+ leagues across England, Spain, Germany, Italy, and France across Tier 1 and Tier 2.
- [ ] Expand `content/data/clubs.json` to 50+ clubs with diverse reputations, facility ratings, budgets, tactical identities, and stadiums.
- [ ] Expand `content/data/events.json` to 100+ unique, narrative-rich life events across all categories (Media, Locker Room, Commercial, Personal, Training, Fan Culture, Dilemmas, Late Career).
- [ ] Verify all content adheres to schemas in `content/data/schema/` with zero missing references or invalid fields.
- [ ] Content validation unit tests verifying 100+ events, 50+ clubs, and 10+ leagues load cleanly through `ContentDataLoader`.
""",
            "labels": ["phase-6", "content", "data", "complexity:L"]
        }
    ]

    created = {}
    for issue in issues_to_create:
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(issue).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created[issue["title"]] = num
                print(f"Created Issue #{num}: {issue['title']}")
        except urllib.error.HTTPError as e:
            print(f"Error creating issue: {e.code} - {e.read().decode('utf-8')}")

    with open("tools/milestone_61_issues.json", "w") as f:
        json.dump(created, f, indent=2)
    print("Saved tools/milestone_61_issues.json")

if __name__ == "__main__":
    main()
