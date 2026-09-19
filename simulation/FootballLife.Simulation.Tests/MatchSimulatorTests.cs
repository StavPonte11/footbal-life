using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MatchSimulatorTests
    {
        private static readonly Guid HomeClub = Guid.NewGuid();
        private static readonly Guid AwayClub = Guid.NewGuid();
        private static readonly Guid LeagueId = Guid.NewGuid();
        private static readonly ScheduledMatch Fixture = ScheduledMatch.Create(new DateOnly(2026, 8, 15), HomeClub, AwayClub, LeagueId);

        private static PlayerAbilities CreateStrikerAbilities(int shooting = 85) =>
            new PlayerAbilities(
                pace: 80, acceleration: 82, stamina: 75, strength: 70, agility: 78,
                passing: 70, shooting: shooting, dribbling: 78, crossing: 60, firstTouch: 78, tackling: 40,
                vision: 70, composure: 80, positioning: 82, decisionMaking: 75);

        private static PlayerState CreateState() =>
            new PlayerState(
                fatigue: 15f,
                confidence: 70f,
                form: 70f,
                happiness: 75f,
                motivation: 80f,
                morale: 75f,
                fitness: 85f);

        [Fact]
        public void MatchSimulator_Striker_CanScoreGoal()
        {
            var abilities = CreateStrikerAbilities(shooting: 90);
            var state = CreateState();

            bool scored = false;
            // Over 20 matches, a 90-shooting striker should score in at least one match
            for (int seed = 1; seed <= 20; seed++)
            {
                var rng = new SimulationRandom(seed);
                var (result, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng);
                if (result.PlayerScored)
                {
                    scored = true;
                    Assert.True(result.PlayerRating >= 6.5f, "Goal scorer should have high rating");
                    break;
                }
            }

            Assert.True(scored, "Expected striker to score at least once across 20 matches");
        }

        [Fact]
        public void MatchSimulator_90MinutesSimulated_ProducesValidRating()
        {
            var abilities = CreateStrikerAbilities();
            var state = CreateState();

            for (int seed = 1; seed <= 50; seed++)
            {
                var rng = new SimulationRandom(seed);
                var (result, updatedState) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng);

                Assert.Equal(90, result.PlayerMinutesPlayed);
                Assert.InRange(result.PlayerRating, 1.0f, 10.0f);
                Assert.InRange(result.HomeScore, 0, 15);
                Assert.InRange(result.AwayScore, 0, 15);

                // Fatigue should increase after match
                Assert.True(updatedState.Fatigue > state.Fatigue);
                Assert.InRange(updatedState.Confidence, 0f, 100f);
                Assert.InRange(updatedState.Form, 0f, 100f);
            }
        }

        [Fact]
        public void MatchSimulator_DeterminismTest_SameSeedSameResult()
        {
            var abilities = CreateStrikerAbilities();
            var state = CreateState();

            for (int seed = 1; seed <= 30; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var (result1, state1) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng1);

                var rng2 = new SimulationRandom(seed);
                var (result2, state2) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng2);

                Assert.Equal(result1.HomeScore, result2.HomeScore);
                Assert.Equal(result1.AwayScore, result2.AwayScore);
                Assert.Equal(result1.PlayerRating, result2.PlayerRating);
                Assert.Equal(result1.PlayerScored, result2.PlayerScored);
                Assert.Equal(result1.PlayerAssisted, result2.PlayerAssisted);
                Assert.Equal(result1.Events.Count, result2.Events.Count);
                Assert.Equal(state1.Fatigue, state2.Fatigue);
                Assert.Equal(state1.Form, state2.Form);
                Assert.Equal(state1.Confidence, state2.Confidence);
            }
        }
    }
}
