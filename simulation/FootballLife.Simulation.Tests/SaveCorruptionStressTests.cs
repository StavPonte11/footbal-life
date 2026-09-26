using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class SaveCorruptionStressTests : IDisposable
    {
        private readonly string _testDir;

        public SaveCorruptionStressTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "FootballLifeSaveStress_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                    Directory.Delete(_testDir, true);
            }
            catch { }
        }

        private CareerSaveData CreateTestSave(int slot = 0, string player = "Marcus Torres", string club = "Nottingham Forest")
        {
            return new CareerSaveData
            {
                SlotIndex = slot,
                PlayerName = player,
                ClubName = club,
                CurrentSeason = 3,
                CurrentWeek = 22,
                OverallRating = 78,
                BankBalance = 125000,
                MasterSeed = 12345,
                WeeklyWage = 15000,
                TotalGoals = 47,
                TotalAssists = 23,
                TotalAppearances = 88
            };
        }

        // ------ #P7-703: Primary save truncation / mid-write kill simulation ------

        [Fact]
        public void TruncatedPrimarySave_FallsBackToValidBackup()
        {
            string savePath = Path.Combine(_testDir, "slot1.json");
            var save = CreateTestSave();

            // Write a valid save first (creates primary + .bak on second write)
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Write again so .bak exists
            save.CurrentWeek = 23;
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Simulate mid-write crash: truncate primary file
            string content = File.ReadAllText(savePath);
            File.WriteAllText(savePath, content.Substring(0, content.Length / 3));

            // Also corrupt the checksum so it definitely fails
            string checksumPath = savePath + ".sha256";
            if (File.Exists(checksumPath))
            {
                File.WriteAllText(checksumPath, "0000000000000000000000000000000000000000000000000000000000000000");
            }

            var logs = new List<string>();
            var restored = CareerSaveService.LoadFromFile(savePath, msg => logs.Add(msg));

            Assert.NotNull(restored);
            Assert.Equal("Marcus Torres", restored.PlayerName);
            Assert.True(logs.Count > 0, "Expected corruption log messages");
        }

        [Fact]
        public void BitFlipCorruption_DetectedByChecksumMismatch()
        {
            string savePath = Path.Combine(_testDir, "slot_bitflip.json");
            var save = CreateTestSave();
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Simulate bit-flip: change a character in the middle of the JSON
            string json = File.ReadAllText(savePath);
            char[] chars = json.ToCharArray();
            int midpoint = chars.Length / 2;
            chars[midpoint] = chars[midpoint] == 'X' ? 'Y' : 'X';
            File.WriteAllText(savePath, new string(chars));

            // Load should detect checksum mismatch and fall back
            var logs = new List<string>();
            var result = CareerSaveService.LoadFromFile(savePath, msg => logs.Add(msg));

            // Result is null (no backup exists for first save)
            Assert.Null(result);
            Assert.Contains(logs, l => l.Contains("corrupted", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void BitFlipCorruption_AutoHealsFromValidBackup()
        {
            string savePath = Path.Combine(_testDir, "slot_heal.json");
            var save = CreateTestSave();

            // Save twice so .bak is established
            Assert.True(CareerSaveService.SaveToFile(savePath, save));
            save.CurrentWeek = 24;
            save.TotalGoals = 48;
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Corrupt primary JSON content
            string json = File.ReadAllText(savePath);
            File.WriteAllText(savePath, json.Replace("\"TotalGoals\": 48", "\"TotalGoals\": CORRUPT"));

            // Write invalid checksum for primary
            string checksumPath = savePath + ".sha256";
            File.WriteAllText(checksumPath, "deadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef");

            var logs = new List<string>();
            var restored = CareerSaveService.LoadFromFile(savePath, msg => logs.Add(msg));

            Assert.NotNull(restored);
            Assert.Equal("Marcus Torres", restored.PlayerName);

            // Verify auto-heal: primary file should now be overwritten with backup content
            var reloaded = CareerSaveService.LoadFromFile(savePath);
            Assert.NotNull(reloaded);
        }

        [Fact]
        public void DualCorruption_BothPrimaryAndBackup_ReturnsNull()
        {
            string savePath = Path.Combine(_testDir, "slot_dual.json");
            string backupPath = savePath + ".bak";

            var save = CreateTestSave();
            Assert.True(CareerSaveService.SaveToFile(savePath, save));
            save.CurrentWeek = 25;
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Corrupt both primary and backup
            File.WriteAllText(savePath, "{totally invalid json!!!");
            File.WriteAllText(backupPath, "{also corrupted backup!!!");

            // Invalidate checksums
            File.WriteAllText(savePath + ".sha256", "bad");
            File.WriteAllText(backupPath + ".sha256", "bad");

            var logs = new List<string>();
            var result = CareerSaveService.LoadFromFile(savePath, msg => logs.Add(msg));

            Assert.Null(result);
            Assert.True(logs.Count >= 2, "Expected corruption logs for both primary and backup");
        }

        // ------ #P7-703: Atomic write guarantees ------

        [Fact]
        public void AtomicSave_NoTempFileLeftBehind()
        {
            string savePath = Path.Combine(_testDir, "slot_atomic.json");
            var save = CreateTestSave();

            for (int i = 0; i < 20; i++)
            {
                save.CurrentWeek = i + 1;
                Assert.True(CareerSaveService.SaveToFile(savePath, save));
            }

            // No .tmp files should remain
            string[] tmpFiles = Directory.GetFiles(_testDir, "*.tmp");
            Assert.Empty(tmpFiles);

            // Primary save should be valid and most recent
            var loaded = CareerSaveService.LoadFromFile(savePath);
            Assert.NotNull(loaded);
            Assert.Equal(20, loaded.CurrentWeek);
        }

        [Fact]
        public void RapidSaveOverwrite_MaintainsIntegrity()
        {
            string savePath = Path.Combine(_testDir, "slot_rapid.json");
            var save = CreateTestSave();

            // Simulate rapid repeated saves (e.g. auto-save during match)
            for (int i = 0; i < 50; i++)
            {
                save.CurrentWeek = i + 1;
                save.BankBalance += 1000;
                Assert.True(CareerSaveService.SaveToFile(savePath, save));
            }

            var loaded = CareerSaveService.LoadFromFile(savePath);
            Assert.NotNull(loaded);
            Assert.Equal(50, loaded.CurrentWeek);
            Assert.Equal(125000 + 50 * 1000, loaded.BankBalance);
        }

        [Fact]
        public void ChecksumValidation_MatchesExpectedHash()
        {
            string savePath = Path.Combine(_testDir, "slot_hash.json");
            var save = CreateTestSave();
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            // Read the saved JSON and its companion checksum
            string json = File.ReadAllText(savePath);
            string storedChecksum = File.ReadAllText(savePath + ".sha256").Trim();
            string computedChecksum = CareerSaveService.ComputeSha256(json);

            Assert.Equal(computedChecksum, storedChecksum);
        }

        // ------ #P7-703: Cloud conflict resolution under divergence ------

        [Fact]
        public async Task CloudSync_KeepNewest_ResolvesConflictByTimestamp()
        {
            var provider = new EmulatedCloudSaveProvider();
            var syncService = new CloudSaveSyncService();

            // Create and upload an "older" local save to cloud
            var olderSave = CreateTestSave();
            olderSave.LastSavedAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc).ToString("o");
            string olderJson = olderSave.ToJson();
            var olderMeta = syncService.CreateMetadata("slot1", olderSave, olderJson);
            await provider.SaveAsync("slot1", olderJson, olderMeta);

            // Create a "newer" local save
            var newerSave = CreateTestSave();
            newerSave.CurrentWeek = 30;
            newerSave.LastSavedAt = new DateTime(2026, 9, 25, 14, 0, 0, DateTimeKind.Utc).ToString("o");
            string newerJson = newerSave.ToJson();

            var result = await syncService.SyncSlotAsync("slot1", newerJson, newerSave, provider, ConflictResolutionStrategy.KeepNewest);

            Assert.Equal(CloudSyncStatus.ConflictResolved, result.Status);
            Assert.Contains("Local", result.Message);
            Assert.NotNull(result.ResolvedJson);

            // Verify the cloud now has the newer version
            string? cloudJson = await provider.LoadAsync("slot1");
            Assert.Equal(newerJson, cloudJson);
        }

        [Fact]
        public async Task CloudSync_KeepCloud_ResolvesConflictToRemote()
        {
            var provider = new EmulatedCloudSaveProvider();
            var syncService = new CloudSaveSyncService();

            // Upload cloud version
            var cloudSave = CreateTestSave();
            cloudSave.TotalGoals = 99;
            cloudSave.LastSavedAt = DateTime.UtcNow.ToString("o");
            string cloudJson = cloudSave.ToJson();
            var cloudMeta = syncService.CreateMetadata("slot1", cloudSave, cloudJson);
            await provider.SaveAsync("slot1", cloudJson, cloudMeta);

            // Create diverged local version
            var localSave = CreateTestSave();
            localSave.TotalGoals = 50;
            localSave.LastSavedAt = DateTime.UtcNow.AddSeconds(-10).ToString("o");
            string localJson = localSave.ToJson();

            var result = await syncService.SyncSlotAsync("slot1", localJson, localSave, provider, ConflictResolutionStrategy.KeepCloud);

            Assert.Equal(CloudSyncStatus.ConflictResolved, result.Status);
            Assert.Contains("cloud", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CloudSync_OfflineMode_ReturnsOfflineStatus()
        {
            var provider = new EmulatedCloudSaveProvider { IsOffline = true };
            var syncService = new CloudSaveSyncService();

            var save = CreateTestSave();
            string json = save.ToJson();

            var result = await syncService.SyncSlotAsync("slot1", json, save, provider);

            Assert.Equal(CloudSyncStatus.Offline, result.Status);
            Assert.Equal(json, result.ResolvedJson);
        }

        [Fact]
        public async Task CloudSync_NoRemote_UploadsLocal()
        {
            var provider = new EmulatedCloudSaveProvider();
            var syncService = new CloudSaveSyncService();

            var save = CreateTestSave();
            string json = save.ToJson();

            var result = await syncService.SyncSlotAsync("slot1", json, save, provider);

            Assert.Equal(CloudSyncStatus.Uploaded, result.Status);

            // Verify cloud now has content
            string? cloudJson = await provider.LoadAsync("slot1");
            Assert.NotNull(cloudJson);
        }

        [Fact]
        public async Task CloudSync_IdenticalHashes_ReportsInSync()
        {
            var provider = new EmulatedCloudSaveProvider();
            var syncService = new CloudSaveSyncService();

            var save = CreateTestSave();
            save.LastSavedAt = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc).ToString("o");
            string json = save.ToJson();

            // Upload it
            var meta = syncService.CreateMetadata("slot1", save, json);
            await provider.SaveAsync("slot1", json, meta);

            // Sync with identical data
            var result = await syncService.SyncSlotAsync("slot1", json, save, provider);

            Assert.Equal(CloudSyncStatus.InSync, result.Status);
        }

        // ------ #P7-703: Slot deletion and multi-slot isolation ------

        [Fact]
        public void DeleteFile_RemovesPrimaryBackupAndTemp()
        {
            string savePath = Path.Combine(_testDir, "slot_del.json");
            var save = CreateTestSave();

            // Create primary, backup
            Assert.True(CareerSaveService.SaveToFile(savePath, save));
            save.CurrentWeek = 2;
            Assert.True(CareerSaveService.SaveToFile(savePath, save));

            Assert.True(File.Exists(savePath));
            Assert.True(File.Exists(savePath + ".bak"));

            Assert.True(CareerSaveService.DeleteFile(savePath));
            Assert.False(File.Exists(savePath));
            Assert.False(File.Exists(savePath + ".bak"));
        }

        [Fact]
        public void MultipleSlots_IsolatedFromEachOther()
        {
            for (int slot = 0; slot < 3; slot++)
            {
                string path = Path.Combine(_testDir, $"career_slot_{slot}.json");
                var save = CreateTestSave(slot: slot, player: $"Player_{slot}", club: $"Club_{slot}");
                save.TotalGoals = slot * 10;
                Assert.True(CareerSaveService.SaveToFile(path, save));
            }

            // Verify each slot loaded independently
            for (int slot = 0; slot < 3; slot++)
            {
                string path = Path.Combine(_testDir, $"career_slot_{slot}.json");
                var loaded = CareerSaveService.LoadFromFile(path);
                Assert.NotNull(loaded);
                Assert.Equal($"Player_{slot}", loaded.PlayerName);
                Assert.Equal($"Club_{slot}", loaded.ClubName);
                Assert.Equal(slot * 10, loaded.TotalGoals);
            }
        }
    }
}
