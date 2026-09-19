using System;
using System.Collections.Generic;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class SeasonTests
    {
        private readonly Guid _leagueId = Guid.NewGuid();
        private readonly Guid _clubA = Guid.NewGuid();
        private readonly Guid _clubB = Guid.NewGuid();
        private readonly Guid _clubC = Guid.NewGuid();
        private readonly DateOnly _startDate = new DateOnly(2026, 8, 15);
        private readonly DateOnly _endDate = new DateOnly(2027, 5, 23);

        // ─── FixtureId Tests ──────────────────────────────────────────────────

        [Fact]
        public void FixtureId_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var matchDate = new DateOnly(2026, 8, 20);
            var fixture = FixtureId.Create(_clubA, _clubB, matchDate);

            Assert.NotEqual(Guid.Empty, fixture.Id);
            Assert.Equal(_clubA, fixture.HomeClubId);
            Assert.Equal(_clubB, fixture.AwayClubId);
            Assert.Equal(matchDate, fixture.Date);
        }

        [Fact]
        public void FixtureId_SameHomeAndAwayClub_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                FixtureId.Create(_clubA, _clubA, new DateOnly(2026, 8, 20)));
        }

        // ─── Matchday Tests ───────────────────────────────────────────────────

        [Fact]
        public void Matchday_CreatedWithValidData_InitializesCorrectly()
        {
            var leagueId = Guid.NewGuid();
            var fixtures = new List<ScheduledMatch> { ScheduledMatch.Create(_startDate, _clubA, _clubB, leagueId) };
            var matchday = new Matchday(1, _startDate, fixtures);

            Assert.Equal(1, matchday.WeekNumber);
            Assert.Equal(_startDate, matchday.Date);
            Assert.Single(matchday.Fixtures);
        }

        [Fact]
        public void Matchday_InvalidWeek_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Matchday(0, _startDate, new List<ScheduledMatch>()));
        }

        // ─── LeagueTableRow Tests ─────────────────────────────────────────────

        [Fact]
        public void LeagueTableRow_PointsAndGoalDifference_ComputedCorrectly()
        {
            // Played = 5, Won = 3, Drawn = 1, Lost = 1. Points = 3*3 + 1 = 10. GD = 8 - 4 = 4.
            var row = new LeagueTableRow(_clubA, played: 5, won: 3, drawn: 1, lost: 1, goalsFor: 8, goalsAgainst: 4);

            Assert.Equal(10, row.Points);
            Assert.Equal(4, row.GoalDifference);
        }

        [Fact]
        public void LeagueTableRow_PlayedNotEqualToSumOfOutcomes_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new LeagueTableRow(_clubA, played: 5, won: 2, drawn: 1, lost: 1)); // 2+1+1 = 4 != 5
        }

        [Fact]
        public void LeagueTableRow_WithMatchOutcome_Win_UpdatesAccurately()
        {
            var initial = LeagueTableRow.Initial(_clubA);
            var afterWin = initial.WithMatchOutcome(goalsScored: 3, goalsConceded: 1);

            Assert.Equal(1, afterWin.Played);
            Assert.Equal(1, afterWin.Won);
            Assert.Equal(0, afterWin.Drawn);
            Assert.Equal(0, afterWin.Lost);
            Assert.Equal(3, afterWin.GoalsFor);
            Assert.Equal(1, afterWin.GoalsAgainst);
            Assert.Equal(2, afterWin.GoalDifference);
            Assert.Equal(3, afterWin.Points);
        }

        [Fact]
        public void LeagueTableRow_WithMatchOutcome_DrawAndLoss_UpdatesAccurately()
        {
            var initial = LeagueTableRow.Initial(_clubA);
            var afterDraw = initial.WithMatchOutcome(2, 2);
            var afterLoss = afterDraw.WithMatchOutcome(0, 1);

            Assert.Equal(2, afterLoss.Played);
            Assert.Equal(0, afterLoss.Won);
            Assert.Equal(1, afterLoss.Drawn);
            Assert.Equal(1, afterLoss.Lost);
            Assert.Equal(2, afterLoss.GoalsFor);
            Assert.Equal(3, afterLoss.GoalsAgainst);
            Assert.Equal(-1, afterLoss.GoalDifference);
            Assert.Equal(1, afterLoss.Points);
        }

        // ─── LeagueTable Sorting Tests ────────────────────────────────────────

        [Fact]
        public void LeagueTable_Sorting_PointsTakePriorityOverGoalDifference()
        {
            // Club A has 6 pts (+2 GD), Club B has 4 pts (+5 GD)
            var rowA = new LeagueTableRow(_clubA, played: 2, won: 2, drawn: 0, lost: 0, goalsFor: 4, goalsAgainst: 2);
            var rowB = new LeagueTableRow(_clubB, played: 2, won: 1, drawn: 1, lost: 0, goalsFor: 6, goalsAgainst: 1);

            var table = new LeagueTable(_leagueId, new[] { rowB, rowA });

            Assert.Equal(_clubA, table.Rows[0].ClubId); // Club A 1st
            Assert.Equal(_clubB, table.Rows[1].ClubId); // Club B 2nd
        }

        [Fact]
        public void LeagueTable_Sorting_GoalDifferenceBreaksTies()
        {
            // Both have 3 pts. Club A: +3 GD, Club B: +1 GD
            var rowA = new LeagueTableRow(_clubA, played: 1, won: 1, drawn: 0, lost: 0, goalsFor: 4, goalsAgainst: 1);
            var rowB = new LeagueTableRow(_clubB, played: 1, won: 1, drawn: 0, lost: 0, goalsFor: 2, goalsAgainst: 1);

            var table = new LeagueTable(_leagueId, new[] { rowB, rowA });

            Assert.Equal(_clubA, table.Rows[0].ClubId);
            Assert.Equal(_clubB, table.Rows[1].ClubId);
        }

        [Fact]
        public void LeagueTable_Sorting_GoalsForBreaksSecondTie()
        {
            // Both have 3 pts and +2 GD. Club A scored 4, Club B scored 3.
            var rowA = new LeagueTableRow(_clubA, played: 1, won: 1, drawn: 0, lost: 0, goalsFor: 4, goalsAgainst: 2);
            var rowB = new LeagueTableRow(_clubB, played: 1, won: 1, drawn: 0, lost: 0, goalsFor: 3, goalsAgainst: 1);

            var table = new LeagueTable(_leagueId, new[] { rowB, rowA });

            Assert.Equal(_clubA, table.Rows[0].ClubId);
            Assert.Equal(_clubB, table.Rows[1].ClubId);
        }

        [Fact]
        public void LeagueTable_WithMatchResult_UpdatesBothClubsAndResorts()
        {
            var table = LeagueTable.Create(_leagueId, new[] { _clubA, _clubB });

            // Club B wins 2-0 away at Club A
            var updated = table.WithMatchResult(_clubA, _clubB, homeGoals: 0, awayGoals: 2);

            Assert.Equal(_clubB, updated.Rows[0].ClubId); // Club B 1st with 3 pts
            Assert.Equal(_clubA, updated.Rows[1].ClubId); // Club A 2nd with 0 pts
            Assert.Equal(1, updated.GetPositionOf(_clubB));
            Assert.Equal(2, updated.GetPositionOf(_clubA));
        }

        // ─── Season Tests ─────────────────────────────────────────────────────

        [Fact]
        public void Season_CreatedWithValidData_InitializesCorrectly()
        {
            var table = LeagueTable.Create(_leagueId, new[] { _clubA, _clubB });
            var season = new Season(
                year: 2026,
                startDate: _startDate,
                endDate: _endDate,
                table: table,
                matchdays: new List<Matchday>(),
                currentWeek: 1);

            Assert.Equal(2026, season.Year);
            Assert.Equal(_startDate, season.StartDate);
            Assert.Equal(_endDate, season.EndDate);
            Assert.Equal(1, season.CurrentWeek);
            Assert.Equal(table, season.Table);
        }

        [Fact]
        public void Season_WithNextWeek_IncrementsCurrentWeekImmutably()
        {
            var table = LeagueTable.Create(_leagueId, new[] { _clubA, _clubB });
            var season = new Season(2026, _startDate, _endDate, table, new List<Matchday>(), 1);

            var nextWeek = season.WithNextWeek();

            Assert.Equal(1, season.CurrentWeek);
            Assert.Equal(2, nextWeek.CurrentWeek);
        }

        [Fact]
        public void Season_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            var table = LeagueTable.Create(_leagueId, new[] { _clubA });
            Assert.Throws<ArgumentException>(() =>
                new Season(2026, _endDate, _startDate, table, new List<Matchday>()));
        }
    }
}
