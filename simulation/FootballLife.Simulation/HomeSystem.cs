using System;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    public sealed record HomeUpgradeResult(
        bool Success,
        string Message,
        LifestyleTier NewTier,
        decimal CostDeducted,
        int RemainingBalance
    );

    public sealed record SleepRecoveryResult(
        int InitialEnergy,
        int RecoveredEnergy,
        int EnergyGained,
        float TierMultiplier
    );

    public sealed record HomeWorkoutResult(
        bool Success,
        string Message,
        int EnergyCost,
        int RemainingEnergy,
        int StaminaXpGained,
        int StrengthXpGained,
        float TierMultiplier
    );

    public enum WorkoutIntensity
    {
        Light,
        Moderate,
        Intense
    }

    /// <summary>
    /// Pure C# simulation system governing player home interactions, lifestyle upgrades,
    /// rest & sleep recovery, and home gym training sessions.
    /// Fully deterministic and decoupled from presentation.
    /// </summary>
    public static class HomeSystem
    {
        public const int MinimumEnergyForWorkout = 20;

        /// <summary>
        /// Checks whether the player has sufficient funds and weekly wage to support a target lifestyle tier.
        /// Rule: Bank balance >= purchase cost, and weekly wage >= 1.5x weekly upkeep.
        /// </summary>
        public static bool CanAffordUpgrade(int bankBalance, int weeklyWage, LifestyleTier targetTier, out string reason)
        {
            var prop = HomePropertyCatalog.GetProperty(targetTier);

            if (bankBalance < (int)prop.PurchaseCost)
            {
                reason = $"Insufficient funds. Purchase cost is £{prop.PurchaseCost:N0}, but you only have £{bankBalance:N0}.";
                return false;
            }

            decimal requiredWage = prop.WeeklyUpkeep * 1.5m;
            if (weeklyWage < (int)requiredWage)
            {
                reason = $"Weekly wage too low. An upkeep of £{prop.WeeklyUpkeep:N0}/wk requires at least £{requiredWage:N0}/wk wage to sustain safely.";
                return false;
            }

            reason = "Affordable";
            return true;
        }

        /// <summary>
        /// Executes a home upgrade, updating save data and deducting the purchase price.
        /// </summary>
        public static HomeUpgradeResult UpgradeHome(CareerSaveData save, LifestyleTier targetTier)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));

            var currentTier = (LifestyleTier)save.LifestyleTier;
            if (targetTier == currentTier)
            {
                return new HomeUpgradeResult(false, "Already residing in this property tier.", currentTier, 0m, save.BankBalance);
            }

            if (targetTier < currentTier)
            {
                // Downsizing: no purchase cost, updates tier
                save.LifestyleTier = (int)targetTier;
                return new HomeUpgradeResult(true, $"Downsized to {HomePropertyCatalog.GetProperty(targetTier).Name}.", targetTier, 0m, save.BankBalance);
            }

            if (!CanAffordUpgrade(save.BankBalance, save.WeeklyWage, targetTier, out var reason))
            {
                return new HomeUpgradeResult(false, reason, currentTier, 0m, save.BankBalance);
            }

            var prop = HomePropertyCatalog.GetProperty(targetTier);
            save.BankBalance -= (int)prop.PurchaseCost;
            save.LifestyleTier = (int)targetTier;

            // Passive morale boost from moving to a better home
            save.Morale = Math.Min(100, save.Morale + prop.WeeklyMoraleBonus + 3);

            return new HomeUpgradeResult(
                true,
                $"Successfully moved into {prop.Name}!",
                targetTier,
                prop.PurchaseCost,
                save.BankBalance
            );
        }

        /// <summary>
        /// Calculates sleep and rest recovery in the home bed.
        /// Higher property tiers and owned wellness equipment provide faster recovery and higher stamina ceilings.
        /// </summary>
        public static SleepRecoveryResult CalculateSleepRecovery(LifestyleTier tier, int currentEnergy, int hoursSlept = 8, float additionalRecoveryBonus = 0f)
        {
            var prop = HomePropertyCatalog.GetProperty(tier);
            hoursSlept = Math.Clamp(hoursSlept, 1, 12);

            // Base recovery: ~5 energy per hour of quality sleep, multiplied by property tier comfort + wellness equipment perks
            float multiplier = prop.RestRecoveryMultiplier + Math.Max(0f, additionalRecoveryBonus);
            float rawGained = hoursSlept * 5.0f * multiplier;
            int energyGained = Math.Max(5, (int)Math.Round(rawGained));

            int newEnergy = Math.Min(100, currentEnergy + energyGained);
            int actualGained = newEnergy - currentEnergy;

            return new SleepRecoveryResult(
                InitialEnergy: currentEnergy,
                RecoveredEnergy: newEnergy,
                EnergyGained: actualGained,
                TierMultiplier: multiplier
            );
        }

        /// <summary>
        /// Calculates a home workout training session.
        /// Generates physical attribute XP (Stamina, Strength) with home gym tier bonuses.
        /// </summary>
        public static HomeWorkoutResult ExecuteHomeWorkout(LifestyleTier tier, int currentEnergy, WorkoutIntensity intensity = WorkoutIntensity.Moderate)
        {
            if (currentEnergy < MinimumEnergyForWorkout)
            {
                return new HomeWorkoutResult(
                    Success: false,
                    Message: "Too fatigued for a home workout. Get some rest first.",
                    EnergyCost: 0,
                    RemainingEnergy: currentEnergy,
                    StaminaXpGained: 0,
                    StrengthXpGained: 0,
                    TierMultiplier: 1.0f
                );
            }

            var prop = HomePropertyCatalog.GetProperty(tier);

            int energyCost = intensity switch
            {
                WorkoutIntensity.Light => 15,
                WorkoutIntensity.Moderate => 25,
                WorkoutIntensity.Intense => 35,
                _ => 25
            };

            // Prevent energy from dropping below 5
            int actualCost = Math.Min(energyCost, Math.Max(0, currentEnergy - 5));
            int remainingEnergy = currentEnergy - actualCost;

            int baseXp = intensity switch
            {
                WorkoutIntensity.Light => 10,
                WorkoutIntensity.Moderate => 20,
                WorkoutIntensity.Intense => 35,
                _ => 20
            };

            int staminaXp = (int)Math.Round(baseXp * prop.GymWorkoutMultiplier);
            int strengthXp = (int)Math.Round(baseXp * 0.8f * prop.GymWorkoutMultiplier);

            return new HomeWorkoutResult(
                Success: true,
                Message: $"Completed {intensity} home workout in your {prop.Name} gym!",
                EnergyCost: actualCost,
                RemainingEnergy: remainingEnergy,
                StaminaXpGained: staminaXp,
                StrengthXpGained: strengthXp,
                TierMultiplier: prop.GymWorkoutMultiplier
            );
        }
    }
}
