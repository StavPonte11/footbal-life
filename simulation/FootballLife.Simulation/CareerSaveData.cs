using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballLife.Simulation.Persistence
{
    /// <summary>
    /// Root serializable data transfer object capturing complete player career state.
    /// Pure C# domain-compatible data contract.
    /// </summary>
    public sealed class CareerSaveData
    {
        public const int CurrentSchemaVersion = 1;
        public const string CurrentGameVersion = "0.2.1";

        public int SaveVersion { get; set; } = CurrentSchemaVersion;
        public string GameVersion { get; set; } = CurrentGameVersion;
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string LastSavedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public int SlotIndex { get; set; }

        // Summary fields for fast slot inspection
        public string PlayerName { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;
        public int CurrentSeason { get; set; } = 1;
        public int CurrentWeek { get; set; } = 1;
        public int OverallRating { get; set; } = 65;
        public int BankBalance { get; set; } = 0;
        public int MasterSeed { get; set; } = 42;

        // Player identity
        public Guid PlayerId { get; set; }
        public string Nationality { get; set; } = "England";
        public string DateOfBirth { get; set; } = "2008-01-01";
        public string PreferredFoot { get; set; } = "Right";
        public string PrimaryPosition { get; set; } = "ST";
        public string? SecondaryPosition { get; set; }

        // Abilities (0-100)
        public byte Pace { get; set; } = 60;
        public byte Acceleration { get; set; } = 60;
        public byte Stamina { get; set; } = 60;
        public byte Strength { get; set; } = 60;
        public byte Agility { get; set; } = 60;
        public byte Passing { get; set; } = 60;
        public byte Shooting { get; set; } = 60;
        public byte Dribbling { get; set; } = 60;
        public byte Crossing { get; set; } = 60;
        public byte FirstTouch { get; set; } = 60;
        public byte Tackling { get; set; } = 60;
        public byte Vision { get; set; } = 60;
        public byte Composure { get; set; } = 60;
        public byte Positioning { get; set; } = 60;
        public byte DecisionMaking { get; set; } = 60;

        // Dynamic State
        public int Energy { get; set; } = 100;
        public int Form { get; set; } = 70;
        public int Morale { get; set; } = 75;
        public int ManagerTrust { get; set; } = 50;

        // Contract & Finances
        public int WeeklyWage { get; set; } = 500;
        public int ContractEndYear { get; set; } = 2028;
        public int MarketValue { get; set; } = 50000;
        public string SquadRole { get; set; } = "Prospect";
        public int LifestyleTier { get; set; } = 1;

        // Statistical Tracking
        public int TotalAppearances { get; set; }
        public int TotalGoals { get; set; }
        public int TotalAssists { get; set; }
        public double AverageRating { get; set; } = 6.5;

        // Relationships
        public List<RelationshipSaveEntry> Relationships { get; set; } = new List<RelationshipSaveEntry>();

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true
        };

        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        public static CareerSaveData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Save data JSON cannot be null or empty.", nameof(json));

            var result = JsonSerializer.Deserialize<CareerSaveData>(json, JsonOptions);
            return result ?? throw new InvalidOperationException("Failed to deserialize CareerSaveData.");
        }
    }

    public sealed class RelationshipSaveEntry
    {
        public string TargetName { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = "Teammate";
        public int Affinity { get; set; } = 50;
        public int Respect { get; set; } = 50;
    }

    public readonly struct SaveSlotSummary
    {
        public int SlotIndex { get; }
        public bool Exists { get; }
        public string PlayerName { get; }
        public string ClubName { get; }
        public int Season { get; }
        public int Week { get; }
        public int OverallRating { get; }
        public int BankBalance { get; }
        public DateTime LastSavedAt { get; }

        public SaveSlotSummary(
            int slotIndex,
            bool exists,
            string playerName = "",
            string clubName = "",
            int season = 1,
            int week = 1,
            int overallRating = 0,
            int bankBalance = 0,
            DateTime lastSavedAt = default)
        {
            SlotIndex = slotIndex;
            Exists = exists;
            PlayerName = playerName;
            ClubName = clubName;
            Season = season;
            Week = week;
            OverallRating = overallRating;
            BankBalance = bankBalance;
            LastSavedAt = lastSavedAt;
        }

        public static SaveSlotSummary Empty(int slotIndex) => new SaveSlotSummary(slotIndex, false);
    }
}
