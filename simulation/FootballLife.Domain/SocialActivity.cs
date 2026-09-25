using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Category of social outings and night life activities (#P4-008).
    /// </summary>
    public enum SocialActivityCategory
    {
        Casual,
        TeamBonding,
        Nightlife,
        Glamour,
        Philanthropy
    }

    /// <summary>
    /// Pure C# domain model representing a social outing or off-pitch lifestyle activity (#P4-008).
    /// Consumes energy and money in exchange for morale boosts, relational bonding, and public fame,
    /// with potential risk of manager disapproval if conducted close to matchday.
    /// </summary>
    public sealed record SocialActivity
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public SocialActivityCategory Category { get; init; }
        public int EnergyCost { get; init; }
        public decimal FinancialCost { get; init; }
        public float MoraleBoost { get; init; }
        public float TeamAffinityBoost { get; init; }
        public int PrestigeBoost { get; init; }
        public float ManagerTrustRisk { get; init; } // 0.0 to 1.0 probability of manager penalty if near matchday
        public string Description { get; init; } = string.Empty;
        public string IconEmoji { get; init; } = "🎉";
    }

    /// <summary>
    /// Catalog of curated social outings and leisure activities (#P4-008).
    /// </summary>
    public static class SocialActivityCatalog
    {
        private static readonly List<SocialActivity> s_activities = new()
        {
            // Casual
            new SocialActivity
            {
                Id = "act_coffee_stroll",
                Name = "Specialty Coffee Stroll",
                Category = SocialActivityCategory.Casual,
                EnergyCost = 8,
                FinancialCost = 25m,
                MoraleBoost = 5f,
                TeamAffinityBoost = 2f,
                PrestigeBoost = 5,
                ManagerTrustRisk = 0.0f,
                Description = "A relaxing walk through the city center with an artisan flat white and fresh pastries.",
                IconEmoji = "☕"
            },
            new SocialActivity
            {
                Id = "act_italian_bistro",
                Name = "Cozy Italian Bistro",
                Category = SocialActivityCategory.Casual,
                EnergyCost = 14,
                FinancialCost = 85m,
                MoraleBoost = 8f,
                TeamAffinityBoost = 4f,
                PrestigeBoost = 10,
                ManagerTrustRisk = 0.0f,
                Description = "Authentic wood-fired pizza and handmade pasta with close friends away from the spotlight.",
                IconEmoji = "🍝"
            },
            new SocialActivity
            {
                Id = "act_cinema_night",
                Name = "IMAX Cinema Premiere",
                Category = SocialActivityCategory.Casual,
                EnergyCost = 12,
                FinancialCost = 60m,
                MoraleBoost = 7f,
                TeamAffinityBoost = 3f,
                PrestigeBoost = 8,
                ManagerTrustRisk = 0.05f,
                Description = "Catching the latest blockbuster in luxury reclining leather seats with popcorn.",
                IconEmoji = "🍿"
            },

            // Team Bonding
            new SocialActivity
            {
                Id = "act_team_bowling",
                Name = "Squad Bowling Tournament",
                Category = SocialActivityCategory.TeamBonding,
                EnergyCost = 20,
                FinancialCost = 120m,
                MoraleBoost = 12f,
                TeamAffinityBoost = 14f,
                PrestigeBoost = 15,
                ManagerTrustRisk = 0.05f,
                Description = "High-energy bowling games, trash talk, and team banter with teammates.",
                IconEmoji = "🎳"
            },
            new SocialActivity
            {
                Id = "act_gaming_lan",
                Name = "Esports & Gaming Night",
                Category = SocialActivityCategory.TeamBonding,
                EnergyCost = 18,
                FinancialCost = 90m,
                MoraleBoost = 11f,
                TeamAffinityBoost = 12f,
                PrestigeBoost = 12,
                ManagerTrustRisk = 0.05f,
                Description = "Battle Royale tournaments and FIFA grudge matches in a private gaming lounge.",
                IconEmoji = "🎮"
            },
            new SocialActivity
            {
                Id = "act_team_barbecue",
                Name = "Captains' Backyard BBQ",
                Category = SocialActivityCategory.TeamBonding,
                EnergyCost = 22,
                FinancialCost = 250m,
                MoraleBoost = 15f,
                TeamAffinityBoost = 18f,
                PrestigeBoost = 20,
                ManagerTrustRisk = 0.05f,
                Description = "Grilled steaks, music, and heart-to-heart locker room bonding at the captain's residence.",
                IconEmoji = "🥩"
            },

            // Nightlife
            new SocialActivity
            {
                Id = "act_cocktail_rooftop",
                Name = "Skyline Cocktail Lounge",
                Category = SocialActivityCategory.Nightlife,
                EnergyCost = 25,
                FinancialCost = 350m,
                MoraleBoost = 14f,
                TeamAffinityBoost = 8f,
                PrestigeBoost = 25,
                ManagerTrustRisk = 0.25f,
                Description = "Scenic panoramic city views, live acoustic sets, and artisan mocktails & cocktails.",
                IconEmoji = "🍸"
            },
            new SocialActivity
            {
                Id = "act_vip_nightclub",
                Name = "Exclusive VIP Club",
                Category = SocialActivityCategory.Nightlife,
                EnergyCost = 38,
                FinancialCost = 1200m,
                MoraleBoost = 20f,
                TeamAffinityBoost = 10f,
                PrestigeBoost = 40,
                ManagerTrustRisk = 0.45f,
                Description = "Pumping DJ beats, sparklers, and champagne bottle service until late in the morning.",
                IconEmoji = "🍾"
            },

            // Glamour
            new SocialActivity
            {
                Id = "act_fashion_week",
                Name = "Fashion Week Front Row",
                Category = SocialActivityCategory.Glamour,
                EnergyCost = 24,
                FinancialCost = 1800m,
                MoraleBoost = 16f,
                TeamAffinityBoost = 4f,
                PrestigeBoost = 50,
                ManagerTrustRisk = 0.15f,
                Description = "High-fashion catwalk presentation, designer suits, and red carpet interviews.",
                IconEmoji = "✨"
            },
            new SocialActivity
            {
                Id = "act_michelin_dinner",
                Name = "3-Star Michelin Tasting",
                Category = SocialActivityCategory.Glamour,
                EnergyCost = 16,
                FinancialCost = 950m,
                MoraleBoost = 15f,
                TeamAffinityBoost = 6f,
                PrestigeBoost = 35,
                ManagerTrustRisk = 0.05f,
                Description = "A magnificent 12-course culinary tasting menu crafted by a world-renowned chef.",
                IconEmoji = "🍷"
            },

            // Philanthropy
            new SocialActivity
            {
                Id = "act_academy_visit",
                Name = "Youth Academy Mentorship",
                Category = SocialActivityCategory.Philanthropy,
                EnergyCost = 18,
                FinancialCost = 100m,
                MoraleBoost = 14f,
                TeamAffinityBoost = 8f,
                PrestigeBoost = 30,
                ManagerTrustRisk = 0.0f,
                Description = "Drills, penalty practice, and motivational chats with young aspiring footballers.",
                IconEmoji = "⚽"
            },
            new SocialActivity
            {
                Id = "act_charity_gala",
                Name = "Children's Hospital Charity Gala",
                Category = SocialActivityCategory.Philanthropy,
                EnergyCost = 20,
                FinancialCost = 1500m,
                MoraleBoost = 18f,
                TeamAffinityBoost = 10f,
                PrestigeBoost = 60,
                ManagerTrustRisk = 0.0f,
                Description = "Black-tie fundraising auction generating donations and widespread public goodwill.",
                IconEmoji = "🎗️"
            }
        };

        public static IReadOnlyList<SocialActivity> AllActivities => s_activities;

        public static SocialActivity? GetById(string id) =>
            s_activities.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<SocialActivity> GetByCategory(SocialActivityCategory category) =>
            s_activities.Where(a => a.Category == category).ToList();
    }
}
