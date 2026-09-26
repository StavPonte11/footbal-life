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
            "id": "#P7-601",
            "title": "#P7-601 Onboarding & FTUE Pass: Gate 7.0 Friction Resolution, Goal Framing & Replay",
            "body": """### Summary
Refine the First-Time User Experience (FTUE) and 7-step onboarding tutorial specifically addressing Gate 7.0 findings:
- Prevent mid-season drop-off by framing clear introductory goals (e.g. "Starting XI Target: Reach 60 Manager Trust").
- Provide tutorial replay capability from Profile / Settings screen.
- Ensure graceful tutorial skip handling with default starter kit allocation.

### Acceptance Criteria
- Clear objective framing and energy management callout during onboarding.
- Tutorial replay trigger accessible in Profile/Settings view.
- Skip confirmation dialog preserving all initial starter rewards.
""",
            "labels": ["ux", "onboarding", "phase-7", "milestone-7.6"]
        },
        {
            "id": "#P7-602",
            "title": "#P7-602 Universal Empty, Loading & Error State System Across All Screens",
            "body": """### Summary
Implement a cohesive empty-state, loading-state, and error-state presentation framework across CareerHub, Transfer Window, Sponsorship, Lifestyle, Trophies, and Cloud Save:
- Contextual empty states with thematic illustrations/icons, explanatory text, and primary actionable buttons.
- Offline and cloud save failure banners with retry options.
- Zero-state fallbacks for transfer offers, active sponsors, and media headlines.

### Acceptance Criteria
- Reusable empty-state visual element template integrated across all tabs.
- Actionable recovery buttons on error/empty states.
- Cloud save offline/conflict resolution error banner with manual retry trigger.
""",
            "labels": ["ui", "ux", "phase-7", "milestone-7.6"]
        },
        {
            "id": "#P7-603",
            "title": "#P7-603 Mobile Accessibility Pass: Touch Targets (44pt), Color-Blind Safe Mode & Typography",
            "body": """### Summary
Conduct an accessibility audit and implementation pass ensuring mobile compliance:
- Minimum 44x44pt touch-target size on all interactive buttons, tabs, and action links.
- Color-blind safe palette tokens (Deuteranopia / Protanopia friendly high-contrast modes) preventing emerald/gold confusion.
- Scalable typography with high-contrast text tokens.

### Acceptance Criteria
- All buttons and touchable elements meet or exceed minimum 44px height and width bounds.
- Color-blind safe token overrides in `tokens.uss` with symbol/shape accompaniments on status meters.
- High-contrast text legibility verified on dark surfaces.
""",
            "labels": ["accessibility", "ui", "phase-7", "milestone-7.6"]
        },
        {
            "id": "#P7-604",
            "title": "#P7-604 Thumb Ergonomics & One-Handed Reachability on 6.7\"+ Mobile Form Factors",
            "body": """### Summary
Review and optimize one-handed thumb ergonomics for modern large-screen mobile devices (6.7"+ displays beyond the 390x844 reference frame):
- Anchor primary actions ("Advance Day", "Confirm", "Play Match", "Next Moment") within the natural lower thumb zone.
- Standardize bottom action bars (`.bottom-action-bar`) and sticky primary action areas.

### Acceptance Criteria
- Primary match and hub actions accessible within bottom 35% screen height.
- Sticky bottom action containers with comfortable margin for system home indicators.
- Verified ergonomic reachability without requiring two-handed top-screen reach.
""",
            "labels": ["ux", "ergonomics", "phase-7", "milestone-7.6"]
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

    with open("tools/milestone_76_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.6 issues recorded in tools/milestone_76_issues.json")

if __name__ == "__main__":
    main()
