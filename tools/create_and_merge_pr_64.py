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

    branch = "feature/milestone-6.4-mobile-performance-release"
    base = "main"

    title = "feat: Milestone 6.4 — Mobile Performance Pass & Release Validation (#181)"
    body = """## Milestone 6.4: Mobile Performance & Release Validation

### Summary of Changes

#### 1. Performance Budgeting & Diagnostics (#P6-002 / Issue #181)
- **Domain Layer (`FootballLife.Domain`)**:
  - `QualityTier` enum: `BatterySaver` (0), `Low` (1), `Medium` (2), `High` (3).
  - `PerformanceBudget` static class: Target frame rates (60 FPS 3D gameplay, 30 FPS battery saver / UI menus), memory limits (200MB total mobile heap, 10MB world state budget), 20% low battery threshold, and `GetRecommendedFrameRate()`.
- **Simulation Layer (`FootballLife.Simulation`)**:
  - `PerformanceProfiler`: lightweight diagnostics tool measuring thread allocation bytes (`GC.GetAllocatedBytesForCurrentThread()`) and execution duration across hot paths.
  - Zero-GC hot path audit: eliminated closure delegate allocation `Func<AttributeName, float>` in `ActionResolver.ComputeWeightedAbilityScore`, bringing match action evaluations down to 0 bytes allocated per call.

#### 2. Unity Presentation Layer (`FootballLife.Unity.Core`)
- `MobilePerformanceManager`:
  - Enforces `Application.targetFrameRate` dynamically (60 FPS during 3D gameplay, 30 FPS in menus / low battery / battery saver).
  - Configures `Screen.sleepTimeout = SleepTimeout.NeverSleep` and disables VSync override.
  - Monitors battery state (`SystemInfo.batteryLevel`) every 30 seconds.
  - Manages `PerformSceneBoundaryCleanup()` (`Resources.UnloadUnusedAssets()` and GC sweep) strictly on scene boundaries, protecting 60 FPS gameplay loops from frame drops.

#### 3. Automated Performance Benchmark Suite & Verification
- `PerformanceBudgetTests.cs`:
  - `PerformanceBudget_RecommendedFrameRates_FollowsBatteryAndGameplayRules`: Validates 60 FPS in gameplay, 30 FPS in menus and battery-saver mode.
  - `PerformanceBudget_MatchSituationEvaluation_HighThroughputZeroGcHotPath`: 5,000 action resolutions benchmarked with zero-GC hot path compliance.
  - `PerformanceBudget_WorldStateMemoryFootprint_UnderThreshold`: Validates full retained world state (66 clubs, 11 leagues) is < 5MB (well below 10MB limit).
  - `PerformanceBudget_SimulationThroughput_MatchesSimulateFast`: 100 90-minute match simulations in < 2.0s.
- **676 / 676 unit tests passing** across `FootballLife.Simulation.Tests` with 0 failures.
- `tools/verify_release_readiness.py`: Automated release gate verifying Release compilation, 676 unit tests, 11 leagues, 66 clubs, 103 life events, 5 language catalogs (86 keys with 100% parity), and synchronized Unity plugin DLLs.

Closes #181
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-6): complete Milestone 6.4 - mobile performance & release validation (#181)"], check=True)
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
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError creating PR: {e.code} {err_msg}")
        if "A pull request already exists" in err_msg:
            list_req = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
                headers=headers
            )
            with urllib.request.urlopen(list_req) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    print(f"Found existing PR #{pr_number}: {prs[0]['html_url']}")

    if not pr_number:
        print("Failed to identify PR number.")
        sys.exit(1)

    print(f"Step 3: Merging PR #{pr_number} via squash merge...")
    time.sleep(2)

    merge_payload = {
        "merge_method": "squash",
        "commit_title": f"feat: Milestone 6.4 — Mobile Performance Pass & Release Validation (#181) (#{pr_number})"
    }
    merge_req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    try:
        with urllib.request.urlopen(merge_req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            if data.get("merged"):
                print(f"Successfully merged PR #{pr_number} to {base}!")
            else:
                print(f"Merge response: {data}")
    except urllib.error.HTTPError as e:
        print(f"Error merging PR: {e.code} {e.read().decode('utf-8')}")
        sys.exit(1)

    print("Step 4: Updating local main branch...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done!")

if __name__ == "__main__":
    main()
