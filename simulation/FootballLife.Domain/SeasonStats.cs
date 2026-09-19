using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Aggregated statistical record for a footballer's performance over an entire season.
    /// Used for career tracking, contract negotiations, and simulation balance validation.
    /// </summary>
    public sealed record SeasonStats
    {
        public int Appearances { get; init; }
        public int Goals { get; init; }
        public int Assists { get; init; }
        public float AverageRating { get; init; }
        public int YellowCards { get; init; }
        public int RedCards { get; init; }
        public decimal WageEarned { get; init; }
        public SquadStatus FinalStatus { get; init; }
        public float AttributeGrowthAverage { get; init; }

        public SeasonStats(
            int appearances,
            int goals,
            int assists,
            float averageRating,
            int yellowCards,
            int redCards,
            decimal wageEarned,
            SquadStatus finalStatus,
            float attributeGrowthAverage)
        {
            if (appearances < 0)
                throw new ArgumentOutOfRangeException(nameof(appearances), "Appearances cannot be negative.");
            if (goals < 0)
                throw new ArgumentOutOfRangeException(nameof(goals), "Goals cannot be negative.");
            if (assists < 0)
                throw new ArgumentOutOfRangeException(nameof(assists), "Assists cannot be negative.");
            if (float.IsNaN(averageRating) || averageRating < 0f || averageRating > 10f)
                throw new ArgumentOutOfRangeException(nameof(averageRating), $"Average rating must be in [0, 10]. Actual: {averageRating}");
            if (yellowCards < 0)
                throw new ArgumentOutOfRangeException(nameof(yellowCards), "Yellow cards cannot be negative.");
            if (redCards < 0)
                throw new ArgumentOutOfRangeException(nameof(redCards), "Red cards cannot be negative.");
            if (wageEarned < 0m)
                throw new ArgumentOutOfRangeException(nameof(wageEarned), "Wage earned cannot be negative.");

            Appearances = appearances;
            Goals = goals;
            Assists = assists;
            AverageRating = averageRating;
            YellowCards = yellowCards;
            RedCards = redCards;
            WageEarned = wageEarned;
            FinalStatus = finalStatus;
            AttributeGrowthAverage = attributeGrowthAverage;
        }
    }
}
