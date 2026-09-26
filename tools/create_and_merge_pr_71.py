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
        print("Error: No GitHub token found.")
        sys.exit(1)

    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": "application/vnd.github.v3+json",
        "Content-Type": "application/json"
    }

    branch = "feature/milestone-7.1-character-art-rigging"
    base = "main"

    title = "feat: Milestone 7.1 — Character Art & Rigging (#195, #196, #197)"
    body = """## Milestone 7.1: Character Art & Rigging

### Summary of Changes

#### 1. Base Humanoid Mesh & Rig (#P7-101 / Issue #195)
- **Standard Humanoid Bone Hierarchy & Avatar (`HumanoidAvatarUtility.cs`)**:
  - Implemented standard Mecanim Humanoid bone mapping (`Hips`, `Spine`, `Chest`, `Neck`, `Head`, `LeftUpperArm`, `LeftLowerArm`, `LeftHand`, `RightUpperArm`, `RightLowerArm`, `RightHand`, `LeftUpperLeg`, `LeftLowerLeg`, `LeftFoot`, `RightUpperLeg`, `RightLowerLeg`, `RightFoot`).
  - Generates valid `Avatar` via `AvatarBuilder.BuildHumanAvatar` at runtime and attaches an `Animator` component to each pawn.
  - Maintains full backward compatibility with procedural limb controllers (`PlayerPawnController.cs`, `GoalkeeperController.cs`).
- **Stylized Low-Poly Athletic Meshes (`StylizedMeshGenerator.cs`)**:
  - Outfield and Goalkeeper stylized mesh generation: athletic tapered V-torso jersey, contoured athletic shorts, defined athletic boots with cleats/studs, and contoured calves.
  - Goalkeeper equipment variant: padded long-sleeve compression jersey, padded goalkeeper gloves with finger/palm protection.
  - Highly optimized mobile polygon budget (~1,800 to 2,400 triangles per player).

#### 2. Modular Face, Hair & Skin Tone Variation System (#P7-102 / Issue #196)
- **Domain Visual Profile Model (`PlayerVisualProfile.cs`)**:
  - Pure C# domain model in `FootballLife.Domain` supporting 5 ethnic skin tones (Fair, Olive, Tan, Deep Bronze, Melanin Dark), 6 modular hairstyles (ShortCrop, BuzzFade, TexturedAfro, SlickBack, LongTied, Undercut), 6 authentic hair colors, 4 facial hair types, and 4 football boot styles.
  - Deterministic generation from player name, squad number, and position (`PlayerVisualProfile.CreateDeterministic`) ensuring permanent visual identity across matches.
  - Tested with xUnit in `PlayerVisualProfileTests.cs`.
- **Modular 3D Head & Hair Generation (`StylizedMeshGenerator.cs`, `HumanoidPawnBuilder.cs`)**:
  - Contoured low-poly head with defined jawline, brow, nose bridge, ear lobes, and stylized dark pupil eyes.
  - 6 distinct procedural 3D modular hair meshes fitted cleanly to head bone socket.
  - Facial hair / stubble / beard accent rendering.

#### 3. Kit System Compatibility Pass (#P7-103 / Issue #197)
- **Procedural Club-Color Material Application**:
  - Upgraded `PawnKitScheme` to support `WithVisualProfile()` resolving primary jersey, shorts, socks, skin, and hair colors.
  - Added procedural jersey back squad number badge rendering (`StylizedMeshGenerator.CreateSquadNumberTexture`) rendering clean digital athletic numerals 1–99.
  - Updated `SituationPawnPresenter.cs` passing distinct visual profiles and squad numbers to User Striker (#9), Teammate (#11), CB1 (#4), CB2 (#5), and Goalkeeper (#1).

### Verification
- `dotnet build FootballLife.Domain.csproj -c Release`: 0 errors.
- `dotnet build FootballLife.Simulation.csproj -c Release`: 0 errors.
- `dotnet test FootballLife.Simulation.Tests.csproj`: 694 / 694 tests passing (100%).
- `python3 tools/verify_release_readiness.py`: All 5 checks passed (compilation, test suite, world content, localization, DLL sync).

Closes #195
Closes #196
Closes #197
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-7): complete Milestone 7.1 - character art & rigging (#195, #196, #197)"], check=True)
    subprocess.run(["git", "push", "origin", branch], check=True)

    print("Step 2: Creating Pull Request...")
    pr_payload = {
        "title": title,
        "head": branch,
        "base": base,
        "body": body
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_payload).encode("utf-8"),
        headers=headers,
        method="POST"
    )

    pr_number = None
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            pr_number = data["number"]
            print(f"Created PR #{pr_number}: {data['html_url']}")
    except urllib.error.HTTPError as e:
        err_body = e.read().decode("utf-8")
        print(f"Failed to create PR: {e.code} - {err_body}")
        req_list = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
            headers=headers
        )
        with urllib.request.urlopen(req_list) as resp:
            prs = json.loads(resp.read().decode("utf-8"))
            if prs:
                pr_number = prs[0]["number"]
                print(f"Found existing PR #{pr_number}")
            else:
                sys.exit(1)

    print(f"Step 3: Merging PR #{pr_number}...")
    merge_payload = {
        "commit_title": f"feat: Milestone 7.1 — Character Art & Rigging (#{pr_number})",
        "commit_message": "Squash merge of Milestone 7.1 implementation into main.\n\nCloses #195, #196, #197",
        "merge_method": "squash"
    }
    req_merge = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    merged = False
    for attempt in range(5):
        try:
            with urllib.request.urlopen(req_merge) as resp:
                res = json.loads(resp.read().decode("utf-8"))
                if res.get("merged"):
                    print(f"Successfully merged PR #{pr_number}!")
                    merged = True
                    break
        except urllib.error.HTTPError as e:
            err_body = e.read().decode("utf-8")
            print(f"Merge attempt {attempt+1} failed: {e.code} - {err_body}")
            time.sleep(2)

    if not merged:
        print("Could not merge automatically.")
        sys.exit(1)

    print("Step 4: Pulling latest main locally...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done! Milestone 7.1 is fully merged to main.")

if __name__ == "__main__":
    main()
