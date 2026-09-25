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
            "key": "#P5-007",
            "title": "#P5-007: Sponsorship system — reputation-gated endorsement deals & commercial perks",
            "body": """### User Story
As a footballer gaining fame and reputation, I want to sign commercial endorsement deals and sponsorships (boot deals, brand ambassador roles, luxury endorsements) that provide recurring commercial income and unique perks, balanced by commitments and reputation prerequisites.

### Acceptance Criteria
- [ ] Pure C# Domain models for endorsements (`SponsorshipDeal.cs`, `SponsorshipTier`, `SponsorshipPerk`, `ActiveSponsorship`).
- [ ] Pure C# Simulation system `SponsorshipSystem.cs` providing:
  - Reputation-gated offer generation (Local Business, Regional Brand, National Brand, Global Mega-Brand).
  - Slot limits (e.g. maximum concurrent sponsorships based on lifestyle/fame).
  - Weekly commercial payout directly integrated with player `FinancialAccount`.
  - Perks (e.g. training energy recovery bonus, boot equipment stats boost, fame multiplier).
  - Termination / expiry conditions when player reputation or form drops below threshold.
- [ ] UI Toolkit integration in Lifestyle / Finances hub allowing players to browse, sign, and manage active sponsorships.
- [ ] Automated unit test suite verifying offer generation, payout integration, and perk application.
""",
            "labels": ["phase-5", "domain", "simulation", "ui", "milestone-5.3"]
        },
        {
            "key": "#P5-008",
            "title": "#P5-008: Retirement arc — late-career physical decline, contract wind-downs & retirement choice",
            "body": """### User Story
As a veteran footballer entering my mid-to-late 30s, I want realistic late-career dynamics including physical attribute decline, shorter contract terms, transitioning squad roles, and the agency to decide when to hang up my boots on my own terms.

### Acceptance Criteria
- [ ] Pure C# Domain models for career retirement (`RetirementDecision.cs`, `RetirementReason`, `PostPlayingRole`).
- [ ] Pure C# Simulation system `RetirementSystem.cs` implementing:
  - Age-scaled physical attribute decay for players age 32+ (pace and stamina decline while tactical/mental attributes stay resilient).
  - Contract wind-down behavior (clubs offer shorter 1-year contracts, squad status shifts to rotation/mentor).
  - Voluntary retirement choices at season end or upon major injury / no contract offers.
  - Selection of post-playing career path (Manager, Pundit, Ambassador, Academy Coach).
- [ ] Persistence of player retired state in `CareerSaveData.cs`.
- [ ] Automated unit test suite verifying late-career decay, voluntary retirement, and forced contract expiration retirement.
""",
            "labels": ["phase-5", "simulation", "milestone-5.3"]
        },
        {
            "key": "#P5-009",
            "title": "#P5-009: Legacy system — hall of fame, career score grade & post-retirement summary",
            "body": """### User Story
As a retired player, I want a comprehensive retrospective evaluating my entire career, calculating a career legacy grade (from Journeyman to Legend to GOAT), inducting me into the Hall of Fame, and memorializing my trophies, records, and lifetime earnings.

### Acceptance Criteria
- [ ] Pure C# Domain models for career legacy (`CareerLegacy.cs`, `LegacyGrade`, `HallOfFameEntry`, `CareerMilestoneRecord`).
- [ ] Pure C# Simulation logic in `LegacySystem.cs` calculating:
  - Lifetime score based on matches, goals, assists, clean sheets, trophies won (League titles, Champions Cup, Domestic cups, International tournaments), and individual accolades.
  - Legacy Grade categorization (`GOAT`, `Legend`, `Icon`, `CultHero`, `Journeyman`, `Underachiever`).
  - Hall of Fame eligibility determination.
- [ ] UI Toolkit Presentation screen (`LegacyView.uxml`, `LegacyViewController.cs`) showing full career retrospective plaque, trophy cabinet, lifetime stats, career grade, and post-career path.
- [ ] Automated unit test suite verifying score calculations, tier grading, and Hall of Fame criteria.
""",
            "labels": ["phase-5", "domain", "simulation", "ui", "milestone-5.3"]
        }
    ]

    print(f"Creating {len(issues_to_create)} issues in {repo}...")
    created_issues = {}
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
                created_issues[item["key"]] = res["number"]
                print(f"Created {item['key']}: #{res['number']} - {res['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"Error creating {item['key']}: {e.code} - {e.read().decode('utf-8')}")

    with open("tools/milestone_53_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

if __name__ == "__main__":
    main()
