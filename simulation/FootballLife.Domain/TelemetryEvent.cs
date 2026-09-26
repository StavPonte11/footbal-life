using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Event type classification in the standard game telemetry taxonomy.
    /// </summary>
    public enum TelemetryEventType
    {
        SessionStart = 0,
        SessionEnd = 1,
        TutorialStep = 2,
        TutorialSkipped = 3,
        MatchStarted = 4,
        MatchEnded = 5,
        GoalScored = 6,
        TrainingCompleted = 7,
        EconomyTransaction = 8,
        CloudSyncCompleted = 9,
        MonetizationPurchased = 10,
        CrashReported = 11
    }

    /// <summary>
    /// Structured telemetry event record for privacy-compliant player journey analytics.
    /// Pure C# domain model compatible with UGS Analytics and PostHog schemas.
    /// </summary>
    public sealed record TelemetryEvent(
        TelemetryEventType EventType,
        string EventName,
        DateTime TimestampUtc,
        string SessionId,
        IReadOnlyDictionary<string, object> Parameters
    )
    {
        public static TelemetryEvent Create(
            TelemetryEventType type,
            string sessionId,
            IReadOnlyDictionary<string, object>? parameters = null)
        {
            string eventName = type switch
            {
                TelemetryEventType.SessionStart => "session_start",
                TelemetryEventType.SessionEnd => "session_end",
                TelemetryEventType.TutorialStep => "tutorial_step",
                TelemetryEventType.TutorialSkipped => "tutorial_skipped",
                TelemetryEventType.MatchStarted => "match_started",
                TelemetryEventType.MatchEnded => "match_ended",
                TelemetryEventType.GoalScored => "goal_scored",
                TelemetryEventType.TrainingCompleted => "training_completed",
                TelemetryEventType.EconomyTransaction => "economy_transaction",
                TelemetryEventType.CloudSyncCompleted => "cloud_sync_completed",
                TelemetryEventType.MonetizationPurchased => "monetization_purchased",
                TelemetryEventType.CrashReported => "crash_reported",
                _ => type.ToString().ToLowerInvariant()
            };

            return new TelemetryEvent(
                EventType: type,
                EventName: eventName,
                TimestampUtc: DateTime.UtcNow,
                SessionId: sessionId ?? string.Empty,
                Parameters: parameters ?? new Dictionary<string, object>()
            );
        }
    }
}
