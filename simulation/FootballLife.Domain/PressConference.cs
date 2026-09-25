using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Tone of the player's press conference response (#P4-009).
    /// </summary>
    public enum PressTone
    {
        Humble,
        Confident,
        Defiant,
        Diplomatic
    }

    /// <summary>
    /// Journalist temperament and reporting angle (#P4-009).
    /// </summary>
    public enum JournalistTemperament
    {
        Supportive,
        Sensationalist,
        Tactical
    }

    /// <summary>
    /// Information about an attending journalist and their media outlet (#P4-009).
    /// </summary>
    public sealed record Journalist
    {
        public string Name { get; init; } = string.Empty;
        public string Outlet { get; init; } = string.Empty;
        public JournalistTemperament Temperament { get; init; }
        public string AvatarEmoji { get; init; } = "🎙️";
    }

    /// <summary>
    /// Response choice for a press question with stat impact deltas (#P4-009).
    /// </summary>
    public sealed record PressResponseChoice
    {
        public PressTone Tone { get; init; }
        public string Text { get; init; } = string.Empty;
        public float ManagerTrustDelta { get; init; }
        public float FanPopularityDelta { get; init; }
        public float TeammateMoraleDelta { get; init; }
        public float MediaReputationDelta { get; init; }
        public string ConsequenceSummary { get; init; } = string.Empty;
    }

    /// <summary>
    /// Question posed by a journalist during a press conference (#P4-009).
    /// </summary>
    public sealed record PressQuestion
    {
        public string Id { get; init; } = string.Empty;
        public Journalist Journalist { get; init; } = new();
        public string QuestionText { get; init; } = string.Empty;
        public string Category { get; init; } = "General"; // "Matchday", "Tactics", "Transfer", "Lifestyle"
        public IReadOnlyList<PressResponseChoice> Responses { get; init; } = Array.Empty<PressResponseChoice>();
    }

    /// <summary>
    /// A structured press conference event with questions and narrative context (#P4-009).
    /// </summary>
    public sealed record PressConference
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = "Press Briefing";
        public string ContextDescription { get; init; } = string.Empty;
        public IReadOnlyList<PressQuestion> Questions { get; init; } = Array.Empty<PressQuestion>();
    }
}
