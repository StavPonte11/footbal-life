using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the legally binding employment contract between a player and a football club.
    /// Dictates guaranteed weekly wage, contractual role, performance incentives, and duration.
    /// </summary>
    public sealed record Contract
    {
        private readonly decimal _weeklySalary;

        /// <summary>
        /// Unique persistent identifier of this contract agreement.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Player bound by this contract.
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Employing club entity.
        /// </summary>
        public Guid ClubId { get; init; }

        /// <summary>
        /// Guaranteed base weekly wage (>= 0).
        /// </summary>
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
        /// Contract start date.
        /// </summary>
        public DateOnly StartDate { get; init; }

        /// <summary>
        /// Contract expiration date (must be strictly after StartDate).
        /// </summary>
        public DateOnly EndDate { get; init; }

        /// <summary>
        /// Agreed role in the first team hierarchy.
        /// </summary>
        public SquadRole ContractRole { get; init; }

        /// <summary>
        /// Performance incentives and appearance bonuses.
        /// </summary>
        public ContractBonuses Bonuses { get; init; }

        public Contract(
            Guid id,
            Guid playerId,
            Guid clubId,
            decimal weeklySalary,
            DateOnly startDate,
            DateOnly endDate,
            SquadRole contractRole,
            ContractBonuses? bonuses = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be an empty Guid.", nameof(id));
            }

            if (playerId == Guid.Empty)
            {
                throw new ArgumentException("Player ID cannot be an empty Guid.", nameof(playerId));
            }

            if (clubId == Guid.Empty)
            {
                throw new ArgumentException("Club ID cannot be an empty Guid.", nameof(clubId));
            }

            if (weeklySalary < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklySalary), "Weekly salary cannot be negative.");
            }

            if (endDate <= startDate)
            {
                throw new ArgumentException(
                    $"Contract EndDate ({endDate:yyyy-MM-dd}) must be strictly after StartDate ({startDate:yyyy-MM-dd}).",
                    nameof(endDate));
            }

            Id = id;
            PlayerId = playerId;
            ClubId = clubId;
            _weeklySalary = weeklySalary;
            StartDate = startDate;
            EndDate = endDate;
            ContractRole = contractRole;
            Bonuses = bonuses ?? ContractBonuses.Zero;
        }

        /// <summary>
        /// Factory method generating a new Contract with a randomly assigned GUID.
        /// </summary>
        public static Contract Create(
            Guid playerId,
            Guid clubId,
            decimal weeklySalary,
            DateOnly startDate,
            DateOnly endDate,
            SquadRole contractRole,
            ContractBonuses? bonuses = null)
        {
            return new Contract(
                Guid.NewGuid(),
                playerId,
                clubId,
                weeklySalary,
                startDate,
                endDate,
                contractRole,
                bonuses);
        }

        /// <summary>
        /// Evaluates whether the contract has elapsed relative to a calendar date.
        /// </summary>
        public bool IsExpired(DateOnly currentDate) => currentDate >= EndDate;

        /// <summary>
        /// Computes remaining duration in full calendar days. Returns 0 if already expired.
        /// </summary>
        public int RemainingDays(DateOnly currentDate)
        {
            if (currentDate >= EndDate) return 0;
            return EndDate.DayNumber - currentDate.DayNumber;
        }

        /// <summary>
        /// Computes remaining duration in full calendar weeks.
        /// </summary>
        public int RemainingWeeks(DateOnly currentDate)
        {
            int days = RemainingDays(currentDate);
            return days / 7;
        }

        /// <summary>
        /// Calculates total match payout including weekly salary portion and performance bonuses.
        /// </summary>
        public decimal CalculateMatchEarnings(int appearances, int goals = 0, int assists = 0, bool cleanSheet = false)
        {
            return Bonuses.CalculateTotal(appearances, goals, assists, cleanSheet);
        }
    }
}
