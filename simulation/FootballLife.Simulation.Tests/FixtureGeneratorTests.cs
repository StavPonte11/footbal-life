using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class FixtureGeneratorTests
    {
        // ─── Helpers ──────────────────────────────────────────────────────────

        private static (WorldState world, League league, List<Guid> clubIds) CreateLeagueWorld(int clubCount)
        {
            var leagueId = Guid.NewGuid();
            var clubIds = new List<Guid>();
            var finances = new ClubFinances(100_000m, 5_000_000m);

            var season = new Season(
                2026,
                new DateOnly(2026, 8, 1),
                new DateOnly(2027, 5, 30),
                LeagueTable.Create(leagueId, new List<Guid>()),
                new List<Matchday>());

            var world = WorldState.CreateEmpty(season);
            var league = new League(leagueId, "Test League", "ENG", 1, clubCount, (clubCount - 1) * 2, 0, 3);
            world = world.WithLeague(league);

            for (int i = 0; i < clubCount; i++)
            {
                var clubId = Guid.NewGuid();
                clubIds.Add(clubId);
                var club = new Club(clubId, $"Club {i + 1}", $"C{i + 1}", leagueId, 50, finances, 3, TacticalIdentity.Possession);
                world = world.WithClub(club);
            }

            return (world, league, clubIds);
        }

        // ─── Matchday Count Tests ─────────────────────────────────────────────

        [Theory]
        [InlineData(20, 38)]  // Premier League
        [InlineData(18, 34)]  // Some leagues
        [InlineData(16, 30)]  // Smaller league
        [InlineData(4, 6)]    // Small 4-club test league
        public void FixtureGenerator_ProducesCorrectMatchdayCount(int clubCount, int expectedMatchdays)
        {
            var (world, league, _) = CreateLeagueWorld(clubCount);
            var rng = new SimulationRandom(42);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            Assert.Equal(expectedMatchdays, season.Matchdays.Count);
        }

        [Fact]
        public void FixtureGenerator_ProducesCorrectMatchdayCount_ForTwentyClubLeague()
        {
            var (world, league, _) = CreateLeagueWorld(20);
            var rng = new SimulationRandom(1);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            Assert.Equal(38, season.Matchdays.Count);
        }

        // ─── Every Club Plays Every Other Club Twice ──────────────────────────

        [Fact]
        public void FixtureGenerator_EachClubPlaysTwice_AgainstEveryOtherClub()
        {
            var (world, league, clubIds) = CreateLeagueWorld(6);
            var rng = new SimulationRandom(99);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            // Build head-to-head table: (homeId, awayId) → count
            var encounters = new Dictionary<(Guid, Guid), int>();
            foreach (var matchday in season.Matchdays)
            {
                foreach (var fixture in matchday.Fixtures)
                {
                    var key = (fixture.HomeClubId, fixture.AwayClubId);
                    encounters[key] = encounters.TryGetValue(key, out int c) ? c + 1 : 1;
                }
            }

            // Every ordered pair (A→B) and (B→A) must appear exactly once each
            foreach (var a in clubIds)
            {
                foreach (var b in clubIds)
                {
                    if (a == b) continue;

                    bool foundAB = encounters.TryGetValue((a, b), out int countAB);
                    bool foundBA = encounters.TryGetValue((b, a), out int countBA);

                    Assert.True(foundAB, $"Club {a} never played at home against {b}");
                    Assert.True(foundBA, $"Club {b} never played at home against {a}");
                    Assert.Equal(1, countAB);
                    Assert.Equal(1, countBA);
                }
            }
        }

        [Fact]
        public void FixtureGenerator_EachClubPlaysTwice_AgainstEveryOtherClub_TwentyClubs()
        {
            var (world, league, clubIds) = CreateLeagueWorld(20);
            var rng = new SimulationRandom(77);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            // Each club plays 38 matches total (2 × 19 opponents)
            var matchesPerClub = new Dictionary<Guid, int>();
            foreach (var matchday in season.Matchdays)
            {
                foreach (var fixture in matchday.Fixtures)
                {
                    matchesPerClub[fixture.HomeClubId] = (matchesPerClub.TryGetValue(fixture.HomeClubId, out var h) ? h : 0) + 1;
                    matchesPerClub[fixture.AwayClubId] = (matchesPerClub.TryGetValue(fixture.AwayClubId, out var a) ? a : 0) + 1;
                }
            }

            foreach (var clubId in clubIds)
            {
                Assert.True(matchesPerClub.TryGetValue(clubId, out int matches),
                    $"Club {clubId} played 0 matches.");
                Assert.Equal(38, matches);
            }
        }

        // ─── Home/Away Balance ─────────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_HomeAwayBalance_IsEqualPerClub()
        {
            var (world, league, clubIds) = CreateLeagueWorld(20);
            var rng = new SimulationRandom(13);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            var homeCount = new Dictionary<Guid, int>();
            var awayCount = new Dictionary<Guid, int>();

            foreach (var matchday in season.Matchdays)
            {
                foreach (var f in matchday.Fixtures)
                {
                    homeCount[f.HomeClubId] = (homeCount.TryGetValue(f.HomeClubId, out var h) ? h : 0) + 1;
                    awayCount[f.AwayClubId] = (awayCount.TryGetValue(f.AwayClubId, out var a) ? a : 0) + 1;
                }
            }

            // In a 20-club double round robin, each club plays 19 home + 19 away = 38
            foreach (var clubId in clubIds)
            {
                int home = homeCount.TryGetValue(clubId, out var hc) ? hc : 0;
                int away = awayCount.TryGetValue(clubId, out var ac) ? ac : 0;
                Assert.Equal(19, home);
                Assert.Equal(19, away);
            }
        }

        // ─── Fixture Dates ────────────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_MatchdayDates_AreWeeklySpaced()
        {
            var (world, league, _) = CreateLeagueWorld(4);
            var startDate = new DateOnly(2026, 8, 1);
            var rng = new SimulationRandom(5);

            var season = FixtureGenerator.Generate(league, world, 2026, startDate, rng);

            for (int i = 0; i < season.Matchdays.Count; i++)
            {
                var expected = startDate.AddDays(i * 7);
                Assert.Equal(expected, season.Matchdays[i].Date);
            }
        }

        [Fact]
        public void FixtureGenerator_AllFixturesOnMatchdayDate()
        {
            var (world, league, _) = CreateLeagueWorld(6);
            var rng = new SimulationRandom(42);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            foreach (var matchday in season.Matchdays)
            {
                foreach (var fixture in matchday.Fixtures)
                {
                    Assert.Equal(matchday.Date, fixture.Date);
                }
            }
        }

        // ─── Table Initialization ─────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_InitialTable_AllClubsAtZero()
        {
            var (world, league, clubIds) = CreateLeagueWorld(4);
            var rng = new SimulationRandom(1);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            Assert.Equal(clubIds.Count, season.Table.Rows.Count);
            foreach (var row in season.Table.Rows)
            {
                Assert.Equal(0, row.Points);
                Assert.Equal(0, row.GoalDifference);
                Assert.Equal(0, row.Played);
            }
        }

        // ─── League ID on Fixtures ────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_AllFixtures_HaveCorrectLeagueId()
        {
            var (world, league, _) = CreateLeagueWorld(4);
            var rng = new SimulationRandom(3);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            foreach (var matchday in season.Matchdays)
            {
                foreach (var fixture in matchday.Fixtures)
                {
                    Assert.Equal(league.Id, fixture.LeagueId);
                }
            }
        }

        // ─── Determinism ──────────────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_Determinism_SameSeed_ProducesSameSchedule()
        {
            var (world1, league1, _) = CreateLeagueWorld(6);
            var (world2, league2, _) = CreateLeagueWorld(6);

            // Can't compare club IDs (different worlds), but can compare match counts and structure
            var season1 = FixtureGenerator.Generate(league1, world1, 2026, new DateOnly(2026, 8, 1), new SimulationRandom(42));
            var season2 = FixtureGenerator.Generate(league2, world2, 2026, new DateOnly(2026, 8, 1), new SimulationRandom(42));

            Assert.Equal(season1.Matchdays.Count, season2.Matchdays.Count);
            for (int i = 0; i < season1.Matchdays.Count; i++)
            {
                Assert.Equal(season1.Matchdays[i].Fixtures.Count, season2.Matchdays[i].Fixtures.Count);
                Assert.Equal(season1.Matchdays[i].Date, season2.Matchdays[i].Date);
            }
        }

        // ─── Error Handling ────────────────────────────────────────────────────

        [Fact]
        public void FixtureGenerator_FewerThanTwoClubs_Throws()
        {
            // Create an empty league world with no clubs registered — bypasses League.clubCount validation
            var leagueId = Guid.NewGuid();
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30),
                LeagueTable.Create(leagueId, new List<Guid>()), new List<Matchday>());

            // Use a league with clubCount=2 (passes League validation), but register 0 clubs in world
            var emptyLeague = new League(leagueId, "Empty", "ENG", 1, 2, 2, 0, 0);
            var emptyWorld = WorldState.CreateEmpty(season).WithLeague(emptyLeague);

            Assert.Throws<ArgumentException>(() =>
                FixtureGenerator.Generate(emptyLeague, emptyWorld, 2026, new DateOnly(2026, 8, 1), new SimulationRandom(1)));
        }

        [Fact]
        public void FixtureGenerator_NullLeague_Throws()
        {
            var (world, _, _) = CreateLeagueWorld(4);
            Assert.Throws<ArgumentNullException>(() =>
                FixtureGenerator.Generate(null!, world, 2026, new DateOnly(2026, 8, 1), new SimulationRandom(1)));
        }

        [Fact]
        public void FixtureGenerator_NullWorld_Throws()
        {
            var (world, league, _) = CreateLeagueWorld(4);
            Assert.Throws<ArgumentNullException>(() =>
                FixtureGenerator.Generate(league, null!, 2026, new DateOnly(2026, 8, 1), new SimulationRandom(1)));
        }

        [Fact]
        public void FixtureGenerator_NoSameClubPlaysItself()
        {
            var (world, league, _) = CreateLeagueWorld(8);
            var rng = new SimulationRandom(888);

            var season = FixtureGenerator.Generate(league, world, 2026, new DateOnly(2026, 8, 1), rng);

            foreach (var matchday in season.Matchdays)
            {
                foreach (var fixture in matchday.Fixtures)
                {
                    Assert.NotEqual(fixture.HomeClubId, fixture.AwayClubId);
                }
            }
        }
    }
}
