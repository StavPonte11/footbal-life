using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# quantitative metrics evaluator for closed beta validation (#P7-903).
    /// Assesses cohort retention, session engagement, career completion, crash rates, and ratings.
    /// </summary>
    public static class BetaMetricsEvaluator
    {
        public static BetaKpiReport EvaluateSnapshot(BetaCohortMetricsSnapshot snapshot, BetaKpiTargets? targets = null)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var t = targets ?? new BetaKpiTargets();

            var scorecards = new List<MetricScorecard>();
            bool meetsAll = true;

            // 1. D1 Retention
            var d1Status = GradeHigherIsBetter(snapshot.D1RetentionRate, t.TargetD1Retention, tolerance: 0.08f);
            if (d1Status == MetricEvaluationStatus.Fail) meetsAll = false;
            scorecards.Add(new MetricScorecard(
                MetricName: "Day 1 Retention",
                ActualValue: snapshot.D1RetentionRate,
                TargetValue: t.TargetD1Retention,
                Status: d1Status,
                EvaluationDetails: $"{snapshot.D1RetentionRate:P1} observed vs {t.TargetD1Retention:P1} target."
            ));

            // 2. D7 Retention
            var d7Status = GradeHigherIsBetter(snapshot.D7RetentionRate, t.TargetD7Retention, tolerance: 0.05f);
            if (d7Status == MetricEvaluationStatus.Fail) meetsAll = false;
            scorecards.Add(new MetricScorecard(
                MetricName: "Day 7 Retention",
                ActualValue: snapshot.D7RetentionRate,
                TargetValue: t.TargetD7Retention,
                Status: d7Status,
                EvaluationDetails: $"{snapshot.D7RetentionRate:P1} observed vs {t.TargetD7Retention:P1} target."
            ));

            // 3. Median Session Duration (minutes)
            MetricEvaluationStatus sessionStatus;
            string sessionDetails;
            if (snapshot.MedianSessionDurationMinutes >= t.MinMedianSessionMinutes &&
                snapshot.MedianSessionDurationMinutes <= t.MaxMedianSessionMinutes)
            {
                sessionStatus = MetricEvaluationStatus.Pass;
                sessionDetails = $"{snapshot.MedianSessionDurationMinutes:F1}m within optimal {t.MinMedianSessionMinutes:F0}-{t.MaxMedianSessionMinutes:F0}m window.";
            }
            else if (snapshot.MedianSessionDurationMinutes >= 5.0f && snapshot.MedianSessionDurationMinutes <= 20.0f)
            {
                sessionStatus = MetricEvaluationStatus.Warning;
                sessionDetails = $"{snapshot.MedianSessionDurationMinutes:F1}m marginally outside target window.";
            }
            else
            {
                sessionStatus = MetricEvaluationStatus.Fail;
                sessionDetails = $"{snapshot.MedianSessionDurationMinutes:F1}m outside acceptable engagement range.";
                meetsAll = false;
            }
            scorecards.Add(new MetricScorecard(
                MetricName: "Median Session Duration",
                ActualValue: snapshot.MedianSessionDurationMinutes,
                TargetValue: t.MinMedianSessionMinutes,
                Status: sessionStatus,
                EvaluationDetails: sessionDetails
            ));

            // 4. Season 1 Career Completion Rate
            var completionStatus = GradeHigherIsBetter(snapshot.Season1CompletionRate, t.TargetSeason1CompletionRate, tolerance: 0.10f);
            if (completionStatus == MetricEvaluationStatus.Fail) meetsAll = false;
            scorecards.Add(new MetricScorecard(
                MetricName: "Season 1 Completion Rate",
                ActualValue: snapshot.Season1CompletionRate,
                TargetValue: t.TargetSeason1CompletionRate,
                Status: completionStatus,
                EvaluationDetails: $"{snapshot.Season1CompletionRate:P1} finished Season 1 vs {t.TargetSeason1CompletionRate:P1} target."
            ));

            // 5. Crash-Free Session Rate
            var crashStatus = GradeHigherIsBetter(snapshot.CrashFreeSessionRate, t.TargetCrashFreeRate, tolerance: 0.01f);
            if (crashStatus == MetricEvaluationStatus.Fail) meetsAll = false;
            scorecards.Add(new MetricScorecard(
                MetricName: "Crash-Free Session Rate",
                ActualValue: snapshot.CrashFreeSessionRate,
                TargetValue: t.TargetCrashFreeRate,
                Status: crashStatus,
                EvaluationDetails: $"{snapshot.CrashFreeSessionRate:P2} crash-free sessions vs {t.TargetCrashFreeRate:P2} stability target."
            ));

            // 6. Average Satisfaction Rating (1.0 to 5.0)
            var ratingStatus = GradeHigherIsBetter(snapshot.AverageSatisfactionRating, t.TargetAverageRating, tolerance: 0.5f);
            if (ratingStatus == MetricEvaluationStatus.Fail) meetsAll = false;
            scorecards.Add(new MetricScorecard(
                MetricName: "User Satisfaction Rating",
                ActualValue: snapshot.AverageSatisfactionRating,
                TargetValue: t.TargetAverageRating,
                Status: ratingStatus,
                EvaluationDetails: $"{snapshot.AverageSatisfactionRating:F2}/5.0 observed vs {t.TargetAverageRating:F1} target."
            ));

            return new BetaKpiReport(
                GeneratedUtc: DateTime.UtcNow,
                Snapshot: snapshot,
                Scorecards: scorecards,
                MeetsAllKeyTargets: meetsAll
            );
        }

        private static MetricEvaluationStatus GradeHigherIsBetter(float actual, float target, float tolerance)
        {
            if (actual >= target) return MetricEvaluationStatus.Pass;
            if (actual >= target - tolerance) return MetricEvaluationStatus.Warning;
            return MetricEvaluationStatus.Fail;
        }
    }
}
