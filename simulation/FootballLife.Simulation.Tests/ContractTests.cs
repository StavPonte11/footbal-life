using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ContractTests
    {
        private readonly Guid _testContractId = Guid.NewGuid();
        private readonly Guid _testPlayerId = Guid.NewGuid();
        private readonly Guid _testClubId = Guid.NewGuid();
        private readonly DateOnly _startDate = new DateOnly(2026, 7, 1);
        private readonly DateOnly _endDate = new DateOnly(2029, 6, 30);

        [Fact]
        public void Contract_CreatedWithValidData_InitializesPropertiesCorrectly()
        {
            var bonuses = new ContractBonuses(
                goalBonus: 5_000m,
                assistBonus: 2_500m,
                appearanceBonus: 1_000m,
                cleanSheetBonus: 3_000m);

            var contract = new Contract(
                _testContractId,
                _testPlayerId,
                _testClubId,
                weeklySalary: 75_000m,
                startDate: _startDate,
                endDate: _endDate,
                contractRole: SquadRole.Starter,
                bonuses: bonuses);

            Assert.Equal(_testContractId, contract.Id);
            Assert.Equal(_testPlayerId, contract.PlayerId);
            Assert.Equal(_testClubId, contract.ClubId);
            Assert.Equal(75_000m, contract.WeeklySalary);
            Assert.Equal(_startDate, contract.StartDate);
            Assert.Equal(_endDate, contract.EndDate);
            Assert.Equal(SquadRole.Starter, contract.ContractRole);
            Assert.Equal(bonuses, contract.Bonuses);
        }

        [Fact]
        public void Contract_CreateFactory_GeneratesUniqueIdAndDefaultBonuses()
        {
            var c1 = Contract.Create(_testPlayerId, _testClubId, 10_000m, _startDate, _endDate, SquadRole.Rotation);
            var c2 = Contract.Create(_testPlayerId, _testClubId, 15_000m, _startDate, _endDate, SquadRole.Starter);

            Assert.NotEqual(Guid.Empty, c1.Id);
            Assert.NotEqual(Guid.Empty, c2.Id);
            Assert.NotEqual(c1.Id, c2.Id);
            Assert.Equal(0m, c1.Bonuses.GoalBonus);
            Assert.Equal(SquadRole.Rotation, c1.ContractRole);
        }

        [Fact]
        public void Contract_WithEmptyId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("id", () =>
                new Contract(Guid.Empty, _testPlayerId, _testClubId, 10_000m, _startDate, _endDate, SquadRole.Squad));
        }

        [Fact]
        public void Contract_WithEmptyPlayerId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("playerId", () =>
                new Contract(_testContractId, Guid.Empty, _testClubId, 10_000m, _startDate, _endDate, SquadRole.Squad));
        }

        [Fact]
        public void Contract_WithEmptyClubId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("clubId", () =>
                new Contract(_testContractId, _testPlayerId, Guid.Empty, 10_000m, _startDate, _endDate, SquadRole.Squad));
        }

        [Fact]
        public void Contract_WithNegativeSalary_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("weeklySalary", () =>
                new Contract(_testContractId, _testPlayerId, _testClubId, -500m, _startDate, _endDate, SquadRole.Squad));
        }

        [Fact]
        public void Contract_WithEndDateBeforeOrEqualToStartDate_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("endDate", () =>
                new Contract(_testContractId, _testPlayerId, _testClubId, 10_000m, _startDate, _startDate, SquadRole.Squad));

            Assert.Throws<ArgumentException>("endDate", () =>
                new Contract(_testContractId, _testPlayerId, _testClubId, 10_000m, _startDate, _startDate.AddDays(-1), SquadRole.Squad));
        }

        // ─── Expiration & Remaining Time ──────────────────────────────────────

        [Fact]
        public void Contract_IsExpired_EvaluatesCorrectly()
        {
            var contract = Contract.Create(_testPlayerId, _testClubId, 10_000m, _startDate, _endDate, SquadRole.Starter);

            Assert.False(contract.IsExpired(new DateOnly(2027, 1, 1)));
            Assert.False(contract.IsExpired(new DateOnly(2029, 6, 29)));
            Assert.True(contract.IsExpired(new DateOnly(2029, 6, 30)));
            Assert.True(contract.IsExpired(new DateOnly(2030, 1, 1)));
        }

        [Fact]
        public void Contract_RemainingDaysAndWeeks_CalculatesAccurately()
        {
            var end = new DateOnly(2026, 7, 22); // 21 days after start
            var contract = Contract.Create(_testPlayerId, _testClubId, 10_000m, _startDate, end, SquadRole.Starter);

            Assert.Equal(21, contract.RemainingDays(_startDate));
            Assert.Equal(3, contract.RemainingWeeks(_startDate));

            // Elapsed date returns 0
            Assert.Equal(0, contract.RemainingDays(new DateOnly(2026, 7, 23)));
            Assert.Equal(0, contract.RemainingWeeks(new DateOnly(2026, 7, 23)));
        }

        // ─── Bonuses & Match Earnings ─────────────────────────────────────────

        [Theory]
        [InlineData(-1, 0, 0, 0)]
        [InlineData(0, -1, 0, 0)]
        [InlineData(0, 0, -1, 0)]
        [InlineData(0, 0, 0, -1)]
        public void ContractBonuses_NegativeValues_ThrowsArgumentOutOfRangeException(
            decimal goal, decimal assist, decimal appearance, decimal cleanSheet)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ContractBonuses(goal, assist, appearance, cleanSheet));
        }

        [Fact]
        public void ContractBonuses_CalculateTotal_AggregatesCorrectly()
        {
            var bonuses = new ContractBonuses(
                goalBonus: 2_000m,
                assistBonus: 1_000m,
                appearanceBonus: 500m,
                cleanSheetBonus: 1_500m);

            // 1 appearance, 2 goals, 1 assist, clean sheet = 500 + 4000 + 1000 + 1500 = 7000
            decimal total = bonuses.CalculateTotal(appearances: 1, goals: 2, assists: 1, cleanSheet: true);
            Assert.Equal(7_000m, total);
        }

        [Fact]
        public void Contract_CalculateMatchEarnings_ReturnsTotalBonuses()
        {
            var bonuses = new ContractBonuses(goalBonus: 3_000m, appearanceBonus: 1_000m);
            var contract = Contract.Create(_testPlayerId, _testClubId, 25_000m, _startDate, _endDate, SquadRole.Starter, bonuses);

            decimal earnings = contract.CalculateMatchEarnings(appearances: 1, goals: 1);
            Assert.Equal(4_000m, earnings);
        }

        [Fact]
        public void SquadRole_AllFourRolesAreDefined()
        {
            var roles = Enum.GetValues(typeof(SquadRole));
            Assert.Equal(4, roles.Length);
        }
    }
}
