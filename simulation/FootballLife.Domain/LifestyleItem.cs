using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Category grouping for lifestyle item purchases.
    /// </summary>
    public enum LifestyleCategory
    {
        Vehicles = 0,
        Fashion = 1,
        Tech = 2,
        Wellness = 3
    }

    /// <summary>
    /// Represents an optional lifestyle purchase (vehicles, jewelry, gadgets, wellness gear)
    /// that modifies player prestige, baseline morale, recovery efficiency, and weekly upkeep.
    /// Pure C# domain model.
    /// </summary>
    public sealed record LifestyleItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public LifestyleCategory Category { get; init; }
        public decimal Price { get; init; }
        public decimal WeeklyUpkeep { get; init; }
        public float MoralePerk { get; init; }
        public float EnergyRecoveryPerk { get; init; }
        public int PrestigeScore { get; init; }
        public string Description { get; init; } = string.Empty;
        public string IconEmoji { get; init; } = "⭐";
    }

    /// <summary>
    /// Static catalog of all available lifestyle items in the shop.
    /// </summary>
    public static class LifestyleCatalog
    {
        private static readonly Dictionary<string, LifestyleItem> _items = new()
        {
            // --- VEHICLES ---
            ["veh_scooter"] = new LifestyleItem
            {
                Id = "veh_scooter",
                Name = "Urban E-Scooter",
                Category = LifestyleCategory.Vehicles,
                Price = 850m,
                WeeklyUpkeep = 15m,
                MoralePerk = 1.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 5,
                Description = "Lightweight electric scooter for quick, eco-friendly trips between the training pitch and city flat.",
                IconEmoji = "🛴"
            },
            ["veh_sedan"] = new LifestyleItem
            {
                Id = "veh_sedan",
                Name = "Executive Sedan",
                Category = LifestyleCategory.Vehicles,
                Price = 28000m,
                WeeklyUpkeep = 120m,
                MoralePerk = 3.0f,
                EnergyRecoveryPerk = 0.02f,
                PrestigeScore = 25,
                Description = "Sleek, dependable German saloon with heated leather seats and whisper-quiet highway ride.",
                IconEmoji = "🚗"
            },
            ["veh_coupe"] = new LifestyleItem
            {
                Id = "veh_coupe",
                Name = "GT Sports Coupe",
                Category = LifestyleCategory.Vehicles,
                Price = 75000m,
                WeeklyUpkeep = 350m,
                MoralePerk = 6.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 55,
                Description = "Twin-turbo coupe with ferocious exhaust roar and unmistakable matchday presence outside the stadium.",
                IconEmoji = "🏎️"
            },
            ["veh_suv"] = new LifestyleItem
            {
                Id = "veh_suv",
                Name = "Luxury V8 SUV",
                Category = LifestyleCategory.Vehicles,
                Price = 140000m,
                WeeklyUpkeep = 600m,
                MoralePerk = 8.0f,
                EnergyRecoveryPerk = 0.03f,
                PrestigeScore = 75,
                Description = "Full-size luxury SUV with air suspension, panoramic roof, and commanding road presence.",
                IconEmoji = "🚙"
            },
            ["veh_hypercar"] = new LifestyleItem
            {
                Id = "veh_hypercar",
                Name = "Carbon Edition Hypercar",
                Category = LifestyleCategory.Vehicles,
                Price = 650000m,
                WeeklyUpkeep = 2200m,
                MoralePerk = 15.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 98,
                Description = "Limited-run aerodynamic masterpiece. Turns every training ground arrival into a media spectacle.",
                IconEmoji = "🚀"
            },

            // --- FASHION & LUXURY ---
            ["fsh_streetwear"] = new LifestyleItem
            {
                Id = "fsh_streetwear",
                Name = "Designer Streetwear Capsule",
                Category = LifestyleCategory.Fashion,
                Price = 1800m,
                WeeklyUpkeep = 40m,
                MoralePerk = 2.5f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 15,
                Description = "Curated luxury oversized hoodies, distressed cargo trousers, and rare collaboration sneakers.",
                IconEmoji = "👟"
            },
            ["fsh_suit"] = new LifestyleItem
            {
                Id = "fsh_suit",
                Name = "Savile Row Bespoke Suit",
                Category = LifestyleCategory.Fashion,
                Price = 4500m,
                WeeklyUpkeep = 60m,
                MoralePerk = 4.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 35,
                Description = "Hand-tailored wool-silk tuxedo for awards galas, press conferences, and club banquets.",
                IconEmoji = "🤵"
            },
            ["fsh_chronograph"] = new LifestyleItem
            {
                Id = "fsh_chronograph",
                Name = "Swiss Gold Chronograph",
                Category = LifestyleCategory.Fashion,
                Price = 24000m,
                WeeklyUpkeep = 100m,
                MoralePerk = 6.5f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 60,
                Description = "Iconic rose gold mechanical timepiece. The quintessential footballer statement watch.",
                IconEmoji = "⌚"
            },
            ["fsh_diamonds"] = new LifestyleItem
            {
                Id = "fsh_diamonds",
                Name = "Diamond Chain & Studs",
                Category = LifestyleCategory.Fashion,
                Price = 60000m,
                WeeklyUpkeep = 200m,
                MoralePerk = 10.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 80,
                Description = "Custom VVS brilliant-cut diamond earrings and tennis necklace designed by celebrity jewelers.",
                IconEmoji = "💎"
            },

            // --- TECH & HOME ---
            ["tch_headphones"] = new LifestyleItem
            {
                Id = "tch_headphones",
                Name = "Pro Noise-Cancelling Cans",
                Category = LifestyleCategory.Tech,
                Price = 450m,
                WeeklyUpkeep = 10m,
                MoralePerk = 1.5f,
                EnergyRecoveryPerk = 0.02f,
                PrestigeScore = 10,
                Description = "Studio-grade wireless headphones for pre-match tunnel focus and team bus journey relaxation.",
                IconEmoji = "🎧"
            },
            ["tch_gaming"] = new LifestyleItem
            {
                Id = "tch_gaming",
                Name = "Custom Esports Battlestation",
                Category = LifestyleCategory.Tech,
                Price = 5200m,
                WeeklyUpkeep = 50m,
                MoralePerk = 4.0f,
                EnergyRecoveryPerk = 0.0f,
                PrestigeScore = 25,
                Description = "Water-cooled triple-monitor gaming rig for evening streaming and multiplayer gaming with teammates.",
                IconEmoji = "🖥️"
            },
            ["tch_cinema"] = new LifestyleItem
            {
                Id = "tch_cinema",
                Name = "Private Dolby Atmos Cinema",
                Category = LifestyleCategory.Tech,
                Price = 45000m,
                WeeklyUpkeep = 250m,
                MoralePerk = 7.0f,
                EnergyRecoveryPerk = 0.04f,
                PrestigeScore = 70,
                Description = "Acoustically tuned home screening room with 4K laser projection and reclining plush leather recliners.",
                IconEmoji = "🎬"
            },

            // --- WELLNESS & RECOVERY ---
            ["wel_espresso"] = new LifestyleItem
            {
                Id = "wel_espresso",
                Name = "Artisan Espresso Machine",
                Category = LifestyleCategory.Wellness,
                Price = 2200m,
                WeeklyUpkeep = 35m,
                MoralePerk = 2.0f,
                EnergyRecoveryPerk = 0.03f,
                PrestigeScore = 20,
                Description = "Dual-boiler Italian espresso bar for morning focus and optimal training session energy.",
                IconEmoji = "☕"
            },
            ["wel_boots"] = new LifestyleItem
            {
                Id = "wel_boots",
                Name = "Pneumatic Compression Boots",
                Category = LifestyleCategory.Wellness,
                Price = 3800m,
                WeeklyUpkeep = 40m,
                MoralePerk = 2.0f,
                EnergyRecoveryPerk = 0.06f,
                PrestigeScore = 30,
                Description = "Sequential pulse compression boots used by elite athletes to flush lactic acid after intense training.",
                IconEmoji = "🦵"
            },
            ["wel_cryo"] = new LifestyleItem
            {
                Id = "wel_cryo",
                Name = "Home Cryotherapy Chamber",
                Category = LifestyleCategory.Wellness,
                Price = 55000m,
                WeeklyUpkeep = 300m,
                MoralePerk = 5.0f,
                EnergyRecoveryPerk = 0.12f,
                PrestigeScore = 65,
                Description = "Sub-zero liquid nitrogen recovery pod providing accelerated systemic muscle and joint regeneration.",
                IconEmoji = "❄️"
            },
            ["wel_chef"] = new LifestyleItem
            {
                Id = "wel_chef",
                Name = "Personal Michelin Performance Chef",
                Category = LifestyleCategory.Wellness,
                Price = 120000m,
                WeeklyUpkeep = 1500m,
                MoralePerk = 10.0f,
                EnergyRecoveryPerk = 0.18f,
                PrestigeScore = 90,
                Description = "Full-time private sports nutritionist preparing organic, performance-optimized meals daily.",
                IconEmoji = "👨‍🍳"
            }
        };

        public static LifestyleItem? GetItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _items.TryGetValue(id, out var item) ? item : null;
        }

        public static IReadOnlyCollection<LifestyleItem> AllItems => _items.Values;

        public static IEnumerable<LifestyleItem> GetByCategory(LifestyleCategory category)
        {
            foreach (var item in _items.Values)
            {
                if (item.Category == category)
                    yield return item;
            }
        }
    }
}
