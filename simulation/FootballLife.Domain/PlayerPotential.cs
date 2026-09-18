using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Encodes a probabilistic development ceiling for a player.
    /// Rather than a rigid predetermined ceiling, potential provides a bounded stochastic band
    /// whose realization probability naturally declines after peak physical age.
    /// </summary>
    public sealed record PlayerPotential
    {
        /// <summary>Minimum valid potential rating ceiling.</summary>
        public const byte MinRating = 50;

        /// <summary>Maximum valid potential rating ceiling.</summary>
        public const byte MaxRating = 99;

        /// <summary>The peak football age after which realization probability begins to decline.</summary>
        public const int PeakAge = 28;

        private readonly byte _potentialRating;

        /// <summary>
        /// Baseline potential rating ceiling within [50, 99].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set outside [50, 99].</exception>
        public byte PotentialRating
        {
            get => _potentialRating;
            init
            {
                if (value < MinRating || value > MaxRating)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Potential rating must be bounded within [{MinRating}, {MaxRating}]. Actual: {value}");
                }
                _potentialRating = value;
            }
        }

        /// <summary>
        /// Stochastic variance band category that dictates uncertainty in progression ceilings.
        /// </summary>
        public PotentialRange Range { get; init; }

        /// <summary>
        /// Initializes a new instance of <see cref="PlayerPotential"/> with strict validation.
        /// </summary>
        /// <param name="potentialRating">The baseline potential rating [50, 99].</param>
        /// <param name="range">The stochastic band category.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when potentialRating is out of bounds.</exception>
        public PlayerPotential(byte potentialRating, PotentialRange range)
        {
            if (potentialRating < MinRating || potentialRating > MaxRating)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(potentialRating),
                    $"Potential rating must be bounded within [{MinRating}, {MaxRating}]. Actual: {potentialRating}");
            }

            _potentialRating = potentialRating;
            Range = range;
        }

        /// <summary>
        /// Factory method that creates a <see cref="PlayerPotential"/> by clamping the rating into [50, 99].
        /// </summary>
        /// <param name="rawRating">The raw rating, which will be clamped to [50, 99].</param>
        /// <param name="range">The stochastic band category.</param>
        /// <returns>A validated <see cref="PlayerPotential"/> instance.</returns>
        public static PlayerPotential CreateClamped(int rawRating, PotentialRange range)
        {
            byte clamped = (byte)Math.Max((int)MinRating, Math.Min((int)MaxRating, rawRating));
            return new PlayerPotential(clamped, range);
        }

        /// <summary>
        /// Returns the half-width variance points associated with the <see cref="Range"/>.
        /// </summary>
        public int GetVarianceBandWidth()
        {
            return Range switch
            {
                PotentialRange.Low => 2,
                PotentialRange.Medium => 4,
                PotentialRange.High => 6,
                PotentialRange.Elite => 8,
                _ => 4
            };
        }

        /// <summary>
        /// Computes the absolute minimum and maximum potential rating bounds taking stochastic variance into account.
        /// </summary>
        /// <returns>A tuple of (MinCeiling, MaxCeiling) clamped to [50, 99].</returns>
        public (byte MinCeiling, byte MaxCeiling) GetCeilingBand()
        {
            int band = GetVarianceBandWidth();
            byte min = (byte)Math.Max((int)MinRating, _potentialRating - band);
            byte max = (byte)Math.Min((int)MaxRating, _potentialRating + band);
            return (min, max);
        }

        /// <summary>
        /// Calculates the probability [0.0f, 1.0f] of a player realizing their remaining development potential
        /// at a given biological age. Remains at maximum through age 28, then decreases monotonically.
        /// </summary>
        /// <param name="age">Player's current age in full years.</param>
        /// <returns>A realization factor between 0.0f and 1.0f.</returns>
        public float RealizationProbability(int age)
        {
            if (age <= PeakAge)
            {
                return 1.0f;
            }

            // Exponential decay after peak age: declines smoothly each year post-28
            int yearsPastPeak = age - PeakAge;
            double decay = Math.Exp(-0.15 * yearsPastPeak);
            return (float)Math.Max(0.01, Math.Min(1.0, decay));
        }
    }
}
