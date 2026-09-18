using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Individual standings row for a club in a league table.
    /// Invariant: Played == Won + Drawn + Lost. Points == Won * 3 + Drawn.
    /// </summary>
    public sealed record LeagueTableRow : IComparable<LeagueTableRow>
    {
        public Guid ClubId { get; init; }
        public int Played { get; init; }
        public int Won { get; init; }
        public int Drawn { get; init; }
        public int Lost { get; init; }
        public int GoalsFor { get; init; }
        public int GoalsAgainst { get; init; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => (Won * 3) + Drawn;

        public LeagueTableRow(
            Guid clubId,
            int played = 0,
            int won = 0,
            int drawn = 0,
            int lost = 0,
            int goalsFor = 0,
            int goalsAgainst = 0)
        {
            if (clubId == Guid.Empty) throw new ArgumentException("Club ID cannot be empty.", nameof(clubId));
            if (played < 0) throw new ArgumentOutOfRangeException(nameof(played));
            if (won < 0) throw new ArgumentOutOfRangeException(nameof(won));
            if (drawn < 0) throw new ArgumentOutOfRangeException(nameof(drawn));
            if (lost < 0) throw new ArgumentOutOfRangeException(nameof(lost));
            if (goalsFor < 0) throw new ArgumentOutOfRangeException(nameof(goalsFor));
            if (goalsAgainst < 0) throw new ArgumentOutOfRangeException(nameof(goalsAgainst));

            if (played != won + drawn + lost)
            {
                throw new ArgumentException($"Matches played ({played}) must equal won ({won}) + drawn ({drawn}) + lost ({lost}).");
            }

            ClubId = clubId;
            Played = played;
            Won = won;
            Drawn = drawn;
            Lost = lost;
            GoalsFor = goalsFor;
            GoalsAgainst = goalsAgainst;
        }

        public static LeagueTableRow Initial(Guid clubId) =>
            new LeagueTableRow(clubId, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Records a completed match outcome and returns an updated row.
        /// </summary>
        public LeagueTableRow WithMatchOutcome(int goalsScored, int goalsConceded)
        {
            if (goalsScored < 0) throw new ArgumentOutOfRangeException(nameof(goalsScored));
            if (goalsConceded < 0) throw new ArgumentOutOfRangeException(nameof(goalsConceded));

            int newWon = Won + (goalsScored > goalsConceded ? 1 : 0);
            int newDrawn = Drawn + (goalsScored == goalsConceded ? 1 : 0);
            int newLost = Lost + (goalsScored < goalsConceded ? 1 : 0);

            return new LeagueTableRow(
                ClubId,
                played: Played + 1,
                won: newWon,
                drawn: newDrawn,
                lost: newLost,
                goalsFor: GoalsFor + goalsScored,
                goalsAgainst: GoalsAgainst + goalsConceded);
        }

        /// <summary>
        /// Compares two rows for league standings: Points DESC, GoalDifference DESC, GoalsFor DESC.
        /// </summary>
        public int CompareTo(LeagueTableRow? other)
        {
            if (other is null) return -1;

            int pointsComp = other.Points.CompareTo(Points);
            if (pointsComp != 0) return pointsComp;

            int gdComp = other.GoalDifference.CompareTo(GoalDifference);
            if (gdComp != 0) return gdComp;

            return other.GoalsFor.CompareTo(GoalsFor);
        }
    }
}
