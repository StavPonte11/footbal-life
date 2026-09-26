using System;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class BetaMetricsEvaluatorTests
    {
        [Fact]
        public void EvaluateSnapshot_HealthyCohort_ReturnsPassAndMeetsAllTargets()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 200,
                TotalSessionsPlayed: 2500,
                D1RetentionRate: 0.52f, // Target >= 0.45
                D7RetentionRate: 0.24f, // Target >= 0.20
                MedianSessionDurationMinutes: 10.5f, // Target 8.0 - 15.0
                Season1CompletionRate: 0.40f, // Target >= 0.35
                CrashFreeSessionRate: 0.998f, // Target >= 0.995
                AverageSatisfactionRating: 4.4f // Target >= 4.0
            );

            var report = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);

            Assert.NotNull(report);
            Assert.True(report.MeetsAllKeyTargets);
            Assert.Equal(6, report.Scorecards.Count);

            foreach (var card in report.Scorecards)
            {
                Assert.Equal(MetricEvaluationStatus.Pass, card.Status);
            }
        }

        [Fact]
        public void EvaluateSnapshot_BorderlineRetention_ReturnsWarning()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 150,
                TotalSessionsPlayed: 1800,
                D1RetentionRate: 0.40f, // Amber: 0.37 - 0.449
                D7RetentionRate: 0.22f, // Pass
                MedianSessionDurationMinutes: 9.0f, // Pass
                Season1CompletionRate: 0.36f, // Pass
                CrashFreeSessionRate: 0.997f, // Pass
                AverageSatisfactionRating: 4.1f // Pass
            );

            var report = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);

            Assert.NotNull(report);
            Assert.True(report.MeetsAllKeyTargets); // Warning still qualifies as non-failing

            var d1Card = report.Scorecards.First(c => c.MetricName.Contains("Day 1"));
            Assert.Equal(MetricEvaluationStatus.Warning, d1Card.Status);
        }

        [Fact]
        public void EvaluateSnapshot_HighCrashRate_ReturnsFail()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 100,
                TotalSessionsPlayed: 1000,
                D1RetentionRate: 0.48f,
                D7RetentionRate: 0.21f,
                MedianSessionDurationMinutes: 11.0f,
                Season1CompletionRate: 0.38f,
                CrashFreeSessionRate: 0.965f, // Fail (< 0.985)
                AverageSatisfactionRating: 3.8f
            );

            var report = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);

            Assert.NotNull(report);
            Assert.False(report.MeetsAllKeyTargets);

            var crashCard = report.Scorecards.First(c => c.MetricName.Contains("Crash-Free"));
            Assert.Equal(MetricEvaluationStatus.Fail, crashCard.Status);
        }
    }
}
