using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# multi-season career execution engine that coordinates all Phase 1 simulation systems
    /// (Training, Matches, Fatigue, Progression, Economy, Transfers, Contracts, and Aging)
    /// to simulate full player careers from debut to retirement.
    /// </summary>
    public static class CareerSimulationEngine
    {
        public const int DefaultMaxSeasons = 20;
        public const int WeeksPerSeason = 38;

        /// <summary>
        /// Simulates a complete multi-season player career from debut until retirement or max seasons.
        /// </summary>
        public static CareerStatistics SimulateCareer(int seed, int maxSeasons = DefaultMaxSeasons)
        {
            var rng = new SimulationRandom(seed);

            // 1. Generate multi-tier league and club ecosystem
            var ecosystem = InitializeEcosystem();
            var world = ecosystem.World;

            // 2. Generate rookie player (age 18, starting in Tier 3 or 4)
            var rookieClub = rng.NextBool(0.5f) ? ecosystem.Tier4Clubs[0] : ecosystem.Tier3Clubs[0];
            var position = PickPosition(rng);
            int birthYear = 2008;
            var dob = new DateOnly(birthYear, rng.NextInt(1, 13), rng.NextInt(1, 28));

            byte[] guidBytes = new byte[16];
            for (int b = 0; b < 16; b++) guidBytes[b] = (byte)rng.NextInt(0, 256);
            var playerId = new Guid(guidBytes);
            var player = new Player(playerId, $"Prospect #{seed % 100000}", "ENG", dob, rng.NextBool(0.8f) ? Foot.Right : Foot.Left, position);

            // Potential generation: 60% Medium (68-78), 25% High (79-85), 10% Low (62-67), 5% Elite (86-93)
            var potential = GeneratePotential(rng);

            // Baseline rookie attributes scaled by potential tier
            var abilities = GenerateRookieAbilities(position, potential, rng);
            int startingOverall = (int)abilities.CalculateAverage();

            var state = new PlayerState(
                fatigue: 10f,
                confidence: 60f,
                form: 55f,
                happiness: 70f,
                motivation: 80f,
                morale: 75f,
                fitness: 85f);

            decimal startingWage = rookieClub.ReputationRating >= 45 ? 1200m : 600m;
            var careerState = new PlayerCareerState(
                clubId: rookieClub.Id,
                status: SquadStatus.Rotation,
                managerTrust: 50f,
                weeklySalary: startingWage,
                marketValue: 100000m,
                reputation: 25f);

            var account = FinanceAccount.Create(1000m);
            var lifestyle = LifestyleTier.Modest;

            var bonuses = new ContractBonuses(
                goalBonus: position == Position.ST || position == Position.LW || position == Position.RW ? 150m : 50m,
                assistBonus: 75m,
                appearanceBonus: 50m,
                cleanSheetBonus: position == Position.CB || position == Position.FB || position == Position.GK ? 100m : 0m);

            var contractStartDate = new DateOnly(2026, 8, 1);
            var contractEndDate = contractStartDate.AddYears(3);
            var contract = Contract.Create(playerId, rookieClub.Id, startingWage, contractStartDate, contractEndDate, SquadRole.Rotation, bonuses);

            // Seed initial relationships
            var relationships = new List<Relationship>
            {
                Relationship.Create(playerId, "Family", RelationshipType.Parent, 85f, 90f, contractStartDate),
                Relationship.Create(playerId, "Agent", RelationshipType.Agent, 60f, 65f, contractStartDate),
                Relationship.Create(playerId, "Club Manager", RelationshipType.Manager, 50f, 50f, contractStartDate)
            };

            // Register rookie into club and world
            rookieClub = rookieClub.WithAddedPlayer(playerId);
            world = world.WithClub(rookieClub)
                         .WithContract(contract)
                         .WithPlayer(player, abilities, state, careerState, potential, account, relationships);

            // Telemetry accumulators
            var seasonRecords = new List<SeasonRecord>(maxSeasons);
            int peakOverall = startingOverall;
            int peakAge = 18;
            decimal peakSalary = startingWage;
            decimal totalEarnings = 0m;
            decimal totalCommercialEarnings = 0m;
            int totalAppearances = 0;
            int totalGoals = 0;
            int totalAssists = 0;
            int totalLeagueTitles = 0;
            int totalDomesticCups = 0;
            int totalContinentalTitles = 0;
            int totalInternationalTrophies = 0;
            int totalInternationalCaps = 0;
            int totalInternationalGoals = 0;
            var activeSponsorships = new List<ActiveSponsorship>();
            double sumRating = 0;
            int transferCount = 0;
            bool bankruptcyOccurred = false;
            int seasonsUnattached = 0;
            int seasonsAtCurrentClub = 0;
            int retirementAge = 18;

            int startSeasonYear = 2026;

            // 3. Multi-season career simulation loop
            for (int seasonIndex = 0; seasonIndex < maxSeasons; seasonIndex++)
            {
                int seasonYear = startSeasonYear + seasonIndex;
                var seasonStartDate = new DateOnly(seasonYear, 8, 1);
                var seasonEndDate = new DateOnly(seasonYear + 1, 5, 30);
                int currentAge = ProgressionSystem.CalculateAge(player.DateOfBirth, seasonStartDate);

                decimal seasonEarnings = 0m;
                int seasonApps = 0;
                int seasonGoals = 0;
                int seasonAssists = 0;
                double seasonRatingSum = 0;

                bool isFreeAgent = careerState.ClubId == Guid.Empty;
                var currentClub = isFreeAgent ? null : world.GetClub(careerState.ClubId);
                string clubName = currentClub != null ? currentClub.Name : "Free Agent";
                int leagueTier = 4;
                if (currentClub != null && world.Leagues.TryGetValue(currentClub.LeagueId, out var league))
                {
                    leagueTier = league.Tier;
                }

                // Check sponsorship deals for upcoming season
                var sponsorshipSys = new SponsorshipSystem();
                int effectiveRep = (int)Math.Clamp(Math.Max(careerState.Reputation, (abilities.CalculateAverage() - 50f) * 2f), 0f, 100f);
                var availableOffers = sponsorshipSys.GetAvailableOffers(effectiveRep, activeSponsorships);
                foreach (var offer in availableOffers)
                {
                    if (activeSponsorships.Count >= 3) break;
                    var (signed, bonus, err) = sponsorshipSys.AcceptDeal(offer, effectiveRep, activeSponsorships);
                    if (signed != null)
                    {
                        activeSponsorships.Add(signed);
                        account = account with { Balance = account.Balance + bonus };
                        totalEarnings += bonus;
                        totalCommercialEarnings += bonus;
                        seasonEarnings += bonus;
                    }
                }

                // Weekly simulation loop (38 weeks)
                for (int week = 1; week <= WeeksPerSeason; week++)
                {
                    var weekDate = seasonStartDate.AddDays((week - 1) * 7);

                    // A. Financials: weekly wage, sponsorships & lifestyle
                    if (!isFreeAgent && careerState.WeeklySalary > 0m)
                    {
                        account = EconomySystem.ApplyWeeklySalary(account, careerState.WeeklySalary, weekDate);
                        seasonEarnings += careerState.WeeklySalary;
                        totalEarnings += careerState.WeeklySalary;
                    }

                    // Process weekly sponsorship payouts
                    if (activeSponsorships.Count > 0)
                    {
                        var (payout, updatedDeals, expiredDeals) = sponsorshipSys.ProcessWeeklyPayouts(activeSponsorships);
                        if (payout > 0)
                        {
                            account = account with { Balance = account.Balance + payout };
                            totalEarnings += payout;
                            seasonEarnings += payout;
                            totalCommercialEarnings += payout;
                        }
                        activeSponsorships = new List<ActiveSponsorship>(updatedDeals);
                    }

                    account = EconomySystem.ApplyLifestyleExpenses(account, lifestyle, weekDate);
                    if (account.IsInDebt) bankruptcyOccurred = true;

                    // B. Training & Condition
                    if (!isFreeAgent)
                    {
                        // Fatigue management: if fatigue is high, do recovery to avoid injury/exhaustion
                        var trainingType = state.Fatigue > 50f
                            ? TrainingType.Recovery
                            : (week % 2 == 0 ? TrainingType.Technical : TrainingType.Physical);

                        var session = new TrainingSession(trainingType, TrainingIntensity.Moderate, 60, weekDate);
                        var trainResult = TrainingSystem.CalculateXP(session, player.PrimaryPosition, abilities, state, rng);
                        state = FatigueSystem.ApplyTraining(state, trainResult);

                        // Weekly professional development: combine session focus with comprehensive weekly training & match coaching
                        world = world.WithPlayer(player, abilities, state, careerState, potential, account, relationships);
                        abilities = ProgressionSystem.ApplyXpGains(playerId, world, weekDate, rng);
                    }

                    // C. Matchday Fixture
                    if (!isFreeAgent && currentClub != null)
                    {
                        // Player selected if squad role allows and fatigue < 85%
                        bool isSelected = state.Fatigue < 85f && careerState.ManagerTrust >= 25f;
                        if (isSelected)
                        {
                            var opponent = PickOpponent(currentClub, ecosystem.AllClubs, rng);
                            var fixture = ScheduledMatch.Create(weekDate.AddDays(6), currentClub.Id, opponent.Id, currentClub.LeagueId);

                            var (matchRes, postState) = MatchSimulator.Simulate(fixture, currentClub.Id, abilities, state, player.PrimaryPosition, rng, playerId);
                            state = postState;

                            // Manager trust
                            careerState = ManagerTrustSystem.ApplyMatchResult(careerState, matchRes, careerState.Status);

                            // Match bonuses
                            var preBonus = account.Balance;
                            account = EconomySystem.ApplyMatchBonuses(account, bonuses, matchRes, player.PrimaryPosition, weekDate, true, playerId);
                            decimal bonusDiff = account.Balance - preBonus;
                            if (bonusDiff > 0m)
                            {
                                seasonEarnings += bonusDiff;
                                totalEarnings += bonusDiff;
                            }

                            // Match records
                            seasonApps++;
                            totalAppearances++;
                            if (matchRes.PlayerScored)
                            {
                                seasonGoals++;
                                totalGoals++;
                            }
                            if (matchRes.PlayerAssisted)
                            {
                                seasonAssists++;
                                totalAssists++;
                            }
                            seasonRatingSum += matchRes.PlayerRating;
                            sumRating += matchRes.PlayerRating;
                        }
                    }

                    // D. Transfer Windows: Week 1 (Summer) & Week 20 (Winter)
                    if (week == 1 || week == 20)
                    {
                        world = world.WithPlayer(player, abilities, state, careerState, potential, account, relationships);

                        // If free agent or in contract, check potential transfer moves
                        var activeContract = world.FindContractForPlayer(playerId);
                        var offers = TransferSystem.GenerateOffers(player, abilities, careerState, activeContract, world, rng, weekDate);

                        if (offers.Count > 0)
                        {
                            // Realistic football transfer commitment:
                            // 1. Free agents sign immediately
                            // 2. Escape toxic manager trust (< 28)
                            // 3. Bosman move (contract expiring in <= 6 months)
                            // 4. Summer transfer window after at least 3 seasons at current club:
                            //    - Promotion to higher league tier with better wage
                            //    - Or massive wage boost (>= 1.75x)
                            bool isBosman = activeContract != null &&
                                            ContractSystem.EvaluateContractStatus(activeContract, weekDate) == ContractExpiryStatus.BosmanEligible;

                            bool eligibleToMove = isFreeAgent ||
                                                  careerState.ManagerTrust < 28f ||
                                                  isBosman ||
                                                  (week == 1 && seasonsAtCurrentClub >= 3);

                            if (eligibleToMove)
                            {
                                var bestOffer = offers.OrderByDescending(o => o.OfferedWage).First();
                                var offeringClub = world.GetClub(bestOffer.OfferingClubId);
                                int targetTier = world.Leagues.TryGetValue(offeringClub.LeagueId, out var targetLeague) ? targetLeague.Tier : 4;

                                bool shouldAccept = false;
                                if (isFreeAgent || isBosman)
                                {
                                    shouldAccept = true;
                                }
                                else if (currentClub != null && targetTier < leagueTier && bestOffer.OfferedWage > careerState.WeeklySalary * 1.2m)
                                {
                                    shouldAccept = true; // Promotion to higher tier
                                }
                                else if (currentClub != null && bestOffer.OfferedWage >= careerState.WeeklySalary * 1.75m)
                                {
                                    shouldAccept = true; // Substantial financial upgrade
                                }
                                else if (careerState.ManagerTrust < 28f)
                                {
                                    shouldAccept = true; // Escape toxic benching
                                }

                                if (shouldAccept)
                                {
                                    world = TransferSystem.AcceptOffer(bestOffer, world, weekDate);
                                    careerState = world.GetCareerState(playerId);
                                    account = world.GetAccount(playerId);
                                    currentClub = world.GetClub(careerState.ClubId);
                                    clubName = currentClub.Name;
                                    leagueTier = world.Leagues.TryGetValue(currentClub.LeagueId, out var l) ? l.Tier : 4;
                                    isFreeAgent = false;
                                    transferCount++;
                                    seasonsAtCurrentClub = 0;

                                    if (careerState.WeeklySalary > peakSalary)
                                    {
                                        peakSalary = careerState.WeeklySalary;
                                    }
                                }
                            }
                        }
                    }

                    // E. Weekly Recovery & Relationship Decay
                    state = FatigueSystem.ApplyRest(state, 8);
                    state = FatigueSystem.ApplyDayTick(state);

                    for (int r = 0; r < relationships.Count; r++)
                    {
                        relationships[r] = RelationshipSystem.ApplyWeeklyDecay(relationships[r]);
                    }
                }

                // 4. End of Season Reviews & Contracts
                world = world.WithPlayer(player, abilities, state, careerState, potential, account, relationships);

                int endOverall = (int)abilities.CalculateAverage();
                if (endOverall > peakOverall)
                {
                    peakOverall = endOverall;
                    peakAge = currentAge;
                }

                float avgSeasonRating = seasonApps > 0 ? (float)(seasonRatingSum / seasonApps) : 6.0f;
                seasonRecords.Add(new SeasonRecord(
                    seasonYear,
                    currentAge,
                    clubName,
                    leagueTier,
                    careerState.WeeklySalary,
                    careerState.Status,
                    seasonApps,
                    seasonGoals,
                    seasonAssists,
                    avgSeasonRating,
                    endOverall,
                    seasonEarnings,
                    account.Balance));

                if (isFreeAgent)
                {
                    seasonsUnattached++;
                    seasonsAtCurrentClub = 0;
                }
                else
                {
                    seasonsUnattached = 0;
                    seasonsAtCurrentClub++;

                    // Evaluate squad status
                    var newStatus = CareerSystem.EvaluateSquadStatus(player, abilities, careerState, currentClub!, world);
                    careerState = careerState with { Status = newStatus };

                    // Contract renewal evaluation
                    var currentContract = world.FindContractForPlayer(playerId);
                    if (currentContract != null)
                    {
                        var status = ContractSystem.EvaluateContractStatus(currentContract, seasonEndDate);
                        if (status == ContractExpiryStatus.Expired)
                        {
                            world = ContractSystem.HandleContractExpiry(player, currentContract, world, seasonEndDate);
                            careerState = world.GetCareerState(playerId);
                            isFreeAgent = true;
                        }
                        else if (status == ContractExpiryStatus.BosmanEligible && careerState.ManagerTrust >= 35f)
                        {
                            var renewal = ContractSystem.OfferRenewal(player, abilities, careerState, currentContract, currentClub!, world, rng, seasonEndDate);
                            if (renewal != null)
                            {
                                world = TransferSystem.AcceptOffer(renewal, world, seasonEndDate);
                                careerState = world.GetCareerState(playerId);
                                if (careerState.WeeklySalary > peakSalary)
                                {
                                    peakSalary = careerState.WeeklySalary;
                                }
                            }
                        }
                    }
                }

                // Silverware / Trophy evaluation
                if (currentClub != null && leagueTier == 1)
                {
                    if (currentClub.ReputationRating >= 85 && seasonApps >= 20 && avgSeasonRating >= 6.8f)
                    {
                        float titleChance = (currentClub.ReputationRating - 75f) / 100f * 0.35f;
                        if (rng.NextFloat(0f, 1f) < titleChance)
                        {
                            totalLeagueTitles++;
                        }
                    }
                    if (currentClub.ReputationRating >= 88 && seasonApps >= 22 && avgSeasonRating >= 7.0f)
                    {
                        if (rng.NextFloat(0f, 1f) < 0.12f)
                        {
                            totalContinentalTitles++;
                        }
                    }
                }
                if (currentClub != null && leagueTier <= 2 && seasonApps >= 15 && avgSeasonRating >= 6.5f)
                {
                    if (rng.NextFloat(0f, 1f) < 0.08f)
                    {
                        totalDomesticCups++;
                    }
                }

                // International football caps and goals
                if (endOverall >= 76 && currentAge >= 19)
                {
                    int caps = rng.NextInt(3, 8);
                    totalInternationalCaps += caps;
                    if (player.PrimaryPosition == Position.ST || player.PrimaryPosition == Position.LW || player.PrimaryPosition == Position.RW || player.PrimaryPosition == Position.AM)
                    {
                        int intGoals = rng.NextInt(0, player.PrimaryPosition == Position.ST ? 4 : 2);
                        totalInternationalGoals += intGoals;
                    }

                    if (seasonIndex % 4 == 0 && endOverall >= 82)
                    {
                        totalInternationalCaps += rng.NextInt(4, 7);
                        if (rng.NextFloat(0f, 1f) < 0.08f)
                        {
                            totalInternationalTrophies++;
                        }
                    }
                }

                // Late-career physical decline for age 32+
                if (currentAge >= 32)
                {
                    var retirementSys = new RetirementSystem();
                    abilities = retirementSys.ApplyLateCareerDecline(abilities, currentAge, rng);
                }

                // Adjust lifestyle proportionally to weekly income and net worth to prevent artificial bankruptcy
                if (isFreeAgent)
                {
                    lifestyle = LifestyleTier.Modest;
                }
                else if (careerState.WeeklySalary >= 80000m && account.Balance > 1000000m)
                {
                    lifestyle = LifestyleTier.Superstar;
                }
                else if (careerState.WeeklySalary >= 25000m && account.Balance > 300000m)
                {
                    lifestyle = LifestyleTier.Extravagant;
                }
                else if (careerState.WeeklySalary >= 6000m && account.Balance > 80000m)
                {
                    lifestyle = LifestyleTier.Luxurious;
                }
                else if (careerState.WeeklySalary >= 1500m && account.Balance > 15000m)
                {
                    lifestyle = LifestyleTier.Comfortable;
                }
                else
                {
                    lifestyle = LifestyleTier.Modest;
                }

                // 5. Retirement Evaluation
                retirementAge = currentAge;

                bool shouldRetire = false;

                // A. Hard ceiling at 38
                if (currentAge >= 38)
                {
                    shouldRetire = true;
                }
                // B. Age >= 35 with physical/ability degradation or inability to get game time
                else if (currentAge >= 35 && (endOverall < 62 || (seasonApps < 8 && careerState.ManagerTrust < 30f)))
                {
                    shouldRetire = true;
                }
                // C. Age >= 34 with severe decline
                else if (currentAge >= 34 && endOverall < 56)
                {
                    shouldRetire = true;
                }
                // D. Free agent stagnation (> 1 full season without finding a club)
                else if (isFreeAgent && seasonsUnattached >= 1 && currentAge >= 33)
                {
                    shouldRetire = true;
                }

                if (shouldRetire)
                {
                    break;
                }
            }

            float careerAvgRating = totalAppearances > 0 ? (float)(sumRating / totalAppearances) : 6.0f;

            int totalTrophies = totalLeagueTitles + totalDomesticCups + totalContinentalTitles + totalInternationalTrophies;
            var legacySys = new LegacySystem();
            int cleanSheets = (player.PrimaryPosition == Position.CB || player.PrimaryPosition == Position.FB || player.PrimaryPosition == Position.GK)
                ? (int)(totalAppearances * 0.35)
                : 0;

            int careerScore = legacySys.CalculateCareerScore(
                appearances: totalAppearances,
                goals: totalGoals,
                assists: totalAssists,
                cleanSheets: cleanSheets,
                internationalCaps: totalInternationalCaps,
                internationalGoals: totalInternationalGoals,
                leagueTitles: totalLeagueTitles,
                continentalTitles: totalContinentalTitles,
                domesticCups: totalDomesticCups,
                internationalTrophies: totalInternationalTrophies,
                peakOvr: peakOverall,
                lifetimeEarnings: (long)totalEarnings);

            var legacyGrade = legacySys.DetermineGrade(careerScore);
            var legacy = new CareerLegacy(
                lifetimeAppearances: totalAppearances,
                lifetimeGoals: totalGoals,
                lifetimeAssists: totalAssists,
                lifetimeCleanSheets: cleanSheets,
                internationalCaps: totalInternationalCaps,
                internationalGoals: totalInternationalGoals,
                leagueTitles: totalLeagueTitles,
                continentalTitles: totalContinentalTitles,
                domesticCups: totalDomesticCups,
                internationalTrophies: totalInternationalTrophies,
                lifetimeEarnings: (long)totalEarnings,
                peakOverallRating: peakOverall,
                seasonsPlayed: seasonRecords.Count,
                careerScore: careerScore,
                grade: legacyGrade,
                isHallOfFameInductee: false,
                trophies: null);

            bool isHallOfFame = legacySys.IsEligibleForHallOfFame(legacy);

            return new CareerStatistics(
                Seed: seed,
                PlayerId: playerId,
                Name: player.Name,
                Position: player.PrimaryPosition,
                StartingOverall: startingOverall,
                PeakOverall: peakOverall,
                PeakAge: peakAge,
                RetirementAge: retirementAge,
                SeasonsPlayed: seasonRecords.Count,
                TotalAppearances: totalAppearances,
                TotalGoals: totalGoals,
                TotalAssists: totalAssists,
                AverageRating: careerAvgRating,
                TotalEarnings: totalEarnings,
                FinalBalance: account.Balance,
                PeakWeeklySalary: peakSalary,
                BankruptcyOccurred: bankruptcyOccurred,
                TransferCount: transferCount,
                Seasons: seasonRecords,
                CommercialEarnings: totalCommercialEarnings,
                TotalTrophies: totalTrophies,
                InternationalCaps: totalInternationalCaps,
                InternationalGoals: totalInternationalGoals,
                CareerScore: careerScore,
                LegacyGrade: legacyGrade,
                IsHallOfFame: isHallOfFame);
        }

        private static Position PickPosition(SimulationRandom rng)
        {
            var positions = new[] { Position.ST, Position.CM, Position.CB, Position.LW, Position.RW, Position.AM, Position.DM, Position.FB };
            return positions[rng.NextInt(0, positions.Length)];
        }

        private static PlayerAbilities GenerateRookieAbilities(Position pos, PlayerPotential potential, SimulationRandom rng)
        {
            int baseVal = potential.Range switch
            {
                PotentialRange.Elite => rng.NextInt(58, 64),
                PotentialRange.High => rng.NextInt(55, 60),
                PotentialRange.Medium => rng.NextInt(52, 57),
                _ => rng.NextInt(48, 53)
            };

            var weights = PositionWeightMap.For(pos);

            int GetAttr(AttributeName attr)
            {
                float weight = weights.TryGetValue(attr, out var w) ? w : 0.05f;
                int bonus = (int)Math.Round(weight * 12f);
                int variance = rng.NextInt(-2, 3);
                return Math.Clamp(baseVal + bonus + variance, 45, 68);
            }

            return new PlayerAbilities(
                pace: GetAttr(AttributeName.Pace),
                acceleration: GetAttr(AttributeName.Acceleration),
                stamina: GetAttr(AttributeName.Stamina),
                strength: GetAttr(AttributeName.Strength),
                agility: GetAttr(AttributeName.Agility),
                passing: GetAttr(AttributeName.Passing),
                shooting: GetAttr(AttributeName.Shooting),
                dribbling: GetAttr(AttributeName.Dribbling),
                crossing: GetAttr(AttributeName.Crossing),
                firstTouch: GetAttr(AttributeName.FirstTouch),
                tackling: GetAttr(AttributeName.Tackling),
                vision: GetAttr(AttributeName.Vision),
                composure: GetAttr(AttributeName.Composure),
                positioning: GetAttr(AttributeName.Positioning),
                decisionMaking: GetAttr(AttributeName.DecisionMaking));
        }

        private static PlayerPotential GeneratePotential(SimulationRandom rng)
        {
            float roll = rng.NextFloat(0f, 1f);
            if (roll < 0.05f)
            {
                return PlayerPotential.CreateClamped(rng.NextInt(86, 94), PotentialRange.Elite);
            }
            if (roll < 0.30f)
            {
                return PlayerPotential.CreateClamped(rng.NextInt(79, 86), PotentialRange.High);
            }
            if (roll < 0.90f)
            {
                return PlayerPotential.CreateClamped(rng.NextInt(68, 79), PotentialRange.Medium);
            }
            return PlayerPotential.CreateClamped(rng.NextInt(60, 68), PotentialRange.Low);
        }

        private static Club PickOpponent(Club myClub, IReadOnlyList<Club> allClubs, SimulationRandom rng)
        {
            var sameLeague = allClubs.Where(c => c.LeagueId == myClub.LeagueId && c.Id != myClub.Id).ToList();
            if (sameLeague.Count > 0)
            {
                return sameLeague[rng.NextInt(0, sameLeague.Count)];
            }
            var others = allClubs.Where(c => c.Id != myClub.Id).ToList();
            return others[rng.NextInt(0, others.Count)];
        }

        private sealed class Ecosystem
        {
            public WorldState World { get; init; } = null!;
            public IReadOnlyList<Club> Tier1Clubs { get; init; } = null!;
            public IReadOnlyList<Club> Tier2Clubs { get; init; } = null!;
            public IReadOnlyList<Club> Tier3Clubs { get; init; } = null!;
            public IReadOnlyList<Club> Tier4Clubs { get; init; } = null!;
            public IReadOnlyList<Club> AllClubs { get; init; } = null!;
        }

        private static Ecosystem InitializeEcosystem()
        {
            var season = new Season(2026, new DateOnly(2026, 8, 1), new DateOnly(2027, 5, 30), new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()), Array.Empty<Matchday>(), 1);
            var world = WorldState.CreateEmpty(season);

            var l1 = League.Create("Premier League", "ENG", 1, 20, 38, 4, 3);
            var l2 = League.Create("Championship", "ENG", 2, 24, 46, 2, 3);
            var l3 = League.Create("League One", "ENG", 3, 24, 46, 2, 3);
            var l4 = League.Create("League Two", "ENG", 4, 24, 46, 2, 3);
            world = world.WithLeague(l1).WithLeague(l2).WithLeague(l3).WithLeague(l4);

            var t1Clubs = new List<Club>
            {
                Club.Create("Arsenal FC", "ARS", l1.Id, 85, new ClubFinances(250000000m, 180000m), 5, TacticalIdentity.Possession),
                Club.Create("Manchester City", "MCI", l1.Id, 88, new ClubFinances(300000000m, 220000m), 5, TacticalIdentity.Possession),
                Club.Create("Aston Villa", "AVL", l1.Id, 76, new ClubFinances(120000000m, 95000m), 4, TacticalIdentity.HighPress),
                Club.Create("West Ham", "WHU", l1.Id, 72, new ClubFinances(90000000m, 75000m), 4, TacticalIdentity.Counter)
            };

            var t2Clubs = new List<Club>
            {
                Club.Create("Leeds United", "LEE", l2.Id, 65, new ClubFinances(40000000m, 45000m), 3, TacticalIdentity.HighPress),
                Club.Create("Sheffield United", "SHU", l2.Id, 62, new ClubFinances(35000000m, 40000m), 3, TacticalIdentity.Direct),
                Club.Create("Bristol City", "BRC", l2.Id, 55, new ClubFinances(20000000m, 28000m), 3, TacticalIdentity.Possession),
                Club.Create("Preston Athletic", "PRE", l2.Id, 50, new ClubFinances(15000000m, 22000m), 2, TacticalIdentity.Counter)
            };

            var t3Clubs = new List<Club>
            {
                Club.Create("Derby County", "DER", l3.Id, 48, new ClubFinances(8000000m, 15000m), 2, TacticalIdentity.HighPress),
                Club.Create("Peterborough", "PET", l3.Id, 44, new ClubFinances(6000000m, 12000m), 2, TacticalIdentity.Direct),
                Club.Create("Lincoln City", "LIN", l3.Id, 40, new ClubFinances(5000000m, 10000m), 2, TacticalIdentity.Possession)
            };

            var t4Clubs = new List<Club>
            {
                Club.Create("Grimsby Town", "GRI", l4.Id, 34, new ClubFinances(3000000m, 6000m), 1, TacticalIdentity.Counter),
                Club.Create("Salford City", "SAL", l4.Id, 32, new ClubFinances(3500000m, 7000m), 2, TacticalIdentity.Possession),
                Club.Create("Crewe Alexandra", "CRE", l4.Id, 30, new ClubFinances(2500000m, 5000m), 1, TacticalIdentity.Direct)
            };

            var all = t1Clubs.Concat(t2Clubs).Concat(t3Clubs).Concat(t4Clubs).ToList();
            foreach (var club in all)
            {
                world = world.WithClub(club);
            }

            return new Ecosystem
            {
                World = world,
                Tier1Clubs = t1Clubs,
                Tier2Clubs = t2Clubs,
                Tier3Clubs = t3Clubs,
                Tier4Clubs = t4Clubs,
                AllClubs = all
            };
        }
    }
}
