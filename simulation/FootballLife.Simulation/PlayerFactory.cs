using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Constructs a complete, internally-consistent <see cref="WorldState"/> for a new player career.
    /// All ability values are generated deterministically from the provided <see cref="SimulationRandom"/> seed
    /// so that replaying career creation with the same seed produces an identical result.
    /// </summary>
    public static class PlayerFactory
    {
        // Salary range for starting academy/youth players (£/week)
        private const decimal MinStartingSalary = 150m;
        private const decimal MaxStartingSalary = 500m;

        // Market value range at career start
        private const decimal MinStartingMarketValue = 20_000m;
        private const decimal MaxStartingMarketValue = 100_000m;

        // Gaussian spread around StartingAbilityBase for each attribute (stddev in ability points)
        private const float AbilityGenerationStddev = 5f;

        // Potential generation: raw ceiling = StartingAbilityBase + rng.NextInt(PotentialMinBonus, PotentialMaxBonus)
        private const int PotentialMinBonus = 10;
        private const int PotentialMaxBonus = 35;

        /// <summary>
        /// Creates a new career and returns an updated <see cref="WorldState"/> that contains the player
        /// in all required facets: Players, Abilities, States, CareerStates, Potentials, and Contracts.
        /// The club's <see cref="Club.SquadPlayerIds"/> is also updated.
        /// </summary>
        /// <param name="world">
        /// The world snapshot to extend. Must contain a club with <see cref="PlayerCreationArgs.StartingClubId"/>.
        /// </param>
        /// <param name="args">Career creation parameters.</param>
        /// <param name="rng">Deterministic RNG — use the same seed to reproduce the same player.</param>
        /// <returns>A new <see cref="WorldState"/> with the player fully registered.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when <see cref="PlayerCreationArgs.StartingClubId"/> is not found in <paramref name="world"/>.
        /// </exception>
        public static WorldState CreateCareer(WorldState world, PlayerCreationArgs args, SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (args is null) throw new ArgumentNullException(nameof(args));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Validate the starting club exists
            var startingClub = world.GetClub(args.StartingClubId);

            // 1. Create Player identity
            var player = new Player(
                Guid.NewGuid(),
                args.Name,
                args.Nationality,
                args.DateOfBirth,
                args.PreferredFoot,
                args.PrimaryPosition);

            // 2. Generate position-weighted abilities
            var abilities = GenerateAbilities(args, rng);

            // 3. Create initial PlayerState (fresh career — low fatigue, neutral form)
            var state = PlayerState.Default;

            // 4. Generate starting salary (randomized within youth wage band)
            decimal salary = MinStartingSalary + (decimal)(rng.NextFloat(0f, 1f) * (float)(MaxStartingSalary - MinStartingSalary));
            salary = Math.Round(salary, 0);

            // 5. Create PlayerCareerState starting as Academy player
            var careerState = PlayerCareerState.CreateAcademy(args.StartingClubId, salary);

            // 6. Generate PlayerPotential
            var potential = GeneratePotential(args, rng);

            // 7. Create a 1-year academy contract starting from the season start date
            var contractStart = world.CurrentSeason.StartDate;
            var contractEnd = contractStart.AddYears(1);
            var contract = Contract.Create(
                player.Id,
                args.StartingClubId,
                salary,
                contractStart,
                contractEnd,
                SquadRole.Academy);

            // 8. Create initial FinanceAccount
            var account = FinanceAccount.Create(0m);

            // 9. Create starter relationships (Family, Agent, Coach)
            var starterRelationships = new[]
            {
                Relationship.Create(player.Id, "Parents", RelationshipType.Parent, initialAffinity: 85f, initialTrust: 90f, initialDate: world.CurrentSeason.StartDate),
                Relationship.Create(player.Id, "Agent", RelationshipType.Agent, initialAffinity: 60f, initialTrust: 65f, initialDate: world.CurrentSeason.StartDate),
                Relationship.Create(player.Id, "Academy Coach", RelationshipType.Manager, initialAffinity: 70f, initialTrust: 70f, initialDate: world.CurrentSeason.StartDate)
            };

            // 10. Update the club's squad roster
            var updatedClub = startingClub.WithAddedPlayer(player.Id);

            // 11. Assemble the new WorldState — all operations are immutable
            return world
                .WithPlayer(player, abilities, state, careerState, potential, account, starterRelationships)
                .WithContract(contract)
                .WithClub(updatedClub);
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private static PlayerAbilities GenerateAbilities(PlayerCreationArgs args, SimulationRandom rng)
        {
            var weights = PositionWeightMap.For(args.PrimaryPosition);
            float baseValue = args.StartingAbilityBase;

            // Each attribute: base ± Gaussian(0, stddev) scaled by position weight
            // Higher-weighted attributes trend closer to base; low-weight attributes can be further below
            int pace         = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Pace));
            int acceleration = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Acceleration));
            int stamina      = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Stamina));
            int strength     = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Strength));
            int agility      = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Agility));
            int passing      = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Passing));
            int shooting     = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Shooting));
            int dribbling    = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Dribbling));
            int crossing     = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Crossing));
            int firstTouch   = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.FirstTouch));
            int tackling     = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Tackling));
            int vision       = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Vision));
            int composure    = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Composure));
            int positioning  = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.Positioning));
            int decision     = ClampAbility(baseValue + rng.NextGaussian(0f, AbilityGenerationStddev) * ScaleWeight(weights, AttributeName.DecisionMaking));

            return new PlayerAbilities(
                pace, acceleration, stamina, strength, agility,
                passing, shooting, dribbling, crossing, firstTouch, tackling,
                vision, composure, positioning, decision);
        }

        /// <summary>
        /// Returns the weight for a given attribute, clamping low weights to 0.5 so that
        /// even irrelevant attributes don't deviate too wildly from the base.
        /// </summary>
        private static float ScaleWeight(
            System.Collections.ObjectModel.ReadOnlyDictionary<AttributeName, float> weights,
            AttributeName attribute)
        {
            float w = weights.TryGetValue(attribute, out var v) ? v : 0.5f;
            // Clamp minimum to 0.5 so every attribute stays recognizably in the ballpark of the base
            return Math.Max(0.5f, w);
        }

        private static int ClampAbility(float raw)
            => Math.Max(1, Math.Min(100, (int)Math.Round(raw)));

        private static PlayerPotential GeneratePotential(PlayerCreationArgs args, SimulationRandom rng)
        {
            int potentialBonus = rng.NextInt(PotentialMinBonus, PotentialMaxBonus + 1);
            int rawPotential = args.StartingAbilityBase + potentialBonus;

            // Assign a PotentialRange based on how high the generated ceiling is
            PotentialRange range = rawPotential switch
            {
                >= 90 => PotentialRange.Elite,
                >= 80 => PotentialRange.High,
                >= 70 => PotentialRange.Medium,
                _     => PotentialRange.Low
            };

            return PlayerPotential.CreateClamped(rawPotential, range);
        }
    }
}
