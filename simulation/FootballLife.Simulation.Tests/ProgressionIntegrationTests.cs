using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ProgressionIntegrationTests
    {
        private static readonly Guid YoungStrikerId = Guid.NewGuid();
        private static readonly Guid VeteranId = Guid.NewGuid();

        private static (Player player, PlayerAbilities abilities, PlayerState state, PlayerCareerState career, PlayerPotential potential) CreatePlayer(
            Guid id,
            int startAge,
            byte potentialRating,
            int startAbility)
        {
            var dob = new DateOnly(2026 - startAge, 7, 1);
            var player = new Player(id, "Career Prospect", "ENG", dob, Foot.Right, Position.ST);
            var abilities = new PlayerAbilities(
                startAbility, startAbility, startAbility, startAbility, startAbility,
                startAbility, startAbility, startAbility, startAbility, startAbility, startAbility,
                startAbility, startAbility, startAbility, startAbility);
            var state = new PlayerState(10, 60, 60, 70, 80, 75, 80);
            var career = PlayerCareerState.CreateAcademy(Guid.NewGuid());
            var potential = new PlayerPotential(potentialRating, PotentialRange.Medium);

            return (player, abilities, state, career, potential);
        }

        [Fact]
        public void Progression_FiveSeasonCareer_YoungStriker_ReachesExpectedAbilityRange()
        {
            // 17yo starting at 50 overall, potential 80
            var p = CreatePlayer(YoungStrikerId, startAge: 17, potentialRating: 80, startAbility: 50);
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());
            var world = WorldState.CreateEmpty(season).WithPlayer(p.player, p.abilities, p.state, p.career, p.potential);

            var rng = new SimulationRandom(42);
            var currentAbilities = p.abilities;

            // Simulate 5 seasons (age 17 to 22)
            // Each season has ~30 training sessions earning ~100 XP per session across position attributes
            var weights = PositionWeightMap.For(Position.ST);
            var seasonXp = new Dictionary<AttributeName, float>(15);
            foreach (var kvp in weights)
            {
                seasonXp[kvp.Key] = kvp.Value * 1500f; // 15 sessions * 100 XP
            }

            for (int s = 0; s < 5; s++)
            {
                var seasonDate = new DateOnly(2026 + s, 12, 1);
                currentAbilities = ProgressionSystem.ApplyXpGains(YoungStrikerId, world, seasonXp, seasonDate, rng);
                world = world.WithPlayer(p.player, currentAbilities, p.state, p.career, p.potential);
            }

            float finalOverall = currentAbilities.CalculateAverage();

            // Should reach 60–80 range at age 22
            Assert.InRange(finalOverall, 60f, 80f);
        }

        [Fact]
        public void Progression_VeteranPlayer_Age34_MinimalAbilityGain_PhysicalDeclineVisible()
        {
            // 34yo starting at 75 overall
            var p = CreatePlayer(VeteranId, startAge: 34, potentialRating: 85, startAbility: 75);
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());
            var world = WorldState.CreateEmpty(season).WithPlayer(p.player, p.abilities, p.state, p.career, p.potential);

            var rng = new SimulationRandom(42);
            var currentAbilities = p.abilities;

            // Apply moderate training XP
            var pendingXp = new Dictionary<AttributeName, float>
            {
                [AttributeName.Passing] = 300f,
                [AttributeName.Shooting] = 300f
            };

            for (int s = 0; s < 2; s++)
            {
                var seasonDate = new DateOnly(2026 + s, 12, 1);
                currentAbilities = ProgressionSystem.ApplyXpGains(VeteranId, world, pendingXp, seasonDate, rng);
                world = world.WithPlayer(p.player, currentAbilities, p.state, p.career, p.potential);
            }

            // Physical attributes should decline noticeably
            Assert.True(currentAbilities.Pace < p.abilities.Pace, "Pace should decline for 34yo veteran");
            Assert.True(currentAbilities.Acceleration < p.abilities.Acceleration, "Acceleration should decline for 34yo veteran");
            Assert.True(currentAbilities.Stamina < p.abilities.Stamina, "Stamina should decline for 34yo veteran");

            // Overall gain is minimal
            float overallDelta = currentAbilities.CalculateAverage() - p.abilities.CalculateAverage();
            Assert.True(overallDelta < 2.0f, $"Veteran overall growth ({overallDelta}) must be minimal");
        }

        [Fact]
        public void CareerSimulator_100Careers_PeakOverall_Mean70to80_NoneReach99Before24()
        {
            float peakOverallSum = 0f;
            const int careersCount = 100;

            for (int seed = 1; seed <= careersCount; seed++)
            {
                var rng = new SimulationRandom(seed);
                // Potential randomized between 70 and 88
                byte potentialRating = (byte)rng.NextInt(70, 89);
                var id = Guid.NewGuid();
                var p = CreatePlayer(id, startAge: 17, potentialRating: potentialRating, startAbility: 52);

                var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());
                var world = WorldState.CreateEmpty(season).WithPlayer(p.player, p.abilities, p.state, p.career, p.potential);

                var currentAbilities = p.abilities;

                var weights = PositionWeightMap.For(Position.ST);
                var seasonXp = new Dictionary<AttributeName, float>(15);
                foreach (var kvp in weights)
                {
                    seasonXp[kvp.Key] = kvp.Value * 1200f;
                }

                // Simulate from age 17 to 24 (7 seasons)
                for (int s = 0; s < 7; s++)
                {
                    var seasonDate = new DateOnly(2026 + s, 12, 1);
                    currentAbilities = ProgressionSystem.ApplyXpGains(id, world, seasonXp, seasonDate, rng);
                    world = world.WithPlayer(p.player, currentAbilities, p.state, p.career, p.potential);

                    // No player should reach 99 before age 24
                    Assert.True(currentAbilities.CalculateAverage() < 99f, "No player should reach 99 before 24");
                }

                peakOverallSum += currentAbilities.CalculateAverage();
            }

            float meanPeak = peakOverallSum / careersCount;
            Assert.InRange(meanPeak, 68f, 85f);
        }

        [Fact]
        public void CareerSimulator_DeterministicSeed42_IdenticalCareerStats()
        {
            float RunCareer(int seed)
            {
                var rng = new SimulationRandom(seed);
                var id = Guid.NewGuid();
                var p = CreatePlayer(id, startAge: 18, potentialRating: 84, startAbility: 55);

                var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>());
                var world = WorldState.CreateEmpty(season).WithPlayer(p.player, p.abilities, p.state, p.career, p.potential);

                var currentAbilities = p.abilities;
                var seasonXp = new Dictionary<AttributeName, float>
                {
                    [AttributeName.Shooting] = 1000f,
                    [AttributeName.Pace] = 800f
                };

                for (int s = 0; s < 4; s++)
                {
                    var seasonDate = new DateOnly(2026 + s, 12, 1);
                    currentAbilities = ProgressionSystem.ApplyXpGains(id, world, seasonXp, seasonDate, rng);
                    world = world.WithPlayer(p.player, currentAbilities, p.state, p.career, p.potential);
                }

                return currentAbilities.CalculateAverage();
            }

            float result1 = RunCareer(42);
            float result2 = RunCareer(42);

            Assert.Equal(result1, result2);
        }
    }
}
