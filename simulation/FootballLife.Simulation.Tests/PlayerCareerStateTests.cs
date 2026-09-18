using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerCareerStateTests
    {
        private static readonly Guid TestClubId = Guid.NewGuid();

        [Fact]
        public void PlayerCareerState_ManagerTrust_RejectsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.Starter, 150f, 5000m, 1000000m, 60f));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.Reserve, -25f, 500m, 50000m, 20f));
        }

        [Fact]
        public void PlayerCareerState_ManagerTrust_ClampsToValidRange()
        {
            var highTrust = PlayerCareerState.CreateClamped(TestClubId, SquadStatus.Starter, 150f, 5000m, 1000000m, 60f);
            var lowTrust = PlayerCareerState.CreateClamped(TestClubId, SquadStatus.Reserve, -25f, 500m, 50000m, 20f);

            Assert.Equal(100f, highTrust.ManagerTrust);
            Assert.Equal(0f, lowTrust.ManagerTrust);
            Assert.Equal(100f, PlayerCareerState.ClampTrust(120f));
            Assert.Equal(0f, PlayerCareerState.ClampTrust(-10f));
        }

        [Fact]
        public void PlayerCareerState_Reputation_RejectsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.KeyPlayer, 80f, 15000m, 5000000m, 120f));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCareerState(TestClubId, SquadStatus.Academy, 50f, 200m, 10000m, -10f));
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
        public void PlayerCareerState_WithExpression_RejectsNegativeValues()
        {
            var valid = PlayerCareerState.CreateAcademy(TestClubId);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                valid with { WeeklySalary = -1m });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                valid with { MarketValue = -100m });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                valid with { ManagerTrust = 110f });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                valid with { Reputation = -5f });
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
