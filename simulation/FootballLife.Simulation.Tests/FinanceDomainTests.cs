using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class FinanceDomainTests
    {
        [Fact]
        public void FinanceAccount_InitialBalance_IsZero_ByDefault()
        {
            var account = FinanceAccount.Create();

            Assert.Equal(0m, account.Balance);
            Assert.Empty(account.History);
            Assert.False(account.IsInDebt);
            Assert.Equal(0m, account.DebtAmount);
        }

        [Fact]
        public void FinanceAccount_CreatedWithInitialBalance_RetainsValue()
        {
            var account = FinanceAccount.Create(1500m);

            Assert.Equal(1500m, account.Balance);
            Assert.Empty(account.History);
            Assert.False(account.IsInDebt);
        }

        [Fact]
        public void FinanceAccount_PositiveTransaction_IncreasesBalance()
        {
            var account = FinanceAccount.Create(500m);
            var date = new DateOnly(2026, 9, 1);
            var tx = FinanceTransaction.Create(date, TransactionType.Salary, 1200m, "Weekly salary");

            var updated = account.WithTransaction(tx);

            Assert.Equal(1700m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(tx.Id, updated.History[0].Id);
            Assert.Equal(1200m, updated.History[0].Amount);
            Assert.Equal(TransactionType.Salary, updated.History[0].Type);
            Assert.Equal(date, updated.History[0].Date);

            // Immutability: original account untouched
            Assert.Equal(500m, account.Balance);
            Assert.Empty(account.History);
        }

        [Fact]
        public void FinanceAccount_NegativeTransaction_DecreasesBalance()
        {
            var account = FinanceAccount.Create(1000m);
            var date = new DateOnly(2026, 9, 8);
            var tx = FinanceTransaction.Create(date, TransactionType.LifestyleExpense, -300m, "Lifestyle rent");

            var updated = account.WithTransaction(tx);

            Assert.Equal(700m, updated.Balance);
            Assert.Single(updated.History);
            Assert.Equal(-300m, updated.History[0].Amount);
            Assert.Equal(TransactionType.LifestyleExpense, updated.History[0].Type);
            Assert.False(updated.IsInDebt);
        }

        [Fact]
        public void FinanceAccount_HistoryPreservesChronologicalOrder()
        {
            var account = FinanceAccount.Create(0m);
            var d1 = new DateOnly(2026, 9, 1);
            var d2 = new DateOnly(2026, 9, 8);
            var d3 = new DateOnly(2026, 9, 15);

            var tx1 = FinanceTransaction.Create(d1, TransactionType.Salary, 500m, "Week 1 wage");
            var tx2 = FinanceTransaction.Create(d2, TransactionType.LifestyleExpense, -150m, "Week 1 expenses");
            var tx3 = FinanceTransaction.Create(d3, TransactionType.MatchBonus, 200m, "Goal bonus");

            var updated = account
                .WithTransaction(tx1)
                .WithTransaction(tx2)
                .WithTransaction(tx3);

            Assert.Equal(550m, updated.Balance);
            Assert.Equal(3, updated.History.Count);
            Assert.Equal(tx1.Id, updated.History[0].Id);
            Assert.Equal(tx2.Id, updated.History[1].Id);
            Assert.Equal(tx3.Id, updated.History[2].Id);
        }

        [Fact]
        public void FinanceAccount_IsInDebt_ReflectsNegativeBalance()
        {
            var account = FinanceAccount.Create(100m);
            var tx = FinanceTransaction.Create(new DateOnly(2026, 9, 1), TransactionType.LifestyleExpense, -500m, "Heavy expenses");

            var updated = account.WithTransaction(tx);

            Assert.Equal(-400m, updated.Balance);
            Assert.True(updated.IsInDebt);
            Assert.Equal(400m, updated.DebtAmount);
        }

        [Fact]
        public void FinanceTransaction_EmptyGuid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new FinanceTransaction(
                Guid.Empty,
                new DateOnly(2026, 9, 1),
                TransactionType.Salary,
                500m,
                "Test"));
        }

        private static Season CreateTestSeason()
        {
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var table = LeagueTable.Create(leagueId, new[] { clubId });
            return new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), table, new List<Matchday>());
        }

        [Fact]
        public void WorldState_AccountsFacet_StoresAndRetrievesFinanceAccount()
        {
            var season = CreateTestSeason();
            var world = WorldState.CreateEmpty(season);
            var playerId = Guid.NewGuid();
            var account = FinanceAccount.Create(2500m);

            var updatedWorld = world.WithPlayerAccount(playerId, account);

            Assert.True(updatedWorld.Accounts.ContainsKey(playerId));
            Assert.Equal(2500m, updatedWorld.GetAccount(playerId).Balance);

            Assert.True(updatedWorld.TryGetAccount(playerId, out var foundAccount));
            Assert.Equal(2500m, foundAccount.Balance);
        }

        [Fact]
        public void WorldState_GetAccount_MissingPlayer_ThrowsKeyNotFoundException()
        {
            var season = CreateTestSeason();
            var world = WorldState.CreateEmpty(season);
            var missingId = Guid.NewGuid();

            Assert.Throws<KeyNotFoundException>(() => world.GetAccount(missingId));
            Assert.False(world.TryGetAccount(missingId, out _));
        }

        [Fact]
        public void PlayerFactory_CreatesCareer_WithInitialFinanceAccount()
        {
            var season = CreateTestSeason();
            var leagueId = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var league = new League(leagueId, "Test League", "ENG", 1, 20, 38, 0, 3);
            var finances = new ClubFinances(50_000m, 100_000m);
            var club = new Club(
                clubId,
                "Test Academy FC",
                "ENG",
                leagueId,
                50,
                finances,
                3,
                TacticalIdentity.Possession);

            var world = WorldState.CreateEmpty(season).WithLeague(league).WithClub(club);
            var rng = new SimulationRandom(42);

            var args = new PlayerCreationArgs(
                "Marcus Hope",
                "England",
                new DateOnly(2007, 5, 15),
                Position.ST,
                Foot.Right,
                clubId,
                55);

            var updatedWorld = PlayerFactory.CreateCareer(world, args, rng);

            // Find the created player ID from the club roster
            var updatedClub = updatedWorld.GetClub(clubId);
            var createdPlayerId = updatedClub.SquadPlayerIds[0];

            // Verify account exists in WorldState with initial zero balance
            Assert.True(updatedWorld.Accounts.ContainsKey(createdPlayerId));
            var account = updatedWorld.GetAccount(createdPlayerId);
            Assert.Equal(0m, account.Balance);
            Assert.Empty(account.History);
        }
    }
}
