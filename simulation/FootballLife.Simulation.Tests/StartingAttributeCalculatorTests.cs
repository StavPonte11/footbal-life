using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class StartingAttributeCalculatorTests
    {
        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void CalculateStartingAbilities_ProducesRealisticRookieOverall(Position position)
        {
            var abilities = StartingAttributeCalculator.CalculateStartingAbilities(position, varianceSeed: 12345);
            int overall = StartingAttributeCalculator.CalculateOverall(position, abilities);

            // Starting rookie rating should be in the range [57, 64]
            Assert.InRange(overall, 57, 65);
        }

        [Fact]
        public void Striker_HasHigherShootingThanTackling()
        {
            var stAbilities = StartingAttributeCalculator.CalculateStartingAbilities(Position.ST, varianceSeed: 42);

            Assert.True(stAbilities.Shooting > stAbilities.Tackling, 
                $"Expected ST Shooting ({stAbilities.Shooting}) > Tackling ({stAbilities.Tackling})");
        }

        [Fact]
        public void CenterBack_HasHigherTacklingThanShooting()
        {
            var cbAbilities = StartingAttributeCalculator.CalculateStartingAbilities(Position.CB, varianceSeed: 42);

            Assert.True(cbAbilities.Tackling > cbAbilities.Shooting, 
                $"Expected CB Tackling ({cbAbilities.Tackling}) > Shooting ({cbAbilities.Shooting})");
        }

        [Fact]
        public void CentralMidfielder_HasHighPassingAndVision()
        {
            var cmAbilities = StartingAttributeCalculator.CalculateStartingAbilities(Position.CM, varianceSeed: 42);

            Assert.True(cmAbilities.Passing >= 58, $"Expected CM Passing >= 58, got {cmAbilities.Passing}");
            Assert.True(cmAbilities.Vision >= 56, $"Expected CM Vision >= 56, got {cmAbilities.Vision}");
        }
    }
}
