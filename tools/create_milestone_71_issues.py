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
            "id": "#P7-101",
            "title": "#P7-101 Base Humanoid Mesh & Rig (Outfield & GK with Standard Bone Hierarchy)",
            "body": """### Summary
Design and build a clean, stylized low-poly Humanoid mesh and standard bone hierarchy (Hips, Spine, Chest, Neck, Head, Shoulders, UpperArms, LowerArms, Hands, UpperLegs, LowerLegs, Feet, Toes) compatible with Unity Humanoid Avatars and Mecanim, supporting outfield players and goalkeeper gear.

### Acceptance Criteria
- Full humanoid anatomical bone hierarchy with proper pivots for Mecanim compatibility.
- Outfield and Goalkeeper mesh variants with goalkeeper gloves, padded shorts, and athletic silhouettes.
- Mobile-optimized polygon budget (< 2,500 triangles per player pawn).
- Clean skinned/rigged transform mapping accessible by animation and procedural systems.
""",
            "labels": ["art", "phase-7", "gameplay"]
        },
        {
            "id": "#P7-102",
            "title": "#P7-102 Modular Face, Hair & Skin Tone Variation System",
            "body": """### Summary
Implement a modular player appearance variation system providing diverse skin tones, hair models/styles (Buzz, ShortFade, Afro, LongTied, Undercut), hair colors, facial hair, and brow styles, replacing primitive head spheres.

### Acceptance Criteria
- Modular head builder generating distinct player visual identities from player seed or configuration.
- Broad skin tone palette (Fair, Olive, Tan, Rich Bronze, Deep Melanin) and authentic hair colors.
- Multiple stylized 3D hair meshes that mount cleanly to head socket transform.
- Deterministic appearance generation based on player name/ID for consistent presentation across matches.
""",
            "labels": ["art", "phase-7", "ui-ux"]
        },
        {
            "id": "#P7-103",
            "title": "#P7-103 Kit & Appearance System Compatibility Pass",
            "body": """### Summary
Verify and adapt procedural club-color material application (primary jersey, secondary accent, shorts, socks, numbers) to work seamlessly with the new stylized humanoid mesh and appearance system.

### Acceptance Criteria
- Procedural kit application works seamlessly across Home, Away, and Goalkeeper kits.
- Dynamic squad number rendering / mapping onto jersey back.
- Compatibility maintained with existing `PawnKitScheme`, `SituationPawnPresenter`, and match gameplay controls.
""",
            "labels": ["art", "phase-7", "gameplay"]
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

    with open("tools/milestone_71_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.1 issues recorded in tools/milestone_71_issues.json")

if __name__ == "__main__":
    main()
