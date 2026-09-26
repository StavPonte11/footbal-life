using System;
using System.Collections.Generic;
using System.Globalization;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Represents the statistical snapshot of a single simulated season within a player's career.
    /// </summary>
    public sealed record SeasonRecord(
        int SeasonYear,
        int Age,
        string ClubName,
        int LeagueTier,
        decimal WeeklySalary,
        SquadStatus SquadStatus,
        int Appearances,
        int Goals,
        int Assists,
        float AverageRating,
        int EndOverall,
        decimal EarningsThisSeason,
        decimal EndBalance);

    /// <summary>
    /// Represents the comprehensive historical and statistical record of a player's entire simulated career.
    /// </summary>
    public sealed record CareerStatistics(
        int Seed,
        Guid PlayerId,
        string Name,
        Position Position,
        int StartingOverall,
        int PeakOverall,
        int PeakAge,
        int RetirementAge,
        int SeasonsPlayed,
        int TotalAppearances,
        int TotalGoals,
        int TotalAssists,
        float AverageRating,
        decimal TotalEarnings,
        decimal FinalBalance,
        decimal PeakWeeklySalary,
        bool BankruptcyOccurred,
        int TransferCount,
        IReadOnlyList<SeasonRecord> Seasons,
        decimal CommercialEarnings = 0m,
        int TotalTrophies = 0,
        int InternationalCaps = 0,
        int InternationalGoals = 0,
        int CareerScore = 0,
        LegacyGrade LegacyGrade = LegacyGrade.Journeyman,
        bool IsHallOfFame = false)
    {
        public const string CsvHeader = "Seed,PlayerId,Name,Position,StartingOverall,PeakOverall,PeakAge,RetirementAge,SeasonsPlayed,TotalAppearances,TotalGoals,TotalAssists,AverageRating,TotalEarnings,FinalBalance,PeakWeeklySalary,BankruptcyOccurred,TransferCount,CommercialEarnings,TotalTrophies,InternationalCaps,InternationalGoals,CareerScore,LegacyGrade,IsHallOfFame";

        public string ToCsvLine()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},\"{2}\",{3},{4},{5},{6},{7},{8},{9},{10},{11},{12:F2},{13:F2},{14:F2},{15:F2},{16},{17},{18:F2},{19},{20},{21},{22},{23},{24}",
                Seed,
                PlayerId,
                Name,
                Position,
                StartingOverall,
                PeakOverall,
                PeakAge,
                RetirementAge,
                SeasonsPlayed,
                TotalAppearances,
                TotalGoals,
                TotalAssists,
                AverageRating,
                TotalEarnings,
                FinalBalance,
                PeakWeeklySalary,
                BankruptcyOccurred ? 1 : 0,
                TransferCount,
                CommercialEarnings,
                TotalTrophies,
                InternationalCaps,
                InternationalGoals,
                CareerScore,
                LegacyGrade,
                IsHallOfFame ? 1 : 0);
        }
    }
}
