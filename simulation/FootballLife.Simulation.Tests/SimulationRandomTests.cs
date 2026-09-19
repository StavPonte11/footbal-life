using System;
using System.Collections.Generic;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class SimulationRandomTests
    {
        // ─── Determinism ──────────────────────────────────────────────────────

        [Fact]
        public void SimulationRandom_Determinism_SameSeed_ProducesIdenticalFloatSequence()
        {
            const int seed = 42;
            const int iterations = 1000;

            var rng1 = new SimulationRandom(seed);
            var rng2 = new SimulationRandom(seed);

            for (int i = 0; i < iterations; i++)
            {
                float a = rng1.NextFloat(0f, 1f);
                float b = rng2.NextFloat(0f, 1f);
                Assert.Equal(a, b);
            }
        }

        [Fact]
        public void SimulationRandom_Determinism_DifferentSeeds_ProduceDifferentSequences()
        {
            var rng1 = new SimulationRandom(1);
            var rng2 = new SimulationRandom(2);

            bool anyDifference = false;
            for (int i = 0; i < 100; i++)
            {
                if (rng1.NextFloat(0f, 1f) != rng2.NextFloat(0f, 1f))
                {
                    anyDifference = true;
                    break;
                }
            }

            Assert.True(anyDifference, "Different seeds should produce different sequences.");
        }

        // ─── NextFloat ────────────────────────────────────────────────────────

        [Fact]
        public void NextFloat_AlwaysWithinRange()
        {
            var rng = new SimulationRandom(99);
            for (int i = 0; i < 10_000; i++)
            {
                float v = rng.NextFloat(-10f, 10f);
                Assert.True(v >= -10f && v < 10f, $"Value {v} out of [-10, 10)");
            }
        }

        [Fact]
        public void NextFloat_MinEqualsMax_Throws()
        {
            var rng = new SimulationRandom(0);
            Assert.Throws<ArgumentException>(() => rng.NextFloat(5f, 5f));
        }

        [Fact]
        public void NextFloat_MinGreaterThanMax_Throws()
        {
            var rng = new SimulationRandom(0);
            Assert.Throws<ArgumentException>(() => rng.NextFloat(10f, 5f));
        }

        // ─── NextInt ──────────────────────────────────────────────────────────

        [Fact]
        public void NextInt_AlwaysWithinRange()
        {
            var rng = new SimulationRandom(7);
            for (int i = 0; i < 10_000; i++)
            {
                int v = rng.NextInt(0, 100);
                Assert.True(v >= 0 && v < 100, $"Value {v} out of [0, 100)");
            }
        }

        [Fact]
        public void NextInt_MinEqualsMax_Throws()
        {
            var rng = new SimulationRandom(0);
            Assert.Throws<ArgumentException>(() => rng.NextInt(5, 5));
        }

        [Fact]
        public void NextInt_NeverReturnsMaxExclusive()
        {
            var rng = new SimulationRandom(123);
            for (int i = 0; i < 10_000; i++)
            {
                int v = rng.NextInt(0, 10);
                Assert.NotEqual(10, v);
            }
        }

        // ─── NextBool ─────────────────────────────────────────────────────────

        [Fact]
        public void NextBool_Zero_AlwaysFalse()
        {
            var rng = new SimulationRandom(1);
            for (int i = 0; i < 1_000; i++)
                Assert.False(rng.NextBool(0f));
        }

        [Fact]
        public void NextBool_One_AlwaysTrue()
        {
            var rng = new SimulationRandom(1);
            for (int i = 0; i < 1_000; i++)
                Assert.True(rng.NextBool(1f));
        }

        [Fact]
        public void NextBool_HalfProbability_StatisticallyCorrect()
        {
            var rng = new SimulationRandom(555);
            int trueCount = 0;
            const int runs = 10_000;
            for (int i = 0; i < runs; i++)
                if (rng.NextBool(0.5f)) trueCount++;

            // Expect 40–60% true (very wide band to avoid flakiness)
            double ratio = (double)trueCount / runs;
            Assert.True(ratio is >= 0.40 and <= 0.60, $"Expected ~50% true but got {ratio:P1}");
        }

        [Fact]
        public void NextBool_OverclampsInputsBeyond01()
        {
            var rng = new SimulationRandom(1);
            // Should not throw for out-of-range inputs
            bool r1 = rng.NextBool(-1f);  // clamped to 0 → always false
            bool r2 = rng.NextBool(2f);   // clamped to 1 → always true
            Assert.False(r1);
            Assert.True(r2);
        }

        // ─── NextGaussian ─────────────────────────────────────────────────────

        [Fact]
        public void NextGaussian_ZeroStddev_ReturnsMean()
        {
            var rng = new SimulationRandom(42);
            for (int i = 0; i < 100; i++)
                Assert.Equal(5.0f, rng.NextGaussian(5f, 0f));
        }

        [Fact]
        public void NextGaussian_NegativeStddev_TreatedAsZero()
        {
            var rng = new SimulationRandom(42);
            for (int i = 0; i < 100; i++)
                Assert.Equal(10f, rng.NextGaussian(10f, -5f));
        }

        [Fact]
        public void NextGaussian_StatisticallyCorrectMeanAndStddev()
        {
            var rng = new SimulationRandom(1234);
            const int n = 50_000;
            const float expectedMean = 50f;
            const float expectedStddev = 10f;

            double sum = 0.0;
            double sumSq = 0.0;

            for (int i = 0; i < n; i++)
            {
                float v = rng.NextGaussian(expectedMean, expectedStddev);
                sum += v;
                sumSq += v * v;
            }

            double actualMean = sum / n;
            double variance = sumSq / n - actualMean * actualMean;
            double actualStddev = Math.Sqrt(variance);

            Assert.True(Math.Abs(actualMean - expectedMean) < 0.5,
                $"Mean deviated too much: expected {expectedMean}, got {actualMean:F2}");
            Assert.True(Math.Abs(actualStddev - expectedStddev) < 0.5,
                $"StdDev deviated too much: expected {expectedStddev}, got {actualStddev:F2}");
        }

        [Fact]
        public void NextGaussian_Determinism_SameSeed_SameSequence()
        {
            var rng1 = new SimulationRandom(77);
            var rng2 = new SimulationRandom(77);
            for (int i = 0; i < 1_000; i++)
                Assert.Equal(rng1.NextGaussian(0f, 1f), rng2.NextGaussian(0f, 1f));
        }

        // ─── Pick ─────────────────────────────────────────────────────────────

        [Fact]
        public void Pick_EmptyList_Throws()
        {
            var rng = new SimulationRandom(0);
            Assert.Throws<ArgumentException>(() => rng.Pick(new List<int>()));
        }

        [Fact]
        public void Pick_NullList_Throws()
        {
            var rng = new SimulationRandom(0);
            Assert.Throws<ArgumentNullException>(() => rng.Pick<int>(null!));
        }

        [Fact]
        public void Pick_SingleElement_AlwaysReturnsThatElement()
        {
            var rng = new SimulationRandom(0);
            var list = new List<string> { "only" };
            for (int i = 0; i < 100; i++)
                Assert.Equal("only", rng.Pick(list));
        }

        [Fact]
        public void Pick_AlwaysReturnsElementFromList()
        {
            var rng = new SimulationRandom(42);
            var list = new List<int> { 10, 20, 30, 40, 50 };
            for (int i = 0; i < 1_000; i++)
            {
                int v = rng.Pick(list);
                Assert.Contains(v, list);
            }
        }

        [Fact]
        public void Pick_Determinism_SameSeed_SamePickSequence()
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var rng1 = new SimulationRandom(42);
            var rng2 = new SimulationRandom(42);
            for (int i = 0; i < 1_000; i++)
                Assert.Equal(rng1.Pick(items), rng2.Pick(items));
        }
    }
}
