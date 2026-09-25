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
    print("Error: Could not obtain GitHub token.", file=sys.stderr)
    sys.exit(1)

print(f"Target Repository: {repo}")

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

issues = [
    {
        "title": "feat(events): Life Event System — Immersive Choice-Cards with Character Context (#P4-005)",
        "body": """### User Story
As a footballer navigating professional football and private life, I want narrative life dilemmas and moral choice events presented as immersive choice-cards with character dialogue and transparent consequence previews so that my decisions carry weight and shape my career trajectory.

### Acceptance Criteria
- [ ] Connect `LifeEventSystem` to the main simulation loop and Career Hub / Home workflows.
- [ ] UI Toolkit dilemma modal (`LifeEventModalView.uxml` and controller) supporting:
  - Event title, category tag (Media, Social, Personal, Club, Commercial), and narrative description.
  - Involved character avatar/badge (e.g. Agent, Manager, Teammate, Journalist).
  - 2 to 3 contextual choice buttons displaying clear decision text and impact previews (e.g. `Happiness +10`, `Manager Trust -5`, `Bank -£1,500`).
- [ ] Consequence execution via `LifeEventSystem.ApplyChoice`: updates player state, manager trust, finances, and applies event cooldowns to `WorldState`.
- [ ] Toast notification and feedback banner summarizing the immediate result of the choice.
- [ ] Unit tests verifying event selection, eligibility conditions, choice execution, and cooldown enforcement.
""",
        "labels": ["phase-4", "milestone:4.3", "unity-ui", "simulation", "complexity:M"]
    },
    {
        "title": "feat(finances): Finances Screen — Balance, Cash Flow, Weekly Income/Expenses & Lifestyle Tier (#P4-006)",
        "body": """### User Story
As a footballer earning weekly wages and match bonuses, I want a dedicated Finances screen showing my bank balance, cash flow summary, weekly income vs expense breakdown, chronological transaction history, and lifestyle tier management so that I can prudently manage my wealth and avoid debt.

### Acceptance Criteria
- [ ] Dedicated UI Toolkit Finances dashboard (`FinancesView.uxml` and `FinancesController.cs`):
  - Primary metric cards: Total Bank Balance, Net Cash Flow (Surplus / Deficit per week), and Estimated Net Worth.
  - Weekly Income breakdown: Base wage, average match bonuses, endorsements/investments.
  - Weekly Expenses breakdown: Apartment upkeep, lifestyle tier cost, agent commission, tax deductions.
  - Chronological transaction ledger with transaction type badges, formatted amounts (+/-), dates, and descriptions.
  - Lifestyle Tier management widget allowing players to upgrade or downscale their tier (Modest to Superstar) with safety checks.
- [ ] Integration with `EconomySystem` and `SimulationBridge` to display live player finance data.
- [ ] Navigation shortcuts from Career Hub, Home HUD, and Phone OS.
- [ ] Unit tests validating cash flow calculations, ledger history updates, and tier adjustment constraints.
""",
        "labels": ["phase-4", "milestone:4.3", "unity-ui", "simulation", "complexity:M"]
    },
    {
        "title": "feat(shop): Lifestyle Item Shop — Vehicles, Fashion, Tech & Wellness Upgrades (#P4-007)",
        "body": """### User Story
As a footballer with disposable income, I want a Lifestyle Item Shop where I can browse and purchase luxury items across categories (Vehicles, Fashion/Jewelry, Tech/Home, Wellness) that grant permanent stat perks and enhance my lifestyle prestige.

### Acceptance Criteria
- [ ] Pure C# Domain models: `LifestyleItem` record (`Id`, `Name`, `Category`, `Price`, `WeeklyUpkeep`, `MoralePerk`, `EnergyPerk`, `PrestigeRating`, `Description`, `IconKey`).
- [ ] Pure C# Simulation system: `LifestyleShopSystem` providing default catalog, purchase validation (`CanAffordItem`), transaction processing (`PurchaseItem`), and inventory persistence in save data.
- [ ] UI Toolkit Lifestyle Shop view (`LifestyleShopView.uxml` and `LifestyleShopController.cs`):
  - Category tabs: 🚗 Vehicles, ⌚ Fashion & Luxury, 💻 Tech & Home, 🧘 Wellness & Recovery.
  - Item catalog cards showing price, upkeep, perk description, and purchase/owned status button.
  - Confirmation purchase modal with bank balance check.
  - Real-time deduction of purchase price via `FinanceTransaction` and addition of weekly upkeep to expenses.
- [ ] Immediate gameplay stat perks:
  - Wellness items (e.g. Cryotherapy Chamber, Espresso Machine) boosting daily rest energy recovery.
  - Luxury/Vehicle items (e.g. Sports Coupe, Designer Chronograph) boosting baseline Morale and Prestige.
- [ ] Unit tests covering catalog validation, item purchasing, insufficient funds handling, duplicate prevention, and upkeep integration.
""",
        "labels": ["phase-4", "milestone:4.3", "simulation", "unity-ui", "complexity:M"]
    }
]

created_issues = []
for issue in issues:
    data = json.dumps(issue).encode("utf-8")
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues",
        data=data,
        headers=headers,
        method="POST"
    )
    try:
        with urllib.request.urlopen(req) as resp:
            res = json.loads(resp.read().decode("utf-8"))
            num = res["number"]
            url = res["html_url"]
            print(f"Created Issue #{num}: {issue['title']} -> {url}")
            created_issues.append((num, issue["title"]))
            time.sleep(1)
    except urllib.error.HTTPError as e:
        print(f"Error creating issue '{issue['title']}': {e.code} - {e.read().decode('utf-8')}")

print("\nSummary of created issues:")
for num, title in created_issues:
    print(f"  #{num}: {title}")
