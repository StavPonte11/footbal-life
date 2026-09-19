using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MatchSituationGeneratorTests
    {
        private static readonly Guid HomeClub = Guid.NewGuid();
        private static readonly Guid AwayClub = Guid.NewGuid();

        private static PlayerState CreateState(float fatigue = 10f) =>
            new PlayerState(
                fatigue: fatigue,
                confidence: 65f,
                form: 70f,
                happiness: 75f,
                motivation: 80f,
                morale: 75f,
                fitness: 85f);

        [Fact]
        public void SituationGenerator_Striker_ReceivesGoalScoringOpportunity()
        {
            var match = MatchState.Create(HomeClub, AwayClub);
            var playerState = CreateState();

            // Over 500 minutes, a striker should receive attacking goal-scoring situations
            var situations = new List<SituationType>();
            for (int seed = 1; seed <= 500; seed++)
            {
                var rng = new SimulationRandom(seed);
                var sit = MatchSituationGenerator.GenerateSituation(match, Position.ST, playerState, rng, playerIsHome: true);
                if (sit != null)
                {
                    situations.Add(sit.Type);
                }
            }

            Assert.NotEmpty(situations);
            Assert.Contains(situations, s => s == SituationType.RunningInBehind || s == SituationType.ReceivingInBox || s == SituationType.LongShot);
        }

        [Fact]
        public void SituationGenerator_Defender_ReceivesTackleAndInterceptionSituations()
        {
            var match = MatchState.Create(HomeClub, AwayClub);
            var playerState = CreateState();

            var situations = new List<SituationType>();
            for (int seed = 1; seed <= 500; seed++)
            {
                var rng = new SimulationRandom(seed);
                var sit = MatchSituationGenerator.GenerateSituation(match, Position.CB, playerState, rng, playerIsHome: true);
                if (sit != null)
                {
                    situations.Add(sit.Type);
                }
            }

            Assert.NotEmpty(situations);
            Assert.Contains(situations, s => s == SituationType.Tackle || s == SituationType.Interception || s == SituationType.AerialChallenge);
        }

        [Fact]
        public void SituationGenerator_Losing2Plus_AttackingPositions_GetHigherFrequency()
        {
            var tiedMatch = MatchState.Create(HomeClub, AwayClub);
            var losingMatch = new MatchState(HomeClub, AwayClub, homeScore: 0, awayScore: 2, minute: 50, isFinished: false);
            var playerState = CreateState();

            int tiedCount = 0;
            int losingCount = 0;

            const int trials = 2000;
            for (int seed = 1; seed <= trials; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                if (MatchSituationGenerator.GenerateSituation(tiedMatch, Position.ST, playerState, rng1, playerIsHome: true) != null)
                {
                    tiedCount++;
                }

                var rng2 = new SimulationRandom(seed);
                if (MatchSituationGenerator.GenerateSituation(losingMatch, Position.ST, playerState, rng2, playerIsHome: true) != null)
                {
                    losingCount++;
                }
            }

            // Losing by 2+ should produce higher attacking opportunity count
            Assert.True(losingCount > tiedCount, $"Losing count ({losingCount}) should exceed tied count ({tiedCount})");
        }

        [Fact]
        public void SituationGenerator_HighFatigue_IncreasesOpponentPressure()
        {
            var match = MatchState.Create(HomeClub, AwayClub);
            var freshState = CreateState(fatigue: 10f);
            var exhaustedState = CreateState(fatigue: 85f);

            float totalFreshPressure = 0f;
            float totalExhaustedPressure = 0f;
            int count = 0;

            for (int seed = 1; seed <= 2000; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var freshSit = MatchSituationGenerator.GenerateSituation(match, Position.CM, freshState, rng1, playerIsHome: true);

                var rng2 = new SimulationRandom(seed);
                var exhaustedSit = MatchSituationGenerator.GenerateSituation(match, Position.CM, exhaustedState, rng2, playerIsHome: true);

                if (freshSit != null && exhaustedSit != null)
                {
                    totalFreshPressure += freshSit.OpponentPressure;
                    totalExhaustedPressure += exhaustedSit.OpponentPressure;
                    count++;
                }
            }

            Assert.True(count > 0);
            float avgFresh = totalFreshPressure / count;
            float avgExhausted = totalExhaustedPressure / count;

            Assert.True(avgExhausted > avgFresh, $"Exhausted pressure ({avgExhausted}) should be higher than fresh ({avgFresh})");
            Assert.True(avgExhausted - avgFresh >= 1.5f, "Fatigue > 70 should add approximately +2 opponent pressure");
        }

        [Fact]
        public void SituationGenerator_DeterminismTest_SameSeedSameSituation()
        {
            var match = MatchState.Create(HomeClub, AwayClub);
            var state = CreateState();

            for (int seed = 1; seed <= 50; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var sit1 = MatchSituationGenerator.GenerateSituation(match, Position.LW, state, rng1, playerIsHome: true);

                var rng2 = new SimulationRandom(seed);
                var sit2 = MatchSituationGenerator.GenerateSituation(match, Position.LW, state, rng2, playerIsHome: true);

                if (sit1 == null)
                {
                    Assert.Null(sit2);
                }
                else
                {
                    Assert.NotNull(sit2);
                    Assert.Equal(sit1.Type, sit2.Type);
                    Assert.Equal(sit1.OpponentPressure, sit2.OpponentPressure);
                    Assert.Equal(sit1.ExpectedDifficulty, sit2.ExpectedDifficulty);
                    Assert.Equal(sit1.AvailableChoices.Length, sit2.AvailableChoices.Length);
                }
            }
        }
    }
}
