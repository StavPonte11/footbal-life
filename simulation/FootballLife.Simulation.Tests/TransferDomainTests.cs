using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class TransferDomainTests
    {
        private static readonly DateOnly OfferDate = new DateOnly(2026, 8, 1);
        private static readonly DateOnly ExpiryDate = new DateOnly(2026, 8, 25);

        [Fact]
        public void TransferOffer_ConstructsAndValidatesParameters()
        {
            var offerId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            var offer = new TransferOffer(
                id: offerId,
                playerId: playerId,
                offeringClubId: clubId,
                offeredRole: SquadRole.Starter,
                offeredWage: 1250m,
                transferFee: 50000m,
                contractYears: 3,
                offerDate: OfferDate,
                expiryDate: ExpiryDate,
                releaseClause: 150000m,
                signingBonus: 5000m,
                status: TransferOfferStatus.Pending);

            Assert.Equal(offerId, offer.Id);
            Assert.Equal(playerId, offer.PlayerId);
            Assert.Equal(clubId, offer.OfferingClubId);
            Assert.Equal(SquadRole.Starter, offer.OfferedRole);
            Assert.Equal(1250m, offer.OfferedWage);
            Assert.Equal(50000m, offer.TransferFee);
            Assert.Equal(3, offer.ContractYears);
            Assert.Equal(OfferDate, offer.OfferDate);
            Assert.Equal(ExpiryDate, offer.ExpiryDate);
            Assert.Equal(150000m, offer.ReleaseClause);
            Assert.Equal(5000m, offer.SigningBonus);
            Assert.Equal(TransferOfferStatus.Pending, offer.Status);
        }

        [Fact]
        public void TransferOffer_InvalidParameters_ThrowsException()
        {
            var id = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            // Empty Guid
            Assert.Throws<ArgumentException>(() => new TransferOffer(Guid.Empty, playerId, clubId, SquadRole.Starter, 1000m, 10000m, 3, OfferDate, ExpiryDate));
            Assert.Throws<ArgumentException>(() => new TransferOffer(id, Guid.Empty, clubId, SquadRole.Starter, 1000m, 10000m, 3, OfferDate, ExpiryDate));
            Assert.Throws<ArgumentException>(() => new TransferOffer(id, playerId, Guid.Empty, SquadRole.Starter, 1000m, 10000m, 3, OfferDate, ExpiryDate));

            // Negative or zero wage
            Assert.Throws<ArgumentOutOfRangeException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, 0m, 10000m, 3, OfferDate, ExpiryDate));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, -500m, 10000m, 3, OfferDate, ExpiryDate));

            // Negative fee
            Assert.Throws<ArgumentOutOfRangeException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, 1000m, -1m, 3, OfferDate, ExpiryDate));

            // Contract years out of bounds
            Assert.Throws<ArgumentOutOfRangeException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, 1000m, 10000m, 0, OfferDate, ExpiryDate));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, 1000m, 10000m, 6, OfferDate, ExpiryDate));

            // Expiry date before offer date
            Assert.Throws<ArgumentException>(() => new TransferOffer(id, playerId, clubId, SquadRole.Starter, 1000m, 10000m, 3, OfferDate, OfferDate.AddDays(-1)));
        }

        [Fact]
        public void TransferOffer_Create_GeneratesValidPendingOffer()
        {
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            var offer = TransferOffer.Create(
                playerId: playerId,
                offeringClubId: clubId,
                offeredRole: SquadRole.Rotation,
                offeredWage: 800m,
                transferFee: 20000m,
                contractYears: 2,
                offerDate: OfferDate,
                expiryDate: ExpiryDate);

            Assert.NotEqual(Guid.Empty, offer.Id);
            Assert.Equal(TransferOfferStatus.Pending, offer.Status);
            Assert.Equal(800m, offer.OfferedWage);
            Assert.False(offer.IsExpired(OfferDate.AddDays(5)));
            Assert.True(offer.IsExpired(ExpiryDate.AddDays(1)));
        }

        [Fact]
        public void TransferOffer_WithStatus_CreatesImmutableCopy()
        {
            var offer = TransferOffer.Create(
                playerId: Guid.NewGuid(),
                offeringClubId: Guid.NewGuid(),
                offeredRole: SquadRole.Starter,
                offeredWage: 3000m,
                transferFee: 100000m,
                contractYears: 4,
                offerDate: OfferDate,
                expiryDate: ExpiryDate);

            var accepted = offer.WithStatus(TransferOfferStatus.Accepted);
            var rejected = offer.WithStatus(TransferOfferStatus.Rejected);

            Assert.Equal(TransferOfferStatus.Pending, offer.Status);
            Assert.Equal(TransferOfferStatus.Accepted, accepted.Status);
            Assert.Equal(TransferOfferStatus.Rejected, rejected.Status);
        }

        [Fact]
        public void ContractNegotiationResult_Factories_CreateExpectedOutcomes()
        {
            var accept = ContractNegotiationResult.Accept(1500m, 3, 3000m, "Terms accepted.");
            Assert.Equal(ContractNegotiationStatus.Accepted, accept.Status);
            Assert.Equal(1500m, accept.AgreedWage);
            Assert.Equal(3, accept.AgreedYears);
            Assert.Equal(3000m, accept.AgreedSigningBonus);

            var counter = ContractNegotiationResult.Counter(1200m, 3, 2000m, "Counter offer.");
            Assert.Equal(ContractNegotiationStatus.CounterOffer, counter.Status);
            Assert.Equal(1200m, counter.AgreedWage);

            var walk = ContractNegotiationResult.WalkAway("Walked away.");
            Assert.Equal(ContractNegotiationStatus.WalkedAway, walk.Status);
            Assert.Equal(0m, walk.AgreedWage);
            Assert.Equal(0, walk.AgreedYears);
        }

        [Fact]
        public void WorldState_TransferOffers_QueryAndMutationHelpersWork()
        {
            var season = new Season(
                2026,
                new DateOnly(2026, 8, 1),
                new DateOnly(2027, 5, 30),
                new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()),
                Array.Empty<Matchday>(),
                1);

            var world = WorldState.CreateEmpty(season);
            var playerId = Guid.NewGuid();
            var offer1 = TransferOffer.Create(playerId, Guid.NewGuid(), SquadRole.Starter, 1000m, 10000m, 2, OfferDate, ExpiryDate);
            var offer2 = TransferOffer.Create(playerId, Guid.NewGuid(), SquadRole.Rotation, 750m, 5000m, 3, OfferDate, ExpiryDate);

            world = world.WithTransferOffer(offer1).WithTransferOffer(offer2);

            Assert.Equal(2, world.TransferOffers.Count);
            Assert.Equal(offer1, world.GetTransferOffer(offer1.Id));

            var playerOffers = world.GetPlayerTransferOffers(playerId);
            Assert.Equal(2, playerOffers.Count);

            var pending = world.GetPendingOffersForPlayer(playerId);
            Assert.Equal(2, pending.Count);

            world = world.RemoveTransferOffer(offer1.Id);
            Assert.Single(world.TransferOffers);
            Assert.Throws<KeyNotFoundException>(() => world.GetTransferOffer(offer1.Id));
        }
    }
}
