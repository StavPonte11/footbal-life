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
            "id": "#P7-901",
            "title": "#P7-901 Closed Beta Cohort Enrollment & Invitation Code Verification System",
            "body": """### Summary
Establish the closed beta enrollment architecture and cohort segmentation for external testers:
- Support for segmented beta cohorts: `ExternalPioneers`, `MobileGamers`, `FootballSimFans`, and `CommunityVIP`.
- Pure C# `BetaEnrollmentService` validating invitation access passes, quotas, and expiration dates.
- Comprehensive closed beta program recruitment and operation plan (`docs/beta/BETA_PROGRAM_PLAN.md`).

### Acceptance Criteria
- Domain model for beta cohorts, access passes, and enrollment status.
- Simulation service validating codes, assigning cohorts, and checking expiration.
- Recruitment and distribution plan for TestFlight and Google Play Internal Testing.
""",
            "labels": ["product", "beta", "phase-7", "milestone-7.9"]
        },
        {
            "id": "#P7-902",
            "title": "#P7-902 In-App Feedback Collection & Diagnostic Reporting Pipeline",
            "body": """### Summary
Implement a frictionless, in-app feedback and bug reporting pipeline so beta testers can submit issues directly without leaving the game:
- Domain model `BetaFeedbackReport` capturing category, satisfaction rating, user comment, device context, and recent breadcrumbs.
- `BetaFeedbackService` managing local disk caching, offline queueing, and dispatch.
- Integration in `SimulationBridge` for UI invocation and automated breadcrumb bundling.

### Acceptance Criteria
- In-game feedback submission pipeline with categories (Bug, Balance, UI/UX, Suggestion).
- Automatic bundling of diagnostic context and breadcrumbs from CrashDiagnosticService.
- Offline disk storage and export capabilities for gathered reports.
""",
            "labels": ["ui", "feedback", "phase-7", "milestone-7.9"]
        },
        {
            "id": "#P7-903",
            "title": "#P7-903 Beta Success Metrics & Telemetry KPI Evaluation Framework",
            "body": """### Summary
Define clear, upfront quantitative success metrics for release readiness and implement an automated evaluator:
- Targets: D1 Retention (>= 45%), D7 Retention (>= 20%), Median Session Length (8-12m), Season 1 Completion (>= 35%), Crash-Free Session Rate (>= 99.5%).
- Domain models `BetaKpiTargets` and `BetaCohortMetricsSnapshot`.
- `BetaMetricsEvaluator` calculating retention, session stats, and completion rates from telemetry data.
- Upfront KPI reference documentation (`docs/beta/BETA_SUCCESS_METRICS.md`).

### Acceptance Criteria
- Upfront target metrics documented and agreed before receiving external data.
- In-engine metrics evaluator scoring beta datasets against target benchmarks.
- Automated tests verifying scoring logic across pass, warning, and failure scenarios.
""",
            "labels": ["product", "analytics", "phase-7", "milestone-7.9"]
        },
        {
            "id": "#P7-904",
            "title": "#P7-904 Beta Retrospective Framework & Release Go/No-Go Decision Matrix",
            "body": """### Summary
Build a formal, multi-pillar Go/No-Go release gate decision framework to objectively evaluate public launch readiness:
- Evaluates 6 pillars: Stability (crashes), Retention (D1/D7), Progression (Season 1 completion), Economy (inflation/fairness), Compliance (stores), Sentiment (feedback scores).
- Domain models `GoNoGoDecision` and `ReleaseGateReport`.
- `BetaGoNoGoEvaluator` in Simulation generating release recommendations (`GoForGlobalLaunch`, `ConditionalGoBeta2`, `NoGoBlocker`).
- Formal retrospective decision matrix documentation (`docs/beta/BETA_GO_NO_GO_TEMPLATE.md`).

### Acceptance Criteria
- Multi-pillar Go/No-Go decision matrix evaluated by pure C# simulation system.
- Comprehensive retrospective documentation template and launch criteria.
- Unit tests verifying correct release decisions across various health profiles.
""",
            "labels": ["product", "release", "phase-7", "milestone-7.9"]
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

    with open("tools/milestone_79_issues.json", "w") as f:
        json.dump(created_issues, f, indent=2)

    print("Milestone 7.9 issues recorded in tools/milestone_79_issues.json")

if __name__ == "__main__":
    main()
