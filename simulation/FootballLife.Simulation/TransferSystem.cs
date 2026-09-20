using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure simulation system managing transfer offers, scouting interest,
    /// transfer executions, roster transitions, and escape clauses.
    /// </summary>
    public static class TransferSystem
    {
        public const int MaxSimultaneousOffers = 3;

        /// <summary>
        /// Generates prospective transfer offers for a player based on ability, reputation,
        /// positional need, and club tiers. Guarantees at least 1 escape offer if manager trust &lt; 30.
        /// </summary>
        public static IReadOnlyList<TransferOffer> GenerateOffers(
            Player player,
            PlayerAbilities abilities,
            PlayerCareerState careerState,
            Contract? currentContract,
            WorldState world,
            SimulationRandom rng,
            DateOnly currentDate)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var offers = new List<TransferOffer>();

            // Exclude current club
            var availableClubs = world.Clubs.Values
                .Where(c => c.Id != careerState.ClubId)
                .ToList();

            if (availableClubs.Count == 0)
            {
                return offers.AsReadOnly();
            }

            float playerOverall = abilities.CalculateAverage();
            int currentTier = GetPlayerCurrentTier(careerState, world);
            bool isLowTrust = careerState.ManagerTrust < 30f;

            // Score and filter eligible suitors
            var scoredCandidates = new List<(Club Club, int Tier, float Score)>();

            foreach (var club in availableClubs)
            {
                int clubTier = 3;
                if (world.Leagues.TryGetValue(club.LeagueId, out var league))
                {
                    clubTier = league.Tier;
                }

                // Tier compatibility check
                bool tierEligible = Math.Abs(clubTier - currentTier) <= 1;

                // High reputation/ability players attract top flight interest
                if ((careerState.Reputation >= 65f || playerOverall >= 75f) && clubTier <= 2)
                {
                    tierEligible = true;
                }

                // Low trust escape offers accept equal or lower tiers
                if (isLowTrust && clubTier >= currentTier)
                {
                    tierEligible = true;
                }

                // Club prestige compatibility
                bool prestigeEligible = club.ReputationRating <= (careerState.Reputation + 30f)
                    || (playerOverall >= 70f && club.ReputationRating <= 80);

                if (isLowTrust)
                {
                    prestigeEligible = true; // Any club can offer an exit route
                }

                if (tierEligible && prestigeEligible)
                {
                    // Fit score combining ability match, reputation alignment, and deterministic randomness
                    float abilityDiff = Math.Abs(playerOverall - club.ReputationRating);
                    float score = 100f - abilityDiff + rng.NextFloat(0f, 20f);

                    // Prefer lower tiers if player has low trust to ensure escape route
                    if (isLowTrust && clubTier >= currentTier)
                    {
                        score += 30f;
                    }

                    scoredCandidates.Add((club, clubTier, score));
                }
            }

            // Fallback: If low trust and no candidate matched, pick the lowest tier available club as forced escape offer
            if (isLowTrust && scoredCandidates.Count == 0 && availableClubs.Count > 0)
            {
                var fallbackClub = availableClubs
                    .OrderByDescending(c => world.Leagues.TryGetValue(c.LeagueId, out var l) ? l.Tier : 3)
                    .First();

                int tier = world.Leagues.TryGetValue(fallbackClub.LeagueId, out var l2) ? l2.Tier : 3;
                scoredCandidates.Add((fallbackClub, tier, 100f));
            }

            if (scoredCandidates.Count == 0)
            {
                return offers.AsReadOnly();
            }

            // Sort candidates by score descending and select up to MaxSimultaneousOffers
            var selectedClubs = scoredCandidates
                .OrderByDescending(c => c.Score)
                .Take(MaxSimultaneousOffers)
                .ToList();

            foreach (var (club, clubTier, _) in selectedClubs)
            {
                // Determine offered squad role
                SquadRole role;
                if (playerOverall >= club.ReputationRating - 3f)
                {
                    role = SquadRole.Starter;
                }
                else if (playerOverall >= club.ReputationRating - 12f)
                {
                    role = SquadRole.Rotation;
                }
                else
                {
                    role = SquadRole.Squad;
                }

                // Calculate wage offer
                decimal offeredWage = CalculateOfferedWage(playerOverall, careerState.Reputation, clubTier, role);

                // Transfer fee (0 for free agents / expired contract)
                decimal transferFee = 0m;
                bool isFreeTransfer = currentContract == null || currentContract.IsExpired(currentDate);

                if (!isFreeTransfer)
                {
                    decimal marketVal = Math.Max(25000m, careerState.MarketValue);
                    float feeMultiplier = rng.NextFloat(0.90f, 1.20f);
                    transferFee = Math.Round(marketVal * (decimal)feeMultiplier, 0);
                }

                // Contract length: 2 to 4 years
                int contractYears = rng.NextInt(2, 5);

                // Signing bonus for significant moves
                decimal signingBonus = 0m;
                if (offeredWage >= 1000m)
                {
                    signingBonus = Math.Round(offeredWage * (decimal)rng.NextFloat(1.5f, 4.0f), 0);
                }

                // Release clause
                decimal releaseClause = Math.Max(transferFee * 2.5m, offeredWage * 52m * 2m);

                // Deadline: 14 to 28 days
                int expiryDays = rng.NextInt(14, 29);
                DateOnly expiryDate = currentDate.AddDays(expiryDays);

                var offer = TransferOffer.Create(
                    playerId: player.Id,
                    offeringClubId: club.Id,
                    offeredRole: role,
                    offeredWage: offeredWage,
                    transferFee: transferFee,
                    contractYears: contractYears,
                    offerDate: currentDate,
                    expiryDate: expiryDate,
                    releaseClause: releaseClause,
                    signingBonus: signingBonus);

                offers.Add(offer);
            }

            return offers.AsReadOnly();
        }

        /// <summary>
        /// Executes acceptance of a transfer offer, modifying contract, career state,
        /// club squads, player bank accounts, and relationships.
        /// </summary>
        public static WorldState AcceptOffer(
            TransferOffer offer,
            WorldState world,
            DateOnly transferDate)
        {
            if (offer is null) throw new ArgumentNullException(nameof(offer));
            if (world is null) throw new ArgumentNullException(nameof(world));

            if (offer.Status != TransferOfferStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot accept offer with status '{offer.Status}'. Must be Pending.");
            }

            if (offer.IsExpired(transferDate))
            {
                throw new InvalidOperationException($"Transfer offer expired on {offer.ExpiryDate:yyyy-MM-dd}; cannot be accepted on {transferDate:yyyy-MM-dd}.");
            }

            var player = world.GetPlayer(offer.PlayerId);
            var careerState = world.GetCareerState(offer.PlayerId);
            var suitorClub = world.GetClub(offer.OfferingClubId);

            Guid oldClubId = careerState.ClubId;
            Club? oldClub = null;
            if (oldClubId != Guid.Empty && world.Clubs.TryGetValue(oldClubId, out var existingClub))
            {
                oldClub = existingClub;
            }

            // 1. Create and register new Contract
            DateOnly contractEnd = transferDate.AddYears(offer.ContractYears);
            var newContract = Contract.Create(
                playerId: player.Id,
                clubId: offer.OfferingClubId,
                weeklySalary: offer.OfferedWage,
                startDate: transferDate,
                endDate: contractEnd,
                contractRole: offer.OfferedRole);

            world = world.WithContract(newContract);

            // 2. Update PlayerCareerState
            float initialTrust = offer.OfferedRole switch
            {
                SquadRole.Starter => 55f,
                SquadRole.Rotation => 45f,
                SquadRole.Squad => 35f,
                _ => 25f
            };

            SquadStatus newSquadStatus = offer.OfferedRole switch
            {
                SquadRole.Starter => SquadStatus.Starter,
                SquadRole.Rotation => SquadStatus.Rotation,
                SquadRole.Squad => SquadStatus.Bench,
                _ => SquadStatus.Academy
            };

            var updatedCareer = careerState with
            {
                ClubId = offer.OfferingClubId,
                Status = newSquadStatus,
                ManagerTrust = initialTrust,
                WeeklySalary = offer.OfferedWage
            };

            world = world.WithPlayerCareerState(player.Id, updatedCareer);

            // 3. Update Clubs' squad rosters
            if (oldClub != null)
            {
                world = world.WithClub(oldClub.WithRemovedPlayer(player.Id));
            }

            world = world.WithClub(suitorClub.WithAddedPlayer(player.Id));

            // 4. Financials: Credit signing bonus if present
            if (offer.SigningBonus > 0m && world.Accounts.TryGetValue(player.Id, out var account))
            {
                var bonusTransaction = FinanceTransaction.Create(
                    date: transferDate,
                    type: TransactionType.TransferBonus,
                    amount: offer.SigningBonus,
                    description: $"Signing bonus for transfer to {suitorClub.Name}");

                world = world.WithPlayerAccount(player.Id, account.WithTransaction(bonusTransaction));
            }

            // 5. Relationships: Apply relocation impact
            var relationships = world.GetPlayerRelationships(player.Id);
            if (relationships.Count > 0)
            {
                var updatedRels = RelationshipSystem.ApplyClubTransfer(
                    relationships,
                    oldClubId,
                    offer.OfferingClubId,
                    transferDate);

                world = world.WithRelationships(updatedRels);
            }

            // 6. Update this offer status to Accepted and withdraw/expire other pending offers
            var acceptedOffer = offer.WithStatus(TransferOfferStatus.Accepted);
            world = world.WithTransferOffer(acceptedOffer);

            foreach (var otherOffer in world.GetPendingOffersForPlayer(player.Id))
            {
                if (otherOffer.Id != offer.Id)
                {
                    world = world.WithTransferOffer(otherOffer.WithStatus(TransferOfferStatus.Withdrawn));
                }
            }

            return world;
        }

        /// <summary>
        /// Rejects a pending transfer offer.
        /// </summary>
        public static TransferOffer RejectOffer(TransferOffer offer)
        {
            if (offer is null) throw new ArgumentNullException(nameof(offer));
            return offer.WithStatus(TransferOfferStatus.Rejected);
        }

        /// <summary>
        /// Rejects a pending transfer offer and saves it in the updated <see cref="WorldState"/>.
        /// </summary>
        public static WorldState RejectOffer(TransferOffer offer, WorldState world)
        {
            if (offer is null) throw new ArgumentNullException(nameof(offer));
            if (world is null) throw new ArgumentNullException(nameof(world));

            var rejected = RejectOffer(offer);
            return world.WithTransferOffer(rejected);
        }

        private static int GetPlayerCurrentTier(PlayerCareerState career, WorldState world)
        {
            if (career.ClubId != Guid.Empty && world.Clubs.TryGetValue(career.ClubId, out var club))
            {
                if (world.Leagues.TryGetValue(club.LeagueId, out var league))
                {
                    return league.Tier;
                }
            }

            // Default fallback tier based on player reputation
            return career.Reputation switch
            {
                >= 80f => 1,
                >= 60f => 2,
                >= 40f => 3,
                >= 20f => 4,
                _ => 5
            };
        }

        private static decimal CalculateOfferedWage(float overall, float reputation, int tier, SquadRole role)
        {
            // Base wage scaled exponentially with overall ability
            decimal baseWage = Math.Max(250m, (decimal)(overall * overall * 2.2f));

            decimal tierMultiplier = tier switch
            {
                1 => 3.5m,
                2 => 1.8m,
                3 => 1.0m,
                4 => 0.6m,
                _ => 0.35m
            };

            decimal roleMultiplier = role switch
            {
                SquadRole.Starter => 1.0m,
                SquadRole.Rotation => 0.75m,
                SquadRole.Squad => 0.50m,
                _ => 0.30m
            };

            decimal reputationMultiplier = 0.75m + (decimal)(reputation / 100f) * 0.50m;

            decimal totalWage = baseWage * tierMultiplier * roleMultiplier * reputationMultiplier;

            // Round to nearest 50
            decimal rounded = Math.Round(totalWage / 50m, 0) * 50m;
            return Math.Max(200m, rounded);
        }
    }
}
