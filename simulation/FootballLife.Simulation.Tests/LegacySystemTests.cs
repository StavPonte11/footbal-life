using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LegacySystemTests
    {
        private readonly LegacySystem _system = new();

        [Fact]
        public void CalculateCareerScore_ComputesPointsCorrectly()
        {
            // 400 apps (= 100 pts), 100 goals (= 200 pts), 50 assists (= 75 pts)
            // 2 league titles (120 pts), 1 continental cup (120 pts)
            // 50 caps (50 pts), 20 int goals (60 pts)
            // Peak OVR 85 (+37.5 pts)
            // Total expected: ~762 pts
            int score = _system.CalculateCareerScore(
                appearances: 400,
                goals: 100,
                assists: 50,
                cleanSheets: 0,
                internationalCaps: 50,
                internationalGoals: 20,
                leagueTitles: 2,
                continentalTitles: 1,
                domesticCups: 0,
                internationalTrophies: 0,
                peakOvr: 85,
                lifetimeEarnings: 5_000_000);

            Assert.InRange(score, 700, 850);
        }

        [Fact]
        public void DetermineGrade_MapsScoreToCorrectTier()
        {
            Assert.Equal(LegacyGrade.GOAT, _system.DetermineGrade(950));
            Assert.Equal(LegacyGrade.Legend, _system.DetermineGrade(750));
            Assert.Equal(LegacyGrade.Icon, _system.DetermineGrade(550));
            Assert.Equal(LegacyGrade.CultHero, _system.DetermineGrade(400));
            Assert.Equal(LegacyGrade.Journeyman, _system.DetermineGrade(250));
            Assert.Equal(LegacyGrade.Underachiever, _system.DetermineGrade(150));
        }

        [Fact]
        public void IsEligibleForHallOfFame_AcceptsLegendsAndChampions()
        {
            var legendLegacy = new CareerLegacy(
                lifetimeAppearances: 500, lifetimeGoals: 180, lifetimeAssists: 80, lifetimeCleanSheets: 0,
                internationalCaps: 80, internationalGoals: 35, leagueTitles: 3, continentalTitles: 1,
                domesticCups: 2, internationalTrophies: 0, lifetimeEarnings: 20_000_000,
                peakOverallRating: 88, seasonsPlayed: 14, careerScore: 820,
                grade: LegacyGrade.Legend, isHallOfFameInductee: false);

            Assert.True(_system.IsEligibleForHallOfFame(legendLegacy));

            var journeymanLegacy = new CareerLegacy(
                lifetimeAppearances: 150, lifetimeGoals: 12, lifetimeAssists: 10, lifetimeCleanSheets: 0,
                internationalCaps: 0, internationalGoals: 0, leagueTitles: 0, continentalTitles: 0,
                domesticCups: 0, internationalTrophies: 0, lifetimeEarnings: 500_000,
                peakOverallRating: 68, seasonsPlayed: 5, careerScore: 180,
                grade: LegacyGrade.Underachiever, isHallOfFameInductee: false);

            Assert.False(_system.IsEligibleForHallOfFame(journeymanLegacy));
        }

        [Fact]
        public void InductIntoHallOfFame_CreatesValidEntryAndCommemorativePlaque()
        {
            var player = new Player(Guid.NewGuid(), "Wayne Rooney", "England", new DateOnly(1985, 10, 24), Foot.Right, Position.ST);

            var legacy = new CareerLegacy(
                lifetimeAppearances: 559, lifetimeGoals: 253, lifetimeAssists: 145, lifetimeCleanSheets: 0,
                internationalCaps: 120, internationalGoals: 53, leagueTitles: 5, continentalTitles: 1,
                domesticCups: 4, internationalTrophies: 0, lifetimeEarnings: 75_000_000,
                peakOverallRating: 92, seasonsPlayed: 18, careerScore: 920,
                grade: LegacyGrade.GOAT, isHallOfFameInductee: true);

            var entry = _system.InductIntoHallOfFame(player, legacy, PostPlayingRole.TVPundit, inductionYear: 2026);

            Assert.Equal("Wayne Rooney", entry.PlayerName);
            Assert.Equal(2026, entry.InductionYear);
            Assert.Equal(LegacyGrade.GOAT, entry.Grade);
            Assert.Equal(920, entry.CareerScore);
            Assert.Equal(10, entry.TotalTrophies);
            Assert.Equal(120, entry.InternationalCaps);
            Assert.Equal(PostPlayingRole.TVPundit, entry.PostCareerRole);
            Assert.Contains("HALL OF FAME INDUCTEE", entry.PlaqueText);
            Assert.Contains("Wayne Rooney", entry.PlaqueText);
        }
    }
}
