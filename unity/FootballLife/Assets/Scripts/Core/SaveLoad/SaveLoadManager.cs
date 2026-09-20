using System;
using System.Collections.Generic;
using System.IO;
using FootballLife.Simulation.Persistence;
using UnityEngine;

namespace FootballLife.Unity.Core.SaveLoad
{
    /// <summary>
    /// Unity presentation adapter managing disk persistence, backups, and slot querying for career saves.
    /// Integrates CareerSaveService with Unity's Application.persistentDataPath.
    /// </summary>
    public sealed class SaveLoadManager
    {
        public const int AutoSaveSlot = 0;
        public const int MaxManualSlots = 3;

        private readonly string _saveDirectory;

        public string SaveDirectory => _saveDirectory;

        public SaveLoadManager(string? customSaveDirectory = null)
        {
            if (!string.IsNullOrWhiteSpace(customSaveDirectory))
            {
                _saveDirectory = customSaveDirectory;
            }
            else
            {
                _saveDirectory = Path.Combine(Application.persistentDataPath, "saves");
            }

            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public string GetSlotFilePath(int slotIndex) => Path.Combine(_saveDirectory, $"save_slot_{slotIndex}.json");
        public string GetBackupFilePath(int slotIndex) => Path.Combine(_saveDirectory, $"save_slot_{slotIndex}.json.bak");
        public string GetTempFilePath(int slotIndex) => Path.Combine(_saveDirectory, $"save_slot_{slotIndex}.json.tmp");

        /// <summary>
        /// Saves career state atomically to disk via CareerSaveService.
        /// </summary>
        public bool SaveCareer(int slotIndex, CareerSaveData data)
        {
            if (slotIndex < 0 || slotIndex > MaxManualSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Slot index must be between 0 (AutoSave) and {MaxManualSlots}.");

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            EnsureDirectoryExists();
            data.SlotIndex = slotIndex;

            string targetPath = GetSlotFilePath(slotIndex);
            return CareerSaveService.SaveToFile(targetPath, data, msg => Debug.LogWarning($"[SaveLoadManager] {msg}"));
        }

        /// <summary>
        /// Loads career state from disk, falling back to backup file if primary is corrupted or missing.
        /// </summary>
        public CareerSaveData? LoadCareer(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > MaxManualSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            string targetPath = GetSlotFilePath(slotIndex);
            return CareerSaveService.LoadFromFile(targetPath, msg => Debug.LogWarning($"[SaveLoadManager] {msg}"));
        }

        /// <summary>
        /// Retrieves summaries of all save slots (AutoSave + manual slots 1-3).
        /// </summary>
        public IReadOnlyList<SaveSlotSummary> GetSaveSlots()
        {
            EnsureDirectoryExists();
            var summaries = new List<SaveSlotSummary>(MaxManualSlots + 1);

            for (int i = 0; i <= MaxManualSlots; i++)
            {
                string targetPath = GetSlotFilePath(i);
                summaries.Add(CareerSaveService.GetSlotSummary(targetPath, i));
            }

            return summaries;
        }

        /// <summary>
        /// Deletes all files associated with a save slot.
        /// </summary>
        public bool DeleteCareer(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > MaxManualSlots)
                return false;

            string targetPath = GetSlotFilePath(slotIndex);
            return CareerSaveService.DeleteFile(targetPath);
        }
    }
}
