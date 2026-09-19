using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ManagerTrustSystemTests
    {
        private static readonly Guid ClubId = Guid.NewGuid();

        private static PlayerCareerState CreateCareerState(float trust = 50f, SquadStatus status = SquadStatus.Rotation) =>
            new PlayerCareerState(
                clubId: ClubId,
                status: status,
                managerTrust: trust,
                weeklySalary: 1000m,
                marketValue: 100000m,
                reputation: 30f);

        private static MatchResult CreateResult(float rating) =>
            new MatchResult(
                homeScore: 1,
                awayScore: 0,
                events: Array.Empty<MatchEvent>(),
                playerRating: rating,
                playerMinutesPlayed: 90,
                playerScored: rating >= 7.5f,
                playerAssisted: false);

        [Fact]
        public void ManagerTrustSystem_ExcellentPerformance_IncreasesTrust()
        {
            var career = CreateCareerState(trust: 50f);
            var result = CreateResult(rating: 8.5f);

            var updated = ManagerTrustSystem.ApplyMatchResult(career, result, SquadStatus.Starter);

            Assert.Equal(54f, updated.ManagerTrust); // 50 + 4
        }

        [Fact]
        public void ManagerTrustSystem_GoodPerformance_IncreasesTrustModerately()
        {
            var career = CreateCareerState(trust: 50f);
            var result = CreateResult(rating: 7.2f);

            var updated = ManagerTrustSystem.ApplyMatchResult(career, result, SquadStatus.Starter);

            Assert.Equal(51.5f, updated.ManagerTrust); // 50 + 1.5
        }

        [Fact]
        public void ManagerTrustSystem_PoorPerformance_DecreasesTrust()
        {
            var career = CreateCareerState(trust: 50f);
            var result = CreateResult(rating: 4.0f);

            var updated = ManagerTrustSystem.ApplyMatchResult(career, result, SquadStatus.Starter);

            Assert.Equal(48f, updated.ManagerTrust); // 50 - 2
        }

        [Fact]
        public void ManagerTrustSystem_DreadfulPerformance_DecreasesTrustSignificantly()
        {
            var career = CreateCareerState(trust: 50f);
            var result = CreateResult(rating: 2.5f);

            var updated = ManagerTrustSystem.ApplyMatchResult(career, result, SquadStatus.Starter);

            Assert.Equal(45f, updated.ManagerTrust); // 50 - 5
        }

        [Fact]
        public void ManagerTrustSystem_KeyPlayerExpectations_AmplifyPenalty()
        {
            var normalCareer = CreateCareerState(trust: 50f, status: SquadStatus.Starter);
            var keyPlayerCareer = CreateCareerState(trust: 50f, status: SquadStatus.KeyPlayer);
            var poorResult = CreateResult(rating: 4.0f); // Base delta: -2f

            var normalUpdated = ManagerTrustSystem.ApplyMatchResult(normalCareer, poorResult, SquadStatus.Starter);
            var keyPlayerUpdated = ManagerTrustSystem.ApplyMatchResult(keyPlayerCareer, poorResult, SquadStatus.KeyPlayer);

            Assert.Equal(48f, normalUpdated.ManagerTrust); // -2.0
            Assert.Equal(47f, keyPlayerUpdated.ManagerTrust); // -2.0 * 1.5 = -3.0
            Assert.True(normalUpdated.ManagerTrust > keyPlayerUpdated.ManagerTrust);
        }

        [Fact]
        public void ManagerTrustSystem_TrustNeverExceeds100()
        {
            var highCareer = CreateCareerState(trust: 98f);
            var greatResult = CreateResult(rating: 9.0f); // +4 delta

            var updated = ManagerTrustSystem.ApplyMatchResult(highCareer, greatResult, SquadStatus.Starter);

            Assert.Equal(100f, updated.ManagerTrust);
        }

        [Fact]
        public void ManagerTrustSystem_TrustNeverFallsBelowZero()
        {
            var lowCareer = CreateCareerState(trust: 2f);
            var badResult = CreateResult(rating: 2.0f); // -5 delta

            var updated = ManagerTrustSystem.ApplyMatchResult(lowCareer, badResult, SquadStatus.Starter);

            Assert.Equal(0f, updated.ManagerTrust);
        }
    }
}
