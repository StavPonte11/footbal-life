using System;
using System.Threading.Tasks;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CloudSaveSyncTests
    {
        [Fact]
        public async Task CloudSaveSync_FirstTimeUpload_UploadsLocalSave()
        {
            var syncService = new CloudSaveSyncService();
            var provider = new EmulatedCloudSaveProvider();

            var save = new CareerSaveData
            {
                PlayerName = "Marcus Vance",
                ClubName = "Arsenal",
                CurrentSeason = 2,
                OverallRating = 72,
                LastSavedAt = DateTime.UtcNow.ToString("o")
            };
            string json = save.ToJson();

            var result = await syncService.SyncSlotAsync("slot_1", json, save, provider);

            Assert.Equal(CloudSyncStatus.Uploaded, result.Status);
            Assert.Null(result.Conflict);

            // Verify provider has it
            var cloudJson = await provider.LoadAsync("slot_1");
            Assert.Equal(json, cloudJson);

            var meta = await provider.GetMetadataAsync("slot_1");
            Assert.NotNull(meta);
            Assert.Equal("Marcus Vance", meta!.PlayerName);
            Assert.Equal(72, meta.OverallRating);
        }

        [Fact]
        public async Task CloudSaveSync_IdenticalSaves_AreInSync()
        {
            var syncService = new CloudSaveSyncService();
            var provider = new EmulatedCloudSaveProvider();

            var save = new CareerSaveData
            {
                PlayerName = "Marcus Vance",
                ClubName = "Arsenal",
                OverallRating = 72,
                LastSavedAt = DateTime.UtcNow.ToString("o")
            };
            string json = save.ToJson();

            // First upload
            await syncService.SyncSlotAsync("slot_1", json, save, provider);

            // Second sync with identical data
            var result = await syncService.SyncSlotAsync("slot_1", json, save, provider);
            Assert.Equal(CloudSyncStatus.InSync, result.Status);
        }

        [Fact]
        public async Task CloudSaveSync_Conflict_KeepNewest_PicksNewestSave()
        {
            var syncService = new CloudSaveSyncService();
            var provider = new EmulatedCloudSaveProvider();

            // Remote save is older (saved 2 hours ago)
            var remoteSave = new CareerSaveData
            {
                PlayerName = "Marcus Vance",
                ClubName = "Northfield Town",
                OverallRating = 65,
                LastSavedAt = DateTime.UtcNow.AddHours(-2).ToString("o")
            };
            string remoteJson = remoteSave.ToJson();
            var remoteMeta = syncService.CreateMetadata("slot_1", remoteSave, remoteJson);
            await provider.SaveAsync("slot_1", remoteJson, remoteMeta);

            // Local save is newer (saved 5 minutes ago)
            var localSave = new CareerSaveData
            {
                PlayerName = "Marcus Vance",
                ClubName = "Arsenal",
                OverallRating = 75,
                LastSavedAt = DateTime.UtcNow.AddMinutes(-5).ToString("o")
            };
            string localJson = localSave.ToJson();

            var result = await syncService.SyncSlotAsync(
                "slot_1",
                localJson,
                localSave,
                provider,
                ConflictResolutionStrategy.KeepNewest
            );

            Assert.Equal(CloudSyncStatus.ConflictResolved, result.Status);
            Assert.NotNull(result.Conflict);
            Assert.Equal(localJson, result.ResolvedJson);

            // Cloud provider now has local's newer save
            var currentCloudJson = await provider.LoadAsync("slot_1");
            Assert.Equal(localJson, currentCloudJson);
        }

        [Fact]
        public async Task CloudSaveSync_Offline_ReturnsOfflineStatus()
        {
            var syncService = new CloudSaveSyncService();
            var provider = new EmulatedCloudSaveProvider
            {
                IsOffline = true
            };

            var save = new CareerSaveData();
            var result = await syncService.SyncSlotAsync("slot_1", save.ToJson(), save, provider);

            Assert.Equal(CloudSyncStatus.Offline, result.Status);
        }
    }
}
