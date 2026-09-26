using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Status result of a monetization purchase attempt.
    /// </summary>
    public enum PurchaseStatus
    {
        Success = 0,
        AlreadyOwned = 1,
        ProductNotFound = 2,
        Failed = 3
    }

    /// <summary>
    /// Result payload of an in-app product purchase or entitlement transaction.
    /// </summary>
    public sealed record PurchaseResult(
        PurchaseStatus Status,
        string Message,
        MonetizationProduct? Product
    );

    /// <summary>
    /// Pure C# monetization and entitlement service managing cosmetic unlockables,
    /// convenience tokens (Career Rewind Tokens), and store catalog logic.
    /// </summary>
    public sealed class MonetizationService
    {
        private static readonly Dictionary<string, MonetizationProduct> _catalog =
            new Dictionary<string, MonetizationProduct>(StringComparer.OrdinalIgnoreCase)
            {
                ["cosmetic.golden_boots"] = new MonetizationProduct(
                    ProductId: "cosmetic.golden_boots",
                    Title: "Golden Touch Boots",
                    Description: "Iconic metallic gold match boots with custom particle flair.",
                    Category: MonetizationCategory.Cosmetic,
                    PriceUsd: 2.99m,
                    Quantity: 1
                ),
                ["cosmetic.retro_kit"] = new MonetizationProduct(
                    ProductId: "cosmetic.retro_kit",
                    Title: "Classic 90s Kit Trim",
                    Description: "Vintage collar and sleeve piping for matchday kits.",
                    Category: MonetizationCategory.Cosmetic,
                    PriceUsd: 1.99m,
                    Quantity: 1
                ),
                ["cosmetic.penthouse_skyline"] = new MonetizationProduct(
                    ProductId: "cosmetic.penthouse_skyline",
                    Title: "Metropolitan Skyline Penthouse",
                    Description: "Floor-to-ceiling glass apartment overlooking the city stadium lights.",
                    Category: MonetizationCategory.Cosmetic,
                    PriceUsd: 4.99m,
                    Quantity: 1
                ),
                ["cosmetic.supercar_midnight"] = new MonetizationProduct(
                    ProductId: "cosmetic.supercar_midnight",
                    Title: "Midnight Stealth Supercar",
                    Description: "Matte black hypercar displayed outside your luxury residence.",
                    Category: MonetizationCategory.Cosmetic,
                    PriceUsd: 4.99m,
                    Quantity: 1
                ),
                ["token.career_rewind_1x"] = new MonetizationProduct(
                    ProductId: "token.career_rewind_1x",
                    Title: "Single Career Rewind Token",
                    Description: "Rewind 1 match situation or week to take another shot at destiny.",
                    Category: MonetizationCategory.ConvenienceToken,
                    PriceUsd: 0.99m,
                    Quantity: 1
                ),
                ["token.career_rewind_3x"] = new MonetizationProduct(
                    ProductId: "token.career_rewind_3x",
                    Title: "Triple Career Rewind Pack",
                    Description: "3 Career Rewind Tokens for crucial derby matches and cup finals.",
                    Category: MonetizationCategory.ConvenienceToken,
                    PriceUsd: 1.99m,
                    Quantity: 3
                ),
                ["token.career_rewind_10x"] = new MonetizationProduct(
                    ProductId: "token.career_rewind_10x",
                    Title: "Career Rewind Vault (10x)",
                    Description: "Vault of 10 Career Rewind Tokens at a 50% discount.",
                    Category: MonetizationCategory.ConvenienceToken,
                    PriceUsd: 4.99m,
                    Quantity: 10
                ),
                ["pass.season_scout_intel"] = new MonetizationProduct(
                    ProductId: "pass.season_scout_intel",
                    Title: "Global Scout Intel Pass",
                    Description: "Unlocks exact manager trust thresholds and hidden club wage budgets.",
                    Category: MonetizationCategory.CareerReplay,
                    PriceUsd: 3.99m,
                    Quantity: 1
                )
            };

        /// <summary>
        /// Retrieves all products available in the game store catalog.
        /// </summary>
        public IReadOnlyList<MonetizationProduct> GetCatalog()
        {
            return new List<MonetizationProduct>(_catalog.Values);
        }

        /// <summary>
        /// Audits the entire monetization catalog for store policy and anti-gambling compliance (#P7-801).
        /// </summary>
        public StoreComplianceReport AuditCompliance()
        {
            return StoreComplianceValidator.AuditCatalog(GetCatalog());
        }

        /// <summary>
        /// Retrieves a product by its unique product identifier.
        /// </summary>
        public MonetizationProduct? GetProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;
            return _catalog.TryGetValue(productId, out var prod) ? prod : null;
        }

        /// <summary>
        /// Validates and grants an in-app product purchase to the career save state.
        /// </summary>
        public PurchaseResult PurchaseProduct(string productId, CareerSaveData save, string transactionId = "")
        {
            if (save == null) throw new ArgumentNullException(nameof(save));

            var product = GetProduct(productId);
            if (product == null)
            {
                return new PurchaseResult(PurchaseStatus.ProductNotFound, $"Product '{productId}' not found.", null);
            }

            if (product.Category == MonetizationCategory.Cosmetic)
            {
                if (save.OwnedCosmeticIds == null)
                {
                    save.OwnedCosmeticIds = new List<string>();
                }

                if (save.OwnedCosmeticIds.Contains(product.ProductId))
                {
                    return new PurchaseResult(PurchaseStatus.AlreadyOwned, $"Cosmetic '{product.Title}' is already owned.", product);
                }

                save.OwnedCosmeticIds.Add(product.ProductId);
                return new PurchaseResult(PurchaseStatus.Success, $"Successfully unlocked '{product.Title}'.", product);
            }

            if (product.Category == MonetizationCategory.ConvenienceToken)
            {
                save.CareerRewindTokens += product.Quantity;
                return new PurchaseResult(PurchaseStatus.Success, $"Added +{product.Quantity} Career Rewind Tokens.", product);
            }

            if (product.Category == MonetizationCategory.CareerReplay)
            {
                if (save.OwnedCosmeticIds == null) save.OwnedCosmeticIds = new List<string>();
                if (!save.OwnedCosmeticIds.Contains(product.ProductId))
                {
                    save.OwnedCosmeticIds.Add(product.ProductId);
                }
                return new PurchaseResult(PurchaseStatus.Success, $"Unlocked '{product.Title}'.", product);
            }

            return new PurchaseResult(PurchaseStatus.Failed, "Unknown product category.", product);
        }

        /// <summary>
        /// Consumes a single Career Rewind Token from player inventory.
        /// </summary>
        public bool ConsumeRewindToken(CareerSaveData save)
        {
            if (save == null) return false;
            if (save.CareerRewindTokens <= 0) return false;

            save.CareerRewindTokens--;
            return true;
        }

        /// <summary>
        /// Checks whether the player owns a specific cosmetic or entitlement.
        /// </summary>
        public bool HasEntitlement(string productId, CareerSaveData save)
        {
            if (save == null || string.IsNullOrEmpty(productId)) return false;
            return save.OwnedCosmeticIds != null && save.OwnedCosmeticIds.Contains(productId);
        }
    }
}
