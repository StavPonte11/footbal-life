using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Immutable record representing a discrete event that occurred during a match.
    /// </summary>
    public sealed record MatchEvent
    {
        public int Minute { get; init; }
        public MatchEventType Type { get; init; }
        public Guid PlayerId { get; init; }
        public string Description { get; init; }

        public MatchEvent(int minute, MatchEventType type, Guid playerId, string description)
        {
            if (minute < 0 || minute > 120)
                throw new ArgumentOutOfRangeException(nameof(minute), $"Minute must be within [0, 120]. Actual: {minute}");

            Minute = minute;
            Type = type;
            PlayerId = playerId;
            Description = description ?? string.Empty;
        }
    }
}
