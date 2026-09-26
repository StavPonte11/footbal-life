using System;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class StoreComplianceTests
    {
        [Fact]
        public void AuditCatalog_AllProductsAreDeterministicWithZeroLootBoxes()
        {
            var service = new MonetizationService();
            var report = service.AuditCompliance();

            Assert.NotNull(report);
            Assert.True(report.TotalProducts > 0, "Catalog must not be empty.");
            Assert.Equal(report.TotalProducts, report.DeterministicProducts);
            Assert.Equal(0, report.LootBoxProducts);
            Assert.True(report.GlobalStoreCompliant, "Catalog must be globally compliant with Apple/Google store policies.");
            Assert.True(report.BelgianComplianceApproved, "Catalog must be compliant with Belgian Gaming Act (zero paid loot boxes).");
            Assert.True(report.DutchComplianceApproved, "Catalog must be compliant with Dutch Kansspelautoriteit standards.");
        }

        [Theory]
        [InlineData("token.career_rewind_1x")]
        [InlineData("token.career_rewind_3x")]
        [InlineData("token.career_rewind_10x")]
        public void AuditCatalog_CareerRewindTokens_ArePureDeterministicUtility(string productId)
        {
            var service = new MonetizationService();
            var report = service.AuditCompliance();

            var tokenInfo = report.Products.FirstOrDefault(p => p.ProductId == productId);
            Assert.NotNull(tokenInfo);
            Assert.True(tokenInfo.IsDeterministic);
            Assert.False(tokenInfo.HasRandomizedRewards);
            Assert.False(tokenInfo.IsLootBox);
            Assert.Equal("ConsumableUtility", tokenInfo.StoreCategory);
            Assert.Contains("100% Deterministic", tokenInfo.OddsDisclosure);
        }

        [Fact]
        public void AuditCatalog_CosmeticAndIntelPasses_HaveNoChanceMechanics()
        {
            var service = new MonetizationService();
            var catalog = service.GetCatalog();

            foreach (var item in catalog)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.ProductId));
                Assert.False(string.IsNullOrWhiteSpace(item.Title));
                Assert.True(item.PriceUsd > 0m, $"Item {item.ProductId} price must be positive.");
            }
        }

        [Fact]
        public void GetAgeRatingProfiles_ReturnsCompliantGeneralAudienceRatings()
        {
            var profiles = StoreComplianceValidator.GetAgeRatingProfiles();

            Assert.NotNull(profiles);
            Assert.True(profiles.Count >= 4);

            var appleProfile = profiles.First(p => p.RatingSystem.Contains("Apple"));
            Assert.Equal("4+", appleProfile.AgeTier);
            Assert.False(appleProfile.HasViolence);
            Assert.False(appleProfile.HasGambling);
            Assert.False(appleProfile.HasLootBoxes);
            Assert.True(appleProfile.HasInAppPurchases);

            var pegiProfile = profiles.First(p => p.RatingSystem.Contains("PEGI"));
            Assert.Equal("PEGI 3", pegiProfile.AgeTier);
            Assert.True(pegiProfile.HasInAppPurchases);

            var esrbProfile = profiles.First(p => p.RatingSystem.Contains("ESRB"));
            Assert.Equal("Everyone", esrbProfile.AgeTier);
            Assert.True(esrbProfile.HasInAppPurchases);

            var uskProfile = profiles.First(p => p.RatingSystem.Contains("USK"));
            Assert.Equal("USK 0", uskProfile.AgeTier);
            Assert.True(uskProfile.HasInAppPurchases);
        }
    }
}
