using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FootballLife.Domain
{
    /// <summary>
    /// The output of a single training session.
    /// Communicates XP gained per attribute, fatigue cost, and injury risk to downstream systems.
    /// This is the explicit protocol between <c>TrainingSystem</c> and <c>FatigueSystem</c>.
    /// </summary>
    public sealed record TrainingResult
    {
        /// <summary>
        /// XP earned per attribute from this session.
        /// Only attributes targeted by the session type will have non-zero values.
        /// For <see cref="TrainingType.Recovery"/>, this dictionary is empty (all zero).
        /// </summary>
        public IReadOnlyDictionary<AttributeName, float> XpGained { get; init; }

        /// <summary>
        /// Net fatigue change from this session.
        /// Positive = more tired, negative = recovery (fatigue reduced).
        /// </summary>
        public float FatigueCost { get; init; }

        /// <summary>
        /// Probability [0.0, 1.0] that the player sustains a training injury during this session.
        /// 0 = safe, 1 = certain injury.
        /// </summary>
        public float InjuryRisk { get; init; }

        /// <summary>
        /// The sum of all attribute XP values gained in this session.
        /// </summary>
        public float TotalXpGained
        {
            get
            {
                float total = 0f;
                foreach (var xp in XpGained.Values)
                    total += xp;
                return total;
            }
        }

        public TrainingResult(
            IReadOnlyDictionary<AttributeName, float> xpGained,
            float fatigueCost,
            float injuryRisk)
        {
            if (xpGained is null) throw new ArgumentNullException(nameof(xpGained));
            if (injuryRisk < 0f || injuryRisk > 1f)
                throw new ArgumentOutOfRangeException(nameof(injuryRisk),
                    $"InjuryRisk must be in [0.0, 1.0]. Actual: {injuryRisk}");

            XpGained = xpGained;
            FatigueCost = fatigueCost;
            InjuryRisk = injuryRisk;
        }

        /// <summary>
        /// The zero-result returned for Recovery sessions — no XP, negative fatigue cost, zero injury risk.
        /// </summary>
        public static TrainingResult Recovery(float fatigueCost = -15f)
        {
            if (fatigueCost >= 0f)
                throw new ArgumentOutOfRangeException(nameof(fatigueCost),
                    "Recovery fatigue cost must be negative (restores fatigue).");

            return new TrainingResult(
                new ReadOnlyDictionary<AttributeName, float>(new Dictionary<AttributeName, float>()),
                fatigueCost,
                0f);
        }

        /// <summary>Creates a <see cref="TrainingResult"/> from a mutable dictionary.</summary>
        public static TrainingResult Create(
            Dictionary<AttributeName, float> xpGained,
            float fatigueCost,
            float injuryRisk)
            => new TrainingResult(
                new ReadOnlyDictionary<AttributeName, float>(new Dictionary<AttributeName, float>(xpGained)),
                fatigueCost,
                injuryRisk);
    }
}
