using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void Player_CreatedWithValidData_HasStableId()
        {
            // Arrange
            var fixedId = Guid.NewGuid();
            var dob = new DateOnly(2005, 6, 24);

            // Act
            var player = new Player(
                fixedId,
                "Lionel Messi",
                "Argentina",
                dob,
                Foot.Left,
                Position.RW,
                Position.ST);

            // Assert
            Assert.Equal(fixedId, player.Id);
            Assert.Equal("Lionel Messi", player.Name);
            Assert.Equal("Argentina", player.Nationality);
            Assert.Equal(dob, player.DateOfBirth);
            Assert.Equal(Foot.Left, player.PreferredFoot);
            Assert.Equal(Position.RW, player.PrimaryPosition);
            Assert.Equal(Position.ST, player.SecondaryPosition);
        }

        [Fact]
        public void Player_CreateFactory_GeneratesNonEmptyUniqueId()
        {
            // Arrange & Act
            var player1 = Player.Create("Player One", "England", new DateOnly(2004, 1, 1), Foot.Right, Position.CM);
            var player2 = Player.Create("Player Two", "England", new DateOnly(2004, 1, 1), Foot.Right, Position.CM);

            // Assert
            Assert.NotEqual(Guid.Empty, player1.Id);
            Assert.NotEqual(Guid.Empty, player2.Id);
            Assert.NotEqual(player1.Id, player2.Id);
        }

        [Theory]
        [InlineData("2000-05-15", "2020-05-14", 19)]
        [InlineData("2000-05-15", "2020-05-15", 20)]
        [InlineData("2000-05-15", "2020-05-16", 20)]
        [InlineData("2004-02-29", "2024-02-28", 19)]
        [InlineData("2004-02-29", "2024-02-29", 20)]
        [InlineData("2004-02-29", "2025-02-28", 20)]
        public void Player_DateOfBirth_CanComputeAgeAtGivenDate(string dobString, string targetDateString, int expectedAge)
        {
            // Arrange
            var dob = DateOnly.Parse(dobString);
            var targetDate = DateOnly.Parse(targetDateString);
            var player = Player.Create("Test Player", "Spain", dob, Foot.Right, Position.ST);

            // Act
            int age = player.GetAgeAt(targetDate);

            // Assert
            Assert.Equal(expectedAge, age);
        }

        [Fact]
        public void Player_GetAgeAt_EarlierThanBirthDate_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dob = new DateOnly(2005, 1, 1);
            var player = Player.Create("Test Player", "Spain", dob, Foot.Right, Position.ST);
            var earlierDate = new DateOnly(2004, 12, 31);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => player.GetAgeAt(earlierDate));
        }

        [Fact]
        public void Player_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new Player(
                Guid.Empty,
                "Test",
                "France",
                new DateOnly(2000, 1, 1),
                Foot.Right,
                Position.CB));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Player_NullOrWhiteSpaceName_ThrowsArgumentException(string? invalidName)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new Player(
                Guid.NewGuid(),
                invalidName!,
                "Brazil",
                new DateOnly(2000, 1, 1),
                Foot.Right,
                Position.ST));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Player_NullOrWhiteSpaceNationality_ThrowsArgumentException(string? invalidNationality)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new Player(
                Guid.NewGuid(),
                "Valid Name",
                invalidNationality!,
                new DateOnly(2000, 1, 1),
                Foot.Right,
                Position.ST));
        }

        [Fact]
        public void Player_TrimmedNameAndNationality_RemovesExtraWhitespace()
        {
            // Arrange
            var player = Player.Create("  Erling Haaland  ", "  Norway  ", new DateOnly(2000, 7, 21), Foot.Left, Position.ST);

            // Assert
            Assert.Equal("Erling Haaland", player.Name);
            Assert.Equal("Norway", player.Nationality);
        }

        [Fact]
        public void Player_RecordEquality_WhenIdenticalData_AreEqual()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dob = new DateOnly(2002, 3, 10);

            var player1 = new Player(id, "Jude Bellingham", "England", dob, Foot.Right, Position.CM, Position.AM);
            var player2 = new Player(id, "Jude Bellingham", "England", dob, Foot.Right, Position.CM, Position.AM);

            // Assert
            Assert.Equal(player1, player2);
            Assert.True(player1 == player2);
        }

        [Fact]
        public void Player_WithExpression_ProducesNewRecordWithModifiedProperty()
        {
            // Arrange
            var player = Player.Create("Kylian Mbappe", "France", new DateOnly(1998, 12, 20), Foot.Right, Position.LW);

            // Act
            var updatedPlayer = player with { PrimaryPosition = Position.ST, SecondaryPosition = Position.LW };

            // Assert
            Assert.Equal(Position.ST, updatedPlayer.PrimaryPosition);
            Assert.Equal(Position.LW, updatedPlayer.SecondaryPosition);
            Assert.Equal(player.Id, updatedPlayer.Id);
            Assert.NotSame(player, updatedPlayer);
        }
    }
}
