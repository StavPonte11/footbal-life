using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Distinct tiers of football competition in the domestic pyramid (#P5-003).
    /// </summary>
    public enum LeagueTier
    {
        Tier1_Premier = 1,
        Tier2_Championship = 2,
        Tier3_LeagueOne = 3,
        Tier4_LeagueTwo = 4
    }

    /// <summary>
    /// Financial parameters, prestige, and promotion/relegation thresholds for a competition tier (#P5-003).
    /// </summary>
    public sealed record LeagueTierProfile
    {
        public LeagueTier Tier { get; init; }
        public string TierName { get; init; } = string.Empty;
        public int PrestigeRating { get; init; } // 1-100 baseline prestige
        public decimal MinWeeklyWage { get; init; }
        public decimal AverageWeeklyWage { get; init; }
        public decimal MaxWeeklyWage { get; init; }
        public int PromotionSpots { get; init; }
        public int RelegationSpots { get; init; }
        public int ContinentalQualificationSpots { get; init; }
        public string TrophyName { get; init; } = string.Empty;
    }

    /// <summary>
    /// Configuration and lookup registry for multi-tier league systems (#P5-003).
    /// </summary>
    public static class LeagueTierConfig
    {
        private static readonly Dictionary<LeagueTier, LeagueTierProfile> s_profiles = new()
        {
            [LeagueTier.Tier1_Premier] = new LeagueTierProfile
            {
                Tier = LeagueTier.Tier1_Premier,
                TierName = "Premier Division",
                PrestigeRating = 88,
                MinWeeklyWage = 15000m,
                AverageWeeklyWage = 70000m,
                MaxWeeklyWage = 350000m,
                PromotionSpots = 0,
                RelegationSpots = 3,
                ContinentalQualificationSpots = 4,
                TrophyName = "Premier League Trophy"
            },
            [LeagueTier.Tier2_Championship] = new LeagueTierProfile
            {
                Tier = LeagueTier.Tier2_Championship,
                TierName = "Championship Division",
                PrestigeRating = 65,
                MinWeeklyWage = 4000m,
                AverageWeeklyWage = 18000m,
                MaxWeeklyWage = 65000m,
                PromotionSpots = 3,
                RelegationSpots = 3,
                ContinentalQualificationSpots = 0,
                TrophyName = "Championship Shield"
            },
            [LeagueTier.Tier3_LeagueOne] = new LeagueTierProfile
            {
                Tier = LeagueTier.Tier3_LeagueOne,
                TierName = "League One",
                PrestigeRating = 48,
                MinWeeklyWage = 1200m,
                AverageWeeklyWage = 4500m,
                MaxWeeklyWage = 15000m,
                PromotionSpots = 3,
                RelegationSpots = 4,
                ContinentalQualificationSpots = 0,
                TrophyName = "League One Cup"
            },
            [LeagueTier.Tier4_LeagueTwo] = new LeagueTierProfile
            {
                Tier = LeagueTier.Tier4_LeagueTwo,
                TierName = "League Two",
                PrestigeRating = 36,
                MinWeeklyWage = 500m,
                AverageWeeklyWage = 1800m,
                MaxWeeklyWage = 5500m,
                PromotionSpots = 4,
                RelegationSpots = 2,
                ContinentalQualificationSpots = 0,
                TrophyName = "League Two Trophy"
            }
        };

        public static LeagueTierProfile GetProfile(LeagueTier tier)
        {
            if (s_profiles.TryGetValue(tier, out var profile))
            {
                return profile;
            }

            return s_profiles[LeagueTier.Tier4_LeagueTwo];
        }

        public static LeagueTierProfile GetProfile(int tierNumber)
        {
            var tierEnum = tierNumber switch
            {
                1 => LeagueTier.Tier1_Premier,
                2 => LeagueTier.Tier2_Championship,
                3 => LeagueTier.Tier3_LeagueOne,
                _ => LeagueTier.Tier4_LeagueTwo
            };

            return GetProfile(tierEnum);
        }

        public static IReadOnlyList<LeagueTierProfile> AllProfiles => s_profiles.Values.ToList();
    }
}
