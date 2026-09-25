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
            "key": "#P5-004",
            "title": "#P5-004: National team system — eligibility, call-ups, international tournaments & career caps",
            "body": """### User Story
As a footballer, I want to earn call-ups for my national team based on my form, rating, and positional need, compete in international fixtures, accumulate caps and goals, and experience the physical fatigue and prestige rewards of representing my country.

### Acceptance Criteria
- [ ] Pure C# Domain models for international football (`NationalTeam.cs`, `InternationalCallUp.cs`, `InternationalCareer.cs`, `InternationalFixture.cs`, `InternationalTier`).
- [ ] Pure C# Simulation system `InternationalSystem.cs` implementing nationality eligibility matching, national squad selection by rating/form with positional quotas (3 GK, 7 DEF, 7 MID, 6 FWD), dynamic call-up invitations, and international match simulation with caps, goals, and assists.
- [ ] Physical and psychological feedback: fatigue increase (+18 to +25), confidence boost on victory (+6 to +10), and career reputation gain (+3 to +6).
- [ ] Persistence in `CareerSaveData.cs` tracking `InternationalCaps`, `InternationalGoals`, `InternationalAssists`, and `IsRetiredFromInternational`.
- [ ] UI Toolkit presentation in `ProfileView.uxml` with an International Football card displaying national team badge, caps, goals, assists, active call-up status, and action button.
- [ ] Automated unit test suite `InternationalSystemTests.cs` (7 tests) passing with 0 failures.
""",
            "labels": ["phase-5", "simulation", "ui", "milestone-5.2"]
        },
        {
            "key": "#P5-005",
            "title": "#P5-005: Continental competitions — Champions Cup, qualification, group stage & knockouts",
            "body": """### User Story
As an elite club footballer, I want my club to qualify for the Champions Cup continental tournament based on league finish, compete in a 32-team group stage and 2-legged knockout bracket, and win prestigious continental glory and massive prize money.

### Acceptance Criteria
- [ ] Pure C# Domain models for continental tournaments (`ContinentalCompetition.cs`, `ContinentalFixture.cs`, `ContinentalGroupStanding.cs`, `ContinentalStage`).
- [ ] Pure C# Simulation system `ContinentalCompetitionSystem.cs` managing 32-club tournament setup with 8 groups of 4 (with same-league avoidance where possible), home/away round-robin group stage resolution (6 matchdays), and 2-legged knockout bracket (RO16, Quarter-Finals, Semi-Finals, and Final).
- [ ] Knockout aggregate resolution with away goals / extra time and penalty shootouts if aggregate scores are tied.
- [ ] Financial rewards: £50,000,000 champion prize money distributed to the winning club's budget and prize money for runner-up (£30M) and semi-finalists (£15M).
- [ ] Full UI Toolkit screen `ContinentalView.uxml` and controller `ContinentalViewController.cs` presenting Groups A-H standings, knockout bracket tree, player's club status, and interactive match simulation button.
- [ ] Integrated into `CareerHubView.uxml` with quick header button (`btn-quick-continental`), action grid button (`btn-continental`), and overlay coordinator handling in `CareerHubCoordinator.cs`.
- [ ] Automated unit test suite `ContinentalCompetitionSystemTests.cs` (5 tests) passing with 0 failures.
""",
            "labels": ["phase-5", "domain", "simulation", "ui", "milestone-5.2"]
        },
        {
            "key": "#P5-006",
            "title": "#P5-006: Manager change system — manager sacking, hiring & tactical trust reset",
            "body": """### User Story
As a footballer, I want clubs to hold managers accountable with sackings when performance falls below expectations, appoint new managers with distinct tactical identities, and reset my manager trust so I must prove myself again to the new boss.

### Acceptance Criteria
- [ ] Pure C# Domain models for managerial appointments and sackings (`ManagerChangeEvent.cs`, `ManagerReason`).
- [ ] Pure C# Simulation system `ManagerChangeSystem.cs` evaluating seasonal performance against board expectations and patience, calculating sacking probability, and appointing a new manager from tier-appropriate names, tactical identities (Attacking, Possessional, Counter, Direct, Defensive), and trust tolerances.
- [ ] Dynamic player impact: player manager trust resets to neutral (50.0) upon a managerial change, requiring the player to earn starting status through training and performances.
- [ ] Real-time event propagation via `SimulationBridge.OnManagerChanged` updating player identity, hub header, and manager trust indicators.
- [ ] Manager change history logged in `WorldState.ManagerChangeHistory`.
- [ ] Automated unit test suite `ManagerChangeSystemTests.cs` (6 tests) passing with 0 failures.
""",
            "labels": ["phase-5", "simulation", "milestone-5.2"]
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

    with open("tools/milestone_52_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

if __name__ == "__main__":
    main()
