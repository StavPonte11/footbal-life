using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the immutable core football abilities of a player across 15 skills.
    /// Each attribute is represented as a byte clamped to [0, 100].
    /// </summary>
    public sealed record PlayerAbilities
    {
        public const byte MinValue = 0;
        public const byte MaxValue = 100;
        public const byte DefaultBaseline = 50;

        private readonly byte _pace;
        private readonly byte _acceleration;
        private readonly byte _stamina;
        private readonly byte _strength;
        private readonly byte _agility;

        private readonly byte _passing;
        private readonly byte _shooting;
        private readonly byte _dribbling;
        private readonly byte _crossing;
        private readonly byte _firstTouch;
        private readonly byte _tackling;

        private readonly byte _vision;
        private readonly byte _composure;
        private readonly byte _positioning;
        private readonly byte _decisionMaking;

        // Physical Attributes (5)
        public byte Pace
        {
            get => _pace;
            init => _pace = Clamp(value);
        }

        public byte Acceleration
        {
            get => _acceleration;
            init => _acceleration = Clamp(value);
        }

        public byte Stamina
        {
            get => _stamina;
            init => _stamina = Clamp(value);
        }

        public byte Strength
        {
            get => _strength;
            init => _strength = Clamp(value);
        }

        public byte Agility
        {
            get => _agility;
            init => _agility = Clamp(value);
        }

        // Technical Attributes (6)
        public byte Passing
        {
            get => _passing;
            init => _passing = Clamp(value);
        }

        public byte Shooting
        {
            get => _shooting;
            init => _shooting = Clamp(value);
        }

        public byte Dribbling
        {
            get => _dribbling;
            init => _dribbling = Clamp(value);
        }

        public byte Crossing
        {
            get => _crossing;
            init => _crossing = Clamp(value);
        }

        public byte FirstTouch
        {
            get => _firstTouch;
            init => _firstTouch = Clamp(value);
        }

        public byte Tackling
        {
            get => _tackling;
            init => _tackling = Clamp(value);
        }

        // Mental Attributes (4)
        public byte Vision
        {
            get => _vision;
            init => _vision = Clamp(value);
        }

        public byte Composure
        {
            get => _composure;
            init => _composure = Clamp(value);
        }

        public byte Positioning
        {
            get => _positioning;
            init => _positioning = Clamp(value);
        }

        public byte DecisionMaking
        {
            get => _decisionMaking;
            init => _decisionMaking = Clamp(value);
        }

        public PlayerAbilities(
            int pace,
            int acceleration,
            int stamina,
            int strength,
            int agility,
            int passing,
            int shooting,
            int dribbling,
            int crossing,
            int firstTouch,
            int tackling,
            int vision,
            int composure,
            int positioning,
            int decisionMaking)
        {
            _pace = Clamp(pace);
            _acceleration = Clamp(acceleration);
            _stamina = Clamp(stamina);
            _strength = Clamp(strength);
            _agility = Clamp(agility);

            _passing = Clamp(passing);
            _shooting = Clamp(shooting);
            _dribbling = Clamp(dribbling);
            _crossing = Clamp(crossing);
            _firstTouch = Clamp(firstTouch);
            _tackling = Clamp(tackling);

            _vision = Clamp(vision);
            _composure = Clamp(composure);
            _positioning = Clamp(positioning);
            _decisionMaking = Clamp(decisionMaking);
        }

        /// <summary>
        /// Creates a balanced default set of abilities with all attributes set to baseline (50).
        /// </summary>
        public static PlayerAbilities Default => CreateUniform(DefaultBaseline);

        /// <summary>
        /// Creates a set of abilities where all 15 attributes are set to the given value (clamped to [0, 100]).
        /// </summary>
        public static PlayerAbilities CreateUniform(int uniformValue)
        {
            byte val = Clamp(uniformValue);
            return new PlayerAbilities(
                val, val, val, val, val,
                val, val, val, val, val, val,
                val, val, val, val);
        }

        /// <summary>
        /// Clamps an integer value to the valid [0, 100] attribute range.
        /// </summary>
        public static byte Clamp(int value)
        {
            if (value < MinValue) return MinValue;
            if (value > MaxValue) return MaxValue;
            return (byte)value;
        }

        /// <summary>
        /// Calculates the unweighted arithmetic mean across all 15 attributes.
        /// </summary>
        public float CalculateAverage()
        {
            int sum = _pace + _acceleration + _stamina + _strength + _agility +
                      _passing + _shooting + _dribbling + _crossing + _firstTouch + _tackling +
                      _vision + _composure + _positioning + _decisionMaking;
            return sum / 15f;
        }

        /// <summary>
        /// Returns the float value of a specific attribute by name.
        /// Used by FatigueSystem and MatchSimulation for attribute-level calculations.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown attribute names.</exception>
        public float Get(AttributeName attribute) => attribute switch
        {
            AttributeName.Pace          => _pace,
            AttributeName.Acceleration  => _acceleration,
            AttributeName.Stamina       => _stamina,
            AttributeName.Strength      => _strength,
            AttributeName.Agility       => _agility,
            AttributeName.Passing       => _passing,
            AttributeName.Shooting      => _shooting,
            AttributeName.Dribbling     => _dribbling,
            AttributeName.Crossing      => _crossing,
            AttributeName.FirstTouch    => _firstTouch,
            AttributeName.Tackling      => _tackling,
            AttributeName.Vision        => _vision,
            AttributeName.Composure     => _composure,
            AttributeName.Positioning   => _positioning,
            AttributeName.DecisionMaking => _decisionMaking,
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), $"Unknown attribute: {attribute}")
        };
    }
}
