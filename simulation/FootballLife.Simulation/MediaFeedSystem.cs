using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# simulation system that procedurally generates journalistic match reports,
    /// transfer gossip columns, and lifestyle features with zero external network dependencies (#P4-009).
    /// Conforms strictly to the LLM Authority Boundary (generates flavour text from simulation state).
    /// </summary>
    public static class MediaFeedSystem
    {
        /// <summary>
        /// Generates a curated batch of sports journalism articles tailored to the player's
        /// current match form, club standing, transfer speculation, and lifestyle prestige.
        /// </summary>
        public static List<MediaArticle> GenerateArticles(CareerSaveData save, MatchResult? recentMatch = null)
        {
            save ??= new CareerSaveData();
            var articles = new List<MediaArticle>();
            var today = new DateOnly(2026, 9, 25);

            // 1. Match Report or Form Analysis
            if (recentMatch != null)
            {
                bool won = recentMatch.HomeScore > recentMatch.AwayScore;
                if (won)
                {
                    articles.Add(new MediaArticle
                    {
                        Id = $"art_match_win_{save.CurrentWeek}",
                        Title = $"{save.PlayerName} Shines as {save.ClubName} Secure Masterful Victory",
                        Subtitle = "Tactical discipline and clinical finishing dismantle the opposition at home.",
                        Body = $"In a high-tempo contest that captivated the stadium, {save.ClubName} delivered an emphatic victory. {save.PlayerName} operated with relentless energy in the attacking third, dragging defenders out of position and creating pivotal scoring sequences. Manager trust appears at an all-time high following this disciplined tactical exhibition.",
                        Author = "Marcus Vance",
                        Publication = "The Athletic",
                        CategoryTag = "MATCHDAY",
                        PublishedDate = today,
                        IconEmoji = "⚽",
                        LikesCount = 2840 + save.FanPopularity * 45,
                        ViewsCount = 14200 + save.FanPopularity * 120
                    });
                }
                else
                {
                    articles.Add(new MediaArticle
                    {
                        Id = $"art_match_loss_{save.CurrentWeek}",
                        Title = $"Heartbreak for {save.ClubName}: Attack Stifled in Tough Defeat",
                        Subtitle = "Missed opportunities prove costly as defensive lapses decide the contest.",
                        Body = $"Despite periods of territorial dominance, {save.ClubName} could not overcome a resilient defensive blockade today. {save.PlayerName} found space difficult to come by under intense pressure. Tactical adjustments will undoubtedly be on the agenda ahead of next week's crucial fixture.",
                        Author = "James Thornton",
                        Publication = "Sky Sports News",
                        CategoryTag = "MATCHDAY",
                        PublishedDate = today,
                        IconEmoji = "📉",
                        LikesCount = 1120 + save.FanPopularity * 20,
                        ViewsCount = 9800 + save.FanPopularity * 80
                    });
                }
            }
            else
            {
                articles.Add(new MediaArticle
                {
                    Id = $"art_form_spotlight_{save.CurrentWeek}",
                    Title = $"Scouting Spotlight: The Evolution of {save.PlayerName}",
                    Subtitle = $"Why {save.ClubName}'s young prospect is turning heads across the division.",
                    Body = $"With an overall rating of {save.OverallRating} and impressive tactical positioning, {save.PlayerName} continues to establish their footprint in professional football. Scouts note exceptional composure under pressure and a rising physical ceiling that could make them a franchise cornerstone.",
                    Author = "Marcus Vance",
                    Publication = "The Athletic",
                    CategoryTag = "TACTICS",
                    PublishedDate = today,
                    IconEmoji = "🔍",
                    LikesCount = 1950 + save.FanPopularity * 30,
                    ViewsCount = 11000 + save.FanPopularity * 90
                });
            }

            // 2. Transfer Rumor Mill
            string transferHeadline = save.OverallRating >= 72
                ? $"Continental Giants Weigh £{(save.MarketValue * 1.5m):N0} Swoop for {save.PlayerName}"
                : $"Championship Rivals Monitor {save.PlayerName}'s Contract Situation";

            articles.Add(new MediaArticle
            {
                Id = $"art_transfer_{save.CurrentWeek}",
                Title = transferHeadline,
                Subtitle = "European scouts sighted in the directors' box during recent fixtures.",
                Body = $"Speculation continues to mount surrounding {save.PlayerName}'s future at {save.ClubName}. Sources close to the player's representation indicate growing interest from top-tier recruiters, though the club hierarchy remains adamant that their prized prospect is not for sale at any discount.",
                Author = "Chloe Bennett",
                Publication = "The Daily Mirror",
                CategoryTag = "TRANSFER",
                PublishedDate = today,
                IconEmoji = "💼",
                LikesCount = 3450 + save.FanPopularity * 50,
                ViewsCount = 18500 + save.FanPopularity * 150
            });

            // 3. Lifestyle & Culture Column
            var prop = HomePropertyCatalog.GetProperty((LifestyleTier)save.LifestyleTier);
            string propName = prop?.Name ?? "Modern City Apartment";
            articles.Add(new MediaArticle
            {
                Id = $"art_lifestyle_{save.CurrentWeek}",
                Title = $"Life Off the Pitch: {save.PlayerName}'s Rising Brand & Lifestyle",
                Subtitle = $"Balancing athletic discipline with luxury living in {propName}.",
                Body = $"Modern footballers are cultural icons as much as athletes. From high-end luxury wellness equipment to designer city apartments, {save.PlayerName} is cultivating an aspirational image that resonates with football enthusiasts and commercial sponsors worldwide.",
                Author = "Elena Ward",
                Publication = "GQ Sports Style",
                CategoryTag = "LIFESTYLE",
                PublishedDate = today,
                IconEmoji = "✨",
                LikesCount = 2200 + save.FanPopularity * 40,
                ViewsCount = 13000 + save.FanPopularity * 100
            });

            // 4. Locker Room & Manager Dynamic
            articles.Add(new MediaArticle
            {
                Id = $"art_insider_{save.CurrentWeek}",
                Title = $"Inside {save.ClubName}: Manager Trust & Dressing Room Harmony",
                Subtitle = "How team bonding and training standards are driving the squad's ambition.",
                Body = $"Behind closed doors at the training complex, high standards are being demanded by the coaching staff. Teammate chemistry and tactical commitment remain paramount as the season reaches its defining stretch.",
                Author = "Liam O'Connor",
                Publication = "BBC Football",
                CategoryTag = "INSIDER",
                PublishedDate = today,
                IconEmoji = "🛡️",
                LikesCount = 1680 + save.FanPopularity * 25,
                ViewsCount = 8900 + save.FanPopularity * 70
            });

            return articles;
        }

        /// <summary>
        /// Helper conforming to `llm-game-integration` schema for formatting
        /// a structured LLM prompt if runtime OpenAI/Gemini commentary is enabled.
        /// </summary>
        public static string BuildLLMPrompt(CareerSaveData save, string eventContext)
        {
            return $"You are a sports journalist. Write a 2-sentence journalistic headline and teaser for player '{save.PlayerName}' of club '{save.ClubName}'. Context: {eventContext}. Respond strictly in JSON: {{ \"headline\": \"...\", \"teaser\": \"...\" }}";
        }
    }
}
