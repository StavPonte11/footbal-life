using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Immutable domain model tracking the live state of a match including score, time, and event log.
    /// Can be serialized, debugged, and replayed from any point.
    /// </summary>
    public sealed record MatchState
    {
        public Guid HomeClubId { get; init; }
        public Guid AwayClubId { get; init; }
        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public int Minute { get; init; }
        public bool IsFinished { get; init; }
        public IReadOnlyList<MatchEvent> EventLog { get; init; }

        public MatchState(
            Guid homeClubId,
            Guid awayClubId,
            int homeScore,
            int awayScore,
            int minute,
            bool isFinished,
            IReadOnlyList<MatchEvent>? eventLog = null)
        {
            if (homeClubId == Guid.Empty)
                throw new ArgumentException("HomeClubId cannot be empty.", nameof(homeClubId));
            if (awayClubId == Guid.Empty)
                throw new ArgumentException("AwayClubId cannot be empty.", nameof(awayClubId));
            if (homeClubId == awayClubId)
                throw new ArgumentException("HomeClubId and AwayClubId cannot be identical.");
            if (homeScore < 0)
                throw new ArgumentOutOfRangeException(nameof(homeScore), "HomeScore cannot be negative.");
            if (awayScore < 0)
                throw new ArgumentOutOfRangeException(nameof(awayScore), "AwayScore cannot be negative.");
            if (minute < 0 || minute > 120)
                throw new ArgumentOutOfRangeException(nameof(minute), $"Minute must be within [0, 120]. Actual: {minute}");

            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            HomeScore = homeScore;
            AwayScore = awayScore;
            Minute = minute;
            IsFinished = isFinished;
            EventLog = eventLog != null ? new List<MatchEvent>(eventLog).AsReadOnly() : Array.Empty<MatchEvent>();
        }

        public static MatchState Create(Guid homeClubId, Guid awayClubId)
        {
            return new MatchState(
                homeClubId: homeClubId,
                awayClubId: awayClubId,
                homeScore: 0,
                awayScore: 0,
                minute: 0,
                isFinished: false,
                eventLog: Array.Empty<MatchEvent>());
        }

        /// <summary>
        /// Appends an event to the match event log and returns a new immutable <see cref="MatchState"/>.
        /// </summary>
        public MatchState AddEvent(MatchEvent e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            var newLog = new List<MatchEvent>(EventLog.Count + 1);
            newLog.AddRange(EventLog);
            newLog.Add(e);

            return this with { EventLog = newLog.AsReadOnly() };
        }

        /// <summary>
        /// Records a goal, incrementing the score for the home or away team and appending Goal (and optional Assist) events.
        /// </summary>
        public MatchState WithGoal(int minute, Guid scorerId, Guid? assisterId, bool isHome)
        {
            if (minute < 0 || minute > 120)
                throw new ArgumentOutOfRangeException(nameof(minute), $"Minute must be within [0, 120]. Actual: {minute}");
            if (scorerId == Guid.Empty)
                throw new ArgumentException("ScorerId cannot be empty.", nameof(scorerId));

            var newLog = new List<MatchEvent>(EventLog.Count + 2);
            newLog.AddRange(EventLog);

            if (assisterId.HasValue && assisterId.Value != Guid.Empty)
            {
                newLog.Add(new MatchEvent(minute, MatchEventType.Assist, assisterId.Value, "Assisted goal"));
            }

            newLog.Add(new MatchEvent(minute, MatchEventType.Goal, scorerId, isHome ? "Goal scored by home team" : "Goal scored by away team"));

            return this with
            {
                HomeScore = isHome ? HomeScore + 1 : HomeScore,
                AwayScore = isHome ? AwayScore : AwayScore + 1,
                Minute = Math.Max(Minute, minute),
                EventLog = newLog.AsReadOnly()
            };
        }
    }
}
