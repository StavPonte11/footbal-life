using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class CrashDiagnosticServiceTests : IDisposable
    {
        private readonly string _testCrashDir;

        public CrashDiagnosticServiceTests()
        {
            _testCrashDir = Path.Combine(Path.GetTempPath(), "FootballLifeCrashTests_" + Guid.NewGuid().ToString("N"));
            if (Directory.Exists(_testCrashDir))
            {
                Directory.Delete(_testCrashDir, true);
            }
            Directory.CreateDirectory(_testCrashDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testCrashDir))
                {
                    Directory.Delete(_testCrashDir, true);
                }
            }
            catch { }
        }

        [Fact]
        public void AddBreadcrumb_MaintainsRingBufferCapacity()
        {
            const int maxCapacity = 5;
            var service = new CrashDiagnosticService(maxCapacity, _testCrashDir);

            for (int i = 0; i < 10; i++)
            {
                service.AddBreadcrumb(DiagnosticBreadcrumbCategory.Match, $"Event {i}");
            }

            var recent = service.GetRecentBreadcrumbs();
            Assert.Equal(maxCapacity, recent.Count);
            Assert.Equal("Event 5", recent[0].Message);
            Assert.Equal("Event 9", recent[4].Message);
        }

        [Fact]
        public void ClearBreadcrumbs_EmptiesRingBuffer()
        {
            var service = new CrashDiagnosticService(10, _testCrashDir);
            service.AddBreadcrumb(DiagnosticBreadcrumbCategory.UI, "Opened inventory");
            Assert.Single(service.GetRecentBreadcrumbs());

            service.ClearBreadcrumbs();
            Assert.Empty(service.GetRecentBreadcrumbs());
        }

        [Fact]
        public void CaptureException_CreatesFullStructuredReport()
        {
            var service = new CrashDiagnosticService(10, _testCrashDir);
            service.AddBreadcrumb(DiagnosticBreadcrumbCategory.Persistence, "Loading save slot 1");
            service.AddBreadcrumb(DiagnosticBreadcrumbCategory.Match, "Kickoff match #42");

            bool eventFired = false;
            CrashReport? capturedViaEvent = null;
            service.OnCrashCaptured += report =>
            {
                eventFired = true;
                capturedViaEvent = report;
            };

            Exception testEx;
            try
            {
                throw new InvalidOperationException("Simulation physics assertion failed");
            }
            catch (Exception ex)
            {
                testEx = ex;
            }

            var crashReport = service.CaptureException(
                testEx,
                context: "MatchSimulationLoop",
                isFatal: true,
                deviceMemoryMb: 3800,
                batteryLevel: 0.85f
            );

            Assert.True(eventFired);
            Assert.NotNull(capturedViaEvent);
            Assert.Equal(crashReport.CrashId, capturedViaEvent.CrashId);

            Assert.Equal("System.InvalidOperationException", crashReport.ExceptionType);
            Assert.Equal("Simulation physics assertion failed", crashReport.Message);
            Assert.Equal("MatchSimulationLoop", crashReport.Context);
            Assert.True(crashReport.IsFatal);
            Assert.Equal(3800, crashReport.DeviceMemoryMb);
            Assert.Equal(0.85f, crashReport.BatteryLevel, 2);
            Assert.Equal(2, crashReport.Breadcrumbs.Count);
            Assert.Equal("Loading save slot 1", crashReport.Breadcrumbs[0].Message);
            Assert.Equal(DiagnosticBreadcrumbCategory.Persistence, crashReport.Breadcrumbs[0].Category);
        }

        [Fact]
        public void CrashReport_SerializationRoundTrip_PreservesAllFields()
        {
            var breadcrumbs = new List<DiagnosticBreadcrumb>
            {
                DiagnosticBreadcrumb.Create(DiagnosticBreadcrumbCategory.Network, "Connecting to cloud", new Dictionary<string, string> { { "slot", "1" } }),
                DiagnosticBreadcrumb.Create(DiagnosticBreadcrumbCategory.System, "Low memory warning")
            };

            var original = new CrashReport(
                exceptionType: "System.NullReferenceException",
                message: "Object reference not set",
                stackTrace: "at FootballLife.Engine.Run() in /app/Engine.cs:line 12",
                context: "AppStartup",
                isFatal: true,
                deviceMemoryMb: 2048,
                batteryLevel: 0.42f,
                breadcrumbs: breadcrumbs
            );

            string json = original.ToJson();
            var restored = CrashReport.FromJson(json);

            Assert.NotNull(restored);
            Assert.Equal(original.CrashId, restored.CrashId);
            Assert.Equal(original.ExceptionType, restored.ExceptionType);
            Assert.Equal(original.Message, restored.Message);
            Assert.Equal(original.StackTrace, restored.StackTrace);
            Assert.Equal(original.Context, restored.Context);
            Assert.Equal(original.IsFatal, restored.IsFatal);
            Assert.Equal(original.DeviceMemoryMb, restored.DeviceMemoryMb);
            Assert.Equal(original.BatteryLevel, restored.BatteryLevel, 2);
            Assert.Equal(2, restored.Breadcrumbs.Count);
            Assert.Equal(DiagnosticBreadcrumbCategory.Network, restored.Breadcrumbs[0].Category);
            Assert.Equal("Connecting to cloud", restored.Breadcrumbs[0].Message);
            Assert.NotNull(restored.Breadcrumbs[0].Data);
            Assert.Equal("1", restored.Breadcrumbs[0].Data!["slot"]);
        }

        [Fact]
        public void FlushPendingCrashReports_DispatchesAndDeletesFiles()
        {
            var service = new CrashDiagnosticService(10, _testCrashDir);

            // Write 3 reports to disk via service
            for (int i = 1; i <= 3; i++)
            {
                service.CaptureException(new Exception($"Error #{i}"), context: $"Context_{i}");
            }

            // Verify files exist in directory
            var filesBefore = Directory.GetFiles(_testCrashDir, "crash_*.json");
            Assert.Equal(3, filesBefore.Length);

            var dispatchedReports = new List<CrashReport>();
            int flushedCount = service.FlushPendingCrashReports(r => dispatchedReports.Add(r));

            Assert.Equal(3, flushedCount);
            Assert.Equal(3, dispatchedReports.Count);

            // Verify directory is now empty
            var filesAfter = Directory.GetFiles(_testCrashDir, "crash_*.json");
            Assert.Empty(filesAfter);
        }

        [Fact]
        public void ConcurrentAddBreadcrumb_IsThreadSafe()
        {
            var service = new CrashDiagnosticService(20, _testCrashDir);

            Parallel.For(0, 100, i =>
            {
                service.AddBreadcrumb(DiagnosticBreadcrumbCategory.System, $"Worker {i}");
            });

            var crumbs = service.GetRecentBreadcrumbs();
            Assert.Equal(20, crumbs.Count);
        }
    }
}
