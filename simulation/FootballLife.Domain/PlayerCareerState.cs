using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the contextual career and contractual state of a player within a club.
    /// Tracks squad role, manager trust, salary, market valuation, and public reputation.
    /// </summary>
    public sealed record PlayerCareerState
    {
        /// <summary>Minimum permissible manager trust value.</summary>
        public const float MinTrust = 0f;

        /// <summary>Maximum permissible manager trust value.</summary>
        public const float MaxTrust = 100f;

        /// <summary>Minimum permissible reputation value.</summary>
        public const float MinReputation = 0f;

        /// <summary>Maximum permissible reputation value.</summary>
        public const float MaxReputation = 100f;

        private readonly float _managerTrust;
        private readonly float _reputation;
        private readonly decimal _weeklySalary;
        private readonly decimal _marketValue;

        /// <summary>
        /// Identifier of the club the player is currently signed with.
        /// </summary>
        public Guid ClubId { get; init; }

        /// <summary>
        /// Hierarchy status within the team squad.
        /// </summary>
        public SquadStatus Status { get; init; }

        /// <summary>
        /// Degree of manager confidence and playing priority [0f, 100f].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set outside [0f, 100f].</exception>
        public float ManagerTrust
        {
            get => _managerTrust;
            init
            {
                if (float.IsNaN(value) || value < MinTrust || value > MaxTrust)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Manager trust must be bounded within [{MinTrust}, {MaxTrust}]. Actual: {value}");
                }
                _managerTrust = value;
            }
        }

        /// <summary>
        /// Weekly wage earnings in financial currency (>= 0).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative value.</exception>
        public decimal WeeklySalary
        {
            get => _weeklySalary;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Weekly salary cannot be negative.");
                }
                _weeklySalary = value;
            }
        }

        /// <summary>
        /// Estimated financial market value for transfer negotiations (>= 0).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative value.</exception>
        public decimal MarketValue
        {
            get => _marketValue;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Market value cannot be negative.");
                }
                _marketValue = value;
            }
        }

        /// <summary>
        /// Overall football world reputation and fame [0f, 100f].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set outside [0f, 100f].</exception>
        public float Reputation
        {
            get => _reputation;
            init
            {
                if (float.IsNaN(value) || value < MinReputation || value > MaxReputation)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Reputation must be bounded within [{MinReputation}, {MaxReputation}]. Actual: {value}");
                }
                _reputation = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCareerState"/> record with strict invariant enforcement.
        /// </summary>
        /// <param name="clubId">The unique ID of the club.</param>
        /// <param name="status">The role within the squad.</param>
        /// <param name="managerTrust">Manager confidence [0, 100].</param>
        /// <param name="weeklySalary">Weekly wage earnings (&gt;= 0).</param>
        /// <param name="marketValue">Market valuation (&gt;= 0).</param>
        /// <param name="reputation">Public reputation [0, 100].</param>
        public PlayerCareerState(
            Guid clubId,
            SquadStatus status,
            float managerTrust,
            decimal weeklySalary,
            decimal marketValue,
            float reputation)
        {
            if (weeklySalary < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklySalary), "Weekly salary cannot be negative.");
            }

            if (marketValue < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(marketValue), "Market value cannot be negative.");
            }

            if (float.IsNaN(managerTrust) || managerTrust < MinTrust || managerTrust > MaxTrust)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(managerTrust),
                    $"Manager trust must be bounded within [{MinTrust}, {MaxTrust}]. Actual: {managerTrust}");
            }

            if (float.IsNaN(reputation) || reputation < MinReputation || reputation > MaxReputation)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reputation),
                    $"Reputation must be bounded within [{MinReputation}, {MaxReputation}]. Actual: {reputation}");
            }

            ClubId = clubId;
            Status = status;
            _managerTrust = managerTrust;
            _weeklySalary = weeklySalary;
            _marketValue = marketValue;
            _reputation = reputation;
        }

        /// <summary>
        /// Factory method to initialize an academy prospect state.
        /// </summary>
        /// <param name="clubId">The club ID.</param>
        /// <param name="weeklySalary">Optional initial salary, defaults to 150.</param>
        /// <param name="marketValue">Optional initial market value, defaults to 25,000.</param>
        public static PlayerCareerState CreateAcademy(Guid clubId, decimal weeklySalary = 150m, decimal marketValue = 25000m)
        {
            return new PlayerCareerState(
                clubId,
                SquadStatus.Academy,
                managerTrust: 30f,
                weeklySalary: weeklySalary,
                marketValue: marketValue,
                reputation: 10f);
        }

        /// <summary>
        /// Factory method that creates a <see cref="PlayerCareerState"/> by clamping input values into valid ranges.
        /// </summary>
        /// <param name="clubId">The club ID.</param>
        /// <param name="status">The squad status.</param>
        /// <param name="managerTrust">Raw trust score, clamped to [0, 100].</param>
        /// <param name="weeklySalary">Raw salary, clamped to >= 0.</param>
        /// <param name="marketValue">Raw market value, clamped to >= 0.</param>
        /// <param name="reputation">Raw reputation, clamped to [0, 100].</param>
        public static PlayerCareerState CreateClamped(
            Guid clubId,
            SquadStatus status,
            float managerTrust,
            decimal weeklySalary,
            decimal marketValue,
            float reputation)
        {
            return new PlayerCareerState(
                clubId,
                status,
                managerTrust: ClampTrust(managerTrust),
                weeklySalary: Math.Max(0m, weeklySalary),
                marketValue: Math.Max(0m, marketValue),
                reputation: ClampReputation(reputation));
        }

        /// <summary>
        /// Clamps a trust value into the valid [0, 100] range.
        /// </summary>
        /// <param name="trust">Raw trust value.</param>
        /// <returns>Clamped value between 0 and 100.</returns>
        public static float ClampTrust(float trust)
        {
            if (float.IsNaN(trust)) return MinTrust;
            if (trust < MinTrust) return MinTrust;
            if (trust > MaxTrust) return MaxTrust;
            return trust;
        }

        /// <summary>
        /// Clamps a reputation value into the valid [0, 100] range.
        /// </summary>
        /// <param name="reputation">Raw reputation value.</param>
        /// <returns>Clamped value between 0 and 100.</returns>
        public static float ClampReputation(float reputation)
        {
            if (float.IsNaN(reputation)) return MinReputation;
            if (reputation < MinReputation) return MinReputation;
            if (reputation > MaxReputation) return MaxReputation;
            return reputation;
        }
    }
}
