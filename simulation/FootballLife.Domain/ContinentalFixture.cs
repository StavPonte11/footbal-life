using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Stages in a continental knockout competition.
    /// </summary>
    public enum ContinentalStage
    {
        GroupStage = 0,
        RoundOf16 = 1,
        QuarterFinal = 2,
        SemiFinal = 3,
        Final = 4
    }

    /// <summary>
    /// Represents a single fixture (match) in a continental competition.
    /// </summary>
    public sealed record ContinentalFixture
    {
        /// <summary>
        /// Unique identifier for this fixture.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Home club in this fixture.
        /// </summary>
        public Guid HomeClubId { get; init; }

        /// <summary>
        /// Away club in this fixture.
        /// </summary>
        public Guid AwayClubId { get; init; }

        /// <summary>
        /// Competition stage this fixture belongs to.
        /// </summary>
        public ContinentalStage Stage { get; init; }

        /// <summary>
        /// Leg number (1 or 2 for two-leg knockout ties; 0 for single-leg group matches or final).
        /// </summary>
        public int Leg { get; init; }

        /// <summary>
        /// Group name (e.g., "A", "B") if this is a group stage fixture, or null for knockout.
        /// </summary>
        public string? GroupName { get; init; }

        /// <summary>
        /// Home team goals scored.
        /// </summary>
        public int HomeScore { get; init; }

        /// <summary>
        /// Away team goals scored.
        /// </summary>
        public int AwayScore { get; init; }

        /// <summary>
        /// Whether this fixture has been played to completion.
        /// </summary>
        public bool IsCompleted { get; init; }

        public ContinentalFixture(
            Guid id,
            Guid homeClubId,
            Guid awayClubId,
            ContinentalStage stage,
            int leg = 0,
            string? groupName = null,
            int homeScore = 0,
            int awayScore = 0,
            bool isCompleted = false)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Fixture ID cannot be empty.", nameof(id));
            if (homeClubId == Guid.Empty)
                throw new ArgumentException("Home club ID cannot be empty.", nameof(homeClubId));
            if (awayClubId == Guid.Empty)
                throw new ArgumentException("Away club ID cannot be empty.", nameof(awayClubId));
            if (homeClubId == awayClubId)
                throw new ArgumentException("A club cannot play against itself.");
            if (homeScore < 0)
                throw new ArgumentOutOfRangeException(nameof(homeScore), "Score cannot be negative.");
            if (awayScore < 0)
                throw new ArgumentOutOfRangeException(nameof(awayScore), "Score cannot be negative.");

            Id = id;
            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            Stage = stage;
            Leg = leg;
            GroupName = groupName;
            HomeScore = homeScore;
            AwayScore = awayScore;
            IsCompleted = isCompleted;
        }

        public static ContinentalFixture Create(
            Guid homeClubId,
            Guid awayClubId,
            ContinentalStage stage,
            int leg = 0,
            string? groupName = null)
        {
            return new ContinentalFixture(
                Guid.NewGuid(), homeClubId, awayClubId, stage, leg, groupName);
        }

        /// <summary>
        /// Returns a new fixture with a recorded result.
        /// </summary>
        public ContinentalFixture WithResult(int homeScore, int awayScore)
        {
            return this with { HomeScore = homeScore, AwayScore = awayScore, IsCompleted = true };
        }
    }
}
