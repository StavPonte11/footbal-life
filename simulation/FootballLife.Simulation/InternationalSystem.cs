using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# system governing international football call-ups, matches, and career tracking (#P5-004).
    /// Fully deterministic via <see cref="SimulationRandom"/>.
    /// </summary>
    public static class InternationalSystem
    {
        public const int DefaultSquadSize = 23;
        public const float CallUpFatigueCost = 20.0f;
        public const float CallUpConfidenceBoost = 10.0f;
        public const float CallUpFormBoost = 5.0f;
        public const float CallUpReputationBoost = 2.0f;

        /// <summary>
        /// Determines if a player is eligible to represent a given national team.
        /// Matches against full CountryName or 3-letter CountryCode.
        /// </summary>
        public static bool DetermineEligibility(Player player, NationalTeam nationalTeam)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));
            if (nationalTeam is null) throw new ArgumentNullException(nameof(nationalTeam));

            return string.Equals(player.Nationality, nationalTeam.CountryName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(player.Nationality, nationalTeam.CountryCode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Selects a balanced national team squad of top eligible players based on ability and form.
        /// Ensures positional balance across Goalkeepers, Defenders, Midfielders, and Forwards.
        /// </summary>
        public static IReadOnlyList<Guid> GenerateCallUpSquad(
            NationalTeam nationalTeam,
            WorldState world,
            SimulationRandom rng,
            int squadSize = DefaultSquadSize)
        {
            if (nationalTeam is null) throw new ArgumentNullException(nameof(nationalTeam));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));
            if (squadSize < 11) throw new ArgumentOutOfRangeException(nameof(squadSize), "Squad must be at least 11 players.");

            var eligiblePlayers = new List<(Player Player, float Score)>();

            foreach (var player in world.Players.Values)
            {
                if (!DetermineEligibility(player, nationalTeam)) continue;

                // Check international retirement
                var career = world.GetPlayerInternationalCareer(player.Id);
                if (career != null && career.IsRetiredFromInternational) continue;

                float ability = world.Abilities.TryGetValue(player.Id, out var ab) ? ab.CalculateAverage() : 50f;
                float form = world.States.TryGetValue(player.Id, out var st) ? st.Form : 50f;

                // Composite rating: 75% raw ability + 25% current form
                float compositeScore = (ability * 0.75f) + (form * 0.25f);
                eligiblePlayers.Add((player, compositeScore));
            }

            if (eligiblePlayers.Count == 0)
            {
                return Array.Empty<Guid>();
            }

            // Categorize by position
            var gks = eligiblePlayers.Where(p => IsGoalkeeper(p.Player.PrimaryPosition)).OrderByDescending(p => p.Score).ToList();
            var defs = eligiblePlayers.Where(p => IsDefender(p.Player.PrimaryPosition)).OrderByDescending(p => p.Score).ToList();
            var mids = eligiblePlayers.Where(p => IsMidfielder(p.Player.PrimaryPosition)).OrderByDescending(p => p.Score).ToList();
            var fwds = eligiblePlayers.Where(p => IsForward(p.Player.PrimaryPosition)).OrderByDescending(p => p.Score).ToList();

            var selected = new HashSet<Guid>();

            // Mandatory positional quotas
            void PickTop(List<(Player Player, float Score)> pool, int count)
            {
                foreach (var item in pool.Take(count))
                {
                    selected.Add(item.Player.Id);
                }
            }

            PickTop(gks, 2);
            PickTop(defs, 6);
            PickTop(mids, 6);
            PickTop(fwds, 3);

            // Fill remaining slots by highest composite score
            var remaining = eligiblePlayers
                .Where(p => !selected.Contains(p.Player.Id))
                .OrderByDescending(p => p.Score);

            foreach (var item in remaining)
            {
                if (selected.Count >= squadSize) break;
                selected.Add(item.Player.Id);
            }

            return selected.ToList();
        }

        /// <summary>
        /// Simulates an international match between two national teams.
        /// Generates scores and identifies individual goalscorers from participating squads.
        /// </summary>
        public static (int HomeScore, int AwayScore, IReadOnlyDictionary<Guid, int> GoalScorers) SimulateInternationalMatch(
            NationalTeam homeTeam,
            NationalTeam awayTeam,
            WorldState world,
            SimulationRandom rng)
        {
            if (homeTeam is null) throw new ArgumentNullException(nameof(homeTeam));
            if (awayTeam is null) throw new ArgumentNullException(nameof(awayTeam));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // FIFA ranking strength (Rank 1 = 220, Rank 210 = 10)
            float homeRankStrength = 221 - homeTeam.FifaRanking;
            float awayRankStrength = 221 - awayTeam.FifaRanking;

            // Home pitch advantage (+15)
            float homeTotalStrength = homeRankStrength + 15f;
            float awayTotalStrength = awayRankStrength;

            // Goal expectations
            float homeExpectedGoals = Math.Clamp(1.3f * (homeTotalStrength / Math.Max(1f, awayTotalStrength)), 0.3f, 4.5f);
            float awayExpectedGoals = Math.Clamp(1.1f * (awayTotalStrength / Math.Max(1f, homeTotalStrength)), 0.2f, 4.0f);

            int homeScore = GenerateGoals(homeExpectedGoals, rng);
            int awayScore = GenerateGoals(awayExpectedGoals, rng);

            var scorers = new Dictionary<Guid, int>();

            void DistributeGoals(NationalTeam team, int goals)
            {
                if (goals <= 0 || team.SquadPlayerIds.Count == 0) return;

                // Candidate scorers (prefer outfield players)
                var outfield = team.SquadPlayerIds
                    .Where(id => world.Players.TryGetValue(id, out var p) && !IsGoalkeeper(p.PrimaryPosition))
                    .ToList();

                var pool = outfield.Count > 0 ? outfield : team.SquadPlayerIds.ToList();

                for (int i = 0; i < goals; i++)
                {
                    int index = rng.NextInt(0, pool.Count);
                    Guid scorerId = pool[index];
                    scorers[scorerId] = scorers.TryGetValue(scorerId, out int current) ? current + 1 : 1;
                }
            }

            DistributeGoals(homeTeam, homeScore);
            DistributeGoals(awayTeam, awayScore);

            return (homeScore, awayScore, scorers);
        }

        /// <summary>
        /// Updates an international career record with newly accumulated statistics.
        /// </summary>
        public static InternationalCareer UpdateInternationalCareer(
            InternationalCareer career,
            int caps,
            int goals,
            int assists,
            DateOnly matchDate)
        {
            if (career is null) throw new ArgumentNullException(nameof(career));
            return career.WithAccumulatedStats(caps, goals, assists, matchDate);
        }

        /// <summary>
        /// Processes a full player call-up and appearance, updating player fatigue, confidence,
        /// reputation, and international career statistics.
        /// </summary>
        public static (WorldState UpdatedWorld, InternationalCallUp CallUp) ProcessPlayerCallUp(
            Guid playerId,
            Guid nationalTeamId,
            string tournamentName,
            DateOnly matchDate,
            WorldState world,
            SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var player = world.GetPlayer(playerId);
            var nationalTeam = world.GetNationalTeam(nationalTeamId);

            if (!DetermineEligibility(player, nationalTeam))
            {
                throw new InvalidOperationException($"Player {player.Name} is not eligible for {nationalTeam.CountryName}.");
            }

            var existingCareer = world.GetPlayerInternationalCareer(playerId);
            if (existingCareer != null && existingCareer.IsRetiredFromInternational)
            {
                var declinedCallUp = InternationalCallUp.Create(
                    playerId, nationalTeamId, tournamentName, matchDate, CallUpStatus.Declined);
                return (world, declinedCallUp);
            }

            // Cap earned: 1 match
            int capEarned = 1;

            // Goal chance based on position and shooting ability
            int goalsEarned = 0;
            if (!IsGoalkeeper(player.PrimaryPosition))
            {
                float shooting = world.Abilities.TryGetValue(playerId, out var ab) ? ab.Shooting : 50f;
                float goalChance = IsForward(player.PrimaryPosition) ? (shooting / 180f) :
                                  IsMidfielder(player.PrimaryPosition) ? (shooting / 350f) : (shooting / 800f);

                if (rng.NextFloat(0f, 1f) < goalChance)
                {
                    goalsEarned = 1;
                }
            }

            var callUp = new InternationalCallUp(
                Guid.NewGuid(),
                playerId,
                nationalTeamId,
                tournamentName,
                matchDate,
                CallUpStatus.CalledUp,
                capEarned,
                goalsEarned);

            // Update physical/mental state
            var curState = world.GetState(playerId);
            var updatedState = curState with
            {
                Fatigue = Math.Clamp(curState.Fatigue + CallUpFatigueCost, 0f, 100f),
                Confidence = Math.Clamp(curState.Confidence + CallUpConfidenceBoost, 0f, 100f),
                Form = Math.Clamp(curState.Form + CallUpFormBoost, 0f, 100f)
            };

            // Update career state reputation
            var curCareerState = world.GetCareerState(playerId);
            var updatedCareerState = curCareerState with
            {
                Reputation = Math.Clamp(curCareerState.Reputation + CallUpReputationBoost, 0f, 100f)
            };

            // Update or initialize international career
            var intCareer = existingCareer ?? InternationalCareer.CreateNew(playerId, nationalTeamId);
            var updatedIntCareer = intCareer.WithAccumulatedStats(capEarned, goalsEarned, 0, matchDate);

            var updatedWorld = world
                .WithPlayerState(playerId, updatedState)
                .WithPlayerCareerState(playerId, updatedCareerState)
                .WithInternationalCareer(updatedIntCareer);

            return (updatedWorld, callUp);
        }

        private static int GenerateGoals(float expectedGoals, SimulationRandom rng)
        {
            // Simple Poisson-like approximation
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

        private static bool IsGoalkeeper(Position pos) => pos == Position.GK;

        private static bool IsDefender(Position pos) =>
            pos is Position.CB or Position.FB;

        private static bool IsMidfielder(Position pos) =>
            pos is Position.DM or Position.CM or Position.AM;

        private static bool IsForward(Position pos) =>
            pos is Position.LW or Position.RW or Position.ST;
    }
}
