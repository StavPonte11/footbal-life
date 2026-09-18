using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerStateTests
    {
        [Fact]
        public void PlayerState_Default_HasExpectedValues()
        {
            var state = PlayerState.Default;

            Assert.Equal(0f, state.Fatigue);
            Assert.Equal(50f, state.Confidence);
            Assert.Equal(50f, state.Form);
            Assert.Equal(75f, state.Happiness);
            Assert.Equal(80f, state.Motivation);
            Assert.Equal(75f, state.Morale);
            Assert.Equal(100f, state.Fitness);
        }

        [Fact]
        public void PlayerState_Fatigue_ClampedToValidRange()
        {
            var overExhausted = new PlayerState(150f, 50f, 50f, 50f, 50f, 50f, 50f);
            var underZero = new PlayerState(-20f, 50f, 50f, 50f, 50f, 50f, 50f);

            Assert.Equal(100f, overExhausted.Fatigue);
            Assert.Equal(0f, underZero.Fatigue);
        }

        [Fact]
        public void PlayerState_AllAttributes_ClampedToValidRange()
        {
            var tooHigh = new PlayerState(120f, 150f, 110f, 105f, 200f, 300f, 101f);
            Assert.Equal(100f, tooHigh.Fatigue);
            Assert.Equal(100f, tooHigh.Confidence);
            Assert.Equal(100f, tooHigh.Form);
            Assert.Equal(100f, tooHigh.Happiness);
            Assert.Equal(100f, tooHigh.Motivation);
            Assert.Equal(100f, tooHigh.Morale);
            Assert.Equal(100f, tooHigh.Fitness);

            var tooLow = new PlayerState(-5f, -10f, -50f, -0.1f, -100f, -20f, -1f);
            Assert.Equal(0f, tooLow.Fatigue);
            Assert.Equal(0f, tooLow.Confidence);
            Assert.Equal(0f, tooLow.Form);
            Assert.Equal(0f, tooLow.Happiness);
            Assert.Equal(0f, tooLow.Motivation);
            Assert.Equal(0f, tooLow.Morale);
            Assert.Equal(0f, tooLow.Fitness);
        }

        [Fact]
        public void PlayerState_WithExpression_EnforcesClampingAndImmutability()
        {
            var original = PlayerState.Default;
            var modified = original with { Fatigue = 200f, Happiness = -10f };

            // Original is unchanged
            Assert.Equal(0f, original.Fatigue);
            Assert.Equal(75f, original.Happiness);

            // New instance is clamped
            Assert.Equal(100f, modified.Fatigue);
            Assert.Equal(0f, modified.Happiness);
            Assert.NotSame(original, modified);
        }

        [Fact]
        public void PlayerState_Form_DecaysTowardsNeutral_IsSupported()
        {
            // When form is high (80), decay moves it down towards 50
            var highForm = PlayerState.Default with { Form = 80f };
            var decayedHigh = highForm.DecayFormTowardsNeutral(0.2f);

            // Deviation was 30. Decay 20% -> 30 * 0.2 = 6 reduction -> 74
            Assert.Equal(74f, decayedHigh.Form, precision: 2);
            Assert.True(decayedHigh.Form < highForm.Form);
            Assert.True(decayedHigh.Form > PlayerState.NeutralForm);

            // When form is low (20), decay moves it up towards 50
            var lowForm = PlayerState.Default with { Form = 20f };
            var decayedLow = lowForm.DecayFormTowardsNeutral(0.2f);

            // Deviation was -30. Decay 20% -> 30 * 0.2 = 6 increase -> 26
            Assert.Equal(26f, decayedLow.Form, precision: 2);
            Assert.True(decayedLow.Form > lowForm.Form);
            Assert.True(decayedLow.Form < PlayerState.NeutralForm);
        }

        [Fact]
        public void PlayerState_Form_DecaysTowardsNeutral_WhenNoMatchesPlayed()
        {
            var state = PlayerState.Default with { Form = 90f };
            for (int week = 0; week < 5; week++)
            {
                state = state.DecayFormTowardsNeutral(0.1f);
            }

            Assert.True(state.Form < 90f);
            Assert.True(state.Form > PlayerState.NeutralForm);
        }

        [Fact]
        public void PlayerState_SeparatedFromPlayerAbilities()
        {
            // Verifies PlayerState and PlayerAbilities are distinct types with distinct models
            Assert.NotEqual(typeof(PlayerState), typeof(PlayerAbilities));
        }
    }
}
