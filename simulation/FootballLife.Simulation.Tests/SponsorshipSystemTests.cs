using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class SponsorshipSystemTests
    {
        private readonly SponsorshipSystem _system = new();

        [Fact]
        public void GetAvailableOffers_FiltersByReputation_ExcludesActiveDeals()
        {
            var deals = SponsorshipSystem.GetAllCatalogDeals();
            Assert.NotEmpty(deals);

            var firstDeal = deals.First();
            var active = new List<ActiveSponsorship>
            {
                new(firstDeal, 20, 5000)
            };

            // Player with low reputation (20) should see low-tier deals, but not the active one
            var available = _system.GetAvailableOffers(20, active);

            Assert.DoesNotContain(available, d => d.Id == firstDeal.Id);
            Assert.All(available, d => Assert.True(20 >= (d.RequiredReputation - 5)));
        }

        [Fact]
        public void AcceptDeal_Succeeds_WhenReputationMet_AndAwardsSigningBonus()
        {
            var deal = SponsorshipSystem.GetAllCatalogDeals().First(d => d.Tier == SponsorshipTier.Local);

            var (active, bonus, error) = _system.AcceptDeal(deal, playerReputation: deal.RequiredReputation + 5, activeDeals: Array.Empty<ActiveSponsorship>());

            Assert.Null(error);
            Assert.NotNull(active);
            Assert.Equal(deal.Id, active!.Deal.Id);
            Assert.Equal(deal.DurationWeeks, active.WeeksRemaining);
            Assert.Equal(deal.SigningBonus, bonus);
        }

        [Fact]
        public void AcceptDeal_Rejects_WhenReputationInsufficient()
        {
            var deal = SponsorshipSystem.GetAllCatalogDeals().First(d => d.Tier == SponsorshipTier.Global);

            var (active, bonus, error) = _system.AcceptDeal(deal, playerReputation: 10, activeDeals: Array.Empty<ActiveSponsorship>());

            Assert.NotNull(error);
            Assert.Null(active);
            Assert.Equal(0, bonus);
            Assert.Contains("Insufficient reputation", error);
        }

        [Fact]
        public void AcceptDeal_Rejects_WhenSlotCapacityReached()
        {
            var deals = SponsorshipSystem.GetAllCatalogDeals().Take(3).ToList();
            var activeDeals = deals.Select(d => new ActiveSponsorship(d, 10, 0)).ToList();
            var candidateDeal = SponsorshipSystem.GetAllCatalogDeals().Skip(3).First();

            var (active, bonus, error) = _system.AcceptDeal(candidateDeal, playerReputation: 99, activeDeals: activeDeals, maxSlots: 3);

            Assert.NotNull(error);
            Assert.Null(active);
            Assert.Contains("Maximum commercial sponsorships capacity", error);
        }

        [Fact]
        public void ProcessWeeklyPayouts_AccruesIncomeAndDecrementsWeeks()
        {
            var deal1 = new SponsorshipDeal("d1", "Brand A", SponsorshipTier.Local, SponsorshipType.Boots, 10, 500, 1000, 2);
            var deal2 = new SponsorshipDeal("d2", "Brand B", SponsorshipTier.Regional, SponsorshipType.Beverage, 20, 1500, 2000, 1);

            var active = new List<ActiveSponsorship>
            {
                new(deal1, weeksRemaining: 2, totalEarned: 0),
                new(deal2, weeksRemaining: 1, totalEarned: 0)
            };

            var (totalPayout, updated, expired) = _system.ProcessWeeklyPayouts(active);

            // Total payout: 500 + 1500 = 2000
            Assert.Equal(2000, totalPayout);

            // deal2 had 1 week remaining, now 0 -> expired
            Assert.Single(expired);
            Assert.Equal("d2", expired[0].Id);

            // deal1 had 2 weeks remaining, now 1 -> survivor
            Assert.Single(updated);
            Assert.Equal("d1", updated[0].Deal.Id);
            Assert.Equal(1, updated[0].WeeksRemaining);
            Assert.Equal(500, updated[0].TotalEarned);
        }

        [Fact]
        public void CheckTermination_TriggersOnReputationCollapseOrCrisisForm()
        {
            var deal = new SponsorshipDeal("d1", "Elite Brand", SponsorshipTier.National, SponsorshipType.Luxury, 70, 5000, 10000, 20);
            var active = new ActiveSponsorship(deal, 20, 0);

            // Normal state: no termination
            var (term1, _) = _system.CheckTermination(active, currentReputation: 72, playerForm: 65f);
            Assert.False(term1);

            // Severe reputation drop: required 70, now 50 (drop > 15)
            var (term2, reason2) = _system.CheckTermination(active, currentReputation: 50, playerForm: 65f);
            Assert.True(term2);
            Assert.Contains("declining public reputation", reason2);

            // Severe form crisis (< 15)
            var (term3, reason3) = _system.CheckTermination(active, currentReputation: 75, playerForm: 10f);
            Assert.True(term3);
            Assert.Contains("on-pitch slump", reason3);
        }

        [Fact]
        public void CalculateTotals_ComputesWeeklyIncomeEnergyAndFame()
        {
            var deal1 = new SponsorshipDeal("d1", "A", SponsorshipTier.Local, SponsorshipType.Boots, 10, 500, 0, 10, energyRecoveryBonus: 2, weeklyFameBonus: 1);
            var deal2 = new SponsorshipDeal("d2", "B", SponsorshipTier.Regional, SponsorshipType.Beverage, 20, 1200, 0, 10, energyRecoveryBonus: 3, weeklyFameBonus: 2);

            var active = new List<ActiveSponsorship>
            {
                new(deal1, 10, 0),
                new(deal2, 10, 0)
            };

            Assert.Equal(1700, _system.CalculateTotalWeeklyCommercialIncome(active));
            Assert.Equal(5, _system.CalculateTotalWeeklyEnergyBonus(active));
            Assert.Equal(3, _system.CalculateTotalWeeklyFameBonus(active));
        }
    }
}
