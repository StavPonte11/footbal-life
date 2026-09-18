namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the width of the stochastic uncertainty band for player development potential.
    /// Higher ranges allow larger deviations above or below the baseline potential rating.
    /// </summary>
    public enum PotentialRange
    {
        /// <summary>Narrow variance band (±2 rating points). Predictable development.</summary>
        Low = 0,

        /// <summary>Standard variance band (±4 rating points). Typical progression variance.</summary>
        Medium = 1,

        /// <summary>Wide variance band (±6 rating points). Volatile development curve.</summary>
        High = 2,

        /// <summary>Extreme variance band (±8 rating points). Boom-or-bust generational prospect.</summary>
        Elite = 3
    }
}
