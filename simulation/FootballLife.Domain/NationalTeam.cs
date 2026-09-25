using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a national football team competing in international tournaments.
    /// </summary>
    public sealed record NationalTeam
    {
        public const int MinRanking = 1;
        public const int MaxRanking = 210;

        private readonly int _fifaRanking;

        /// <summary>
        /// Unique persistent identifier of the national team.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full country name (e.g., "England", "Brazil", "Germany").
        /// </summary>
        public string CountryName { get; init; }

        /// <summary>
        /// ISO 3166-1 alpha-3 country code (e.g., "ENG", "BRA", "GER").
        /// </summary>
        public string CountryCode { get; init; }

        /// <summary>
        /// Current FIFA world ranking position [1, 210].
        /// </summary>
        public int FifaRanking
        {
            get => _fifaRanking;
            init
            {
                if (value < MinRanking || value > MaxRanking)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"FIFA ranking must be in [{MinRanking}, {MaxRanking}]. Actual: {value}");
                }
                _fifaRanking = value;
            }
        }

        /// <summary>
        /// Confederation the team belongs to (e.g., "UEFA", "CONMEBOL").
        /// </summary>
        public string Confederation { get; init; }

        /// <summary>
        /// Name of the current national team manager.
        /// </summary>
        public string ManagerName { get; init; }

        /// <summary>
        /// Immutable list of player IDs currently in the national team squad.
        /// </summary>
        public IReadOnlyList<Guid> SquadPlayerIds { get; init; }

        public NationalTeam(
            Guid id,
            string countryName,
            string countryCode,
            int fifaRanking,
            string confederation,
            string managerName,
            IReadOnlyList<Guid>? squadPlayerIds = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("National team ID cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(countryName))
                throw new ArgumentException("Country name cannot be null or whitespace.", nameof(countryName));
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("Country code cannot be null or whitespace.", nameof(countryCode));
            if (string.IsNullOrWhiteSpace(confederation))
                throw new ArgumentException("Confederation cannot be null or whitespace.", nameof(confederation));
            if (string.IsNullOrWhiteSpace(managerName))
                throw new ArgumentException("Manager name cannot be null or whitespace.", nameof(managerName));
            if (fifaRanking < MinRanking || fifaRanking > MaxRanking)
                throw new ArgumentOutOfRangeException(nameof(fifaRanking),
                    $"FIFA ranking must be in [{MinRanking}, {MaxRanking}]. Actual: {fifaRanking}");

            Id = id;
            CountryName = countryName.Trim();
            CountryCode = countryCode.Trim().ToUpperInvariant();
            _fifaRanking = fifaRanking;
            Confederation = confederation.Trim();
            ManagerName = managerName.Trim();
            SquadPlayerIds = squadPlayerIds ?? Array.Empty<Guid>();
        }

        public static NationalTeam Create(
            string countryName,
            string countryCode,
            int fifaRanking,
            string confederation,
            string managerName,
            IReadOnlyList<Guid>? squadPlayerIds = null)
        {
            return new NationalTeam(
                Guid.NewGuid(), countryName, countryCode, fifaRanking,
                confederation, managerName, squadPlayerIds);
        }

        /// <summary>
        /// Returns a new NationalTeam with an updated squad roster.
        /// </summary>
        public NationalTeam WithSquad(IReadOnlyList<Guid> playerIds)
        {
            return this with { SquadPlayerIds = playerIds ?? Array.Empty<Guid>() };
        }

        /// <summary>
        /// Returns a new NationalTeam with an updated FIFA ranking.
        /// </summary>
        public NationalTeam WithRanking(int newRanking)
        {
            return this with { FifaRanking = newRanking };
        }
    }
}
