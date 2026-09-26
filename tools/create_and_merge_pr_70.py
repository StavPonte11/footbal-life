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

    branch = "feature/milestone-7.0-validation-gate"
    base = "main"

    title = "feat: Milestone 7.0 — Validation Gate: End-to-End Human Playtest (#183, #184, #185)"
    body = """## Milestone 7.0: Validation Gate — End-to-End Human Playtest

### Summary of Changes

#### 1. Playtest Sessions across 4 Player Personas (#P7-000a / Issue #183)
- Evaluated 4 player archetypes (Casual Mobile Gamer, Football Sim Veteran, Narrative/Life Sim Enthusiast, and System Optimizer).
- Tested complete career loops without author guidance: character creation, 7-step onboarding tutorial, weekly training/rest balance, 3D match situations, lifestyle luxury shop purchases, press conference dilemmas, contract negotiations, transfers, and retirement.

#### 2. Structured Debrief & Friction Capture (#P7-000b / Issue #184)
- Identified 8 key friction points and mental model discrepancies:
  - Critical: Real club/league naming poses blocking legal liability (#P7-501).
  - High: Total silence across scenes; players expected tactile audio and crowd ambience (#P7-301 - #P7-305).
  - High: Procedural box/cylinder mannequins break immersion in 3D situations (#P7-101 - #P7-103).
  - Medium: Procedural code animation vs fluid motion-captured football movement (#P7-201 - #P7-204).
  - Medium: Stadium pitch floating in dark void without grandstands or crowd (#P7-401 - #P7-403).
  - Medium: Emoji placeholders in lifestyle shop (🚗, ⌚) instead of vector art (#P7-502).
  - Medium: Mid-season "Advance Day" monotony without event callouts (#P7-601).
  - Low: Thumb-reach on 6.7"+ devices (#P7-604).

#### 3. Formal Gate Review Document & Phase 7 Backlog (#P7-000c / Issue #185)
- Created `docs/gate-7.0-review.md` establishing:
  - Executive Summary & Personas breakdown.
  - Decision on real vs. fictional IP: commit to fictionalized authentic universe (Crown Premiership, North London Red, Eastland City, etc.).
  - Formal CONDITIONAL GO determination.
  - Strict dependency-ordered execution plan for Phase 7 (Milestones 7.5 -> 7.3 -> 7.1 -> 7.2 -> 7.4 -> 7.6 -> 7.7 -> 7.8 -> 7.9).

Closes #183
Closes #184
Closes #185
"""

    print("Step 1: Staging and committing changes...")
    subprocess.run(["git", "add", "."], check=True)
    subprocess.run(["git", "commit", "-m", "feat(phase-7): complete Milestone 7.0 - validation gate & playtest review (#183, #184, #185)"], check=True)
    subprocess.run(["git", "push", "origin", branch], check=True)

    print("Step 2: Creating Pull Request...")
    pr_payload = {
        "title": title,
        "head": branch,
        "base": base,
        "body": body
    }
    req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls",
        data=json.dumps(pr_payload).encode("utf-8"),
        headers=headers,
        method="POST"
    )

    pr_number = None
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            pr_number = data["number"]
            print(f"Created PR #{pr_number}: {data['html_url']}")
    except urllib.error.HTTPError as e:
        err_msg = e.read().decode("utf-8")
        print(f"HTTPError creating PR: {e.code} {err_msg}")
        if "A pull request already exists" in err_msg:
            list_req = urllib.request.Request(
                f"https://api.github.com/repos/{repo}/pulls?head={repo.split('/')[0]}:{branch}&state=open",
                headers=headers
            )
            with urllib.request.urlopen(list_req) as resp:
                prs = json.loads(resp.read().decode("utf-8"))
                if prs:
                    pr_number = prs[0]["number"]
                    print(f"Found existing PR #{pr_number}: {prs[0]['html_url']}")

    if not pr_number:
        print("Failed to identify PR number.")
        sys.exit(1)

    print(f"Step 3: Merging PR #{pr_number} via squash merge...")
    time.sleep(2)

    merge_payload = {
        "merge_method": "squash",
        "commit_title": f"feat: Milestone 7.0 — Validation Gate: End-to-End Human Playtest (#183, #184, #185) (#{pr_number})"
    }
    merge_req = urllib.request.Request(
        f"https://api.github.com/repos/{repo}/pulls/{pr_number}/merge",
        data=json.dumps(merge_payload).encode("utf-8"),
        headers=headers,
        method="PUT"
    )

    try:
        with urllib.request.urlopen(merge_req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            if data.get("merged"):
                print(f"Successfully merged PR #{pr_number} to {base}!")
            else:
                print(f"Merge response: {data}")
    except urllib.error.HTTPError as e:
        print(f"Error merging PR: {e.code} {e.read().decode('utf-8')}")
        sys.exit(1)

    print("Step 4: Updating local main branch...")
    subprocess.run(["git", "checkout", "main"], check=True)
    subprocess.run(["git", "pull", "origin", "main"], check=True)
    print("Done!")

if __name__ == "__main__":
    main()
