using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Captures a residential property definition, its associated lifestyle tier,
    /// financial upkeep, and player condition modifiers.
    /// Pure C# domain model.
    /// </summary>
    public sealed record HomeProperty
    {
        public LifestyleTier Tier { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal WeeklyUpkeep { get; init; }
        public decimal PurchaseCost { get; init; }
        public float RestRecoveryMultiplier { get; init; } = 1.0f;
        public float GymWorkoutMultiplier { get; init; } = 1.0f;
        public int WeeklyMoraleBonus { get; init; } = 0;
        public int PrestigeScore { get; init; } = 10;
    }

    /// <summary>
    /// Static catalog of standard property progression tiers.
    /// </summary>
    public static class HomePropertyCatalog
    {
        private static readonly Dictionary<LifestyleTier, HomeProperty> _properties = new()
        {
            [LifestyleTier.Modest] = new HomeProperty
            {
                Tier = LifestyleTier.Modest,
                Name = "Academy Dormitory / Shared Flat",
                Location = "Northfield Suburbs",
                Description = "A modest shared apartment close to the training ground. Basic single bed, folding study desk, and simple living space.",
                WeeklyUpkeep = 150m,
                PurchaseCost = 0m,
                RestRecoveryMultiplier = 1.0f,
                GymWorkoutMultiplier = 1.0f,
                WeeklyMoraleBonus = 0,
                PrestigeScore = 10
            },
            [LifestyleTier.Comfortable] = new HomeProperty
            {
                Tier = LifestyleTier.Comfortable,
                Name = "Riverside Modern Apartment",
                Location = "Westford Marina",
                Description = "A bright private 2-bedroom flat with hardwood floors, a comfortable double bed, and dedicated home workout corner.",
                WeeklyUpkeep = 500m,
                PurchaseCost = 15000m,
                RestRecoveryMultiplier = 1.15f,
                GymWorkoutMultiplier = 1.10f,
                WeeklyMoraleBonus = 1,
                PrestigeScore = 35
            },
            [LifestyleTier.Luxurious] = new HomeProperty
            {
                Tier = LifestyleTier.Luxurious,
                Name = "Skyline Luxury Penthouse",
                Location = "City Centre Financial District",
                Description = "High-rise penthouse boasting panoramic skyline views, king-size orthopaedic bed, and a private cardio & resistance gym suite.",
                WeeklyUpkeep = 2000m,
                PurchaseCost = 75000m,
                RestRecoveryMultiplier = 1.30f,
                GymWorkoutMultiplier = 1.25f,
                WeeklyMoraleBonus = 3,
                PrestigeScore = 70
            },
            [LifestyleTier.Extravagant] = new HomeProperty
            {
                Tier = LifestyleTier.Extravagant,
                Name = "Gated Celebrity Villa",
                Location = "Oakridge Hills",
                Description = "Exclusive gated residence with marble interior, master suite, professional athlete gym, home cinema, and secure compound.",
                WeeklyUpkeep = 8000m,
                PurchaseCost = 350000m,
                RestRecoveryMultiplier = 1.45f,
                GymWorkoutMultiplier = 1.35f,
                WeeklyMoraleBonus = 5,
                PrestigeScore = 90
            },
            [LifestyleTier.Superstar] = new HomeProperty
            {
                Tier = LifestyleTier.Superstar,
                Name = "Superstar Country Estate",
                Location = "Crown Valley Estates",
                Description = "Sprawling private compound featuring Olympic recovery spa, cryotherapy recovery pod, state-of-the-art gym, and full personal security.",
                WeeklyUpkeep = 25000m,
                PurchaseCost = 1200000m,
                RestRecoveryMultiplier = 1.60f,
                GymWorkoutMultiplier = 1.50f,
                WeeklyMoraleBonus = 8,
                PrestigeScore = 100
            }
        };

        public static HomeProperty GetProperty(LifestyleTier tier)
        {
            if (_properties.TryGetValue(tier, out var prop))
                return prop;
            return _properties[LifestyleTier.Modest];
        }

        public static IReadOnlyCollection<HomeProperty> AllProperties => _properties.Values;
    }
}
