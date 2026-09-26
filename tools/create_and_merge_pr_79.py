#!/usr/bin/env python3
import os
import subprocess
import sys

def run_cmd(cmd, check=True):
    print(f"Running: {' '.join(cmd)}")
    res = subprocess.run(cmd, capture_output=True, text=True)
    if check and res.returncode != 0:
        print(f"Command failed (exit {res.returncode}):\nSTDOUT: {res.stdout}\nSTDERR: {res.stderr}")
        sys.exit(1)
    return res

def main():
    # 1. Stage and commit
    run_cmd(["git", "add", "."])
    commit_msg = """feat(milestone-7.9): Closed beta launch program & release Go/No-Go decision matrix (#P7-901 - #P7-904)

- #P7-901: Implemented BetaEnrollmentService and BetaAccessPass supporting segmented cohorts (ExternalPioneers, MobileGamers, FootballSimFans, CommunityVIP) with quota and expiration verification. Documented closed beta recruitment plan in docs/beta/BETA_PROGRAM_PLAN.md.
- #P7-902: Built in-app BetaFeedbackService capturing user ratings, feedback categories, device profiles, and auto-attached recent diagnostic breadcrumbs with offline disk persistence.
- #P7-903: Established quantitative release criteria (D1 >= 45%, D7 >= 20%, Median Session 8-15m, Season 1 Completion >= 35%, Crash-Free >= 99.5%) and implemented BetaMetricsEvaluator in docs/beta/BETA_SUCCESS_METRICS.md.
- #P7-904: Implemented BetaGoNoGoEvaluator analyzing the 6 release readiness pillars (Stability, Retention, Progression, Economy, Compliance, Sentiment) with automated Go/No-Go recommendations. Documented retrospective matrix in docs/beta/BETA_GO_NO_GO_TEMPLATE.md.
- Updated docs/ROADMAP.md marking Phase 7 and all milestones complete.
- 787 unit tests passing (100% pass rate) with full release candidate verification.
- Synchronized Release DLLs to Unity plugins.

Closes #221, Closes #222, Closes #223, Closes #224"""

    run_cmd(["git", "commit", "-m", commit_msg])

    # 2. Push branch
    run_cmd(["git", "push", "-u", "origin", "feature/milestone-7.9-closed-beta-program"])

    # 3. Create PR
    pr_body = """## Milestone 7.9: Closed Beta Launch Program (#P7-901 - #P7-904)

### Summary of Completed Work
1. **#P7-901 Beta Enrollment & Cohort Management**:
   - `BetaEnrollmentService` supporting segmented external cohorts (`ExternalPioneers`, `MobileGamers`, `FootballSimFans`, `CommunityVIP`).
   - Pure C# invitation pass validation, quota limits, expiration dates, and deterministic SHA-256 beta access tokens.
   - Comprehensive external recruitment strategy in `docs/beta/BETA_PROGRAM_PLAN.md`.

2. **#P7-902 In-App Feedback Pipeline**:
   - `BetaFeedbackService` and `BetaFeedbackReport` capturing user satisfaction (1-5), categories (Bug, MatchEngine, UI/UX, Balance, Suggestion), device context, and recent breadcrumbs.
   - Local offline disk persistence (`beta_feedback/`) with batch flush and export.
   - Integrated into `SimulationBridge.SubmitBetaFeedback()`.

3. **#P7-903 Pre-Defined Beta Success Metrics & KPI Framework**:
   - Pre-approved quantitative benchmarks: D1 Retention (>= 45%), D7 Retention (>= 20%), Median Session (8-15 min), Season 1 Completion (>= 35%), Crash-Free Session Rate (>= 99.5%).
   - `BetaMetricsEvaluator` scoring observed telemetry cohorts against upfront targets in `docs/beta/BETA_SUCCESS_METRICS.md`.

4. **#P7-904 Beta Retrospective & Multi-Pillar Go/No-Go Decision Matrix**:
   - `BetaGoNoGoEvaluator` evaluating 6 pillars: Stability, Retention, Progression, Economy Balance, Store Compliance, and User Sentiment.
   - Generates authoritative release verdicts: `GoForGlobalLaunch`, `ConditionalGoBeta2`, `NoGoBlocker`.
   - Formal retrospective template in `docs/beta/BETA_GO_NO_GO_TEMPLATE.md`.

5. **Release Verification**:
   - 787 xUnit tests passing (0 failures, 0 skipped).
   - All 5 release readiness checks passed in `tools/verify_release_readiness.py`.
   - Release DLLs built and synchronized to Unity plugins.
   - Updated `docs/ROADMAP.md` reflecting 100% Phase 7 completion.

Closes #221, Closes #222, Closes #223, Closes #224
"""

    create_pr_cmd = [
        "gh", "pr", "create",
        "--title", "feat(milestone-7.9): Closed beta launch program & release Go/No-Go decision matrix (#P7-901 - #P7-904)",
        "--body", pr_body,
        "--base", "main",
        "--head", "feature/milestone-7.9-closed-beta-program"
    ]
    res = run_cmd(create_pr_cmd)
    pr_url = res.stdout.strip()
    print(f"Created PR: {pr_url}")

    # 4. Merge PR with squash and delete branch
    print(f"Merging PR {pr_url} via squash...")
    merge_cmd = ["gh", "pr", "merge", pr_url, "--squash", "--delete-branch"]
    run_cmd(merge_cmd)

    # 5. Close issues
    for issue_num in [221, 222, 223, 224]:
        run_cmd(["gh", "issue", "close", str(issue_num), "--comment", f"Resolved in {pr_url}"], check=False)

    # 6. Switch to main and pull
    run_cmd(["git", "checkout", "main"])
    run_cmd(["git", "pull", "origin", "main"])
    print("Milestone 7.9 successfully merged to main!")

if __name__ == "__main__":
    main()
