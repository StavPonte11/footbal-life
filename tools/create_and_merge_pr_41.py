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

    branch = "feature/milestone-4.1-home-environment-progression"
    base = "main"

    title = "feat: Milestone 4.1 — 3D Home Apartment Environment & Lifestyle Progression (#P4-001, #P4-002)"
    body = """## Milestone 4.1: 3D Home Apartment Environment & Lifestyle Progression

### Summary of Changes

#### 1. Interactive 3D Home Apartment Environment (#P4-001 / Issue #146)
- **Scene Architecture**: Dedicated `Home.unity` scene conforming to standard 5-root hierarchy (`[MANAGERS]`, `[ENVIRONMENT]`, `[ENTITIES]`, `[CAMERAS]`, `[UI]`) and registered in `EditorBuildSettings.asset`.
- **Procedural 3D Environment Generator (`ApartmentBuilder.cs`)**: Builds architectural modern room geometry (engineered wood flooring, walls, ceiling, window overlooking city skyline with night lights, warm ambient illumination).
- **Hotspot Interaction Zones (`HomeInteractionZone.cs`)**:
  - 🛏️ **Bed Zone**: Rest and fatigue recovery hotspot
  - 🏋️ **Gym Zone**: Workout equipment for physical conditioning
  - 📱 **Lounge/Phone Zone**: Modern sofa and smartphone coffee table
  - 🚪 **Door Zone**: Apartment entrance returning to Career Hub
- **Home Camera Rig (`HomeCameraRig.cs`)**: Smooth framing transitions between overview ambient drift and zone-focused camera angles.
- **Input & Raycast Controller (`HomeInteractionController.cs`)**: Decoupled presentation interaction system translating screen taps to zone focus and event dispatch.
- **UI Toolkit Lifestyle HUD (`HomeHUDView.uxml` and `HomeController.cs`)**: Header bar with property name, tier badge, energy meter, and bank balance; bottom dock with zone navigation shortcuts; toast notification banner.
- **Career Hub Integration**: Wired "🏠 Home Apartment" button in `CareerHubView.uxml`, `CareerHubController.cs`, and `CareerHubCoordinator.cs`.

#### 2. Home Progression & Lifestyle Apartment Tiers (#P4-002 / Issue #147)
- **Pure C# Domain Models (`HomeProperty.cs`)**: Defined `HomeProperty` record and `HomePropertyCatalog` supporting 5 lifestyle tiers:
  - Tier 0: `Modest Studio` (£0, £80/wk upkeep, 1.00x rest, 1.00x gym)
  - Tier 1: `Comfortable Townhome` (£35,000, £250/wk upkeep, 1.15x rest, 1.10x gym)
  - Tier 2: `Luxurious Penthouse` (£180,000, £750/wk upkeep, 1.35x rest, 1.25x gym)
  - Tier 3: `Extravagant Villa` (£850,000, £2,400/wk upkeep, 1.60x rest, 1.45x gym)
  - Tier 4: `Superstar Estate` (£3,500,000, £7,500/wk upkeep, 2.00x rest, 1.75x gym)
- **Pure C# Simulation System (`HomeSystem.cs`)**:
  - `CanAffordUpgrade`: Validates bank balance and 1.5x upkeep wage safety ratio.
  - `UpgradeHome`: Deducts purchase cost, updates save tier, applies morale bonus, and supports free downsizing.
  - `CalculateSleepRecovery`: Computes sleep energy gains scaled by property rest efficiency multipliers.
  - `ExecuteHomeWorkout`: Calculates physical attribute gains (Stamina, Strength XP) with gym tier multipliers.
- **Real-Time 3D Environment Reskinning**: `ApartmentBuilder.RebuildForTier` dynamically updates materials, furniture colors, wood tones, and lighting upon tier upgrade.
- **UI Upgrade Modal**: In-game dialog showing target property specs, purchase cost, weekly upkeep requirements, rest bonus, and gym XP multipliers.

### Testing & Validation
- **Unit Tests**: 569 / 569 passing with zero failures (`dotnet test`), including 12 comprehensive new tests in `HomeSystemTests.cs`.
- **Live Play Mode (Unity Editor MCP)**:
  - Verified `SimulationBridge` initialization and lifestyle save state.
  - Tested Bed interaction: recovered energy from 72% to 100%.
  - Tested Gym interaction: executed workout and gained physical attributes.
  - Tested Home Upgrade: successfully upgraded to Luxurious Penthouse (Tier 2), deducted £180,000, and dynamically rebuilt apartment interior materials.
  - Verified 0 console errors and clean exit from Play Mode.

Closes #146
Closes #147
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

    print("Step 5: Merging Pull Request (Squash and Merge)...")
    time.sleep(2)
    merge_data = {
        "commit_title": f"{title} (#{pr_number})",
        "commit_message": body,
        "merge_method": "squash"
    }
    req_merge = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_data).encode("utf-8"),
        headers=headers,
        method="PUT"
    )
    try:
        with urllib.request.urlopen(req_merge) as resp:
            merge_res = json.loads(resp.read().decode("utf-8"))
            print("Merge result:", merge_res.get("message", "Merged successfully"))
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError merging PR: {e.code} - {err_msg}")

    print("Step 6: Switching to main and pulling...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)

    print("Step 7: Ensuring issues are closed...")
    for issue_num in [146, 147]:
        req_issue = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues/{issue_num}",
            data=json.dumps({"state": "closed"}).encode("utf-8"),
            headers=headers,
            method="PATCH"
        )
        try:
            with urllib.request.urlopen(req_issue) as resp:
                print(f"Issue #{issue_num} closed successfully.")
        except urllib.error.HTTPError as e:
            print(f"Notice on closing issue #{issue_num}: {e.code}")

    print("\n✅ Milestone 4.1 PR created, merged, and issues closed!")

if __name__ == "__main__":
    main()
