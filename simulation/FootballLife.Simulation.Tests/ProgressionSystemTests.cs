using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ProgressionSystemTests
    {
        private static readonly Guid YoungPlayerId = Guid.NewGuid();
        private static readonly Guid VeteranPlayerId = Guid.NewGuid();
        private static readonly DateOnly TestDate = new DateOnly(2026, 8, 1);

        private static (Player player, PlayerAbilities abilities, PlayerState state, PlayerCareerState career, PlayerPotential potential) CreatePlayerSetup(
            Guid id,
            int age,
            byte potentialRating = 85,
            int baseAbility = 50)
        {
            var dob = new DateOnly(TestDate.Year - age, 1, 1);
            var player = new Player(id, "Test Player", "ENG", dob, Foot.Right, Position.ST);
            var abilities = new PlayerAbilities(
                baseAbility, baseAbility, baseAbility, baseAbility, baseAbility,
                baseAbility, baseAbility, baseAbility, baseAbility, baseAbility, baseAbility,
                baseAbility, baseAbility, baseAbility, baseAbility);
            var state = new PlayerState(10, 60, 60, 70, 80, 75, 80);
            var career = PlayerCareerState.CreateAcademy(Guid.NewGuid());
            var potential = new PlayerPotential(potentialRating, PotentialRange.Medium);

            return (player, abilities, state, career, potential);
        }

        private static Season CreateTestSeason() =>
            new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());

        private static WorldState BuildWorld(
            (Player player, PlayerAbilities abilities, PlayerState state, PlayerCareerState career, PlayerPotential potential) p)
        {
            var world = WorldState.CreateEmpty(CreateTestSeason());
            return world.WithPlayer(p.player, p.abilities, p.state, p.career, p.potential);
        }

        [Fact]
        public void ProgressionSystem_YoungPlayer_GainsFasterThanVeteran()
        {
            var young = CreatePlayerSetup(YoungPlayerId, age: 18, baseAbility: 50);
            var veteran = CreatePlayerSetup(VeteranPlayerId, age: 30, baseAbility: 50);

            var world = WorldState.CreateEmpty(CreateTestSeason())
                .WithPlayer(young.player, young.abilities, young.state, young.career, young.potential)
                .WithPlayer(veteran.player, veteran.abilities, veteran.state, veteran.career, veteran.potential);

            var pendingXp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Shooting] = 500f,
                [AttributeName.Passing] = 500f
            };

            var rng = new SimulationRandom(42);
            var youngUpdated = ProgressionSystem.ApplyXpGains(YoungPlayerId, world, pendingXp, TestDate, rng);
            var veteranUpdated = ProgressionSystem.ApplyXpGains(VeteranPlayerId, world, pendingXp, TestDate, rng);

            int youngGain = youngUpdated.Shooting - young.abilities.Shooting;
            int veteranGain = veteranUpdated.Shooting - veteran.abilities.Shooting;

            Assert.True(youngGain > veteranGain, $"Young player gain ({youngGain}) must exceed veteran gain ({veteranGain})");
        }

        [Fact]
        public void ProgressionSystem_AbilityCappedAtPotentialRating()
        {
            const byte ceiling = 75;
            var playerSetup = CreatePlayerSetup(YoungPlayerId, age: 19, potentialRating: ceiling, baseAbility: 74);
            var world = BuildWorld(playerSetup);

            // Huge amount of pending XP that would normally award +14 ability points
            var pendingXp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Shooting] = 1000f
            };

            var rng = new SimulationRandom(42);
            var updated = ProgressionSystem.ApplyXpGains(YoungPlayerId, world, pendingXp, TestDate, rng);

            Assert.Equal(ceiling, updated.Shooting);
        }

        [Fact]
        public void ProgressionSystem_PhysicalDecline_BeginsAt31()
        {
            var veteran = CreatePlayerSetup(VeteranPlayerId, age: 32, baseAbility: 70);
            var world = BuildWorld(veteran);

            // Zero XP applied
            var emptyXp = new Dictionary<AttributeName, float>();
            var rng = new SimulationRandom(42);

            var updated = ProgressionSystem.ApplyXpGains(VeteranPlayerId, world, emptyXp, TestDate, rng);

            // Physical attributes should show decline (-0.5f rounded)
            Assert.True(updated.Pace < veteran.abilities.Pace ||
                        updated.Acceleration < veteran.abilities.Acceleration ||
                        updated.Stamina < veteran.abilities.Stamina);
            // Technical attributes without XP remain constant
            Assert.Equal(veteran.abilities.Shooting, updated.Shooting);
        }

        [Fact]
        public void ProgressionSystem_DeterminismTest_SameSeedSameGains()
        {
            var playerSetup = CreatePlayerSetup(YoungPlayerId, age: 19, potentialRating: 88, baseAbility: 55);
            var world = BuildWorld(playerSetup);

            var pendingXp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Dribbling] = 250f,
                [AttributeName.Shooting] = 300f
            };

            for (int seed = 1; seed <= 30; seed++)
            {
                var rng1 = new SimulationRandom(seed);
                var ab1 = ProgressionSystem.ApplyXpGains(YoungPlayerId, world, pendingXp, TestDate, rng1);

                var rng2 = new SimulationRandom(seed);
                var ab2 = ProgressionSystem.ApplyXpGains(YoungPlayerId, world, pendingXp, TestDate, rng2);

                Assert.Equal(ab1.Shooting, ab2.Shooting);
                Assert.Equal(ab1.Dribbling, ab2.Dribbling);
                Assert.Equal(ab1.Pace, ab2.Pace);
            }
        }
    }
}
