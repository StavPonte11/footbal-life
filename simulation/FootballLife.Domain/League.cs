using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an organized domestic football division with tiered hierarchy,
    /// fixture scheduling rules, and promotion/relegation thresholds.
    /// </summary>
    public sealed record League
    {
        public const int MinTier = 1;
        public const int MaxTier = 5;

        private readonly int _tier;
        private readonly int _clubCount;
        private readonly int _matchdaysPerSeason;
        private readonly int _promotionSlots;
        private readonly int _relegationSlots;

        /// <summary>
        /// Unique persistent identifier of the league.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Full display name of the league (e.g., "Premier League", "La Liga").
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Three-letter ISO alpha-3 country code (e.g., "ENG", "ESP", "DEU", "ITA", "FRA").
        /// </summary>
        public string CountryCode { get; init; }

        /// <summary>
        /// Tier depth in the national pyramid [1, 5] (1 = top flight).
        /// </summary>
        public int Tier
        {
            get => _tier;
            init
            {
                if (value < MinTier || value > MaxTier)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"League tier must be in [{MinTier}, {MaxTier}]. Actual: {value}");
                }
                _tier = value;
            }
        }

        /// <summary>
        /// Total number of participating clubs in this division (&gt;= 2).
        /// </summary>
        public int ClubCount
        {
            get => _clubCount;
            init
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "League must contain at least 2 clubs.");
                }
                _clubCount = value;
            }
        }

        /// <summary>
        /// Total scheduled matchdays in a full regular season calendar (&gt;= 1).
        /// </summary>
        public int MatchdaysPerSeason
        {
            get => _matchdaysPerSeason;
            init
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Matchdays per season must be positive.");
                }
                _matchdaysPerSeason = value;
            }
        }

        /// <summary>
        /// Number of top table positions granting automatic promotion to the tier above.
        /// </summary>
        public int PromotionSlots
        {
            get => _promotionSlots;
            init
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Promotion slots cannot be negative.");
                }
                _promotionSlots = value;
            }
        }

        /// <summary>
        /// Number of bottom table positions resulting in relegation to the tier below.
        /// </summary>
        public int RelegationSlots
        {
            get => _relegationSlots;
            init
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Relegation slots cannot be negative.");
                }
                _relegationSlots = value;
            }
        }

        public League(
            Guid id,
            string name,
            string countryCode,
            int tier,
            int clubCount,
            int matchdaysPerSeason,
            int promotionSlots = 0,
            int relegationSlots = 3)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("League ID cannot be an empty Guid.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("League name cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException("Country code cannot be null or whitespace.", nameof(countryCode));
            }

            if (tier < MinTier || tier > MaxTier)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tier),
                    $"Tier must be bounded within [{MinTier}, {MaxTier}]. Actual: {tier}");
            }

            if (clubCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(clubCount), "League must contain at least 2 clubs.");
            }

            if (matchdaysPerSeason < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(matchdaysPerSeason), "Matchdays per season must be positive.");
            }

            if (promotionSlots < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(promotionSlots), "Promotion slots cannot be negative.");
            }

            if (relegationSlots < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relegationSlots), "Relegation slots cannot be negative.");
            }

            if (promotionSlots + relegationSlots >= clubCount)
            {
                throw new ArgumentException("Combined promotion and relegation slots cannot equal or exceed total club count.");
            }

            Id = id;
            Name = name.Trim();
            CountryCode = countryCode.Trim().ToUpperInvariant();
            _tier = tier;
            _clubCount = clubCount;
            _matchdaysPerSeason = matchdaysPerSeason;
            _promotionSlots = promotionSlots;
            _relegationSlots = relegationSlots;
        }

        /// <summary>
        /// Factory method to instantiate a new League with a randomly assigned GUID.
        /// </summary>
        public static League Create(
            string name,
            string countryCode,
            int tier,
            int clubCount = 20,
            int matchdaysPerSeason = 38,
            int promotionSlots = 0,
            int relegationSlots = 3)
        {
            return new League(
                Guid.NewGuid(),
                name,
                countryCode,
                tier,
                clubCount,
                matchdaysPerSeason,
                promotionSlots,
                relegationSlots);
        }
    }
}
