using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a scheduled or completed match fixture between two clubs on a specific date.
    /// </summary>
    public sealed record FixtureId
    {
        public Guid Id { get; init; }
        public Guid HomeClubId { get; init; }
        public Guid AwayClubId { get; init; }
        public DateOnly Date { get; init; }

        public FixtureId(Guid id, Guid homeClubId, Guid awayClubId, DateOnly date)
        {
            if (id == Guid.Empty) throw new ArgumentException("Fixture ID cannot be empty.", nameof(id));
            if (homeClubId == Guid.Empty) throw new ArgumentException("Home club ID cannot be empty.", nameof(homeClubId));
            if (awayClubId == Guid.Empty) throw new ArgumentException("Away club ID cannot be empty.", nameof(awayClubId));
            if (homeClubId == awayClubId) throw new ArgumentException("Home and away clubs cannot be the same club.");

            Id = id;
            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            Date = date;
        }

        public static FixtureId Create(Guid homeClubId, Guid awayClubId, DateOnly date) =>
            new FixtureId(Guid.NewGuid(), homeClubId, awayClubId, date);
    }
}
