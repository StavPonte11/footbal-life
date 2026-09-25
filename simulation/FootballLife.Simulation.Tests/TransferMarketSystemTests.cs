using System;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class TransferMarketSystemTests
    {
        private static WorldState CreateMockWorld(out Player player, out PlayerAbilities abilities, out PlayerCareerState careerState, out Club currentClub)
        {
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>(), 1);
            var world = WorldState.CreateEmpty(season);

            var l1 = League.Create("Premier League", "ENG", 1, 4, 6, 0, 1);
            var l2 = League.Create("Championship", "ENG", 2, 4, 6, 1, 1);

            world = world.WithLeague(l1).WithLeague(l2);

            currentClub = Club.Create("Northfield Town", "NOR", l2.Id, 55, new ClubFinances(20000000m, 25000m), 3, TacticalIdentity.Possession);
            var suitor1 = Club.Create("London Royals", "LRO", l1.Id, 82, new ClubFinances(150000000m, 120000m), 5, TacticalIdentity.HighPress);
            var suitor2 = Club.Create("Midland City", "MCI", l1.Id, 78, new ClubFinances(110000000m, 95000m), 4, TacticalIdentity.Possession);
            var suitor3 = Club.Create("Coast Rovers", "CRO", l2.Id, 60, new ClubFinances(25000000m, 22000m), 3, TacticalIdentity.Direct);

            player = new Player(Guid.NewGuid(), "Striker Prodigy", "ENG", new DateOnly(2005, 5, 12), Foot.Right, Position.ST); // age 21
            abilities = new PlayerAbilities(pace: 78, acceleration: 80, stamina: 72, strength: 68, agility: 75, passing: 65, shooting: 76, dribbling: 74, crossing: 55, firstTouch: 75, tackling: 35, vision: 66, composure: 72, positioning: 75, decisionMaking: 68);
            careerState = new PlayerCareerState(currentClub.Id, SquadStatus.Starter, 65f, 3500m, 2500000m, 50f);
            var account = FinanceAccount.Create(10000m);

            world = world.WithClub(currentClub).WithClub(suitor1).WithClub(suitor2).WithClub(suitor3)
                         .WithPlayer(player, abilities, PlayerState.Default, careerState, PlayerPotential.CreateClamped(85, PotentialRange.High), account);

            return world;
        }

        [Fact]
        public void TransferMarket_GenerateBiddingWar_CreatesCompetingOffers()
        {
            var world = CreateMockWorld(out var player, out var abilities, out var careerState, out var currentClub);
            var rng = new SimulationRandom(505);
            var date = new DateOnly(2026, 8, 15);

            var biddingWar = TransferMarketSystem.GenerateBiddingWar(player, abilities, careerState, world, rng, date, windowWeek: 1);

            Assert.NotNull(biddingWar);
            Assert.True(biddingWar.HasActiveBids);
            Assert.InRange(biddingWar.Bids.Count, 2, 4);

            foreach (var bid in biddingWar.Bids)
            {
                Assert.NotEqual(currentClub.Id, bid.BiddingClubId);
                Assert.True(bid.OfferedWeeklyWage > 0m);
                Assert.True(bid.TransferFee > 0m);
                Assert.True(bid.SigningBonus > 0m);
            }
        }

        [Fact]
        public void TransferMarket_BiddingWar_ScalesWagesToLeagueTier()
        {
            var world = CreateMockWorld(out var player, out var abilities, out var careerState, out var currentClub);
            var rng = new SimulationRandom(606);
            var date = new DateOnly(2026, 8, 15);

            var biddingWar = TransferMarketSystem.GenerateBiddingWar(player, abilities, careerState, world, rng, date, windowWeek: 1);

            var tier1Bid = biddingWar.Bids.FirstOrDefault(b => b.LeagueTier == 1);
            var tier2Bid = biddingWar.Bids.FirstOrDefault(b => b.LeagueTier == 2);

            if (tier1Bid != null && tier2Bid != null)
            {
                Assert.True(tier1Bid.OfferedWeeklyWage > tier2Bid.OfferedWeeklyWage,
                    "Tier 1 wage offer should be higher than Tier 2 wage offer.");
            }
        }

        [Fact]
        public void TransferMarket_RequestTransferListing_ApprovedWhenLowTrust()
        {
            var world = CreateMockWorld(out var player, out var abilities, out var careerState, out var currentClub);
            var rng = new SimulationRandom(707);
            var date = new DateOnly(2026, 11, 1);

            // Unhappy player with low manager trust (25)
            var unhappyCareer = careerState with { ManagerTrust = 25f, Status = SquadStatus.Bench };

            var result = TransferMarketSystem.RequestTransferListing(player, unhappyCareer, currentClub, "Seeking first team football", rng, date);

            Assert.True(result.Approved);
            Assert.NotNull(result.Listing);
            Assert.Equal(PlayerTransferStatus.TransferRequestedByPlayer, result.Listing.Status);
            Assert.Contains("Manager agreed", result.ManagerResponse);
        }

        [Fact]
        public void TransferMarket_RequestTransferListing_RejectedWhenKeyPlayerAndHighTrust()
        {
            var world = CreateMockWorld(out var player, out var abilities, out var careerState, out var currentClub);
            var rng = new SimulationRandom(808);
            var date = new DateOnly(2026, 11, 1);

            // Star player with high trust (85)
            var starCareer = careerState with { ManagerTrust = 85f, Status = SquadStatus.KeyPlayer };

            var result = TransferMarketSystem.RequestTransferListing(player, starCareer, currentClub, "Want big club move", rng, date);

            Assert.False(result.Approved);
            Assert.Null(result.Listing);
            Assert.True(result.ManagerTrustDelta < 0f);
            Assert.Contains("Manager rejected", result.ManagerResponse);
        }

        [Fact]
        public void TransferMarket_AcceptBid_TransitionsPlayerClubContractAndBonus()
        {
            var world = CreateMockWorld(out var player, out var abilities, out var careerState, out var currentClub);
            var rng = new SimulationRandom(909);
            var date = new DateOnly(2026, 8, 20);

            var biddingWar = TransferMarketSystem.GenerateBiddingWar(player, abilities, careerState, world, rng, date, windowWeek: 1);
            var chosenBid = biddingWar.Bids.First();

            decimal initialBalance = world.GetAccount(player.Id).Balance;

            var updatedWorld = TransferMarketSystem.AcceptBid(chosenBid, player, world, date);

            // Check new career state
            var updatedCareer = updatedWorld.GetCareerState(player.Id);
            Assert.Equal(chosenBid.BiddingClubId, updatedCareer.ClubId);
            Assert.Equal(chosenBid.OfferedWeeklyWage, updatedCareer.WeeklySalary);

            // Check contract
            var updatedContract = updatedWorld.FindContractForPlayer(player.Id);
            Assert.NotNull(updatedContract);
            Assert.Equal(chosenBid.BiddingClubId, updatedContract.ClubId);

            // Check signing bonus credited
            var updatedAccount = updatedWorld.GetAccount(player.Id);
            Assert.Equal(initialBalance + chosenBid.SigningBonus, updatedAccount.Balance);
        }
    }
}
