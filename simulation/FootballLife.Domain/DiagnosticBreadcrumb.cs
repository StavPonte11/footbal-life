using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Category classification for diagnostic breadcrumbs.
    /// </summary>
    public enum DiagnosticBreadcrumbCategory
    {
        UI = 0,
        Match = 1,
        Simulation = 2,
        Persistence = 3,
        Network = 4,
        System = 5
    }

    /// <summary>
    /// Lightweight chronological marker recorded during player progression and gameplay.
    /// Included in crash and exception dumps to diagnose user reproduction steps (#P7-702).
    /// </summary>
    public sealed record DiagnosticBreadcrumb(
        DateTime TimestampUtc,
        DiagnosticBreadcrumbCategory Category,
        string Message,
        IReadOnlyDictionary<string, string>? Data = null
    )
    {
        public static DiagnosticBreadcrumb Create(
            DiagnosticBreadcrumbCategory category,
            string message,
            IReadOnlyDictionary<string, string>? data = null)
        {
            return new DiagnosticBreadcrumb(
                TimestampUtc: DateTime.UtcNow,
                Category: category,
                Message: message,
                Data: data
            );
        }
    }
}
