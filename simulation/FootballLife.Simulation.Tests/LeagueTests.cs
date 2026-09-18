using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LeagueTests
    {
        private readonly Guid _testLeagueId = Guid.NewGuid();

        [Fact]
        public void League_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var league = new League(
                _testLeagueId,
                "Premier League",
                "ENG",
                tier: 1,
                clubCount: 20,
                matchdaysPerSeason: 38,
                promotionSlots: 0,
                relegationSlots: 3);

            Assert.Equal(_testLeagueId, league.Id);
            Assert.Equal("Premier League", league.Name);
            Assert.Equal("ENG", league.CountryCode);
            Assert.Equal(1, league.Tier);
            Assert.Equal(20, league.ClubCount);
            Assert.Equal(38, league.MatchdaysPerSeason);
            Assert.Equal(0, league.PromotionSlots);
            Assert.Equal(3, league.RelegationSlots);
        }

        [Fact]
        public void League_CreateFactory_GeneratesUniqueIdAndDefaults()
        {
            var l1 = League.Create("La Liga", "esp", 1);
            var l2 = League.Create("Championship", "eng", 2, clubCount: 24, matchdaysPerSeason: 46, promotionSlots: 3, relegationSlots: 3);

            Assert.NotEqual(Guid.Empty, l1.Id);
            Assert.NotEqual(Guid.Empty, l2.Id);
            Assert.NotEqual(l1.Id, l2.Id);
            Assert.Equal("ESP", l1.CountryCode);
            Assert.Equal(20, l1.ClubCount);
            Assert.Equal(24, l2.ClubCount);
            Assert.Equal(3, l2.PromotionSlots);
        }

        [Fact]
        public void League_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("id", () =>
                new League(Guid.Empty, "League", "ENG", 1, 20, 38));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void League_WithNullOrWhitespaceName_ThrowsArgumentException(string? invalidName)
        {
            Assert.Throws<ArgumentException>("name", () =>
                new League(_testLeagueId, invalidName!, "ENG", 1, 20, 38));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void League_WithNullOrWhitespaceCountryCode_ThrowsArgumentException(string? invalidCountry)
        {
            Assert.Throws<ArgumentException>("countryCode", () =>
                new League(_testLeagueId, "League", invalidCountry!, 1, 20, 38));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        [InlineData(10)]
        public void League_TierOutOfBounds_ThrowsArgumentOutOfRangeException(int invalidTier)
        {
            Assert.Throws<ArgumentOutOfRangeException>("tier", () =>
                new League(_testLeagueId, "League", "ENG", invalidTier, 20, 38));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-5)]
        public void League_ClubCountLessThanTwo_ThrowsArgumentOutOfRangeException(int invalidCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>("clubCount", () =>
                new League(_testLeagueId, "League", "ENG", 1, invalidCount, 38));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void League_NonPositiveMatchdays_ThrowsArgumentOutOfRangeException(int invalidMatchdays)
        {
            Assert.Throws<ArgumentOutOfRangeException>("matchdaysPerSeason", () =>
                new League(_testLeagueId, "League", "ENG", 1, 20, invalidMatchdays));
        }

        [Fact]
        public void League_NegativePromotionSlots_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("promotionSlots", () =>
                new League(_testLeagueId, "League", "ENG", 1, 20, 38, promotionSlots: -1));
        }

        [Fact]
        public void League_NegativeRelegationSlots_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("relegationSlots", () =>
                new League(_testLeagueId, "League", "ENG", 1, 20, 38, relegationSlots: -1));
        }

        [Fact]
        public void League_PromotionAndRelegationExceedClubCount_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new League(_testLeagueId, "League", "ENG", 1, clubCount: 4, matchdaysPerSeason: 6, promotionSlots: 2, relegationSlots: 2));
        }

        // ─── Competition Tests ────────────────────────────────────────────────

        [Fact]
        public void Competition_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var compId = Guid.NewGuid();
            var comp = new Competition(compId, "FA Cup", CompetitionFormat.Cup, 64);

            Assert.Equal(compId, comp.Id);
            Assert.Equal("FA Cup", comp.Name);
            Assert.Equal(CompetitionFormat.Cup, comp.Format);
            Assert.Equal(64, comp.TotalTeams);
        }

        [Fact]
        public void Competition_CreateFactory_GeneratesUniqueId()
        {
            var c1 = Competition.Create("Champions Cup", CompetitionFormat.GroupStage, 32);
            var c2 = Competition.Create("Super Cup", CompetitionFormat.Cup, 2);

            Assert.NotEqual(Guid.Empty, c1.Id);
            Assert.NotEqual(Guid.Empty, c2.Id);
            Assert.NotEqual(c1.Id, c2.Id);
            Assert.Equal(32, c1.TotalTeams);
            Assert.Equal(2, c2.TotalTeams);
        }

        [Fact]
        public void Competition_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("id", () =>
                new Competition(Guid.Empty, "Cup", CompetitionFormat.Cup));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Competition_WithNullOrWhitespaceName_ThrowsArgumentException(string? invalidName)
        {
            Assert.Throws<ArgumentException>("name", () =>
                new Competition(Guid.NewGuid(), invalidName!, CompetitionFormat.Cup));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-5)]
        public void Competition_TotalTeamsLessThanTwo_ThrowsArgumentOutOfRangeException(int invalidTeams)
        {
            Assert.Throws<ArgumentOutOfRangeException>("totalTeams", () =>
                new Competition(Guid.NewGuid(), "Tournament", CompetitionFormat.Cup, invalidTeams));
        }

        [Fact]
        public void CompetitionFormat_AllThreeFormatsAreDefined()
        {
            var formats = Enum.GetValues(typeof(CompetitionFormat));
            Assert.Equal(3, formats.Length);
        }
    }
}
