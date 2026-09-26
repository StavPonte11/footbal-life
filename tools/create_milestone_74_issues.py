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
            "id": "#P7-401",
            "title": "#P7-401 Stadium Grandstands: Tiered Seating, Dugouts & Reputation Tier Scaling",
            "body": """### Summary
Implement procedural 3D stadium grandstand architecture with tiered seating geometry, team dugouts with technical boxes, and cantilever roof trusses, dynamically scaled according to the home club's reputation tier (Lower-League Terrace, Mid-Tier Ground, Elite Multi-Tier Arena).

### Acceptance Criteria
- Tiered grandstand meshes generated around the pitch with concrete terracing and club-tinted seating blocks.
- Scaling behavior based on club reputation:
  - Grassroots / Lower League (Reputation <= 40): Open terrace, single low tier, compact bench dugouts.
  - Mid-Tier Club (Reputation 41-74): Covered single-tier 4-side stands, corrugated cantilever roofs, full team dugouts.
  - Elite Contender (Reputation >= 75): Double-tier bowl with executive suite ribbon, sweeping trussed roof, luxury dugouts, corner floodlight towers.
- Clean integration with `PitchBuilder.cs` and runtime match scene setup with zero GC allocations in match loop.
""",
            "labels": ["stadium", "art", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-402",
            "title": "#P7-402 Dynamic Mobile-Budget Crowd Spectator System & Reactive Animations",
            "body": """### Summary
Implement a high-performance procedural crowd system that populates stadium grandstands within mobile performance budgets (<200MB RAM, 60 FPS target), featuring club kit color palettes and reactive match animations (idle murmur/sway, goal roar celebration, near-miss gasp).

### Acceptance Criteria
- Batched spectator seating geometry/quads avoiding thousands of individual GameObjects to stay strictly within mobile memory/draw-call budgets.
- Fan clothing palettes tinting with home/away team colors and spectator variation.
- Dynamic crowd excitement states triggered by match events:
  - Idle ambient breathing/sway.
  - Goal roar eruption on `GoalTrigger.OnGoalScored`.
  - Woodwork clang / near-miss gasp reaction on post/crossbar hit.
- Zero GC allocations during update frames.
""",
            "labels": ["crowd", "art", "performance", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-403",
            "title": "#P7-403 Match VFX Pass: Goal-Net Physics Ripple, Turf Dust Particles & Ball Trail Polish",
            "body": """### Summary
Deliver a match visual effects pass comprising goal-net ripple reaction physics upon ball entry, pooled turf dust particle bursts on slide tackles and shots, and velocity-scaled ball trail rendering.

### Acceptance Criteria
- Goal-net ripple reaction: dynamic impulse propagation across net geometry when the ball strikes `GoalTrigger` or net mesh.
- Turf dust & grass blade particle effects using `UnityEngine.Pool.ObjectPool` for zero-GC spawning on kicks and slide tackles.
- Velocity-scaled ball trail renderer with dynamic width tapering, alpha fade, and gold/white highlight aesthetics.
- 100% test passing across the test suite.
""",
            "labels": ["vfx", "phase-7", "gameplay"]
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

    with open("tools/milestone_74_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.4 issues recorded in tools/milestone_74_issues.json")

if __name__ == "__main__":
    main()
