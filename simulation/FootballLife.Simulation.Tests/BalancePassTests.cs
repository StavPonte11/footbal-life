using System;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class BalancePassTests
    {
        [Fact]
        public void CareerBalancePass_200Careers_RespectsTargetStatisticalDistributions()
        {
            const int careersCount = 200;
            const int masterSeed = 42;

            var report = BulkSimulationRunner.Run(
                careersCount: careersCount,
                maxSeasons: 20,
                masterSeed: masterSeed,
                parallel: true);

            Assert.Equal(careersCount, report.Careers.Count);

            // 1. Peak Overall Distribution: Mean between 65 and 73, max <= 95
            Assert.InRange(report.Metrics.PeakOverall.Mean, 65.0, 73.0);
            Assert.True(report.Metrics.PeakOverall.Max <= 95.0, $"Max peak overall exceeded 95: {report.Metrics.PeakOverall.Max}");
            Assert.True(report.Metrics.PeakOverall.Min >= 52.0, $"Min peak overall too low: {report.Metrics.PeakOverall.Min}");

            // Elite rate: between 1% and 10% of players achieve 80+
            double eliteRate = report.Careers.Count(c => c.PeakOverall >= 80) / (double)careersCount;
            Assert.InRange(eliteRate, 0.01, 0.10);

            // 2. Longevity & Retirement: Mean between 33.0 and 36.5
            Assert.InRange(report.Metrics.RetirementAge.Mean, 33.0, 36.5);
            Assert.True(report.Metrics.RetirementAge.Min >= 32.0, $"Player retired too young: {report.Metrics.RetirementAge.Min}");
            Assert.True(report.Metrics.RetirementAge.Max <= 40.0, $"Player played past mandatory age 40: {report.Metrics.RetirementAge.Max}");

            // 3. Economy & Solvency: Bankruptcy rate strictly under 1%
            Assert.True(report.Metrics.BankruptcyRate < 1.0, $"Bankruptcy rate too high: {report.Metrics.BankruptcyRate}%");
            Assert.True(report.Metrics.TotalEarnings.Mean > 5000000.0, "Average career earnings unexpectedly low");
            Assert.True(report.Metrics.CommercialEarnings.Mean > 200000.0, "Average commercial earnings unexpectedly low");

            // 4. Legacy System & Hall of Fame
            Assert.InRange(report.Metrics.HallOfFameRate, 2.0, 10.0);
            Assert.True(report.Metrics.CareerScore.Mean >= 250.0, "Average career score below baseline threshold");

            // Check that all legacy grades are valid and distribution is pyramid-shaped
            var dist = report.Metrics.LegacyGradeDistribution;
            Assert.True(dist[LegacyGrade.Journeyman] > dist[LegacyGrade.Icon], "Expected more Journeymen than Icons");
            Assert.True(dist[LegacyGrade.Icon] >= dist[LegacyGrade.Legend], "Expected more Icons than Legends");
        }

        [Fact]
        public void CareerBalancePass_SingleCareer_GeneratesCompleteLegacyAndSponsorshipMetrics()
        {
            var career = CareerSimulationEngine.SimulateCareer(seed: 12345, maxSeasons: 20);

            Assert.True(career.SeasonsPlayed >= 10, $"Seasons played should be at least 10, got {career.SeasonsPlayed}");
            Assert.True(career.TotalAppearances > 0);
            Assert.True(career.TotalEarnings > 0);
            Assert.True(career.CommercialEarnings >= 0);
            Assert.True(career.CareerScore > 0);
            Assert.True(Enum.IsDefined(typeof(LegacyGrade), career.LegacyGrade));
        }
    }
}
