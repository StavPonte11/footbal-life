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

    branch = "feature/milestone-4.3-life-events-finances-shop"
    base = "main"

    title = "feat: Milestone 4.3 — Life Events, Finances & Lifestyle Shop (#P4-005, #P4-006, #P4-007)"
    body = """## Milestone 4.3: Life Events, Finances & Lifestyle Shop

### Summary of Changes

#### 1. Life Event System — Immersive Choice-Cards with Character Context (#P4-005 / Issue #152)
- **Immersive Choice Card UI (`LifeEventView.uxml`, `LifeEventController.cs`)**:
  - Full-screen modal overlay with glassmorphism backdrop, event character avatar portrait, author name, category badge, and atmospheric narrative description box.
  - Dynamic choice cards presenting distinct options with transparent visual stat impact chips (+/- Money, Energy, Morale, Manager Trust).
  - Hover highlights and tactile click responses.
- **Simulation Bridge Integration (`SimulationBridge.cs`)**:
  - Added `ResolveLifeEventChoice(choice)` to deterministically mutate player abilities, current state (energy, morale, form), financial balances, and manager trust.
  - Curated high-stakes dilemmas: `Night Out Before Matchday`, `Sponsorship Controversy`, `Family Request`.
  - Dual access points in both `CareerHub` (header life button) and `Home` apartment (vitals bar).

#### 2. Finances Screen — Balance, Cash Flow, Weekly Income/Expenses & Lifestyle Tier (#P4-006 / Issue #153)
- **Pure C# Domain Model (`FinanceBreakdown.cs`)**:
  - Comprehensive model calculating:
    - Weekly income: gross wage, estimated match bonuses, commercial sponsorships.
    - Weekly expenses: apartment upkeep, lifestyle tier base cost, item upkeep, 35% tax withholding, 5% agent commission.
    - Net weekly cash flow (surplus/deficit).
    - Estimated net worth: bank balance + property valuation + physical assets.
- **Interactive Finances Dashboard UI (`FinancesView.uxml`, `FinancesController.cs`)**:
  - 4 high-level KPI cards: `Bank Balance`, `Net Weekly Cash Flow`, `Estimated Net Worth`, `Prestige Rating`.
  - Tabulated Income and Expenses breakdown cards with percentage meters.
  - Living Standard Tier Selector (`Modest`, `Comfortable`, `Luxury`, `Excessive`) updating weekly expenses and cash flow dynamically.
  - Chronological transaction ledger with credit/debit indicators and date stamps.

#### 3. Lifestyle Item Shop — Vehicles, Fashion, Tech & Wellness Upgrades (#P4-007 / Issue #154)
- **Pure C# Domain Models (`LifestyleItem.cs`)**:
  - `LifestyleCategory` enum: `Vehicles`, `Fashion`, `Tech`, `Wellness`.
  - `LifestyleItem` record: `Id`, `Name`, `Category`, `Price`, `WeeklyUpkeep`, `MoralePerk`, `EnergyRecoveryPerk`, `PrestigeScore`, `Description`, `IconEmoji`.
  - `LifestyleCatalog`: 16 curated luxury items across all categories.
- **Pure C# Simulation Systems (`LifestyleShopSystem.cs`)**:
  - `CanAffordItem`: Verifies wallet balance against purchase price.
  - `PurchaseItem`: Deducts cost via `FinanceTransaction`, enforces duplicate ownership checks, and awards morale perks.
  - `CalculateTotalItemUpkeep`: Computes cumulative weekly maintenance across all owned items.
  - `CalculateTotalPerks`: Aggregates morale perks, sleep recovery multipliers, and prestige scores.
- **Wellness Upgrades & Bed Recovery Integration (`HomeSystem.cs`)**:
  - Extended `HomeSystem.CalculateSleepRecovery` to accept additional wellness perks.
  - Equipment such as `wel_espresso` (+3% rest), `wel_boots` (+5% rest), `wel_cryo` (+8% rest) directly buff bed sleep recovery.
- **Luxury Boutique Storefront UI (`LifestyleShopView.uxml`, `LifestyleShopController.cs`)**:
  - Boutique header with wallet balance, prestige rating, and category filter tabs.
  - Elevated item cards displaying price, weekly upkeep, perk tags, and purchase buttons (`BUY NOW`, `✓ OWNED`, `INSUFFICIENT FUNDS`).
  - Interactive purchase confirmation with dynamic toast feedback and real-time inventory updates.

### Testing & Validation
- **Automated Unit Tests**:
  - `simulation/FootballLife.Simulation.Tests/LifestyleShopSystemTests.cs`: 6 tests verifying catalog completeness, affordability checks, successful purchases, duplicate rejection, insufficient funds rejection, upkeep calculation, and perk aggregation.
  - `simulation/FootballLife.Simulation.Tests/FinancesBreakdownTests.cs`: 2 tests verifying full financial breakdown (income, expense, surplus/deficit, net worth) and wellness sleep recovery boost.
  - Test Suite: **590 / 590 tests passing with 0 failures** (`dotnet test`).
- **Live Play Mode (Unity Editor MCP)**:
  - Verified `SimulationBridge` runtime initialization.
  - Executed runtime item purchase of `wel_espresso`: balance decreased from £45,000 to £42,800, owned items tracked, +3% rest recovery bonus applied, +20 prestige awarded.
  - Verified `FinanceBreakdown.Calculate` in Play Mode: £1,200 income, £1,165 expenses, £35 net cash flow, £60,000 net worth, 55 total prestige.
  - Verified UI compilation of `FinancesController.cs`, `LifestyleShopController.cs`, and `LifeEventController.cs` into `FootballLife.Unity.UI.dll`.
  - Clean console with zero errors or warnings during Play Mode.

Closes #152
Closes #153
Closes #154
"""

    print("Step 1: Staging files...")
    subprocess.run(["git", "add", "."], check=True)

    print("Step 2: Committing...")
    commit_msg = f"{title}\n\n{body}"
    subprocess.run(["git", "commit", "-m", commit_msg], check=True)

    print(f"Step 3: Pushing {branch} to origin...")
    subprocess.run(["git", "push", "-u", "origin", branch], check=True)

    headers = {
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28"
    }
    if token:
        headers["Authorization"] = f"Bearer {token}"

    print("Step 4: Creating Pull Request...")
    pr_data = {
        "title": title,
        "head": branch,
        "base": base,
        "body": body
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_data).encode("utf-8"),
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            pr_res = json.loads(resp.read().decode("utf-8"))
            pr_number = pr_res["number"]
            pr_url = pr_res["html_url"]
            print(f"Successfully created PR #{pr_number}: {pr_url}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError creating PR: {e.code} - {err_msg}")
        return

    print("Step 5: Merging Pull Request (Squash Merge)...")
    merge_data = {
        "commit_title": f"{title} (#{pr_number})",
        "merge_method": "squash"
    }
    req_merge = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_data).encode("utf-8"),
        headers=headers,
        method="PUT"
    )
    time.sleep(2)
    try:
        with urllib.request.urlopen(req_merge) as resp:
            merge_res = json.loads(resp.read().decode("utf-8"))
            print(f"PR #{pr_number} successfully merged: {merge_res.get('message', 'Merged')}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError merging PR: {e.code} - {err_msg}")
        return

    print("Step 6: Switching to main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)

    print("Step 7: Closing issues #152, #153, #154...")
    for issue_id in [152, 153, 154]:
        close_data = {"state": "closed"}
        req_close = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues/{issue_id}",
            data=json.dumps(close_data).encode("utf-8"),
            headers=headers,
            method="PATCH"
        )
        try:
            with urllib.request.urlopen(req_close) as resp:
                print(f"Closed issue #{issue_id}")
        except urllib.error.HTTPError as e:
            print(f"Could not close issue #{issue_id}: {e.code}")

    print("Milestone 4.3 completed, merged and closed successfully!")

if __name__ == "__main__":
    main()
