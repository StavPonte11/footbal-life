using System;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CareerSaveDataTests
    {
        [Fact]
        public void CareerSaveData_DefaultValues_AreValid()
        {
            var data = new CareerSaveData();

            Assert.Equal(CareerSaveData.CurrentSchemaVersion, data.SaveVersion);
            Assert.Equal("0.2.1", data.GameVersion);
            Assert.False(string.IsNullOrWhiteSpace(data.CreatedAt));
            Assert.False(string.IsNullOrWhiteSpace(data.LastSavedAt));
            Assert.Equal(65, data.OverallRating);
            Assert.Equal(100, data.Energy);
            Assert.Equal(70, data.Form);
            Assert.Equal(75, data.Morale);
            Assert.Equal(50, data.ManagerTrust);
        }

        [Fact]
        public void CareerSaveData_JsonSerializationRoundtrip_PreservesAllFields()
        {
            var original = new CareerSaveData
            {
                SlotIndex = 2,
                PlayerId = Guid.NewGuid(),
                PlayerName = "Marcus Vance",
                Nationality = "England",
                DateOfBirth = "2007-05-18",
                PrimaryPosition = "ST",
                PreferredFoot = "Right",
                ClubName = "Northfield Town",
                CurrentSeason = 2,
                CurrentWeek = 14,
                OverallRating = 72,
                BankBalance = 15400,
                WeeklyWage = 1200,
                ContractEndYear = 2029,
                MarketValue = 250000,
                SquadRole = "Regular",
                LifestyleTier = 2,
                Pace = 75,
                Acceleration = 78,
                Stamina = 68,
                Shooting = 74,
                Passing = 65,
                Dribbling = 72,
                TotalAppearances = 24,
                TotalGoals = 11,
                TotalAssists = 5,
                AverageRating = 7.15,
                MasterSeed = 999
            };

            original.Relationships.Add(new RelationshipSaveEntry
            {
                TargetName = "Coach Bennett",
                RelationshipType = "Manager",
                Affinity = 68,
                Respect = 80
            });

            string json = original.ToJson();
            Assert.False(string.IsNullOrWhiteSpace(json));

            var restored = CareerSaveData.FromJson(json);

            Assert.Equal(original.SlotIndex, restored.SlotIndex);
            Assert.Equal(original.PlayerId, restored.PlayerId);
            Assert.Equal(original.PlayerName, restored.PlayerName);
            Assert.Equal(original.Nationality, restored.Nationality);
            Assert.Equal(original.PrimaryPosition, restored.PrimaryPosition);
            Assert.Equal(original.PreferredFoot, restored.PreferredFoot);
            Assert.Equal(original.ClubName, restored.ClubName);
            Assert.Equal(original.CurrentSeason, restored.CurrentSeason);
            Assert.Equal(original.CurrentWeek, restored.CurrentWeek);
            Assert.Equal(original.OverallRating, restored.OverallRating);
            Assert.Equal(original.BankBalance, restored.BankBalance);
            Assert.Equal(original.WeeklyWage, restored.WeeklyWage);
            Assert.Equal(original.Pace, restored.Pace);
            Assert.Equal(original.Shooting, restored.Shooting);
            Assert.Equal(original.TotalGoals, restored.TotalGoals);
            Assert.Equal(original.AverageRating, restored.AverageRating);
            Assert.Single(restored.Relationships);
            Assert.Equal("Coach Bennett", restored.Relationships[0].TargetName);
            Assert.Equal("Manager", restored.Relationships[0].RelationshipType);
            Assert.Equal(68, restored.Relationships[0].Affinity);
            Assert.Equal(80, restored.Relationships[0].Respect);
        }

        [Fact]
        public void CareerSaveData_FromJson_ThrowsOnNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => CareerSaveData.FromJson(""));
            Assert.Throws<ArgumentException>(() => CareerSaveData.FromJson("   "));
        }
    }
}
