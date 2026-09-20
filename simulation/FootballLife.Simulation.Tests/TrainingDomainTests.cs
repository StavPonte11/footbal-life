using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class TrainingDomainTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2026, 9, 1);

        // ─── TrainingSession Validation ───────────────────────────────────────

        [Fact]
        public void TrainingSession_ValidData_CreatesSuccessfully()
        {
            var session = new TrainingSession(TrainingType.Technical, TrainingIntensity.Moderate, 60, TestDate);

            Assert.Equal(TrainingType.Technical, session.Type);
            Assert.Equal(TrainingIntensity.Moderate, session.Intensity);
            Assert.Equal(60, session.DurationMinutes);
            Assert.Equal(TestDate, session.Date);
        }

        [Fact]
        public void TrainingSession_Duration_OutOfRange_Throws()
        {
            // Below minimum
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TrainingSession(TrainingType.Technical, TrainingIntensity.Hard, 14, TestDate));

            // Above maximum
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TrainingSession(TrainingType.Physical, TrainingIntensity.Light, 121, TestDate));
        }

        [Fact]
        public void TrainingSession_Duration_AtBoundaries_IsValid()
        {
            var min = new TrainingSession(TrainingType.Recovery, TrainingIntensity.Light, 15, TestDate);
            var max = new TrainingSession(TrainingType.Gym, TrainingIntensity.Maximum, 120, TestDate);

            Assert.Equal(15, min.DurationMinutes);
            Assert.Equal(120, max.DurationMinutes);
        }

        [Theory]
        [InlineData(TrainingType.Technical, TrainingIntensity.Light)]
        [InlineData(TrainingType.Physical, TrainingIntensity.Moderate)]
        [InlineData(TrainingType.Mental, TrainingIntensity.Hard)]
        [InlineData(TrainingType.PositionSpecific, TrainingIntensity.Maximum)]
        [InlineData(TrainingType.Recovery, TrainingIntensity.Light)]
        [InlineData(TrainingType.Gym, TrainingIntensity.Moderate)]
        public void TrainingSession_AllCombinations_CreateWithout_Exception(TrainingType type, TrainingIntensity intensity)
        {
            var session = new TrainingSession(type, intensity, 60, TestDate);
            Assert.Equal(type, session.Type);
            Assert.Equal(intensity, session.Intensity);
        }

        // ─── TrainingResult TotalXpGained ─────────────────────────────────────

        [Fact]
        public void TrainingResult_TotalXp_SumsAllAttributes()
        {
            var xp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Passing]   = 2.5f,
                [AttributeName.Shooting]  = 1.8f,
                [AttributeName.Dribbling] = 3.1f,
            };

            var result = TrainingResult.Create(xp, fatigueCost: 8f, injuryRisk: 0f);

            Assert.Equal(2.5f + 1.8f + 3.1f, result.TotalXpGained, precision: 4);
        }

        [Fact]
        public void TrainingResult_EmptyXp_TotalIsZero()
        {
            var result = TrainingResult.Create(new Dictionary<AttributeName, float>(), 5f, 0f);
            Assert.Equal(0f, result.TotalXpGained);
        }

        [Fact]
        public void TrainingResult_Recovery_HasNegativeFatigueCostAndZeroXp()
        {
            var recovery = TrainingResult.Recovery(-15f);

            Assert.Equal(-15f, recovery.FatigueCost);
            Assert.Equal(0f, recovery.InjuryRisk);
            Assert.Empty(recovery.XpGained);
            Assert.Equal(0f, recovery.TotalXpGained);
        }

        [Fact]
        public void TrainingResult_Recovery_PositiveFatigueCost_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TrainingResult.Recovery(5f));
        }

        [Fact]
        public void TrainingResult_InjuryRisk_OutOfRange_Throws()
        {
            var xp = new Dictionary<AttributeName, float>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TrainingResult(
                    new System.Collections.ObjectModel.ReadOnlyDictionary<AttributeName, float>(xp),
                    8f,
                    1.1f)); // > 1.0

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TrainingResult(
                    new System.Collections.ObjectModel.ReadOnlyDictionary<AttributeName, float>(xp),
                    8f,
                    -0.1f)); // < 0.0
        }

        [Fact]
        public void TrainingResult_ImmutabilityOfXpDictionary()
        {
            var xp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Passing] = 3.0f
            };

            var result = TrainingResult.Create(xp, 5f, 0f);

            // Mutating the original dict should not affect the result
            xp[AttributeName.Passing] = 99f;
            Assert.Equal(3.0f, result.XpGained[AttributeName.Passing]);
        }

        // ─── Enum Completeness ────────────────────────────────────────────────

        [Fact]
        public void TrainingType_HasAllSixTypes()
        {
            var values = Enum.GetValues<TrainingType>();
            Assert.Equal(6, values.Length);
            Assert.Contains(TrainingType.Technical, values);
            Assert.Contains(TrainingType.Physical, values);
            Assert.Contains(TrainingType.Mental, values);
            Assert.Contains(TrainingType.PositionSpecific, values);
            Assert.Contains(TrainingType.Recovery, values);
            Assert.Contains(TrainingType.Gym, values);
        }

        [Fact]
        public void TrainingIntensity_HasAllFourLevels()
        {
            var values = Enum.GetValues<TrainingIntensity>();
            Assert.Equal(4, values.Length);
            Assert.Contains(TrainingIntensity.Light, values);
            Assert.Contains(TrainingIntensity.Moderate, values);
            Assert.Contains(TrainingIntensity.Hard, values);
            Assert.Contains(TrainingIntensity.Maximum, values);
        }
    }
}
