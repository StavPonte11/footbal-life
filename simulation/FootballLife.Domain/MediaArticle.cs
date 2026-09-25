using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Pure C# domain model representing a sports journalism news article,
    /// match report, or transfer column (#P4-009).
    /// </summary>
    public sealed record MediaArticle
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string Author { get; init; } = "Sports Desk";
        public string Publication { get; init; } = "Football Daily";
        public string CategoryTag { get; init; } = "MATCHDAY"; // "MATCHDAY", "TRANSFER", "OPINION", "LIFESTYLE"
        public DateOnly PublishedDate { get; init; }
        public string IconEmoji { get; init; } = "📰";
        public int LikesCount { get; init; }
        public int ViewsCount { get; init; }
    }
}
