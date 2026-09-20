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

# Ensure milestone:1.7 label exists
label_data = {
    "name": "milestone:1.7",
    "color": "ededed",
    "description": "Milestone 1.7 - Economy System"
}
try:
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/labels",
        data=json.dumps(label_data).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    with urllib.request.urlopen(req) as resp:
        print("Created label milestone:1.7")
except urllib.error.HTTPError as e:
    if e.code == 422:
        print("Label milestone:1.7 already exists")
    else:
        print(f"Label creation warning: {e.code} {e.read().decode('utf-8')}")

issues_to_create = [
    {
        "title": "[P1-044] FinanceAccount Domain Model",
        "labels": ["phase-1", "milestone:1.7", "domain", "complexity:S"],
        "body": """### User Story
As a footballer, I want a `FinanceAccount` domain model tracking my financial balance and transaction history so that I can manage my earnings, expenses, and savings over time.

### Acceptance Criteria
- [ ] `FinanceAccount` record in `FootballLife.Domain`: `decimal Balance`, `IReadOnlyList<FinanceTransaction> History`
- [ ] `FinanceTransaction` record: `Guid Id`, `DateOnly Date`, `TransactionType Type`, `decimal Amount`, `string Description`
- [ ] `TransactionType` enum: `Salary`, `MatchBonus`, `LifestyleExpense`, `Fine`, `Investment`, `TransferBonus`
- [ ] `FinanceAccount.WithTransaction(FinanceTransaction tx)` -> returns new immutable `FinanceAccount` with updated `Balance` (`Balance + tx.Amount`) and transaction appended to `History`
- [ ] `FinanceAccount.Create(decimal initialBalance = 0)` factory method
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `FinanceAccount_InitialBalance_IsZero_ByDefault()`
- [ ] Unit test: `FinanceAccount_PositiveTransaction_IncreasesBalance()`
- [ ] Unit test: `FinanceAccount_NegativeTransaction_DecreasesBalance()`
- [ ] Unit test: `FinanceAccount_HistoryPreservesChronologicalOrder()`
"""
    },
    {
        "title": "[P1-045] EconomySystem — Weekly Salary Credit",
        "labels": ["phase-1", "milestone:1.7", "simulation", "complexity:S"],
        "body": """### User Story
As a footballer, I want my weekly contracted salary automatically credited to my bank account each week so that I have funds to spend on lifestyle and investments.

### Acceptance Criteria
- [ ] `EconomySystem` static class in `FootballLife.Simulation`
- [ ] `EconomySystem.ApplyWeeklySalary(FinanceAccount account, decimal weeklySalary, DateOnly date)` -> `FinanceAccount`
- [ ] Creates a `Salary` transaction with amount = `weeklySalary`
- [ ] Enforces non-negative salary parameter (`weeklySalary >= 0`)
- [ ] Returns new immutable `FinanceAccount`
- [ ] Zero `UnityEngine` references
- [ ] Unit test: `EconomySystem_WeeklySalary_CreditsContractedAmount()`
- [ ] Unit test: `EconomySystem_ZeroSalary_StillCreatesTransactionRecord()`
- [ ] Unit test: `EconomySystem_NegativeSalary_ThrowsArgumentOutOfRangeException()`
"""
    },
    {
        "title": "[P1-046] EconomySystem — Match Bonuses",
        "labels": ["phase-1", "milestone:1.7", "simulation", "complexity:S"],
        "body": """### User Story
As a footballer, I want my performance match bonuses (appearance, goal, assist, clean sheet) credited after matches so that strong on-pitch performance directly rewards my personal wealth.

### Acceptance Criteria
- [ ] `EconomySystem.ApplyMatchBonuses(FinanceAccount account, ContractBonuses bonuses, MatchResult result, Position position, DateOnly date)` -> `FinanceAccount`
- [ ] Evaluates bonuses earned:
  - Appearance: `result.PlayerMinutesPlayed > 0 ? bonuses.AppearanceBonus : 0`
  - Goals: `result.PlayerScored ? (goalsCount * bonuses.GoalBonus) : 0`
  - Assists: `result.PlayerAssisted ? (assistsCount * bonuses.AssistBonus) : 0`
  - Clean Sheet: `(position == Position.GK || position == Position.CB || position == Position.FB) && result.OpponentScore == 0 && result.PlayerMinutesPlayed >= 60 ? bonuses.CleanSheetBonus : 0`
- [ ] If total bonus > 0, appends single `MatchBonus` transaction with itemized breakdown in `Description`
- [ ] If total bonus == 0, returns original account untouched without empty transaction
- [ ] Zero allocations in evaluation path
- [ ] Unit test: `EconomySystem_MatchBonus_CalculatesGoalAndAppearanceCorrectly()`
- [ ] Unit test: `EconomySystem_DefenderCleanSheet_AwardsBonusWhenEligible()`
- [ ] Unit test: `EconomySystem_AttackerCleanSheet_NotAwardedCleanSheetBonus()`
- [ ] Unit test: `EconomySystem_DidNotPlay_ZeroBonusEarned()`
"""
    },
    {
        "title": "[P1-047] EconomySystem — Lifestyle Expense Deductions",
        "labels": ["phase-1", "milestone:1.7", "simulation", "complexity:M"],
        "body": """### User Story
As a footballer, I want weekly lifestyle expenses deducted according to my living tier so that higher luxury requires maintaining high earnings to avoid debt.

### Acceptance Criteria
- [ ] `LifestyleTier` enum in `FootballLife.Domain`: `Modest` (£150/wk), `Comfortable` (£500/wk), `Luxurious` (£2,000/wk), `Extravagant` (£8,000/wk), `Superstar` (£25,000/wk)
- [ ] `EconomySystem.GetLifestyleWeeklyCost(LifestyleTier tier)` -> `decimal`
- [ ] `EconomySystem.ApplyLifestyleExpenses(FinanceAccount account, LifestyleTier tier, DateOnly date)` -> `FinanceAccount`
- [ ] Deducts standard weekly cost as negative amount transaction of type `LifestyleExpense`
- [ ] Overdraft support: allows negative balance when expenses exceed funds
- [ ] Unit test: `EconomySystem_LifestyleExpenses_DeductsAccurateTierCost()`
- [ ] Unit test: `EconomySystem_Overdraft_AllowsNegativeBalance()`
- [ ] Unit test: `EconomySystem_AllLifestyleTiers_HaveExpectedWeeklyCosts()`
"""
    },
    {
        "title": "[P1-048] Unit & Integration Tests — Economy System",
        "labels": ["phase-1", "milestone:1.7", "testing", "complexity:M"],
        "body": """### User Story
Full test suite verifying economy transactions, multi-season financial stability, and zero-allocation compliance.

### Acceptance Criteria
- [ ] Test: `EconomySystem_FullSeasonAccumulation_38Weeks_NetWorthMatchesFormula()`
- [ ] Test: `EconomySystem_DeterministicFinancialTrajectory_SameContractSamePerformance()`
- [ ] Test: `EconomySystem_DebtScenarios_BalanceTracksAccuratelyAcrossWeeks()`
- [ ] Test: `WorldState_Integration_FinanceAccountPersistsInWorldState()`
- [ ] All tests pass with zero warnings, zero allocations in hot path
"""
    }
]

created_issues = []
for issue_spec in issues_to_create:
    payload = {
        "title": issue_spec["title"],
        "body": issue_spec["body"],
        "labels": issue_spec["labels"]
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues",
        data=json.dumps(payload).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            issue_num = data["number"]
            created_issues.append((issue_num, issue_spec["title"]))
            print(f"Created #{issue_num}: {issue_spec['title']}")
        time.sleep(1)
    except urllib.error.HTTPError as e:
        print(f"Failed to create {issue_spec['title']}: {e.code} {e.read().decode('utf-8')}", file=sys.stderr)
        sys.exit(1)

print("\n--- Summary of Created Issues ---")
for num, title in created_issues:
    print(f"#{num}: {title}")
