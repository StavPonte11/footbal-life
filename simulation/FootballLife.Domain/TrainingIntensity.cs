namespace FootballLife.Domain
{
    /// <summary>
    /// Describes the effort level of a training session.
    /// Multiplied against BaseXP and BaseFatigueCost to determine the final values.
    /// </summary>
    public enum TrainingIntensity
    {
        /// <summary>Multiplier: 0.5x XP, 0.5x fatigue.</summary>
        Light,

        /// <summary>Multiplier: 1.0x XP, 1.0x fatigue (baseline).</summary>
        Moderate,

        /// <summary>Multiplier: 1.5x XP, 1.5x fatigue.</summary>
        Hard,

        /// <summary>Multiplier: 2.0x XP, 2.0x fatigue. Also applies 2× injury risk.</summary>
        Maximum,
    }
}
