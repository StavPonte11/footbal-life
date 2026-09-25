using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Result of a player-initiated transfer request (#P5-002).
    /// </summary>
    public sealed record TransferRequestResult
    {
        public bool Approved { get; init; }
        public TransferListing? Listing { get; init; }
        public float ManagerTrustDelta { get; init; }
        public string ManagerResponse { get; init; } = string.Empty;
    }

    /// <summary>
    /// Pure simulation system managing dynamic multi-club transfer bidding wars,
    /// player-initiated transfer requests, and cross-tier market valuations (#P5-002, #P5-003).
    /// </summary>
    public static class TransferMarketSystem
    {
        /// <summary>
        /// Generates a multi-club bidding war for a player with competing offers across league tiers.
        /// </summary>
        public static TransferBiddingWar GenerateBiddingWar(
            Player player,
            PlayerAbilities abilities,
            PlayerCareerState careerState,
            WorldState world,
            SimulationRandom rng,
            DateOnly currentDate,
            int windowWeek,
            bool isTransferRequested = false)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            float overall = abilities.CalculateAverage();
            int currentTier = 4;
            string currentClubName = "Free Agent";

            if (world.Clubs.TryGetValue(careerState.ClubId, out var currentClub))
            {
                currentClubName = currentClub.Name;
                if (world.Leagues.TryGetValue(currentClub.LeagueId, out var l))
                {
                    currentTier = l.Tier;
                }
            }

            decimal marketVal = CalculateMarketValue(overall, player.GetAgeAt(currentDate), currentTier);

            // Find interested clubs (excluding current club)
            var eligibleClubs = world.Clubs.Values
                .Where(c => c.Id != careerState.ClubId)
                .ToList();

            var bids = new List<ClubBid>();

            if (eligibleClubs.Count > 0)
            {
                // Score clubs for suitability
                var scoredClubs = eligibleClubs
                    .Select(c =>
                    {
                        int clubTier = world.Leagues.TryGetValue(c.LeagueId, out var cl) ? cl.Tier : 4;
                        float tierDelta = Math.Abs(clubTier - currentTier);
                        float abilityMatch = Math.Abs(c.ReputationRating - (overall + 5f));
                        float score = 100f - (tierDelta * 25f) - abilityMatch + rng.NextFloat(0f, 20f);
                        if (isTransferRequested) score += 15f; // Extra interest when player is listed
                        return (Club: c, Tier: clubTier, Score: score);
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(rng.NextInt(2, 5))
                    .ToList();

                foreach (var (club, tier, _) in scoredClubs)
                {
                    var tierProfile = LeagueTierConfig.GetProfile(tier);
                    string leagueName = world.Leagues.TryGetValue(club.LeagueId, out var leg) ? leg.Name : tierProfile.TierName;

                    // Wage scaling based on tier profile and player overall
                    decimal wageRatio = Math.Clamp((decimal)(overall - 45f) / 45m, 0.2m, 1.0m);
                    decimal baseWage = tierProfile.MinWeeklyWage + ((tierProfile.AverageWeeklyWage - tierProfile.MinWeeklyWage) * wageRatio);
                    decimal offeredWage = Math.Round(baseWage * (decimal)rng.NextFloat(0.95f, 1.25f), 0);

                    decimal fee = Math.Round(marketVal * (decimal)rng.NextFloat(0.85f, 1.35f), 0);
                    decimal bonus = Math.Round(offeredWage * (decimal)rng.NextInt(4, 12), 0);

                    SquadRole promisedRole = overall switch
                    {
                        >= 70 => SquadRole.Starter,
                        >= 60 => SquadRole.Rotation,
                        _ => SquadRole.Squad
                    };

                    bids.Add(new ClubBid
                    {
                        BidId = Guid.NewGuid().ToString("N")[..8],
                        BiddingClubId = club.Id,
                        BiddingClubName = club.Name,
                        LeagueTier = tier,
                        LeagueName = leagueName,
                        TransferFee = fee,
                        OfferedWeeklyWage = offeredWage,
                        SigningBonus = bonus,
                        PromisedSquadRole = promisedRole,
                        ContractLengthYears = rng.NextInt(2, 5),
                        ClubPrestige = club.ReputationRating,
                        BidDate = currentDate
                    });
                }
            }

            return new TransferBiddingWar
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                CurrentClubId = careerState.ClubId,
                CurrentClubName = currentClubName,
                EstimatedMarketValue = marketVal,
                WindowWeek = windowWeek,
                Bids = bids.OrderByDescending(b => b.OfferedWeeklyWage).ToList()
            };
        }

        /// <summary>
        /// Processes a player's formal transfer request with manager response and trust consequences.
        /// </summary>
        public static TransferRequestResult RequestTransferListing(
            Player player,
            PlayerCareerState careerState,
            Club club,
            string reason,
            SimulationRandom rng,
            DateOnly currentDate)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Low manager trust (< 40) or unhappy status -> Manager approves easily
            if (careerState.ManagerTrust < 40f || careerState.Status == SquadStatus.Reserve || careerState.Status == SquadStatus.Bench)
            {
                var listing = new TransferListing
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    Status = PlayerTransferStatus.TransferRequestedByPlayer,
                    AskingPrice = careerState.MarketValue * 0.9m,
                    ListedDate = currentDate,
                    Reason = reason
                };

                return new TransferRequestResult
                {
                    Approved = true,
                    Listing = listing,
                    ManagerTrustDelta = -5f,
                    ManagerResponse = $"Manager agreed: \"If your heart isn't at {club.Name}, we will listen to offers in the transfer window.\""
                };
            }

            // High manager trust (> 70) and key player -> Manager rejects request to keep player
            if (careerState.ManagerTrust >= 70f && careerState.Status == SquadStatus.KeyPlayer)
            {
                return new TransferRequestResult
                {
                    Approved = false,
                    Listing = null,
                    ManagerTrustDelta = -12f,
                    ManagerResponse = $"Manager rejected request: \"You are vital to our future at {club.Name}. I will not sanction a departure right now.\""
                };
            }

            // Middle ground: 50% chance of approval with moderate friction
            bool approved = rng.NextBool(0.55f);
            if (approved)
            {
                var listing = new TransferListing
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    Status = PlayerTransferStatus.TransferRequestedByPlayer,
                    AskingPrice = careerState.MarketValue * 1.1m,
                    ListedDate = currentDate,
                    Reason = reason
                };

                return new TransferRequestResult
                {
                    Approved = true,
                    Listing = listing,
                    ManagerTrustDelta = -8f,
                    ManagerResponse = $"Manager accepted request: \"We'll list you on the market, but only if our valuation is met.\""
                };
            }

            return new TransferRequestResult
            {
                Approved = false,
                Listing = null,
                ManagerTrustDelta = -10f,
                ManagerResponse = $"Manager refused request: \"Focus on your training and performances on the pitch. Transfer talks are closed for now.\""
            };
        }

        /// <summary>
        /// Accepts a club bid, transfers the player to the purchasing club, and sets up contract/finances.
        /// </summary>
        public static WorldState AcceptBid(
            ClubBid bid,
            Player player,
            WorldState world,
            DateOnly currentDate)
        {
            if (bid is null) throw new ArgumentNullException(nameof(bid));
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (world is null) throw new ArgumentNullException(nameof(world));

            var updatedClubs = new Dictionary<Guid, Club>(world.Clubs);
            var updatedCareerStates = new Dictionary<Guid, PlayerCareerState>(world.CareerStates);
            var updatedContracts = new Dictionary<Guid, Contract>(world.Contracts);
            var updatedAccounts = new Dictionary<Guid, FinanceAccount>(world.Accounts);

            // Remove from old club
            if (world.CareerStates.TryGetValue(player.Id, out var oldCs) && updatedClubs.TryGetValue(oldCs.ClubId, out var oldClub))
            {
                updatedClubs[oldClub.Id] = oldClub.WithRemovedPlayer(player.Id);
            }

            // Add to new club
            if (updatedClubs.TryGetValue(bid.BiddingClubId, out var newClub))
            {
                updatedClubs[newClub.Id] = newClub.WithAddedPlayer(player.Id);
            }

            // Update player career state
            var newCareerState = new PlayerCareerState(
                clubId: bid.BiddingClubId,
                status: bid.PromisedSquadRole == SquadRole.Starter ? SquadStatus.Starter : SquadStatus.Rotation,
                managerTrust: 65f, // New manager gives fresh start
                weeklySalary: bid.OfferedWeeklyWage,
                marketValue: bid.TransferFee,
                reputation: Math.Min(100f, (oldCs?.Reputation ?? 30f) + (bid.ClubPrestige * 0.15f)));

            updatedCareerStates[player.Id] = newCareerState;

            // Create new contract
            var newContract = Contract.Create(
                player.Id,
                bid.BiddingClubId,
                bid.OfferedWeeklyWage,
                currentDate,
                currentDate.AddYears(bid.ContractLengthYears),
                bid.PromisedSquadRole,
                new ContractBonuses(
                    goalBonus: bid.OfferedWeeklyWage * 0.15m,
                    assistBonus: bid.OfferedWeeklyWage * 0.08m,
                    appearanceBonus: bid.OfferedWeeklyWage * 0.05m,
                    cleanSheetBonus: bid.OfferedWeeklyWage * 0.10m));

            updatedContracts[player.Id] = newContract;

            // Award signing bonus to player's bank account
            if (world.Accounts.TryGetValue(player.Id, out var account))
            {
                var tx = FinanceTransaction.Create(
                    currentDate,
                    TransactionType.TransferBonus,
                    bid.SigningBonus,
                    $"Signing Bonus from {bid.BiddingClubName}");

                updatedAccounts[player.Id] = account.WithTransaction(tx);
            }

            return new WorldState(
                updatedClubs,
                world.Players,
                world.Abilities,
                world.States,
                updatedCareerStates,
                world.Leagues,
                world.Managers,
                updatedContracts,
                world.Potentials,
                world.CurrentSeason,
                updatedAccounts,
                world.EventCooldowns,
                world.Relationships,
                world.TransferOffers);
        }

        private static decimal CalculateMarketValue(float overall, int age, int tier)
        {
            decimal baseValue = overall switch
            {
                >= 85 => 45000000m,
                >= 80 => 25000000m,
                >= 75 => 10000000m,
                >= 70 => 4000000m,
                >= 65 => 1200000m,
                >= 60 => 450000m,
                _ => 150000m
            };

            // Age curve multiplier (young prospects command high resale value, veterans discount)
            decimal ageMultiplier = age switch
            {
                <= 21 => 1.45m,
                <= 24 => 1.25m,
                <= 28 => 1.00m,
                <= 31 => 0.70m,
                _ => 0.35m
            };

            return Math.Round(baseValue * ageMultiplier, 0);
        }
    }
}
