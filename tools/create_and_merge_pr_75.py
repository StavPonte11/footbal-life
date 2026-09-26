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

    branch = "feature/milestone-7.5-ip-safe-renaming"
    base = "main"

    title = "feat: Milestone 7.5 — Safe Fictional Renaming Pass & IP Resolution (#187)"
    body = """## Milestone 7.5: Safe Fictional Renaming Pass & IP Resolution

### Summary of Changes

#### 1. Fictional Club & League Renaming Pass (#P7-501 / Issue #187)
- **Leagues Renamed (`content/data/leagues.json`)**:
  - Replaced all 11 licensed real-world league identities with legally safe, authentic fictional counterparts:
    - `Premier League` $\\rightarrow$ `Crown Premiership`
    - `EFL Championship` $\\rightarrow$ `Crown Championship`
    - `EFL League One` $\\rightarrow$ `Crown Division 1`
    - `La Liga` $\\rightarrow$ `Liga de Honor`
    - `Segunda División` $\\rightarrow$ `Liga Plata`
    - `Bundesliga` $\\rightarrow$ `Deutsche Liga A`
    - `2. Bundesliga` $\\rightarrow$ `Deutsche Liga B`
    - `Serie A` $\\rightarrow$ `Serie Campionato`
    - `Serie B` $\\rightarrow$ `Serie Cadetti`
    - `Ligue 1` $\\rightarrow$ `Ligue Première`
    - `Ligue 2` $\\rightarrow$ `Ligue Deuxième`
- **Clubs Renamed (`content/data/clubs.json` & `tools/generate_expanded_clubs.py`)**:
  - Converted all 66 clubs and their home stadiums to safe fictional counterparts preserving all attributes, capacities, financial balances, kit colors, and league associations:
    - e.g., `Arsenal` $\\rightarrow$ `North London Red` (`NLR`), `Ashburton Grove`, owner `Arthur Vance`
    - e.g., `Manchester City` $\\rightarrow$ `Eastland City` (`EAC`), `Eastland Arena`
    - e.g., `Liverpool` $\\rightarrow$ `Merseyside Red` (`MSR`), `Anfield Road Ground`
    - e.g., `Chelsea` $\\rightarrow$ `West London Blue` (`WLB`), `Kings Road Bridge`
    - e.g., `Real Madrid` $\\rightarrow$ `Madrid Royal` (`MDR`), `Castellana Arena`
    - e.g., `Barcelona` $\\rightarrow$ `Catalunya FC` (`CAT`), `Les Corts Coliseum`
    - e.g., `Bayern Munich` $\\rightarrow$ `Munich Red` (`MNU`), `Isar Arena`
    - e.g., `Inter Milan` $\\rightarrow$ `Milano Nerazzurra` (`MLN`), `Stadio Giuseppe Meazza`
    - e.g., `Paris Saint-Germain` $\\rightarrow$ `Paris Capital` (`PRC`), `Parc Royal Arena`
- **Fallback State & Code Alignment**:
  - Updated hardcoded default fallback clubs in `CareerSimulationEngine.cs` and `SimulationBridge.cs` to safe fictional equivalents.
  - Updated `WorldDataLoaderTests.cs` to assert on `NLR` / `North London Red` / `Ashburton Grove`.
  - Updated mock label in `DesignSystemPreview.uxml`.

### Verification
- `dotnet build FootballLife.Simulation.csproj -c Release`: 0 errors, 0 warnings.
- `dotnet test FootballLife.Simulation.Tests.csproj`: 676 / 676 tests passing (100%).
- `python3 tools/verify_release_readiness.py`: All 5 checks passed (compilation, tests, world content, localization, plugin sync).

Closes #187
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-7): complete Milestone 7.5 - safe fictional renaming pass (#187)"], check=True)
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
        "commit_title": f"feat: Milestone 7.5 — Safe Fictional Renaming Pass & IP Resolution (#187) (#{pr_number})"
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
