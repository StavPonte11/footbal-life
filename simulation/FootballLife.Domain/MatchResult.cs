using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Post-match summary record encapsulating final score, player individual performance rating,
    /// events, and result outcomes.
    /// </summary>
    public sealed record MatchResult
    {
        public const float MinRating = 1.0f;
        public const float MaxRating = 10.0f;

        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public IReadOnlyList<MatchEvent> Events { get; init; }
        public float PlayerRating { get; init; }
        public int PlayerMinutesPlayed { get; init; }
        public bool PlayerScored { get; init; }
        public bool PlayerAssisted { get; init; }

        public MatchResult(
            int homeScore,
            int awayScore,
            IReadOnlyList<MatchEvent>? events,
            float playerRating,
            int playerMinutesPlayed,
            bool playerScored,
            bool playerAssisted)
        {
            if (homeScore < 0)
                throw new ArgumentOutOfRangeException(nameof(homeScore), "HomeScore cannot be negative.");
            if (awayScore < 0)
                throw new ArgumentOutOfRangeException(nameof(awayScore), "AwayScore cannot be negative.");
            if (float.IsNaN(playerRating) || playerRating < MinRating || playerRating > MaxRating)
                throw new ArgumentOutOfRangeException(nameof(playerRating), $"PlayerRating must be in [{MinRating}, {MaxRating}]. Actual: {playerRating}");
            if (playerMinutesPlayed < 0 || playerMinutesPlayed > 120)
                throw new ArgumentOutOfRangeException(nameof(playerMinutesPlayed), $"PlayerMinutesPlayed must be in [0, 120]. Actual: {playerMinutesPlayed}");

            HomeScore = homeScore;
            AwayScore = awayScore;
            Events = events != null ? new List<MatchEvent>(events).AsReadOnly() : Array.Empty<MatchEvent>();
            PlayerRating = playerRating;
            PlayerMinutesPlayed = playerMinutesPlayed;
            PlayerScored = playerScored;
            PlayerAssisted = playerAssisted;
        }

        public bool IsWin(bool playerWasHome) => playerWasHome ? HomeScore > AwayScore : AwayScore > HomeScore;
        public bool IsDraw() => HomeScore == AwayScore;
        public bool IsLoss(bool playerWasHome) => playerWasHome ? HomeScore < AwayScore : AwayScore < HomeScore;
    }
}
