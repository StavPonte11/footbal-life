using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class PlayerFactoryTests
    {
        // ─── Helpers ──────────────────────────────────────────────────────────

        private static WorldState CreateWorldWithClub(out Guid clubId)
        {
            var leagueId = Guid.NewGuid();
            clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());

            var league = new League(leagueId, "Test League", "ENG", 1, 20, 38, 0, 3);
            var finances = new ClubFinances(100_000m, 10_000_000m);
            var club = new Club(clubId, "Test Club", "TST", leagueId, 50, finances, 3, TacticalIdentity.Possession);

            return WorldState.CreateEmpty(season).WithLeague(league).WithClub(club);
        }

        private static PlayerCreationArgs DefaultArgs(Guid clubId) => new PlayerCreationArgs(
            "John Player",
            "ENG",
            new DateOnly(2004, 6, 15),
            Position.ST,
            Foot.Right,
            clubId,
            50);

        // ─── Core Creation Tests ──────────────────────────────────────────────

        [Fact]
        public void PlayerFactory_CreatesPlayer_WithValidBoundedAbilities()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);
            var rng = new SimulationRandom(42);

            var result = PlayerFactory.CreateCareer(world, args, rng);

            Assert.Single(result.Players);
            var playerId = System.Linq.Enumerable.First(result.Players).Key;
            var abilities = result.GetAbilities(playerId);

            // All abilities must be in [1, 100]
            Assert.InRange(abilities.Pace, 1, 100);
            Assert.InRange(abilities.Acceleration, 1, 100);
            Assert.InRange(abilities.Stamina, 1, 100);
            Assert.InRange(abilities.Strength, 1, 100);
            Assert.InRange(abilities.Agility, 1, 100);
            Assert.InRange(abilities.Passing, 1, 100);
            Assert.InRange(abilities.Shooting, 1, 100);
            Assert.InRange(abilities.Dribbling, 1, 100);
            Assert.InRange(abilities.Crossing, 1, 100);
            Assert.InRange(abilities.FirstTouch, 1, 100);
            Assert.InRange(abilities.Tackling, 1, 100);
            Assert.InRange(abilities.Vision, 1, 100);
            Assert.InRange(abilities.Composure, 1, 100);
            Assert.InRange(abilities.Positioning, 1, 100);
            Assert.InRange(abilities.DecisionMaking, 1, 100);
        }

        [Fact]
        public void PlayerFactory_CreatedWorldState_HasPlayerInAllFacets()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);
            var rng = new SimulationRandom(7);

            var result = PlayerFactory.CreateCareer(world, args, rng);

            Assert.Single(result.Players);
            var playerId = System.Linq.Enumerable.First(result.Players).Key;

            // Player must exist in all 5 core facet dictionaries
            Assert.True(result.Players.ContainsKey(playerId));
            Assert.True(result.Abilities.ContainsKey(playerId));
            Assert.True(result.States.ContainsKey(playerId));
            Assert.True(result.CareerStates.ContainsKey(playerId));
            Assert.True(result.Potentials.ContainsKey(playerId));

            // Contract must exist for the player
            var contract = result.FindContractForPlayer(playerId);
            Assert.NotNull(contract);
            Assert.Equal(clubId, contract!.ClubId);

            // Club squad must include the player
            var club = result.GetClub(clubId);
            Assert.Contains(playerId, club.SquadPlayerIds);
        }

        [Fact]
        public void PlayerFactory_CareerState_IsAcademy_WithValidSalary()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);
            var rng = new SimulationRandom(1);

            var result = PlayerFactory.CreateCareer(world, args, rng);
            var playerId = System.Linq.Enumerable.First(result.Players).Key;
            var careerState = result.GetCareerState(playerId);

            Assert.Equal(SquadStatus.Academy, careerState.Status);
            Assert.Equal(clubId, careerState.ClubId);
            Assert.InRange(careerState.WeeklySalary, 150m, 500m);
            Assert.Equal(30f, careerState.ManagerTrust);
        }

        [Fact]
        public void PlayerFactory_Contract_IsOneYear_FromSeasonStart()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);
            var rng = new SimulationRandom(1);

            var result = PlayerFactory.CreateCareer(world, args, rng);
            var playerId = System.Linq.Enumerable.First(result.Players).Key;
            var contract = result.FindContractForPlayer(playerId)!;

            Assert.Equal(world.CurrentSeason.StartDate, contract.StartDate);
            Assert.Equal(world.CurrentSeason.StartDate.AddYears(1), contract.EndDate);
            Assert.Equal(SquadRole.Academy, contract.ContractRole);
        }

        [Fact]
        public void PlayerFactory_Potential_IsWithinExpectedBounds()
        {
            var world = CreateWorldWithClub(out var clubId);
            // Base 50, bonus range [10, 35] → potential in [50, 99] after clamping
            var args = DefaultArgs(clubId);
            var rng = new SimulationRandom(99);

            var result = PlayerFactory.CreateCareer(world, args, rng);
            var playerId = System.Linq.Enumerable.First(result.Players).Key;
            var potential = result.GetPotential(playerId);

            Assert.InRange(potential.PotentialRating, PlayerPotential.MinRating, PlayerPotential.MaxRating);
            // For base 50, ceiling should be at least 60 (base + min bonus 10)
            Assert.True(potential.PotentialRating >= 50 + 10,
                $"Potential {potential.PotentialRating} below expected floor for base 50");
        }

        // ─── Statistical Balance Tests ─────────────────────────────────────────

        [Fact]
        public void PlayerFactory_HigherBase_ProducesHigherInitialAbilities_OnAverage()
        {
            var worldLow = CreateWorldWithClub(out var clubIdLow);
            var worldHigh = CreateWorldWithClub(out var clubIdHigh);

            const int runs = 100;
            double sumLow = 0, sumHigh = 0;

            for (int i = 0; i < runs; i++)
            {
                var rng = new SimulationRandom(i);
                var argsLow = new PlayerCreationArgs("Low", "ENG", new DateOnly(2004, 1, 1), Position.ST, Foot.Right, clubIdLow, 40);
                var resultLow = PlayerFactory.CreateCareer(worldLow, argsLow, rng);
                var pidLow = System.Linq.Enumerable.First(resultLow.Players).Key;
                sumLow += resultLow.GetAbilities(pidLow).CalculateAverage();

                rng = new SimulationRandom(i);
                var argsHigh = new PlayerCreationArgs("High", "ENG", new DateOnly(2004, 1, 1), Position.ST, Foot.Right, clubIdHigh, 60);
                var resultHigh = PlayerFactory.CreateCareer(worldHigh, argsHigh, rng);
                var pidHigh = System.Linq.Enumerable.First(resultHigh.Players).Key;
                sumHigh += resultHigh.GetAbilities(pidHigh).CalculateAverage();
            }

            double avgLow = sumLow / runs;
            double avgHigh = sumHigh / runs;

            Assert.True(avgHigh > avgLow,
                $"High base (60) average {avgHigh:F1} was not higher than low base (40) average {avgLow:F1}");

            // The gap should be statistically meaningful (not just noise)
            Assert.True(avgHigh - avgLow > 5.0,
                $"Gap between bases was too small: {avgHigh - avgLow:F1}");
        }

        // ─── Determinism Tests ────────────────────────────────────────────────

        [Fact]
        public void PlayerFactory_SameSeedSameArgs_ProducesIdenticalPlayer()
        {
            var world1 = CreateWorldWithClub(out var clubId1);
            var world2 = CreateWorldWithClub(out var clubId2);
            // Note: club IDs differ but we can compare the player's own properties

            var args1 = new PlayerCreationArgs("Determinism Test", "ENG", new DateOnly(2003, 5, 10), Position.CM, Foot.Left, clubId1, 50);
            var args2 = new PlayerCreationArgs("Determinism Test", "ENG", new DateOnly(2003, 5, 10), Position.CM, Foot.Left, clubId2, 50);

            var result1 = PlayerFactory.CreateCareer(world1, args1, new SimulationRandom(42));
            var result2 = PlayerFactory.CreateCareer(world2, args2, new SimulationRandom(42));

            var pid1 = System.Linq.Enumerable.First(result1.Players).Key;
            var pid2 = System.Linq.Enumerable.First(result2.Players).Key;

            var abilities1 = result1.GetAbilities(pid1);
            var abilities2 = result2.GetAbilities(pid2);

            Assert.Equal(abilities1.Pace, abilities2.Pace);
            Assert.Equal(abilities1.Shooting, abilities2.Shooting);
            Assert.Equal(abilities1.Dribbling, abilities2.Dribbling);
            Assert.Equal(abilities1.Passing, abilities2.Passing);
            Assert.Equal(abilities1.Stamina, abilities2.Stamina);

            var potential1 = result1.GetPotential(pid1);
            var potential2 = result2.GetPotential(pid2);
            Assert.Equal(potential1.PotentialRating, potential2.PotentialRating);
            Assert.Equal(potential1.Range, potential2.Range);
        }

        // ─── Error Handling Tests ─────────────────────────────────────────────

        [Fact]
        public void PlayerFactory_NullWorld_Throws()
        {
            var args = DefaultArgs(Guid.NewGuid());
            Assert.Throws<ArgumentNullException>(() => PlayerFactory.CreateCareer(null!, args, new SimulationRandom(1)));
        }

        [Fact]
        public void PlayerFactory_NullArgs_Throws()
        {
            var world = CreateWorldWithClub(out _);
            Assert.Throws<ArgumentNullException>(() => PlayerFactory.CreateCareer(world, null!, new SimulationRandom(1)));
        }

        [Fact]
        public void PlayerFactory_NullRng_Throws()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);
            Assert.Throws<ArgumentNullException>(() => PlayerFactory.CreateCareer(world, args, null!));
        }

        [Fact]
        public void PlayerFactory_NonExistentClub_Throws()
        {
            var world = CreateWorldWithClub(out _);
            var args = DefaultArgs(Guid.NewGuid()); // club does not exist
            Assert.Throws<KeyNotFoundException>(() => PlayerFactory.CreateCareer(world, args, new SimulationRandom(1)));
        }

        [Fact]
        public void PlayerCreationArgs_InvalidAbilityBase_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCreationArgs("X", "ENG", new DateOnly(2005, 1, 1), Position.ST, Foot.Right, Guid.NewGuid(), 61));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PlayerCreationArgs("X", "ENG", new DateOnly(2005, 1, 1), Position.ST, Foot.Right, Guid.NewGuid(), 39));
        }

        [Fact]
        public void PlayerFactory_DoesNotMutateOriginalWorld()
        {
            var world = CreateWorldWithClub(out var clubId);
            var args = DefaultArgs(clubId);

            var result = PlayerFactory.CreateCareer(world, args, new SimulationRandom(1));

            // Original world must be unchanged
            Assert.Empty(world.Players);
            Assert.Empty(world.Potentials);
            Assert.Empty(world.Contracts);
            Assert.Empty(world.GetClub(clubId).SquadPlayerIds);

            // New world has the player
            Assert.Single(result.Players);
        }
    }
}
