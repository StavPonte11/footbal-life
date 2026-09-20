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

# Fetch or create milestone 1.10
milestone_url = f"https://api.github.com/repos/{repo}/milestones?state=all"
req = urllib.request.Request(milestone_url, headers=headers)
milestone_num = None
try:
    with urllib.request.urlopen(req) as resp:
        milestones = json.loads(resp.read().decode('utf-8'))
        for m in milestones:
            if "1.10" in m["title"]:
                milestone_num = m["number"]
                print(f"Found milestone 1.10: #{milestone_num} ({m['title']})")
                break
except Exception as e:
    print(f"Error fetching milestones: {e}")

if not milestone_num:
    create_ms_url = f"https://api.github.com/repos/{repo}/milestones"
    ms_payload = json.dumps({
        "title": "Milestone 1.10 — Transfer & Contract System",
        "state": "open",
        "description": "Transfer & Contract System: TransferOffer domain model, transfer offer generation, player acceptance flow, contract negotiation, contract renewal, and unit/integration tests."
    }).encode('utf-8')
    req = urllib.request.Request(create_ms_url, data=ms_payload, headers=headers, method="POST")
    try:
        with urllib.request.urlopen(req) as resp:
            ms_obj = json.loads(resp.read().decode('utf-8'))
            milestone_num = ms_obj["number"]
            print(f"Created milestone 1.10: #{milestone_num}")
    except Exception as e:
        print(f"Could not create milestone: {e}")

