using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Stateless simulation system that converts accumulated training and match XP into permanent ability improvements
    /// governed by age-gated development curves, physical decline thresholds, and potential ceilings.
    /// </summary>
    public static class ProgressionSystem
    {
        private const float XpConversionDivisor = 100f; // 100 XP baseline for 1.0 ability point at 1.0 devRate
        private const float PhysicalDeclinePerYear = 0.5f;

        /// <summary>
        /// Applies accumulated XP gains for a player using a default balanced XP allocation.
        /// </summary>
        public static PlayerAbilities ApplyXpGains(
            Guid playerId,
            WorldState world,
            DateOnly currentDate,
            SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Default allocation: 100 XP across primary position attributes
            var player = world.GetPlayer(playerId);
            var weights = PositionWeightMap.For(player.PrimaryPosition);

            var defaultXp = new Dictionary<AttributeName, float>(15);
            foreach (var kvp in weights)
            {
                defaultXp[kvp.Key] = kvp.Value * 100f;
            }

            return ApplyXpGains(playerId, world, defaultXp, currentDate, rng);
        }

        /// <summary>
        /// Converts pending XP into permanent ability improvements, applying age gating, potential ceilings,
        /// and physical decline.
        /// </summary>
        public static PlayerAbilities ApplyXpGains(
            Guid playerId,
            WorldState world,
            IReadOnlyDictionary<AttributeName, float> pendingXp,
            DateOnly currentDate,
            SimulationRandom rng)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (pendingXp is null) throw new ArgumentNullException(nameof(pendingXp));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            var player = world.GetPlayer(playerId);
            var currentAbilities = world.GetAbilities(playerId);
            var potential = world.GetPotential(playerId);

            int age = CalculateAge(player.DateOfBirth, currentDate);
            float developmentRate = GetDevelopmentRate(age);

            int ceiling = potential.PotentialRating;
            if (potential.Range == PotentialRange.Elite)
            {
                int eliteVariance = rng.NextInt(-5, 6);
                ceiling = Math.Min(99, Math.Max(50, ceiling + eliteVariance));
            }

            // Calculate new attributes
            float pace = ComputeNewAttribute(currentAbilities.Pace, AttributeName.Pace, pendingXp, developmentRate, ceiling, age);
            float acceleration = ComputeNewAttribute(currentAbilities.Acceleration, AttributeName.Acceleration, pendingXp, developmentRate, ceiling, age);
            float stamina = ComputeNewAttribute(currentAbilities.Stamina, AttributeName.Stamina, pendingXp, developmentRate, ceiling, age);
            float strength = ComputeNewAttribute(currentAbilities.Strength, AttributeName.Strength, pendingXp, developmentRate, ceiling, age);
            float agility = ComputeNewAttribute(currentAbilities.Agility, AttributeName.Agility, pendingXp, developmentRate, ceiling, age);

            float passing = ComputeNewAttribute(currentAbilities.Passing, AttributeName.Passing, pendingXp, developmentRate, ceiling, age);
            float shooting = ComputeNewAttribute(currentAbilities.Shooting, AttributeName.Shooting, pendingXp, developmentRate, ceiling, age);
            float dribbling = ComputeNewAttribute(currentAbilities.Dribbling, AttributeName.Dribbling, pendingXp, developmentRate, ceiling, age);
            float crossing = ComputeNewAttribute(currentAbilities.Crossing, AttributeName.Crossing, pendingXp, developmentRate, ceiling, age);
            float firstTouch = ComputeNewAttribute(currentAbilities.FirstTouch, AttributeName.FirstTouch, pendingXp, developmentRate, ceiling, age);
            float tackling = ComputeNewAttribute(currentAbilities.Tackling, AttributeName.Tackling, pendingXp, developmentRate, ceiling, age);

            float vision = ComputeNewAttribute(currentAbilities.Vision, AttributeName.Vision, pendingXp, developmentRate, ceiling, age);
            float composure = ComputeNewAttribute(currentAbilities.Composure, AttributeName.Composure, pendingXp, developmentRate, ceiling, age);
            float positioning = ComputeNewAttribute(currentAbilities.Positioning, AttributeName.Positioning, pendingXp, developmentRate, ceiling, age);
            float decisionMaking = ComputeNewAttribute(currentAbilities.DecisionMaking, AttributeName.DecisionMaking, pendingXp, developmentRate, ceiling, age);

            return new PlayerAbilities(
                pace: (int)pace,
                acceleration: (int)acceleration,
                stamina: (int)stamina,
                strength: (int)strength,
                agility: (int)agility,
                passing: (int)passing,
                shooting: (int)shooting,
                dribbling: (int)dribbling,
                crossing: (int)crossing,
                firstTouch: (int)firstTouch,
                tackling: (int)tackling,
                vision: (int)vision,
                composure: (int)composure,
                positioning: (int)positioning,
                decisionMaking: (int)decisionMaking);
        }

        public static float GetDevelopmentRate(int age) => age switch
        {
            <= 20 => 1.4f,
            <= 24 => 1.1f,
            <= 28 => 0.8f,
            <= 31 => 0.4f,
            _ => 0.1f
        };

        public static int CalculateAge(DateOnly dateOfBirth, DateOnly currentDate)
        {
            int age = currentDate.Year - dateOfBirth.Year;
            if (currentDate < dateOfBirth.AddYears(age))
            {
                age--;
            }
            return age;
        }

        private static float ComputeNewAttribute(
            byte currentVal,
            AttributeName attribute,
            IReadOnlyDictionary<AttributeName, float> pendingXp,
            float developmentRate,
            int ceiling,
            int age)
        {
            float xp = pendingXp.TryGetValue(attribute, out var v) ? v : 0f;
            float rawGain = (xp / XpConversionDivisor) * developmentRate;

            // Cap at potential ceiling
            float newVal = currentVal;
            if (currentVal < ceiling)
            {
                newVal = Math.Min((float)ceiling, currentVal + rawGain);
            }

            // Physical decline at age 31+ for Pace, Acceleration, Stamina (-0.5/year)
            if (age >= 31 && (attribute == AttributeName.Pace || attribute == AttributeName.Acceleration || attribute == AttributeName.Stamina))
            {
                newVal = Math.Max(0f, newVal - PhysicalDeclinePerYear);
            }

            return Math.Min(99f, Math.Max(0f, newVal));
        }
    }
}
