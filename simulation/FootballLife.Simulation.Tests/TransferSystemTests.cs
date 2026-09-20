using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class TransferSystemTests
    {
        private static readonly DateOnly CurrentDate = new DateOnly(2026, 8, 1);

        private static (WorldState world, Player player, Club currentClub, Club targetClub1, Club targetClub2) CreateTestEnvironment(float managerTrust = 60f)
        {
            var season = new Season(
                2026,
                CurrentDate,
                new DateOnly(2027, 5, 30),
                new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()),
                Array.Empty<Matchday>(),
                1);

            var world = WorldState.CreateEmpty(season);

            var leagueTier2 = League.Create("Championship", "ENG", 2, 24, 46, 2, 3);
            var leagueTier3 = League.Create("League One", "ENG", 3, 24, 46, 2, 3);
            var leagueTier4 = League.Create("League Two", "ENG", 4, 24, 46, 2, 3);

            world = world.WithLeague(leagueTier2).WithLeague(leagueTier3).WithLeague(leagueTier4);

            var finances = new ClubFinances(5000000m, 50000m);
            var currentClub = Club.Create("Bristol City", "BRC", leagueTier2.Id, 55, finances, 3, TacticalIdentity.Possession);
            var targetClub1 = Club.Create("Derby County", "DER", leagueTier2.Id, 58, finances, 3, TacticalIdentity.HighPress);
            var targetClub2 = Club.Create("Peterborough", "PET", leagueTier3.Id, 45, finances, 2, TacticalIdentity.Direct);
            var targetClub3 = Club.Create("Grimsby Town", "GRI", leagueTier4.Id, 32, finances, 1, TacticalIdentity.Counter);

            world = world.WithClub(currentClub).WithClub(targetClub1).WithClub(targetClub2).WithClub(targetClub3);

            var playerId = Guid.NewGuid();
            var player = new Player(playerId, "Tom Davis", "ENG", new DateOnly(2005, 5, 12), Foot.Right, Position.ST);
            var abilities = new PlayerAbilities(60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60);
            var state = new PlayerState(15, 65, 70, 75, 80, 80, 75);
            var career = new PlayerCareerState(currentClub.Id, SquadStatus.Rotation, managerTrust, 1200m, 150000m, 40f);
            var potential = new PlayerPotential(78, PotentialRange.Medium);
            var account = FinanceAccount.Create(5000m);

            var teammateRel = Relationship.Create(playerId, "Jack (Teammate)", RelationshipType.Teammate, 80f, 75f, CurrentDate);
            var partnerRel = Relationship.Create(playerId, "Emma (Partner)", RelationshipType.Partner, 70f, 80f, CurrentDate);
            var managerRel = Relationship.Create(playerId, "Coach Miller", RelationshipType.Manager, 50f, 50f, CurrentDate);

            world = world.WithPlayer(player, abilities, state, career, potential, account, new[] { teammateRel, partnerRel, managerRel });

            // Assign player to current club squad
            currentClub = currentClub.WithAddedPlayer(playerId);
            world = world.WithClub(currentClub);

            // Register current contract
            var contract = Contract.Create(playerId, currentClub.Id, 1200m, CurrentDate, CurrentDate.AddYears(2), SquadRole.Rotation);
            world = world.WithContract(contract);

            return (world, player, currentClub, targetClub1, targetClub2);
        }

        [Fact]
        public void GenerateOffers_GeneratesSuitorOffersExcludingCurrentClub()
        {
            var (world, player, currentClub, _, _) = CreateTestEnvironment();
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var contract = world.GetContractForPlayer(player.Id);
            var rng = new SimulationRandom(42);

            var offers = TransferSystem.GenerateOffers(player, abilities, career, contract, world, rng, CurrentDate);

            Assert.NotEmpty(offers);
            Assert.True(offers.Count <= TransferSystem.MaxSimultaneousOffers);

            // Ensure current club never bids for own player
            Assert.DoesNotContain(offers, o => o.OfferingClubId == currentClub.Id);

            foreach (var offer in offers)
            {
                Assert.Equal(player.Id, offer.PlayerId);
                Assert.True(offer.OfferedWage > 0m);
                Assert.True(offer.ContractYears >= 1 && offer.ContractYears <= 5);
                Assert.Equal(TransferOfferStatus.Pending, offer.Status);
            }
        }

        [Fact]
        public void GenerateOffers_LowManagerTrust_GuaranteesEscapeOffer()
        {
            // Low trust player (trust = 15 < 30)
            var (world, player, _, _, _) = CreateTestEnvironment(managerTrust: 15f);
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var contract = world.GetContractForPlayer(player.Id);
            var rng = new SimulationRandom(101);

            var offers = TransferSystem.GenerateOffers(player, abilities, career, contract, world, rng, CurrentDate);

            // Must produce at least 1 escape offer
            Assert.NotEmpty(offers);
        }

        [Fact]
        public void GenerateOffers_DeterministicGivenSameSeed()
        {
            var (world, player, _, _, _) = CreateTestEnvironment();
            var abilities = world.GetAbilities(player.Id);
            var career = world.GetCareerState(player.Id);
            var contract = world.GetContractForPlayer(player.Id);

            var rng1 = new SimulationRandom(999);
            var offers1 = TransferSystem.GenerateOffers(player, abilities, career, contract, world, rng1, CurrentDate);

            var rng2 = new SimulationRandom(999);
            var offers2 = TransferSystem.GenerateOffers(player, abilities, career, contract, world, rng2, CurrentDate);

            Assert.Equal(offers1.Count, offers2.Count);
            for (int i = 0; i < offers1.Count; i++)
            {
                Assert.Equal(offers1[i].OfferingClubId, offers2[i].OfferingClubId);
                Assert.Equal(offers1[i].OfferedWage, offers2[i].OfferedWage);
                Assert.Equal(offers1[i].ContractYears, offers2[i].ContractYears);
                Assert.Equal(offers1[i].TransferFee, offers2[i].TransferFee);
            }
        }

        [Fact]
        public void AcceptOffer_TransfersRosterContractAndFinancials()
        {
            var (world, player, currentClub, targetClub1, _) = CreateTestEnvironment();
            var initialAccount = world.GetAccount(player.Id);

            var offer = TransferOffer.Create(
                playerId: player.Id,
                offeringClubId: targetClub1.Id,
                offeredRole: SquadRole.Starter,
                offeredWage: 2500m,
                transferFee: 180000m,
                contractYears: 3,
                offerDate: CurrentDate,
                expiryDate: CurrentDate.AddDays(20),
                releaseClause: 500000m,
                signingBonus: 10000m);

            world = world.WithTransferOffer(offer);

            var transferDate = CurrentDate.AddDays(5);
            var updatedWorld = TransferSystem.AcceptOffer(offer, world, transferDate);

            // 1. Check Career State
            var updatedCareer = updatedWorld.GetCareerState(player.Id);
            Assert.Equal(targetClub1.Id, updatedCareer.ClubId);
            Assert.Equal(2500m, updatedCareer.WeeklySalary);
            Assert.Equal(SquadStatus.Starter, updatedCareer.Status);
            Assert.Equal(55f, updatedCareer.ManagerTrust); // Starter initial trust

            // 2. Check Club Rosters
            var updatedOldClub = updatedWorld.GetClub(currentClub.Id);
            var updatedNewClub = updatedWorld.GetClub(targetClub1.Id);
            Assert.DoesNotContain(player.Id, updatedOldClub.SquadPlayerIds);
            Assert.Contains(player.Id, updatedNewClub.SquadPlayerIds);

            // 3. Check Finances
            var updatedAccount = updatedWorld.GetAccount(player.Id);
            Assert.Equal(initialAccount.Balance + 10000m, updatedAccount.Balance);
            Assert.Contains(updatedAccount.History, t => t.Type == TransactionType.TransferBonus && t.Amount == 10000m);

            // 4. Check Relationships
            var rels = updatedWorld.GetPlayerRelationships(player.Id);
            var partnerRel = rels.First(r => r.Type == RelationshipType.Partner);
            Assert.True(partnerRel.Trust > 80f); // High affinity partner trust boost
            var managerRel = rels.First(r => r.Type == RelationshipType.Manager);
            Assert.True(managerRel.Affinity < 50f); // Former manager cooled down

            // 5. Check Offer Status
            var acceptedOffer = updatedWorld.GetTransferOffer(offer.Id);
            Assert.Equal(TransferOfferStatus.Accepted, acceptedOffer.Status);
        }

        [Fact]
        public void AcceptOffer_WithdrawnOtherPendingOffers()
        {
            var (world, player, _, targetClub1, targetClub2) = CreateTestEnvironment();

            var offer1 = TransferOffer.Create(player.Id, targetClub1.Id, SquadRole.Starter, 2000m, 100000m, 3, CurrentDate, CurrentDate.AddDays(15));
            var offer2 = TransferOffer.Create(player.Id, targetClub2.Id, SquadRole.Rotation, 2200m, 120000m, 3, CurrentDate, CurrentDate.AddDays(15));

            world = world.WithTransferOffer(offer1).WithTransferOffer(offer2);

            var updatedWorld = TransferSystem.AcceptOffer(offer1, world, CurrentDate.AddDays(2));

            Assert.Equal(TransferOfferStatus.Accepted, updatedWorld.GetTransferOffer(offer1.Id).Status);
            Assert.Equal(TransferOfferStatus.Withdrawn, updatedWorld.GetTransferOffer(offer2.Id).Status);
        }

        [Fact]
        public void AcceptOffer_ExpiredOffer_ThrowsInvalidOperationException()
        {
            var (world, player, _, targetClub1, _) = CreateTestEnvironment();

            var offer = TransferOffer.Create(player.Id, targetClub1.Id, SquadRole.Starter, 2000m, 100000m, 3, CurrentDate, CurrentDate.AddDays(10));
            world = world.WithTransferOffer(offer);

            // Accept 12 days after current date (expired)
            Assert.Throws<InvalidOperationException>(() => TransferSystem.AcceptOffer(offer, world, CurrentDate.AddDays(12)));
        }

        [Fact]
        public void RejectOffer_SetsStatusToRejected()
        {
            var (world, player, _, targetClub1, _) = CreateTestEnvironment();

            var offer = TransferOffer.Create(player.Id, targetClub1.Id, SquadRole.Rotation, 1500m, 50000m, 2, CurrentDate, CurrentDate.AddDays(10));
            world = world.WithTransferOffer(offer);

            var updatedWorld = TransferSystem.RejectOffer(offer, world);

            Assert.Equal(TransferOfferStatus.Rejected, updatedWorld.GetTransferOffer(offer.Id).Status);
        }
    }
}