issues_to_create = [
    {
        "title": "[P1-060] TransferOffer Domain Model",
        "body": """### User Story
> As a simulation system, I want a structured `TransferOffer` domain model representing official transfer and contractual proposals between clubs and players so that career movements are legally and financially well-defined.

### Acceptance Criteria
- [ ] `TransferOffer` immutable record in `FootballLife.Domain`:
  - `Guid Id`
  - `Guid PlayerId`
  - `Guid OfferingClubId`
  - `SquadRole OfferedRole`
  - `decimal OfferedWage` (> 0)
  - `decimal TransferFee` (>= 0)
  - `int ContractYears` (1–5)
  - `decimal ReleaseClause` (>= 0)
  - `decimal SigningBonus` (>= 0)
  - `DateOnly OfferDate`
  - `DateOnly ExpiryDate` (>= OfferDate)
  - `TransferOfferStatus Status` (`Pending`, `Accepted`, `Rejected`, `Expired`, `Withdrawn`)
- [ ] Factory method `TransferOffer.Create(...)` and transition `WithStatus(TransferOfferStatus status)`
- [ ] Invariant validation: strict parameter checks, non-negative financials, positive wage, valid durations
- [ ] Unit tests for `TransferOffer` construction, clamping, and status transitions

**Branch:** `feature/p1-060-transfer-offer-model`
**Layer:** `Domain` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.10", "domain", "complexity:M"]
    },
    {
        "title": "[P1-061] TransferSystem — Offer Generation",
        "body": """### User Story
> As a footballer, I want transfer offers to appear when my reputation, form, and position need align with a club's requirements so that transfer opportunities feel earned and believable.

### Acceptance Criteria
- [ ] `TransferSystem.GenerateOffers(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Contract currentContract, WorldState world, SimulationRandom rng, DateOnly currentDate)` -> `IReadOnlyList<TransferOffer>`
- [ ] Clubs filtered by `LeagueTier` relative to player reputation and overall ability
- [ ] Current club excluded from prospective transfer suitors
- [ ] Offered wage calculated based on club tier, player reputation, and squad role
- [ ] Minimum 1 escape offer generated during transfer windows if `ManagerTrust < 30`
- [ ] Maximum active offers capped (up to 3 simultaneous offers)
- [ ] Deterministic offer generation with identical `SimulationRandom` seeds
- [ ] Unit tests for high reputation interest, escape offers under low trust, and determinism

**Branch:** `feature/p1-061-transfer-offer-generation`
**Layer:** `Simulation` | **Complexity:** `L`""",
        "labels": ["phase-1", "milestone:1.10", "simulation", "complexity:L"]
    },
    {
        "title": "[P1-062] TransferSystem — Acceptance & Rejection Flow",
        "body": """### User Story
> As a player, I want to accept or reject transfer offers, seamlessly moving to the new club, registering my new contract, receiving my signing bonus, and adjusting my relationships.

### Acceptance Criteria
- [ ] `TransferSystem.AcceptOffer(TransferOffer offer, WorldState world, DateOnly transferDate)` -> `WorldState`
- [ ] Creates and registers new `Contract` in `WorldState`
- [ ] Updates `PlayerCareerState` with new `ClubId`, `WeeklySalary`, `SquadStatus`, and initial `ManagerTrust`
- [ ] Updates `Club` squad rosters: removes player from old club, adds to new club
- [ ] Credits `SigningBonus` to player's `FinanceAccount` (if present)
- [ ] Invokes `RelationshipSystem.ApplyClubTransfer` for teammate and manager dynamics
- [ ] `TransferSystem.RejectOffer(TransferOffer offer)` updates offer status to `Rejected`
- [ ] Unit tests for full transfer acceptance lifecycle and state integrity

**Branch:** `feature/p1-062-transfer-acceptance-flow`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.10", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-063] ContractSystem — Contract Negotiation Simulation",
        "body": """### User Story
> As a footballer and agent, I want to negotiate wage, role, and duration terms with prospective or existing clubs so that I can maximize my earnings and secure playing time.

### Acceptance Criteria
- [ ] `ContractSystem.NegotiateTerms(Player player, PlayerAbilities abilities, PlayerCareerState careerState, Club club, decimal demandedWage, int demandedYears, WorldState world, SimulationRandom rng)` -> `ContractNegotiationResult`
- [ ] `ContractNegotiationResult` record tracking outcome (`Accepted`, `CounterOffer`, `WalkedAway`), agreed wage, years, and bonus
- [ ] Club wage budget and tolerance curves based on club reputation and player overall
- [ ] Agent relationship modifier: agent affinity boosts negotiation leverage / improves compromise terms
- [ ] Counter-offer logic when demands are slightly above club threshold
- [ ] Walk-away logic when demands exceed club ceiling or negotiation breaks down
- [ ] Unit tests for acceptable wage demands, counter-offers, and walkaways

**Branch:** `feature/p1-063-contract-negotiation`
**Layer:** `Simulation` | **Complexity:** `L`""",
        "labels": ["phase-1", "milestone:1.10", "simulation", "complexity:L"]
    },
    {
        "title": "[P1-064] ContractSystem — Contract Expiry, Renewal & Free Agency",
        "body": """### User Story
> As a footballer, I want my contract to progress toward expiry, allowing renewal negotiations, pre-contract Bosman moves, or free agency release when a contract runs out.

### Acceptance Criteria
- [ ] `ContractSystem.EvaluateContractStatus(Contract contract, DateOnly currentDate)` -> `ContractExpiryStatus` (`Active`, `BosmanEligible`, `Expired`)
- [ ] Bosman eligibility triggered when remaining contract duration <= 6 months (<= 26 weeks)
- [ ] `ContractSystem.HandleContractExpiry(Player player, Contract contract, WorldState world, DateOnly currentDate)` -> `WorldState`
- [ ] Expired players become free agents (unattached `ClubId == Guid.Empty` or free agent pool, salary discontinued)
- [ ] Free agents remain eligible for free transfer offers without transfer fees
- [ ] `ContractSystem.OfferRenewal(...)` for current club retention
- [ ] Unit tests for Bosman timeline, free agency transition, and contract renewal

**Branch:** `feature/p1-064-contract-expiry-renewal`
**Layer:** `Simulation` | **Complexity:** `M`""",
        "labels": ["phase-1", "milestone:1.10", "simulation", "complexity:M"]
    },
    {
        "title": "[P1-065] Unit & Integration Tests — Transfer Scenarios & Contract Lifecycles",
        "body": """### User Story
> Comprehensive unit and integration test suite validating transfer offer generation, contract negotiations, free agency, and career stability.

### Acceptance Criteria
- [ ] Test: `TransferOffer_DomainInvariants_ValidateBoundsAndClamping()`
- [ ] Test: `TransferSystem_GenerateOffers_RespectsTierAndReputation()`
- [ ] Test: `TransferSystem_LowTrust_ForcesEscapeOffer()`
- [ ] Test: `TransferSystem_AcceptOffer_TransfersClubRosterAndAccount()`
- [ ] Test: `ContractSystem_Negotiation_AgentAffinityProvidesLeverage()`
- [ ] Test: `ContractSystem_BosmanEligibility_AtSixMonths()`
- [ ] Test: `ContractSystem_Expiry_TransitionsToFreeAgent()`

**Branch:** `feature/p1-065-transfer-tests`
**Layer:** `Tests` | **Complexity:** `L`""",
        "labels": ["phase-1", "milestone:1.10", "testing", "complexity:L"]
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
