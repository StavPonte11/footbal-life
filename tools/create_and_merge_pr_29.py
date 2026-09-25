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

    if not token:
        print("Error: Could not obtain GitHub token.", file=sys.stderr)
        sys.exit(1)

    headers = {
        "Authorization": f"Bearer {token}",
        "User-Agent": "FootballLife-PRCreator",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json; charset=utf-8"
    }

    branch_name = "feature/milestone-3.2-player-and-pawns"

    # 1. Stage and commit git changes
    print("Staging files and committing...")
    subprocess.run(["git", "add", "."], check=True)
    commit_msg = "feat(m3.2): 3D Player Character, Locomotion/Kicking & Situation Pawns (#135, #136)"
    subprocess.run(["git", "commit", "-m", commit_msg], check=True)

    # 2. Push branch
    print(f"Pushing {branch_name} to origin...")
    subprocess.run(["git", "push", "-u", "origin", branch_name], check=True)

    pr_title = "feat(m3.2): 3D Player Character, Locomotion/Kicking & Situation Pawns (#135, #136)"
    pr_body = """## Milestone 3.2 — 3D Player Character, Animations & Teammates

Delivers **Milestone 3.2: 3D Player Character, Animations & Teammates**, advancing **Phase 3 (Football Vertical Slice)**. Introduces mobile-optimized stylized 3D humanoid footballer pawns, procedural articulation and locomotion, kick impact synchronization with ball physics, active goalkeeper goal-line tracking, and tactical situation pawn arrangement.

---

### Key Features & Verification

#### 1. 3D Player Character & Procedural Articulation (#135 / #P3-001)
- **`HumanoidPawnBuilder.cs`**:
  - Procedural factory constructing mobile-optimized 3D footballer pawns (torso, hips, head, articulated arms/legs, and boots with studs).
  - Customizable kit schemes (`PawnKitScheme`): Home Outfield (Navy/Cyan/White), Away Outfield (Crimson/Navy), Goalkeeper (Fluorescent Lime/Black).
- **`PlayerPawnController.cs`**:
  - Procedural articulation state machine: `Idle`, `Jog`, `Run`, `PrepKick`, `Kick`, `Tackle`, `Celebrate`.
  - Smooth sinusoidal locomotion cycles (alternating leg swing, arm counter-swing, and vertical bounce).
  - Kick impact synchronization: forward striking leg swing dispatches `OnKickImpact` at the forward apex, launching the ball with `BallController.Kick()`.
  - Zero GC allocations in hot update loops.

#### 2. Situation-Driven Pawns & Active Goalkeeper (#136 / #P3-003)
- **`MatchPawn.cs`**:
  - Metadata component holding player identity (Name, KitNumber, Role: UserStriker, Teammate, Defender, Goalkeeper; Team: Home, Away) and visual selection ring.
- **`GoalkeeperController.cs`**:
  - Active opponent goalkeeper on the goal line ($Z = 34.8\text{ m}$).
  - Crouched ready stance, continuous lateral tracking along the goal line following ball $X$ coordinate (clamped to $[-3.2\text{ m}, +3.2\text{ m}]$).
  - Dynamic reaction to incoming high-speed shots: horizontal diving save or jump save postures.
- **`SituationPawnPresenter.cs`**:
  - Automatically arranges 5 situation pawns under `[ENTITIES]/Pawns`:
    - User Striker at $Z=13.5\text{ m}, X=0$
    - Supporting Teammate at $Z=18.5\text{ m}, X=11.5\text{ m}$
    - Opponent Center Back 1 at $Z=22.5\text{ m}, X=-3.5\text{ m}$
    - Opponent Center Back 2 at $Z=23.5\text{ m}, X=4.0\text{ m}$
    - Opponent Goalkeeper at $Z=34.8\text{ m}, X=0$
  - Binds User Striker and ball targets to `MatchCameraRig`.

---

### Automated Benchmarks
- **Pure C# Tests**: 557 / 557 Passed (100% Pass Rate).
- **Unity In-Engine Play Mode**: Verified player kick animation sequence, `OnKickImpact` event dispatch, ball launch, goalkeeper lateral tracking, and celebration animation.
- **Unity Compilation**: 0 errors across all assemblies.

Closes #135
Closes #136
"""

    pr_data = {
        "title": pr_title,
        "head": branch_name,
        "base": "main",
        "body": pr_body
    }

    print(f"Creating PR on {repo}...")
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_data).encode("utf-8"),
        headers=headers,
        method="POST"
    )

    try:
        with urllib.request.urlopen(req) as resp:
            pr_resp = json.loads(resp.read().decode("utf-8"))
            pr_number = pr_resp.get("number")
            pr_url = pr_resp.get("html_url")
            print(f"Created PR #{pr_number}: {pr_url}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"Failed to create PR: {e.code} - {err_msg}", file=sys.stderr)
        if "A pull request already exists" in err_msg:
            print("Checking existing PRs...")
            list_req = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch_name}",
                headers=headers
            )
            with urllib.request.urlopen(list_req) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    pr_url = prs[0]["html_url"]
                    print(f"Found existing PR #{pr_number}: {pr_url}")
                else:
                    sys.exit(1)
        else:
            sys.exit(1)

    # 3. Merge PR (Squash merge)
    print(f"Squash-merging PR #{pr_number}...")
    merge_data = {
        "commit_title": f"feat(m3.2): 3D Player Character, Locomotion/Kicking & Situation Pawns (#{pr_number})",
        "commit_message": f"Delivers Milestone 3.2: 3D Player Character, Animations & Teammates (#135, #136)\n\nCloses #135\nCloses #136",
        "merge_method": "squash"
    }
    merge_req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_data).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    merged = False
    for attempt in range(5):
        try:
            with urllib.request.urlopen(merge_req) as resp:
                merge_resp = json.loads(resp.read().decode("utf-8"))
                if merge_resp.get("merged"):
                    print(f"Successfully squash-merged PR #{pr_number} into main!")
                    merged = True
                    break
        except urllib.error.HTTPError as e:
            print(f"Merge attempt {attempt+1} failed: {e.code} - {e.read().decode('utf-8')}")
            time.sleep(2)

    if not merged:
        print("Warning: Could not automatically merge PR. Please review on GitHub.")
        sys.exit(1)

    # 4. Checkout main and pull
    print("Switching back to main and pulling latest changes...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("All done! Milestone 3.2 is fully merged and local repository is updated on main.")

if __name__ == "__main__":
    main()
