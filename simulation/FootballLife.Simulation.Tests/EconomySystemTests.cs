using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class EconomySystemTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2026, 9, 1);

        // ─── P1-045: Weekly Salary Tests ──────────────────────────────────────

        [Fact]
        public void EconomySystem_WeeklySalary_CreditsContractedAmount()
        {
            var account = FinanceAccount.Create(200m);

            var updated = EconomySystem.ApplyWeeklySalary(account, 1250m, TestDate);

            Assert.Equal(1450m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(TransactionType.Salary, updated.History[0].Type);
            Assert.Equal(1250m, updated.History[0].Amount);
            Assert.Contains("1,250", updated.History[0].Description);
        }

        [Fact]
        public void EconomySystem_ZeroSalary_StillCreatesTransactionRecord()
        {
            var account = FinanceAccount.Create(50m);

            var updated = EconomySystem.ApplyWeeklySalary(account, 0m, TestDate);

            Assert.Equal(50m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(TransactionType.Salary, updated.History[0].Type);
            Assert.Equal(0m, updated.History[0].Amount);
        }

        [Fact]
        public void EconomySystem_NegativeSalary_ThrowsArgumentOutOfRangeException()
        {
            var account = FinanceAccount.Create();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                EconomySystem.ApplyWeeklySalary(account, -100m, TestDate));
        }

        [Fact]
        public void EconomySystem_NullAccount_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EconomySystem.ApplyWeeklySalary(null!, 500m, TestDate));
        }

        // ─── P1-046: Match Bonuses Tests ──────────────────────────────────────

        [Fact]
        public void EconomySystem_MatchBonus_CalculatesGoalAndAppearanceCorrectly()
        {
            var account = FinanceAccount.Create(1000m);
            var bonuses = new ContractBonuses(
                goalBonus: 500m,
                assistBonus: 250m,
                appearanceBonus: 100m,
                cleanSheetBonus: 300m);

            var playerId = Guid.NewGuid();
            var events = new List<MatchEvent>
            {
                new MatchEvent(24, MatchEventType.Goal, playerId, "Clinical finish"),
                new MatchEvent(68, MatchEventType.Goal, playerId, "Header")
            };

            var matchResult = new MatchResult(
                homeScore: 2,
                awayScore: 1,
                events: events,
                playerRating: 8.5f,
                playerMinutesPlayed: 90,
                playerScored: true,
                playerAssisted: false);

            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.ST,
                TestDate,
                playerWasHome: true,
                playerId: playerId);

            // Expected: 1 appearance (£100) + 2 goals (£1000) = £1100
            Assert.Equal(2100m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(TransactionType.MatchBonus, updated.History[0].Type);
            Assert.Equal(1100m, updated.History[0].Amount);
            Assert.Contains("Goals (2)", updated.History[0].Description);
        }

        [Fact]
        public void EconomySystem_DefenderCleanSheet_AwardsBonusWhenEligible()
        {
            var account = FinanceAccount.Create(0m);
            var bonuses = new ContractBonuses(
                goalBonus: 100m,
                assistBonus: 100m,
                appearanceBonus: 200m,
                cleanSheetBonus: 500m);

            var matchResult = new MatchResult(
                homeScore: 1,
                awayScore: 0,
                events: Array.Empty<MatchEvent>(),
                playerRating: 7.2f,
                playerMinutesPlayed: 90,
                playerScored: false,
                playerAssisted: false);

            // CB played 90 mins, home club won 1-0 (opposition scored 0)
            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.CB,
                TestDate,
                playerWasHome: true);

            // Appearance (£200) + Clean sheet (£500) = £700
            Assert.Equal(700m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(700m, updated.History[0].Amount);
            Assert.Contains("Clean Sheet", updated.History[0].Description);
        }

        [Fact]
        public void EconomySystem_AttackerCleanSheet_NotAwardedCleanSheetBonus()
        {
            var account = FinanceAccount.Create(0m);
            var bonuses = new ContractBonuses(
                appearanceBonus: 150m,
                cleanSheetBonus: 500m);

            var matchResult = new MatchResult(
                homeScore: 3,
                awayScore: 0,
                events: Array.Empty<MatchEvent>(),
                playerRating: 7.0f,
                playerMinutesPlayed: 90,
                playerScored: false,
                playerAssisted: false);

            // Striker: clean sheet bonus is NOT eligible for attackers
            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.ST,
                TestDate,
                playerWasHome: true);

            // Only appearance bonus £150
            Assert.Equal(150m, updated.Balance);
            Assert.Equal(150m, updated.History[0].Amount);
        }

        [Fact]
        public void EconomySystem_DefenderLessThan60Minutes_CleanSheetNotAwarded()
        {
            var account = FinanceAccount.Create(0m);
            var bonuses = new ContractBonuses(
                appearanceBonus: 100m,
                cleanSheetBonus: 400m);

            var matchResult = new MatchResult(
                homeScore: 0,
                awayScore: 0,
                events: Array.Empty<MatchEvent>(),
                playerRating: 6.0f,
                playerMinutesPlayed: 45, // Less than 60 mins required
                playerScored: false,
                playerAssisted: false);

            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.CB,
                TestDate,
                playerWasHome: true);

            // Only appearance bonus £100 awarded, clean sheet skipped
            Assert.Equal(100m, updated.Balance);
        }

        [Fact]
        public void EconomySystem_OppositionScored_CleanSheetNotAwarded()
        {
            var account = FinanceAccount.Create(0m);
            var bonuses = new ContractBonuses(
                appearanceBonus: 100m,
                cleanSheetBonus: 400m);

            var matchResult = new MatchResult(
                homeScore: 2,
                awayScore: 1, // Opposition away team scored 1
                events: Array.Empty<MatchEvent>(),
                playerRating: 6.5f,
                playerMinutesPlayed: 90,
                playerScored: false,
                playerAssisted: false);

            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.GK,
                TestDate,
                playerWasHome: true);

            Assert.Equal(100m, updated.Balance);
        }

        [Fact]
        public void EconomySystem_DidNotPlay_ZeroBonusEarned_AccountUntouched()
        {
            var account = FinanceAccount.Create(500m);
            var bonuses = new ContractBonuses(appearanceBonus: 200m);

            var matchResult = new MatchResult(
                homeScore: 1,
                awayScore: 0,
                events: Array.Empty<MatchEvent>(),
                playerRating: 6.0f,
                playerMinutesPlayed: 0, // Did not play
                playerScored: false,
                playerAssisted: false);

            var updated = EconomySystem.ApplyMatchBonuses(
                account,
                bonuses,
                matchResult,
                Position.ST,
                TestDate,
                playerWasHome: true);

            // Account is returned untouched with 0 new transactions
            Assert.Equal(500m, updated.Balance);
            Assert.Empty(updated.History);
        }

        // ─── P1-047: Lifestyle Expense Tests ──────────────────────────────────

        [Fact]
        public void EconomySystem_AllLifestyleTiers_HaveExpectedWeeklyCosts()
        {
            Assert.Equal(150m, EconomySystem.GetLifestyleWeeklyCost(LifestyleTier.Modest));
            Assert.Equal(500m, EconomySystem.GetLifestyleWeeklyCost(LifestyleTier.Comfortable));
            Assert.Equal(2000m, EconomySystem.GetLifestyleWeeklyCost(LifestyleTier.Luxurious));
            Assert.Equal(8000m, EconomySystem.GetLifestyleWeeklyCost(LifestyleTier.Extravagant));
            Assert.Equal(25000m, EconomySystem.GetLifestyleWeeklyCost(LifestyleTier.Superstar));
        }

        [Fact]
        public void EconomySystem_LifestyleExpenses_DeductsAccurateTierCost()
        {
            var account = FinanceAccount.Create(5000m);

            var updated = EconomySystem.ApplyLifestyleExpenses(account, LifestyleTier.Luxurious, TestDate);

            Assert.Equal(3000m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(TransactionType.LifestyleExpense, updated.History[0].Type);
            Assert.Equal(-2000m, updated.History[0].Amount);
            Assert.Contains("Luxurious", updated.History[0].Description);
        }

        [Fact]
        public void EconomySystem_Overdraft_AllowsNegativeBalance()
        {
            var account = FinanceAccount.Create(300m);

            var updated = EconomySystem.ApplyLifestyleExpenses(account, LifestyleTier.Comfortable, TestDate);

            // 300 - 500 = -200
            Assert.Equal(-200m, updated.Balance);
            Assert.True(updated.IsInDebt);
            Assert.Equal(200m, updated.DebtAmount);
        }

        // ─── P1-048: Integration & Multi-Week Simulation Tests ─────────────────

        [Fact]
        public void EconomySystem_FullSeasonAccumulation_38Weeks_NetWorthMatchesFormula()
        {
            // Player starts with £0
            var account = FinanceAccount.Create(0m);
            decimal weeklySalary = 1000m;
            var tier = LifestyleTier.Comfortable; // £500/week cost
            decimal netWeeklySavings = weeklySalary - EconomySystem.GetLifestyleWeeklyCost(tier); // £500/week

            var bonuses = new ContractBonuses(appearanceBonus: 100m, goalBonus: 200m);
            var startDate = new DateOnly(2026, 8, 1);

            int totalGoals = 15;
            int totalAppearances = 30; // Played in 30 out of 38 matches

            for (int week = 1; week <= 38; week++)
            {
                var currentDate = startDate.AddDays(week * 7);

                // 1. Weekly salary
                account = EconomySystem.ApplyWeeklySalary(account, weeklySalary, currentDate);

                // 2. Weekly lifestyle expenses
                account = EconomySystem.ApplyLifestyleExpenses(account, tier, currentDate);

                // 3. Match bonus on weeks played
                if (week <= totalAppearances)
                {
                    bool scoredThisWeek = week <= totalGoals;
                    var result = new MatchResult(
                        homeScore: scoredThisWeek ? 2 : 1,
                        awayScore: 0,
                        events: scoredThisWeek
                            ? new List<MatchEvent> { new MatchEvent(35, MatchEventType.Goal, Guid.Empty, "Goal") }
                            : Array.Empty<MatchEvent>(),
                        playerRating: 7.0f,
                        playerMinutesPlayed: 90,
                        playerScored: scoredThisWeek,
                        playerAssisted: false);

                    account = EconomySystem.ApplyMatchBonuses(
                        account,
                        bonuses,
                        result,
                        Position.ST,
                        currentDate);
                }
            }

            decimal expectedSalaryTotal = 38 * 1000m; // £38,000
            decimal expectedExpensesTotal = 38 * 500m; // £19,000
            decimal expectedAppearanceBonuses = totalAppearances * 100m; // £3,000
            decimal expectedGoalBonuses = totalGoals * 200m; // £3,000
            decimal expectedFinalBalance = expectedSalaryTotal - expectedExpensesTotal + expectedAppearanceBonuses + expectedGoalBonuses;

            Assert.Equal(expectedFinalBalance, account.Balance);
            Assert.Equal(25000m, account.Balance); // £19k savings + £6k bonuses = £25,000
            Assert.False(account.IsInDebt);
        }

        [Fact]
        public void EconomySystem_DeterministicFinancialTrajectory_SameContractSamePerformance()
        {
            var account1 = FinanceAccount.Create(500m);
            var account2 = FinanceAccount.Create(500m);

            var bonuses = new ContractBonuses(appearanceBonus: 150m, goalBonus: 350m);
            var date = new DateOnly(2026, 9, 1);

            var result = new MatchResult(2, 0, Array.Empty<MatchEvent>(), 7.5f, 90, true, false);

            account1 = EconomySystem.ApplyWeeklySalary(account1, 2000m, date);
            account1 = EconomySystem.ApplyLifestyleExpenses(account1, LifestyleTier.Luxurious, date);
            account1 = EconomySystem.ApplyMatchBonuses(account1, bonuses, result, Position.ST, date);

            account2 = EconomySystem.ApplyWeeklySalary(account2, 2000m, date);
            account2 = EconomySystem.ApplyLifestyleExpenses(account2, LifestyleTier.Luxurious, date);
            account2 = EconomySystem.ApplyMatchBonuses(account2, bonuses, result, Position.ST, date);

            Assert.Equal(account1.Balance, account2.Balance);
            Assert.Equal(account1.History.Count, account2.History.Count);
            for (int i = 0; i < account1.History.Count; i++)
            {
                Assert.Equal(account1.History[i].Amount, account2.History[i].Amount);
                Assert.Equal(account1.History[i].Type, account2.History[i].Type);
            }
        }

        [Fact]
        public void EconomySystem_DebtScenarios_BalanceTracksAccuratelyAcrossWeeks()
        {
            // Youth player earns £200/wk but picks Extravagant lifestyle (£8,000/wk)
            var account = FinanceAccount.Create(0m);
            decimal youthWage = 200m;
            var luxuryTier = LifestyleTier.Extravagant; // £8,000/wk
            var date = new DateOnly(2026, 9, 1);

            for (int week = 1; week <= 4; week++)
            {
                var curDate = date.AddDays(week * 7);
                account = EconomySystem.ApplyWeeklySalary(account, youthWage, curDate);
                account = EconomySystem.ApplyLifestyleExpenses(account, luxuryTier, curDate);
            }

            // Net weekly: +200 - 8000 = -7,800/wk
            // 4 weeks: -31,200
            Assert.Equal(-31200m, account.Balance);
            Assert.True(account.IsInDebt);
            Assert.Equal(31200m, account.DebtAmount);
            Assert.Equal(8, account.History.Count);
        }
    }
}
