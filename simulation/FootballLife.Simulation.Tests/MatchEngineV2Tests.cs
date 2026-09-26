using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MatchEngineV2Tests
    {
        private static readonly Guid HomeClub = Guid.NewGuid();
        private static readonly Guid AwayClub = Guid.NewGuid();

        private static PlayerAbilities CreateSpecialistAbilities(
            int shooting = 70,
            int passing = 70,
            int dribbling = 70,
            int crossing = 70,
            int composure = 70,
            int strength = 70,
            int tackling = 70) =>
            new PlayerAbilities(
                pace: 75, acceleration: 75, stamina: 75, strength: strength, agility: 75,
                passing: passing, shooting: shooting, dribbling: dribbling, crossing: crossing,
                firstTouch: 70, tackling: tackling, vision: 70, composure: composure,
                positioning: 70, decisionMaking: 70);

        private static PlayerState CreateState(float fatigue = 10f) =>
            new PlayerState(
                fatigue: fatigue,
                confidence: 70f,
                form: 75f,
                happiness: 80f,
                motivation: 80f,
                morale: 80f,
                fitness: 85f);

        [Fact]
        public void MatchEngineV2_FreeKickSituation_ContainsDirectAndCrossOptions()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.FreeKick);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.FreeKick_Direct);
            Assert.Contains(choices, c => c.Action == MatchAction.FreeKick_Cross);
            Assert.Contains(choices, c => c.Action == MatchAction.ShortPass);
        }

        [Fact]
        public void MatchEngineV2_PenaltyKickSituation_ContainsPenaltyActionWithHighExpectedValue()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.PenaltyKick);

            Assert.Single(choices);
            Assert.Equal(MatchAction.PenaltyKick, choices[0].Action);
            Assert.True(choices[0].ExpectedValue >= 0.70f);
        }

        [Fact]
        public void MatchEngineV2_CornerKickSituation_ContainsCornerDeliveryOption()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.CornerKick);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.CornerDelivery);
            Assert.Contains(choices, c => c.Action == MatchAction.ShortPass);
        }

        [Fact]
        public void MatchEngineV2_Dribbling1v1Situation_ContainsSkillMoveAndDribble()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.Dribbling1v1);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.SkillMove);
            Assert.Contains(choices, c => c.Action == MatchAction.Dribble);
            Assert.Contains(choices, c => c.Action == MatchAction.ShortPass);
        }

        [Fact]
        public void MatchEngineV2_GKOneOnOne_ContainsChipShotAndCloseShot()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.GKOneOnOne);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.ChipShot);
            Assert.Contains(choices, c => c.Action == MatchAction.Shot_Close);
            Assert.Contains(choices, c => c.Action == MatchAction.Dribble);
        }

        [Fact]
        public void MatchEngineV2_HeaderOpportunity_ContainsDivingHeaderAndVolley()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.HeaderOpportunity);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.Header);
            Assert.Contains(choices, c => c.Action == MatchAction.DivingHeader);
            Assert.Contains(choices, c => c.Action == MatchAction.AerialChallenge);
        }

        [Fact]
        public void MatchEngineV2_CounterAttackRun_ContainsFastTransitionOptions()
        {
            var choices = MatchSituationGenerator.GetChoicesForSituation(SituationType.CounterAttackRun);

            Assert.NotEmpty(choices);
            Assert.Contains(choices, c => c.Action == MatchAction.ThroughBall);
            Assert.Contains(choices, c => c.Action == MatchAction.Dribble);
            Assert.Contains(choices, c => c.Action == MatchAction.Shot_Long);
        }

        [Fact]
        public void MatchEngineV2_ActionResolver_FreeKickDirect_HighShootingOutperformsLow()
        {
            var lowFK = CreateSpecialistAbilities(shooting: 40, composure: 40);
            var highFK = CreateSpecialistAbilities(shooting: 90, composure: 90);
            var state = CreateState();
            var situation = new MatchSituation(
                type: SituationType.FreeKick,
                opponentPressure: 3.0f,
                expectedDifficulty: 0.70f,
                positionalAdvantage: 0.1f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.FreeKick));

            int lowGoals = 0;
            int highGoals = 0;
            const int trials = 1000;

            for (int seed = 1; seed <= trials; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var outcome1 = ActionResolver.Resolve(MatchAction.FreeKick_Direct, situation, lowFK, state, rng1);
                if (outcome1.Success && outcome1.Type == OutcomeType.FreeKickGoal)
                {
                    lowGoals++;
                }

                var rng2 = new SimulationRandom(seed);
                var outcome2 = ActionResolver.Resolve(MatchAction.FreeKick_Direct, situation, highFK, state, rng2);
                if (outcome2.Success && outcome2.Type == OutcomeType.FreeKickGoal)
                {
                    highGoals++;
                }
            }

            Assert.True(highGoals > lowGoals, $"High FK goals ({highGoals}) must exceed low FK goals ({lowGoals})");
            Assert.True(highGoals - lowGoals >= 150, "High FK ability should significantly outperform low FK ability");
        }

        [Fact]
        public void MatchEngineV2_ActionResolver_PenaltyKick_ReturnsPenaltyGoalOrPenaltyMissed()
        {
            var abilities = CreateSpecialistAbilities(shooting: 80, composure: 80);
            var state = CreateState();
            var situation = new MatchSituation(
                type: SituationType.PenaltyKick,
                opponentPressure: 4.0f,
                expectedDifficulty: 0.40f,
                positionalAdvantage: 0.5f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.PenaltyKick));

            int goals = 0;
            int misses = 0;
            const int trials = 500;

            for (int seed = 1; seed <= trials; seed++)
            {
                var rng = new SimulationRandom(seed);
                var outcome = ActionResolver.Resolve(MatchAction.PenaltyKick, situation, abilities, state, rng);
                if (outcome.Success)
                {
                    Assert.Equal(OutcomeType.PenaltyGoal, outcome.Type);
                    goals++;
                }
                else
                {
                    Assert.Equal(OutcomeType.PenaltyMissed, outcome.Type);
                    misses++;
                }
            }

            Assert.True(goals > misses, $"Penalties should have high success rate (Goals: {goals}, Misses: {misses})");
            Assert.True(goals >= 350, $"Expected at least 70% penalty conversion, got {goals}/{trials}");
        }

        [Fact]
        public void MatchEngineV2_ActionResolver_SkillMove_ReturnsSkillBeatDefenderOutcome()
        {
            var abilities = CreateSpecialistAbilities(dribbling: 90);
            var state = CreateState();
            var situation = new MatchSituation(
                type: SituationType.Dribbling1v1,
                opponentPressure: 3.0f,
                expectedDifficulty: 0.55f,
                positionalAdvantage: 0.2f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.Dribbling1v1));

            var rng = new SimulationRandom(42);
            bool beatDefenderFound = false;

            for (int i = 0; i < 50; i++)
            {
                var outcome = ActionResolver.Resolve(MatchAction.SkillMove, situation, abilities, state, rng);
                if (outcome.Success && outcome.Type == OutcomeType.SkillBeatDefender)
                {
                    beatDefenderFound = true;
                    break;
                }
            }

            Assert.True(beatDefenderFound, "Expected at least one SkillBeatDefender outcome with high dribbling");
        }

        [Fact]
        public void MatchEngineV2_ActionResolver_BlockShot_ReturnsBlockMadeOutcome()
        {
            var abilities = CreateSpecialistAbilities(tackling: 85);
            var state = CreateState();
            var situation = new MatchSituation(
                type: SituationType.Tackle,
                opponentPressure: 4.0f,
                expectedDifficulty: 0.55f,
                positionalAdvantage: 0.0f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.Tackle));

            var rng = new SimulationRandom(101);
            bool blockFound = false;

            for (int i = 0; i < 50; i++)
            {
                var outcome = ActionResolver.Resolve(MatchAction.BlockShot, situation, abilities, state, rng);
                if (outcome.Success && outcome.Type == OutcomeType.BlockMade)
                {
                    blockFound = true;
                    break;
                }
            }

            Assert.True(blockFound, "Expected at least one BlockMade outcome with high defensive ability");
        }

        [Fact]
        public void MatchEngineV2_DeterministicOutcomes_SameSeedProducesIdenticalResults()
        {
            var abilities = CreateSpecialistAbilities();
            var state = CreateState();
            var situation = new MatchSituation(
                type: SituationType.FreeKick,
                opponentPressure: 3.5f,
                expectedDifficulty: 0.65f,
                positionalAdvantage: 0.1f,
                availableChoices: MatchSituationGenerator.GetChoicesForSituation(SituationType.FreeKick));

            for (int seed = 1; seed <= 50; seed++)
            {
                var rngA = new SimulationRandom(seed);
                var rngB = new SimulationRandom(seed);

                var outcomeA = ActionResolver.Resolve(MatchAction.FreeKick_Direct, situation, abilities, state, rngA);
                var outcomeB = ActionResolver.Resolve(MatchAction.FreeKick_Direct, situation, abilities, state, rngB);

                Assert.Equal(outcomeA.Success, outcomeB.Success);
                Assert.Equal(outcomeA.Type, outcomeB.Type);
                Assert.Equal(outcomeA.ConfidenceDelta, outcomeB.ConfidenceDelta);
                Assert.Equal(outcomeA.ManagerTrustDelta, outcomeB.ManagerTrustDelta);
            }
        }
    }
}
