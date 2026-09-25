using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# system governing manager evaluation, sackings, resignations, appointments,
    /// and trust reset dynamics (#P5-006).
    /// Fully deterministic via <see cref="SimulationRandom"/>.
    /// </summary>
    public static class ManagerChangeSystem
    {
        public const float NeutralTrustValue = 50.0f;

        /// <summary>
        /// Evaluates a manager's seasonal performance against board expectations and patience.
        /// Returns the probability of the manager being sacked [0.0, 1.0].
        /// </summary>
        public static float EvaluateManagerPerformance(
            Club club,
            ClubSeasonOutcome outcome,
            WorldState world)
        {
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (outcome is null) throw new ArgumentNullException(nameof(outcome));
            if (world is null) throw new ArgumentNullException(nameof(world));

            // Champions and promoted teams almost never sack their manager
            if (outcome.WonTitle || outcome.Promoted)
            {
                return 0.0f;
            }

            // Continental qualification provides massive job security
            if (outcome.QualifiedForContinental)
            {
                return 0.02f;
            }

            // Calculate expected finish position based on club reputation within its league
            int expectedPosition = CalculateExpectedPosition(club, world);
            int positionDeficit = outcome.FinalPosition - expectedPosition;

            float baseSackProbability = 0.0f;

            if (outcome.Relegated)
            {
                baseSackProbability = 0.85f;
            }
            else if (positionDeficit > 0)
            {
                // Each place finished below expectations adds 10-15% sacking risk
                baseSackProbability = Math.Clamp(positionDeficit * 0.12f, 0.0f, 0.70f);
            }

            // Board patience modifier
            float patienceModifier = club.Board.Patience switch
            {
                BoardPatience.Impatient => 0.20f,
                BoardPatience.Balanced  => 0.0f,
                BoardPatience.Patient   => -0.20f,
                _                       => 0.0f
            };

            float finalProbability = Math.Clamp(baseSackProbability + patienceModifier, 0.0f, 0.98f);
            return finalProbability;
        }

        /// <summary>
        /// Generates a new manager suited for the club's tier and prestige.
        /// </summary>
        public static Manager GenerateNewManager(Club club, SimulationRandom rng)
        {
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            string nationality = RandomNameGenerator.SupportedNationalities[rng.NextInt(0, RandomNameGenerator.SupportedNationalities.Count)];
            var (firstName, lastName) = RandomNameGenerator.GenerateFullName(nationality, rng);
            string fullName = $"{firstName} {lastName}";

            int managerRep = Math.Clamp(club.ReputationRating + rng.NextInt(-6, 7), 1, 100);
            Formation formation = (Formation)rng.NextInt(0, 5);
            TacticalIdentity style = (TacticalIdentity)rng.NextInt(0, 4);

            float trustDecay = rng.NextFloat(0.03f, 0.08f);
            float tolerance = rng.NextFloat(0.30f, 0.50f);

            return Manager.Create(
                fullName,
                formation,
                style,
                trustDecay,
                tolerance,
                managerRep,
                club.Id);
        }

        /// <summary>
        /// Triggers a manager departure and new appointment at a club, resetting player manager trust
        /// to neutral (50.0) and logging the event in WorldState history.
        /// </summary>
        public static (WorldState UpdatedWorld, ManagerChangeEvent ChangeEvent) TriggerManagerChange(
            Club club,
            ManagerChangeReason reason,
            WorldState world,
            SimulationRandom rng,
            DateOnly date,
            Guid? playerFocusId = null)
        {
            if (club is null) throw new ArgumentNullException(nameof(club));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            Guid? oldManagerId = club.ManagerId;
            var newManager = GenerateNewManager(club, rng);

            // Record event
            var changeEvent = ManagerChangeEvent.Create(
                club.Id,
                oldManagerId,
                newManager.Id,
                reason,
                date,
                playerTrustReset: NeutralTrustValue);

            var updatedWorld = world;

            // Unassign old manager if present
            if (oldManagerId.HasValue && updatedWorld.Managers.TryGetValue(oldManagerId.Value, out var oldMgr))
            {
                updatedWorld = updatedWorld.WithManager(oldMgr.WithClub(null));
            }

            // Register new manager
            updatedWorld = updatedWorld.WithManager(newManager);

            // Update club manager pointer
            var updatedClub = club.WithManager(newManager.Id);
            updatedWorld = updatedWorld.WithClub(updatedClub);

            // Reset trust for focus player and/or club squad members
            var playersToReset = new HashSet<Guid>();
            if (playerFocusId.HasValue)
            {
                playersToReset.Add(playerFocusId.Value);
            }
            foreach (var pid in club.SquadPlayerIds)
            {
                playersToReset.Add(pid);
            }

            foreach (var pid in playersToReset)
            {
                if (updatedWorld.CareerStates.TryGetValue(pid, out var careerState))
                {
                    var updatedCareerState = careerState with { ManagerTrust = NeutralTrustValue };
                    updatedWorld = updatedWorld.WithPlayerCareerState(pid, updatedCareerState);
                }
            }

            // Append event to WorldState
            updatedWorld = updatedWorld.WithManagerChangeEvent(changeEvent);

            return (updatedWorld, changeEvent);
        }

        /// <summary>
        /// Resets a specific player's manager trust score to neutral or specified value.
        /// </summary>
        public static WorldState ApplyTrustReset(
            Guid playerId,
            WorldState world,
            float resetTrust = NeutralTrustValue)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));

            if (world.CareerStates.TryGetValue(playerId, out var careerState))
            {
                var updated = careerState with { ManagerTrust = Math.Clamp(resetTrust, 0f, 100f) };
                return world.WithPlayerCareerState(playerId, updated);
            }

            return world;
        }

        private static int CalculateExpectedPosition(Club club, WorldState world)
        {
            // Rank clubs in the same league by reputation
            var leagueClubs = world.Clubs.Values
                .Where(c => c.LeagueId == club.LeagueId)
                .OrderByDescending(c => c.ReputationRating)
                .ToList();

            int index = leagueClubs.FindIndex(c => c.Id == club.Id);
            return index >= 0 ? index + 1 : 10;
        }
    }
}
