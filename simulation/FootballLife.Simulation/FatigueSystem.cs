using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure stateless simulation system for fatigue accumulation, recovery, and performance modifiers.
    /// All methods return new immutable <see cref="PlayerState"/> records — the input is never mutated.
    /// </summary>
    /// <remarks>
    /// Covers P1-024 (core accumulation) and P1-025 (effect on performance).
    /// No LINQ, no heap allocations in hot paths, no UnityEngine references.
    /// </remarks>
    public static class FatigueSystem
    {
        // ─── Match fatigue constants ───────────────────────────────────────────
        private const float MatchFatigueBase       = 25f;   // full 90 min at matchIntensity=1.0
        private const int   HighStaminaThreshold   = 80;    // stamina ≥ 80 reduces fatigue gain
        private const float HighStaminaDiscount    = 0.85f; // 15% reduction

        // ─── Sleep constants ──────────────────────────────────────────────────
        private const int   OptimalSleepHours      = 8;
        private const float OptimalSleepRecovery   = -12f;  // -12 fatigue at 8h
        private const float ExtraHourRecovery      = -1.5f; // per hour above 8h
        private const int   MaxSleepBonusHours     = 2;     // capped at 10h total
        private const int   PoorSleepThreshold     = 6;
        private const float PoorSleepPenalty       = 3f;    // no recovery + adds 3

        // ─── Daily passive recovery ───────────────────────────────────────────
        private const float DailyPassiveRecovery   = -3f;
        private const float MinFitnessForRecovery  = 40f;

        // ─── Performance modifier thresholds ─────────────────────────────────
        private const float FatigueImpactThreshold = 30f;   // no penalty below this
        private const float FatigueImpactRange     = 70f;   // 30–100 maps to 0.0–1.0

        // Attribute sensitivity to fatigue (1.0 = full, 0.6 = moderate, 0.4 = minor)
        private const float SensitivityPhysical    = 1.0f;
        private const float SensitivityTechnical   = 0.6f;
        private const float SensitivityMental      = 0.4f;

        // ─── Form modifier constants ──────────────────────────────────────────
        private const float FormModifierBase       = 0.70f;
        private const float FormContribution       = 0.20f;
        private const float ConfidenceContribution = 0.15f;
        private const float MotivationContribution = 0.15f;
        private const float FormModifierMin        = 0.70f;
        private const float FormModifierMax        = 1.20f;

        // ─── P1-024: Core Accumulation ────────────────────────────────────────

        /// <summary>
        /// Applies training fatigue cost to the player state.
        /// Fatigue += result.FatigueCost, clamped [0, 100].
        /// Recovery results (negative FatigueCost) reduce fatigue.
        /// </summary>
        public static PlayerState ApplyTraining(PlayerState state, TrainingResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            float newFatigue = Clamp(state.Fatigue + result.FatigueCost, 0f, 100f);
            return state with { Fatigue = newFatigue };
        }

        /// <summary>
        /// Applies match fatigue to the player state.
        /// Full 90 min at matchIntensity=1.0 adds 25 fatigue.
        /// Proportional for substitutes. High stamina (≥80) reduces fatigue gain by 15%.
        /// </summary>
        /// <param name="state">Current player state.</param>
        /// <param name="abilities">Player abilities — Stamina affects fatigue resistance.</param>
        /// <param name="minutesPlayed">Minutes played [0, 120].</param>
        /// <param name="matchIntensity">Intensity multiplier (0.0 = cup walkover, 1.0 = full league match).</param>
        public static PlayerState ApplyMatch(
            PlayerState state,
            PlayerAbilities abilities,
            int minutesPlayed,
            float matchIntensity = 1.0f)
        {
            if (minutesPlayed < 0 || minutesPlayed > 120)
                throw new ArgumentOutOfRangeException(nameof(minutesPlayed), "Minutes played must be in [0, 120].");
            if (matchIntensity < 0f)
                throw new ArgumentOutOfRangeException(nameof(matchIntensity), "Match intensity must be non-negative.");

            float baseFatigue = MatchFatigueBase * (minutesPlayed / 90f) * matchIntensity;

            // High stamina reduces fatigue gain
            if (abilities.Stamina >= HighStaminaThreshold)
                baseFatigue *= HighStaminaDiscount;

            float newFatigue = Clamp(state.Fatigue + baseFatigue, 0f, 100f);
            return state with { Fatigue = newFatigue };
        }

        /// <summary>
        /// Applies a night's sleep to the player's fatigue.
        /// 8h: -12 fatigue. Each hour above 8 (max 2): -1.5.
        /// Below 6h: no recovery, adds 3 fatigue (sleep deprivation penalty).
        /// </summary>
        public static PlayerState ApplyRest(PlayerState state, int hoursSlept)
        {
            if (hoursSlept < 0)
                throw new ArgumentOutOfRangeException(nameof(hoursSlept), "hoursSlept must be non-negative.");

            float delta;

            if (hoursSlept < PoorSleepThreshold)
            {
                // Poor sleep: no recovery + penalty
                delta = PoorSleepPenalty;
            }
            else
            {
                // Base 8h recovery
                delta = OptimalSleepRecovery;

                // Bonus hours above 8 (capped at 2 extra hours)
                int extraHours = Math.Min(hoursSlept - OptimalSleepHours, MaxSleepBonusHours);
                if (extraHours > 0)
                    delta += extraHours * ExtraHourRecovery;
            }

            float newFatigue = Clamp(state.Fatigue + delta, 0f, 100f);
            return state with { Fatigue = newFatigue };
        }

        /// <summary>
        /// Applies passive daily recovery (off-day tick).
        /// -3 fatigue per day. No recovery if Fitness &lt; 40.
        /// </summary>
        public static PlayerState ApplyDayTick(PlayerState state)
        {
            if (state.Fitness < MinFitnessForRecovery)
                return state; // Injured/very unfit — no passive recovery

            float newFatigue = Clamp(state.Fatigue + DailyPassiveRecovery, 0f, 100f);
            return state with { Fatigue = newFatigue };
        }

        // ─── P1-025: Effect on Performance ────────────────────────────────────

        /// <summary>
        /// Computes the fatigue-adjusted effective ability value for a given attribute.
        /// Formula: effectiveAbility = baseAbility × (1 - fatigueImpact × attributeSensitivity)
        /// Result clamped to [0, baseAbility] — fatigue never boosts an attribute.
        /// </summary>
        /// <param name="abilities">Base player abilities.</param>
        /// <param name="state">Current player state (Fatigue used).</param>
        /// <param name="attribute">The attribute to adjust.</param>
        public static float ComputeEffectiveAbility(
            PlayerAbilities abilities,
            PlayerState state,
            AttributeName attribute)
        {
            float baseAbility = abilities.Get(attribute);

            float fatigueImpact = MathF.Max(0f, (state.Fatigue - FatigueImpactThreshold) / FatigueImpactRange);
            float sensitivity = GetAttributeSensitivity(attribute);

            float effective = baseAbility * (1f - fatigueImpact * sensitivity);
            return Clamp(effective, 0f, baseAbility);
        }

        /// <summary>
        /// Computes a form modifier [0.7, 1.2] combining Form, Confidence, and Motivation.
        /// Formula: 0.7 + (Form/100 × 0.2) + (Confidence/100 × 0.15) + (Motivation/100 × 0.15)
        /// </summary>
        public static float ComputeFormModifier(PlayerState state)
        {
            float modifier = FormModifierBase
                + (state.Form        / 100f * FormContribution)
                + (state.Confidence  / 100f * ConfidenceContribution)
                + (state.Motivation  / 100f * MotivationContribution);

            return Clamp(modifier, FormModifierMin, FormModifierMax);
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private static float GetAttributeSensitivity(AttributeName attribute) => attribute switch
        {
            // Physical attributes — fully sensitive to fatigue
            AttributeName.Pace         or AttributeName.Acceleration or
            AttributeName.Stamina      or AttributeName.Strength     or
            AttributeName.Agility
                => SensitivityPhysical,

            // Technical attributes — moderately sensitive
            AttributeName.Passing      or AttributeName.Shooting     or
            AttributeName.Dribbling    or AttributeName.Crossing     or
            AttributeName.FirstTouch   or AttributeName.Tackling
                => SensitivityTechnical,

            // Mental/cognitive attributes — least sensitive
            AttributeName.Vision       or AttributeName.Composure    or
            AttributeName.Positioning  or AttributeName.DecisionMaking
                => SensitivityMental,

            // Default — moderate sensitivity for any unmapped attribute
            _ => SensitivityTechnical,
        };

        private static float Clamp(float value, float min, float max)
            => value < min ? min : value > max ? max : value;
    }
}
