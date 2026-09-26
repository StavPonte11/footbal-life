using System;
using System.Collections.Generic;
using System.IO;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# diagnostic service managing the rolling breadcrumb ring buffer,
    /// crash dump generation, and offline crash report persistence (#P7-702).
    /// </summary>
    public sealed class CrashDiagnosticService
    {
        private static readonly Lazy<CrashDiagnosticService> _lazyInstance =
            new Lazy<CrashDiagnosticService>(() => new CrashDiagnosticService());

        public static CrashDiagnosticService Instance => _lazyInstance.Value;

        private readonly object _lock = new object();
        private readonly Queue<DiagnosticBreadcrumb> _breadcrumbBuffer = new Queue<DiagnosticBreadcrumb>();
        private readonly int _maxBreadcrumbs;
        private readonly string? _crashDirectory;

        public event Action<CrashReport>? OnCrashCaptured;

        public int MaxBreadcrumbs => _maxBreadcrumbs;
        public string? CrashDirectory => _crashDirectory;

        public CrashDiagnosticService(int maxBreadcrumbs = 30, string? crashDirectory = null)
        {
            _maxBreadcrumbs = Math.Max(5, maxBreadcrumbs);
            _crashDirectory = crashDirectory;

            if (!string.IsNullOrWhiteSpace(_crashDirectory) && !Directory.Exists(_crashDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_crashDirectory);
                }
                catch { }
            }
        }

        /// <summary>
        /// Records a diagnostic breadcrumb in the circular ring buffer.
        /// </summary>
        public void AddBreadcrumb(DiagnosticBreadcrumbCategory category, string message, IReadOnlyDictionary<string, string>? data = null)
        {
            var breadcrumb = DiagnosticBreadcrumb.Create(category, message, data);

            lock (_lock)
            {
                if (_breadcrumbBuffer.Count >= _maxBreadcrumbs)
                {
                    _breadcrumbBuffer.Dequeue();
                }
                _breadcrumbBuffer.Enqueue(breadcrumb);
            }
        }

        /// <summary>
        /// Retrieves a snapshot of recent breadcrumbs in chronological order.
        /// </summary>
        public IReadOnlyList<DiagnosticBreadcrumb> GetRecentBreadcrumbs()
        {
            lock (_lock)
            {
                return _breadcrumbBuffer.ToArray();
            }
        }

        /// <summary>
        /// Clears all recorded breadcrumbs.
        /// </summary>
        public void ClearBreadcrumbs()
        {
            lock (_lock)
            {
                _breadcrumbBuffer.Clear();
            }
        }

        /// <summary>
        /// Captures an exception, creates a structured CrashReport with context and breadcrumbs,
        /// persists it to the offline directory if configured, and fires OnCrashCaptured.
        /// </summary>
        public CrashReport CaptureException(
            Exception ex,
            string? context = null,
            bool isFatal = false,
            int deviceMemoryMb = 0,
            float batteryLevel = 1.0f)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            var breadcrumbs = GetRecentBreadcrumbs();
            var report = new CrashReport(
                exceptionType: ex.GetType().FullName ?? ex.GetType().Name,
                message: ex.Message,
                stackTrace: ex.StackTrace ?? string.Empty,
                context: context,
                isFatal: isFatal,
                deviceMemoryMb: deviceMemoryMb,
                batteryLevel: batteryLevel,
                breadcrumbs: breadcrumbs
            );

            // Persist report to offline disk storage if directory is configured
            if (!string.IsNullOrWhiteSpace(_crashDirectory))
            {
                SaveReportToDisk(report);
            }

            OnCrashCaptured?.Invoke(report);
            return report;
        }

        private void SaveReportToDisk(CrashReport report)
        {
            if (string.IsNullOrWhiteSpace(_crashDirectory)) return;

            try
            {
                if (!Directory.Exists(_crashDirectory))
                {
                    Directory.CreateDirectory(_crashDirectory);
                }

                string filePath = Path.Combine(_crashDirectory, $"crash_{report.CrashId}.json");
                File.WriteAllText(filePath, report.ToJson());
            }
            catch { }
        }

        /// <summary>
        /// Scans the crash directory for pending offline crash reports, invokes onDispatch for each,
        /// and deletes the processed file to ensure at-most-once telemetry delivery (#P7-702).
        /// </summary>
        public int FlushPendingCrashReports(Action<CrashReport> onDispatch)
        {
            if (string.IsNullOrWhiteSpace(_crashDirectory) || !Directory.Exists(_crashDirectory))
            {
                return 0;
            }

            if (onDispatch == null) throw new ArgumentNullException(nameof(onDispatch));

            int count = 0;
            try
            {
                var files = Directory.GetFiles(_crashDirectory, "crash_*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var report = CrashReport.FromJson(json);
                        if (report != null)
                        {
                            onDispatch(report);
                            count++;
                        }
                        File.Delete(file);
                    }
                    catch
                    {
                        // Clean up malformed crash dump
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }

            return count;
        }
    }
}
