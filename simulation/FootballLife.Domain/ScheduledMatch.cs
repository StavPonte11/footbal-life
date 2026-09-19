using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a single scheduled league match between two clubs on a specific matchday.
    /// Produced by <c>FixtureGenerator</c> and stored in <see cref="Matchday.Fixtures"/>.
    /// </summary>
    public sealed record ScheduledMatch
    {
        /// <summary>Unique identifier for this fixture.</summary>
        public Guid Id { get; init; }

        /// <summary>Calendar date the match is scheduled to be played.</summary>
        public DateOnly Date { get; init; }

        /// <summary>Club playing at home.</summary>
        public Guid HomeClubId { get; init; }

        /// <summary>Club playing away.</summary>
        public Guid AwayClubId { get; init; }

        /// <summary>The league competition this fixture belongs to.</summary>
        public Guid LeagueId { get; init; }

        public ScheduledMatch(Guid id, DateOnly date, Guid homeClubId, Guid awayClubId, Guid leagueId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ScheduledMatch ID cannot be empty.", nameof(id));
            if (homeClubId == Guid.Empty)
                throw new ArgumentException("Home club ID cannot be empty.", nameof(homeClubId));
            if (awayClubId == Guid.Empty)
                throw new ArgumentException("Away club ID cannot be empty.", nameof(awayClubId));
            if (homeClubId == awayClubId)
                throw new ArgumentException("Home and away clubs cannot be the same club.");
            if (leagueId == Guid.Empty)
                throw new ArgumentException("League ID cannot be empty.", nameof(leagueId));

            Id = id;
            Date = date;
            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            LeagueId = leagueId;
        }

        public static ScheduledMatch Create(DateOnly date, Guid homeClubId, Guid awayClubId, Guid leagueId)
            => new ScheduledMatch(Guid.NewGuid(), date, homeClubId, awayClubId, leagueId);
    }
}
