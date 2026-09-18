using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the immutable financial status and operating budgets of a football club.
    /// </summary>
    public sealed record ClubFinances
    {
        private readonly decimal _weeklyWageBudget;
        private readonly decimal _transferBudget;

        /// <summary>
        /// Total weekly financial budget allocated for player and staff wages (&gt;= 0).
        /// </summary>
        public decimal WeeklyWageBudget
        {
            get => _weeklyWageBudget;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Weekly wage budget cannot be negative.");
                }
                _weeklyWageBudget = value;
            }
        }

        /// <summary>
        /// Total funds available for player transfer fees (&gt;= 0).
        /// </summary>
        public decimal TransferBudget
        {
            get => _transferBudget;
            init
            {
                if (value < 0m)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Transfer budget cannot be negative.");
                }
                _transferBudget = value;
            }
        }

        public ClubFinances(decimal weeklyWageBudget, decimal transferBudget)
        {
            if (weeklyWageBudget < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklyWageBudget), "Weekly wage budget cannot be negative.");
            }

            if (transferBudget < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(transferBudget), "Transfer budget cannot be negative.");
            }

            _weeklyWageBudget = weeklyWageBudget;
            _transferBudget = transferBudget;
        }

        /// <summary>
        /// Checks if the club can afford a proposed weekly wage expense.
        /// </summary>
        public bool CanAffordWage(decimal wage) => wage >= 0m && wage <= _weeklyWageBudget;

        /// <summary>
        /// Checks if the club can afford a proposed transfer fee expenditure.
        /// </summary>
        public bool CanAffordTransfer(decimal transferFee) => transferFee >= 0m && transferFee <= _transferBudget;
    }
}
