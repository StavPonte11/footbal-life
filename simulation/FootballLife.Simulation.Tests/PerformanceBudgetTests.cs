using System;
using System.IO;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PerformanceBudgetTests
    {
        private static string GetContentDataDir()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "content", "data"));
        }

        private static PlayerAbilities CreateAbilities() =>
            new PlayerAbilities(
                pace: 75, acceleration: 75, stamina: 70, strength: 70, agility: 70,
                passing: 75, shooting: 75, dribbling: 75, crossing: 70, firstTouch: 75, tackling: 60,
                vision: 75, composure: 75, positioning: 75, decisionMaking: 75);

        private static PlayerState CreateState() =>
            new PlayerState(
                fatigue: 20f,
                confidence: 70f,
                form: 70f,
                happiness: 70f,
                motivation: 70f,
                morale: 70f,
                fitness: 80f);

        private static MatchSituation CreateSituation() =>
            new MatchSituation(
                type: SituationType.RunningInBehind,
                opponentPressure: 3.5f,
                expectedDifficulty: 0.5f,
                positionalAdvantage: 0.3f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.RunningInBehind));

        [Fact]
        public void PerformanceBudget_RecommendedFrameRates_FollowsBatteryAndGameplayRules()
        {
            // 3D Gameplay with ample battery -> 60 FPS
            Assert.Equal(60, PerformanceBudget.GetRecommendedFrameRate(batteryLevel: 0.85f, isBatterySaverActive: false, isIn3DGameplay: true));

            // UI navigation (not in 3D gameplay) with ample battery -> 30 FPS
            Assert.Equal(30, PerformanceBudget.GetRecommendedFrameRate(batteryLevel: 0.85f, isBatterySaverActive: false, isIn3DGameplay: false));

            // 3D Gameplay with low battery (<= 20%) -> 30 FPS
            Assert.Equal(30, PerformanceBudget.GetRecommendedFrameRate(batteryLevel: 0.18f, isBatterySaverActive: false, isIn3DGameplay: true));
            Assert.Equal(30, PerformanceBudget.GetRecommendedFrameRate(batteryLevel: 0.20f, isBatterySaverActive: false, isIn3DGameplay: true));

            // Battery saver override active -> 30 FPS regardless of gameplay or battery level
            Assert.Equal(30, PerformanceBudget.GetRecommendedFrameRate(batteryLevel: 0.95f, isBatterySaverActive: true, isIn3DGameplay: true));
        }

        [Fact]
        public void PerformanceBudget_MatchSituationEvaluation_HighThroughputZeroGcHotPath()
        {
            var abilities = CreateAbilities();
            var state = CreateState();
            var situation = CreateSituation();
            var rng = new SimulationRandom(42);

            const int iterations = 5000;

            var result = PerformanceProfiler.Benchmark("MatchSituationEvaluation_HotPath", iterations, () =>
            {
                _ = ActionResolver.Resolve(MatchAction.Shot_Close, situation, abilities, state, rng);
            });

            // 5,000 action resolutions should take < 500ms (typically under 30ms on modern CPU)
            Assert.True(result.ElapsedMs < 500.0, $"Expected 5,000 iterations < 500ms, but took {result.ElapsedMs:F2}ms");

            // Zero or near-zero allocation: under 64 bytes per iteration on average (structs + value types)
            double bytesPerOp = (double)result.AllocatedBytes / iterations;
            Assert.True(bytesPerOp < 64.0, $"Expected < 64 bytes allocated per hot-path evaluation, got {bytesPerOp:F1} bytes");
        }

        [Fact]
        public void PerformanceBudget_WorldStateMemoryFootprint_UnderThreshold()
        {
            string dataDir = GetContentDataDir();
            if (!Directory.Exists(dataDir)) return;

            long startAllocated = GC.GetAllocatedBytesForCurrentThread();
            var loadResult = WorldDataLoader.LoadFromDirectory(dataDir);
            long endAllocated = GC.GetAllocatedBytesForCurrentThread();

            Assert.True(loadResult.IsSuccess, "World data load failed");
            Assert.NotNull(loadResult.WorldState);

            long allocatedBytes = endAllocated - startAllocated;
            double allocatedMb = allocatedBytes / (1024.0 * 1024.0);

            // Complete retained world state data (66 clubs, 11 leagues) must fit well below the 10 MB budget (< 5 MB target)
            Assert.True(allocatedMb < PerformanceBudget.MaxWorldDataMemoryMb,
                $"World data load allocated {allocatedMb:F2}MB, which exceeds budget {PerformanceBudget.MaxWorldDataMemoryMb}MB");
            Assert.True(allocatedMb < 5.0,
                $"World data load allocated {allocatedMb:F2}MB, exceeding mobile target 5.0MB");
        }

        [Fact]
        public void PerformanceBudget_SimulationThroughput_MatchesSimulateFast()
        {
            var homeClub = Guid.NewGuid();
            var awayClub = Guid.NewGuid();
            var leagueId = Guid.NewGuid();
            var fixture = ScheduledMatch.Create(new DateOnly(2026, 8, 15), homeClub, awayClub, leagueId);
            var abilities = CreateAbilities();
            var state = CreateState();
            var rng = new SimulationRandom(100);

            const int matchCount = 100;

            var result = PerformanceProfiler.Benchmark("MatchSimulator_Batch", matchCount, () =>
            {
                _ = MatchSimulator.Simulate(fixture, homeClub, abilities, state, Position.ST, rng);
            });

            // 100 full 90-minute match simulations must complete in < 2000ms
            Assert.True(result.ElapsedMs < 2000.0, $"Simulating 100 matches took {result.ElapsedMs:F2}ms, exceeding 2000ms limit");
        }
    }
}
