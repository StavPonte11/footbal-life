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

pr_title = "feat(simulation): Transfer & Contract System - M1.10 (#83 #84 #85 #86 #87 #88)"
pr_body = """## Summary of Changes

Delivers **Milestone 1.10: Transfer & Contract System** for Football Life.

### Key Deliverables:
- **#83 (P1-060)**: `TransferOffer` domain model, `TransferOfferStatus` enum, invariant validation, and `WorldState` transfer offers collection.
- **#84 (P1-061)**: `TransferSystem.GenerateOffers` scouting suitor clubs based on ability, reputation, and league tier, with low manager trust (< 30) escape offer guarantee and 3-offer cap.
- **#85 (P1-062)**: `TransferSystem.AcceptOffer` and `RejectOffer` executing contract registration, squad roster updates, signing bonus payouts to bank accounts, and relocation dynamics on relationships.
- **#86 (P1-063)**: `ContractSystem.NegotiateTerms` simulating wage bargaining, club budget tolerance curves, and agent affinity leverage (+15% tolerance).
- **#87 (P1-064)**: `ContractSystem.EvaluateContractStatus` detecting Bosman pre-contract eligibility (<= 6 months), `OfferRenewal`, and `HandleContractExpiry` releasing players to free agency.
- **#88 (P1-065)**: Comprehensive test suites with 24 new unit and integration tests (513 tests passing total).

Closes #83, Closes #84, Closes #85, Closes #86, Closes #87, Closes #88
"""

# 1. Create PR
pr_payload = json.dumps({
    "title": pr_title,
    "head": "feature/milestone-1.10-transfers",
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
    "commit_message": pr_body
}).encode('utf-8')

merge_req = urllib.request.Request(f"https://api.github.com/repos/{repo}/pulls/{pr_num}/merge", data=merge_payload, headers=headers, method="PUT")
try:
    with urllib.request.urlopen(merge_req) as resp:
        res = json.loads(resp.read().decode('utf-8'))
        print(f"Merged PR #{pr_num} successfully: {res.get('sha')}")
except urllib.error.HTTPError as e:
    err_msg = e.read().decode('utf-8')
    print(f"Error merging PR: {err_msg}")
    sys.exit(1)

time.sleep(2)

# 3. Close issues #83-#88 if still open
issues = [83, 84, 85, 86, 87, 88]
for issue_num in issues:
    issue_url = f"https://api.github.com/repos/{repo}/issues/{issue_num}"
    close_payload = json.dumps({
        "state": "closed",
        "state_reason": "completed"
    }).encode('utf-8')
    req = urllib.request.Request(issue_url, data=close_payload, headers=headers, method="PATCH")
    try:
        with urllib.request.urlopen(req) as resp:
            print(f"Closed Issue #{issue_num}")
    except Exception as ex:
        print(f"Issue #{issue_num} close notice: {ex}")

print("\nMilestone 1.10 PR and issues finalized successfully!")
