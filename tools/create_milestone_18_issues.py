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
    print("Error: Could not obtain GitHub token.", file=sys.stderr)
    sys.exit(1)

print(f"Target Repository: {repo}")

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

# Fetch milestones
milestone_url = f"https://api.github.com/repos/{repo}/milestones?state=all"
req = urllib.request.Request(milestone_url, headers=headers)
milestone_num = None
try:
    with urllib.request.urlopen(req) as resp:
        milestones = json.loads(resp.read().decode('utf-8'))
        for m in milestones:
            if "1.8" in m["title"]:
                milestone_num = m["number"]
                print(f"Found milestone 1.8: #{milestone_num} ({m['title']})")
                break
except Exception as e:
    print(f"Error fetching milestones: {e}")

if not milestone_num:
    create_ms_url = f"https://api.github.com/repos/{repo}/milestones"
    ms_payload = json.dumps({
        "title": "Milestone 1.8 — Life Events System (v1)",
        "state": "open",
        "description": "Core Life Events System: domain models, condition evaluation, weighted selection, effect application, 20 seed events, and full test suite."
    }).encode('utf-8')
    req = urllib.request.Request(create_ms_url, data=ms_payload, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            ms_obj = json.loads(resp.read().decode('utf-8'))
            milestone_num = ms_obj["number"]
            print(f"Created milestone 1.8: #{milestone_num}")
    except Exception as e:
        print(f"Could not create milestone: {e}")

issues_to_create = [
    {
        "title": "[P1-049] LifeEvent Domain Model",
        "body": """### User Story
> As the narrative engine, I want a `LifeEvent` domain model encoding conditions, choices, and consequences so that personal life dilemmas are data-driven and systemic.

### Acceptance Criteria
- [ ] `LifeEvent` record in `FootballLife.Domain`: `string Id`, `string Title`, `string Description`, `EventCategory Category`, `IReadOnlyList<EventChoice> Choices`, `EventPreconditions Conditions`, `int Weight`, `int CooldownWeeks`
- [ ] `EventChoice` record: `string Id`, `string Text`, `IReadOnlyList<EventEffect> Effects`
- [ ] `EventEffect` record: `EffectTarget Target`, `float Delta`, `string Description`
- [ ] `EventCategory` enum: `Lifestyle`, `Media`, `LockerRoom`, `Family`, `Commercial`, `Training`
- [ ] `EffectTarget` enum: `Happiness`, `Fatigue`, `Confidence`, `Morale`, `ManagerTrust`, `Money`
- [ ] `EventPreconditions` record: min/max age, min/max fatigue, min/max salary, min/max trust
- [ ] Unit test: `LifeEvent_Serialization_RoundTripsFromJson()`

**Branch:** `feature/p1-049-life-event-model`
**Layer:** `Domain` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.8", "domain", "complexity:M"]
    },
    {
        "title": "[P1-050] LifeEventSystem — Condition Evaluator",
        "body": """### User Story
> As the life simulation, I want conditions evaluated against current player state and career context so that only plausible events trigger.

### Acceptance Criteria
- [ ] `LifeEventSystem.EvaluateConditions(LifeEvent ev, Player player, PlayerState state, PlayerCareerState career, FinanceAccount finance, WorldState world)` -> `bool`
- [ ] Checks age min/max, fatigue min/max, salary bounds, manager trust bounds, and cooldown tracking
- [ ] Unit test: `LifeEventSystem_HighSalaryEvent_DoesNotTriggerForYouthPlayer()`
- [ ] Unit test: `LifeEventSystem_ExhaustedEvent_TriggersOnlyWhenFatigued()`

**Branch:** `feature/p1-050-event-conditions`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.8", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-051] LifeEventSystem — Weighted Selection",
        "body": """### User Story
> As the life simulation, I want probabilistic event selection weighted by current player situation so that narrative pacing is dynamic yet predictable with seed.

### Acceptance Criteria
- [ ] `LifeEventSystem.SelectWeeklyEvent(IReadOnlyList<LifeEvent> pool, Player player, WorldState world, SimulationRandom rng)` -> `LifeEvent?`
- [ ] Evaluates cooldowns from `WorldState`, filters eligible events, rolls weighted random via `SimulationRandom`
- [ ] Determinism test: same seed -> identical chosen event sequence
- [ ] Returns null if no eligible events or if empty roll

**Branch:** `feature/p1-051-event-selection`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.8", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-052] LifeEventSystem — Effect Applicator",
        "body": """### User Story
> As the simulation, I want chosen event effects applied to mutate player state, relationships, or finances deterministically.

### Acceptance Criteria
- [ ] `LifeEventSystem.ApplyChoice(LifeEvent ev, EventChoice choice, Player player, WorldState world, DateOnly date)` -> `WorldState`
- [ ] Mutates relevant state: `Fatigue`, `Happiness`, `Confidence`, `Morale`, `ManagerTrust`, `FinanceAccount`
- [ ] Records event cooldown into `WorldState`
- [ ] Unit test: `LifeEventSystem_ChoiceReducesFatigue_AndIncreasesHappiness()`
- [ ] Unit test: `LifeEventSystem_FinancialEffect_CreditsOrDebitsFinanceAccount()`

**Branch:** `feature/p1-052-event-effects`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.8", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-053] Seed Data (events.json)",
        "body": """### User Story
> Canonical data file containing 20 core life events covering press dilemmas, sponsor offers, social nights out, family requests, and training controversies.

### Acceptance Criteria
- [ ] `content/data/events.json` with 20 diverse, balanced life events across all categories
- [ ] `LifeEventDataLoader.Load()` parsing and schema validation
- [ ] Schema validation tests verify all event IDs, choices, and effects

**Branch:** `feature/p1-053-seed-events`
**Layer:** `Data` | **Complexity:** `L`""",
        "labels": ["phase-1", "milestone:1.8", "data", "complexity:L"]
    },
    {
        "title": "[P1-054] Unit & Integration Tests — Life Events System",
        "body": """### User Story
> Comprehensive tests ensuring life events fire reliably, observe cooldowns, and mutate state without corrupting invariants.

### Acceptance Criteria
- [ ] Test: `LifeEventSystem_CooldownObserved_CannotFireSameEventConsecutively()`
- [ ] Test: `LifeEventSystem_StatisticalSpread_Over1000Weeks()`
- [ ] Integration with `FootballLife.CareerSimulator` interactive loop

**Branch:** `feature/p1-054-life-events-tests`
**Layer:** `Tests` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.8", "testing", "complexity:M"]
    }
]

created_issues = []
for item in issues_to_create:
    payload = {
        "title": item["title"],
        "body": item["body"],
        "labels": item["labels"]
    }
    if milestone_num:
        payload["milestone"] = milestone_num
    
    url = f"https://api.github.com/repos/{repo}/issues"
    data = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(url, data=data, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            res = json.loads(resp.read().decode('utf-8'))
            print(f"Created Issue #{res['number']}: {res['title']}")
            created_issues.append((res['number'], res['title']))
    except urllib.error.HTTPError as e:
        err_body = e.read().decode('utf-8')
        print(f"HTTP Error {e.code} creating issue '{item['title']}': {err_body}")
    except Exception as ex:
        print(f"Error creating issue '{item['title']}': {ex}")
    time.sleep(1)

print("\nSummary of created issues:")
for num, title in created_issues:
    print(f"- #{num}: {title}")
