# -*- coding: utf-8 -*-
import os, sys, json, urllib.request, urllib.error, subprocess
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

def get_token():
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GITHUB_PERSONAL_ACCESS_TOKEN")
    if token: return token
    try:
        proc = subprocess.run(["git","credential","fill"], input="protocol=https\nhost=github.com\n",
                              text=True, capture_output=True, check=True)
        for line in proc.stdout.splitlines():
            if line.startswith("password="): return line.split("=",1)[1].strip()
    except: pass
    return None

token = get_token()
repo = "StavPonte11/footbal-life"
headers = {
    "Authorization": f"Bearer {token}",
    "User-Agent": "FootballLife-IssueChecker",
    "Accept": "application/vnd.github.v3+json",
}

# List open issues
req = urllib.request.Request(
    f"https://api.github.com/repos/{repo}/issues?state=open&per_page=50&sort=created&direction=asc",
    headers=headers)
with urllib.request.urlopen(req) as resp:
    issues = json.loads(resp.read().decode("utf-8"))

print(f"{'#':>5}  {'Title':<70}  Labels")
print("-" * 110)
for issue in issues:
    labels = ", ".join(l["name"] for l in issue.get("labels", []))
    print(f"#{issue['number']:>4}  {issue['title'][:70]:<70}  {labels}")

print(f"\nTotal open: {len(issues)}")

# Close duplicate issues 110-113 (created by mistake)
duplicates = [110, 111, 112, 113]
print("\nClosing duplicate issues...")
for num in duplicates:
    payload = json.dumps({"state": "closed", "state_reason": "duplicate"}).encode("utf-8")
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues/{num}",
        data=payload, headers={**headers, "Content-Type": "application/json"}, method="PATCH")
    try:
        with urllib.request.urlopen(req) as r:
            print(f"  Closed #{num}")
    except Exception as e:
        print(f"  Error #{num}: {e}")
