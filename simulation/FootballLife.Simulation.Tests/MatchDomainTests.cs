using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MatchDomainTests
    {
        private static readonly Guid HomeClub = Guid.NewGuid();
        private static readonly Guid AwayClub = Guid.NewGuid();
        private static readonly Guid PlayerId = Guid.NewGuid();
        private static readonly Guid AssisterId = Guid.NewGuid();

        [Fact]
        public void MatchState_ScoreUpdates_Correctly_AfterGoal()
        {
            var match = MatchState.Create(HomeClub, AwayClub);
            Assert.Equal(0, match.HomeScore);
            Assert.Equal(0, match.AwayScore);
            Assert.Empty(match.EventLog);

            // Home goal with assist
            var updatedHome = match.WithGoal(minute: 14, scorerId: PlayerId, assisterId: AssisterId, isHome: true);
            Assert.Equal(1, updatedHome.HomeScore);
            Assert.Equal(0, updatedHome.AwayScore);
            Assert.Equal(14, updatedHome.Minute);
            Assert.Equal(2, updatedHome.EventLog.Count);
            Assert.Equal(MatchEventType.Assist, updatedHome.EventLog[0].Type);
            Assert.Equal(MatchEventType.Goal, updatedHome.EventLog[1].Type);

            // Away goal without assist
            var updatedAway = updatedHome.WithGoal(minute: 45, scorerId: Guid.NewGuid(), assisterId: null, isHome: false);
            Assert.Equal(1, updatedAway.HomeScore);
            Assert.Equal(1, updatedAway.AwayScore);
            Assert.Equal(45, updatedAway.Minute);
            Assert.Equal(3, updatedAway.EventLog.Count);
        }

        [Fact]
        public void MatchResult_IsWin_ReturnsCorrectly_ForHomeAndAway()
        {
            var winResult = new MatchResult(
                homeScore: 3,
                awayScore: 1,
                events: Array.Empty<MatchEvent>(),
                playerRating: 8.5f,
                playerMinutesPlayed: 90,
                playerScored: true,
                playerAssisted: false);

            Assert.True(winResult.IsWin(playerWasHome: true));
            Assert.False(winResult.IsLoss(playerWasHome: true));
            Assert.False(winResult.IsDraw());

            Assert.False(winResult.IsWin(playerWasHome: false));
            Assert.True(winResult.IsLoss(playerWasHome: false));
            Assert.False(winResult.IsDraw());

            var drawResult = new MatchResult(
                homeScore: 2,
                awayScore: 2,
                events: Array.Empty<MatchEvent>(),
                playerRating: 6.5f,
                playerMinutesPlayed: 90,
                playerScored: false,
                playerAssisted: false);

            Assert.True(drawResult.IsDraw());
            Assert.False(drawResult.IsWin(playerWasHome: true));
            Assert.False(drawResult.IsLoss(playerWasHome: true));
        }

        [Fact]
        public void MatchState_InvariantsEnforced()
        {
            Assert.Throws<ArgumentException>(() => new MatchState(Guid.Empty, AwayClub, 0, 0, 0, false));
            Assert.Throws<ArgumentException>(() => new MatchState(HomeClub, HomeClub, 0, 0, 0, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchState(HomeClub, AwayClub, -1, 0, 0, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchState(HomeClub, AwayClub, 0, 0, 125, false));
        }

        [Fact]
        public void MatchEvent_ValidatesMinute()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchEvent(-1, MatchEventType.Goal, PlayerId, "test"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchEvent(121, MatchEventType.Goal, PlayerId, "test"));
            var validEvent = new MatchEvent(90, MatchEventType.YellowCard, PlayerId, "Foul");
            Assert.Equal(90, validEvent.Minute);
            Assert.Equal(MatchEventType.YellowCard, validEvent.Type);
        }

        [Fact]
        public void MatchResult_ValidatesRatingBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchResult(1, 0, null, 0.5f, 90, false, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchResult(1, 0, null, 10.5f, 90, false, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MatchResult(1, 0, null, 7.0f, -1, false, false));
        }
    }
}
