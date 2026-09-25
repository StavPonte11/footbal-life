using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class WorldSimulationSystemTests
    {
        private static WorldState CreateMockWorld(int seed = 42)
        {
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>(), 1);
            var world = WorldState.CreateEmpty(season);

            var l1 = League.Create("Premier League", "ENG", 1, 4, 6, 0, 1);
            var l2 = League.Create("Championship", "ENG", 2, 4, 6, 1, 1);
            var l3 = League.Create("League One", "ENG", 3, 4, 6, 1, 1);
            var l4 = League.Create("League Two", "ENG", 4, 4, 6, 1, 0);

            world = world.WithLeague(l1).WithLeague(l2).WithLeague(l3).WithLeague(l4);

            var c1_1 = Club.Create("City Alpha", "CIA", l1.Id, 85, new ClubFinances(200000000m, 150000m), 5, TacticalIdentity.Possession);
            var c1_2 = Club.Create("United Beta", "UNB", l1.Id, 75, new ClubFinances(120000000m, 90000m), 4, TacticalIdentity.HighPress);

            var c2_1 = Club.Create("Champs Gamma", "CHG", l2.Id, 65, new ClubFinances(40000000m, 35000m), 3, TacticalIdentity.Direct);
            var c2_2 = Club.Create("Rovers Delta", "ROD", l2.Id, 55, new ClubFinances(25000000m, 20000m), 3, TacticalIdentity.Counter);

            var c3_1 = Club.Create("Athletic Epsilon", "ATE", l3.Id, 48, new ClubFinances(12000000m, 10000m), 2, TacticalIdentity.Possession);
            var c3_2 = Club.Create("Town Zeta", "TOZ", l3.Id, 42, new ClubFinances(8000000m, 6000m), 2, TacticalIdentity.Counter);

            var c4_1 = Club.Create("County Eta", "COE", l4.Id, 38, new ClubFinances(4000000m, 3000m), 1, TacticalIdentity.Direct);
            var c4_2 = Club.Create("Wanderers Theta", "WAT", l4.Id, 32, new ClubFinances(2500000m, 1800m), 1, TacticalIdentity.HighPress);

            return world.WithClub(c1_1).WithClub(c1_2)
                        .WithClub(c2_1).WithClub(c2_2)
                        .WithClub(c3_1).WithClub(c3_2)
                        .WithClub(c4_1).WithClub(c4_2);
        }

        [Fact]
        public void WorldSimulation_NpcPlayers_DevelopAndDeclineByAge()
        {
            var world = CreateMockWorld();
            var rng = new SimulationRandom(101);
            var seasonEnd = new DateOnly(2027, 6, 1);

            // 1. Young prospect (age 18, high potential)
            var pYoung = new Player(Guid.NewGuid(), "Young Wonderkid", "ENG", new DateOnly(2008, 9, 1), Foot.Right, Position.ST);
            var aYoung = new PlayerAbilities(pace: 65, acceleration: 65, stamina: 60, strength: 55, agility: 65, passing: 58, shooting: 62, dribbling: 64, crossing: 50, firstTouch: 62, tackling: 35, vision: 55, composure: 58, positioning: 60, decisionMaking: 55);
            var potYoung = PlayerPotential.CreateClamped(88, PotentialRange.High);

            // 2. Veteran (age 34)
            var pVeteran = new Player(Guid.NewGuid(), "Aging Veteran", "ENG", new DateOnly(1992, 9, 1), Foot.Right, Position.CM);
            var aVeteran = new PlayerAbilities(pace: 68, acceleration: 66, stamina: 70, strength: 72, agility: 62, passing: 80, shooting: 74, dribbling: 72, crossing: 75, firstTouch: 82, tackling: 65, vision: 84, composure: 85, positioning: 80, decisionMaking: 82);
            var potVeteran = PlayerPotential.CreateClamped(80, PotentialRange.High);

            world = world.WithPlayer(pYoung, aYoung, PlayerState.Default, new PlayerCareerState(world.Clubs.Keys.First(), SquadStatus.Rotation, 50, 1000, 500000, 30), potYoung)
                         .WithPlayer(pVeteran, aVeteran, PlayerState.Default, new PlayerCareerState(world.Clubs.Keys.First(), SquadStatus.Starter, 60, 5000, 200000, 60), potVeteran);

            // Run age and develop simulation
            var (updatedWorld, _) = WorldSimulationSystem.AgeAndDevelopNpcPlayers(world, protagonistPlayerId: null, seasonEnd, rng);

            var updatedYoungAbilities = updatedWorld.Abilities[pYoung.Id];
            var updatedVeteranAbilities = updatedWorld.Abilities[pVeteran.Id];

            // Young player should develop
            Assert.True(updatedYoungAbilities.CalculateAverage() > aYoung.CalculateAverage(), "Young prospect should have gained attributes.");
            // Veteran should experience physical decline (Pace drops)
            Assert.True(updatedVeteranAbilities.Pace < aVeteran.Pace, "Veteran physical pace should decline with age 34+.");
        }

        [Fact]
        public void WorldSimulation_NpcPlayers_RetireAtOldAge()
        {
            var world = CreateMockWorld();
            var rng = new SimulationRandom(202);
            var seasonEnd = new DateOnly(2027, 6, 1);
            var club = world.Clubs.Values.First();

            var pOld = new Player(Guid.NewGuid(), "Legend Veteran", "ENG", new DateOnly(1989, 5, 1), Foot.Right, Position.CB); // Age 38
            var aOld = new PlayerAbilities(pace: 45, acceleration: 42, stamina: 50, strength: 70, agility: 45, passing: 65, shooting: 40, dribbling: 50, crossing: 45, firstTouch: 65, tackling: 75, vision: 70, composure: 78, positioning: 76, decisionMaking: 74);
            var potOld = PlayerPotential.CreateClamped(75, PotentialRange.Medium);

            club = club.WithAddedPlayer(pOld.Id);
            world = world.WithClub(club)
                         .WithPlayer(pOld, aOld, PlayerState.Default, new PlayerCareerState(club.Id, SquadStatus.Bench, 40, 1000, 50000, 50), potOld);

            var (updatedWorld, retiredCount) = WorldSimulationSystem.AgeAndDevelopNpcPlayers(world, protagonistPlayerId: null, seasonEnd, rng);

            Assert.Equal(1, retiredCount);
            // Club squad should have removed retired player
            Assert.DoesNotContain(pOld.Id, updatedWorld.Clubs[club.Id].SquadPlayerIds);
        }

        [Fact]
        public void WorldSimulation_ReplenishSquads_MaintainsMinimumRoster()
        {
            var world = CreateMockWorld();
            var rng = new SimulationRandom(303);

            // Mock world clubs start with 0 players
            var updatedWorld = WorldSimulationSystem.ReplenishSquads(world, rng);

            foreach (var club in updatedWorld.Clubs.Values)
            {
                Assert.True(club.SquadPlayerIds.Count >= WorldSimulationSystem.MinSquadSize,
                    $"Club {club.Name} must have at least {WorldSimulationSystem.MinSquadSize} players.");
            }
        }

        [Fact]
        public void WorldSimulation_ResolveLeagueSeason_SwapsClubsAcrossTiers()
        {
            var world = CreateMockWorld();
            var rng = new SimulationRandom(404);

            var (updatedWorld, resolution) = WorldSimulationSystem.ResolveLeagueSeason(world, 2026, rng);

            Assert.NotNull(resolution);
            Assert.Equal(2026, resolution.SeasonYear);
            Assert.NotEmpty(resolution.LeagueResolutions);
            Assert.NotEmpty(resolution.PromotedClubIds);
            Assert.NotEmpty(resolution.RelegatedClubIds);

            // Verifies clubs were swapped between leagues
            var t1League = updatedWorld.Leagues.Values.First(l => l.Tier == 1);
            var t2League = updatedWorld.Leagues.Values.First(l => l.Tier == 2);

            var t1Clubs = updatedWorld.Clubs.Values.Where(c => c.LeagueId == t1League.Id).ToList();
            var t2Clubs = updatedWorld.Clubs.Values.Where(c => c.LeagueId == t2League.Id).ToList();

            Assert.Equal(2, t1Clubs.Count);
            Assert.Equal(2, t2Clubs.Count);
        }

        [Fact]
        public void WorldSimulation_LeagueTierConfig_ProvidesRealisticWageBands()
        {
            var p1 = LeagueTierConfig.GetProfile(1);
            var p2 = LeagueTierConfig.GetProfile(2);
            var p3 = LeagueTierConfig.GetProfile(3);
            var p4 = LeagueTierConfig.GetProfile(4);

            Assert.True(p1.AverageWeeklyWage > p2.AverageWeeklyWage);
            Assert.True(p2.AverageWeeklyWage > p3.AverageWeeklyWage);
            Assert.True(p3.AverageWeeklyWage > p4.AverageWeeklyWage);

            Assert.True(p1.PrestigeRating > p2.PrestigeRating);
            Assert.True(p4.PrestigeRating < p3.PrestigeRating);
        }
    }
}
