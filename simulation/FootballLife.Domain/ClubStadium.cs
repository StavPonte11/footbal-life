using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the home ground infrastructure and pitch condition of a club.
    /// </summary>
    public sealed record ClubStadium
    {
        public const int MinPitchQuality = 1;
        public const int MaxPitchQuality = 5;

        private readonly int _pitchQuality;

        /// <summary>
        /// Name of the stadium or arena.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Total spectator capacity (>= 100).
        /// </summary>
        public int Capacity { get; init; }

        /// <summary>
        /// Quality rating of the playing pitch [1, 5]. Higher quality reduces fatigue and miscontrols.
        /// </summary>
        public int PitchQuality
        {
            get => _pitchQuality;
            init
            {
                if (value < MinPitchQuality || value > MaxPitchQuality)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Pitch quality must be in [{MinPitchQuality}, {MaxPitchQuality}]. Actual: {value}");
                }
                _pitchQuality = value;
            }
        }

        public ClubStadium(string name, int capacity, int pitchQuality)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Stadium name cannot be null or whitespace.", nameof(name));
            }

            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Stadium capacity must be positive.");
            }

            if (pitchQuality < MinPitchQuality || pitchQuality > MaxPitchQuality)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pitchQuality),
                    $"Pitch quality must be in [{MinPitchQuality}, {MaxPitchQuality}]. Actual: {pitchQuality}");
            }

            Name = name.Trim();
            Capacity = capacity;
            _pitchQuality = pitchQuality;
        }

        public static ClubStadium Default(string clubName) =>
            new ClubStadium($"{clubName} Stadium", 15_000, 3);
    }
}
