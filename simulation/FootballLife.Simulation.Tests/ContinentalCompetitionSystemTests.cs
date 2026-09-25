using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ContinentalCompetitionSystemTests
    {
        private static Season CreateTestSeason()
        {
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }

        private static Club CreateTestClub(string name, int rep = 75, decimal transferBudget = 10_000_000m)
        {
            var finances = new ClubFinances(weeklyWageBudget: 500_000m, transferBudget: transferBudget);
            return Club.Create(
                name: name,
                shortName: name.Substring(0, Math.Min(3, name.Length)).ToUpperInvariant(),
                leagueId: Guid.NewGuid(),
                reputationRating: rep,
                finances: finances,
                facilityRating: 4,
                tacticalStyle: TacticalIdentity.Possession);
        }

        [Fact]
        public void QualifyClubs_TopTierFinishers_QualifySuccessfully()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var clubs = new List<Club>();
            var standings = new List<ClubSeasonOutcome>();
            var leagueId = Guid.NewGuid();

            for (int i = 1; i <= 20; i++)
            {
                var club = CreateTestClub($"Club_{i}", rep: 90 - i);
                clubs.Add(club);
                standings.Add(new ClubSeasonOutcome
                {
                    ClubId = club.Id,
                    ClubName = club.Name,
                    LeagueId = leagueId,
                    FinalPosition = i,
                    Points = 100 - (i * 3),
                    GoalDifference = 50 - (i * 2),
                    WonTitle = i == 1,
                    Promoted = false,
                    Relegated = i >= 18,
                    QualifiedForContinental = i <= 4
                });
            }

            world = world.WithClubs(clubs);
            var resolution = new LeagueSeasonResolution
            {
                SeasonYear = 2026,
                LeagueId = leagueId,
                LeagueName = "Premier Division",
                Tier = 1,
                ChampionClubId = clubs[0].Id,
                Standings = standings
            };

            var qualified = ContinentalCompetitionSystem.QualifyClubs(
                world, new[] { resolution }, slotsPerTopLeague: 4, totalSlots: 16);

            // First 4 clubs should be the top 4 finishers
            Assert.Contains(clubs[0].Id, qualified);
            Assert.Contains(clubs[1].Id, qualified);
            Assert.Contains(clubs[2].Id, qualified);
            Assert.Contains(clubs[3].Id, qualified);
            Assert.Equal(16, qualified.Count); // Total filled to 16 by reputation
        }

        [Fact]
        public void DrawGroups_Distributes32ClubsInto8GroupsOf4()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var clubs = new List<Club>();
            for (int i = 0; i < 32; i++)
            {
                clubs.Add(CreateTestClub($"EuroClub_{i}"));
            }
            world = world.WithClubs(clubs);

            var rng = new SimulationRandom(42);
            var groups = ContinentalCompetitionSystem.DrawGroups(
                clubs.Select(c => c.Id).ToList(), world, rng, groupCount: 8);

            Assert.Equal(8, groups.Count);
            foreach (var g in groups)
            {
                Assert.Equal(4, g.Entries.Count);
            }
        }

        [Fact]
        public void SimulateGroupStage_PlaysRoundRobinAndGeneratesStandings()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var clubs = new List<Club>();
            for (int i = 0; i < 32; i++)
            {
                clubs.Add(CreateTestClub($"EuroClub_{i}"));
            }
            world = world.WithClubs(clubs);

            var rng = new SimulationRandom(77);
            var groups = ContinentalCompetitionSystem.DrawGroups(
                clubs.Select(c => c.Id).ToList(), world, rng, groupCount: 8);

            var tournament = ContinentalCompetition.Create("Champions Cup", 32) with { GroupStandings = groups };

            var simulatedTournament = ContinentalCompetitionSystem.SimulateGroupStage(tournament, world, rng);

            foreach (var g in simulatedTournament.GroupStandings)
            {
                foreach (var entry in g.Entries)
                {
                    Assert.Equal(6, entry.Played);
                    Assert.Equal(entry.Won + entry.Drawn + entry.Lost, entry.Played);
                    Assert.True(entry.Points >= 0);
                }

                Assert.NotEqual(Guid.Empty, g.GetGroupWinner());
                Assert.NotEqual(Guid.Empty, g.GetGroupRunnerUp());
                Assert.NotEqual(g.GetGroupWinner(), g.GetGroupRunnerUp());
            }
        }

        [Fact]
        public void GenerateKnockoutBracket_PairsWinnersAndRunnersUp()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var clubs = new List<Club>();
            for (int i = 0; i < 32; i++)
            {
                clubs.Add(CreateTestClub($"EuroClub_{i}"));
            }
            world = world.WithClubs(clubs);

            var rng = new SimulationRandom(101);
            var groups = ContinentalCompetitionSystem.DrawGroups(clubs.Select(c => c.Id).ToList(), world, rng);
            var tournament = ContinentalCompetition.Create("Champions Cup", 32) with { GroupStandings = groups };
            tournament = ContinentalCompetitionSystem.SimulateGroupStage(tournament, world, rng);

            var bracketTournament = ContinentalCompetitionSystem.GenerateKnockoutBracket(tournament, world, rng);

            Assert.Equal(16, bracketTournament.KnockoutFixtures.Count);
            Assert.All(bracketTournament.KnockoutFixtures, f => Assert.Equal(ContinentalStage.RoundOf16, f.Stage));
        }

        [Fact]
        public void SimulateFullTournament_ResolvesWinnerAndAwardsPrizeMoney()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var clubs = new List<Club>();
            for (int i = 0; i < 32; i++)
            {
                clubs.Add(CreateTestClub($"EuroClub_{i}", rep: 70 + (i % 25), transferBudget: 5_000_000m));
            }
            world = world.WithClubs(clubs);

            var rng = new SimulationRandom(2026);
            var (updatedWorld, completedTournament) = ContinentalCompetitionSystem.SimulateFullTournament(
                clubs.Select(c => c.Id).ToList(), world, rng, "Champions Cup");

            Assert.True(completedTournament.IsCompleted);
            Assert.NotNull(completedTournament.WinnerId);
            Assert.Contains(completedTournament.WinnerId.Value, clubs.Select(c => c.Id));

            var winnerClub = updatedWorld.GetClub(completedTournament.WinnerId.Value);
            Assert.True(winnerClub.Finances.TransferBudget >= 25_000_000m);
        }
    }
}
