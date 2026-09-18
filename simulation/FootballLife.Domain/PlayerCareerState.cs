using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the contextual career and contractual state of a player within a club.
    /// Tracks squad role, manager trust, salary, market valuation, and public reputation.
    /// </summary>
    public sealed record PlayerCareerState
    {
        public const float MinTrust = 0f;
        public const float MaxTrust = 100f;
        public const float MinReputation = 0f;
        public const float MaxReputation = 100f;

        private readonly float _managerTrust;
        private readonly float _reputation;

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
        public float ManagerTrust
        {
            get => _managerTrust;
            init => _managerTrust = ClampPercent(value);
        }

        /// <summary>
        /// Weekly wage earnings in financial currency (>= 0).
        /// </summary>
        public decimal WeeklySalary { get; init; }

        /// <summary>
        /// Estimated financial market value for transfer negotiations (>= 0).
        /// </summary>
        public decimal MarketValue { get; init; }

        /// <summary>
        /// Overall football world reputation and fame [0f, 100f].
        /// </summary>
        public float Reputation
        {
            get => _reputation;
            init => _reputation = ClampPercent(value);
        }

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

            ClubId = clubId;
            Status = status;
            _managerTrust = ClampPercent(managerTrust);
            WeeklySalary = weeklySalary;
            MarketValue = marketValue;
            _reputation = ClampPercent(reputation);
        }

        /// <summary>
        /// Factory method to initialize an academy prospect state.
        /// </summary>
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

        private static float ClampPercent(float value)
        {
            if (float.IsNaN(value)) return 0f;
            if (value < 0f) return 0f;
            if (value > 100f) return 100f;
            return value;
        }
    }
}
