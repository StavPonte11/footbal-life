using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballLife.Domain
{
    /// <summary>
    /// Structured diagnostic snapshot captured during an unhandled exception or critical error (#P7-702).
    /// </summary>
    public sealed record CrashReport
    {
        public string CrashId { get; init; } = Guid.NewGuid().ToString("N");
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public string ExceptionType { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string StackTrace { get; init; } = string.Empty;
        public string? Context { get; init; }
        public bool IsFatal { get; init; }
        public int DeviceMemoryMb { get; init; }
        public float BatteryLevel { get; init; }
        public IReadOnlyList<DiagnosticBreadcrumb> Breadcrumbs { get; init; } = Array.Empty<DiagnosticBreadcrumb>();

        public CrashReport() { }

        public CrashReport(
            string exceptionType,
            string message,
            string stackTrace,
            string? context,
            bool isFatal,
            int deviceMemoryMb,
            float batteryLevel,
            IReadOnlyList<DiagnosticBreadcrumb> breadcrumbs,
            string? crashId = null,
            DateTime? timestampUtc = null)
        {
            CrashId = crashId ?? Guid.NewGuid().ToString("N");
            TimestampUtc = timestampUtc ?? DateTime.UtcNow;
            ExceptionType = exceptionType;
            Message = message;
            StackTrace = stackTrace;
            Context = context;
            IsFatal = isFatal;
            DeviceMemoryMb = deviceMemoryMb;
            BatteryLevel = batteryLevel;
            Breadcrumbs = breadcrumbs ?? Array.Empty<DiagnosticBreadcrumb>();
        }

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string ToJson() => JsonSerializer.Serialize(this, JsonOpts);

        public static CrashReport? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<CrashReport>(json, JsonOpts);
        }
    }
}
