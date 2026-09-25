using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class HomeSystemTests
    {
        [Fact]
        public void HomePropertyCatalog_ContainsAllLifestyleTiers()
        {
            var tiers = Enum.GetValues<LifestyleTier>();
            foreach (var tier in tiers)
            {
                var prop = HomePropertyCatalog.GetProperty(tier);
                Assert.NotNull(prop);
                Assert.Equal(tier, prop.Tier);
                Assert.False(string.IsNullOrWhiteSpace(prop.Name));
                Assert.True(prop.WeeklyUpkeep > 0m);
                Assert.True(prop.RestRecoveryMultiplier >= 1.0f);
                Assert.True(prop.GymWorkoutMultiplier >= 1.0f);
            }
        }

        [Fact]
        public void HomePropertyCatalog_MultipliersScaleWithTier()
        {
            var modest = HomePropertyCatalog.GetProperty(LifestyleTier.Modest);
            var comfortable = HomePropertyCatalog.GetProperty(LifestyleTier.Comfortable);
            var luxurious = HomePropertyCatalog.GetProperty(LifestyleTier.Luxurious);
            var superstar = HomePropertyCatalog.GetProperty(LifestyleTier.Superstar);

            Assert.True(comfortable.RestRecoveryMultiplier > modest.RestRecoveryMultiplier);
            Assert.True(luxurious.RestRecoveryMultiplier > comfortable.RestRecoveryMultiplier);
            Assert.True(superstar.RestRecoveryMultiplier > luxurious.RestRecoveryMultiplier);

            Assert.True(superstar.GymWorkoutMultiplier > modest.GymWorkoutMultiplier);
            Assert.True(superstar.WeeklyUpkeep > modest.WeeklyUpkeep);
        }

        [Theory]
        [InlineData(1000, 500, LifestyleTier.Comfortable, false)] // Insufficient balance (needs 15,000)
        [InlineData(20000, 400, LifestyleTier.Comfortable, false)] // Wage too low (needs 500 * 1.5 = 750)
        [InlineData(20000, 1000, LifestyleTier.Comfortable, true)] // Can afford
        public void CanAffordUpgrade_ValidatesBalanceAndWage(int balance, int wage, LifestyleTier tier, bool expectedAffordable)
        {
            bool canAfford = HomeSystem.CanAffordUpgrade(balance, wage, tier, out var reason);
            Assert.Equal(expectedAffordable, canAfford);
            Assert.NotNull(reason);
        }

        [Fact]
        public void UpgradeHome_SuccessfulUpgrade_DeductsFundsAndUpdatesTier()
        {
            var save = new CareerSaveData
            {
                LifestyleTier = (int)LifestyleTier.Modest,
                BankBalance = 30000,
                WeeklyWage = 1500,
                Morale = 60
            };

            var result = HomeSystem.UpgradeHome(save, LifestyleTier.Comfortable);

            Assert.True(result.Success);
            Assert.Equal(LifestyleTier.Comfortable, result.NewTier);
            Assert.Equal(15000m, result.CostDeducted);
            Assert.Equal(15000, save.BankBalance);
            Assert.Equal((int)LifestyleTier.Comfortable, save.LifestyleTier);
            Assert.True(save.Morale > 60);
        }

        [Fact]
        public void UpgradeHome_InsufficientFunds_FailsCleanly()
        {
            var save = new CareerSaveData
            {
                LifestyleTier = (int)LifestyleTier.Modest,
                BankBalance = 5000, // Needs 15,000
                WeeklyWage = 1500
            };

            var result = HomeSystem.UpgradeHome(save, LifestyleTier.Comfortable);

            Assert.False(result.Success);
            Assert.Equal(5000, save.BankBalance);
            Assert.Equal((int)LifestyleTier.Modest, save.LifestyleTier);
        }

        [Fact]
        public void UpgradeHome_Downsizing_SucceedsWithoutChargingCost()
        {
            var save = new CareerSaveData
            {
                LifestyleTier = (int)LifestyleTier.Luxurious,
                BankBalance = 10000,
                WeeklyWage = 1000
            };

            var result = HomeSystem.UpgradeHome(save, LifestyleTier.Comfortable);

            Assert.True(result.Success);
            Assert.Equal(LifestyleTier.Comfortable, result.NewTier);
            Assert.Equal(0m, result.CostDeducted);
            Assert.Equal(10000, save.BankBalance);
            Assert.Equal((int)LifestyleTier.Comfortable, save.LifestyleTier);
        }

        [Fact]
        public void CalculateSleepRecovery_RestoresEnergyAndCapsAt100()
        {
            var result = HomeSystem.CalculateSleepRecovery(LifestyleTier.Modest, currentEnergy: 50, hoursSlept: 8);
            Assert.True(result.RecoveredEnergy > 50);
            Assert.True(result.EnergyGained > 0);

            // Cap test
            var maxResult = HomeSystem.CalculateSleepRecovery(LifestyleTier.Superstar, currentEnergy: 95, hoursSlept: 8);
            Assert.Equal(100, maxResult.RecoveredEnergy);
            Assert.Equal(5, maxResult.EnergyGained);
        }

        [Fact]
        public void CalculateSleepRecovery_HigherTierRecoversFaster()
        {
            var modest = HomeSystem.CalculateSleepRecovery(LifestyleTier.Modest, currentEnergy: 30, hoursSlept: 6);
            var penthouse = HomeSystem.CalculateSleepRecovery(LifestyleTier.Luxurious, currentEnergy: 30, hoursSlept: 6);

            Assert.True(penthouse.EnergyGained > modest.EnergyGained);
            Assert.True(penthouse.RecoveredEnergy > modest.RecoveredEnergy);
        }

        [Fact]
        public void ExecuteHomeWorkout_FailsWhenTooFatigued()
        {
            var result = HomeSystem.ExecuteHomeWorkout(LifestyleTier.Modest, currentEnergy: 10, WorkoutIntensity.Moderate);

            Assert.False(result.Success);
            Assert.Equal(0, result.StaminaXpGained);
            Assert.Equal(10, result.RemainingEnergy);
        }

        [Fact]
        public void ExecuteHomeWorkout_SucceedsAndGrantsTierScaledXp()
        {
            var modest = HomeSystem.ExecuteHomeWorkout(LifestyleTier.Modest, currentEnergy: 80, WorkoutIntensity.Moderate);
            var villa = HomeSystem.ExecuteHomeWorkout(LifestyleTier.Extravagant, currentEnergy: 80, WorkoutIntensity.Moderate);

            Assert.True(modest.Success);
            Assert.True(villa.Success);
            Assert.True(villa.StaminaXpGained > modest.StaminaXpGained);
            Assert.True(villa.StrengthXpGained > modest.StrengthXpGained);
            Assert.True(villa.RemainingEnergy < 80);
        }
    }
}
