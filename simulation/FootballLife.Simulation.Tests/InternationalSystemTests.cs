using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class InternationalSystemTests
    {
        private static readonly Guid EnglandId = Guid.NewGuid();
        private static readonly NationalTeam England = new(
            EnglandId, "England", "ENG", 5, "UEFA", "Gareth Southgate");

        private static Season CreateTestSeason()
        {
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }

        private static (Player Player, PlayerAbilities Abilities, PlayerState State, PlayerCareerState Career) CreateTestPlayer(
            string name, string nationality, Position position, byte ability = 75, float form = 60f)
        {
            var id = Guid.NewGuid();
            var player = new Player(id, name, nationality, new DateOnly(2000, 1, 1), Foot.Right, position);
            var abilities = PlayerAbilities.CreateUniform(ability);
            var state = PlayerState.Default with { Form = form, Fatigue = 10f, Confidence = 50f };
            var career = new PlayerCareerState(Guid.NewGuid(), SquadStatus.Starter, 60f, 5000m, 100000m, 50f);
            return (player, abilities, state, career);
        }

        [Fact]
        public void DetermineEligibility_MatchesCountryNameAndCode_CaseInsensitive()
        {
            var player1 = new Player(Guid.NewGuid(), "Harry Kane", "England", new DateOnly(1993, 7, 28), Foot.Right, Position.ST);
            var player2 = new Player(Guid.NewGuid(), "Bukayo Saka", "eng", new DateOnly(2001, 9, 5), Foot.Left, Position.RW);
            var player3 = new Player(Guid.NewGuid(), "Kylian Mbappe", "France", new DateOnly(1998, 12, 20), Foot.Right, Position.ST);

            Assert.True(InternationalSystem.DetermineEligibility(player1, England));
            Assert.True(InternationalSystem.DetermineEligibility(player2, England));
            Assert.False(InternationalSystem.DetermineEligibility(player3, England));
        }

        [Fact]
        public void GenerateCallUpSquad_SelectsEligiblePlayers_RespectsPositionQuotas()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            void AddPool(Position pos, int count, byte baseRating)
            {
                for (int i = 0; i < count; i++)
                {
                    var p = CreateTestPlayer($"Player_{pos}_{i}", "England", pos, (byte)(baseRating + i));
                    world = world.WithPlayer(p.Player, p.Abilities, p.State, p.Career);
                }
            }

            AddPool(Position.GK, 3, 70);
            AddPool(Position.CB, 8, 72);
            AddPool(Position.CM, 8, 74);
            AddPool(Position.ST, 6, 75);

            var rng = new SimulationRandom(42);
            var squad = InternationalSystem.GenerateCallUpSquad(England, world, rng, squadSize: 23);

            Assert.Equal(23, squad.Count);
            int gkCount = 0, defCount = 0, midCount = 0, fwdCount = 0;
            foreach (var id in squad)
            {
                var p = world.GetPlayer(id);
                if (p.PrimaryPosition == Position.GK) gkCount++;
                else if (p.PrimaryPosition == Position.CB) defCount++;
                else if (p.PrimaryPosition == Position.CM) midCount++;
                else if (p.PrimaryPosition == Position.ST) fwdCount++;
            }

            Assert.True(gkCount >= 2);
            Assert.True(defCount >= 6);
            Assert.True(midCount >= 6);
            Assert.True(fwdCount >= 3);
        }

        [Fact]
        public void GenerateCallUpSquad_SkipsRetiredPlayers()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var veteran = CreateTestPlayer("Veteran Star", "England", Position.ST, ability: 95);
            world = world.WithPlayer(veteran.Player, veteran.Abilities, veteran.State, veteran.Career);

            // Mark veteran as retired from international duty
            var intCareer = InternationalCareer.CreateNew(veteran.Player.Id, England.Id).WithInternationalRetirement();
            world = world.WithInternationalCareer(intCareer);

            var rng = new SimulationRandom(42);
            var squad = InternationalSystem.GenerateCallUpSquad(England, world, rng, squadSize: 11);

            Assert.DoesNotContain(veteran.Player.Id, squad);
        }

        [Fact]
        public void SimulateInternationalMatch_ProducesValidScoreAndGoalscorers()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var brazil = new NationalTeam(Guid.NewGuid(), "Brazil", "BRA", 1, "CONMEBOL", "Dorival Junior");

            var engPlayer = CreateTestPlayer("Kane", "England", Position.ST, 90);
            var braPlayer = CreateTestPlayer("Vinicius", "Brazil", Position.LW, 90);

            world = world.WithPlayer(engPlayer.Player, engPlayer.Abilities, engPlayer.State, engPlayer.Career)
                         .WithPlayer(braPlayer.Player, braPlayer.Abilities, braPlayer.State, braPlayer.Career);

            var engWithSquad = England.WithSquad(new[] { engPlayer.Player.Id });
            var braWithSquad = brazil.WithSquad(new[] { braPlayer.Player.Id });

            var rng = new SimulationRandom(12345);
            var (hScore, aScore, scorers) = InternationalSystem.SimulateInternationalMatch(
                engWithSquad, braWithSquad, world, rng);

            Assert.True(hScore >= 0);
            Assert.True(aScore >= 0);
            Assert.NotNull(scorers);
        }

        [Fact]
        public void UpdateInternationalCareer_AccumulatesCapsAndGoalsCorrectly()
        {
            var playerId = Guid.NewGuid();
            var career = InternationalCareer.CreateNew(playerId, England.Id);
            Assert.Equal(0, career.TotalCaps);
            Assert.Equal(0, career.TotalGoals);
            Assert.Null(career.DebutDate);

            var date1 = new DateOnly(2026, 6, 15);
            var updated = InternationalSystem.UpdateInternationalCareer(career, caps: 1, goals: 2, assists: 1, date1);

            Assert.Equal(1, updated.TotalCaps);
            Assert.Equal(2, updated.TotalGoals);
            Assert.Equal(1, updated.TotalAssists);
            Assert.Equal(date1, updated.DebutDate);

            var date2 = new DateOnly(2026, 6, 20);
            var updated2 = InternationalSystem.UpdateInternationalCareer(updated, caps: 1, goals: 1, assists: 0, date2);

            Assert.Equal(2, updated2.TotalCaps);
            Assert.Equal(3, updated2.TotalGoals);
            Assert.Equal(1, updated2.TotalAssists);
            Assert.Equal(date1, updated2.DebutDate);
        }

        [Fact]
        public void ProcessPlayerCallUp_AppliesFatigueAndConfidenceBoost()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var (player, abilities, state, career) = CreateTestPlayer("Bellingham", "England", Position.CM, ability: 88);
            world = world.WithPlayer(player, abilities, state, career)
                         .WithNationalTeam(England);

            var rng = new SimulationRandom(999);
            var (updatedWorld, callUp) = InternationalSystem.ProcessPlayerCallUp(
                player.Id, England.Id, "World Cup Qualifier", new DateOnly(2026, 9, 1), world, rng);

            Assert.Equal(CallUpStatus.CalledUp, callUp.Status);
            Assert.Equal(1, callUp.CapsEarned);

            var newState = updatedWorld.GetState(player.Id);
            Assert.Equal(state.Fatigue + InternationalSystem.CallUpFatigueCost, newState.Fatigue);
            Assert.Equal(state.Confidence + InternationalSystem.CallUpConfidenceBoost, newState.Confidence);

            var newCareerState = updatedWorld.GetCareerState(player.Id);
            Assert.Equal(career.Reputation + InternationalSystem.CallUpReputationBoost, newCareerState.Reputation);

            var intCareer = updatedWorld.GetPlayerInternationalCareer(player.Id);
            Assert.NotNull(intCareer);
            Assert.Equal(1, intCareer.TotalCaps);
        }

        [Fact]
        public void ProcessPlayerCallUp_RetiredPlayer_DeclinesCallUp()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var (player, abilities, state, career) = CreateTestPlayer("Retired Legend", "England", Position.ST, ability: 90);
            var retiredCareer = InternationalCareer.CreateNew(player.Id, England.Id).WithInternationalRetirement();

            world = world.WithPlayer(player, abilities, state, career)
                         .WithNationalTeam(England)
                         .WithInternationalCareer(retiredCareer);

            var rng = new SimulationRandom(111);
            var (updatedWorld, callUp) = InternationalSystem.ProcessPlayerCallUp(
                player.Id, England.Id, "Friendly", new DateOnly(2026, 9, 1), world, rng);

            Assert.Equal(CallUpStatus.Declined, callUp.Status);
            Assert.Equal(state.Fatigue, updatedWorld.GetState(player.Id).Fatigue);
        }
    }
}
