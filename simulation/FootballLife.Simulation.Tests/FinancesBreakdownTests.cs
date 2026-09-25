using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class FinancesBreakdownTests
    {
        [Fact]
        public void FinanceBreakdown_CalculatesIncomeAndExpenses_Correctly()
        {
            decimal weeklyWage = 5000m;
            decimal bankBalance = 25000m;
            var tier = LifestyleTier.Comfortable; // tierCost = 500
            var prop = HomePropertyCatalog.GetProperty(tier); // upkeep = 500, value = 15,000
            var owned = new List<LifestyleItem>
            {
                LifestyleCatalog.GetItem("veh_sedan")!, // upkeep 120, price 28,000, prestige 25
                LifestyleCatalog.GetItem("wel_espresso")! // upkeep 35, price 2,200, prestige 20
            };

            var breakdown = FinanceBreakdown.Calculate(
                weeklyWage: weeklyWage,
                bankBalance: bankBalance,
                tier: tier,
                property: prop,
                ownedItems: owned,
                matchBonusesRecent: 1000m,
                sponsorshipWeekly: 500m);

            // Income: 5000 + 1000 + 500 = 6500
            Assert.Equal(6500m, breakdown.TotalWeeklyIncome);

            // Expenses:
            // aptUpkeep = 500
            // tierCost = 500
            // itemsUpkeep = 120 + 35 = 155
            // tax = (5000 - 1000) * 0.35 = 1400
            // agent = 5000 * 0.05 = 250
            // total expenses = 500 + 500 + 155 + 1400 + 250 = 2805
            Assert.Equal(2805m, breakdown.TotalWeeklyExpenses);

            // Net Cash Flow: 6500 - 2805 = 3695
            Assert.Equal(3695m, breakdown.NetWeeklyCashFlow);
            Assert.True(breakdown.NetWeeklyCashFlow > 0m);

            // Net Worth:
            // bankBalance (25,000) + propertyValue (15,000) + itemsValue (28,000 + 2,200 = 30,200) = 70,200
            Assert.Equal(70200m, breakdown.EstimatedNetWorth);

            // Prestige: prop prestige (35) + items prestige (25 + 20 = 45) = 80
            Assert.Equal(80, breakdown.TotalPrestige);
        }

        [Fact]
        public void SleepRecovery_WithWellnessPerks_RecoversMoreEnergy()
        {
            var tier = LifestyleTier.Comfortable;
            int initialEnergy = 50;

            // Without wellness items
            var resultBase = HomeSystem.CalculateSleepRecovery(tier, initialEnergy, hoursSlept: 8, additionalRecoveryBonus: 0f);

            // With wellness items (e.g. Cryotherapy + Boots = +0.18f)
            var resultPerks = HomeSystem.CalculateSleepRecovery(tier, initialEnergy, hoursSlept: 8, additionalRecoveryBonus: 0.18f);

            Assert.True(resultPerks.EnergyGained > resultBase.EnergyGained);
            Assert.True(resultPerks.RecoveredEnergy > resultBase.RecoveredEnergy);
        }
    }
}
