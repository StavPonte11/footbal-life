using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ActionResolverTests
    {
        private static PlayerAbilities CreateAbilities(int shooting = 50, int passing = 50, int tackling = 50) =>
            new PlayerAbilities(
                pace: 60, acceleration: 60, stamina: 65, strength: 60, agility: 60,
                passing: passing, shooting: shooting, dribbling: 60, crossing: 50, firstTouch: 60, tackling: tackling,
                vision: 55, composure: 60, positioning: 60, decisionMaking: 60);

        private static PlayerState CreateState(float fatigue = 10f, float form = 60f, float confidence = 60f) =>
            new PlayerState(
                fatigue: fatigue,
                confidence: confidence,
                form: form,
                happiness: 70f,
                motivation: 70f,
                morale: 70f,
                fitness: 80f);

        private static MatchSituation CreateSituation(float pressure = 4.0f) =>
            new MatchSituation(
                type: SituationType.RunningInBehind,
                opponentPressure: pressure,
                expectedDifficulty: 0.6f,
                positionalAdvantage: 0.2f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.RunningInBehind));

        [Fact]
        public void ActionResolver_HighFinishing_IncreasesGoalProbability()
        {
            var lowShooting = CreateAbilities(shooting: 40);
            var highShooting = CreateAbilities(shooting: 85);
            var state = CreateState();
            var situation = CreateSituation();

            int lowGoals = 0;
            int highGoals = 0;
            const int trials = 1000;

            for (int seed = 1; seed <= trials; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var outcome1 = ActionResolver.Resolve(MatchAction.Shot_Close, situation, lowShooting, state, rng1);
                if (outcome1.Success && outcome1.Type == OutcomeType.Goal)
                {
                    lowGoals++;
                }

                var rng2 = new SimulationRandom(seed);
                var outcome2 = ActionResolver.Resolve(MatchAction.Shot_Close, situation, highShooting, state, rng2);
                if (outcome2.Success && outcome2.Type == OutcomeType.Goal)
                {
                    highGoals++;
                }
            }

            Assert.True(highGoals > lowGoals, $"High finishing goals ({highGoals}) must exceed low finishing goals ({lowGoals})");
            Assert.True(highGoals - lowGoals >= 200, "High finishing (85) should significantly outperform low finishing (40)");
        }

        [Fact]
        public void ActionResolver_HighFatigue_ReducesSuccessRate()
        {
            var abilities = CreateAbilities(passing: 75);
            var freshState = CreateState(fatigue: 10f);
            var exhaustedState = CreateState(fatigue: 90f);
            var situation = CreateSituation();

            int freshSuccesses = 0;
            int exhaustedSuccesses = 0;
            const int trials = 1000;

            for (int seed = 1; seed <= trials; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var outcome1 = ActionResolver.Resolve(MatchAction.ShortPass, situation, abilities, freshState, rng1);
                if (outcome1.Success) freshSuccesses++;

                var rng2 = new SimulationRandom(seed);
                var outcome2 = ActionResolver.Resolve(MatchAction.ShortPass, situation, abilities, exhaustedState, rng2);
                if (outcome2.Success) exhaustedSuccesses++;
            }

            Assert.True(freshSuccesses > exhaustedSuccesses,
                $"Fresh successes ({freshSuccesses}) must exceed exhausted successes ({exhaustedSuccesses})");
        }

        [Fact]
        public void ActionResolver_RiskyAction_HasHigherVariance_ThanSafeAction()
        {
            float safeStdDev = ActionResolver.GetStandardDeviation(MatchAction.ShortPass);
            float riskyStdDev = ActionResolver.GetStandardDeviation(MatchAction.Shot_Long);

            Assert.Equal(5.0f, safeStdDev);
            Assert.Equal(15.0f, riskyStdDev);
            Assert.True(riskyStdDev > safeStdDev);
        }

        [Fact]
        public void ActionResolver_Goal_IncreasesConfidenceAndTrust()
        {
            var abilities = CreateAbilities(shooting: 95);
            var state = CreateState(confidence: 50f);
            var situation = CreateSituation(pressure: 1.0f);

            // Find a seed where Shot_Close succeeds
            bool foundGoal = false;
            for (int seed = 1; seed <= 50; seed++)
            {
                var rng = new SimulationRandom(seed);
                var outcome = ActionResolver.Resolve(MatchAction.Shot_Close, situation, abilities, state, rng);
                if (outcome.Success && outcome.Type == OutcomeType.Goal)
                {
                    Assert.True(outcome.ConfidenceDelta > 0f, "Successful goal must produce positive confidence delta");
                    Assert.Equal(5.0f, outcome.ManagerTrustDelta);
                    Assert.True(outcome.XpContribution > 0f);
                    foundGoal = true;
                    break;
                }
            }

            Assert.True(foundGoal, "Expected at least one successful goal in 50 trials with 95 shooting");
        }

        [Fact]
        public void ActionResolver_DeterminismTest_IdenticalInputs_IdenticalOutcome()
        {
            var abilities = CreateAbilities(shooting: 70, passing: 70);
            var state = CreateState();
            var situation = CreateSituation();

            for (int seed = 1; seed <= 50; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var outcome1 = ActionResolver.Resolve(MatchAction.Shot_Close, situation, abilities, state, rng1);

                var rng2 = new SimulationRandom(seed);
                var outcome2 = ActionResolver.Resolve(MatchAction.Shot_Close, situation, abilities, state, rng2);

                Assert.Equal(outcome1.Success, outcome2.Success);
                Assert.Equal(outcome1.Type, outcome2.Type);
                Assert.Equal(outcome1.XpContribution, outcome2.XpContribution);
                Assert.Equal(outcome1.ConfidenceDelta, outcome2.ConfidenceDelta);
                Assert.Equal(outcome1.ManagerTrustDelta, outcome2.ManagerTrustDelta);
            }
        }
    }
}
