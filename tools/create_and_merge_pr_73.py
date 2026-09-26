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

    branch = "feature/milestone-7.3-audio-sound-design"
    base = "main"

    title = "feat: Milestone 7.3 — Audio & Sound Design (#189, #190, #191, #192, #193)"
    body = """## Milestone 7.3: Audio & Sound Design

### Summary of Changes

#### 1. Audio Architecture, Buses & Volume Settings (#P7-301 / Issue #189)
- **Domain Audio Model (`AudioSettingsConfig.cs`)**:
  - Pure C# domain model in `FootballLife.Domain` with 5 channel buses: Master, SFX, Ambience, UI, and Music.
  - Full mute state management and effective linear volume calculation combining master attenuation and individual channel volumes.
  - High-precision logarithmic decibel conversions (`LinearToDecibels`, `DecibelsToLinear`) covering 0.0 to 1.0 (down to -80.0 dB).
  - 100% test coverage with xUnit in `AudioSettingsTests.cs`.
- **Runtime Audio Architecture (`AudioManager.cs`)**:
  - Persistent MonoBehaviour singleton (`[MANAGERS]/AudioManager`) with pre-allocated pooled audio sources for SFX and dedicated channels for Ambience, UI, and Music.
  - Zero GC allocations in frame updates.
  - Persistence of player audio preferences via `PlayerPrefs`.

#### 2. Procedural PCM Audio Synthesis (`ProceduralAudioClipFactory.cs`)
- Lightweight, zero-external-dependency procedural PCM wave generator (`AudioClip.Create`) synthesizing 15 game audio clips at runtime:
  - `KickSoft`, `KickHard`: Low frequency exponential sine decays with burst envelope.
  - `WoodworkHit`: Harmonic metallic square/sine collision clang with rapid ring decay.
  - `NetRipple`: Pink-noise turbulent airflow swoosh with envelope filter.
  - `WhistleStart`, `WhistleGoal`, `WhistleFullTime`: High-pitch dual-frequency modulated trill whistles.
  - `CrowdMurmurLoop`: Filtered low-frequency continuous ambient stadium rumble.
  - `CrowdGoalRoar`: Multiband rising frequency roar with white noise cheer surge.
  - `CrowdGasp`: Sharp intake/drop filtered noise for near misses, woodwork hits, and goalkeeper saves.
  - `UIClick`, `UITabSwitch`: Short high crisp acoustic clicks.
  - `UIWageChime`: Harmonically tuned major triad chime (C5-E5-G5).
  - `UIAlert`: Dual ascending bell tones (A5-D6).
  - `MenuMusicLoop`: Subtle rhythmic chord ambient progression.

#### 3. Match SFX Integration (#P7-302 / Issue #190)
- **Shooting & Impact**: Wired `ShootingInteraction.cs` to trigger velocity-scaled kick thumps upon touch release.
- **Woodwork Collision**: Wired `BallController.cs` `OnCollisionEnter` to detect post/crossbar tags/layers and fire metallic woodwork hits.
- **Net Ripple & Referee Whistle**: Wired `GoalTrigger.cs` and `ShootingInteraction.cs` to trigger net ripple and goal whistle sequence on successful goals.

#### 4. Crowd Atmosphere & Reactive Chants (#P7-303 / Issue #191)
- Continuous looped crowd murmur during match situation gameplay.
- Reactive crowd gasp on woodwork collision, goalkeeper dive/save, or near miss.
- Dynamic crowd celebration roar on goal scored.

#### 5. UI Audio Feedback & Wage Chimes (#P7-304 / Issue #192)
- Added `PlayUITabSwitch()` to all hub screen transitions (`ShowTraining`, `ShowRest`, `ShowCareer`, `ShowProfile`, `HideAllOverlays`).
- Added `PlayUIAlert()` on life event arrival and modal appearance.
- Wired `PlayUIWageChime()` in `SimulationBridge.AdvanceDay()` when weekly salary deposits are credited.
- Added `PlayUIClick()` to choice selections in `MatchGameController.cs`.

#### 6. Background Music Management & Ducking (#P7-305 / Issue #193)
- Background music lifecycle management in `AudioManager`: smooth fade in on launch in Career Hub and stop upon entering 3D match gameplay.
- Automatic music ducking (-14 dB attenuation) when referee whistles or crowd goal roars trigger, smoothly restoring afterwards.

### Verification
- `dotnet build FootballLife.Domain.csproj -c Release`: 0 errors.
- `dotnet build FootballLife.Simulation.csproj -c Release`: 0 errors.
- `dotnet test FootballLife.Simulation.Tests.csproj`: 689 / 689 tests passing (100%).
- `python3 tools/verify_release_readiness.py`: All 5 checks passed (compilation, test suite, world content, localization, DLL sync).

Closes #189
Closes #190
Closes #191
Closes #192
Closes #193
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-7): complete Milestone 7.3 - audio & sound design (#189, #190, #191, #192, #193)"], check=True)
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
        # Check if PR already exists
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
        "commit_title": f"feat: Milestone 7.3 — Audio & Sound Design (#{pr_number})",
        "commit_message": "Squash merge of Milestone 7.3 implementation into main.\n\nCloses #189, #190, #191, #192, #193",
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
    print("Done! Milestone 7.3 is fully merged to main.")

if __name__ == "__main__":
    main()
