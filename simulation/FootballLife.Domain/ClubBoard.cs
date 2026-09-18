using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Describes the board and ownership posture towards results, manager tenure, and investment.
    /// </summary>
    public enum BoardPatience
    {
        Impatient = 0,
        Balanced = 1,
        Patient = 2
    }

    /// <summary>
    /// Represents the governing hierarchy, owner persona, and strategic ambition of a football club.
    /// </summary>
    public sealed record ClubBoard
    {
        public const int MinAmbition = 1;
        public const int MaxAmbition = 5;

        private readonly int _financialAmbition;

        /// <summary>
        /// Name of the primary owner, ownership group, or president.
        /// </summary>
        public string OwnerName { get; init; }

        /// <summary>
        /// Degree of tolerance towards manager and squad slumps.
        /// </summary>
        public BoardPatience Patience { get; init; }

        /// <summary>
        /// Willingness to inject external capital and fund squad growth [1, 5].
        /// </summary>
        public int FinancialAmbition
        {
            get => _financialAmbition;
            init
            {
                if (value < MinAmbition || value > MaxAmbition)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Financial ambition must be in [{MinAmbition}, {MaxAmbition}]. Actual: {value}");
                }
                _financialAmbition = value;
            }
        }

        public ClubBoard(string ownerName, BoardPatience patience, int financialAmbition)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
            {
                throw new ArgumentException("Owner name cannot be null or whitespace.", nameof(ownerName));
            }

            if (financialAmbition < MinAmbition || financialAmbition > MaxAmbition)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(financialAmbition),
                    $"Financial ambition must be in [{MinAmbition}, {MaxAmbition}]. Actual: {financialAmbition}");
            }

            OwnerName = ownerName.Trim();
            Patience = patience;
            _financialAmbition = financialAmbition;
        }

        public static ClubBoard Default(string clubName) =>
            new ClubBoard($"{clubName} Board", BoardPatience.Balanced, 3);
    }
}
