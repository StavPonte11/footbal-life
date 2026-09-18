using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Financial performance incentives attached to a player's employment contract.
    /// </summary>
    public sealed record ContractBonuses
    {
        public decimal GoalBonus { get; init; }
        public decimal AssistBonus { get; init; }
        public decimal AppearanceBonus { get; init; }
        public decimal CleanSheetBonus { get; init; }

        public ContractBonuses(
            decimal goalBonus = 0m,
            decimal assistBonus = 0m,
            decimal appearanceBonus = 0m,
            decimal cleanSheetBonus = 0m)
        {
            if (goalBonus < 0m) throw new ArgumentOutOfRangeException(nameof(goalBonus), "Goal bonus cannot be negative.");
            if (assistBonus < 0m) throw new ArgumentOutOfRangeException(nameof(assistBonus), "Assist bonus cannot be negative.");
            if (appearanceBonus < 0m) throw new ArgumentOutOfRangeException(nameof(appearanceBonus), "Appearance bonus cannot be negative.");
            if (cleanSheetBonus < 0m) throw new ArgumentOutOfRangeException(nameof(cleanSheetBonus), "Clean sheet bonus cannot be negative.");

            GoalBonus = goalBonus;
            AssistBonus = assistBonus;
            AppearanceBonus = appearanceBonus;
            CleanSheetBonus = cleanSheetBonus;
        }

        public static ContractBonuses Zero => new ContractBonuses(0m, 0m, 0m, 0m);

        /// <summary>
        /// Computes total earned bonuses from performance metrics.
        /// </summary>
        public decimal CalculateTotal(int appearances, int goals = 0, int assists = 0, bool cleanSheet = false)
        {
            decimal total = 0m;
            if (appearances > 0) total += appearances * AppearanceBonus;
            if (goals > 0) total += goals * GoalBonus;
            if (assists > 0) total += assists * AssistBonus;
            if (cleanSheet) total += CleanSheetBonus;
            return total;
        }
    }
}
