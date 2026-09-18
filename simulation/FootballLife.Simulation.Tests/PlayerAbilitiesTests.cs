using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerAbilitiesTests
    {
        [Fact]
        public void PlayerAbilities_Default_HasBaseline50ForAllAttributes()
        {
            var abilities = PlayerAbilities.Default;

            Assert.Equal(50, abilities.Pace);
            Assert.Equal(50, abilities.Acceleration);
            Assert.Equal(50, abilities.Stamina);
            Assert.Equal(50, abilities.Strength);
            Assert.Equal(50, abilities.Agility);

            Assert.Equal(50, abilities.Passing);
            Assert.Equal(50, abilities.Shooting);
            Assert.Equal(50, abilities.Dribbling);
            Assert.Equal(50, abilities.Crossing);
            Assert.Equal(50, abilities.FirstTouch);
            Assert.Equal(50, abilities.Tackling);

            Assert.Equal(50, abilities.Vision);
            Assert.Equal(50, abilities.Composure);
            Assert.Equal(50, abilities.Positioning);
            Assert.Equal(50, abilities.DecisionMaking);
            Assert.Equal(50f, abilities.CalculateAverage());
        }

        [Fact]
        public void PlayerAbilities_CannotExceedMaximumBounds()
        {
            // Passing 150 (greater than MaxValue of 100) must clamp to 100
            var abilities = new PlayerAbilities(
                pace: 150,
                acceleration: 200,
                stamina: 101,
                strength: 999,
                agility: 120,
                passing: 105,
                shooting: 110,
                dribbling: 150,
                crossing: 180,
                firstTouch: 190,
                tackling: 130,
                vision: 140,
                composure: 111,
                positioning: 102,
                decisionMaking: 500);

            Assert.Equal(100, abilities.Pace);
            Assert.Equal(100, abilities.Acceleration);
            Assert.Equal(100, abilities.Stamina);
            Assert.Equal(100, abilities.Strength);
            Assert.Equal(100, abilities.Agility);

            Assert.Equal(100, abilities.Passing);
            Assert.Equal(100, abilities.Shooting);
            Assert.Equal(100, abilities.Dribbling);
            Assert.Equal(100, abilities.Crossing);
            Assert.Equal(100, abilities.FirstTouch);
            Assert.Equal(100, abilities.Tackling);

            Assert.Equal(100, abilities.Vision);
            Assert.Equal(100, abilities.Composure);
            Assert.Equal(100, abilities.Positioning);
            Assert.Equal(100, abilities.DecisionMaking);
        }

        [Fact]
        public void PlayerAbilities_CannotBeBelowZero()
        {
            // Passing negative values must clamp to 0
            var abilities = new PlayerAbilities(
                pace: -10,
                acceleration: -1,
                stamina: -50,
                strength: -100,
                agility: -25,
                passing: -5,
                shooting: -30,
                dribbling: -4,
                crossing: -80,
                firstTouch: -12,
                tackling: -99,
                vision: -44,
                composure: -15,
                positioning: -7,
                decisionMaking: -200);

            Assert.Equal(0, abilities.Pace);
            Assert.Equal(0, abilities.Acceleration);
            Assert.Equal(0, abilities.Stamina);
            Assert.Equal(0, abilities.Strength);
            Assert.Equal(0, abilities.Agility);

            Assert.Equal(0, abilities.Passing);
            Assert.Equal(0, abilities.Shooting);
            Assert.Equal(0, abilities.Dribbling);
            Assert.Equal(0, abilities.Crossing);
            Assert.Equal(0, abilities.FirstTouch);
            Assert.Equal(0, abilities.Tackling);

            Assert.Equal(0, abilities.Vision);
            Assert.Equal(0, abilities.Composure);
            Assert.Equal(0, abilities.Positioning);
            Assert.Equal(0, abilities.DecisionMaking);
            Assert.Equal(0f, abilities.CalculateAverage());
        }

        [Fact]
        public void PlayerAbilities_WithExpression_ProducesNewInstance()
        {
            var original = PlayerAbilities.Default;
            var updated = original with { Pace = 85, Shooting = 90 };

            // Original remains unchanged
            Assert.Equal(50, original.Pace);
            Assert.Equal(50, original.Shooting);

            // New instance has updated values
            Assert.Equal(85, updated.Pace);
            Assert.Equal(90, updated.Shooting);

            // Other fields remain unchanged
            Assert.Equal(50, updated.Passing);
            Assert.NotSame(original, updated);
        }

        [Fact]
        public void PlayerAbilities_IsImmutable_WithExpressionProducesNewInstance()
        {
            var original = PlayerAbilities.CreateUniform(70);
            var modified = original with { Vision = 95, Tackling = 40 };

            Assert.NotSame(original, modified);
            Assert.Equal(70, original.Vision);
            Assert.Equal(70, original.Tackling);
            Assert.Equal(95, modified.Vision);
            Assert.Equal(40, modified.Tackling);
        }

        [Fact]
        public void PlayerAbilities_WithExpression_ClampsOutOfBoundsValues()
        {
            var baseline = PlayerAbilities.Default;
            var clampedUpper = baseline with { Pace = 250 };
            var clampedLower = baseline with { Stamina = 0 }; // byte cannot be negative, test 0 and Clamp

            Assert.Equal(100, clampedUpper.Pace);
            Assert.Equal(0, clampedLower.Stamina);
        }

        [Fact]
        public void PlayerAbilities_Equality_WorksCorrectly()
        {
            var a = new PlayerAbilities(70, 75, 80, 65, 72, 85, 90, 88, 76, 82, 45, 84, 78, 86, 80);
            var b = new PlayerAbilities(70, 75, 80, 65, 72, 85, 90, 88, 76, 82, 45, 84, 78, 86, 80);
            var c = a with { Pace = 71 };

            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }

        [Fact]
        public void PlayerAbilities_CalculateAverage_ReturnsCorrectMean()
        {
            // 15 attributes with known sum: 15 * 60 = 900 => average 60.0
            var abilities = PlayerAbilities.CreateUniform(60);
            Assert.Equal(60f, abilities.CalculateAverage(), precision: 2);
        }
    }
}
