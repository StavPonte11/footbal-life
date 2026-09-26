using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Segmented closed beta cohorts for external target group analysis (#P7-901).
    /// </summary>
    public enum BetaCohortType
    {
        ExternalPioneers = 0,
        MobileGamers = 1,
        FootballSimFans = 2,
        CommunityVIP = 3
    }

    /// <summary>
    /// Status result of a closed beta invitation code redemption attempt (#P7-901).
    /// </summary>
    public enum BetaEnrollmentStatus
    {
        Valid = 0,
        Expired = 1,
        CodeNotFound = 2,
        CohortFull = 3,
        Revoked = 4
    }

    /// <summary>
    /// Closed beta access invitation pass definition (#P7-901).
    /// </summary>
    public sealed record BetaAccessPass(
        string Code,
        BetaCohortType Cohort,
        DateTime ExpirationUtc,
        int MaxRedemptions,
        int CurrentRedemptions = 0,
        bool IsActive = true
    );

    /// <summary>
    /// Result payload of an invitation code redemption.
    /// </summary>
    public sealed record BetaEnrollmentResult(
        BetaEnrollmentStatus Status,
        string Message,
        BetaAccessPass? Pass,
        string? BetaToken
    );

    /// <summary>
    /// Category classification for in-app beta tester feedback (#P7-902).
    /// </summary>
    public enum FeedbackCategory
    {
        Bug = 0,
        MatchEngine = 1,
        CareerProgression = 2,
        UIUX = 3,
        BalanceEconomy = 4,
        GeneralSuggestion = 5
    }

    /// <summary>
    /// Hardware and runtime diagnostic context attached to beta feedback submissions (#P7-902).
    /// </summary>
    public sealed record FeedbackDeviceContext(
        string Platform,
        string OperatingSystem,
        int SystemMemoryMb,
        float BatteryLevel,
        DeviceTier Tier
    );

    /// <summary>
    /// In-app beta tester feedback report with diagnostic breadcrumb attachments (#P7-902).
    /// </summary>
    public sealed record BetaFeedbackReport(
        string FeedbackId,
        string SessionId,
        DateTime TimestampUtc,
        FeedbackCategory Category,
        int SatisfactionRating,
        string Comment,
        string? ContactEmail,
        FeedbackDeviceContext DeviceContext,
        IReadOnlyList<DiagnosticBreadcrumb> Breadcrumbs,
        string? SaveSnapshotSummary,
        bool IsSynced = false
    );

    /// <summary>
    /// Health grading status for an individual beta KPI metric (#P7-903).
    /// </summary>
    public enum MetricEvaluationStatus
    {
        Pass = 0,
        Warning = 1,
        Fail = 2
    }

    /// <summary>
    /// Upfront target benchmarks and thresholds for closed beta validation (#P7-903).
    /// </summary>
    public sealed class BetaKpiTargets
    {
        public float TargetD1Retention { get; set; } = 0.45f; // 45%
        public float TargetD7Retention { get; set; } = 0.20f; // 20%
        public float MinMedianSessionMinutes { get; set; } = 8.0f; // 8 mins
        public float MaxMedianSessionMinutes { get; set; } = 15.0f; // 15 mins
        public float TargetSeason1CompletionRate { get; set; } = 0.35f; // 35%
        public float TargetCrashFreeRate { get; set; } = 0.995f; // 99.5%
        public float TargetAverageRating { get; set; } = 4.0f; // 4.0 / 5.0
    }

    /// <summary>
    /// Aggregate telemetry and engagement metrics snapshot from a beta cohort (#P7-903).
    /// </summary>
    public sealed record BetaCohortMetricsSnapshot(
        int TotalEnrolledUsers,
        int TotalSessionsPlayed,
        float D1RetentionRate,
        float D7RetentionRate,
        float MedianSessionDurationMinutes,
        float Season1CompletionRate,
        float CrashFreeSessionRate,
        float AverageSatisfactionRating
    );

    /// <summary>
    /// Individual metric scorecard grading an observed metric against its target (#P7-903).
    /// </summary>
    public sealed record MetricScorecard(
        string MetricName,
        float ActualValue,
        float TargetValue,
        MetricEvaluationStatus Status,
        string EvaluationDetails
    );

    /// <summary>
    /// Comprehensive KPI scorecard report evaluating beta cohort performance (#P7-903).
    /// </summary>
    public sealed record BetaKpiReport(
        DateTime GeneratedUtc,
        BetaCohortMetricsSnapshot Snapshot,
        IReadOnlyList<MetricScorecard> Scorecards,
        bool MeetsAllKeyTargets
    );

    /// <summary>
    /// Authoritative release decision outcome for public launch (#P7-904).
    /// </summary>
    public enum GoNoGoDecision
    {
        GoForGlobalLaunch = 0,
        ConditionalGoBeta2 = 1,
        NoGoBlocker = 2
    }

    /// <summary>
    /// Pillars of release readiness evaluated during the beta retrospective (#P7-904).
    /// </summary>
    public enum ReleasePillar
    {
        Stability = 0,
        Retention = 1,
        Progression = 2,
        EconomyBalance = 3,
        StoreCompliance = 4,
        UserSentiment = 5
    }

    /// <summary>
    /// Pillar-level evaluation verdict in the release gate report (#P7-904).
    /// </summary>
    public sealed record PillarEvaluation(
        ReleasePillar Pillar,
        MetricEvaluationStatus Status,
        string Rationale
    );

    /// <summary>
    /// Formal release gate evaluation report generated for executive sign-off (#P7-904).
    /// </summary>
    public sealed record ReleaseGateReport(
        DateTime EvaluatedUtc,
        GoNoGoDecision FinalDecision,
        string ExecutiveSummary,
        IReadOnlyList<PillarEvaluation> PillarEvaluations,
        IReadOnlyList<string> RecommendedNextActions
    );
}
