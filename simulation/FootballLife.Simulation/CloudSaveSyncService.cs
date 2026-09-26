using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Status outcome of a cloud synchronization attempt.
    /// </summary>
    public enum CloudSyncStatus
    {
        InSync = 0,
        Uploaded = 1,
        Downloaded = 2,
        ConflictResolved = 3,
        Offline = 4,
        Failed = 5
    }

    /// <summary>
    /// Result payload of a cloud synchronization execution.
    /// </summary>
    public sealed record CloudSyncResult(
        CloudSyncStatus Status,
        string? ResolvedJson,
        CloudSaveConflict? Conflict,
        string Message
    );

    /// <summary>
    /// Contract for backend cloud save storage providers (UGS Cloud Save, REST, Emulated).
    /// </summary>
    public interface ICloudSaveProvider
    {
        bool IsOffline { get; set; }
        Task<bool> SaveAsync(string slotKey, string jsonPayload, CloudSaveMetadata metadata);
        Task<string?> LoadAsync(string slotKey);
        Task<CloudSaveMetadata?> GetMetadataAsync(string slotKey);
        Task<IReadOnlyList<CloudSaveMetadata>> ListSlotsAsync();
    }

    /// <summary>
    /// In-memory emulated cloud save provider for deterministic testing and offline simulation.
    /// </summary>
    public sealed class EmulatedCloudSaveProvider : ICloudSaveProvider
    {
        private readonly Dictionary<string, (string Json, CloudSaveMetadata Meta)> _store =
            new Dictionary<string, (string, CloudSaveMetadata)>();

        public bool IsOffline { get; set; } = false;

        public Task<bool> SaveAsync(string slotKey, string jsonPayload, CloudSaveMetadata metadata)
        {
            if (IsOffline) return Task.FromResult(false);
            _store[slotKey] = (jsonPayload, metadata);
            return Task.FromResult(true);
        }

        public Task<string?> LoadAsync(string slotKey)
        {
            if (IsOffline) return Task.FromResult<string?>(null);
            if (_store.TryGetValue(slotKey, out var tuple))
            {
                return Task.FromResult<string?>(tuple.Json);
            }
            return Task.FromResult<string?>(null);
        }

        public Task<CloudSaveMetadata?> GetMetadataAsync(string slotKey)
        {
            if (IsOffline) return Task.FromResult<CloudSaveMetadata?>(null);
            if (_store.TryGetValue(slotKey, out var tuple))
            {
                return Task.FromResult<CloudSaveMetadata?>(tuple.Meta);
            }
            return Task.FromResult<CloudSaveMetadata?>(null);
        }

        public Task<IReadOnlyList<CloudSaveMetadata>> ListSlotsAsync()
        {
            if (IsOffline) return Task.FromResult<IReadOnlyList<CloudSaveMetadata>>(Array.Empty<CloudSaveMetadata>());
            var list = new List<CloudSaveMetadata>();
            foreach (var kvp in _store)
            {
                list.Add(kvp.Value.Meta);
            }
            return Task.FromResult<IReadOnlyList<CloudSaveMetadata>>(list);
        }

        public void Clear() => _store.Clear();
    }

    /// <summary>
    /// Pure C# orchestrator for cloud save synchronization, integrity hashing, and conflict resolution.
    /// </summary>
    public sealed class CloudSaveSyncService
    {
        /// <summary>
        /// Computes cryptographic SHA256 content hash of serialized save payload.
        /// </summary>
        public string ComputeDataHash(string content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates lightweight metadata header for a save payload.
        /// </summary>
        public CloudSaveMetadata CreateMetadata(string slotKey, CareerSaveData save, string content)
        {
            DateTime lastSaved = DateTime.UtcNow;
            if (DateTime.TryParse(save.LastSavedAt, out var parsed))
            {
                lastSaved = parsed.ToUniversalTime();
            }

            return new CloudSaveMetadata(
                SlotKey: slotKey,
                PlayerName: save.PlayerName,
                ClubName: save.ClubName,
                Season: save.CurrentSeason,
                Week: save.CurrentWeek,
                OverallRating: save.OverallRating,
                LastSavedUtc: lastSaved,
                DataHash: ComputeDataHash(content)
            );
        }

        /// <summary>
        /// Synchronizes local save state with remote cloud provider.
        /// Resolves conflicts deterministically using the specified strategy.
        /// </summary>
        public async Task<CloudSyncResult> SyncSlotAsync(
            string slotKey,
            string localJson,
            CareerSaveData localSave,
            ICloudSaveProvider provider,
            ConflictResolutionStrategy strategy = ConflictResolutionStrategy.KeepNewest)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (provider.IsOffline)
            {
                return new CloudSyncResult(CloudSyncStatus.Offline, localJson, null, "Cloud provider is currently offline.");
            }

            var localMeta = CreateMetadata(slotKey, localSave, localJson);
            var remoteMeta = await provider.GetMetadataAsync(slotKey);

            // Case 1: No remote save exists yet -> Upload local
            if (remoteMeta == null)
            {
                bool saved = await provider.SaveAsync(slotKey, localJson, localMeta);
                return saved
                    ? new CloudSyncResult(CloudSyncStatus.Uploaded, localJson, null, "Save uploaded to cloud.")
                    : new CloudSyncResult(CloudSyncStatus.Failed, localJson, null, "Failed to upload save to cloud.");
            }

            // Case 2: Hashes match -> Already in sync
            if (string.Equals(localMeta.DataHash, remoteMeta.DataHash, StringComparison.OrdinalIgnoreCase))
            {
                return new CloudSyncResult(CloudSyncStatus.InSync, localJson, null, "Local and cloud saves are identical.");
            }

            // Case 3: Conflict detected (diverged saves)
            var conflict = new CloudSaveConflict(localMeta, remoteMeta);
            string? remoteJson = await provider.LoadAsync(slotKey);

            if (remoteJson == null)
            {
                // Fallback to uploading local if remote payload cannot be read
                await provider.SaveAsync(slotKey, localJson, localMeta);
                return new CloudSyncResult(CloudSyncStatus.Uploaded, localJson, conflict, "Remote payload missing; re-uploaded local.");
            }

            // Apply resolution strategy
            switch (strategy)
            {
                case ConflictResolutionStrategy.KeepLocal:
                    await provider.SaveAsync(slotKey, localJson, localMeta);
                    return new CloudSyncResult(CloudSyncStatus.ConflictResolved, localJson, conflict, "Conflict resolved: Kept local save.");

                case ConflictResolutionStrategy.KeepCloud:
                    return new CloudSyncResult(CloudSyncStatus.ConflictResolved, remoteJson, conflict, "Conflict resolved: Kept cloud save.");

                case ConflictResolutionStrategy.KeepNewest:
                default:
                    if (localMeta.LastSavedUtc >= remoteMeta.LastSavedUtc)
                    {
                        await provider.SaveAsync(slotKey, localJson, localMeta);
                        return new CloudSyncResult(CloudSyncStatus.ConflictResolved, localJson, conflict, "Conflict resolved: Kept newest save (Local).");
                    }
                    else
                    {
                        return new CloudSyncResult(CloudSyncStatus.ConflictResolved, remoteJson, conflict, "Conflict resolved: Kept newest save (Cloud).");
                    }
            }
        }
    }
}
