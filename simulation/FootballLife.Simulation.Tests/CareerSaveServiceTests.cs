using System;
using System.IO;
using FootballLife.Simulation.Persistence;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CareerSaveServiceTests : IDisposable
    {
        private readonly string _testDirectory;

        public CareerSaveServiceTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "FootballLife_SaveTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }

        [Fact]
        public void SaveToFile_CreatesTargetFileWithValidJson()
        {
            string filePath = Path.Combine(_testDirectory, "slot_1.json");
            var data = new CareerSaveData
            {
                PlayerName = "Jack Sterling",
                ClubName = "Bristol Rovers",
                CurrentSeason = 1,
                CurrentWeek = 5
            };

            bool success = CareerSaveService.SaveToFile(filePath, data);

            Assert.True(success);
            Assert.True(File.Exists(filePath));

            var loaded = CareerSaveService.LoadFromFile(filePath);
            Assert.NotNull(loaded);
            Assert.Equal("Jack Sterling", loaded.PlayerName);
            Assert.Equal("Bristol Rovers", loaded.ClubName);
        }

        [Fact]
        public void SaveToFile_OverwritingExistingFile_CreatesBackup()
        {
            string filePath = Path.Combine(_testDirectory, "slot_2.json");
            string backupPath = filePath + ".bak";

            var initialData = new CareerSaveData
            {
                PlayerName = "Version 1",
                CurrentWeek = 1
            };
            CareerSaveService.SaveToFile(filePath, initialData);

            Assert.True(File.Exists(filePath));
            Assert.False(File.Exists(backupPath));

            var updatedData = new CareerSaveData
            {
                PlayerName = "Version 2",
                CurrentWeek = 2
            };
            CareerSaveService.SaveToFile(filePath, updatedData);

            Assert.True(File.Exists(filePath));
            Assert.True(File.Exists(backupPath));

            var primary = CareerSaveService.LoadFromFile(filePath);
            Assert.NotNull(primary);
            Assert.Equal("Version 2", primary.PlayerName);

            var backup = CareerSaveService.LoadFromFile(backupPath);
            Assert.NotNull(backup);
            Assert.Equal("Version 1", backup.PlayerName);
        }

        [Fact]
        public void LoadFromFile_WhenPrimaryIsCorrupted_RestoresFromBackup()
        {
            string filePath = Path.Combine(_testDirectory, "slot_corrupt.json");
            string backupPath = filePath + ".bak";

            var validData = new CareerSaveData
            {
                PlayerName = "Backup Hero",
                CurrentWeek = 10
            };
            CareerSaveService.SaveToFile(filePath, validData);

            var secondVersion = new CareerSaveData
            {
                PlayerName = "Second Version",
                CurrentWeek = 11
            };
            CareerSaveService.SaveToFile(filePath, secondVersion);

            // Corrupt primary file
            File.WriteAllText(filePath, "{ corrupt json ... invalid syntax !!!");

            var restored = CareerSaveService.LoadFromFile(filePath);

            Assert.NotNull(restored);
            Assert.Equal("Backup Hero", restored.PlayerName);
            Assert.Equal(10, restored.CurrentWeek);
        }

        [Fact]
        public void GetSlotSummary_ReturnsAccurateMetadata()
        {
            string filePath = Path.Combine(_testDirectory, "slot_summary.json");
            var data = new CareerSaveData
            {
                SlotIndex = 1,
                PlayerName = "Leo Thorne",
                ClubName = "Fulham",
                CurrentSeason = 3,
                CurrentWeek = 22,
                OverallRating = 77,
                BankBalance = 45000
            };
            CareerSaveService.SaveToFile(filePath, data);

            var summary = CareerSaveService.GetSlotSummary(filePath, 1);

            Assert.True(summary.Exists);
            Assert.Equal(1, summary.SlotIndex);
            Assert.Equal("Leo Thorne", summary.PlayerName);
            Assert.Equal("Fulham", summary.ClubName);
            Assert.Equal(3, summary.Season);
            Assert.Equal(22, summary.Week);
            Assert.Equal(77, summary.OverallRating);
            Assert.Equal(45000, summary.BankBalance);
        }

        [Fact]
        public void GetSlotSummary_WhenFileDoesNotExist_ReturnsEmpty()
        {
            string nonExistent = Path.Combine(_testDirectory, "ghost.json");
            var summary = CareerSaveService.GetSlotSummary(nonExistent, 3);

            Assert.False(summary.Exists);
            Assert.Equal(3, summary.SlotIndex);
        }

        [Fact]
        public void DeleteFile_RemovesPrimaryAndBackupFiles()
        {
            string filePath = Path.Combine(_testDirectory, "delete_me.json");
            string backupPath = filePath + ".bak";

            CareerSaveService.SaveToFile(filePath, new CareerSaveData { PlayerName = "V1" });
            CareerSaveService.SaveToFile(filePath, new CareerSaveData { PlayerName = "V2" });

            Assert.True(File.Exists(filePath));
            Assert.True(File.Exists(backupPath));

            bool deleted = CareerSaveService.DeleteFile(filePath);

            Assert.True(deleted);
            Assert.False(File.Exists(filePath));
            Assert.False(File.Exists(backupPath));
        }
    }
}
