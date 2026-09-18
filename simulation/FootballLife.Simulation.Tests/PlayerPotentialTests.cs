using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerPotentialTests
    {
        [Fact]
        public void PlayerPotential_Rating_BoundedBetween50And99()
        {
            // Valid boundary ratings
            var min = new PlayerPotential(50, PotentialRange.Medium);
            var max = new PlayerPotential(99, PotentialRange.Elite);

            Assert.Equal(50, min.PotentialRating);
            Assert.Equal(99, max.PotentialRating);

            // Below 50 should throw
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerPotential(49, PotentialRange.Low));

            // Above 99 should throw
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerPotential(100, PotentialRange.High));
        }

        [Fact]
        public void PlayerPotential_RealizationProbability_DecreasesAfterPeakAge()
        {
            var potential = new PlayerPotential(85, PotentialRange.Medium);

            // At or before peak age (28), realization is at maximum (1.0f)
            float probYouth = potential.RealizationProbability(18);
            float probPeak = potential.RealizationProbability(28);
            Assert.Equal(1.0f, probYouth);
            Assert.Equal(1.0f, probPeak);

            // Each subsequent year post-peak should decrease monotonically
            float prob29 = potential.RealizationProbability(29);
            float prob31 = potential.RealizationProbability(31);
            float prob34 = potential.RealizationProbability(34);
            float prob38 = potential.RealizationProbability(38);

            Assert.True(prob29 < probPeak, "Age 29 realization should be lower than peak.");
            Assert.True(prob31 < prob29, "Age 31 realization should be lower than age 29.");
            Assert.True(prob34 < prob31, "Age 34 realization should be lower than age 31.");
            Assert.True(prob38 < prob34, "Age 38 realization should be lower than age 34.");

            // Always bounded in [0f, 1f]
            Assert.InRange(prob38, 0.0f, 1.0f);
        }

        [Fact]
        public void PlayerPotential_WithExpression_EnforcesBounds()
        {
            var original = new PlayerPotential(80, PotentialRange.Medium);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                original with { PotentialRating = 105 });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                original with { PotentialRating = 40 });

            var updated = original with { PotentialRating = 90, Range = PotentialRange.Elite };
            Assert.Equal(90, updated.PotentialRating);
            Assert.Equal(PotentialRange.Elite, updated.Range);
            Assert.NotSame(original, updated);
        }

        [Fact]
        public void PlayerPotential_GetVarianceBandWidth_ReturnsExpectedValues()
        {
            var low = new PlayerPotential(75, PotentialRange.Low);
            var med = new PlayerPotential(75, PotentialRange.Medium);
            var high = new PlayerPotential(75, PotentialRange.High);
            var elite = new PlayerPotential(75, PotentialRange.Elite);

            Assert.Equal(2, low.GetVarianceBandWidth());
            Assert.Equal(4, med.GetVarianceBandWidth());
            Assert.Equal(6, high.GetVarianceBandWidth());
            Assert.Equal(8, elite.GetVarianceBandWidth());
        }

        [Fact]
        public void PlayerPotential_GetCeilingBand_ClampsToValidBounds()
        {
            // Elite prospect near top: 96 + 8 = 104 -> clamped to 99
            var nearTop = new PlayerPotential(96, PotentialRange.Elite);
            var (minTop, maxTop) = nearTop.GetCeilingBand();
            Assert.Equal(88, minTop);
            Assert.Equal(99, maxTop);

            // Low prospect near bottom: 52 - 4 = 48 -> clamped to 50
            var nearBottom = new PlayerPotential(52, PotentialRange.Medium);
            var (minBottom, maxBottom) = nearBottom.GetCeilingBand();
            Assert.Equal(50, minBottom);
            Assert.Equal(56, maxBottom);
        }

        [Fact]
        public void PlayerPotential_CreateClamped_ClampsInput()
        {
            var clampedLow = PlayerPotential.CreateClamped(20, PotentialRange.Low);
            var clampedHigh = PlayerPotential.CreateClamped(150, PotentialRange.Elite);

            Assert.Equal(50, clampedLow.PotentialRating);
            Assert.Equal(99, clampedHigh.PotentialRating);
        }
    }
}
