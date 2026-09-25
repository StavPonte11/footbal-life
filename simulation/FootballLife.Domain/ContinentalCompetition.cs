using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a continental club competition (e.g., Champions Cup) with qualification,
    /// group stage, and knockout rounds.
    /// </summary>
    public sealed record ContinentalCompetition
    {
        /// <summary>
        /// Unique identifier for this competition instance.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Display name (e.g., "Champions Cup", "Continental Shield").
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Total number of club slots in the tournament (typically 32).
        /// </summary>
        public int TotalSlots { get; init; }

        /// <summary>
        /// Prestige bonus applied to participating clubs' reputation.
        /// </summary>
        public int PrestigeBonus { get; init; }

        /// <summary>
        /// Total prize money pool distributed across finishing positions.
        /// </summary>
        public decimal PrizeMoney { get; init; }

        /// <summary>
        /// Club IDs that have qualified for this competition.
        /// </summary>
        public IReadOnlyList<Guid> QualifiedClubIds { get; init; }

        /// <summary>
        /// Group stage standings, if the competition has progressed to or past groups.
        /// </summary>
        public IReadOnlyList<ContinentalGroupStanding> GroupStandings { get; init; }

        /// <summary>
        /// Knockout round fixtures (RO16, QF, SF, Final).
        /// </summary>
        public IReadOnlyList<ContinentalFixture> KnockoutFixtures { get; init; }

        /// <summary>
        /// The club that won the competition, or null if not yet decided.
        /// </summary>
        public Guid? WinnerId { get; init; }

        /// <summary>
        /// Whether the competition has been fully resolved.
        /// </summary>
        public bool IsCompleted { get; init; }

        public ContinentalCompetition(
            Guid id,
            string name,
            int totalSlots = 32,
            int prestigeBonus = 10,
            decimal prizeMoney = 50_000_000m,
            IReadOnlyList<Guid>? qualifiedClubIds = null,
            IReadOnlyList<ContinentalGroupStanding>? groupStandings = null,
            IReadOnlyList<ContinentalFixture>? knockoutFixtures = null,
            Guid? winnerId = null,
            bool isCompleted = false)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Competition ID cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Competition name cannot be null or whitespace.", nameof(name));
            if (totalSlots < 2)
                throw new ArgumentOutOfRangeException(nameof(totalSlots), "Must have at least 2 slots.");
            if (prestigeBonus < 0)
                throw new ArgumentOutOfRangeException(nameof(prestigeBonus), "Prestige bonus cannot be negative.");
            if (prizeMoney < 0)
                throw new ArgumentOutOfRangeException(nameof(prizeMoney), "Prize money cannot be negative.");

            Id = id;
            Name = name.Trim();
            TotalSlots = totalSlots;
            PrestigeBonus = prestigeBonus;
            PrizeMoney = prizeMoney;
            QualifiedClubIds = qualifiedClubIds ?? Array.Empty<Guid>();
            GroupStandings = groupStandings ?? Array.Empty<ContinentalGroupStanding>();
            KnockoutFixtures = knockoutFixtures ?? Array.Empty<ContinentalFixture>();
            WinnerId = winnerId;
            IsCompleted = isCompleted;
        }

        public static ContinentalCompetition Create(
            string name,
            int totalSlots = 32,
            int prestigeBonus = 10,
            decimal prizeMoney = 50_000_000m)
        {
            return new ContinentalCompetition(
                Guid.NewGuid(), name, totalSlots, prestigeBonus, prizeMoney);
        }
    }
}
