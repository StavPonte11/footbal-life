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
            "title": "#P7-000a: Milestone 7.0 — Validation Gate: End-to-End Human Playtest Sessions",
            "body": """### User Story
3–5 people outside the build's author(s) play a full career session (creation → several seasons → at least one transfer, one life event, one retirement path if time allows) on the current build with no guidance given.

### Acceptance Criteria
- Run structured playtest sessions across varied player backgrounds (casual mobile gamer, football sim veteran, narrative RPG player).
- Track user progression through player creation, onboarding tutorial, weekly match loops, lifestyle purchases, contract negotiation, and transfer window.
- Observe unprompted interaction, friction points, confusion, and engagement falloff points.

Part of Phase 7 Milestone 7.0 Validation Gate.""",
            "labels": ["quality", "phase-7"]
        },
        {
            "title": "#P7-000b: Milestone 7.0 — Structured Debrief & Friction Capture",
            "body": """### User Story
Conduct structured post-playtest debriefs to pinpoint where players got confused, bored, stuck, or quit, and identify discrepancies between what players *thought* a screen/button did vs. what it actually did.

### Acceptance Criteria
- Collect qualitative feedback across key friction dimensions:
  1. Onboarding clarity & first-session retention.
  2. 3D Match situation readability & touch controls responsiveness.
  3. Weekly loop pacing (advance day vs manual training/rest).
  4. Career decision agency (transfers, contracts, lifestyle spending).
  5. UI affordances & button expectations.
- Catalog all misunderstandings and friction areas with severity ratings.

Part of Phase 7 Milestone 7.0 Validation Gate.""",
            "labels": ["quality", "phase-7"]
        },
        {
            "title": "#P7-000c: Milestone 7.0 — Gate 7.0 Review Document & Ranked Phase 7 Backlog",
            "body": """### User Story
Synthesize playtest findings into `docs/gate-7.0-review.md` following the formal gate review structure (Executive Summary, Detailed Playtest Observations, Evaluation against Fantasy & Agency, Benchmarks, GO/NO-GO determination, and Ranked Friction Backlog prioritizing Milestones 7.1–7.8).

### Acceptance Criteria
- Complete `docs/gate-7.0-review.md` with explicit GO/NO-GO decision.
- Rank friction items from Playtest Sessions and map them directly to Phase 7 Milestones (Art, Animation, Audio, Stadium, Content/Legal IP, UX Flow, Device QA, Compliance).
- Provide clear execution order for Phase 7 implementation.

Part of Phase 7 Milestone 7.0 Validation Gate.""",
            "labels": ["documentation", "phase-7"]
        }
    ]

    created_issues = {}

    for issue in issues_to_create:
        print(f"Creating issue: {issue['title']}...")
        payload = {
            "title": issue["title"],
            "body": issue["body"],
            "labels": issue.get("labels", [])
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
                created_issues[issue["title"]] = data["number"]
                print(f"Created #{data['number']}: {data['html_url']}")
        except urllib.error.HTTPError as e:
            print(f"Failed to create issue {issue['title']}: {e.code} {e.read().decode('utf-8')}")

    out_file = os.path.join(os.path.dirname(__file__), "milestone_70_issues.json")
    with open(out_file, "w", encoding="utf-8") as f:
        json.dump(created_issues, f, indent=2)
    print(f"Saved created issue numbers to {out_file}")

if __name__ == "__main__":
    main()
