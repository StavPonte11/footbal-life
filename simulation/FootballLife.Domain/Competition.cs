using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a cup, continental, or international tournament competition.
    /// </summary>
    public sealed record Competition
    {
        private readonly int _totalTeams;

        /// <summary>
        /// Unique persistent identifier of the competition.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full display name of the tournament (e.g., "FA Cup", "Champions Cup").
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Tournament bracket or league format structure.
        /// </summary>
        public CompetitionFormat Format { get; init; }

        /// <summary>
        /// Total number of competing clubs/nations.
        /// </summary>
        public int TotalTeams
        {
            get => _totalTeams;
            init
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Competition must involve at least 2 teams.");
                }
                _totalTeams = value;
            }
        }

        public Competition(Guid id, string name, CompetitionFormat format, int totalTeams = 32)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Competition ID cannot be an empty Guid.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Competition name cannot be null or whitespace.", nameof(name));
            }

            if (totalTeams < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(totalTeams), "Competition must involve at least 2 teams.");
            }

            Id = id;
            Name = name.Trim();
            Format = format;
            _totalTeams = totalTeams;
        }

        public static Competition Create(string name, CompetitionFormat format, int totalTeams = 32) =>
            new Competition(Guid.NewGuid(), name, format, totalTeams);
    }
}
