using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CareerSimulatorTests
    {
        [Fact]
        public void SingleCareer_SimulatesToCompletionWithoutExceptions()
        {
            var career = CareerSimulationEngine.SimulateCareer(seed: 42, maxSeasons: 5);

            Assert.Equal(42, career.Seed);
            Assert.NotEqual(Guid.Empty, career.PlayerId);
            Assert.False(string.IsNullOrWhiteSpace(career.Name));
            Assert.InRange(career.StartingOverall, 45, 75);
            Assert.InRange(career.PeakOverall, career.StartingOverall, 99);
            Assert.InRange(career.RetirementAge, 18, 40);
            Assert.InRange(career.SeasonsPlayed, 1, 5);
            Assert.NotEmpty(career.Seasons);
            Assert.Equal(career.SeasonsPlayed, career.Seasons.Count);
        }

        [Fact]
        public void CareerSimulator_DeterministicSeed_ProducesIdenticalResults()
        {
            var career1 = CareerSimulationEngine.SimulateCareer(seed: 12345, maxSeasons: 10);
            var career2 = CareerSimulationEngine.SimulateCareer(seed: 12345, maxSeasons: 10);

            Assert.Equal(career1.StartingOverall, career2.StartingOverall);
            Assert.Equal(career1.PeakOverall, career2.PeakOverall);
            Assert.Equal(career1.PeakAge, career2.PeakAge);
            Assert.Equal(career1.RetirementAge, career2.RetirementAge);
            Assert.Equal(career1.SeasonsPlayed, career2.SeasonsPlayed);
            Assert.Equal(career1.TotalAppearances, career2.TotalAppearances);
            Assert.Equal(career1.TotalGoals, career2.TotalGoals);
            Assert.Equal(career1.TotalAssists, career2.TotalAssists);
            Assert.Equal(career1.TotalEarnings, career2.TotalEarnings);
            Assert.Equal(career1.FinalBalance, career2.FinalBalance);
            Assert.Equal(career1.TransferCount, career2.TransferCount);
            Assert.Equal(career1.ToCsvLine(), career2.ToCsvLine());
        }

        [Fact]
        public void CareerStatistics_CsvHeaderAndLine_GeneratesValidFormat()
        {
            var career = CareerSimulationEngine.SimulateCareer(seed: 99, maxSeasons: 2);
            string line = career.ToCsvLine();

            Assert.NotEmpty(CareerStatistics.CsvHeader);
            var headerColumns = CareerStatistics.CsvHeader.Split(',');
            var lineColumns = line.Split(',');

            Assert.True(lineColumns.Length >= headerColumns.Length);
            Assert.StartsWith("99,", line);
        }

        [Fact]
        public void AggregateMetrics_ComputesPercentilesAccurately()
        {
            var values = new double[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var dist = MetricDistribution.Create(values);

            Assert.Equal(55, dist.Mean);
            Assert.Equal(55, dist.Median);
            Assert.Equal(19, dist.P10);
            Assert.Equal(91, dist.P90);
            Assert.Equal(10, dist.Min);
            Assert.Equal(100, dist.Max);
        }

        [Fact]
        public void AggregateReport_GeneratesAsciiHistogram()
        {
            var careers = new List<CareerStatistics>();
            for (int i = 0; i < 5; i++)
            {
                careers.Add(CareerSimulationEngine.SimulateCareer(i * 100, maxSeasons: 2));
            }

            var report = new AggregateReport(careers);
            string histogram = report.GenerateAsciiHistogram(new double[] { 60, 65, 70, 75, 80 }, 50, 90, 4, "Test");

            Assert.Contains("Test Distribution", histogram);
            Assert.Contains("#", histogram);
        }

        [Fact]
        public void BulkSimulationRunner_RunsBatchHeadlessAndComputesMetrics()
        {
            var report = BulkSimulationRunner.Run(careersCount: 10, maxSeasons: 3, masterSeed: 777);

            Assert.Equal(10, report.Careers.Count);
            Assert.Equal(10, report.Metrics.TotalCareers);
            Assert.InRange(report.Metrics.PeakOverall.Mean, 50, 95);
            Assert.True(report.Elapsed.TotalMilliseconds > 0);
            Assert.True(report.CareersPerSecond > 0);
        }
    }
}
