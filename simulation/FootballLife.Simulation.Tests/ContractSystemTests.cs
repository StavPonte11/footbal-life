using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class ContractSystemTests
    {
        private static readonly DateOnly CurrentDate = new DateOnly(2026, 8, 1);

        private static (WorldState world, Player player, Club club, Contract contract) CreateContractEnvironment(
            float managerTrust = 60f,
            float agentAffinity = 50f,
            int contractRemainingMonths = 24)
        {
            var season = new Season(
                2026,
                CurrentDate,
                new DateOnly(2027, 5, 30),
                new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()),
                Array.Empty<Matchday>(),
                1);

            var world = WorldState.CreateEmpty(season);

            var league = League.Create("Premier League", "ENG", 1, 20, 38, 0, 3);
            world = world.WithLeague(league);

            var finances = new ClubFinances(20000000m, 200000m);
            var club = Club.Create("Arsenal", "ARS", league.Id, 85, finances, 5, TacticalIdentity.Possession);
            world = world.WithClub(club);

            var playerId = Guid.NewGuid();
            var player = new Player(playerId, "Liam Walker", "ENG", new DateOnly(2004, 3, 10), Foot.Left, Position.LW);
            var abilities = new PlayerAbilities(75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75);
            var state = new PlayerState(10, 70, 75, 80, 85, 80, 80);
            var career = new PlayerCareerState(club.Id, SquadStatus.Starter, managerTrust, 15000m, 12000000m, 70f);
            var potential = new PlayerPotential(88, PotentialRange.Low);
            var account = FinanceAccount.Create(25000m);

            var agentRel = Relationship.Create(playerId, "Julian Vance (Agent)", RelationshipType.Agent, agentAffinity, 70f, CurrentDate);
            world = world.WithPlayer(player, abilities, state, career, potential, account, new[] { agentRel });

            // Squad assignment
            club = club.WithAddedPlayer(playerId);
            world = world.WithClub(club);

            // Contract
            DateOnly endDate = CurrentDate.AddMonths(contractRemainingMonths);
            var contract = Contract.Create(playerId, club.Id, 15000m, CurrentDate.AddYears(-1), endDate, SquadRole.Starter);
            world = world.WithContract(contract);

            return (world, player, club, contract);
        }

        [Fact]
        public void EvaluateContractStatus_MoreThanSixMonths_ReturnsActive()
        {
            var (_, _, _, contract) = CreateContractEnvironment(contractRemainingMonths: 18);
            var status = ContractSystem.EvaluateContractStatus(contract, CurrentDate);
            Assert.Equal(ContractExpiryStatus.Active, status);
        }

        [Fact]
        public void EvaluateContractStatus_SixMonthsOrLess_ReturnsBosmanEligible()
        {
            var (_, _, _, contract) = CreateContractEnvironment(contractRemainingMonths: 5); // 5 months <= 182 days
            var status = ContractSystem.EvaluateContractStatus(contract, CurrentDate);
            Assert.Equal(ContractExpiryStatus.BosmanEligible, status);
        }

        [Fact]
        public void EvaluateContractStatus_PastEndDate_ReturnsExpired()
        {
            var (_, _, _, contract) = CreateContractEnvironment(contractRemainingMonths: 1);
            var status = ContractSystem.EvaluateContractStatus(contract, CurrentDate.AddMonths(2));
            Assert.Equal(ContractExpiryStatus.Expired, status);
        }

        [Fact]
        public void NegotiateTerms_ReasonableDemand_Accepted()
        {
            var (world, player, club, _) = CreateContractEnvironment();
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var rng = new SimulationRandom(42);

            // Ask for 40,000/wk (within normal fair valuation of ~40,800)
            var result = ContractSystem.NegotiateTerms(player, abilities, career, club, demandedWage: 40000m, demandedYears: 3, world, rng);

            Assert.Equal(ContractNegotiationStatus.Accepted, result.Status);
            Assert.Equal(40000m, result.AgreedWage);
            Assert.Equal(3, result.AgreedYears);
            Assert.True(result.AgreedSigningBonus > 0m);
        }

        [Fact]
        public void NegotiateTerms_SlightlyElevatedDemand_Counters()
        {
            var (world, player, club, _) = CreateContractEnvironment();
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var rng = new SimulationRandom(42);

            // Ask for 48,000/wk (elevated above 1.10x fair valuation but below 1.30x)
            var result = ContractSystem.NegotiateTerms(player, abilities, career, club, demandedWage: 48000m, demandedYears: 4, world, rng);

            Assert.Equal(ContractNegotiationStatus.CounterOffer, result.Status);
            Assert.True(result.AgreedWage >= 40000m && result.AgreedWage <= 48000m);
            Assert.Equal(4, result.AgreedYears);
        }

        [Fact]
        public void NegotiateTerms_ExorbitantDemand_WalksAway()
        {
            var (world, player, club, _) = CreateContractEnvironment();
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var rng = new SimulationRandom(42);

            // Absurd ask: 120,000/wk (> 1.30x tolerance ceiling)
            var result = ContractSystem.NegotiateTerms(player, abilities, career, club, demandedWage: 120000m, demandedYears: 5, world, rng);

            Assert.Equal(ContractNegotiationStatus.WalkedAway, result.Status);
            Assert.Equal(0m, result.AgreedWage);
        }

        [Fact]
        public void NegotiateTerms_HighAgentAffinity_IncreasesWageAcceptanceThreshold()
        {
            // Low agent affinity (10%) vs high agent affinity (100%)
            var (worldLow, playerLow, clubLow, _) = CreateContractEnvironment(agentAffinity: 10f);
            var (worldHigh, playerHigh, clubHigh, _) = CreateContractEnvironment(agentAffinity: 100f);

            var abilities = worldLow.GetAbilities(playerLow.Id);
            var career = worldLow.GetCareerState(playerLow.Id);

            decimal testWage = 48000m;

            var resLow = ContractSystem.NegotiateTerms(playerLow, abilities, career, clubLow, testWage, 3, worldLow, new SimulationRandom(1));
            var resHigh = ContractSystem.NegotiateTerms(playerHigh, abilities, career, clubHigh, testWage, 3, worldHigh, new SimulationRandom(1));

            // With high agent affinity, player gets accepted directly where low affinity triggers a counter
            Assert.Equal(ContractNegotiationStatus.CounterOffer, resLow.Status);
            Assert.Equal(ContractNegotiationStatus.Accepted, resHigh.Status);
        }

        [Fact]
        public void OfferRenewal_HighTrust_GeneratesRenewalProposal()
        {
            var (world, player, club, contract) = CreateContractEnvironment(managerTrust: 75f);
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var rng = new SimulationRandom(123);

            var renewalOffer = ContractSystem.OfferRenewal(player, abilities, career, contract, club, world, rng, CurrentDate);

            Assert.NotNull(renewalOffer);
            Assert.Equal(club.Id, renewalOffer.OfferingClubId);
            Assert.True(renewalOffer.OfferedWage >= contract.WeeklySalary);
            Assert.True(renewalOffer.ContractYears >= 2);
        }

        [Fact]
        public void OfferRenewal_LowTrust_RefusesToRenew()
        {
            var (world, player, club, contract) = CreateContractEnvironment(managerTrust: 20f);
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var rng = new SimulationRandom(123);

            var renewalOffer = ContractSystem.OfferRenewal(player, abilities, career, contract, club, world, rng, CurrentDate);

            Assert.Null(renewalOffer);
        }

        [Fact]
        public void HandleContractExpiry_ExpiredContract_ReleasesPlayerToFreeAgency()
        {
            var (world, player, club, contract) = CreateContractEnvironment(contractRemainingMonths: 1);

            // Advance date past contract expiration
            var expiredDate = contract.EndDate.AddDays(1);
            var updatedWorld = ContractSystem.HandleContractExpiry(player, contract, world, expiredDate);

            var updatedCareer = updatedWorld.GetCareerState(player.Id);
            Assert.Equal(Guid.Empty, updatedCareer.ClubId);
            Assert.Equal(0m, updatedCareer.WeeklySalary);
            Assert.Equal(SquadStatus.Reserve, updatedCareer.Status);

            var updatedClub = updatedWorld.GetClub(club.Id);
            Assert.DoesNotContain(player.Id, updatedClub.SquadPlayerIds);
        }

        [Fact]
        public void HandleContractExpiry_ActiveContract_DoesNotMutateState()
        {
            var (world, player, club, contract) = CreateContractEnvironment(contractRemainingMonths: 12);

            var updatedWorld = ContractSystem.HandleContractExpiry(player, contract, world, CurrentDate);

            var career = updatedWorld.GetCareerState(player.Id);
            Assert.Equal(club.Id, career.ClubId);
            Assert.Equal(15000m, career.WeeklySalary);
        }
    }
}
