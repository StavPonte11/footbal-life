using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a specific round or week of fixtures in the season calendar.
    /// </summary>
    public sealed record Matchday
    {
        public int WeekNumber { get; init; }
        public DateOnly Date { get; init; }
        public IReadOnlyList<ScheduledMatch> Fixtures { get; init; }

        public Matchday(int weekNumber, DateOnly date, IReadOnlyList<ScheduledMatch> fixtures)
        {
            if (weekNumber < 1) throw new ArgumentOutOfRangeException(nameof(weekNumber), "Week number must be >= 1.");
            if (fixtures is null) throw new ArgumentNullException(nameof(fixtures));

            WeekNumber = weekNumber;
            Date = date;
            Fixtures = fixtures;
        }
    }
}
