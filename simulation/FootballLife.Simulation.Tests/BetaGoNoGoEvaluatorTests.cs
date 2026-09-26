using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class BetaGoNoGoEvaluatorTests
    {
        [Fact]
        public void Evaluate_AllPillarsGreen_ReturnsGoForGlobalLaunch()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 250,
                TotalSessionsPlayed: 3200,
                D1RetentionRate: 0.50f,
                D7RetentionRate: 0.23f,
                MedianSessionDurationMinutes: 11.2f,
                Season1CompletionRate: 0.42f,
                CrashFreeSessionRate: 0.998f,
                AverageSatisfactionRating: 4.5f
            );
            var kpiReport = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);
            var complianceReport = new MonetizationService().AuditCompliance();

            var gateReport = BetaGoNoGoEvaluator.Evaluate(kpiReport, complianceReport);

            Assert.NotNull(gateReport);
            Assert.Equal(GoNoGoDecision.GoForGlobalLaunch, gateReport.FinalDecision);
            Assert.Contains("GO FOR GLOBAL LAUNCH", gateReport.ExecutiveSummary);
            Assert.Equal(6, gateReport.PillarEvaluations.Count);
            Assert.Contains(gateReport.RecommendedNextActions, a => a.Contains("Submit final Release Candidate"));
        }

        [Fact]
        public void Evaluate_CriticalCrashRate_ReturnsNoGoBlocker()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 200,
                TotalSessionsPlayed: 1500,
                D1RetentionRate: 0.48f,
                D7RetentionRate: 0.22f,
                MedianSessionDurationMinutes: 10.0f,
                Season1CompletionRate: 0.38f,
                CrashFreeSessionRate: 0.960f, // Critical fail (< 0.985)
                AverageSatisfactionRating: 4.0f
            );
            var kpiReport = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);
            var complianceReport = new MonetizationService().AuditCompliance();

            var gateReport = BetaGoNoGoEvaluator.Evaluate(kpiReport, complianceReport);

            Assert.NotNull(gateReport);
            Assert.Equal(GoNoGoDecision.NoGoBlocker, gateReport.FinalDecision);
            Assert.Contains("NO-GO", gateReport.ExecutiveSummary);
            Assert.Contains(gateReport.RecommendedNextActions, a => a.Contains("Investigate top crash stack traces"));
        }

        [Fact]
        public void Evaluate_StoreComplianceFailure_ReturnsNoGoBlocker()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 200,
                TotalSessionsPlayed: 2000,
                D1RetentionRate: 0.50f,
                D7RetentionRate: 0.25f,
                MedianSessionDurationMinutes: 10.0f,
                Season1CompletionRate: 0.40f,
                CrashFreeSessionRate: 0.998f,
                AverageSatisfactionRating: 4.3f
            );
            var kpiReport = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);

            // Fabricate a non-compliant report with a simulated loot-box failure
            var nonCompliantReport = new StoreComplianceReport(
                TotalProducts: 8,
                DeterministicProducts: 7,
                LootBoxProducts: 1, // Non-compliant!
                GlobalStoreCompliant: false,
                BelgianComplianceApproved: false,
                DutchComplianceApproved: false,
                AuditTimestampUtc: DateTime.UtcNow,
                Products: new List<ProductComplianceInfo>()
            );

            var gateReport = BetaGoNoGoEvaluator.Evaluate(kpiReport, nonCompliantReport);

            Assert.NotNull(gateReport);
            Assert.Equal(GoNoGoDecision.NoGoBlocker, gateReport.FinalDecision);
            Assert.Contains(gateReport.RecommendedNextActions, a => a.Contains("non-deterministic"));
        }

        [Fact]
        public void Evaluate_WarningRetention_ReturnsConditionalGoBeta2()
        {
            var snapshot = new BetaCohortMetricsSnapshot(
                TotalEnrolledUsers: 150,
                TotalSessionsPlayed: 1200,
                D1RetentionRate: 0.40f, // Warning (target 0.45)
                D7RetentionRate: 0.18f, // Warning (target 0.20)
                MedianSessionDurationMinutes: 9.0f,
                Season1CompletionRate: 0.36f,
                CrashFreeSessionRate: 0.996f,
                AverageSatisfactionRating: 4.1f
            );
            var kpiReport = BetaMetricsEvaluator.EvaluateSnapshot(snapshot);
            var complianceReport = new MonetizationService().AuditCompliance();

            var gateReport = BetaGoNoGoEvaluator.Evaluate(kpiReport, complianceReport);

            Assert.NotNull(gateReport);
            Assert.Equal(GoNoGoDecision.ConditionalGoBeta2, gateReport.FinalDecision);
            Assert.Contains("CONDITIONAL GO", gateReport.ExecutiveSummary);
            Assert.Contains(gateReport.RecommendedNextActions, a => a.Contains("Beta 2"));
        }
    }
}
