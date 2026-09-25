using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# simulation system managing commercial endorsements, reputation-gated deals,
    /// weekly payouts, and brand perks.
    /// </summary>
    public sealed class SponsorshipSystem
    {
        public const int MaxConcurrentSponsorships = 3;

        private static readonly IReadOnlyList<SponsorshipDeal> Catalog = new List<SponsorshipDeal>
        {
            // ── Local Tier (Reputation 15 - 35) ──────────────────────────────
            new SponsorshipDeal("sp_local_bakery", "County Bakery & Deli", SponsorshipTier.Local, SponsorshipType.Apparel, 15, 250, 1000, 26, energyRecoveryBonus: 0, weeklyFameBonus: 1, "Local matchday tracksuits and community storefront appearances."),
            new SponsorshipDeal("sp_local_gym", "Metro Fitness & Recovery", SponsorshipTier.Local, SponsorshipType.Beverage, 25, 450, 2000, 26, energyRecoveryBonus: 2, weeklyFameBonus: 1, "Post-match recovery shakes and local fitness club membership."),
            new SponsorshipDeal("sp_local_auto", "Midland Motor Hub", SponsorshipTier.Local, SponsorshipType.Luxury, 35, 750, 3500, 26, energyRecoveryBonus: 0, weeklyFameBonus: 2, "Drive local showroom cars and appear on regional billboard campaigns."),

            // ── Regional Tier (Reputation 45 - 55) ───────────────────────────
            new SponsorshipDeal("sp_reg_energy", "Pulse Hydration", SponsorshipTier.Regional, SponsorshipType.Beverage, 45, 1500, 8000, 36, energyRecoveryBonus: 3, weeklyFameBonus: 2, "Official regional isotonic energy drink partner."),
            new SponsorshipDeal("sp_reg_boots", "Strikeline Pro Footwear", SponsorshipTier.Regional, SponsorshipType.Boots, 50, 2200, 12000, 36, energyRecoveryBonus: 1, weeklyFameBonus: 2, "Custom leather matchday boots with personalized embroidery."),
            new SponsorshipDeal("sp_reg_fashion", "Urban Edge Apparel", SponsorshipTier.Regional, SponsorshipType.Apparel, 55, 2800, 15000, 36, energyRecoveryBonus: 0, weeklyFameBonus: 3, "High-street fashion collection tailored for rising football talent."),

            // ── National Tier (Reputation 65 - 80) ───────────────────────────
            new SponsorshipDeal("sp_nat_boots", "Apex Velocity Footwear", SponsorshipTier.National, SponsorshipType.Boots, 65, 6500, 35000, 52, energyRecoveryBonus: 2, weeklyFameBonus: 3, "Elite carbon-sole speed boots featured in national television ads."),
            new SponsorshipDeal("sp_nat_fuel", "Titan Performance Fuel", SponsorshipTier.National, SponsorshipType.Beverage, 70, 8000, 45000, 52, energyRecoveryBonus: 4, weeklyFameBonus: 3, "Advanced electrolyte recovery system for high-intensity footballers."),
            new SponsorshipDeal("sp_nat_watch", "Vanguard Chronographs", SponsorshipTier.National, SponsorshipType.Luxury, 75, 12000, 65000, 52, energyRecoveryBonus: 0, weeklyFameBonus: 4, "Ambassador for premium Swiss-engineered sports chronographs."),
            new SponsorshipDeal("sp_nat_tech", "Zenith Acoustics & Wearables", SponsorshipTier.National, SponsorshipType.Tech, 80, 15000, 80000, 52, energyRecoveryBonus: 0, weeklyFameBonus: 4, "Noise-cancelling tunnel walk headphones and biometric smart rings."),

            // ── Global Mega-Brand Tier (Reputation 85 - 95) ──────────────────
            new SponsorshipDeal("sp_glob_boots", "AeroStrike HyperElite", SponsorshipTier.Global, SponsorshipType.Boots, 85, 35000, 200000, 52, energyRecoveryBonus: 3, weeklyFameBonus: 5, "Signature worldwide custom boots line with bespoke colorway drops."),
            new SponsorshipDeal("sp_glob_energy", "Red Nova Worldwide", SponsorshipTier.Global, SponsorshipType.Beverage, 90, 45000, 300000, 52, energyRecoveryBonus: 5, weeklyFameBonus: 5, "Global extreme sports ambassador featuring in international commercials."),
            new SponsorshipDeal("sp_glob_luxury", "Royal Sovereign Horology", SponsorshipTier.Global, SponsorshipType.Luxury, 92, 65000, 500000, 52, energyRecoveryBonus: 0, weeklyFameBonus: 6, "Ultra-luxury global timepiece icon with Met Gala and red carpet invites."),
            new SponsorshipDeal("sp_glob_tech", "OmniVision Silicon", SponsorshipTier.Global, SponsorshipType.Tech, 95, 80000, 750000, 52, energyRecoveryBonus: 0, weeklyFameBonus: 6, "Flagship ambassador for global smartphones, wearables, and VR gaming.")
        };

        public static IReadOnlyList<SponsorshipDeal> GetAllCatalogDeals() => Catalog;

        /// <summary>
        /// Retrieves available deals that the player is eligible for or close to unlocking,
        /// excluding deals already active.
        /// </summary>
        public IReadOnlyList<SponsorshipDeal> GetAvailableOffers(
            int playerReputation,
            IReadOnlyList<ActiveSponsorship> activeDeals,
            SimulationRandom? random = null)
        {
            var activeIds = new HashSet<string>(activeDeals?.Select(a => a.Deal.Id) ?? Enumerable.Empty<string>());
            
            // Available if reputation meets requirement or within 5 points (preview)
            var eligible = Catalog
                .Where(d => !activeIds.Contains(d.Id) && playerReputation >= (d.RequiredReputation - 5))
                .OrderByDescending(d => d.RequiredReputation)
                .ThenByDescending(d => d.WeeklyPayout)
                .ToList();

            return new ReadOnlyCollection<SponsorshipDeal>(eligible);
        }

        /// <summary>
        /// Attempts to accept and sign an endorsement deal.
        /// </summary>
        public (ActiveSponsorship? activeDeal, int signingBonus, string? error) AcceptDeal(
            SponsorshipDeal deal,
            int playerReputation,
            IReadOnlyList<ActiveSponsorship> activeDeals,
            int maxSlots = MaxConcurrentSponsorships)
        {
            if (deal is null) throw new ArgumentNullException(nameof(deal));
            activeDeals ??= Array.Empty<ActiveSponsorship>();

            if (activeDeals.Count >= maxSlots)
            {
                return (null, 0, $"Maximum commercial sponsorships capacity ({maxSlots}) reached.");
            }

            if (activeDeals.Any(a => a.Deal.Id == deal.Id))
            {
                return (null, 0, $"Already endorsed by {deal.BrandName}.");
            }

            if (playerReputation < deal.RequiredReputation)
            {
                return (null, 0, $"Insufficient reputation ({playerReputation}/{deal.RequiredReputation}) to sign with {deal.BrandName}.");
            }

            var active = new ActiveSponsorship(deal, deal.DurationWeeks, 0);
            return (active, deal.SigningBonus, null);
        }

        /// <summary>
        /// Advances active sponsorships by one week, crediting weekly payouts and noting expirations.
        /// </summary>
        public (int totalPayout, IReadOnlyList<ActiveSponsorship> updatedDeals, IReadOnlyList<SponsorshipDeal> expiredDeals) ProcessWeeklyPayouts(
            IReadOnlyList<ActiveSponsorship> currentDeals)
        {
            if (currentDeals is null || currentDeals.Count == 0)
            {
                return (0, Array.Empty<ActiveSponsorship>(), Array.Empty<SponsorshipDeal>());
            }

            int totalPayout = 0;
            var updated = new List<ActiveSponsorship>(currentDeals.Count);
            var expired = new List<SponsorshipDeal>();

            foreach (var deal in currentDeals)
            {
                totalPayout += deal.Deal.WeeklyPayout;
                var ticked = deal.TickWeek();

                if (ticked.IsExpired)
                {
                    expired.Add(deal.Deal);
                }
                else
                {
                    updated.Add(ticked);
                }
            }

            return (totalPayout, updated, expired);
        }

        /// <summary>
        /// Checks whether brand terminates sponsorship due to severe reputation drop or form collapse.
        /// </summary>
        public (bool shouldTerminate, string? reason) CheckTermination(
            ActiveSponsorship active,
            int currentReputation,
            float playerForm)
        {
            if (active is null) throw new ArgumentNullException(nameof(active));

            // If reputation drops 15+ points below requirement
            if (currentReputation < (active.Deal.RequiredReputation - 15))
            {
                return (true, $"{active.Deal.BrandName} terminated their deal due to declining public reputation ({currentReputation}).");
            }

            // If player form is below 15 (severe crisis)
            if (playerForm < 15f)
            {
                return (true, $"{active.Deal.BrandName} terminated endorsement due to prolonged on-pitch slump.");
            }

            return (false, null);
        }

        /// <summary>
        /// Evaluates active deals, removing any that breach brand standards or have expired.
        /// </summary>
        public IReadOnlyList<ActiveSponsorship> FilterActiveDeals(
            IReadOnlyList<ActiveSponsorship> activeDeals,
            int currentReputation,
            float playerForm,
            out List<string> terminationLogs)
        {
            terminationLogs = new List<string>();
            if (activeDeals is null || activeDeals.Count == 0)
            {
                return Array.Empty<ActiveSponsorship>();
            }

            var survivors = new List<ActiveSponsorship>();
            foreach (var deal in activeDeals)
            {
                var (terminated, reason) = CheckTermination(deal, currentReputation, playerForm);
                if (terminated)
                {
                    terminationLogs.Add(reason!);
                }
                else
                {
                    survivors.Add(deal);
                }
            }

            return survivors;
        }

        public int CalculateTotalWeeklyCommercialIncome(IReadOnlyList<ActiveSponsorship> activeDeals)
        {
            return activeDeals?.Sum(a => a.Deal.WeeklyPayout) ?? 0;
        }

        public int CalculateTotalWeeklyEnergyBonus(IReadOnlyList<ActiveSponsorship> activeDeals)
        {
            return activeDeals?.Sum(a => a.Deal.EnergyRecoveryBonus) ?? 0;
        }

        public int CalculateTotalWeeklyFameBonus(IReadOnlyList<ActiveSponsorship> activeDeals)
        {
            return activeDeals?.Sum(a => a.Deal.WeeklyFameBonus) ?? 0;
        }
    }
}
