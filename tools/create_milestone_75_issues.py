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

    issue_data = {
        "title": "#P7-501: Fictional Club & League Renaming Pass (IP-Safe Data & Content Resolution)",
        "body": """### User Story
Resolve real vs. fictional club/league naming across all data files, domain references, and tests before any external user, closed beta, or public testing. Replace trademarked club and league identities with authentic, culturally evocative fictional counterparts (in the tradition of Pro Evolution Soccer and New Star Soccer).

### Acceptance Criteria
- Rename all 11 leagues in `content/data/leagues.json` to legally safe names (e.g. Crown Premiership, Liga de Honor, Serie Campionato, Deutsche Liga A, Ligue Première, Liga Lusitana).
- Rename all 66 clubs and their home stadiums in `content/data/clubs.json` to fictional counterparts (e.g. North London Red, West London Blue, Merseyside Red, Eastland City, Manchester Red, Madrid Royal, Catalunya FC).
- Update unit tests and fallback references to match new names.
- Verify 100% test pass rate across `FootballLife.Simulation.Tests` and release readiness validation.

Part of Phase 7 Milestone 7.5 Content, Iconography & IP Resolution.""",
        "labels": ["content", "phase-7"]
    }

    print(f"Creating issue: {issue_data['title']}...")
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/issues",
        data=json.dumps(issue_data).encode("utf-8"),
        headers=headers,
        method="POST"
    )

    created_issues = {}
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            created_issues[issue_data["title"]] = data["number"]
            print(f"Created #{data['number']}: {data['html_url']}")
    except urllib.error.HTTPError as e:
        print(f"Failed to create issue: {e.code} {e.read().decode('utf-8')}")

    out_file = os.path.join(os.path.dirname(__file__), "milestone_75_issues.json")
    with open(out_file, "w", encoding="utf-8") as f:
        json.dump(created_issues, f, indent=2)
    print(f"Saved created issue numbers to {out_file}")

if __name__ == "__main__":
    main()
