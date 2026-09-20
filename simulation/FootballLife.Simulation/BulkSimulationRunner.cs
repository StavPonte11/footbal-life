using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Encapsulates the complete result of a bulk multi-career batch simulation run.
    /// </summary>
    public sealed class BulkSimulationReport
    {
        public IReadOnlyList<CareerStatistics> Careers { get; }
        public AggregateReport Metrics { get; }
        public TimeSpan Elapsed { get; }
        public double CareersPerSecond => Elapsed.TotalSeconds > 0 ? Careers.Count / Elapsed.TotalSeconds : 0;

        public BulkSimulationReport(IReadOnlyList<CareerStatistics> careers, AggregateReport metrics, TimeSpan elapsed)
        {
            Careers = careers ?? throw new ArgumentNullException(nameof(careers));
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            Elapsed = elapsed;
        }
    }

    /// <summary>
    /// High-throughput multi-career orchestrator supporting sequential and parallel headless execution.
    /// </summary>
    public static class BulkSimulationRunner
    {
        /// <summary>
        /// Runs a batch of full careers headlessly, generating per-career metrics and aggregate distributions.
        /// </summary>
        public static BulkSimulationReport Run(
            int careersCount,
            int maxSeasons = CareerSimulationEngine.DefaultMaxSeasons,
            int masterSeed = 42,
            bool parallel = false,
            Action<int, int>? progressCallback = null)
        {
            if (careersCount <= 0) throw new ArgumentOutOfRangeException(nameof(careersCount), "Careers count must be > 0.");

            var sw = Stopwatch.StartNew();
            var results = new CareerStatistics[careersCount];
            int completedCount = 0;

            if (parallel)
            {
                Parallel.For(0, careersCount, i =>
                {
                    int careerSeed = unchecked(masterSeed + i * 10007);
                    results[i] = CareerSimulationEngine.SimulateCareer(careerSeed, maxSeasons);
                    int done = Interlocked.Increment(ref completedCount);
                    if (progressCallback != null && (done % 100 == 0 || done == careersCount))
                    {
                        progressCallback(done, careersCount);
                    }
                });
            }
            else
            {
                for (int i = 0; i < careersCount; i++)
                {
                    int careerSeed = unchecked(masterSeed + i * 10007);
                    results[i] = CareerSimulationEngine.SimulateCareer(careerSeed, maxSeasons);
                    int done = ++completedCount;
                    if (progressCallback != null && (done % 100 == 0 || done == careersCount))
                    {
                        progressCallback(done, careersCount);
                    }
                }
            }

            sw.Stop();

            var metrics = new AggregateReport(results);
            return new BulkSimulationReport(results, metrics, sw.Elapsed);
        }
    }
}
