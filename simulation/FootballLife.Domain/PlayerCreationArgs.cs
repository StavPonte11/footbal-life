using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Immutable input arguments for creating a new player career via <c>PlayerFactory</c>.
    /// All required fields are validated by the factory, not here, to keep the record a plain data container.
    /// </summary>
    public sealed record PlayerCreationArgs
    {
        /// <summary>Display name of the new player.</summary>
        public string Name { get; init; }

        /// <summary>Nationality (country code or full name).</summary>
        public string Nationality { get; init; }

        /// <summary>Date of birth — determines starting age and development curve band.</summary>
        public DateOnly DateOfBirth { get; init; }

        /// <summary>Primary on-pitch position for the career.</summary>
        public Position PrimaryPosition { get; init; }

        /// <summary>Preferred kicking foot.</summary>
        public Foot PreferredFoot { get; init; }

        /// <summary>
        /// The club the player starts their career at.
        /// Must correspond to a club that exists in the <see cref="WorldState"/> passed to the factory.
        /// </summary>
        public Guid StartingClubId { get; init; }

        /// <summary>
        /// Base value for initial ability generation.
        /// All attributes will be generated around this baseline weighted by position relevance.
        /// Valid range: [40, 60].
        /// </summary>
        public byte StartingAbilityBase { get; init; }

        public PlayerCreationArgs(
            string name,
            string nationality,
            DateOnly dateOfBirth,
            Position primaryPosition,
            Foot preferredFoot,
            Guid startingClubId,
            byte startingAbilityBase)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(nationality))
                throw new ArgumentException("Player nationality cannot be null or whitespace.", nameof(nationality));
            if (startingClubId == Guid.Empty)
                throw new ArgumentException("Starting club ID cannot be empty.", nameof(startingClubId));
            if (startingAbilityBase < 40 || startingAbilityBase > 60)
                throw new ArgumentOutOfRangeException(nameof(startingAbilityBase),
                    $"Starting ability base must be in [40, 60]. Actual: {startingAbilityBase}");

            Name = name.Trim();
            Nationality = nationality.Trim();
            DateOfBirth = dateOfBirth;
            PrimaryPosition = primaryPosition;
            PreferredFoot = preferredFoot;
            StartingClubId = startingClubId;
            StartingAbilityBase = startingAbilityBase;
        }
    }
}
