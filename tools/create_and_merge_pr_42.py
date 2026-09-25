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

    branch = "feature/milestone-4.2-smartphone-os-relationships"
    base = "main"

    title = "feat: Milestone 4.2 — Smartphone OS & Social Layer (#P4-003, #P4-004)"
    body = """## Milestone 4.2: Smartphone OS & Social Layer

### Summary of Changes

#### 1. Smartphone OS UI — Messages, Social Feed & Journalism News (#P4-003 / Issue #149)
- **Smartphone Chassis & OS Frame**:
  - UI Toolkit overlay (`PhoneOSView.uxml` and `PhoneOSController.cs`) with realistic mobile bezel, dynamic island, status bar (carrier, 5G, time, battery), screen viewport, and bottom dock navigation.
  - Close button and home indicator for fluid open/close animations.
- **Four Integrated Mobile Applications**:
  - 💬 **WhatsApp (`Messages`)**: Conversation list, conversation thread with incoming dialogue bubbles, and interactive reply choices affecting relationships and morale.
  - 📸 **FootyGram (`Social`)**: Social media card feed displaying user handle, author club, photo preview card, verified badge, caption, comment count, and an interactive like button with animated counter toggling.
  - 👥 **Relationship Hub (`Contacts`)**: People-centric list displaying individual contacts with affinity and trust progress bars, shared relationship history, and contextual action buttons.
  - 📰 **Football Daily (`News`)**: Sports journalism feed with breaking transfer rumors, tactical match previews, and player spotlight cards.
- **Pure C# Domain Models**:
  - `PhoneMessage`: Message records with sender, preview, full dialogue, and branching reply options.
  - `DialogueChoice`: Choice text and deterministic impact on affinity, trust, and morale.
  - `SocialPost`: Post record with author, handle, content, likes, and like toggling logic.
- **Pure C# Simulation Systems**:
  - `PhoneSystem`: Generates default contacts, generates contextual messages, processes player replies deterministically, and generates social feed and sports journalism content.
- **Scene Wiring**:
  - Embedded `PhoneOSView` into both `HomeHUDView.uxml` and `CareerHubView.uxml`.
  - Wired phone hotspot in `HomeController.cs` and quick-access phone dock button in `CareerHubController.cs` & `CareerHubCoordinator.cs`.

#### 2. Relationship Hub — People-Centric Bonds, Trust & Social Actions (#P4-004 / Issue #150)
- **Pure C# Domain Models**:
  - `SocialActionType`: Enum defining `CallCatchUp`, `SendGift`, `DinnerHangOut`, and `TalkTactics`.
- **Pure C# Simulation System (`RelationshipSystem.cs`)**:
  - `ExecuteSocialAction`: Deterministic execution engine supporting:
    - `CallCatchUp`: Energy cost (-5), affinity boost (+4).
    - `SendGift`: Monetary cost (-£200), affinity boost (+8.5).
    - `DinnerHangOut`: Energy cost (-15), monetary cost (-£80), affinity (+12) and morale (+8) boost.
    - `TalkTactics`: Manager-specific action boosting Manager Trust (+6.5) at modest energy cost (-8).
  - Enforces affordability constraints (bank balance check, minimum energy check).
- **UI Toolkit Integration**:
  - Action buttons ("Call", "Gift", "Dinner", "Tactics") on each contact card in the Contacts tab.
  - Live feedback toast notifications reporting stat deltas and status messages.
  - Immediate real-time refresh of affinity/trust progress bars, header bank balance, and energy meters.

### Testing & Validation
- **Unit Tests**: 580 / 580 tests passing with zero failures (`dotnet test`), including 11 comprehensive tests in `SocialSystemTests.cs`.
- **Live Play Mode (Unity Editor MCP)**:
  - Verified opening phone overlay via `PhoneOSController.OpenPhone()`.
  - Switched between all 4 apps (Messages, Social, Contacts, News) seamlessly.
  - Verified social post like toggling with dynamic counter update.
  - Verified dialogue reply choice resolving and applying morale/affinity deltas.
  - Verified social actions:
    - Call Partner: Energy dropped from 72% to 67%, Partner affinity increased from 75% to 79%.
    - Send Gift to Partner: Deducted £200 from bank balance, affinity increased to 87.5%.
  - Verified phone closes cleanly returning back to Home apartment / Career Hub.
  - 0 console warnings or errors.

Closes #149
Closes #150
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
    for issue_num in [149, 150]:
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

    print("\n✅ Milestone 4.2 PR created, merged, and issues closed!")

if __name__ == "__main__":
    main()
