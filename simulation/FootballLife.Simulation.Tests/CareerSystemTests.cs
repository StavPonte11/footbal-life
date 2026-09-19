using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CareerSystemTests
    {
        private static readonly Guid PlayerId = Guid.NewGuid();
        private static readonly Guid ClubId = Guid.NewGuid();

        private static Club CreateTestClub(int reputation = 50, IReadOnlyList<Guid>? squad = null)
        {
            return new Club(
                id: ClubId,
                name: "Test FC",
                shortName: "TFC",
                leagueId: Guid.NewGuid(),
                reputationRating: reputation,
                finances: new ClubFinances(50000m, 1000000m),
                facilityRating: 3,
                tacticalStyle: TacticalIdentity.HighPress,
                squadPlayerIds: squad);
        }

        private static Season CreateTestSeason() =>
            new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());

        private static Player CreatePlayer() =>
            new Player(PlayerId, "Star Player", "ENG", new DateOnly(2005, 5, 1), Foot.Right, Position.ST);

        private static PlayerAbilities CreateAbilities(int avg = 70) =>
            new PlayerAbilities(avg, avg, avg, avg, avg, avg, avg, avg, avg, avg, avg, avg, avg, avg, avg);

        [Fact]
        public void CareerSystem_HighTrustHighReputation_EvaluatesAsStarter()
        {
            var player = CreatePlayer();
            var abilities = CreateAbilities(75);
            var career = new PlayerCareerState(ClubId, SquadStatus.Rotation, managerTrust: 85f, weeklySalary: 5000m, marketValue: 2000000m, reputation: 80f);
            var club = CreateTestClub(reputation: 50);

            var world = WorldState.CreateEmpty(CreateTestSeason());

            // Starting from Rotation (level 3), composite score > 66 promotes to Starter (level 4)
            var newStatus = CareerSystem.EvaluateSquadStatus(player, abilities, career, club, world);

            Assert.Equal(SquadStatus.Starter, newStatus);
        }

        [Fact]
        public void CareerSystem_LowTrustDespiteHighAbility_EvaluatesBelowAbilityExpectation()
        {
            var player = CreatePlayer();
            var abilities = CreateAbilities(85); // Elite player
            // But manager trust is rock bottom (5) and reputation is low (10)
            var career = new PlayerCareerState(ClubId, SquadStatus.Starter, managerTrust: 5f, weeklySalary: 5000m, marketValue: 2000000m, reputation: 10f);
            var club = CreateTestClub(reputation: 70);

            var world = WorldState.CreateEmpty(CreateTestSeason());

            var newStatus = CareerSystem.EvaluateSquadStatus(player, abilities, career, club, world);

            // Starter (level 4) should be demoted to Rotation (level 3) due to lack of manager trust
            Assert.Equal(SquadStatus.Rotation, newStatus);
        }

        [Fact]
        public void CareerSystem_StatusOnlyChangesOneLevel_PerEvaluation()
        {
            var player = CreatePlayer();
            var abilities = CreateAbilities(90); // World class
            // Currently in Academy (level 0), but with 100 trust and 100 reputation
            var career = new PlayerCareerState(ClubId, SquadStatus.Academy, managerTrust: 100f, weeklySalary: 500m, marketValue: 500000m, reputation: 100f);
            var club = CreateTestClub(reputation: 30);

            var world = WorldState.CreateEmpty(CreateTestSeason());

            // Even though composite score is > 81 (KeyPlayer), status can only climb 1 level: Academy -> Reserve
            var newStatus = CareerSystem.EvaluateSquadStatus(player, abilities, career, club, world);

            Assert.Equal(SquadStatus.Reserve, newStatus);
        }

        [Fact]
        public void CareerSystem_SeasonStats_CorrectGoalAndAssistCounts()
        {
            var results = new List<MatchResult>
            {
                new MatchResult(2, 1, Array.Empty<MatchEvent>(), playerRating: 8.0f, playerMinutesPlayed: 90, playerScored: true, playerAssisted: true),
                new MatchResult(1, 0, Array.Empty<MatchEvent>(), playerRating: 7.5f, playerMinutesPlayed: 90, playerScored: true, playerAssisted: false),
                new MatchResult(0, 0, Array.Empty<MatchEvent>(), playerRating: 6.0f, playerMinutesPlayed: 90, playerScored: false, playerAssisted: false)
            };

            var career = new PlayerCareerState(ClubId, SquadStatus.Starter, 70f, 2000m, 500000m, 50f);
            var stats = CareerSystem.AggregateSeasonStats(results, career, weeklyWage: 2000m, weeksInSeason: 38, attributeGrowthAverage: 3.2f);

            Assert.Equal(3, stats.Appearances);
            Assert.Equal(2, stats.Goals);
            Assert.Equal(1, stats.Assists);
            Assert.Equal(7.17f, stats.AverageRating);
            Assert.Equal(76000m, stats.WageEarned); // 2000 * 38
            Assert.Equal(SquadStatus.Starter, stats.FinalStatus);
            Assert.Equal(3.2f, stats.AttributeGrowthAverage);
        }

        [Fact]
        public void CareerSystem_AverageRating_ExcludesDidNotPlayMatches()
        {
            var results = new List<MatchResult>
            {
                new MatchResult(1, 0, Array.Empty<MatchEvent>(), playerRating: 8.0f, playerMinutesPlayed: 90, playerScored: false, playerAssisted: false),
                // Benched match (0 minutes played, rating default 5.0 should NOT count towards average)
                new MatchResult(0, 1, Array.Empty<MatchEvent>(), playerRating: 5.0f, playerMinutesPlayed: 0, playerScored: false, playerAssisted: false)
            };

            var career = new PlayerCareerState(ClubId, SquadStatus.Bench, 40f, 1000m, 100000m, 20f);
            var stats = CareerSystem.AggregateSeasonStats(results, career, weeklyWage: 1000m, weeksInSeason: 38);

            Assert.Equal(1, stats.Appearances);
            Assert.Equal(8.0f, stats.AverageRating);
        }
    }
}
