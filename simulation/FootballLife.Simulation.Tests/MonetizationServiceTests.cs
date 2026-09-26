using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class MonetizationServiceTests
    {
        [Fact]
        public void MonetizationService_ReturnsCatalogProducts()
        {
            var service = new MonetizationService();
            var catalog = service.GetCatalog();

            Assert.NotNull(catalog);
            Assert.True(catalog.Count >= 6, $"Expected >= 6 catalog items, got {catalog.Count}");

            var boots = service.GetProduct("cosmetic.golden_boots");
            Assert.NotNull(boots);
            Assert.Equal("Golden Touch Boots", boots!.Title);
            Assert.Equal(MonetizationCategory.Cosmetic, boots.Category);
        }

        [Fact]
        public void MonetizationService_PurchasingCosmetic_PreventsDuplicates()
        {
            var service = new MonetizationService();
            var save = new CareerSaveData();

            Assert.False(service.HasEntitlement("cosmetic.golden_boots", save));

            // First purchase
            var res1 = service.PurchaseProduct("cosmetic.golden_boots", save);
            Assert.Equal(PurchaseStatus.Success, res1.Status);
            Assert.True(service.HasEntitlement("cosmetic.golden_boots", save));
            Assert.Contains("cosmetic.golden_boots", save.OwnedCosmeticIds);

            // Duplicate purchase attempt
            var res2 = service.PurchaseProduct("cosmetic.golden_boots", save);
            Assert.Equal(PurchaseStatus.AlreadyOwned, res2.Status);
        }

        [Fact]
        public void MonetizationService_PurchasingTokens_IncrementsAndConsumesCorrectly()
        {
            var service = new MonetizationService();
            var save = new CareerSaveData
            {
                CareerRewindTokens = 1
            };

            // Purchase 3-pack
            var result = service.PurchaseProduct("token.career_rewind_3x", save);
            Assert.Equal(PurchaseStatus.Success, result.Status);
            Assert.Equal(4, save.CareerRewindTokens); // 1 + 3

            // Consume tokens
            bool used1 = service.ConsumeRewindToken(save);
            Assert.True(used1);
            Assert.Equal(3, save.CareerRewindTokens);

            service.ConsumeRewindToken(save);
            service.ConsumeRewindToken(save);
            service.ConsumeRewindToken(save);
            Assert.Equal(0, save.CareerRewindTokens);

            // Cannot consume when 0
            bool failedUse = service.ConsumeRewindToken(save);
            Assert.False(failedUse);
            Assert.Equal(0, save.CareerRewindTokens);
        }
    }
}
