using System;
using System.Collections.Generic;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Deterministic, seeded pseudo-random number generator for all simulation systems.
    /// Wraps <see cref="System.Random"/> and exposes simulation-domain helpers.
    /// All stochastic outcomes must route through this class so that any replay or test
    /// can reproduce identical results by supplying the same seed.
    /// </summary>
    /// <remarks>
    /// Zero allocations after construction: the Gaussian implementation stores two
    /// <c>double</c> fields (<c>_spare</c> and <c>_hasSpare</c>) via the Box–Muller transform
    /// so no heap objects are needed between calls.
    /// </remarks>
    public sealed class SimulationRandom
    {
        private readonly Random _rng;

        // Box–Muller spare value for NextGaussian
        private double _spare;
        private bool _hasSpare;

        /// <summary>
        /// Initialises a new <see cref="SimulationRandom"/> with the given <paramref name="seed"/>.
        /// </summary>
        /// <param name="seed">
        /// Deterministic seed. Identical seeds produce identical output sequences.
        /// </param>
        public SimulationRandom(int seed)
        {
            _rng = new Random(seed);
        }

        // ─── Core Generators ─────────────────────────────────────────────────

        /// <summary>
        /// Returns a uniform float in [<paramref name="min"/>, <paramref name="max"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> &gt;= <paramref name="max"/>.</exception>
        public float NextFloat(float min, float max)
        {
            if (min >= max)
                throw new ArgumentException($"min ({min}) must be less than max ({max}).", nameof(min));
            return min + (float)_rng.NextDouble() * (max - min);
        }

        /// <summary>
        /// Returns a uniform integer in [<paramref name="min"/>, <paramref name="maxExclusive"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> &gt;= <paramref name="maxExclusive"/>.</exception>
        public int NextInt(int min, int maxExclusive)
        {
            if (min >= maxExclusive)
                throw new ArgumentException($"min ({min}) must be less than maxExclusive ({maxExclusive}).", nameof(min));
            return _rng.Next(min, maxExclusive);
        }

        /// <summary>
        /// Returns <c>true</c> with the given <paramref name="probability"/> ∈ [0, 1].
        /// </summary>
        /// <param name="probability">Probability of returning <c>true</c>. Clamped to [0, 1].</param>
        public bool NextBool(float probability)
        {
            float p = Math.Max(0f, Math.Min(1f, probability));
            return _rng.NextDouble() < p;
        }

        /// <summary>
        /// Returns a normally-distributed float with the given <paramref name="mean"/> and
        /// <paramref name="stddev"/> using the Box–Muller transform.
        /// </summary>
        /// <param name="mean">Distribution mean.</param>
        /// <param name="stddev">Standard deviation. Clamped to ≥ 0.</param>
        public float NextGaussian(float mean, float stddev)
        {
            float sd = Math.Max(0f, stddev);
            if (sd == 0f) return mean;

            double u1, u2, mag, sample;

            if (_hasSpare)
            {
                _hasSpare = false;
                sample = _spare;
            }
            else
            {
                do
                {
                    u1 = _rng.NextDouble() * 2.0 - 1.0;
                    u2 = _rng.NextDouble() * 2.0 - 1.0;
                    mag = u1 * u1 + u2 * u2;
                } while (mag >= 1.0 || mag == 0.0);

                double factor = Math.Sqrt(-2.0 * Math.Log(mag) / mag);
                _spare = u2 * factor;
                _hasSpare = true;
                sample = u1 * factor;
            }

            return (float)(mean + sd * sample);
        }

        /// <summary>
        /// Picks a uniformly random element from <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <exception cref="ArgumentException">Thrown when <paramref name="list"/> is empty.</exception>
        public T Pick<T>(IReadOnlyList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (list.Count == 0) throw new ArgumentException("Cannot pick from an empty list.", nameof(list));
            return list[_rng.Next(list.Count)];
        }
    }
}
