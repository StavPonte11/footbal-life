using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Result structure for an attempted lifestyle shop purchase.
    /// </summary>
    public sealed record LifestylePurchaseResult
    {
        public bool Success { get; init; }
        public FinanceAccount Account { get; init; } = FinanceAccount.Create();
        public IReadOnlyList<string> OwnedItemIds { get; init; } = Array.Empty<string>();
        public float MoraleDelta { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>
    /// Pure C# stateless simulation system managing the lifestyle shop,
    /// catalog purchases, inventory ownership, upkeep aggregation, and passive perks.
    /// </summary>
    public static class LifestyleShopSystem
    {
        /// <summary>
        /// Validates whether the player's bank account has sufficient funds to purchase an item.
        /// </summary>
        public static bool CanAffordItem(FinanceAccount account, LifestyleItem item)
        {
            if (account == null || item == null) return false;
            return account.Balance >= item.Price;
        }

        /// <summary>
        /// Attempts to purchase an item from the lifestyle shop.
        /// Deducts purchase price as a FinanceTransaction, appends item ID to inventory,
        /// and returns updated account and inventory state.
        /// </summary>
        public static LifestylePurchaseResult PurchaseItem(
            FinanceAccount account,
            IReadOnlyList<string> ownedItemIds,
            LifestyleItem item,
            DateOnly date)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (item == null) throw new ArgumentNullException(nameof(item));

            var currentInventory = ownedItemIds != null ? new List<string>(ownedItemIds) : new List<string>();

            // Duplicate check
            if (currentInventory.Contains(item.Id))
            {
                return new LifestylePurchaseResult
                {
                    Success = false,
                    Account = account,
                    OwnedItemIds = currentInventory,
                    MoraleDelta = 0f,
                    Message = $"You already own {item.Name}."
                };
            }

            // Funds check
            if (account.Balance < item.Price)
            {
                return new LifestylePurchaseResult
                {
                    Success = false,
                    Account = account,
                    OwnedItemIds = currentInventory,
                    MoraleDelta = 0f,
                    Message = $"Insufficient funds. {item.Name} costs £{item.Price:N0}, but your balance is £{account.Balance:N0}."
                };
            }

            // Create transaction
            var tx = new FinanceTransaction(
                Guid.NewGuid(),
                date,
                TransactionType.LifestyleExpense,
                -item.Price,
                $"Lifestyle Purchase: {item.Name} (£{item.Price:N0})");

            var updatedAccount = account.WithTransaction(tx);
            currentInventory.Add(item.Id);

            return new LifestylePurchaseResult
            {
                Success = true,
                Account = updatedAccount,
                OwnedItemIds = currentInventory,
                MoraleDelta = item.MoralePerk,
                Message = $"Purchased {item.Name} for £{item.Price:N0}! Morale +{item.MoralePerk:0.0}."
            };
        }

        /// <summary>
        /// Calculates total weekly upkeep for all owned items.
        /// </summary>
        public static decimal CalculateTotalItemUpkeep(IEnumerable<string> ownedItemIds)
        {
            if (ownedItemIds == null) return 0m;

            decimal total = 0m;
            foreach (var id in ownedItemIds)
            {
                var item = LifestyleCatalog.GetItem(id);
                if (item != null)
                {
                    total += item.WeeklyUpkeep;
                }
            }
            return total;
        }

        /// <summary>
        /// Aggregates passive perk bonuses from all owned lifestyle items.
        /// </summary>
        public static (float TotalMoraleBonus, float TotalEnergyRecoveryBonus, int TotalPrestige) CalculateTotalPerks(IEnumerable<string> ownedItemIds)
        {
            if (ownedItemIds == null) return (0f, 0f, 0);

            float morale = 0f;
            float energy = 0f;
            int prestige = 0;

            foreach (var id in ownedItemIds)
            {
                var item = LifestyleCatalog.GetItem(id);
                if (item != null)
                {
                    morale += item.MoralePerk;
                    energy += item.EnergyRecoveryPerk;
                    prestige += item.PrestigeScore;
                }
            }

            return (morale, energy, prestige);
        }

        /// <summary>
        /// Resolves owned LifestyleItem objects from an ID collection.
        /// </summary>
        public static IReadOnlyList<LifestyleItem> ResolveOwnedItems(IEnumerable<string> ownedItemIds)
        {
            if (ownedItemIds == null) return Array.Empty<LifestyleItem>();

            var list = new List<LifestyleItem>();
            foreach (var id in ownedItemIds)
            {
                var item = LifestyleCatalog.GetItem(id);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
