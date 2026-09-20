using System;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure simulation system managing contract negotiations, bargaining tolerance curves,
    /// agent leverage, Bosman ruling eligibility, contract renewals, and free agency transitions.
    /// </summary>
    public static class ContractSystem
    {
        public const int BosmanThresholdDays = 182; // ~6 months (26 weeks)

        /// <summary>
        /// Evaluates the legal and calendar status of an employment contract.
        /// </summary>
        public static ContractExpiryStatus EvaluateContractStatus(Contract contract, DateOnly currentDate)
        {
            if (contract is null) throw new ArgumentNullException(nameof(contract));

            if (contract.IsExpired(currentDate))
            {
                return ContractExpiryStatus.Expired;
            }

            if (contract.RemainingDays(currentDate) <= BosmanThresholdDays)
            {
                return ContractExpiryStatus.BosmanEligible;
            }

            return ContractExpiryStatus.Active;
        }

        /// <summary>
        /// Simulates contract negotiations between a player (and agent) and a club.
        /// Evaluates wage demands against club budget tolerances and agent affinity leverage.
        /// </summary>
        public static ContractNegotiationResult NegotiateTerms(
            Player player,
            PlayerAbilities abilities,
            PlayerCareerState careerState,
            Club club,
            decimal demandedWage,
            int demandedYears,
            WorldState world,
            SimulationRandom rng)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            demandedWage = Math.Max(100m, demandedWage);
            demandedYears = Math.Clamp(demandedYears, 1, 5);

            float overall = abilities.CalculateAverage();
            decimal fairWage = CalculateFairValuation(overall, careerState.Reputation, club, world);

            // Agent leverage modifier: high agent affinity increases club acceptable threshold
            float agentLeverageBonus = 0f;
            var agentRel = world.GetPlayerRelationships(player.Id)
                .FirstOrDefault(r => r.Type == RelationshipType.Agent);

            if (agentRel != null)
            {
                // Up to +15% tolerance based on agent affinity
                agentLeverageBonus = (agentRel.Affinity / 100f) * 0.15f;
            }

            decimal maxAcceptableWage = fairWage * (1.10m + (decimal)agentLeverageBonus);
            decimal maxCounterWage = fairWage * (1.30m + (decimal)agentLeverageBonus);

            // 1. Direct Acceptance
            if (demandedWage <= maxAcceptableWage)
            {
                decimal bonus = Math.Round(demandedWage * (decimal)rng.NextFloat(1.0f, 3.0f), 0);
                return ContractNegotiationResult.Accept(
                    wage: demandedWage,
                    years: demandedYears,
                    signingBonus: bonus,
                    feedback: "Club board and management accepted your requested wage and contract terms.");
            }

            // 2. Counter-Offer
            if (demandedWage <= maxCounterWage)
            {
                // Compromise between fair valuation and demand
                decimal compromiseWage = Math.Round(((fairWage + demandedWage) / 2m) / 50m, 0) * 50m;
                compromiseWage = Math.Max(fairWage, compromiseWage);
                decimal bonus = Math.Round(compromiseWage * 1.5m, 0);

                return ContractNegotiationResult.Counter(
                    counterWage: compromiseWage,
                    counterYears: demandedYears,
                    signingBonus: bonus,
                    feedback: "Club felt the initial wage demand was slightly high, countering with a balanced compromise package.");
            }

            // 3. Walk Away
            return ContractNegotiationResult.WalkAway(
                feedback: "Club considered the wage demands exorbitant and walked away from the negotiating table.");
        }

        /// <summary>
        /// Generates a contract renewal offer from the player's current club if performance and trust warrant retention.
        /// </summary>
        public static TransferOffer? OfferRenewal(
            Player player,
            PlayerAbilities abilities,
            PlayerCareerState careerState,
            Contract currentContract,
            Club club,
            WorldState world,
            SimulationRandom rng,
            DateOnly currentDate)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (careerState is null) throw new ArgumentNullException(nameof(careerState));
            if (currentContract is null) throw new ArgumentNullException(nameof(currentContract));
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // If manager trust is poor (< 30), club refuses to renew
            if (careerState.ManagerTrust < 30f)
            {
                return null;
            }

            float overall = abilities.CalculateAverage();
            decimal fairWage = CalculateFairValuation(overall, careerState.Reputation, club, world);

            // Offer at least current wage + 10%, or fair valuation
            decimal renewalWage = Math.Max(currentContract.WeeklySalary * 1.10m, fairWage);
            renewalWage = Math.Round(renewalWage / 50m, 0) * 50m;

            int renewalYears = rng.NextInt(2, 5);
            decimal bonus = Math.Round(renewalWage * 2.0m, 0);
            decimal releaseClause = Math.Max(careerState.MarketValue * 2m, renewalWage * 52m * 3m);
            DateOnly expiry = currentDate.AddDays(21);

            return TransferOffer.Create(
                playerId: player.Id,
                offeringClubId: club.Id,
                offeredRole: currentContract.ContractRole,
                offeredWage: renewalWage,
                transferFee: 0m,
                contractYears: renewalYears,
                offerDate: currentDate,
                expiryDate: expiry,
                releaseClause: releaseClause,
                signingBonus: bonus);
        }

        /// <summary>
        /// Processes contract expiration when a contract reaches its end date without renewal.
        /// Releases the player into free agency with zero wage and detached club status.
        /// </summary>
        public static WorldState HandleContractExpiry(
            Player player,
            Contract contract,
            WorldState world,
            DateOnly currentDate)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (contract is null) throw new ArgumentNullException(nameof(contract));
            if (world is null) throw new ArgumentNullException(nameof(world));

            if (!contract.IsExpired(currentDate))
            {
                return world; // Contract has not expired yet
            }

            var careerState = world.GetCareerState(player.Id);
            Guid oldClubId = careerState.ClubId;

            // Update career state to Free Agent
            var updatedCareer = careerState with
            {
                ClubId = Guid.Empty,
                WeeklySalary = 0m,
                Status = SquadStatus.Reserve,
                ManagerTrust = 20f
            };

            world = world.WithPlayerCareerState(player.Id, updatedCareer);

            // Remove from old club squad roster
            if (oldClubId != Guid.Empty && world.Clubs.TryGetValue(oldClubId, out var oldClub))
            {
                world = world.WithClub(oldClub.WithRemovedPlayer(player.Id));
            }

            return world;
        }

        private static decimal CalculateFairValuation(float overall, float reputation, Club club, WorldState world)
        {
            int tier = 3;
            if (world.Leagues.TryGetValue(club.LeagueId, out var league))
            {
                tier = league.Tier;
            }

            decimal baseWage = Math.Max(250m, (decimal)(overall * overall * 2.1f));

            decimal tierMultiplier = tier switch
            {
                1 => 3.2m,
                2 => 1.7m,
                3 => 1.0m,
                4 => 0.6m,
                _ => 0.35m
            };

            decimal repMultiplier = 0.80m + (decimal)(reputation / 100f) * 0.40m;
            decimal total = baseWage * tierMultiplier * repMultiplier;

            return Math.Max(250m, Math.Round(total / 50m, 0) * 50m);
        }
    }
}
