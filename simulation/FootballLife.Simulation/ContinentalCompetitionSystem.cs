using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# system governing continental club tournaments (e.g. Champions Cup) (#P5-005).
    /// Handles qualification, group stage draws, knockout progression, aggregate score resolution,
    /// and prize money distribution.
    /// Fully deterministic via <see cref="SimulationRandom"/>.
    /// </summary>
    public static class ContinentalCompetitionSystem
    {
        public const int DefaultSlots = 32;
        public const int DefaultGroups = 8;
        public const decimal DefaultPrizePool = 50_000_000m;

        /// <summary>
        /// Selects qualifying clubs from top-tier league season resolutions.
        /// </summary>
        public static IReadOnlyList<Guid> QualifyClubs(
            WorldState world,
            IReadOnlyList<LeagueSeasonResolution> leagueResolutions,
            int slotsPerTopLeague = 4,
            int totalSlots = DefaultSlots)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (leagueResolutions is null) throw new ArgumentNullException(nameof(leagueResolutions));

            var qualified = new HashSet<Guid>();

            // 1. Take top finishing clubs from top-flight (Tier 1) leagues
            var tier1Leagues = leagueResolutions.Where(lr => lr.Tier == 1).ToList();
            foreach (var resolution in tier1Leagues)
            {
                var topClubs = resolution.Standings
                    .OrderBy(s => s.FinalPosition)
                    .Take(slotsPerTopLeague)
                    .Select(s => s.ClubId);

                foreach (var clubId in topClubs)
                {
                    if (qualified.Count >= totalSlots) break;
                    qualified.Add(clubId);
                }
            }

            // 2. Fill any remaining slots with highest reputation clubs in the world not yet qualified
            if (qualified.Count < totalSlots)
            {
                var remaining = world.Clubs.Values
                    .Where(c => !qualified.Contains(c.Id))
                    .OrderByDescending(c => c.ReputationRating)
                    .Select(c => c.Id);

                foreach (var clubId in remaining)
                {
                    if (qualified.Count >= totalSlots) break;
                    qualified.Add(clubId);
                }
            }

            return qualified.ToList();
        }

        /// <summary>
        /// Draws qualified clubs into 8 groups of 4 clubs (Groups A through H).
        /// Respects league separation constraints where possible.
        /// </summary>
        public static IReadOnlyList<ContinentalGroupStanding> DrawGroups(
            IReadOnlyList<Guid> qualifiedClubs,
            WorldState world,
            SimulationRandom rng,
            int groupCount = DefaultGroups)
        {
            if (qualifiedClubs is null) throw new ArgumentNullException(nameof(qualifiedClubs));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));
            if (groupCount <= 0) throw new ArgumentOutOfRangeException(nameof(groupCount));

            char startLetter = 'A';
            var groupBuckets = new List<List<Guid>>();
            for (int i = 0; i < groupCount; i++)
            {
                groupBuckets.Add(new List<Guid>());
            }

            // Shuffle clubs deterministically
            var shuffledClubs = qualifiedClubs.OrderBy(_ => rng.NextFloat(0f, 1f)).ToList();

            // Distribute clubs avoiding same league when possible
            foreach (var clubId in shuffledClubs)
            {
                var club = world.Clubs.TryGetValue(clubId, out var c) ? c : null;
                Guid leagueId = club?.LeagueId ?? Guid.Empty;

                // Find a group with fewer than (total/groupCount) members and no same league club
                int bestGroup = -1;
                int minGroupSize = int.MaxValue;

                for (int g = 0; g < groupCount; g++)
                {
                    var group = groupBuckets[g];
                    if (group.Count < minGroupSize)
                    {
                        bool hasSameLeague = group.Any(id =>
                            world.Clubs.TryGetValue(id, out var other) && other.LeagueId == leagueId);

                        if (!hasSameLeague)
                        {
                            bestGroup = g;
                            minGroupSize = group.Count;
                        }
                    }
                }

                // Fallback to least filled group if conflict unavoidable
                if (bestGroup == -1)
                {
                    bestGroup = 0;
                    for (int g = 1; g < groupCount; g++)
                    {
                        if (groupBuckets[g].Count < groupBuckets[bestGroup].Count)
                        {
                            bestGroup = g;
                        }
                    }
                }

                groupBuckets[bestGroup].Add(clubId);
            }

            var standings = new List<ContinentalGroupStanding>();
            for (int i = 0; i < groupCount; i++)
            {
                string groupName = ((char)(startLetter + i)).ToString();
                var entries = groupBuckets[i].Select(id => new GroupEntry(id)).ToList();
                standings.Add(new ContinentalGroupStanding(groupName, entries));
            }

            return standings;
        }

        /// <summary>
        /// Simulates all group stage fixtures (home & away round-robin) and resolves final standings.
        /// </summary>
        public static ContinentalCompetition SimulateGroupStage(
            ContinentalCompetition competition,
            WorldState world,
            SimulationRandom rng)
        {
            if (competition is null) throw new ArgumentNullException(nameof(competition));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var updatedGroups = new List<ContinentalGroupStanding>();

            foreach (var group in competition.GroupStandings)
            {
                var entryMap = group.Entries.ToDictionary(e => e.ClubId);
                var clubIds = group.Entries.Select(e => e.ClubId).ToList();

                // Round-robin: each pair plays home & away (2 matches each)
                for (int i = 0; i < clubIds.Count; i++)
                {
                    for (int j = i + 1; j < clubIds.Count; j++)
                    {
                        Guid home = clubIds[i];
                        Guid away = clubIds[j];

                        // Match 1: i at home
                        var (hScore1, aScore1) = SimulateMatchScore(home, away, world, rng);
                        entryMap[home] = entryMap[home].WithMatchResult(hScore1, aScore1);
                        entryMap[away] = entryMap[away].WithMatchResult(aScore1, hScore1);

                        // Match 2: j at home
                        var (hScore2, aScore2) = SimulateMatchScore(away, home, world, rng);
                        entryMap[away] = entryMap[away].WithMatchResult(hScore2, aScore2);
                        entryMap[home] = entryMap[home].WithMatchResult(aScore2, hScore2);
                    }
                }

                var resolvedStanding = new ContinentalGroupStanding(group.GroupName, entryMap.Values.ToList());
                updatedGroups.Add(resolvedStanding);
            }

            return competition with { GroupStandings = updatedGroups };
        }

        /// <summary>
        /// Generates Round of 16 knockout fixtures pairing group winners against group runners-up.
        /// Prevents pairing clubs from the same group.
        /// </summary>
        public static ContinentalCompetition GenerateKnockoutBracket(
            ContinentalCompetition competition,
            WorldState world,
            SimulationRandom rng)
        {
            if (competition is null) throw new ArgumentNullException(nameof(competition));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var winners = competition.GroupStandings.Select(g => (g.GroupName, ClubId: g.GetGroupWinner())).ToList();
            var runnersUp = competition.GroupStandings.Select(g => (g.GroupName, ClubId: g.GetGroupRunnerUp())).ToList();

            var shuffledRunnersUp = runnersUp.OrderBy(_ => rng.NextFloat(0f, 1f)).ToList();
            var fixtures = new List<ContinentalFixture>();

            for (int i = 0; i < winners.Count; i++)
            {
                var winner = winners[i];
                // Find runner-up not from same group
                var matchedRunnerUp = shuffledRunnersUp.FirstOrDefault(r => r.GroupName != winner.GroupName);
                if (matchedRunnerUp.ClubId == Guid.Empty)
                {
                    matchedRunnerUp = shuffledRunnersUp[0];
                }
                shuffledRunnersUp.Remove(matchedRunnerUp);

                // Two legs for Round of 16 (Leg 1: runner-up at home, Leg 2: winner at home)
                fixtures.Add(ContinentalFixture.Create(matchedRunnerUp.ClubId, winner.ClubId, ContinentalStage.RoundOf16, 1));
                fixtures.Add(ContinentalFixture.Create(winner.ClubId, matchedRunnerUp.ClubId, ContinentalStage.RoundOf16, 2));
            }

            return competition with { KnockoutFixtures = fixtures };
        }

        /// <summary>
        /// Simulates the specified knockout stage and advances winners to the next stage.
        /// Handles 2-legged ties, aggregate scores, extra time/penalties, and the Final.
        /// </summary>
        public static ContinentalCompetition SimulateKnockoutRound(
            ContinentalCompetition competition,
            ContinentalStage stage,
            WorldState world,
            SimulationRandom rng)
        {
            if (competition is null) throw new ArgumentNullException(nameof(competition));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var currentStageFixtures = competition.KnockoutFixtures
                .Where(f => f.Stage == stage)
                .ToList();

            if (currentStageFixtures.Count == 0) return competition;

            var resolvedFixtures = new List<ContinentalFixture>();
            var roundWinners = new List<Guid>();

            if (stage == ContinentalStage.Final)
            {
                // Single-leg final
                var final = currentStageFixtures[0];
                var (hScore, aScore) = SimulateMatchScore(final.HomeClubId, final.AwayClubId, world, rng);

                // If draw in final, resolve via penalties
                if (hScore == aScore)
                {
                    if (rng.NextFloat(0f, 1f) > 0.5f) hScore++;
                    else aScore++;
                }

                var completedFinal = final.WithResult(hScore, aScore);
                resolvedFixtures.Add(completedFinal);

                Guid championId = hScore > aScore ? completedFinal.HomeClubId : completedFinal.AwayClubId;

                var allFixtures = competition.KnockoutFixtures
                    .Where(f => f.Stage != stage)
                    .Concat(resolvedFixtures)
                    .ToList();

                return competition with
                {
                    KnockoutFixtures = allFixtures,
                    WinnerId = championId,
                    IsCompleted = true
                };
            }

            // 2-legged rounds (RoundOf16, QuarterFinal, SemiFinal)
            // Group fixtures by club pairs
            for (int i = 0; i < currentStageFixtures.Count; i += 2)
            {
                var leg1 = currentStageFixtures[i];
                var leg2 = currentStageFixtures[i + 1];

                var (h1, a1) = SimulateMatchScore(leg1.HomeClubId, leg1.AwayClubId, world, rng);
                var (h2, a2) = SimulateMatchScore(leg2.HomeClubId, leg2.AwayClubId, world, rng);

                var resolvedLeg1 = leg1.WithResult(h1, a1);
                var resolvedLeg2 = leg2.WithResult(h2, a2);

                resolvedFixtures.Add(resolvedLeg1);
                resolvedFixtures.Add(resolvedLeg2);

                // Aggregate: leg1.HomeClubId scored h1 + a2
                // leg1.AwayClubId scored a1 + h2
                int club1Goals = h1 + a2;
                int club2Goals = a1 + h2;

                Guid tieWinner;
                if (club1Goals > club2Goals)
                {
                    tieWinner = leg1.HomeClubId;
                }
                else if (club2Goals > club1Goals)
                {
                    tieWinner = leg1.AwayClubId;
                }
                else
                {
                    // Decider (penalties)
                    tieWinner = rng.NextFloat(0f, 1f) > 0.5f ? leg1.HomeClubId : leg1.AwayClubId;
                }

                roundWinners.Add(tieWinner);
            }

            // Generate next stage fixtures
            ContinentalStage nextStage = stage switch
            {
                ContinentalStage.RoundOf16 => ContinentalStage.QuarterFinal,
                ContinentalStage.QuarterFinal => ContinentalStage.SemiFinal,
                ContinentalStage.SemiFinal => ContinentalStage.Final,
                _ => ContinentalStage.Final
            };

            var nextStageFixtures = new List<ContinentalFixture>();
            if (nextStage == ContinentalStage.Final)
            {
                nextStageFixtures.Add(ContinentalFixture.Create(roundWinners[0], roundWinners[1], ContinentalStage.Final));
            }
            else
            {
                // Pair up winners
                for (int i = 0; i < roundWinners.Count; i += 2)
                {
                    Guid c1 = roundWinners[i];
                    Guid c2 = roundWinners[i + 1];
                    nextStageFixtures.Add(ContinentalFixture.Create(c1, c2, nextStage, 1));
                    nextStageFixtures.Add(ContinentalFixture.Create(c2, c1, nextStage, 2));
                }
            }

            var updatedAllFixtures = competition.KnockoutFixtures
                .Where(f => f.Stage != stage)
                .Concat(resolvedFixtures)
                .Concat(nextStageFixtures)
                .ToList();

            return competition with { KnockoutFixtures = updatedAllFixtures };
        }

        /// <summary>
        /// Distributes prize money pool to clubs based on their finishing positions.
        /// </summary>
        public static WorldState AwardPrizeMoney(
            ContinentalCompetition competition,
            WorldState world)
        {
            if (competition is null) throw new ArgumentNullException(nameof(competition));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (competition.WinnerId == null) return world;

            decimal totalPool = competition.PrizeMoney;
            var prizeAwards = new Dictionary<Guid, decimal>();

            // Final winner: 40%
            prizeAwards[competition.WinnerId.Value] = totalPool * 0.40m;

            // Final runner-up: 20%
            var final = competition.KnockoutFixtures.FirstOrDefault(f => f.Stage == ContinentalStage.Final && f.IsCompleted);
            if (final != null)
            {
                Guid runnerUp = final.HomeClubId == competition.WinnerId.Value ? final.AwayClubId : final.HomeClubId;
                prizeAwards[runnerUp] = totalPool * 0.20m;
            }

            // Semi-finalists (excluding finalists): 10% each
            var sfFixtures = competition.KnockoutFixtures.Where(f => f.Stage == ContinentalStage.SemiFinal).ToList();
            var sfClubs = sfFixtures.Select(f => f.HomeClubId).Distinct().Where(id => !prizeAwards.ContainsKey(id)).ToList();
            foreach (var clubId in sfClubs)
            {
                prizeAwards[clubId] = totalPool * 0.10m;
            }

            // Update club transfer budgets
            var updatedWorld = world;
            foreach (var kvp in prizeAwards)
            {
                if (updatedWorld.Clubs.TryGetValue(kvp.Key, out var club))
                {
                    var updatedFinances = club.Finances with
                    {
                        TransferBudget = club.Finances.TransferBudget + kvp.Value
                    };
                    updatedWorld = updatedWorld.WithClub(club.WithFinances(updatedFinances));
                }
            }

            return updatedWorld;
        }

        /// <summary>
        /// Simulates a complete continental competition tournament from qualification to the Final.
        /// </summary>
        public static (WorldState UpdatedWorld, ContinentalCompetition CompletedTournament) SimulateFullTournament(
            IReadOnlyList<Guid> qualifiedClubs,
            WorldState world,
            SimulationRandom rng,
            string competitionName = "Champions Cup")
        {
            if (qualifiedClubs is null) throw new ArgumentNullException(nameof(qualifiedClubs));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // 1. Create tournament
            var tournament = ContinentalCompetition.Create(competitionName, qualifiedClubs.Count);
            tournament = tournament with { QualifiedClubIds = qualifiedClubs };

            // 2. Draw groups
            var groups = DrawGroups(qualifiedClubs, world, rng);
            tournament = tournament with { GroupStandings = groups };

            // 3. Simulate group stage
            tournament = SimulateGroupStage(tournament, world, rng);

            // 4. Draw knockout bracket
            tournament = GenerateKnockoutBracket(tournament, world, rng);

            // 5. Simulate knockout stages
            tournament = SimulateKnockoutRound(tournament, ContinentalStage.RoundOf16, world, rng);
            tournament = SimulateKnockoutRound(tournament, ContinentalStage.QuarterFinal, world, rng);
            tournament = SimulateKnockoutRound(tournament, ContinentalStage.SemiFinal, world, rng);
            tournament = SimulateKnockoutRound(tournament, ContinentalStage.Final, world, rng);

            // 6. Award prize money
            var updatedWorld = AwardPrizeMoney(tournament, world);

            return (updatedWorld.WithActiveContinentalCompetition(tournament), tournament);
        }

        private static (int HomeScore, int AwayScore) SimulateMatchScore(
            Guid homeClubId,
            Guid awayClubId,
            WorldState world,
            SimulationRandom rng)
        {
            float homeRep = world.Clubs.TryGetValue(homeClubId, out var hc) ? hc.ReputationRating : 50f;
            float awayRep = world.Clubs.TryGetValue(awayClubId, out var ac) ? ac.ReputationRating : 50f;

            // Home advantage: +5 reputation points
            homeRep += 5f;

            float repRatio = homeRep / Math.Max(1f, awayRep);
            float homeExpected = Math.Clamp(1.3f * repRatio, 0.4f, 4.0f);
            float awayExpected = Math.Clamp(1.1f / repRatio, 0.3f, 3.5f);

            int homeScore = GenerateGoals(homeExpected, rng);
            int awayScore = GenerateGoals(awayExpected, rng);

            return (homeScore, awayScore);
        }

        private static int GenerateGoals(float expectedGoals, SimulationRandom rng)
        {
            int goals = 0;
            float p = MathF.Exp(-expectedGoals);
            float s = p;
            float u = rng.NextFloat(0f, 1f);

            while (u > s && goals < 8)
            {
                goals++;
                p *= expectedGoals / goals;
                s += p;
            }

            return goals;
        }
    }
}
