using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# simulation system calculating career retrospective scores,
    /// assigning legacy tiers/grades, and orchestrating Hall of Fame inductions.
    /// </summary>
    public sealed class LegacySystem
    {
        public const int GoatScoreThreshold = 900;
        public const int LegendScoreThreshold = 700;
        public const int IconScoreThreshold = 500;
        public const int CultHeroScoreThreshold = 350;
        public const int JourneymanScoreThreshold = 200;

        /// <summary>
        /// Calculates the comprehensive lifetime career score from statistical performance,
        /// trophies, international accomplishments, peak rating, and commercial success.
        /// </summary>
        public int CalculateCareerScore(
            int appearances,
            int goals,
            int assists,
            int cleanSheets,
            int internationalCaps,
            int internationalGoals,
            int leagueTitles,
            int continentalTitles,
            int domesticCups,
            int internationalTrophies,
            int peakOvr,
            long lifetimeEarnings)
        {
            double score = 0;

            // 1. Appearances & Longevity (max 150)
            score += Math.Min(150.0, appearances * 0.25);

            // 2. Goal Contributions (goals + assists)
            score += Math.Min(250.0, goals * 2.0);
            score += Math.Min(150.0, assists * 1.5);
            score += Math.Min(100.0, cleanSheets * 1.5);

            // 3. Trophies & Silverware
            score += leagueTitles * 60.0;
            score += continentalTitles * 120.0;
            score += domesticCups * 30.0;
            score += internationalTrophies * 150.0;

            // 4. International Football
            score += Math.Min(100.0, internationalCaps * 1.0);
            score += Math.Min(100.0, internationalGoals * 3.0);

            // 5. Peak Overall Rating
            if (peakOvr > 70)
            {
                score += (peakOvr - 70) * 2.5;
            }

            // 6. Wealth & Commercial Longevity
            if (lifetimeEarnings > 0)
            {
                score += Math.Min(100.0, (double)lifetimeEarnings / 500_000.0);
            }

            return (int)Math.Round(score);
        }

        /// <summary>
        /// Maps a numeric career score to its descriptive historical Legacy Grade.
        /// </summary>
        public LegacyGrade DetermineGrade(int careerScore)
        {
            if (careerScore >= GoatScoreThreshold) return LegacyGrade.GOAT;
            if (careerScore >= LegendScoreThreshold) return LegacyGrade.Legend;
            if (careerScore >= IconScoreThreshold) return LegacyGrade.Icon;
            if (careerScore >= CultHeroScoreThreshold) return LegacyGrade.CultHero;
            if (careerScore >= JourneymanScoreThreshold) return LegacyGrade.Journeyman;
            return LegacyGrade.Underachiever;
        }

        /// <summary>
        /// Evaluates whether a player's career meets the bar for Hall of Fame induction.
        /// </summary>
        public bool IsEligibleForHallOfFame(CareerLegacy legacy)
        {
            if (legacy is null) return false;

            // Automatic induction for Legend or GOAT status
            if (legacy.Grade >= LegacyGrade.Legend) return true;

            // Silverware threshold: 3+ major titles (League or Continental)
            if ((legacy.LeagueTitles + legacy.ContinentalTitles) >= 3) return true;

            // High statistical performance threshold
            if (legacy.CareerScore >= 600) return true;

            // Century club international captain
            if (legacy.InternationalCaps >= 100 && legacy.TotalTrophies >= 1) return true;

            return false;
        }

        /// <summary>
        /// Inducts a retired player into the Hall of Fame, formatting their commemorative plaque.
        /// </summary>
        public HallOfFameEntry InductIntoHallOfFame(
            Player player,
            CareerLegacy legacy,
            PostPlayingRole postRole,
            int inductionYear)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (legacy is null) throw new ArgumentNullException(nameof(legacy));

            string plaque = GeneratePlaqueText(player.Name, legacy, postRole, inductionYear);

            return new HallOfFameEntry(
                id: Guid.NewGuid(),
                playerName: player.Name,
                inductionYear: inductionYear,
                grade: legacy.Grade,
                careerScore: legacy.CareerScore,
                totalAppearances: legacy.LifetimeAppearances,
                totalGoals: legacy.LifetimeGoals,
                totalAssists: legacy.LifetimeAssists,
                totalTrophies: legacy.TotalTrophies,
                internationalCaps: legacy.InternationalCaps,
                internationalGoals: legacy.InternationalGoals,
                totalCareerEarnings: legacy.LifetimeEarnings,
                postCareerRole: postRole,
                plaqueText: plaque);
        }

        private static string GeneratePlaqueText(
            string playerName,
            CareerLegacy legacy,
            PostPlayingRole postRole,
            int year)
        {
            string trophySummary = legacy.TotalTrophies switch
            {
                0 => "a celebrated cult career of grit and passion",
                1 => "1 major silverware title",
                _ => $"{legacy.TotalTrophies} major championship trophies including {legacy.LeagueTitles} league title(s) and {legacy.ContinentalTitles} continental cup(s)"
            };

            string statsSummary = $"{legacy.LifetimeAppearances} appearances, {legacy.LifetimeGoals} goals, and {legacy.LifetimeAssists} assists";
            if (legacy.InternationalCaps > 0)
            {
                statsSummary += $", proudly representing their nation across {legacy.InternationalCaps} caps and {legacy.InternationalGoals} international goals";
            }

            return $"HALL OF FAME INDUCTEE — CLASS OF {year}\n\n" +
                   $"In recognition of an extraordinary footballing career culminating in {legacy.Grade.ToString().ToUpperInvariant()} status (Career Score: {legacy.CareerScore}).\n" +
                   $"{playerName} graced the pitch across {statsSummary}, securing {trophySummary}.\n\n" +
                   $"A titan of the sport whose legacy endures as they continue their footballing journey as a {postRole}.";
        }
    }
}
