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

headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-PRCreator",
    "Accept": "application/vnd.github.v3+json",
    "Content-Type": "application/json; charset=utf-8"
}

pr_title = "feat(simulation): Career Simulator CLI & 10k Career Balance - M1.11 (#90 #91 #92 #93)"
pr_body = """## Summary of Changes

Delivers **Milestone 1.11: Career Simulator CLI & 10,000 Career Balance Validation**, the culminating milestone of Phase 1 of Football Life.

### Key Deliverables:
- **#90 (P1-066)**: `CareerSimulationEngine` wiring all Phase 1 domain and simulation systems end-to-end into complete multi-season player careers (up to 20 seasons) with realistic training progression, match simulation, fatigue, manager trust, economy, contracts, transfers, and aging/retirement lifecycles.
- **#91 (P1-067)**: `CareerStatistics` and `SeasonRecord` models capturing per-career and seasonal snapshots with streaming CSV header/rows (`--csv`) and formatted JSON (`--json`) export support.
- **#92 (P1-068)**: `BulkSimulationRunner` high-throughput multi-career orchestrator (supporting sequential and parallel execution up to 10,000 careers at 60+ careers/sec), `AggregateReport` percentiles (P10, P50, P90, Mean, Min, Max, StdDev), and ASCII terminal histograms for Peak Overall and Retirement Age.
- **#93 (P1-069)**: Automated test fixtures (`CareerSimulatorTests.cs` and `CareerSimulatorBalanceTests.cs`) validating peak overall distributions (mean ~68-70, range 56-88, no teenagers hitting 90+ overall), retirement age (mean 34.4), career transfer frequency (mean ~4.3 transfers), low bankruptcy rate (< 1%), and bit-exact reproducibility under deterministic random seeds.

Closes #90, Closes #91, Closes #92, Closes #93
"""

# 1. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-1.11-career-simulator",
    "base": "main",
    "body": pr_body
}).encode('utf-8')

req = urllib.request.Request(f"https://api.github.com/repos/{repo}/pulls", data=pr_payload, headers=headers, method="POST")
pr_num = None
try:
    with urllib.request.urlopen(req) as resp:
        res = json.loads(resp.read().decode('utf-8'))
        pr_num = res["number"]
        print(f"Created PR #{pr_num}: {res['html_url']}")
except urllib.error.HTTPError as e:
    err_msg = e.read().decode('utf-8')
    print(f"Error creating PR: {err_msg}")
    sys.exit(1)

time.sleep(2)

# 2. Merge PR (squash)
merge_payload = json.dumps({
    "merge_method": "squash",
    "commit_title": f"{pr_title} (#{pr_num})",
    "commit_message": f"Squash merge PR #{pr_num} into main."
}).encode('utf-8')

merge_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/pulls/{pr_num}/merge", data=merge_payload, headers=headers, method="PUT")
try:
    with urllib.request.urlopen(merge_req) as resp:
        merge_res = json.loads(resp.read().decode('utf-8'))
        print(f"Merged PR #{pr_num}: {merge_res['message']} (SHA: {merge_res.get('sha')})")
except urllib.error.HTTPError as e:
    err_msg = e.read().decode('utf-8')
    print(f"Error merging PR #{pr_num}: {err_msg}")
    sys.exit(1)

time.sleep(1)

# 3. Close issues explicitly if not closed
issue_ids = [90, 91, 92, 93]
for iid in issue_ids:
    patch_payload = json.dumps({"state": "closed"}).encode('utf-8')
    patch_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/issues/{iid}", data=patch_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(patch_req) as resp:
            print(f"Closed Issue #{iid}")
    except Exception as e:
        print(f"Could not close issue #{iid}: {e}")
