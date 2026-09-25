using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class SocialActivitySystemTests
    {
        [Fact]
        public void Catalog_ContainsCuratedOutingsAcrossAllCategories()
        {
            var all = SocialActivityCatalog.AllActivities;
            Assert.NotEmpty(all);
            Assert.True(all.Count >= 10);

            Assert.NotEmpty(SocialActivityCatalog.GetByCategory(SocialActivityCategory.Casual));
            Assert.NotEmpty(SocialActivityCatalog.GetByCategory(SocialActivityCategory.TeamBonding));
            Assert.NotEmpty(SocialActivityCatalog.GetByCategory(SocialActivityCategory.Nightlife));
            Assert.NotEmpty(SocialActivityCatalog.GetByCategory(SocialActivityCategory.Glamour));
            Assert.NotEmpty(SocialActivityCatalog.GetByCategory(SocialActivityCategory.Philanthropy));
        }

        [Fact]
        public void CanAffordActivity_WhenEnergyOrMoneyLow_ReturnsFalseWithReason()
        {
            var save = new CareerSaveData
            {
                Energy = 5,
                BankBalance = 1000
            };
            var activity = SocialActivityCatalog.GetById("act_vip_nightclub");
            Assert.NotNull(activity);

            bool canAffordEnergy = SocialActivitySystem.CanAffordActivity(save, activity, out string reasonEnergy);
            Assert.False(canAffordEnergy);
            Assert.Contains("exhausted", reasonEnergy, StringComparison.OrdinalIgnoreCase);

            save.Energy = 100;
            save.BankBalance = 10;
            bool canAffordMoney = SocialActivitySystem.CanAffordActivity(save, activity, out string reasonMoney);
            Assert.False(canAffordMoney);
            Assert.Contains("Insufficient funds", reasonMoney, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ExecuteActivity_ValidOuting_DeductsEnergyAndFunds_BoostsMorale()
        {
            var save = new CareerSaveData
            {
                Energy = 80,
                BankBalance = 5000,
                Morale = 60
            };
            var activity = SocialActivityCatalog.GetById("act_italian_bistro");
            Assert.NotNull(activity);

            var result = SocialActivitySystem.ExecuteActivity(save, activity, new DateOnly(2026, 9, 25));

            Assert.True(result.Success);
            Assert.Equal(80 - activity.EnergyCost, save.Energy);
            Assert.Equal(5000 - (int)activity.FinancialCost, save.BankBalance);
            Assert.True(save.Morale > 60);
            Assert.False(result.IncurredManagerDisapproval);
        }

        [Fact]
        public void ExecuteActivity_TeamBonding_BoostsTeammateAffinity()
        {
            var save = new CareerSaveData
            {
                Energy = 90,
                BankBalance = 1000,
                Relationships = new List<RelationshipSaveEntry>
                {
                    new() { TargetName = "Liam Walker", RelationshipType = "Teammate", Affinity = 50 },
                    new() { TargetName = "Elena Rostova", RelationshipType = "Manager", Affinity = 50 }
                }
            };
            var activity = SocialActivityCatalog.GetById("act_team_bowling");
            Assert.NotNull(activity);

            var result = SocialActivitySystem.ExecuteActivity(save, activity, new DateOnly(2026, 9, 25));

            Assert.True(result.Success);
            var teammate = save.Relationships.Find(r => r.TargetName == "Liam Walker");
            var manager = save.Relationships.Find(r => r.TargetName == "Elena Rostova");

            Assert.NotNull(teammate);
            Assert.NotNull(manager);
            Assert.True(teammate.Affinity > 50);
            Assert.Equal(50, manager.Affinity); // Non-teammates unaffected
        }

        [Fact]
        public void ExecuteActivity_NightlifeNearMatchday_TriggersManagerDisapprovalRisk()
        {
            var save = new CareerSaveData
            {
                Energy = 90,
                BankBalance = 5000,
                ManagerTrust = 70
            };
            var activity = SocialActivityCatalog.GetById("act_vip_nightclub");
            Assert.NotNull(activity);

            // Force random roll to trigger penalty
            var result = SocialActivitySystem.ExecuteActivity(
                save,
                activity,
                new DateOnly(2026, 9, 25),
                nearMatchday: true,
                forcedRandom: 0.10f); // Less than ManagerTrustRisk (0.45)

            Assert.True(result.Success);
            Assert.True(result.IncurredManagerDisapproval);
            Assert.True(save.ManagerTrust < 70);
            Assert.Contains("manager heard", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ExecuteActivity_Philanthropy_BoostsFanPopularityAndMediaReputation()
        {
            var save = new CareerSaveData
            {
                Energy = 90,
                BankBalance = 5000,
                FanPopularity = 40,
                MediaReputation = 40
            };
            var activity = SocialActivityCatalog.GetById("act_charity_gala");
            Assert.NotNull(activity);

            var result = SocialActivitySystem.ExecuteActivity(save, activity, new DateOnly(2026, 9, 25));

            Assert.True(result.Success);
            Assert.True(save.FanPopularity > 40);
            Assert.True(save.MediaReputation > 40);
        }
    }
}
