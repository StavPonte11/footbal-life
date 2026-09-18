using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the fan demographic profile, expectations, and loyalty dynamics of a club.
    /// Drives match day atmosphere, media pressure on poor form, and commercial merchandise revenue.
    /// </summary>
    public sealed record ClubFanbase
    {
        public const int MinRating = 1;
        public const int MaxRating = 100;

        private readonly int _loyaltyRating;
        private readonly int _expectationRating;

        /// <summary>
        /// Global registered supporter base estimate.
        /// </summary>
        public int SupporterCount { get; init; }

        /// <summary>
        /// Degree of steadfast fan support during slumps [1, 100].
        /// </summary>
        public int LoyaltyRating
        {
            get => _loyaltyRating;
            init
            {
                if (value < MinRating || value > MaxRating)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Loyalty rating must be in [{MinRating}, {MaxRating}]. Actual: {value}");
                }
                _loyaltyRating = value;
            }
        }

        /// <summary>
        /// Degree of performance demand and scrutiny placed on players and manager [1, 100].
        /// Higher expectations increase pressure and reduce patience on poor match ratings.
        /// </summary>
        public int ExpectationRating
        {
            get => _expectationRating;
            init
            {
                if (value < MinRating || value > MaxRating)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Expectation rating must be in [{MinRating}, {MaxRating}]. Actual: {value}");
                }
                _expectationRating = value;
            }
        }

        public ClubFanbase(int supporterCount, int loyaltyRating, int expectationRating)
        {
            if (supporterCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(supporterCount), "Supporter count cannot be negative.");
            }

            if (loyaltyRating < MinRating || loyaltyRating > MaxRating)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(loyaltyRating),
                    $"Loyalty rating must be in [{MinRating}, {MaxRating}]. Actual: {loyaltyRating}");
            }

            if (expectationRating < MinRating || expectationRating > MaxRating)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(expectationRating),
                    $"Expectation rating must be in [{MinRating}, {MaxRating}]. Actual: {expectationRating}");
            }

            SupporterCount = supporterCount;
            _loyaltyRating = loyaltyRating;
            _expectationRating = expectationRating;
        }

        public static ClubFanbase Default(int reputationRating)
        {
            int baseFans = reputationRating * 25_000;
            return new ClubFanbase(
                supporterCount: Math.Max(5_000, baseFans),
                loyaltyRating: 70,
                expectationRating: Math.Max(20, reputationRating));
        }
    }
}
