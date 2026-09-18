using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerCareerStateTests
    {
        private static readonly Guid TestClubId = Guid.NewGuid();

        [Fact]
        public void PlayerCareerState_ManagerTrust_ClampsToValidRange()
        {
            var highTrust = new PlayerCareerState(TestClubId, SquadStatus.Starter, 150f, 5000m, 1000000m, 60f);
            var lowTrust = new PlayerCareerState(TestClubId, SquadStatus.Reserve, -25f, 500m, 50000m, 20f);

            Assert.Equal(100f, highTrust.ManagerTrust);
            Assert.Equal(0f, lowTrust.ManagerTrust);
        }

        [Fact]
        public void PlayerCareerState_Reputation_ClampsToValidRange()
        {
            var highRep = new PlayerCareerState(TestClubId, SquadStatus.KeyPlayer, 80f, 15000m, 5000000m, 120f);
            var lowRep = new PlayerCareerState(TestClubId, SquadStatus.Academy, 50f, 200m, 10000m, -10f);

            Assert.Equal(100f, highRep.Reputation);
            Assert.Equal(0f, lowRep.Reputation);
        }

        [Fact]
        public void PlayerCareerState_MarketValue_CannotBeNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.Rotation, 60f, 3000m, -100m, 50f));
        }

        [Fact]
        public void PlayerCareerState_WeeklySalary_CannotBeNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.Rotation, 60f, -500m, 100000m, 50f));
        }

        [Fact]
        public void PlayerCareerState_WithExpression_ProducesNewInstance()
        {
            var original = PlayerCareerState.CreateAcademy(TestClubId);
            var promoted = original with { Status = SquadStatus.Starter, ManagerTrust = 85f, WeeklySalary = 4000m };

            Assert.NotSame(original, promoted);
            Assert.Equal(SquadStatus.Academy, original.Status);
            Assert.Equal(SquadStatus.Starter, promoted.Status);
            Assert.Equal(85f, promoted.ManagerTrust);
            Assert.Equal(4000m, promoted.WeeklySalary);
        }

        [Fact]
        public void PlayerCareerState_CreateAcademy_SetsExpectedDefaults()
        {
            var state = PlayerCareerState.CreateAcademy(TestClubId, weeklySalary: 250m, marketValue: 50000m);

            Assert.Equal(TestClubId, state.ClubId);
            Assert.Equal(SquadStatus.Academy, state.Status);
            Assert.Equal(30f, state.ManagerTrust);
            Assert.Equal(250m, state.WeeklySalary);
            Assert.Equal(50000m, state.MarketValue);
            Assert.Equal(10f, state.Reputation);
        }
    }
}
