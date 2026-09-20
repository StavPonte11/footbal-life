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

# Fetch or create milestone 1.9
milestone_url = f"https://api.github.com/repos/{repo}/milestones?state=all"
req = urllib.request.Request(milestone_url, headers=headers)
milestone_num = None
try:
    with urllib.request.urlopen(req) as resp:
        milestones = json.loads(resp.read().decode('utf-8'))
        for m in milestones:
            if "1.9" in m["title"]:
                milestone_num = m["number"]
                print(f"Found milestone 1.9: #{milestone_num} ({m['title']})")
                break
except Exception as e:
    print(f"Error fetching milestones: {e}")

if not milestone_num:
    create_ms_url = f"https://api.github.com/repos/{repo}/milestones"
    ms_payload = json.dumps({
        "title": "Milestone 1.9 — Basic Relationships",
        "state": "open",
        "description": "Basic Relationships System: Relationship domain model, affinity decay from neglect, interaction events, transfer impact, and unit/integration tests."
    }).encode('utf-8')
    req = urllib.request.Request(create_ms_url, data=ms_payload, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            ms_obj = json.loads(resp.read().decode('utf-8'))
            milestone_num = ms_obj["number"]
            print(f"Created milestone 1.9: #{milestone_num}")
    except Exception as e:
        print(f"Could not create milestone: {e}")

issues_to_create = [
    {
        "title": "[P1-055] Relationship Domain Model",
        "body": """### User Story
> As a life simulation, I want relationship entities with emotional context so that the game presents people as people rather than numeric bars.

### Acceptance Criteria
- [ ] `Relationship` record in `FootballLife.Domain`: `Guid Id`, `string Name`, `RelationshipType Type`, `float Affinity` [0–100], `float Trust` [0–100], `DateOnly LastInteraction`, `IReadOnlyList<string> SharedHistory`
- [ ] `RelationshipType` enum: `Partner`, `Parent`, `Sibling`, `Friend`, `Teammate`, `Manager`, `Agent`
- [ ] Factory method `Relationship.Create(...)` and immutable `WithInteraction` / `WithDecay` transitions
- [ ] Invariants: Affinity and Trust strictly clamped to [0, 100]
- [ ] `WorldState` integration with `Relationships` dictionary and lookup methods
- [ ] Unit test: `Relationship_Initialization_ClampsAffinityAndTrustWithinBounds()`

**Branch:** `feature/p1-055-relationship-model`
**Layer:** `Domain` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.9", "domain", "complexity:M"]
    },
    {
        "title": "[P1-056] RelationshipSystem — Affinity Decay from Neglect",
        "body": """### User Story
> As a footballer, I want neglected personal relationships to gradually lose affinity over time so that maintaining relationships requires active attention and time investment.

### Acceptance Criteria
- [ ] `RelationshipSystem.ApplyWeeklyDecay(Relationship rel, DateOnly currentDate)` -> `Relationship`
- [ ] Default decay rate: 0.5 affinity points per week without interaction
- [ ] Family minimum floor: Family relationships (`Parent`, `Sibling`) never decay below 20.0
- [ ] Non-family relationships can decay to 0.0
- [ ] Unit test: `Relationship_Affinity_DecaysFromNeglect()`
- [ ] Unit test: `Relationship_FamilyAffinity_HasMinimumFloor()`

**Branch:** `feature/p1-056-affinity-decay`
**Layer:** `Simulation` | **Complexity:** `S`""",
        "labels": ["phase-1", "milestone:1.9", "simulation", "complexity:S"]
    },
    {
        "title": "[P1-057] RelationshipSystem — Interaction Events Affecting Affinity",
        "body": """### User Story
> As a footballer, I want social interactions, shared moments, and dialogue choices to adjust affinity and trust so that personal dynamics evolve meaningfully.

### Acceptance Criteria
- [ ] `RelationshipSystem.RecordInteraction(Relationship rel, DateOnly date, float affinityDelta, float trustDelta, string context)` -> `Relationship`
- [ ] Updates `LastInteraction` date to interaction date
- [ ] Clamps `Affinity` and `Trust` within [0, 100]
- [ ] Appends formatted summary entry into `SharedHistory`
- [ ] Unit test: `RelationshipSystem_PositiveInteraction_IncreasesAffinityAndAppendsHistory()`
- [ ] Unit test: `RelationshipSystem_NegativeInteraction_ReducesTrust()`

**Branch:** `feature/p1-057-relationship-interactions`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.9", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-058] RelationshipSystem — Club Transfer Impact on Relationships",
        "body": """### User Story
> As a footballer, I want club transfers to affect my relationships with teammates and managers so that changing clubs creates realistic social distance and new locker room dynamics.

### Acceptance Criteria
- [ ] `RelationshipSystem.ApplyClubTransfer(IReadOnlyList<Relationship> relationships, Guid previousClubId, Guid newClubId, DateOnly transferDate)` -> `IReadOnlyList<Relationship>`
- [ ] Former teammates transition: affinity slight decay (-5) and logged in `SharedHistory` ("Transferred to new club")
- [ ] Former manager relationship archived / trust decay
- [ ] Partners and family relationships remain intact with transfer notes
- [ ] Unit test: `RelationshipSystem_ClubTransfer_ImpactsFormerTeammatesAndManager()`

**Branch:** `feature/p1-058-transfer-impact`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.9", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-059] Unit & Integration Tests — Relationship Dynamics",
        "body": """### User Story
> Comprehensive tests validating relationship lifecycle, decay floors, multi-season dynamics, and interaction history tracking.

### Acceptance Criteria
- [ ] Test: `RelationshipSystem_MultiSeasonDecay_ReachesFloorForFamily()`
- [ ] Test: `RelationshipSystem_ActiveInteractions_PreserveAffinityAbove90()`
- [ ] Test: `RelationshipSystem_ZeroAllocations_InSimulationTick()`

**Branch:** `feature/p1-059-relationship-tests`
**Layer:** `Tests` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.9", "testing", "complexity:M"]
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
