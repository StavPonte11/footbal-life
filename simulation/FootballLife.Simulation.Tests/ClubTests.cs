using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ClubTests
    {
        private readonly Guid _testClubId = Guid.NewGuid();
        private readonly Guid _testLeagueId = Guid.NewGuid();
        private readonly ClubFinances _testFinances = new ClubFinances(250_000m, 50_000_000m);

        [Fact]
        public void Club_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var managerId = Guid.NewGuid();
            var stadium = new ClubStadium("Emirates Stadium", 60_704, 5);
            var visuals = new ClubVisuals("badges/arsenal", new KitColors("#EF0107", "#FFFFFF"), new KitColors("#000000", "#FFD700"));
            var fanbase = new ClubFanbase(3_000_000, 85, 90);
            var board = new ClubBoard("Stan Kroenke", BoardPatience.Balanced, 4);
            var history = new ClubHistory(1886, 13, 14, 2);
            var squad = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var club = new Club(
                _testClubId,
                "North London FC",
                "NLF",
                _testLeagueId,
                88,
                _testFinances,
                5,
                TacticalIdentity.Possession,
                managerId,
                stadium,
                visuals,
                fanbase,
                board,
                history,
                squad);

            Assert.Equal(_testClubId, club.Id);
            Assert.Equal("North London FC", club.Name);
            Assert.Equal("NLF", club.ShortName);
            Assert.Equal(_testLeagueId, club.LeagueId);
            Assert.Equal(88, club.ReputationRating);
            Assert.Equal(_testFinances, club.Finances);
            Assert.Equal(5, club.FacilityRating);
            Assert.Equal(TacticalIdentity.Possession, club.TacticalStyle);
            Assert.Equal(managerId, club.ManagerId);
            Assert.Equal(stadium, club.Stadium);
            Assert.Equal(visuals, club.Visuals);
            Assert.Equal(fanbase, club.Fanbase);
            Assert.Equal(board, club.Board);
            Assert.Equal(history, club.History);
            Assert.Equal(2, club.SquadPlayerIds.Count);
        }

        [Fact]
        public void Club_DefaultFallbackSubModels_ArePopulatedAutomatically()
        {
            var club = Club.Create(
                "Default FC",
                "DFC",
                _testLeagueId,
                65,
                _testFinances,
                3,
                TacticalIdentity.Counter);

            Assert.NotNull(club.Stadium);
            Assert.Contains("Default FC Stadium", club.Stadium.Name);
            Assert.NotNull(club.Visuals);
            Assert.Equal("badges/dfc", club.Visuals.LogoAssetKey);
            Assert.NotNull(club.Fanbase);
            Assert.True(club.Fanbase.SupporterCount > 0);
            Assert.NotNull(club.Board);
            Assert.NotNull(club.History);
            Assert.Empty(club.SquadPlayerIds);
            Assert.Null(club.ManagerId);
        }

        [Fact]
        public void Club_CreateFactory_GeneratesNonEmptyUniqueId()
        {
            var club1 = Club.Create("Club A", "CA", _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Counter);
            var club2 = Club.Create("Club B", "CB", _testLeagueId, 72, _testFinances, 3, TacticalIdentity.HighPress);

            Assert.NotEqual(Guid.Empty, club1.Id);
            Assert.NotEqual(Guid.Empty, club2.Id);
            Assert.NotEqual(club1.Id, club2.Id);
            Assert.Equal("Club A", club1.Name);
            Assert.Equal(TacticalIdentity.HighPress, club2.TacticalStyle);
        }

        [Fact]
        public void Club_WithAddedPlayer_AddsIdImmutably()
        {
            var club = Club.Create("Club A", "CA", _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Counter);
            var playerId = Guid.NewGuid();

            var updated = club.WithAddedPlayer(playerId);

            Assert.Empty(club.SquadPlayerIds);
            Assert.Single(updated.SquadPlayerIds);
            Assert.Contains(playerId, updated.SquadPlayerIds);

            // Adding same player again does not duplicate
            var readded = updated.WithAddedPlayer(playerId);
            Assert.Single(readded.SquadPlayerIds);
        }

        [Fact]
        public void Club_WithRemovedPlayer_RemovesIdImmutably()
        {
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();
            var club = Club.Create("Club A", "CA", _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Counter,
                squadPlayerIds: new List<Guid> { p1, p2 });

            var updated = club.WithRemovedPlayer(p1);

            Assert.Equal(2, club.SquadPlayerIds.Count);
            Assert.Single(updated.SquadPlayerIds);
            Assert.Equal(p2, updated.SquadPlayerIds[0]);
        }

        [Fact]
        public void Club_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("id", () =>
                new Club(Guid.Empty, "Club", "C", _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Direct));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Club_WithNullOrWhitespaceName_ThrowsArgumentException(string? invalidName)
        {
            Assert.Throws<ArgumentException>("name", () =>
                new Club(_testClubId, invalidName!, "C", _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Direct));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Club_WithNullOrWhitespaceShortName_ThrowsArgumentException(string? invalidShortName)
        {
            Assert.Throws<ArgumentException>("shortName", () =>
                new Club(_testClubId, "Club Name", invalidShortName!, _testLeagueId, 70, _testFinances, 3, TacticalIdentity.Direct));
        }

        [Fact]
        public void Club_WithEmptyLeagueId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("leagueId", () =>
                new Club(_testClubId, "Club", "C", Guid.Empty, 70, _testFinances, 3, TacticalIdentity.Direct));
        }

        [Fact]
        public void Club_WithNullFinances_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("finances", () =>
                new Club(_testClubId, "Club", "C", _testLeagueId, 70, null!, 3, TacticalIdentity.Direct));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Club_ReputationOutOfBounds_ThrowsArgumentOutOfRangeException(int invalidReputation)
        {
            Assert.Throws<ArgumentOutOfRangeException>("reputationRating", () =>
                new Club(_testClubId, "Club", "C", _testLeagueId, invalidReputation, _testFinances, 3, TacticalIdentity.Direct));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        [InlineData(10)]
        public void Club_FacilityOutOfBounds_ThrowsArgumentOutOfRangeException(int invalidFacility)
        {
            Assert.Throws<ArgumentOutOfRangeException>("facilityRating", () =>
                new Club(_testClubId, "Club", "C", _testLeagueId, 70, _testFinances, invalidFacility, TacticalIdentity.Direct));
        }

        // ─── ClubFinances Tests ───────────────────────────────────────────────

        [Fact]
        public void ClubFinances_ValidBudgets_StoresCorrectly()
        {
            var finances = new ClubFinances(100_000m, 20_000_000m);
            Assert.Equal(100_000m, finances.WeeklyWageBudget);
            Assert.Equal(20_000_000m, finances.TransferBudget);
        }

        [Fact]
        public void ClubFinances_NegativeWageBudget_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("weeklyWageBudget", () =>
                new ClubFinances(-1m, 10_000_000m));
        }

        [Fact]
        public void ClubFinances_NegativeTransferBudget_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("transferBudget", () =>
                new ClubFinances(50_000m, -500m));
        }

        [Fact]
        public void ClubFinances_CanAffordWage_EvaluatesCorrectly()
        {
            var finances = new ClubFinances(50_000m, 1_000_000m);
            Assert.True(finances.CanAffordWage(50_000m));
            Assert.True(finances.CanAffordWage(20_000m));
            Assert.True(finances.CanAffordWage(0m));
            Assert.False(finances.CanAffordWage(50_001m));
            Assert.False(finances.CanAffordWage(-100m));
        }

        [Fact]
        public void ClubFinances_CanAffordTransfer_EvaluatesCorrectly()
        {
            var finances = new ClubFinances(50_000m, 1_000_000m);
            Assert.True(finances.CanAffordTransfer(1_000_000m));
            Assert.True(finances.CanAffordTransfer(500_000m));
            Assert.True(finances.CanAffordTransfer(0m));
            Assert.False(finances.CanAffordTransfer(1_000_001m));
            Assert.False(finances.CanAffordTransfer(-1m));
        }

        // ─── Stadium Tests ───────────────────────────────────────────────────

        [Fact]
        public void ClubStadium_ValidData_StoresCorrectly()
        {
            var stadium = new ClubStadium("Old Trafford", 74_310, 5);
            Assert.Equal("Old Trafford", stadium.Name);
            Assert.Equal(74_310, stadium.Capacity);
            Assert.Equal(5, stadium.PitchQuality);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500)]
        public void ClubStadium_NonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () =>
                new ClubStadium("Arena", capacity, 3));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void ClubStadium_InvalidPitchQuality_ThrowsArgumentOutOfRangeException(int pitch)
        {
            Assert.Throws<ArgumentOutOfRangeException>("pitchQuality", () =>
                new ClubStadium("Arena", 20_000, pitch));
        }

        // ─── Visuals & Kits Tests ────────────────────────────────────────────

        [Fact]
        public void KitColors_NormalizesHexValues()
        {
            var kit = new KitColors("ff0000", "#00ff00", "0000ff");
            Assert.Equal("#FF0000", kit.PrimaryHex);
            Assert.Equal("#00FF00", kit.SecondaryHex);
            Assert.Equal("#0000FF", kit.TrimHex);
        }

        [Fact]
        public void ClubVisuals_ValidData_StoresCorrectly()
        {
            var visuals = new ClubVisuals(
                "badges/real_madrid",
                new KitColors("#FFFFFF", "#000000"),
                new KitColors("#0000FF", "#FFFFFF"));

            Assert.Equal("badges/real_madrid", visuals.LogoAssetKey);
            Assert.Equal("#FFFFFF", visuals.HomeKit.PrimaryHex);
            Assert.Equal("#0000FF", visuals.AwayKit.PrimaryHex);
        }

        // ─── Fanbase Tests ───────────────────────────────────────────────────

        [Fact]
        public void ClubFanbase_ValidData_StoresCorrectly()
        {
            var fanbase = new ClubFanbase(500_000, 80, 75);
            Assert.Equal(500_000, fanbase.SupporterCount);
            Assert.Equal(80, fanbase.LoyaltyRating);
            Assert.Equal(75, fanbase.ExpectationRating);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void ClubFanbase_InvalidLoyalty_ThrowsArgumentOutOfRangeException(int loyalty)
        {
            Assert.Throws<ArgumentOutOfRangeException>("loyaltyRating", () =>
                new ClubFanbase(10_000, loyalty, 50));
        }

        // ─── Board & Ownership Tests ─────────────────────────────────────────

        [Fact]
        public void ClubBoard_ValidData_StoresCorrectly()
        {
            var board = new ClubBoard("Florentino Perez", BoardPatience.Impatient, 5);
            Assert.Equal("Florentino Perez", board.OwnerName);
            Assert.Equal(BoardPatience.Impatient, board.Patience);
            Assert.Equal(5, board.FinancialAmbition);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void ClubBoard_InvalidAmbition_ThrowsArgumentOutOfRangeException(int ambition)
        {
            Assert.Throws<ArgumentOutOfRangeException>("financialAmbition", () =>
                new ClubBoard("Owner", BoardPatience.Balanced, ambition));
        }

        // ─── History Tests ───────────────────────────────────────────────────

        [Fact]
        public void ClubHistory_CalculatesTotalTrophiesCorrectly()
        {
            var history = new ClubHistory(1902, 35, 20, 15);
            Assert.Equal(1902, history.FoundedYear);
            Assert.Equal(70, history.TotalMajorTrophies);
        }

        [Theory]
        [InlineData(1800)]
        [InlineData(2150)]
        public void ClubHistory_UnrealisticYear_ThrowsArgumentOutOfRangeException(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>("foundedYear", () =>
                new ClubHistory(year));
        }
    }
}
