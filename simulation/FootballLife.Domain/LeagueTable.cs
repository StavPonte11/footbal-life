using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents the complete sorted league standings table.
    /// Automatically maintains sorting by Points DESC -> GoalDifference DESC -> GoalsFor DESC.
    /// </summary>
    public sealed record LeagueTable
    {
        public Guid LeagueId { get; init; }
        public IReadOnlyList<LeagueTableRow> Rows { get; init; }

        public LeagueTable(Guid leagueId, IEnumerable<LeagueTableRow> rows)
        {
            if (leagueId == Guid.Empty) throw new ArgumentException("League ID cannot be empty.", nameof(leagueId));
            if (rows is null) throw new ArgumentNullException(nameof(rows));

            LeagueId = leagueId;
            var list = rows.ToList();
            list.Sort();
            Rows = list.AsReadOnly();
        }

        public static LeagueTable Create(Guid leagueId, IEnumerable<Guid> clubIds)
        {
            if (clubIds is null) throw new ArgumentNullException(nameof(clubIds));
            var initialRows = clubIds.Select(LeagueTableRow.Initial);
            return new LeagueTable(leagueId, initialRows);
        }

        /// <summary>
        /// Applies a match score between home and away clubs, returning a new sorted LeagueTable.
        /// </summary>
        public LeagueTable WithMatchResult(Guid homeClubId, Guid awayClubId, int homeGoals, int awayGoals)
        {
            var updatedRows = new List<LeagueTableRow>(Rows.Count);

            foreach (var row in Rows)
            {
                if (row.ClubId == homeClubId)
                {
                    updatedRows.Add(row.WithMatchOutcome(homeGoals, awayGoals));
                }
                else if (row.ClubId == awayClubId)
                {
                    updatedRows.Add(row.WithMatchOutcome(awayGoals, homeGoals));
                }
                else
                {
                    updatedRows.Add(row);
                }
            }

            return new LeagueTable(LeagueId, updatedRows);
        }

        /// <summary>
        /// Returns the 1-based ranking position of a club in the standings.
        /// </summary>
        public int GetPositionOf(Guid clubId)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].ClubId == clubId) return i + 1;
            }
            throw new KeyNotFoundException($"Club {clubId} is not in this league table.");
        }
    }
}
