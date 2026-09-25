using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Comprehensive weekly financial snapshot and cash flow breakdown for the player.
    /// Pure C# domain model.
    /// </summary>
    public sealed record FinanceBreakdown
    {
        public decimal WeeklyWage { get; init; }
        public decimal EstimatedMatchBonuses { get; init; }
        public decimal SponsorshipIncome { get; init; }
        public decimal TotalWeeklyIncome => WeeklyWage + EstimatedMatchBonuses + SponsorshipIncome;

        public decimal ApartmentUpkeep { get; init; }
        public decimal LifestyleTierCost { get; init; }
        public decimal LifestyleItemsUpkeep { get; init; }
        public decimal TaxEstimate { get; init; }
        public decimal AgentCommission { get; init; }
        public decimal TotalWeeklyExpenses => ApartmentUpkeep + LifestyleTierCost + LifestyleItemsUpkeep + TaxEstimate + AgentCommission;

        public decimal NetWeeklyCashFlow => TotalWeeklyIncome - TotalWeeklyExpenses;
        public decimal CurrentBankBalance { get; init; }
        public decimal PropertyValue { get; init; }
        public decimal AssetsValue { get; init; }
        public decimal EstimatedNetWorth => CurrentBankBalance + PropertyValue + AssetsValue;

        public LifestyleTier CurrentTier { get; init; }
        public int TotalPrestige { get; init; }

        public static FinanceBreakdown Calculate(
            decimal weeklyWage,
            decimal bankBalance,
            LifestyleTier tier,
            HomeProperty property,
            IReadOnlyCollection<LifestyleItem> ownedItems,
            decimal matchBonusesRecent = 0m,
            decimal sponsorshipWeekly = 0m)
        {
            decimal aptUpkeep = property?.WeeklyUpkeep ?? 150m;
            decimal propValue = property?.PurchaseCost ?? 0m;
            decimal tierCost = tier switch
            {
                LifestyleTier.Modest => 150m,
                LifestyleTier.Comfortable => 500m,
                LifestyleTier.Luxurious => 2000m,
                LifestyleTier.Extravagant => 8000m,
                LifestyleTier.Superstar => 25000m,
                _ => 150m
            };

            decimal itemsUpkeep = 0m;
            decimal itemsValue = 0m;
            int itemsPrestige = 0;
            if (ownedItems != null)
            {
                foreach (var item in ownedItems)
                {
                    itemsUpkeep += item.WeeklyUpkeep;
                    itemsValue += item.Price;
                    itemsPrestige += item.PrestigeScore;
                }
            }

            decimal agentComm = weeklyWage * 0.05m; // 5% agent standard fee
            decimal taxEst = weeklyWage > 1000m ? (weeklyWage - 1000m) * 0.35m : 0m; // 35% tax on amount above threshold

            int totalPrestige = (property?.PrestigeScore ?? 10) + itemsPrestige;

            return new FinanceBreakdown
            {
                WeeklyWage = weeklyWage,
                EstimatedMatchBonuses = matchBonusesRecent,
                SponsorshipIncome = sponsorshipWeekly,
                ApartmentUpkeep = aptUpkeep,
                LifestyleTierCost = tierCost,
                LifestyleItemsUpkeep = itemsUpkeep,
                TaxEstimate = taxEst,
                AgentCommission = agentComm,
                CurrentBankBalance = bankBalance,
                PropertyValue = propValue,
                AssetsValue = itemsValue,
                CurrentTier = tier,
                TotalPrestige = totalPrestige
            };
        }
    }
}
