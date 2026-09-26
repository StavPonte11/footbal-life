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
            "id": "#P7-201",
            "title": "#P7-201 Mecanim Animator Controller & Locomotion BlendTree",
            "body": """### Summary
Implement a high-performance Mecanim Animator Controller with a Locomotion BlendTree (Idle <-> Jog <-> Sprint parameterized by normalized MoveSpeed) replacing pure procedural sway.

### Acceptance Criteria
- Parameterized locomotion blend tree responding to speed parameter without GC allocations.
- Seamless transitions between Idle, Jog, and Sprint.
- Zero per-frame allocations with hashed parameter lookups.
""",
            "labels": ["animation", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-202",
            "title": "#P7-202 Action Clip Set (Power Shot, Finesse Curl, Header, Slide, GK Save)",
            "body": """### Summary
Author and integrate realistic football action animation clips for power shooting, finesse curling, diving goalkeeper saves, headers, and slide tackles.

### Acceptance Criteria
- Power shot clip with athletic wind-up, planting foot, strike contact, and dynamic follow-through.
- Finesse curl clip with inside-of-foot wrap and body lean.
- Goalkeeper diving save clips (left, right, aerial deflection).
- Synchronized impact callback events on ball strike frames.
""",
            "labels": ["animation", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-203",
            "title": "#P7-203 Celebration Clip Set (Knee Slide, Fist Pump, Crowd Wave)",
            "body": """### Summary
Author and integrate post-goal celebration clips (knee slide across turf, energetic fist pump, and two-handed crowd wave) triggered on goal scored.

### Acceptance Criteria
- Distinct goal celebration animations with natural athletic transitions.
- Automatic trigger upon goal confirmation from `ShootingInteraction` / `GoalTrigger`.
""",
            "labels": ["animation", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-204",
            "title": "#P7-204 Retirement of Legacy Procedural Rotations in Favor of Clip-Driven Animation",
            "body": """### Summary
Safely deprecate and retire old procedural trigonometric bone rotation methods in `PlayerPawnController.cs` and `GoalkeeperController.cs` in favor of clip-driven Mecanim animation, maintaining headless test compatibility.

### Acceptance Criteria
- Clip-driven animation cleanly supersedes old code-driven transforms.
- Fallback mechanism retained for headless/batch test environments where Animator is unavailable.
- All unit and simulation tests continue to pass 100%.
""",
            "labels": ["animation", "phase-7", "refactor"]
        }
    ]

    created_issues = {}

    for item in issues_to_create:
        print(f"Creating issue: {item['title']}...")
        payload = {
            "title": item["title"],
            "body": item["body"],
            "labels": item["labels"]
        }
        req = urllib.request.Request(
            f"https://api.github.com/repos/{repo}/issues",
            data=json.dumps(payload).encode("utf-8"),
            headers=headers,
            method="POST"
        )
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                num = data["number"]
                created_issues[item["id"]] = num
                print(f"Created Issue #{num}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"Failed to create issue: {e.code} - {e.read().decode('utf-8')}")

    with open("tools/milestone_72_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.2 issues recorded in tools/milestone_72_issues.json")

if __name__ == "__main__":
    main()
