using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the temporary physical and mental condition of a footballer.
    /// Strictly separated from permanent abilities. Values are floats clamped to [0f, 100f].
    /// </summary>
    public sealed record PlayerState
    {
        public const float MinValue = 0f;
        public const float MaxValue = 100f;
        public const float NeutralForm = 50f;

        private readonly float _fatigue;
        private readonly float _confidence;
        private readonly float _form;
        private readonly float _happiness;
        private readonly float _motivation;
        private readonly float _morale;
        private readonly float _fitness;

        /// <summary>
        /// Accumulated physical exhaustion (0 = fresh, 100 = completely exhausted).
        /// </summary>
        public float Fatigue
        {
            get => _fatigue;
            init => _fatigue = Clamp(value);
        }

        /// <summary>
        /// Self-belief and composure under pressure (0 = shattered, 50 = neutral, 100 = supreme confidence).
        /// </summary>
        public float Confidence
        {
            get => _confidence;
            init => _confidence = Clamp(value);
        }

        /// <summary>
        /// Recent match momentum and form (0 = slump, 50 = neutral, 100 = peak form).
        /// </summary>
        public float Form
        {
            get => _form;
            init => _form = Clamp(value);
        }

        /// <summary>
        /// Personal happiness and life satisfaction (0 = miserable, 100 = ecstatic).
        /// </summary>
        public float Happiness
        {
            get => _happiness;
            init => _happiness = Clamp(value);
        }

        /// <summary>
        /// Drive and ambition to train and compete (0 = checked out, 100 = laser-focused).
        /// </summary>
        public float Motivation
        {
            get => _motivation;
            init => _motivation = Clamp(value);
        }

        /// <summary>
        /// Team and dressing room spirit (0 = defeated, 100 = rock-solid morale).
        /// </summary>
        public float Morale
        {
            get => _morale;
            init => _morale = Clamp(value);
        }

        /// <summary>
        /// Match fitness and physical conditioning (0 = unfit, 100 = peak match fitness).
        /// </summary>
        public float Fitness
        {
            get => _fitness;
            init => _fitness = Clamp(value);
        }

        public PlayerState(
            float fatigue,
            float confidence,
            float form,
            float happiness,
            float motivation,
            float morale,
            float fitness)
        {
            _fatigue = Clamp(fatigue);
            _confidence = Clamp(confidence);
            _form = Clamp(form);
            _happiness = Clamp(happiness);
            _motivation = Clamp(motivation);
            _morale = Clamp(morale);
            _fitness = Clamp(fitness);
        }

        /// <summary>
        /// Creates a baseline initial state for a player at the start of a season or career.
        /// </summary>
        public static PlayerState Default => new PlayerState(
            fatigue: 0f,
            confidence: 50f,
            form: NeutralForm,
            happiness: 75f,
            motivation: 80f,
            morale: 75f,
            fitness: 100f);

        /// <summary>
        /// Decays form towards neutral (50) over time when no matches are played.
        /// </summary>
        /// <param name="decayRate">Proportion of deviation to remove, between 0.0 and 1.0.</param>
        public PlayerState DecayFormTowardsNeutral(float decayRate = 0.1f)
        {
            float rate = Math.Max(0f, Math.Min(1f, decayRate));
            float newForm = _form + (NeutralForm - _form) * rate;
            return this with { Form = newForm };
        }

        /// <summary>
        /// Clamps a float value within the [0, 100] range, handling NaN/infinities safely.
        /// </summary>
        public static float Clamp(float value)
        {
            if (float.IsNaN(value)) return MinValue;
            if (value < MinValue) return MinValue;
            if (value > MaxValue) return MaxValue;
            return value;
        }
    }
}
