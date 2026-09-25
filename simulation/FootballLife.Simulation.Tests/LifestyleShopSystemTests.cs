using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LifestyleShopSystemTests
    {
        [Fact]
        public void LifestyleCatalog_ContainsAllCategories_AndValidItems()
        {
            var allItems = LifestyleCatalog.AllItems;
            Assert.NotEmpty(allItems);
            Assert.True(allItems.Count >= 12);

            foreach (var item in allItems)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Id));
                Assert.False(string.IsNullOrWhiteSpace(item.Name));
                Assert.True(item.Price > 0m);
                Assert.True(item.WeeklyUpkeep >= 0m);
                Assert.True(item.PrestigeScore > 0);
            }

            var vehicles = new List<LifestyleItem>(LifestyleCatalog.GetByCategory(LifestyleCategory.Vehicles));
            var fashion = new List<LifestyleItem>(LifestyleCatalog.GetByCategory(LifestyleCategory.Fashion));
            var tech = new List<LifestyleItem>(LifestyleCatalog.GetByCategory(LifestyleCategory.Tech));
            var wellness = new List<LifestyleItem>(LifestyleCatalog.GetByCategory(LifestyleCategory.Wellness));

            Assert.NotEmpty(vehicles);
            Assert.NotEmpty(fashion);
            Assert.NotEmpty(tech);
            Assert.NotEmpty(wellness);
        }

        [Fact]
        public void CanAffordItem_ReturnsTrue_WhenBalanceSufficient()
        {
            var account = FinanceAccount.Create(10000m);
            var item = LifestyleCatalog.GetItem("tch_gaming")!; // 5,200

            Assert.True(LifestyleShopSystem.CanAffordItem(account, item));
        }

        [Fact]
        public void CanAffordItem_ReturnsFalse_WhenBalanceInsufficient()
        {
            var account = FinanceAccount.Create(3000m);
            var item = LifestyleCatalog.GetItem("tch_gaming")!; // 5,200

            Assert.False(LifestyleShopSystem.CanAffordItem(account, item));
        }

        [Fact]
        public void PurchaseItem_Succeeds_WhenFundsAvailable()
        {
            var account = FinanceAccount.Create(50000m);
            var item = LifestyleCatalog.GetItem("veh_sedan")!; // 28,000
            var date = new DateOnly(2026, 8, 15);

            var result = LifestyleShopSystem.PurchaseItem(account, new List<string>(), item, date);

            Assert.True(result.Success);
            Assert.Equal(22000m, result.Account.Balance);
            Assert.Contains(item.Id, result.OwnedItemIds);
            Assert.Equal(item.MoralePerk, result.MoraleDelta);
            Assert.Single(result.Account.History);
            Assert.Equal(-28000m, result.Account.History[0].Amount);
        }

        [Fact]
        public void PurchaseItem_Fails_WhenDuplicateItem()
        {
            var account = FinanceAccount.Create(50000m);
            var item = LifestyleCatalog.GetItem("wel_espresso")!;
            var owned = new List<string> { item.Id };
            var date = new DateOnly(2026, 8, 15);

            var result = LifestyleShopSystem.PurchaseItem(account, owned, item, date);

            Assert.False(result.Success);
            Assert.Equal(50000m, result.Account.Balance);
            Assert.Contains("already own", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PurchaseItem_Fails_WhenInsufficientFunds()
        {
            var account = FinanceAccount.Create(500m);
            var item = LifestyleCatalog.GetItem("veh_hypercar")!;
            var date = new DateOnly(2026, 8, 15);

            var result = LifestyleShopSystem.PurchaseItem(account, new List<string>(), item, date);

            Assert.False(result.Success);
            Assert.Equal(500m, result.Account.Balance);
            Assert.Contains("Insufficient funds", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CalculateTotalItemUpkeep_SumsCorrectly()
        {
            // veh_sedan: 120, wel_espresso: 35
            var owned = new[] { "veh_sedan", "wel_espresso" };
            decimal upkeep = LifestyleShopSystem.CalculateTotalItemUpkeep(owned);

            Assert.Equal(155m, upkeep);
        }

        [Fact]
        public void CalculateTotalPerks_AggregatesCorrectly()
        {
            // wel_boots: Morale 2.0, Energy 0.06, Prestige 30
            // wel_cryo: Morale 5.0, Energy 0.12, Prestige 65
            var owned = new[] { "wel_boots", "wel_cryo" };
            var (morale, energy, prestige) = LifestyleShopSystem.CalculateTotalPerks(owned);

            Assert.Equal(7.0f, morale);
            Assert.Equal(0.18f, energy, 3);
            Assert.Equal(95, prestige);
        }
    }
}
