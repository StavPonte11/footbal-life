using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Commercial sponsorship tier based on player reputation and commercial reach.
    /// </summary>
    public enum SponsorshipTier
    {
        Local = 1,
        Regional = 2,
        National = 3,
        Global = 4
    }

    /// <summary>
    /// Type of sponsorship endorsement product/category.
    /// </summary>
    public enum SponsorshipType
    {
        Boots = 1,
        Apparel = 2,
        Beverage = 3,
        Luxury = 4,
        Tech = 5
    }

    /// <summary>
    /// Pure C# domain model for a commercial sponsorship endorsement contract offer.
    /// </summary>
    public sealed record SponsorshipDeal
    {
        public string Id { get; init; }
        public string BrandName { get; init; }
        public SponsorshipTier Tier { get; init; }
        public SponsorshipType Type { get; init; }
        public int RequiredReputation { get; init; }
        public int WeeklyPayout { get; init; }
        public int SigningBonus { get; init; }
        public int DurationWeeks { get; init; }
        public int EnergyRecoveryBonus { get; init; }
        public int WeeklyFameBonus { get; init; }
        public string Description { get; init; }

        public SponsorshipDeal(
            string id,
            string brandName,
            SponsorshipTier tier,
            SponsorshipType type,
            int requiredReputation,
            int weeklyPayout,
            int signingBonus,
            int durationWeeks,
            int energyRecoveryBonus = 0,
            int weeklyFameBonus = 1,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(brandName)) throw new ArgumentException("BrandName cannot be empty.", nameof(brandName));
            if (requiredReputation < 0) throw new ArgumentOutOfRangeException(nameof(requiredReputation));
            if (weeklyPayout < 0) throw new ArgumentOutOfRangeException(nameof(weeklyPayout));
            if (signingBonus < 0) throw new ArgumentOutOfRangeException(nameof(signingBonus));
            if (durationWeeks <= 0) throw new ArgumentOutOfRangeException(nameof(durationWeeks));

            Id = id;
            BrandName = brandName;
            Tier = tier;
            Type = type;
            RequiredReputation = requiredReputation;
            WeeklyPayout = weeklyPayout;
            SigningBonus = signingBonus;
            DurationWeeks = durationWeeks;
            EnergyRecoveryBonus = Math.Max(0, energyRecoveryBonus);
            WeeklyFameBonus = Math.Max(0, weeklyFameBonus);
            Description = description ?? $"{brandName} {type} endorsement ({tier} tier).";
        }
    }

    /// <summary>
    /// Represents an active ongoing sponsorship contract held by a footballer.
    /// </summary>
    public sealed record ActiveSponsorship
    {
        public SponsorshipDeal Deal { get; init; }
        public int WeeksRemaining { get; init; }
        public int TotalEarned { get; init; }

        public bool IsExpired => WeeksRemaining <= 0;

        public ActiveSponsorship(SponsorshipDeal deal, int weeksRemaining, int totalEarned = 0)
        {
            Deal = deal ?? throw new ArgumentNullException(nameof(deal));
            WeeksRemaining = Math.Max(0, weeksRemaining);
            TotalEarned = Math.Max(0, totalEarned);
        }

        public ActiveSponsorship TickWeek()
        {
            int nextRemaining = Math.Max(0, WeeksRemaining - 1);
            int nextEarned = TotalEarned + Deal.WeeklyPayout;
            return new ActiveSponsorship(Deal, nextRemaining, nextEarned);
        }
    }
}
