using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the overall campaign structure for a competitive year,
    /// tracking current week, calendar matchdays, and current standings.
    /// </summary>
    public sealed record Season
    {
        public int Year { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public int CurrentWeek { get; init; }
        public IReadOnlyList<Matchday> Matchdays { get; init; }
        public LeagueTable Table { get; init; }

        public Season(
            int year,
            DateOnly startDate,
            DateOnly endDate,
            LeagueTable table,
            IReadOnlyList<Matchday> matchdays,
            int currentWeek = 1)
        {
            if (year < 1900 || year > 2200) throw new ArgumentOutOfRangeException(nameof(year));
            if (endDate <= startDate) throw new ArgumentException("EndDate must be after StartDate.");
            if (table is null) throw new ArgumentNullException(nameof(table));
            if (matchdays is null) throw new ArgumentNullException(nameof(matchdays));
            if (currentWeek < 1) throw new ArgumentOutOfRangeException(nameof(currentWeek));

            Year = year;
            StartDate = startDate;
            EndDate = endDate;
            Table = table;
            Matchdays = matchdays;
            CurrentWeek = currentWeek;
        }

        public Season WithNextWeek() => this with { CurrentWeek = CurrentWeek + 1 };

        public Season WithUpdatedTable(LeagueTable newTable)
        {
            if (newTable is null) throw new ArgumentNullException(nameof(newTable));
            return this with { Table = newTable };
        }
    }
}
