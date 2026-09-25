using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# deterministic world simulation engine that drives NPC player progression,
    /// aging/decline, club squad replenishment, and multi-tier league promotion/relegation (#P5-001, #P5-003).
    /// </summary>
    public static class WorldSimulationSystem
    {
        public const int MinSquadSize = 18;
        public const int StandardRetirementAge = 36;

        /// <summary>
        /// Simulates developmental growth, physical decline, and retirements for all NPC players in the universe.
        /// </summary>
        public static (WorldState World, int RetiredCount) AgeAndDevelopNpcPlayers(
            WorldState world,
            Guid? protagonistPlayerId,
            DateOnly seasonEndDate,
            SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var updatedPlayers = new Dictionary<Guid, Player>(world.Players);
            var updatedAbilities = new Dictionary<Guid, PlayerAbilities>(world.Abilities);
            var updatedCareerStates = new Dictionary<Guid, PlayerCareerState>(world.CareerStates);
            var updatedClubs = new Dictionary<Guid, Club>(world.Clubs);
            var updatedContracts = new Dictionary<Guid, Contract>(world.Contracts);

            int retiredCount = 0;

            foreach (var kvp in world.Players)
            {
                var playerId = kvp.Key;
                var player = kvp.Value;

                // Skip protagonist (progressed independently via player loop)
                if (protagonistPlayerId.HasValue && playerId == protagonistPlayerId.Value)
                {
                    continue;
                }

                if (!world.Abilities.TryGetValue(playerId, out var abilities))
                {
                    continue;
                }

                int age = player.GetAgeAt(seasonEndDate);

                // Retirement evaluation
                bool shouldRetire = false;
                if (age >= StandardRetirementAge)
                {
                    shouldRetire = true;
                }
                else if (age >= 33 && abilities.CalculateAverage() < 55)
                {
                    shouldRetire = true;
                }

                if (shouldRetire)
                {
                    retiredCount++;
                    // Remove from club roster
                    if (world.CareerStates.TryGetValue(playerId, out var cs) && updatedClubs.TryGetValue(cs.ClubId, out var club))
                    {
                        updatedClubs[club.Id] = club.WithRemovedPlayer(playerId);
                    }
                    continue;
                }

                // Age-bracket development & decline
                float potentialCeiling = 75f;
                if (world.Potentials.TryGetValue(playerId, out var pot))
                {
                    potentialCeiling = pot.PotentialRating;
                }

                var evolvedAbilities = EvolveAbilities(abilities, age, potentialCeiling, rng);
                updatedAbilities[playerId] = evolvedAbilities;
            }

            var nextWorld = new WorldState(
                updatedClubs,
                updatedPlayers,
                updatedAbilities,
                world.States,
                updatedCareerStates,
                world.Leagues,
                world.Managers,
                updatedContracts,
                world.Potentials,
                world.CurrentSeason,
                world.Accounts,
                world.EventCooldowns,
                world.Relationships,
                world.TransferOffers);

            return (nextWorld, retiredCount);
        }

        private static PlayerAbilities EvolveAbilities(
            PlayerAbilities current,
            int age,
            float potentialRating,
            SimulationRandom rng)
        {
            float currentAvg = current.CalculateAverage();

            if (age < 21)
            {
                // Rapid youth growth: +2 to +4 points towards potential
                if (currentAvg < potentialRating)
                {
                    int gain = rng.NextInt(2, 5);
                    return ApplyAttributeDelta(current, physicalDelta: gain, technicalDelta: gain, mentalDelta: gain / 2);
                }
            }
            else if (age <= 28)
            {
                // Maturation / Prime: moderate growth until potential
                if (currentAvg < potentialRating)
                {
                    int gain = rng.NextInt(0, 3);
                    return ApplyAttributeDelta(current, physicalDelta: gain, technicalDelta: gain, mentalDelta: gain);
                }
            }
            else if (age <= 30)
            {
                // Peak plateau: stable
                return current;
            }
            else
            {
                // 31+: Physical decline, mental retention
                int physicalDrop = rng.NextInt(1, 3);
                int mentalGain = rng.NextInt(0, 2);
                return ApplyAttributeDelta(current, physicalDelta: -physicalDrop, technicalDelta: 0, mentalDelta: mentalGain);
            }

            return current;
        }

        private static PlayerAbilities ApplyAttributeDelta(
            PlayerAbilities current,
            int physicalDelta,
            int technicalDelta,
            int mentalDelta)
        {
            byte ClampAttr(int val, int delta) => (byte)Math.Clamp(val + delta, 25, 99);

            return current with
            {
                Pace = ClampAttr(current.Pace, physicalDelta),
                Acceleration = ClampAttr(current.Acceleration, physicalDelta),
                Stamina = ClampAttr(current.Stamina, physicalDelta),
                Strength = ClampAttr(current.Strength, physicalDelta),

                Passing = ClampAttr(current.Passing, technicalDelta),
                Shooting = ClampAttr(current.Shooting, technicalDelta),
                Dribbling = ClampAttr(current.Dribbling, technicalDelta),
                FirstTouch = ClampAttr(current.FirstTouch, technicalDelta),
                Crossing = ClampAttr(current.Crossing, technicalDelta),

                Composure = ClampAttr(current.Composure, mentalDelta),
                DecisionMaking = ClampAttr(current.DecisionMaking, mentalDelta),
                Vision = ClampAttr(current.Vision, mentalDelta)
            };
        }

        /// <summary>
        /// Replenishes AI club squads that have dropped below minimum roster threshold.
        /// </summary>
        public static WorldState ReplenishSquads(WorldState world, SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var updatedClubs = new Dictionary<Guid, Club>(world.Clubs);
            var updatedPlayers = new Dictionary<Guid, Player>(world.Players);
            var updatedAbilities = new Dictionary<Guid, PlayerAbilities>(world.Abilities);
            var updatedCareerStates = new Dictionary<Guid, PlayerCareerState>(world.CareerStates);
            var updatedContracts = new Dictionary<Guid, Contract>(world.Contracts);
            var updatedPotentials = new Dictionary<Guid, PlayerPotential>(world.Potentials);

            var positions = new[] { Position.ST, Position.CM, Position.CB, Position.FB, Position.LW, Position.RW, Position.GK };

            foreach (var club in world.Clubs.Values)
            {
                int needed = MinSquadSize - club.SquadPlayerIds.Count;
                if (needed <= 0) continue;

                var clubObj = club;
                int tier = 4;
                if (world.Leagues.TryGetValue(club.LeagueId, out var league))
                {
                    tier = league.Tier;
                }

                var tierProfile = LeagueTierConfig.GetProfile(tier);

                for (int i = 0; i < needed; i++)
                {
                    var newPlayerId = Guid.NewGuid();
                    var pos = positions[rng.NextInt(0, positions.Length)];
                    var dob = new DateOnly(2007, rng.NextInt(1, 13), rng.NextInt(1, 28));
                    var newPlayer = new Player(newPlayerId, $"Prospect {club.ShortName}-{i + 1}", "ENG", dob, Foot.Right, pos);

                    int baseStat = tier switch
                    {
                        1 => rng.NextInt(68, 76),
                        2 => rng.NextInt(60, 68),
                        3 => rng.NextInt(52, 60),
                        _ => rng.NextInt(46, 54)
                    };

                    byte b(int val) => (byte)Math.Clamp(val + rng.NextInt(-3, 4), 30, 99);
                    var newAbilities = new PlayerAbilities(
                        pace: b(baseStat),
                        acceleration: b(baseStat),
                        stamina: b(baseStat),
                        strength: b(baseStat),
                        agility: b(baseStat),
                        passing: b(baseStat),
                        shooting: b(baseStat),
                        dribbling: b(baseStat),
                        crossing: b(baseStat),
                        firstTouch: b(baseStat),
                        tackling: b(baseStat),
                        vision: b(baseStat),
                        composure: b(baseStat),
                        positioning: b(baseStat),
                        decisionMaking: b(baseStat));

                    var newCareerState = new PlayerCareerState(
                        clubId: club.Id,
                        status: SquadStatus.Rotation,
                        managerTrust: 50f,
                        weeklySalary: tierProfile.AverageWeeklyWage * 0.4m,
                        marketValue: tierProfile.AverageWeeklyWage * 50m,
                        reputation: tierProfile.PrestigeRating * 0.5f);

                    var newContract = Contract.Create(
                        newPlayerId,
                        club.Id,
                        newCareerState.WeeklySalary,
                        world.CurrentSeason.StartDate,
                        world.CurrentSeason.EndDate.AddYears(2),
                        SquadRole.Rotation,
                        new ContractBonuses(50m, 25m, 20m, 30m));

                    updatedPlayers[newPlayerId] = newPlayer;
                    updatedAbilities[newPlayerId] = newAbilities;
                    updatedCareerStates[newPlayerId] = newCareerState;
                    updatedContracts[newPlayerId] = newContract;
                    updatedPotentials[newPlayerId] = PlayerPotential.CreateClamped(baseStat + rng.NextInt(8, 16), PotentialRange.Medium);

                    clubObj = clubObj.WithAddedPlayer(newPlayerId);
                }

                updatedClubs[club.Id] = clubObj;
            }

            return new WorldState(
                updatedClubs,
                updatedPlayers,
                updatedAbilities,
                world.States,
                updatedCareerStates,
                world.Leagues,
                world.Managers,
                updatedContracts,
                updatedPotentials,
                world.CurrentSeason,
                world.Accounts,
                world.EventCooldowns,
                world.Relationships,
                world.TransferOffers);
        }

        /// <summary>
        /// Resolves final season standings, titles, promotions, and relegations across all leagues (#P5-001, #P5-003).
        /// </summary>
        public static (WorldState World, WorldSeasonResolution Resolution) ResolveLeagueSeason(
            WorldState world,
            int seasonYear,
            SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var updatedClubs = new Dictionary<Guid, Club>(world.Clubs);
            var leagueResolutions = new List<LeagueSeasonResolution>();
            var allPromotedClubIds = new List<Guid>();
            var allRelegatedClubIds = new List<Guid>();

            // Group clubs by league tier
            var clubsByTier = new Dictionary<int, List<Club>>();
            for (int t = 1; t <= 4; t++) clubsByTier[t] = new List<Club>();

            foreach (var club in world.Clubs.Values)
            {
                int tier = 4;
                if (world.Leagues.TryGetValue(club.LeagueId, out var league))
                {
                    tier = league.Tier;
                }
                clubsByTier[tier].Add(club);
            }

            // Resolve each league's standings
            var standingsByTier = new Dictionary<int, List<ClubSeasonOutcome>>();

            for (int tier = 1; tier <= 4; tier++)
            {
                var clubsInTier = clubsByTier[tier];
                if (clubsInTier.Count == 0) continue;

                var league = world.Leagues.Values.FirstOrDefault(l => l.Tier == tier);
                Guid leagueId = league?.Id ?? Guid.NewGuid();
                string leagueName = league?.Name ?? $"Tier {tier} League";

                // Score clubs based on squad ability + club reputation + deterministic variance
                var ranked = clubsInTier
                    .Select(c =>
                    {
                        float squadStrength = c.ReputationRating;
                        float roll = rng.NextFloat(0.85f, 1.15f);
                        float performanceScore = squadStrength * roll;
                        return (Club: c, Score: performanceScore);
                    })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var tierProfile = LeagueTierConfig.GetProfile(tier);
                int totalClubs = ranked.Count;
                int promoSpots = league != null && league.PromotionSlots > 0 ? league.PromotionSlots : tierProfile.PromotionSpots;
                int relegSpots = league != null && league.RelegationSlots > 0 ? league.RelegationSlots : tierProfile.RelegationSpots;

                promoSpots = Math.Clamp(promoSpots, 0, Math.Max(0, totalClubs - 1));
                relegSpots = Math.Clamp(relegSpots, 0, Math.Max(0, totalClubs - 1));
                if (promoSpots + relegSpots >= totalClubs)
                {
                    relegSpots = Math.Max(0, totalClubs - promoSpots - 1);
                }

                var outcomes = new List<ClubSeasonOutcome>();
                var promotedThisTier = new List<Guid>();
                var relegatedThisTier = new List<Guid>();

                for (int pos = 0; pos < totalClubs; pos++)
                {
                    var club = ranked[pos].Club;
                    int rank = pos + 1;
                    int pts = Math.Max(20, (int)(ranked[pos].Score * 0.9f) - (pos * 2));
                    int gd = (totalClubs - pos) * 3 - 25;

                    bool wonTitle = rank == 1;
                    bool isPromoted = rank <= promoSpots && tier > 1;
                    bool isRelegated = rank > (totalClubs - relegSpots) && tier < 4;
                    bool isContinental = rank <= tierProfile.ContinentalQualificationSpots && tier == 1;

                    if (isPromoted)
                    {
                        promotedThisTier.Add(club.Id);
                        allPromotedClubIds.Add(club.Id);
                    }
                    if (isRelegated)
                    {
                        relegatedThisTier.Add(club.Id);
                        allRelegatedClubIds.Add(club.Id);
                    }

                    outcomes.Add(new ClubSeasonOutcome
                    {
                        ClubId = club.Id,
                        ClubName = club.Name,
                        LeagueId = leagueId,
                        FinalPosition = rank,
                        Points = pts,
                        GoalDifference = gd,
                        WonTitle = wonTitle,
                        Promoted = isPromoted,
                        Relegated = isRelegated,
                        QualifiedForContinental = isContinental
                    });
                }

                standingsByTier[tier] = outcomes;

                leagueResolutions.Add(new LeagueSeasonResolution
                {
                    SeasonYear = seasonYear,
                    LeagueId = leagueId,
                    LeagueName = leagueName,
                    Tier = tier,
                    ChampionClubId = ranked[0].Club.Id,
                    ChampionClubName = ranked[0].Club.Name,
                    PromotedClubIds = promotedThisTier,
                    RelegatedClubIds = relegatedThisTier,
                    Standings = outcomes
                });
            }

            // Swap clubs between tiers safely without double-swapping
            var clubsAlreadyMoved = new HashSet<Guid>();
            SwapClubsBetweenLeagues(updatedClubs, world, 1, 2, standingsByTier, clubsAlreadyMoved);
            SwapClubsBetweenLeagues(updatedClubs, world, 2, 3, standingsByTier, clubsAlreadyMoved);
            SwapClubsBetweenLeagues(updatedClubs, world, 3, 4, standingsByTier, clubsAlreadyMoved);

            var nextWorld = new WorldState(
                updatedClubs,
                world.Players,
                world.Abilities,
                world.States,
                world.CareerStates,
                world.Leagues,
                world.Managers,
                world.Contracts,
                world.Potentials,
                world.CurrentSeason,
                world.Accounts,
                world.EventCooldowns,
                world.Relationships,
                world.TransferOffers);

            var resolution = new WorldSeasonResolution
            {
                SeasonYear = seasonYear,
                LeagueResolutions = leagueResolutions,
                PromotedClubIds = allPromotedClubIds,
                RelegatedClubIds = allRelegatedClubIds,
                TotalPlayerTransfers = 0,
                TotalPlayerRetirements = 0
            };

            return (nextWorld, resolution);
        }

        private static void SwapClubsBetweenLeagues(
            Dictionary<Guid, Club> updatedClubs,
            WorldState world,
            int upperTier,
            int lowerTier,
            Dictionary<int, List<ClubSeasonOutcome>> standingsByTier,
            HashSet<Guid> clubsAlreadyMoved)
        {
            if (!standingsByTier.TryGetValue(upperTier, out var upperStandings) ||
                !standingsByTier.TryGetValue(lowerTier, out var lowerStandings))
            {
                return;
            }

            var upperLeague = world.Leagues.Values.FirstOrDefault(l => l.Tier == upperTier);
            var lowerLeague = world.Leagues.Values.FirstOrDefault(l => l.Tier == lowerTier);

            if (upperLeague == null || lowerLeague == null) return;

            var relegatedFromUpper = upperStandings
                .Where(s => s.Relegated && !clubsAlreadyMoved.Contains(s.ClubId))
                .Select(s => s.ClubId)
                .ToList();
            var promotedFromLower = lowerStandings
                .Where(s => s.Promoted && !clubsAlreadyMoved.Contains(s.ClubId))
                .Select(s => s.ClubId)
                .ToList();

            int swapCount = Math.Min(relegatedFromUpper.Count, promotedFromLower.Count);

            for (int i = 0; i < swapCount; i++)
            {
                var relClubId = relegatedFromUpper[i];
                var promClubId = promotedFromLower[i];

                if (updatedClubs.TryGetValue(relClubId, out var relClub))
                {
                    updatedClubs[relClubId] = relClub.WithLeague(lowerLeague.Id);
                    clubsAlreadyMoved.Add(relClubId);
                }

                if (updatedClubs.TryGetValue(promClubId, out var promClub))
                {
                    updatedClubs[promClubId] = promClub.WithLeague(upperLeague.Id);
                    clubsAlreadyMoved.Add(promClubId);
                }
            }
        }
    }
}
