using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class WorldStateTests
    {
        private static Season CreateTestSeason()
        {
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }

        [Fact]
        public void WorldState_CreateEmpty_InitializesEmptyCollections()
        {
            var season = CreateTestSeason();
            var world = WorldState.CreateEmpty(season);

            Assert.Empty(world.Clubs);
            Assert.Empty(world.Players);
            Assert.Empty(world.Abilities);
            Assert.Empty(world.States);
            Assert.Empty(world.CareerStates);
            Assert.Empty(world.Leagues);
            Assert.Empty(world.Managers);
            Assert.Empty(world.Contracts);
            Assert.Empty(world.Potentials);
            Assert.Equal(season, world.CurrentSeason);
        }

        [Fact]
        public void WorldState_WithPlayer_RegistersAllPlayerFacets()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());

            var player = Player.Create("Leo Messi", "ARG", new DateOnly(1987, 6, 24), Foot.Left, Position.RW);
            var abilities = PlayerAbilities.CreateUniform(90);
            var state = PlayerState.Default;
            var career = PlayerCareerState.CreateAcademy(Guid.NewGuid());

            var updated = world.WithPlayer(player, abilities, state, career);

            Assert.Empty(world.Players); // Immutability
            Assert.Single(updated.Players);
            Assert.Equal(player, updated.GetPlayer(player.Id));
            Assert.Equal(abilities, updated.GetAbilities(player.Id));
            Assert.Equal(state, updated.GetState(player.Id));
            Assert.Equal(career, updated.GetCareerState(player.Id));
        }

        [Fact]
        public void WorldState_WithPlayerState_UpdatesStateImmutably()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var player = Player.Create("Striker", "ENG", new DateOnly(2000, 1, 1), Foot.Right, Position.ST);
            var populated = world.WithPlayer(player, PlayerAbilities.Default, PlayerState.Default, PlayerCareerState.CreateAcademy(Guid.NewGuid()));

            var exhaustedState = PlayerState.Default with { Fatigue = 85f };
            var updated = populated.WithPlayerState(player.Id, exhaustedState);

            Assert.Equal(0f, populated.GetState(player.Id).Fatigue);
            Assert.Equal(85f, updated.GetState(player.Id).Fatigue);
        }

        [Fact]
        public void WorldState_WithClub_RegistersClubImmutably()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var club = Club.Create("Arsenal FC", "ARS", Guid.NewGuid(), 85, new ClubFinances(200_000m, 50_000_000m), 4, TacticalIdentity.Possession);

            var updated = world.WithClub(club);

            Assert.Empty(world.Clubs);
            Assert.Single(updated.Clubs);
            Assert.Equal(club, updated.GetClub(club.Id));
        }

        [Fact]
        public void WorldState_WithLeague_RegistersLeagueImmutably()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var league = League.Create("Premier League", "ENG", 1);

            var updated = world.WithLeague(league);

            Assert.Empty(world.Leagues);
            Assert.Single(updated.Leagues);
            Assert.Equal(league, updated.GetLeague(league.Id));
        }

        [Fact]
        public void WorldState_WithManager_RegistersManagerImmutably()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var manager = Manager.Create("Head Coach", Formation.F433, TacticalIdentity.HighPress);

            var updated = world.WithManager(manager);

            Assert.Empty(world.Managers);
            Assert.Single(updated.Managers);
            Assert.Equal(manager, updated.GetManager(manager.Id));
        }

        [Fact]
        public void WorldState_WithContract_RegistersContractAndEnablesLookup()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var contract = Contract.Create(playerId, clubId, 50_000m, new DateOnly(2026, 7, 1), new DateOnly(2029, 6, 30), SquadRole.Starter);

            var updated = world.WithContract(contract);

            Assert.Equal(contract, updated.GetContractForPlayer(playerId));
            Assert.Equal(contract, updated.FindContractForPlayer(playerId));
        }

        [Fact]
        public void WorldState_Lookups_NotFound_ThrowDescriptiveKeyNotFoundExceptions()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var randomId = Guid.NewGuid();

            Assert.Throws<KeyNotFoundException>(() => world.GetPlayer(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetAbilities(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetState(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetCareerState(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetClub(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetLeague(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetManager(randomId));
            Assert.Throws<KeyNotFoundException>(() => world.GetContractForPlayer(randomId));
            Assert.Null(world.FindContractForPlayer(randomId));
        }

        [Fact]
        public void WorldState_WithPotential_RoundTrips()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var playerId = Guid.NewGuid();
            var potential = new PlayerPotential(75, PotentialRange.High);

            var updated = world.WithPlayerPotential(playerId, potential);

            // Original unchanged (immutability)
            Assert.Empty(world.Potentials);

            // New snapshot contains the potential
            Assert.Single(updated.Potentials);
            Assert.Equal(potential, updated.GetPotential(playerId));
            Assert.Equal(75, updated.GetPotential(playerId).PotentialRating);
            Assert.Equal(PotentialRange.High, updated.GetPotential(playerId).Range);
        }

        [Fact]
        public void WorldState_GetPotential_NotFound_ThrowsKeyNotFoundException()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var randomId = Guid.NewGuid();
            Assert.Throws<KeyNotFoundException>(() => world.GetPotential(randomId));
        }

        [Fact]
        public void WorldState_WithPlayer_WithPotential_RegistersAllFacets()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var player = Player.Create("Test Player", "ENG", new DateOnly(2002, 3, 15), Foot.Right, Position.ST);
            var abilities = PlayerAbilities.CreateUniform(55);
            var state = PlayerState.Default;
            var career = PlayerCareerState.CreateAcademy(Guid.NewGuid());
            var potential = new PlayerPotential(80, PotentialRange.Elite);

            var updated = world.WithPlayer(player, abilities, state, career, potential);

            Assert.Single(updated.Players);
            Assert.Single(updated.Potentials);
            Assert.Equal(potential, updated.GetPotential(player.Id));
        }

        [Fact]
        public void WorldState_WithPlayer_WithoutPotential_LeavesPotetialEmpty()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var player = Player.Create("Test Player", "ENG", new DateOnly(2002, 3, 15), Foot.Right, Position.ST);

            var updated = world.WithPlayer(player, PlayerAbilities.Default, PlayerState.Default, PlayerCareerState.CreateAcademy(Guid.NewGuid()));

            Assert.Single(updated.Players);
            Assert.Empty(updated.Potentials);
        }

        [Fact]
        public void WorldState_Lookups_NotFound_ThrowDescriptiveKeyNotFoundExceptions_IncludesPotential()
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            var randomId = Guid.NewGuid();
            Assert.Throws<KeyNotFoundException>(() => world.GetPotential(randomId));
        }
    }
}
