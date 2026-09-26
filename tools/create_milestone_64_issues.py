import os
import sys
import json
import urllib.request
import urllib.error
import subprocess

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

    issues_to_create = [
        {
            "title": "#P6-002: Mobile Performance Pass — 60 FPS, <200MB RAM, Zero-GC Hot Paths & Release Validation",
            "body": """## Summary
Conduct a rigorous mobile performance pass across Unity gameplay presentation and simulation hot paths:
1. Verify zero GC allocations in hot update loops (`Update`, `FixedUpdate`, match simulation ticks).
2. Validate mobile frame budgeting (60 FPS target, <16.6ms frame time), memory footprint (<200MB target), and mobile battery optimization (dynamic target frame rates).
3. Implement `PerformanceBudgetValidator` automated benchmarks in `FootballLife.Simulation.Tests` measuring simulation throughput, allocation limits, and GC generation spikes.
4. Establish standalone release build automation and verification scripts.
""",
            "labels": ["performance", "unity", "simulation", "phase-6", "milestone-6.4", "complexity:L"]
        }
    ]

    created_issues = {}

    for item in issues_to_create:
        print(f"Creating issue: {item['title']}...")
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(item).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created_issues[item['title']] = num
                print(f"Created #{num}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"HTTPError: {e.code} {e.read().decode('utf-8')}")

    with open("tools/milestone_64_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

if __name__ == "__main__":
    main()
