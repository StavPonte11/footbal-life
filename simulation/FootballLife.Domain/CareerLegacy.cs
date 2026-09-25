using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Overall career historical legacy grade.
    /// </summary>
    public enum LegacyGrade
    {
        Underachiever = 1,
        Journeyman = 2,
        CultHero = 3,
        Icon = 4,
        Legend = 5,
        GOAT = 6
    }

    /// <summary>
    /// Category of trophy won by player.
    /// </summary>
    public enum TrophyCategory
    {
        LeagueTitle = 1,
        ChampionsCup = 2,
        DomesticCup = 3,
        InternationalTournament = 4
    }

    /// <summary>
    /// Record of a major trophy won during player's career.
    /// </summary>
    public sealed record CareerTrophyRecord
    {
        public Guid Id { get; init; }
        public TrophyCategory Category { get; init; }
        public string CompetitionName { get; init; }
        public int Season { get; init; }
        public string ClubOrCountry { get; init; }

        public CareerTrophyRecord(
            Guid id,
            TrophyCategory category,
            string competitionName,
            int season,
            string clubOrCountry)
        {
            if (string.IsNullOrWhiteSpace(competitionName)) throw new ArgumentException("CompetitionName cannot be empty.", nameof(competitionName));
            if (string.IsNullOrWhiteSpace(clubOrCountry)) throw new ArgumentException("ClubOrCountry cannot be empty.", nameof(clubOrCountry));

            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Category = category;
            CompetitionName = competitionName;
            Season = season;
            ClubOrCountry = clubOrCountry;
        }
    }

    /// <summary>
    /// Formal entry memorializing a player in the football Hall of Fame.
    /// </summary>
    public sealed record HallOfFameEntry
    {
        public Guid Id { get; init; }
        public string PlayerName { get; init; }
        public int InductionYear { get; init; }
        public LegacyGrade Grade { get; init; }
        public int CareerScore { get; init; }
        public int TotalAppearances { get; init; }
        public int TotalGoals { get; init; }
        public int TotalAssists { get; init; }
        public int TotalTrophies { get; init; }
        public int InternationalCaps { get; init; }
        public int InternationalGoals { get; init; }
        public long TotalCareerEarnings { get; init; }
        public PostPlayingRole PostCareerRole { get; init; }
        public string PlaqueText { get; init; }

        public HallOfFameEntry(
            Guid id,
            string playerName,
            int inductionYear,
            LegacyGrade grade,
            int careerScore,
            int totalAppearances,
            int totalGoals,
            int totalAssists,
            int totalTrophies,
            int internationalCaps,
            int internationalGoals,
            long totalCareerEarnings,
            PostPlayingRole postCareerRole,
            string? plaqueText = null)
        {
            if (string.IsNullOrWhiteSpace(playerName)) throw new ArgumentException("PlayerName cannot be empty.", nameof(playerName));

            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            PlayerName = playerName;
            InductionYear = inductionYear;
            Grade = grade;
            CareerScore = careerScore;
            TotalAppearances = totalAppearances;
            TotalGoals = totalGoals;
            TotalAssists = totalAssists;
            TotalTrophies = totalTrophies;
            InternationalCaps = internationalCaps;
            InternationalGoals = internationalGoals;
            TotalCareerEarnings = totalCareerEarnings;
            PostCareerRole = postCareerRole;
            PlaqueText = plaqueText ?? $"{playerName} was inducted into the Football Hall of Fame in {inductionYear} with a legacy grade of {grade} (Score: {careerScore}).";
        }
    }

    /// <summary>
    /// Pure C# domain model aggregating complete career statistics, accomplishments, and legacy score.
    /// </summary>
    public sealed record CareerLegacy
    {
        public int LifetimeAppearances { get; init; }
        public int LifetimeGoals { get; init; }
        public int LifetimeAssists { get; init; }
        public int LifetimeCleanSheets { get; init; }
        public int InternationalCaps { get; init; }
        public int InternationalGoals { get; init; }
        public int LeagueTitles { get; init; }
        public int ContinentalTitles { get; init; }
        public int DomesticCups { get; init; }
        public int InternationalTrophies { get; init; }
        public long LifetimeEarnings { get; init; }
        public int PeakOverallRating { get; init; }
        public int SeasonsPlayed { get; init; }
        public int CareerScore { get; init; }
        public LegacyGrade Grade { get; init; }
        public bool IsHallOfFameInductee { get; init; }
        public IReadOnlyList<CareerTrophyRecord> Trophies { get; init; }

        public int TotalTrophies => LeagueTitles + ContinentalTitles + DomesticCups + InternationalTrophies;

        public CareerLegacy(
            int lifetimeAppearances,
            int lifetimeGoals,
            int lifetimeAssists,
            int lifetimeCleanSheets,
            int internationalCaps,
            int internationalGoals,
            int leagueTitles,
            int continentalTitles,
            int domesticCups,
            int internationalTrophies,
            long lifetimeEarnings,
            int peakOverallRating,
            int seasonsPlayed,
            int careerScore,
            LegacyGrade grade,
            bool isHallOfFameInductee,
            IReadOnlyList<CareerTrophyRecord>? trophies = null)
        {
            LifetimeAppearances = Math.Max(0, lifetimeAppearances);
            LifetimeGoals = Math.Max(0, lifetimeGoals);
            LifetimeAssists = Math.Max(0, lifetimeAssists);
            LifetimeCleanSheets = Math.Max(0, lifetimeCleanSheets);
            InternationalCaps = Math.Max(0, internationalCaps);
            InternationalGoals = Math.Max(0, internationalGoals);
            LeagueTitles = Math.Max(0, leagueTitles);
            ContinentalTitles = Math.Max(0, continentalTitles);
            DomesticCups = Math.Max(0, domesticCups);
            InternationalTrophies = Math.Max(0, internationalTrophies);
            LifetimeEarnings = Math.Max(0, lifetimeEarnings);
            PeakOverallRating = Math.Max(0, peakOverallRating);
            SeasonsPlayed = Math.Max(0, seasonsPlayed);
            CareerScore = Math.Max(0, careerScore);
            Grade = grade;
            IsHallOfFameInductee = isHallOfFameInductee;
            Trophies = trophies != null ? new ReadOnlyCollection<CareerTrophyRecord>(trophies.ToList()) : Array.Empty<CareerTrophyRecord>();
        }
    }
}
