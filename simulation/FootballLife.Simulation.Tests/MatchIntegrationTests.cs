using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MatchIntegrationTests
    {
        private static readonly Guid HomeClub = Guid.NewGuid();
        private static readonly Guid AwayClub = Guid.NewGuid();
        private static readonly Guid LeagueId = Guid.NewGuid();
        private static readonly ScheduledMatch Fixture = ScheduledMatch.Create(new DateOnly(2026, 9, 1), HomeClub, AwayClub, LeagueId);

        private static PlayerAbilities CreateStrikerAbilities(int shooting = 80) =>
            new PlayerAbilities(
                pace: 80, acceleration: 80, stamina: 75, strength: 75, agility: 75,
                passing: 65, shooting: shooting, dribbling: 75, crossing: 55, firstTouch: 75, tackling: 40,
                vision: 65, composure: 78, positioning: 80, decisionMaking: 72);

        private static PlayerAbilities CreateAverageAbilities() =>
            new PlayerAbilities(
                pace: 60, acceleration: 60, stamina: 65, strength: 60, agility: 60,
                passing: 60, shooting: 60, dribbling: 60, crossing: 60, firstTouch: 60, tackling: 60,
                vision: 60, composure: 60, positioning: 60, decisionMaking: 60);

        private static PlayerState CreateFreshState() =>
            new PlayerState(
                fatigue: 10f,
                confidence: 70f,
                form: 65f,
                happiness: 75f,
                motivation: 80f,
                morale: 75f,
                fitness: 85f);

        private static PlayerState CreateFatiguedState() =>
            new PlayerState(
                fatigue: 85f,
                confidence: 50f,
                form: 45f,
                happiness: 60f,
                motivation: 65f,
                morale: 60f,
                fitness: 50f);

        [Fact]
        public void MatchPipeline_Striker_ScoresSomeGoals_Over100Games()
        {
            var abilities = CreateStrikerAbilities(shooting: 80);
            var state = CreateFreshState();

            int matchesWithGoals = 0;
            const int totalMatches = 100;

            for (int seed = 1; seed <= totalMatches; seed++)
            {
                var rng = new SimulationRandom(seed);
                var (result, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng);
                if (result.PlayerScored)
                {
                    matchesWithGoals++;
                }
            }

            Assert.True(matchesWithGoals >= 25,
                $"Expected striker with 80 shooting to score in at least 25-30 matches out of 100. Actual: {matchesWithGoals}");
        }

        [Fact]
        public void MatchPipeline_HighFatigue_ReducesPerformanceRating_Statistically()
        {
            var abilities = CreateAverageAbilities();
            var freshState = CreateFreshState();
            var fatiguedState = CreateFatiguedState();

            float freshRatingSum = 0f;
            float fatiguedRatingSum = 0f;
            const int totalMatches = 100;

            for (int seed = 1; seed <= totalMatches; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var (freshResult, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, freshState, Position.CM, rng1);
                freshRatingSum += freshResult.PlayerRating;

                var rng2 = new SimulationRandom(seed);
                var (fatiguedResult, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, fatiguedState, Position.CM, rng2);
                fatiguedRatingSum += fatiguedResult.PlayerRating;
            }

            float avgFresh = freshRatingSum / totalMatches;
            float avgFatigued = fatiguedRatingSum / totalMatches;

            Assert.True(avgFresh > avgFatigued,
                $"Average fresh rating ({avgFresh}) must exceed average fatigued rating ({avgFatigued})");
        }

        [Fact]
        public void MatchPipeline_FatigueIncreases_AfterEachMatch()
        {
            var abilities = CreateAverageAbilities();
            var initialState = CreateFreshState();
            var rng = new SimulationRandom(42);

            var (result, updatedState) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, initialState, Position.CM, rng);

            Assert.True(updatedState.Fatigue > initialState.Fatigue,
                $"Fatigue after match ({updatedState.Fatigue}) should exceed initial fatigue ({initialState.Fatigue})");
        }

        [Fact]
        public void MatchPipeline_DeterminismTest_SameSeedProducesSameEntireMatchLog()
        {
            var abilities = CreateStrikerAbilities();
            var state = CreateFreshState();

            for (int seed = 1; seed <= 20; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var (res1, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng1);

                var rng2 = new SimulationRandom(seed);
                var (res2, _) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.ST, rng2);

                Assert.Equal(res1.HomeScore, res2.HomeScore);
                Assert.Equal(res1.AwayScore, res2.AwayScore);
                Assert.Equal(res1.PlayerRating, res2.PlayerRating);
                Assert.Equal(res1.Events.Count, res2.Events.Count);

                for (int i = 0; i < res1.Events.Count; i++)
                {
                    Assert.Equal(res1.Events[i].Minute, res2.Events[i].Minute);
                    Assert.Equal(res1.Events[i].Type, res2.Events[i].Type);
                    Assert.Equal(res1.Events[i].PlayerId, res2.Events[i].PlayerId);
                }
            }
        }

        [Fact]
        public void MatchPipeline_TenSeasonSimulation_TrustConverges_NotRunaway()
        {
            var abilities = CreateAverageAbilities();
            var state = CreateFreshState();
            var career = new PlayerCareerState(
                clubId: HomeClub,
                status: SquadStatus.Rotation,
                managerTrust: 50f,
                weeklySalary: 1000m,
                marketValue: 100000m,
                reputation: 40f);

            var rng = new SimulationRandom(12345);
            // Simulate 380 matches (10 seasons of 38 matches)
            for (int match = 1; match <= 380; match++)
            {
                var (res, updatedState) = MatchSimulator.Simulate(Fixture, HomeClub, abilities, state, Position.CM, rng);
                career = ManagerTrustSystem.ApplyMatchResult(career, res, SquadStatus.Rotation);

                // Simulate weekly recovery between matches
                state = FatigueSystem.ApplyRest(updatedState, hoursSlept: 8);
                state = FatigueSystem.ApplyDayTick(state);
            }

            // Manager trust should remain within sensible boundaries [10, 95], not explode or permanently break
            Assert.InRange(career.ManagerTrust, 10f, 95f);
        }
    }
}
