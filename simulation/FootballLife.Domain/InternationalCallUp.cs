using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Status of a player's international call-up for a specific window.
    /// </summary>
    public enum CallUpStatus
    {
        /// <summary>Player is available for selection but not yet called up.</summary>
        Available = 0,

        /// <summary>Player has been selected for the national team squad.</summary>
        CalledUp = 1,

        /// <summary>Player is unavailable due to injury.</summary>
        Injured = 2,

        /// <summary>Player declined the call-up or retired from international duty.</summary>
        Declined = 3
    }

    /// <summary>
    /// Represents a single international call-up event for a player in a specific window/tournament.
    /// </summary>
    public sealed record InternationalCallUp
    {
        /// <summary>
        /// Unique identifier for this call-up event.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// The player being called up.
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// The national team issuing the call-up.
        /// </summary>
        public Guid NationalTeamId { get; init; }

        /// <summary>
        /// Name of the tournament or window (e.g., "World Cup Qualifier", "Friendly").
        /// </summary>
        public string TournamentName { get; init; }

        /// <summary>
        /// Date of the international match or window.
        /// </summary>
        public DateOnly MatchDate { get; init; }

        /// <summary>
        /// Current status of the call-up.
        /// </summary>
        public CallUpStatus Status { get; init; }

        /// <summary>
        /// Number of caps earned during this call-up window (0 or 1 per match).
        /// </summary>
        public int CapsEarned { get; init; }

        /// <summary>
        /// Goals scored during this call-up window.
        /// </summary>
        public int GoalsScored { get; init; }

        public InternationalCallUp(
            Guid id,
            Guid playerId,
            Guid nationalTeamId,
            string tournamentName,
            DateOnly matchDate,
            CallUpStatus status = CallUpStatus.Available,
            int capsEarned = 0,
            int goalsScored = 0)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Call-up ID cannot be empty.", nameof(id));
            if (playerId == Guid.Empty)
                throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            if (nationalTeamId == Guid.Empty)
                throw new ArgumentException("National team ID cannot be empty.", nameof(nationalTeamId));
            if (string.IsNullOrWhiteSpace(tournamentName))
                throw new ArgumentException("Tournament name cannot be null or whitespace.", nameof(tournamentName));
            if (capsEarned < 0)
                throw new ArgumentOutOfRangeException(nameof(capsEarned), "Caps earned cannot be negative.");
            if (goalsScored < 0)
                throw new ArgumentOutOfRangeException(nameof(goalsScored), "Goals scored cannot be negative.");

            Id = id;
            PlayerId = playerId;
            NationalTeamId = nationalTeamId;
            TournamentName = tournamentName.Trim();
            MatchDate = matchDate;
            Status = status;
            CapsEarned = capsEarned;
            GoalsScored = goalsScored;
        }

        public static InternationalCallUp Create(
            Guid playerId,
            Guid nationalTeamId,
            string tournamentName,
            DateOnly matchDate,
            CallUpStatus status = CallUpStatus.Available)
        {
            return new InternationalCallUp(
                Guid.NewGuid(), playerId, nationalTeamId, tournamentName, matchDate, status);
        }

        /// <summary>
        /// Returns a new call-up with updated match results.
        /// </summary>
        public InternationalCallUp WithResults(int capsEarned, int goalsScored)
        {
            return this with { CapsEarned = capsEarned, GoalsScored = goalsScored, Status = CallUpStatus.CalledUp };
        }
    }
}
