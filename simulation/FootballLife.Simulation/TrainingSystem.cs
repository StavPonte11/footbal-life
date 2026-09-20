using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure stateless simulation system for training sessions.
    /// Computes XP gained per attribute, fatigue cost, and injury risk from a single
    /// <see cref="TrainingSession"/> — or applies weekly diminishing returns over a full week.
    /// </summary>
    /// <remarks>
    /// All methods are deterministic given identical inputs and <see cref="SimulationRandom"/> seed.
    /// No LINQ or heap allocations in hot paths — pre-allocated dictionaries only.
    /// </remarks>
    public static class TrainingSystem
    {
        // ─── BaseXP per training type (per attribute, per 60 min) ─────────────
        private const float BaseXpTechnical       = 3.0f;
        private const float BaseXpPhysical        = 3.0f;
        private const float BaseXpMental          = 2.0f;
        private const float BaseXpPositionSpecific = 4.0f;
        private const float BaseXpGym             = 2.5f;
        // Recovery = 0 XP always

        // ─── BaseFatigueCost per training type (per 60 min, Moderate intensity) ─
        private const float BaseFatigueTechnical       = 8f;
        private const float BaseFatiguePhysical        = 12f;
        private const float BaseFatigueMental          = 4f;
        private const float BaseFatiguePositionSpecific = 10f;
        private const float BaseFatigueGym             = 10f;
        private const float RecoveryFatigueCost        = -15f;

        // ─── StateModifier clamp ──────────────────────────────────────────────
        private const float MinStateModifier = 0.3f;

        // ─── Injury risk parameters ───────────────────────────────────────────
        private const float InjuryRiskFatigueThreshold  = 40f;
        private const float InjuryRiskMaxFatigue        = 100f;
        private const float InjuryRiskMaxBase           = 0.08f; // 8% at fatigue=100, normal intensity
        private const float MaximumIntensityRiskMultiplier = 2f;

        // ─── rng_variance clamp ───────────────────────────────────────────────
        private const float RngVarianceMin = 0.7f;
        private const float RngVarianceMax = 1.3f;

        // ─── Diminishing returns ──────────────────────────────────────────────
        private const float DiminishingReturnsFactor = 0.70f;   // 30% reduction per repeat
        private const int   MaxSessionsPerWeek       = 3;

        // ─── Attribute sets per training type ─────────────────────────────────
        // These are static readonly arrays — no allocation on each call.
        private static readonly AttributeName[] TechnicalAttributes = {
            AttributeName.Passing, AttributeName.Shooting, AttributeName.Dribbling,
            AttributeName.Crossing, AttributeName.FirstTouch
        };

        private static readonly AttributeName[] PhysicalAttributes = {
            AttributeName.Pace, AttributeName.Acceleration, AttributeName.Stamina,
            AttributeName.Strength, AttributeName.Agility
        };

        private static readonly AttributeName[] MentalAttributes = {
            AttributeName.Vision, AttributeName.Composure, AttributeName.Positioning,
            AttributeName.DecisionMaking
        };

        private static readonly AttributeName[] GymAttributes = {
            AttributeName.Strength, AttributeName.Stamina, AttributeName.Pace,
            AttributeName.Acceleration
        };

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the <see cref="TrainingResult"/> for a single training session.
        /// </summary>
        /// <param name="session">The training session descriptor.</param>
        /// <param name="position">Player's primary position — determines PositionRelevance weights.</param>
        /// <param name="abilities">Current player abilities — unused in XP calculation but required for future extensions.</param>
        /// <param name="state">Current player state — Motivation drives StateModifier; Fatigue drives injury risk.</param>
        /// <param name="rng">Seeded RNG for Gaussian variance. Advance the same rng instance across the session.</param>
        public static TrainingResult CalculateXP(
            TrainingSession session,
            Position position,
            PlayerAbilities abilities,
            PlayerState state,
            SimulationRandom rng)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Recovery is a special case — no XP, fixed fatigue restore
            if (session.Type == TrainingType.Recovery)
                return TrainingResult.Recovery(RecoveryFatigueCost);

            float intensityMultiplier = GetIntensityMultiplier(session.Intensity);
            float stateModifier = MathF.Max(MinStateModifier, state.Motivation / 100f);
            float durationScale = session.DurationMinutes / 60f;

            // XP per attribute
            var xpGained = new Dictionary<AttributeName, float>(15);
            AttributeName[] targetAttributes = GetTargetAttributes(session.Type, position);
            float baseXp = GetBaseXP(session.Type);

            foreach (var attr in targetAttributes)
            {
                float positionRelevance = PositionWeightMap.GetWeight(position, attr);
                float rngVariance = Clamp(rng.NextGaussian(1.0f, 0.1f), RngVarianceMin, RngVarianceMax);
                float xp = baseXp * intensityMultiplier * positionRelevance * stateModifier * rngVariance * durationScale;
                if (xp > 0f)
                    xpGained[attr] = xp;
            }

            // Fatigue cost
            float baseFatigue = GetBaseFatigueCost(session.Type);
            float fatigueCost = baseFatigue * intensityMultiplier * durationScale;

            // Injury risk
            float injuryRisk = CalculateInjuryRisk(state.Fatigue, session.Intensity);

            return TrainingResult.Create(xpGained, fatigueCost, injuryRisk);
        }

        /// <summary>
        /// Calculates XP for a full week of sessions with diminishing returns.
        /// Sessions processed in order; repeated <see cref="TrainingType"/> within the week reduce XP by 30% per repeat.
        /// Maximum <see cref="MaxSessionsPerWeek"/> sessions per week.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if more than 3 sessions are provided.</exception>
        public static IReadOnlyList<TrainingResult> CalculateWeekXP(
            IReadOnlyList<TrainingSession> weekSessions,
            Position position,
            PlayerAbilities abilities,
            PlayerState state,
            SimulationRandom rng)
        {
            if (weekSessions is null) throw new ArgumentNullException(nameof(weekSessions));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            if (weekSessions.Count > MaxSessionsPerWeek)
            {
                throw new InvalidOperationException(
                    $"Maximum {MaxSessionsPerWeek} training sessions allowed per week. " +
                    $"Got: {weekSessions.Count}. The 4th session would cause overtraining.");
            }

            // Track repetition count per type to compute DR multiplier
            var repeatCounts = new Dictionary<TrainingType, int>(6);
            var results = new TrainingResult[weekSessions.Count];

            for (int i = 0; i < weekSessions.Count; i++)
            {
                var session = weekSessions[i];

                // Recovery sessions are exempt from DR — process at full value
                if (session.Type == TrainingType.Recovery)
                {
                    results[i] = TrainingResult.Recovery(RecoveryFatigueCost);
                    continue;
                }

                // Base result for this session
                var baseResult = CalculateXP(session, position, abilities, state, rng);

                // Compute DR multiplier based on how many times this type was already done this week
                int previousReps = repeatCounts.TryGetValue(session.Type, out var count) ? count : 0;
                float drMultiplier = MathF.Pow(DiminishingReturnsFactor, previousReps);

                // Increment rep count
                repeatCounts[session.Type] = previousReps + 1;

                // Apply DR to all XP values
                if (drMultiplier >= 0.9999f)
                {
                    // First session of this type — no reduction
                    results[i] = baseResult;
                }
                else
                {
                    var scaledXp = new Dictionary<AttributeName, float>(baseResult.XpGained.Count);
                    foreach (var kvp in baseResult.XpGained)
                        scaledXp[kvp.Key] = kvp.Value * drMultiplier;

                    results[i] = TrainingResult.Create(scaledXp, baseResult.FatigueCost, baseResult.InjuryRisk);
                }
            }

            return Array.AsReadOnly(results);
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private static float GetIntensityMultiplier(TrainingIntensity intensity) => intensity switch
        {
            TrainingIntensity.Light    => 0.5f,
            TrainingIntensity.Moderate => 1.0f,
            TrainingIntensity.Hard     => 1.5f,
            TrainingIntensity.Maximum  => 2.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(intensity))
        };

        private static float GetBaseXP(TrainingType type) => type switch
        {
            TrainingType.Technical        => BaseXpTechnical,
            TrainingType.Physical         => BaseXpPhysical,
            TrainingType.Mental           => BaseXpMental,
            TrainingType.PositionSpecific => BaseXpPositionSpecific,
            TrainingType.Gym              => BaseXpGym,
            TrainingType.Recovery         => 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        private static float GetBaseFatigueCost(TrainingType type) => type switch
        {
            TrainingType.Technical        => BaseFatigueTechnical,
            TrainingType.Physical         => BaseFatiguePhysical,
            TrainingType.Mental           => BaseFatigueMental,
            TrainingType.PositionSpecific => BaseFatiguePositionSpecific,
            TrainingType.Gym              => BaseFatigueGym,
            TrainingType.Recovery         => 0f, // Recovery is handled separately
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        /// <summary>
        /// Returns the set of attributes targeted by a given training type.
        /// For <see cref="TrainingType.PositionSpecific"/>, returns the 3 highest-weighted attributes
        /// for the given position from <see cref="PositionWeightMap"/>.
        /// </summary>
        private static AttributeName[] GetTargetAttributes(TrainingType type, Position position)
        {
            return type switch
            {
                TrainingType.Technical        => TechnicalAttributes,
                TrainingType.Physical         => PhysicalAttributes,
                TrainingType.Mental           => MentalAttributes,
                TrainingType.Gym              => GymAttributes,
                TrainingType.PositionSpecific => GetTopThreeAttributesForPosition(position),
                TrainingType.Recovery         => Array.Empty<AttributeName>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        /// <summary>
        /// Returns the 3 highest-weighted attributes for a position, sorted descending by weight.
        /// Uses a simple in-place sort on a stack-friendly 3-slot structure — no LINQ.
        /// </summary>
        private static AttributeName[] GetTopThreeAttributesForPosition(Position position)
        {
            var weights = PositionWeightMap.For(position);

            // Find top-3 by iterating through all entries — O(n) selection, no allocation
            AttributeName top1 = default, top2 = default, top3 = default;
            float w1 = -1f, w2 = -1f, w3 = -1f;

            foreach (var kvp in weights)
            {
                float w = kvp.Value;
                if (w > w1) { top3 = top2; w3 = w2; top2 = top1; w2 = w1; top1 = kvp.Key; w1 = w; }
                else if (w > w2) { top3 = top2; w3 = w2; top2 = kvp.Key; w2 = w; }
                else if (w > w3) { top3 = kvp.Key; w3 = w; }
            }

            return new[] { top1, top2, top3 };
        }

        private static float CalculateInjuryRisk(float currentFatigue, TrainingIntensity intensity)
        {
            if (currentFatigue < InjuryRiskFatigueThreshold) return 0f;

            // Linear interpolation: 0% at fatigue=40, 8% at fatigue=100
            float t = (currentFatigue - InjuryRiskFatigueThreshold) /
                      (InjuryRiskMaxFatigue - InjuryRiskFatigueThreshold);
            float baseRisk = t * InjuryRiskMaxBase;

            if (intensity == TrainingIntensity.Maximum)
                baseRisk *= MaximumIntensityRiskMultiplier;

            return Clamp(baseRisk, 0f, 1f);
        }

        private static float Clamp(float value, float min, float max)
            => value < min ? min : value > max ? max : value;
    }
}
