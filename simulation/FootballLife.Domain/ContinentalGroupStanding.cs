using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Entry for a single club within a continental group stage table.
    /// </summary>
    public sealed record GroupEntry
    {
        public Guid ClubId { get; init; }
        public int Played { get; init; }
        public int Won { get; init; }
        public int Drawn { get; init; }
        public int Lost { get; init; }
        public int GoalsFor { get; init; }
        public int GoalsAgainst { get; init; }
        public int Points => (Won * 3) + Drawn;
        public int GoalDifference => GoalsFor - GoalsAgainst;

        public GroupEntry(
            Guid clubId,
            int played = 0,
            int won = 0,
            int drawn = 0,
            int lost = 0,
            int goalsFor = 0,
            int goalsAgainst = 0)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("Club ID cannot be empty.", nameof(clubId));

            ClubId = clubId;
            Played = played;
            Won = won;
            Drawn = drawn;
            Lost = lost;
            GoalsFor = goalsFor;
            GoalsAgainst = goalsAgainst;
        }

        /// <summary>
        /// Returns a new entry with a match result applied.
        /// </summary>
        public GroupEntry WithMatchResult(int goalsFor, int goalsAgainst)
        {
            bool isWin = goalsFor > goalsAgainst;
            bool isDraw = goalsFor == goalsAgainst;
            return this with
            {
                Played = Played + 1,
                Won = Won + (isWin ? 1 : 0),
                Drawn = Drawn + (isDraw ? 1 : 0),
                Lost = Lost + (!isWin && !isDraw ? 1 : 0),
                GoalsFor = GoalsFor + goalsFor,
                GoalsAgainst = GoalsAgainst + goalsAgainst
            };
        }
    }

    /// <summary>
    /// Represents a group within a continental competition (e.g., Group A with 4 clubs).
    /// </summary>
    public sealed record ContinentalGroupStanding
    {
        /// <summary>
        /// Group identifier (e.g., "A", "B", "C"...).
        /// </summary>
        public string GroupName { get; init; }

        /// <summary>
        /// Entries for each club in the group, ordered by points then goal difference.
        /// </summary>
        public IReadOnlyList<GroupEntry> Entries { get; init; }

        public ContinentalGroupStanding(
            string groupName,
            IReadOnlyList<GroupEntry> entries)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Group name cannot be null or whitespace.", nameof(groupName));
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("Group must have at least one entry.", nameof(entries));

            GroupName = groupName.Trim();
            Entries = entries;
        }

        /// <summary>
        /// Returns entries sorted by points (desc), goal difference (desc), goals for (desc).
        /// </summary>
        public IReadOnlyList<GroupEntry> GetRankedEntries()
        {
            return Entries
                .OrderByDescending(e => e.Points)
                .ThenByDescending(e => e.GoalDifference)
                .ThenByDescending(e => e.GoalsFor)
                .ToList();
        }

        /// <summary>
        /// Returns the club ID of the group winner (1st place).
        /// </summary>
        public Guid GetGroupWinner()
        {
            return GetRankedEntries()[0].ClubId;
        }

        /// <summary>
        /// Returns the club ID of the group runner-up (2nd place).
        /// </summary>
        public Guid GetGroupRunnerUp()
        {
            var ranked = GetRankedEntries();
            return ranked.Count > 1 ? ranked[1].ClubId : Guid.Empty;
        }
    }
}
