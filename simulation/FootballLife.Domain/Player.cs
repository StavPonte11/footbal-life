using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the immutable core identity of a footballer.
    /// </summary>
    public sealed record Player
    {
        /// <summary>
        /// Unique persistent identifier that remains stable across saves and transfers.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full display name of the footballer.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Nationality of the player (country code or name).
        /// </summary>
        public string Nationality { get; init; }

        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateOnly DateOfBirth { get; init; }

        /// <summary>
        /// Preferred kicking foot.
        /// </summary>
        public Foot PreferredFoot { get; init; }

        /// <summary>
        /// Primary on-pitch position.
        /// </summary>
        public Position PrimaryPosition { get; init; }

        /// <summary>
        /// Optional secondary position where the player can also perform.
        /// </summary>
        public Position? SecondaryPosition { get; init; }

        public Player(
            Guid id,
            string name,
            string nationality,
            DateOnly dateOfBirth,
            Foot preferredFoot,
            Position primaryPosition,
            Position? secondaryPosition = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Player ID cannot be an empty Guid.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Player name cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(nationality))
            {
                throw new ArgumentException("Player nationality cannot be null or whitespace.", nameof(nationality));
            }

            Id = id;
            Name = name.Trim();
            Nationality = nationality.Trim();
            DateOfBirth = dateOfBirth;
            PreferredFoot = preferredFoot;
            PrimaryPosition = primaryPosition;
            SecondaryPosition = secondaryPosition;
        }

        /// <summary>
        /// Factory method to create a new player with an automatically generated unique ID.
        /// </summary>
        public static Player Create(
            string name,
            string nationality,
            DateOnly dateOfBirth,
            Foot preferredFoot,
            Position primaryPosition,
            Position? secondaryPosition = null)
        {
            return new Player(
                Guid.NewGuid(),
                name,
                nationality,
                dateOfBirth,
                preferredFoot,
                primaryPosition,
                secondaryPosition);
        }

        /// <summary>
        /// Calculates the player's age in full years at a specified target date.
        /// </summary>
        public int GetAgeAt(DateOnly date)
        {
            if (date < DateOfBirth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(date),
                    $"Target date ({date:yyyy-MM-dd}) cannot be earlier than player date of birth ({DateOfBirth:yyyy-MM-dd}).");
            }

            int age = date.Year - DateOfBirth.Year;
            if (date.Month < DateOfBirth.Month || (date.Month == DateOfBirth.Month && date.Day < DateOfBirth.Day))
            {
                age--;
            }

            return age;
        }
    }
}
