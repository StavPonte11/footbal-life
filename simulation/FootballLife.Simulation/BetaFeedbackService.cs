using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# in-app beta tester feedback and diagnostic reporting service (#P7-902).
    /// Stores submissions locally with diagnostic context and handles dispatch.
    /// </summary>
    public sealed class BetaFeedbackService
    {
        private static readonly Lazy<BetaFeedbackService> _lazyInstance =
            new Lazy<BetaFeedbackService>(() => new BetaFeedbackService());

        public static BetaFeedbackService Instance => _lazyInstance.Value;

        private readonly object _lock = new object();
        private readonly List<BetaFeedbackReport> _feedbackQueue = new List<BetaFeedbackReport>();
        private readonly string? _storageDirectory;

        public event Action<BetaFeedbackReport>? OnFeedbackSubmitted;

        public string? StorageDirectory => _storageDirectory;
        public int QueuedCount
        {
            get
            {
                lock (_lock)
                {
                    return _feedbackQueue.Count;
                }
            }
        }

        public BetaFeedbackService(string? storageDirectory = null)
        {
            _storageDirectory = storageDirectory;
            if (!string.IsNullOrWhiteSpace(_storageDirectory) && !Directory.Exists(_storageDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_storageDirectory);
                }
                catch { }
            }
        }

        /// <summary>
        /// Submits tester feedback, captures recent breadcrumbs and device context,
        /// persists report to local disk, and notifies listeners.
        /// </summary>
        public BetaFeedbackReport SubmitFeedback(
            FeedbackCategory category,
            int satisfactionRating,
            string comment,
            string? contactEmail = null,
            FeedbackDeviceContext? deviceContext = null,
            IReadOnlyList<DiagnosticBreadcrumb>? breadcrumbs = null,
            string? saveSnapshotSummary = null)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new ArgumentException("Feedback comment cannot be empty.", nameof(comment));
            }

            int ratingClamped = Math.Clamp(satisfactionRating, 1, 5);
            var context = deviceContext ?? new FeedbackDeviceContext("Unknown", "Generic OS", 2048, 1.0f, DeviceTier.Medium);
            var crumbs = breadcrumbs ?? CrashDiagnosticService.Instance.GetRecentBreadcrumbs();

            var report = new BetaFeedbackReport(
                FeedbackId: Guid.NewGuid().ToString("N"),
                SessionId: TelemetryService.Instance.SessionId,
                TimestampUtc: DateTime.UtcNow,
                Category: category,
                SatisfactionRating: ratingClamped,
                Comment: comment.Trim(),
                ContactEmail: contactEmail?.Trim(),
                DeviceContext: context,
                Breadcrumbs: crumbs,
                SaveSnapshotSummary: saveSnapshotSummary
            );

            lock (_lock)
            {
                _feedbackQueue.Add(report);
            }

            SaveReportToDisk(report);
            OnFeedbackSubmitted?.Invoke(report);

            // Log diagnostic breadcrumb so the user submission is tracked in subsequent crash contexts
            CrashDiagnosticService.Instance.AddBreadcrumb(
                DiagnosticBreadcrumbCategory.UI,
                $"Beta feedback submitted ({category}): {comment.Substring(0, Math.Min(30, comment.Length))}..."
            );

            return report;
        }

        private void SaveReportToDisk(BetaFeedbackReport report)
        {
            if (string.IsNullOrWhiteSpace(_storageDirectory)) return;

            try
            {
                if (!Directory.Exists(_storageDirectory))
                {
                    Directory.CreateDirectory(_storageDirectory);
                }

                string filePath = Path.Combine(_storageDirectory, $"feedback_{report.FeedbackId}.json");
                string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        /// <summary>
        /// Retrieves all feedback submissions held in memory.
        /// </summary>
        public IReadOnlyList<BetaFeedbackReport> GetSubmittedFeedback()
        {
            lock (_lock)
            {
                return _feedbackQueue.ToArray();
            }
        }

        /// <summary>
        /// Flushes stored feedback files from disk and invokes the dispatch callback.
        /// </summary>
        public int FlushStoredFeedback(Action<BetaFeedbackReport> onDispatch)
        {
            if (string.IsNullOrWhiteSpace(_storageDirectory) || !Directory.Exists(_storageDirectory))
            {
                return 0;
            }

            if (onDispatch == null) throw new ArgumentNullException(nameof(onDispatch));

            int count = 0;
            try
            {
                var files = Directory.GetFiles(_storageDirectory, "feedback_*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var report = JsonSerializer.Deserialize<BetaFeedbackReport>(json);
                        if (report != null)
                        {
                            onDispatch(report);
                            count++;
                        }
                        File.Delete(file);
                    }
                    catch
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }

            return count;
        }
    }
}
