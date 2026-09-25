using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class RetirementSystemTests
    {
        private readonly RetirementSystem _system = new();

        [Fact]
        public void IsEligibleForRetirement_RequiresAge32OrOlder()
        {
            Assert.False(_system.IsEligibleForRetirement(25));
            Assert.False(_system.IsEligibleForRetirement(31));
            Assert.True(_system.IsEligibleForRetirement(32));
            Assert.True(_system.IsEligibleForRetirement(37));
        }

        [Fact]
        public void ApplyLateCareerDecline_DoesNotAffectYoungPlayers()
        {
            var initial = PlayerAbilities.CreateUniform(80);
            var random = new SimulationRandom(42);

            var result = _system.ApplyLateCareerDecline(initial, playerAge: 27, random);

            Assert.Equal(initial.Pace, result.Pace);
            Assert.Equal(initial.Stamina, result.Stamina);
            Assert.Equal(initial.Vision, result.Vision);
        }

        [Fact]
        public void ApplyLateCareerDecline_DegradesPhysicalAttributesMoreThanMental()
        {
            var initial = PlayerAbilities.CreateUniform(85);
            var random = new SimulationRandom(101);

            var aged = _system.ApplyLateCareerDecline(initial, playerAge: 35, random);

            // Physical attributes should drop significantly
            int paceDiff = initial.Pace - aged.Pace;
            int staminaDiff = initial.Stamina - aged.Stamina;
            int visionDiff = initial.Vision - aged.Vision;

            Assert.True(paceDiff >= 2);
            Assert.True(staminaDiff >= 3);
            Assert.True(visionDiff <= 2);
            Assert.True(staminaDiff > visionDiff);
        }

        [Fact]
        public void ApplyLateCareerDecline_DeterministicAcrossIdenticalSeeds()
        {
            var initial = PlayerAbilities.CreateUniform(80);

            var aged1 = _system.ApplyLateCareerDecline(initial, playerAge: 38, new SimulationRandom(555));
            var aged2 = _system.ApplyLateCareerDecline(initial, playerAge: 38, new SimulationRandom(555));

            Assert.Equal(aged1.Pace, aged2.Pace);
            Assert.Equal(aged1.Stamina, aged2.Stamina);
            Assert.Equal(aged1.Vision, aged2.Vision);
        }

        [Fact]
        public void EvaluateContractMaxYears_RestrictsOlderVeterans()
        {
            Assert.Equal(4, _system.EvaluateContractMaxYears(28));
            Assert.Equal(3, _system.EvaluateContractMaxYears(31));
            Assert.Equal(2, _system.EvaluateContractMaxYears(33));
            Assert.Equal(1, _system.EvaluateContractMaxYears(36));
        }

        [Fact]
        public void RetirePlayer_CreatesValidDecisionAndStatement()
        {
            var player = new Player(Guid.NewGuid(), "Steven Gerrard", "England", new DateOnly(1980, 5, 30), Foot.Right, Position.CM);

            var decision = _system.RetirePlayer(
                player,
                season: 15,
                currentAge: 35,
                reason: RetirementReason.VoluntaryAtPeak,
                chosenRole: PostPlayingRole.Manager);

            Assert.True(decision.IsRetired);
            Assert.Equal(35, decision.RetirementAge);
            Assert.Equal(15, decision.RetirementSeason);
            Assert.Equal(RetirementReason.VoluntaryAtPeak, decision.Reason);
            Assert.Equal(PostPlayingRole.Manager, decision.ChosenRole);
            Assert.Contains("Steven Gerrard", decision.Statement);
            Assert.Contains("football management", decision.Statement);
        }
    }
}
