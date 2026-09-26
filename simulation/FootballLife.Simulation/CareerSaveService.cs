using System;
using System.IO;
using System.Text.Json;

namespace FootballLife.Simulation.Persistence
{
    /// <summary>
    /// Pure C# atomic file persistence service for saving, loading, and managing career saves.
    /// Operates without any dependency on UnityEngine.
    /// </summary>
    public static class CareerSaveService
    {
        /// <summary>
        /// Computes SHA256 checksum string for save integrity verification (#P7-703).
        /// </summary>
        public static string ComputeSha256(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            byte[] hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Saves career data atomically to disk by writing to a temporary file, flushing, and replacing destination.
        /// Preserves the prior version as a .bak file if it exists, and writes companion .sha256 checksum file (#P7-703).
        /// </summary>
        public static bool SaveToFile(string targetPath, CareerSaveData data, Action<string>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentException("Target path cannot be empty.", nameof(targetPath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string? directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = targetPath + ".tmp";
            string backupPath = targetPath + ".bak";
            string checksumPath = targetPath + ".sha256";
            string backupChecksumPath = backupPath + ".sha256";

            try
            {
                data.LastSavedAt = DateTime.UtcNow.ToString("o");
                string json = data.ToJson();
                string checksum = ComputeSha256(json);

                // 1. Write and flush to temp file
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(true);
                }

                // 2. Backup existing file & checksum if present
                if (File.Exists(targetPath))
                {
                    try
                    {
                        File.Copy(targetPath, backupPath, overwrite: true);
                        if (File.Exists(checksumPath))
                        {
                            File.Copy(checksumPath, backupChecksumPath, overwrite: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"Warning: Could not create backup file: {ex.Message}");
                    }
                }

                // 3. Atomically move/replace temp file to target
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Move(tempPath, targetPath);

                // 4. Update checksum companion file
                try
                {
                    File.WriteAllText(checksumPath, checksum);
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Error saving career file: {ex.Message}");
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                return false;
            }
        }

        /// <summary>
        /// Loads career data from file with checksum verification, falling back to .bak file
        /// and auto-healing the primary save if corrupted (#P7-703).
        /// </summary>
        public static CareerSaveData? LoadFromFile(string targetPath, Action<string>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentException("Target path cannot be empty.", nameof(targetPath));

            string backupPath = targetPath + ".bak";
            string checksumPath = targetPath + ".sha256";
            string backupChecksumPath = backupPath + ".sha256";

            // Attempt primary load with checksum verification
            if (File.Exists(targetPath))
            {
                try
                {
                    string json = File.ReadAllText(targetPath);

                    if (File.Exists(checksumPath))
                    {
                        string expected = File.ReadAllText(checksumPath).Trim();
                        string actual = ComputeSha256(json);
                        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidDataException($"Checksum mismatch in primary save: expected {expected}, got {actual}");
                        }
                    }

                    var save = CareerSaveData.FromJson(json);
                    if (save != null) return save;
                    throw new JsonException("Deserialized save was null.");
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"Primary save file corrupted ({ex.Message}), attempting backup restore.");
                }
            }

            // Attempt backup load and auto-heal
            if (File.Exists(backupPath))
            {
                try
                {
                    string json = File.ReadAllText(backupPath);

                    if (File.Exists(backupChecksumPath))
                    {
                        string expected = File.ReadAllText(backupChecksumPath).Trim();
                        string actual = ComputeSha256(json);
                        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidDataException($"Checksum mismatch in backup save: expected {expected}, got {actual}");
                        }
                    }

                    var restored = CareerSaveData.FromJson(json);
                    if (restored != null)
                    {
                        logger?.Invoke("Restored save successfully from backup. Auto-healing primary save file.");

                        // Auto-healing: copy healthy backup to primary path so subsequent loads succeed cleanly (#P7-703)
                        try
                        {
                            File.Copy(backupPath, targetPath, overwrite: true);
                            if (File.Exists(backupChecksumPath))
                            {
                                File.Copy(backupChecksumPath, checksumPath, overwrite: true);
                            }
                        }
                        catch { }

                        return restored;
                    }
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"Backup save file also corrupted: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Reads lightweight slot metadata without parsing entire object graph.
        /// </summary>
        public static SaveSlotSummary GetSlotSummary(string targetPath, int slotIndex)
        {
            if (!File.Exists(targetPath))
            {
                return SaveSlotSummary.Empty(slotIndex);
            }

            try
            {
                string json = File.ReadAllText(targetPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string playerName = root.TryGetProperty("PlayerName", out var p) ? p.GetString() ?? "" : "";
                string clubName = root.TryGetProperty("ClubName", out var c) ? c.GetString() ?? "" : "";
                int season = root.TryGetProperty("CurrentSeason", out var s) ? s.GetInt32() : 1;
                int week = root.TryGetProperty("CurrentWeek", out var w) ? w.GetInt32() : 1;
                int rating = root.TryGetProperty("OverallRating", out var r) ? r.GetInt32() : 60;
                int balance = root.TryGetProperty("BankBalance", out var b) ? b.GetInt32() : 0;
                DateTime lastSaved = root.TryGetProperty("LastSavedAt", out var d) && DateTime.TryParse(d.GetString(), out var dt)
                    ? dt
                    : File.GetLastWriteTimeUtc(targetPath);

                return new SaveSlotSummary(slotIndex, true, playerName, clubName, season, week, rating, balance, lastSaved);
            }
            catch
            {
                return SaveSlotSummary.Empty(slotIndex);
            }
        }

        /// <summary>
        /// Deletes the primary save file, backup file, and any leftover temp files.
        /// </summary>
        public static bool DeleteFile(string targetPath)
        {
            bool deleted = false;
            string backupPath = targetPath + ".bak";
            string tempPath = targetPath + ".tmp";

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
                deleted = true;
            }
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                deleted = true;
            }
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return deleted;
        }
    }
}
