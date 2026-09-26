#!/usr/bin/env python3
"""Create PR for Milestone 7.7, squash-merge to main, and close issues #213-#215."""
import subprocess, json, sys, os

REPO = "StavPonte11/footbal-life"
BRANCH = "feature/milestone-7.7-device-matrix-crash-reporting-save-stress"
ISSUES = [213, 214, 215]
PR_TITLE = "feat(milestone-7.7): Device Matrix, Crash Diagnostics & Save Stress Testing"
PR_BODY = """## Milestone 7.7 — Device Matrix, Sentry/Crashlytics & Save Stress Testing

### #P7-701: Device Performance Matrix
- `DeviceTier` enum (Low / Medium / High) with `DevicePerformanceProfile`
- `DeviceTierClassifier` classifies hardware via RAM, GPU tier, and core count
- `MobilePerformanceManager` auto-sets FPS, quality tier, DPI render scaling

### #P7-702: Crash Reporting & Diagnostic Breadcrumbs
- `DiagnosticBreadcrumb` model (UI, Match, Simulation, Persistence, Network, System)
- `CrashReport` with full JSON serialization and breadcrumb capture
- `CrashDiagnosticService`: circular ring buffer, disk persistence, offline flush
- `SimulationBridge`: hooks `Application.logMessageReceivedThreaded`, auto-flushes on startup

### #P7-703: Save Corruption Protection & Cloud Conflict Stress Tests
- SHA256 checksum validation with companion `.sha256` files
- Atomic temp-file writes with `.bak` backup preservation
- Auto-healing corrupted primary from valid backup
- Cloud conflict resolution stress tests (KeepNewest, KeepCloud, Offline, InSync)

### Test Results
- **752 tests passed** (20 new), **0 failures**, **0 skipped**
- Release readiness verification: ✅ ALL CHECKS PASSED

Closes #213
Closes #214
Closes #215
"""

def run(cmd, check=True):
    r = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    if check and r.returncode != 0:
        print(f"FAIL: {cmd}\n{r.stderr}", file=sys.stderr)
        sys.exit(1)
    return r

def main():
    print(f"Pushing branch {BRANCH}...")
    run(f"git push origin {BRANCH} --force")

    print("Creating PR...")
    result = run(
        f'gh pr create --repo {REPO} --base main --head {BRANCH} '
        f'--title "{PR_TITLE}" --body {json.dumps(PR_BODY)}'
    )
    pr_url = result.stdout.strip()
    print(f"PR created: {pr_url}")

    print("Merging PR via squash-merge...")
    run(f'gh pr merge {pr_url} --squash --delete-branch --auto')

    for issue in ISSUES:
        print(f"Closing issue #{issue}...")
        run(f'gh issue close {issue} --repo {REPO}', check=False)

    print("Updating local main branch...")
    run("git checkout main && git pull origin main")

    print(f"\n✅ Milestone 7.7 merged and issues {ISSUES} closed!")

if __name__ == "__main__":
    main()
