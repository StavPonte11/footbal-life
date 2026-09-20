using System;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CareerSimulatorBalanceTests
    {
        [Fact]
        public void Balance_PeakOverallDistribution_RealisticAndBounded()
        {
            var report = BulkSimulationRunner.Run(careersCount: 100, maxSeasons: 20, masterSeed: 1001);

            // Mean peak overall should reflect a realistic distribution centered in late 60s / early 70s
            Assert.InRange(report.Metrics.PeakOverall.Mean, 65.0, 75.0);

            // P10 represents lower tier career players (minimum baseline >= 58)
            Assert.True(report.Metrics.PeakOverall.P10 >= 58.0, $"Expected P10 >= 58.0, got {report.Metrics.PeakOverall.P10}");

            // P90 should capture high-tier/elite performers without runaway inflation (<= 85)
            Assert.True(report.Metrics.PeakOverall.P90 <= 85.0, $"Expected P90 <= 85.0, got {report.Metrics.PeakOverall.P90}");

            // Ceiling check: no career exceeds 99
            Assert.True(report.Metrics.PeakOverall.Max <= 99.0, $"Expected Max <= 99.0, got {report.Metrics.PeakOverall.Max}");

            // No player should reach 90+ overall before age 24
            foreach (var career in report.Careers)
            {
                if (career.PeakOverall >= 90)
                {
                    Assert.True(career.PeakAge >= 24, $"Player {career.Name} reached {career.PeakOverall} overall at age {career.PeakAge} (expected >= 24).");
                }
            }
        }

        [Fact]
        public void Balance_CareerLongevityAndRetirement_WithinRealisticLifecycles()
        {
            var report = BulkSimulationRunner.Run(careersCount: 100, maxSeasons: 20, masterSeed: 2002);

            // Realistic retirement age distribution: mean in [33, 38]
            Assert.InRange(report.Metrics.RetirementAge.Mean, 33.0, 38.0);

            // Career length: mean in [12, 20]
            Assert.InRange(report.Metrics.SeasonsPlayed.Mean, 12.0, 20.0);

            // Boundaries
            Assert.True(report.Metrics.RetirementAge.Min >= 30, $"Retirement age min expected >= 30, got {report.Metrics.RetirementAge.Min}");
            Assert.True(report.Metrics.RetirementAge.Max <= 40, $"Retirement age max expected <= 40, got {report.Metrics.RetirementAge.Max}");
        }

        [Fact]
        public void Balance_TransferFrequency_RealisticCareerMoves()
        {
            var report = BulkSimulationRunner.Run(careersCount: 100, maxSeasons: 20, masterSeed: 3003);

            // Realistic career transfers: average 2 to 6 transfers over an entire 15-20 year career
            Assert.InRange(report.Metrics.Transfers.Mean, 2.0, 6.0);

            // No player should bounce clubs constantly (> 10 transfers)
            Assert.True(report.Metrics.Transfers.Max <= 10.0, $"Max career transfers was {report.Metrics.Transfers.Max}, expected <= 10");
        }

        [Fact]
        public void Balance_FinancialSustainability_LowBankruptcyRate()
        {
            var report = BulkSimulationRunner.Run(careersCount: 100, maxSeasons: 20, masterSeed: 4004);

            // Bankruptcy rate should be less than 10% under sustainable wage & lifestyle curves
            Assert.True(report.Metrics.BankruptcyRate < 10.0, $"Expected bankruptcy rate < 10%, got {report.Metrics.BankruptcyRate}%");

            // Professional career earnings should be positive and substantial
            Assert.True(report.Metrics.TotalEarnings.Mean > 1000000.0, $"Expected mean career earnings > £1M, got £{report.Metrics.TotalEarnings.Mean}");
        }

        [Fact]
        public void Balance_StrictDeterminism_BitExactOutput()
        {
            var report1 = BulkSimulationRunner.Run(careersCount: 20, maxSeasons: 15, masterSeed: 7777);
            var report2 = BulkSimulationRunner.Run(careersCount: 20, maxSeasons: 15, masterSeed: 7777);

            Assert.Equal(report1.Careers.Count, report2.Careers.Count);
            for (int i = 0; i < report1.Careers.Count; i++)
            {
                Assert.Equal(report1.Careers[i].ToCsvLine(), report2.Careers[i].ToCsvLine());
            }

            Assert.Equal(report1.Metrics.PeakOverall.Mean, report2.Metrics.PeakOverall.Mean);
            Assert.Equal(report1.Metrics.RetirementAge.Mean, report2.Metrics.RetirementAge.Mean);
            Assert.Equal(report1.Metrics.Transfers.Mean, report2.Metrics.Transfers.Mean);
        }
    }
}
