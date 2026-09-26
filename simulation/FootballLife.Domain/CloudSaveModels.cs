using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Strategy for resolving desynchronization between local save and remote cloud save.
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        KeepNewest = 0,
        KeepLocal = 1,
        KeepCloud = 2
    }

    /// <summary>
    /// Lightweight header metadata describing a cloud save payload for fast comparison.
    /// </summary>
    public sealed record CloudSaveMetadata(
        string SlotKey,
        string PlayerName,
        string ClubName,
        int Season,
        int Week,
        int OverallRating,
        DateTime LastSavedUtc,
        string DataHash
    )
    {
        public static CloudSaveMetadata Empty(string slotKey) => new CloudSaveMetadata(
            SlotKey: slotKey ?? string.Empty,
            PlayerName: string.Empty,
            ClubName: string.Empty,
            Season: 1,
            Week: 1,
            OverallRating: 0,
            LastSavedUtc: DateTime.MinValue,
            DataHash: string.Empty
        );
    }

    /// <summary>
    /// Represents an active desynchronization conflict between local and cloud save state.
    /// </summary>
    public sealed record CloudSaveConflict(
        CloudSaveMetadata Local,
        CloudSaveMetadata Remote
    );
}
