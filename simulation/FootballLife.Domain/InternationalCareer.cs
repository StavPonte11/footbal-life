using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Tracks a player's cumulative international career statistics with a national team.
    /// </summary>
    public sealed record InternationalCareer
    {
        /// <summary>
        /// The player this international career belongs to.
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// The national team the player represents.
        /// </summary>
        public Guid NationalTeamId { get; init; }

        /// <summary>
        /// Total international caps (appearances) earned.
        /// </summary>
        public int TotalCaps { get; init; }

        /// <summary>
        /// Total international goals scored.
        /// </summary>
        public int TotalGoals { get; init; }

        /// <summary>
        /// Total international assists provided.
        /// </summary>
        public int TotalAssists { get; init; }

        /// <summary>
        /// Date of first international appearance, or null if uncapped.
        /// </summary>
        public DateOnly? DebutDate { get; init; }

        /// <summary>
        /// Whether the player has voluntarily retired from international duty.
        /// </summary>
        public bool IsRetiredFromInternational { get; init; }

        public InternationalCareer(
            Guid playerId,
            Guid nationalTeamId,
            int totalCaps = 0,
            int totalGoals = 0,
            int totalAssists = 0,
            DateOnly? debutDate = null,
            bool isRetiredFromInternational = false)
        {
            if (playerId == Guid.Empty)
                throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            if (nationalTeamId == Guid.Empty)
                throw new ArgumentException("National team ID cannot be empty.", nameof(nationalTeamId));
            if (totalCaps < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCaps), "Total caps cannot be negative.");
            if (totalGoals < 0)
                throw new ArgumentOutOfRangeException(nameof(totalGoals), "Total goals cannot be negative.");
            if (totalAssists < 0)
                throw new ArgumentOutOfRangeException(nameof(totalAssists), "Total assists cannot be negative.");

            PlayerId = playerId;
            NationalTeamId = nationalTeamId;
            TotalCaps = totalCaps;
            TotalGoals = totalGoals;
            TotalAssists = totalAssists;
            DebutDate = debutDate;
            IsRetiredFromInternational = isRetiredFromInternational;
        }

        /// <summary>
        /// Creates a fresh international career for a newly eligible player.
        /// </summary>
        public static InternationalCareer CreateNew(Guid playerId, Guid nationalTeamId)
        {
            return new InternationalCareer(playerId, nationalTeamId);
        }

        /// <summary>
        /// Accumulates caps, goals, and assists from a call-up window. Sets debut date if first cap.
        /// </summary>
        public InternationalCareer WithAccumulatedStats(int newCaps, int newGoals, int newAssists, DateOnly matchDate)
        {
            var debut = DebutDate ?? (newCaps > 0 ? matchDate : (DateOnly?)null);
            return this with
            {
                TotalCaps = TotalCaps + newCaps,
                TotalGoals = TotalGoals + newGoals,
                TotalAssists = TotalAssists + newAssists,
                DebutDate = debut
            };
        }

        /// <summary>
        /// Marks the player as retired from international football.
        /// </summary>
        public InternationalCareer WithInternationalRetirement()
        {
            return this with { IsRetiredFromInternational = true };
        }
    }
}
