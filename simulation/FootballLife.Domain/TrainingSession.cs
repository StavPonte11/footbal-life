using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a single, focused training block performed by the player.
    /// Immutable input to <c>TrainingSystem.CalculateXP</c>.
    /// </summary>
    public sealed record TrainingSession
    {
        private const int MinDuration = 15;
        private const int MaxDuration = 120;

        /// <summary>The category of training activity.</summary>
        public TrainingType Type { get; init; }

        /// <summary>The effort level — drives XP and fatigue multipliers.</summary>
        public TrainingIntensity Intensity { get; init; }

        /// <summary>
        /// Duration of the session in minutes. Valid range: [15, 120].
        /// Shorter than 15 is considered a warm-up (no training effect);
        /// longer than 120 is unrealistic for a single focused session.
        /// </summary>
        public int DurationMinutes { get; init; }

        /// <summary>Calendar date the session was performed.</summary>
        public DateOnly Date { get; init; }

        public TrainingSession(TrainingType type, TrainingIntensity intensity, int durationMinutes, DateOnly date)
        {
            if (durationMinutes < MinDuration || durationMinutes > MaxDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationMinutes),
                    $"DurationMinutes must be in [{MinDuration}, {MaxDuration}]. Actual: {durationMinutes}");
            }

            Type = type;
            Intensity = intensity;
            DurationMinutes = durationMinutes;
            Date = date;
        }

        /// <summary>
        /// Creates a training session with common defaults: Moderate intensity, 60 minutes, today.
        /// </summary>
        public static TrainingSession Create(TrainingType type, TrainingIntensity intensity, int durationMinutes, DateOnly date)
            => new TrainingSession(type, intensity, durationMinutes, date);
    }
}
