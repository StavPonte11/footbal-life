using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class StadiumAtmosphereTests
    {
        [Theory]
        [InlineData(1, StadiumReputationTier.Grassroots)]
        [InlineData(25, StadiumReputationTier.Grassroots)]
        [InlineData(40, StadiumReputationTier.Grassroots)]
        [InlineData(41, StadiumReputationTier.MidTier)]
        [InlineData(60, StadiumReputationTier.MidTier)]
        [InlineData(74, StadiumReputationTier.MidTier)]
        [InlineData(75, StadiumReputationTier.Elite)]
        [InlineData(90, StadiumReputationTier.Elite)]
        [InlineData(100, StadiumReputationTier.Elite)]
        public void GetTierFromReputation_ClassifiesCorrectly(int reputation, StadiumReputationTier expectedTier)
        {
            var tier = StadiumAtmosphereUtility.GetTierFromReputation(reputation);
            Assert.Equal(expectedTier, tier);
        }

        [Theory]
        [InlineData(3500, StadiumReputationTier.Grassroots)]
        [InlineData(11999, StadiumReputationTier.Grassroots)]
        [InlineData(12000, StadiumReputationTier.MidTier)]
        [InlineData(35000, StadiumReputationTier.MidTier)]
        [InlineData(45000, StadiumReputationTier.Elite)]
        [InlineData(75000, StadiumReputationTier.Elite)]
        public void GetTierFromCapacity_ClassifiesCorrectly(int capacity, StadiumReputationTier expectedTier)
        {
            var tier = StadiumAtmosphereUtility.GetTierFromCapacity(capacity);
            Assert.Equal(expectedTier, tier);
        }

        [Fact]
        public void GetTierConfig_Grassroots_HasOpenTerraceAndLowHeight()
        {
            var config = StadiumAtmosphereUtility.GetTierConfig(StadiumReputationTier.Grassroots);

            Assert.Equal(StadiumReputationTier.Grassroots, config.Tier);
            Assert.Equal(1, config.TierCount);
            Assert.False(config.HasCantileverRoof);
            Assert.False(config.HasExecutiveBoxes);
            Assert.True(config.StandHeightMeters < 8.0f);
            Assert.Equal(FloodlightStyle.PoleMasts, config.LightingStyle);
        }

        [Fact]
        public void GetTierConfig_MidTier_HasCoveredStands()
        {
            var config = StadiumAtmosphereUtility.GetTierConfig(StadiumReputationTier.MidTier);

            Assert.Equal(StadiumReputationTier.MidTier, config.Tier);
            Assert.Equal(1, config.TierCount);
            Assert.True(config.HasCantileverRoof);
            Assert.False(config.HasExecutiveBoxes);
            Assert.True(config.StandHeightMeters >= 10.0f);
            Assert.Equal(FloodlightStyle.GantryTowers, config.LightingStyle);
        }

        [Fact]
        public void GetTierConfig_Elite_HasDoubleTierAndExecutiveBoxes()
        {
            var config = StadiumAtmosphereUtility.GetTierConfig(StadiumReputationTier.Elite);

            Assert.Equal(StadiumReputationTier.Elite, config.Tier);
            Assert.Equal(2, config.TierCount);
            Assert.True(config.HasCantileverRoof);
            Assert.True(config.HasExecutiveBoxes);
            Assert.True(config.StandHeightMeters >= 20.0f);
            Assert.Equal(FloodlightStyle.CornerArenaTowers, config.LightingStyle);
        }

        [Fact]
        public void ComputeNetRipple_DecaysOverTime()
        {
            float r0 = Math.Abs(StadiumAtmosphereUtility.ComputeNetRipple(100f, 0.05f));
            float rLate = Math.Abs(StadiumAtmosphereUtility.ComputeNetRipple(100f, 1.5f));
            float rEnd = StadiumAtmosphereUtility.ComputeNetRipple(100f, 2.5f);

            Assert.True(r0 > 0.01f);
            Assert.True(rLate < r0);
            Assert.Equal(0f, rEnd); // Zero beyond duration window
        }

        [Theory]
        [InlineData(0.0f, false, 8)]
        [InlineData(1.0f, false, 30)]
        [InlineData(0.5f, true, 27)]
        [InlineData(1.0f, true, 40)]
        public void ComputeTurfParticleCount_ScalesWithPowerAndType(float power, bool isSlide, int expectedCount)
        {
            int count = StadiumAtmosphereUtility.ComputeTurfParticleCount(power, isSlide);
            Assert.Equal(expectedCount, count);
        }

        [Theory]
        [InlineData(0.5f, 0f)]      // Still or negligible roll
        [InlineData(10.0f, 0.168f)] // Normal pass
        [InlineData(31.0f, 0.28f)]  // Rocket shot at max width
        public void ComputeBallTrailWidth_ScalesWithSpeed(float speed, float expectedWidth)
        {
            float width = StadiumAtmosphereUtility.ComputeBallTrailWidth(speed);
            Assert.Equal(expectedWidth, width, 2);
        }
    }
}
