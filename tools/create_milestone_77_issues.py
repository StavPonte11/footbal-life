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
            "id": "#P7-701",
            "title": "#P7-701 Device Matrix & Real-Device Performance Profiles (Low, Mid, High Tiers)",
            "body": """### Summary
Establish a concrete mobile device compatibility matrix and adaptive device-performance profiles (Low, Mid, High tiers) to validate the <200MB RAM budget and target frame rates (30fps on low-end, 60fps on mid/high-end):
- Device Tier Classifier based on system memory, CPU cores, and GPU capabilities.
- Dynamic performance profile scaling: crowd spectator density, shadow cascades, render scale, and particle pool limits.
- Automated memory and framerate profiling test harness verifying strict compliance under load.

### Acceptance Criteria
- Device performance classifier categorizing devices into Low, Mid, High tiers.
- Quality settings auto-applied on startup according to device tier.
- Automated simulation/stress tests verifying peak memory budget remains within target.
""",
            "labels": ["performance", "unity", "phase-7", "milestone-7.7"]
        },
        {
            "id": "#P7-702",
            "title": "#P7-702 Crash, Unhandled Exception & Diagnostic Breadcrumb Reporting Service",
            "body": """### Summary
Implement an in-engine crash, unhandled exception, and diagnostic reporting service (Sentry / Crashlytics / local persistent crash dump pipeline) to guarantee zero silent failures during closed beta:
- Log callback and unhandled exception hook capturing stack traces, device context, and memory snapshot.
- Rolling breadcrumb ring buffer recording user actions, navigation transitions, and simulation ticks prior to error.
- Offline crash cache persisting unsent reports to disk and flushing on next online session launch.

### Acceptance Criteria
- Global unhandled exception hook active across all scenes.
- Rolling breadcrumb tracker recording user flow, match events, and navigation steps.
- Crash report serialization with offline disk persistence and retry delivery.
""",
            "labels": ["telemetry", "crash-reporting", "phase-7", "milestone-7.7"]
        },
        {
            "id": "#P7-703",
            "title": "#P7-703 Save Corruption Protection, Atomic Writes & Cloud Conflict Stress Test Pass",
            "body": """### Summary
Harden career save data against real-world mobile failure modes (mid-write OS kill, storage exhaustion, file corruption, multi-device cloud divergence):
- Atomic file writes using temporary files (`.tmp`) and checksum validation before committing replacement.
- Automatic corrupt save recovery restoring from `.bak` backup files with telemetry alert.
- Comprehensive automated test suite simulating mid-write interruptions, bit corruption, and cloud timestamp conflicts.

### Acceptance Criteria
- SaveManager atomic write mechanism with integrity verification and rolling backup.
- Automated test suite asserting recovery from corrupted save files without crash.
- Deterministic conflict resolution tested under concurrent multi-device clock skew.
""",
            "labels": ["persistence", "qa", "phase-7", "milestone-7.7"]
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

    with open("tools/milestone_77_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.7 issues recorded in tools/milestone_77_issues.json")

if __name__ == "__main__":
    main()
