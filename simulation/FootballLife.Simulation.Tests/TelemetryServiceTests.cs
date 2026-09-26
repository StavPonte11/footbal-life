using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class TelemetryServiceTests
    {
        [Fact]
        public void TelemetryService_BuffersAndFlushesEvents()
        {
            var service = new TelemetryService
            {
                SessionId = "test-session-123",
                BufferFlushThreshold = 5
            };
            service.Clear();

            Assert.Equal(0, service.BufferedEventCount);

            service.Track(TelemetryEventType.SessionStart, new Dictionary<string, object>
            {
                ["app_version"] = "1.0.0"
            });

            service.Track(TelemetryEventType.MatchStarted, new Dictionary<string, object>
            {
                ["opponent"] = "Chelsea"
            });

            Assert.Equal(2, service.BufferedEventCount);

            var flushed = service.Flush();
            Assert.Equal(2, flushed.Count);
            Assert.Equal(0, service.BufferedEventCount);

            Assert.Equal("session_start", flushed[0].EventName);
            Assert.Equal("1.0.0", flushed[0].Parameters["app_version"]);
            Assert.Equal("test-session-123", flushed[0].SessionId);

            Assert.Equal("match_started", flushed[1].EventName);
            Assert.Equal("Chelsea", flushed[1].Parameters["opponent"]);
        }

        [Fact]
        public void TelemetryService_AutoFlushesAtThreshold()
        {
            var service = new TelemetryService
            {
                BufferFlushThreshold = 3
            };
            service.Clear();

            int batchEventCount = 0;
            service.OnBatchFlushed += batch =>
            {
                batchEventCount = batch.Count;
            };

            service.Track(TelemetryEventType.GoalScored);
            service.Track(TelemetryEventType.GoalScored);
            Assert.Equal(2, service.BufferedEventCount);

            // 3rd event exceeds threshold -> auto flush!
            service.Track(TelemetryEventType.MatchEnded);
            Assert.Equal(0, service.BufferedEventCount);
            Assert.Equal(3, batchEventCount);
        }

        [Fact]
        public void TelemetryService_OptOut_SuppressesAllEvents()
        {
            var service = new TelemetryService
            {
                IsOptedOut = true
            };
            service.Clear();

            service.Track(TelemetryEventType.SessionStart);
            service.Track(TelemetryEventType.GoalScored);
            service.Track(TelemetryEventType.EconomyTransaction);

            Assert.Equal(0, service.BufferedEventCount);
            var flushed = service.Flush();
            Assert.Empty(flushed);
        }
    }
}
