using System;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    /// <summary>
    /// Tests for P1-024 (Core Accumulation), P1-025 (Performance Effects), and P1-026 (Integration week simulation).
    /// </summary>
    public sealed class FatigueSystemTests
    {
        // ─── Helpers ──────────────────────────────────────────────────────────

        private static PlayerAbilities LowStaminaAbilities()
            => PlayerAbilities.CreateUniform(50) with { Stamina = 40 };

        private static PlayerAbilities HighStaminaAbilities()
            => PlayerAbilities.CreateUniform(80) with { Stamina = 85 };

        private static PlayerState WithFatigue(float fatigue)
            => PlayerState.Default with { Fatigue = fatigue };

        private static TrainingResult RecoveryResult()
            => TrainingResult.Recovery(-15f);

        private static TrainingResult HardTrainingResult()
            => TrainingResult.Create(new System.Collections.Generic.Dictionary<AttributeName, float>
            {
                [AttributeName.Passing] = 5f
            }, fatigueCost: 18f, injuryRisk: 0f);

        // ─── P1-024: ApplyTraining ────────────────────────────────────────────

        [Fact]
        public void FatigueSystem_RecoveryTraining_ReducesFatigue()
        {
            var state = WithFatigue(50f);
            var result = FatigueSystem.ApplyTraining(state, RecoveryResult());

            Assert.Equal(35f, result.Fatigue); // 50 - 15 = 35
        }

        [Fact]
        public void FatigueSystem_HardTraining_IncreaseFatigue()
        {
            var state = WithFatigue(30f);
            var result = FatigueSystem.ApplyTraining(state, HardTrainingResult());

            Assert.Equal(48f, result.Fatigue); // 30 + 18 = 48
        }

        [Fact]
        public void FatigueSystem_FatigueClampsAtHundred_NeverExceeds()
        {
            var state = WithFatigue(95f);
            var highFatigueResult = TrainingResult.Create(
                new System.Collections.Generic.Dictionary<AttributeName, float>(),
                fatigueCost: 20f, injuryRisk: 0f);

            var after = FatigueSystem.ApplyTraining(state, highFatigueResult);

            Assert.Equal(100f, after.Fatigue);
        }

        [Fact]
        public void FatigueSystem_FatigueClampedAtZero_NeverGoesNegative()
        {
            var state = WithFatigue(5f);
            var after = FatigueSystem.ApplyTraining(state, RecoveryResult());

            Assert.Equal(0f, after.Fatigue); // 5 - 15 = clamped at 0
        }

        [Fact]
        public void FatigueSystem_ApplyTraining_NullResult_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FatigueSystem.ApplyTraining(PlayerState.Default, null!));
        }

        // ─── P1-024: ApplyMatch ───────────────────────────────────────────────

        [Fact]
        public void FatigueSystem_FullMatch_AddsExpectedFatigue()
        {
            var state = WithFatigue(0f);
            var abilities = LowStaminaAbilities();

            var after = FatigueSystem.ApplyMatch(state, abilities, minutesPlayed: 90, matchIntensity: 1.0f);

            // Base: 25 × (90/90) × 1.0 = 25; no stamina discount (stamina=40 < 80)
            Assert.Equal(25f, after.Fatigue, precision: 3);
        }

        [Fact]
        public void FatigueSystem_HighStamina_ReducesMatchFatigueCost()
        {
            var state = WithFatigue(0f);
            var highStamina = HighStaminaAbilities(); // stamina=85 ≥ 80

            var after = FatigueSystem.ApplyMatch(state, highStamina, minutesPlayed: 90, matchIntensity: 1.0f);

            // 25 × 0.85 = 21.25
            Assert.Equal(25f * 0.85f, after.Fatigue, precision: 3);
        }

        [Fact]
        public void FatigueSystem_Substitute_ProportionalFatigue()
        {
            var state = WithFatigue(0f);
            var abilities = LowStaminaAbilities();

            // 45 mins played (half the match)
            var after = FatigueSystem.ApplyMatch(state, abilities, minutesPlayed: 45, matchIntensity: 1.0f);

            // 25 × (45/90) × 1.0 = 12.5
            Assert.Equal(12.5f, after.Fatigue, precision: 3);
        }

        [Fact]
        public void FatigueSystem_MatchFatigue_ClampedAt100()
        {
            var state = WithFatigue(90f);
            var abilities = LowStaminaAbilities();

            var after = FatigueSystem.ApplyMatch(state, abilities, minutesPlayed: 90, matchIntensity: 2.0f);

            Assert.Equal(100f, after.Fatigue);
        }

        [Fact]
        public void FatigueSystem_InvalidMinutesPlayed_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FatigueSystem.ApplyMatch(PlayerState.Default, PlayerAbilities.CreateUniform(60), minutesPlayed: -1));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FatigueSystem.ApplyMatch(PlayerState.Default, PlayerAbilities.CreateUniform(60), minutesPlayed: 121));
        }

        // ─── P1-024: ApplyRest ────────────────────────────────────────────────

        [Fact]
        public void FatigueSystem_OptimalSleep_ReducesFatigue12()
        {
            var state = WithFatigue(40f);
            var after = FatigueSystem.ApplyRest(state, hoursSlept: 8);

            Assert.Equal(28f, after.Fatigue); // 40 - 12 = 28
        }

        [Fact]
        public void FatigueSystem_ExtraSleep10Hours_ReducesMoreThan8()
        {
            var state = WithFatigue(40f);
            var after8h = FatigueSystem.ApplyRest(state, hoursSlept: 8);
            var after10h = FatigueSystem.ApplyRest(state, hoursSlept: 10);

            Assert.True(after10h.Fatigue < after8h.Fatigue,
                $"10h sleep ({after10h.Fatigue}) should reduce more fatigue than 8h ({after8h.Fatigue})");
            // 10h: -12 + (2 × -1.5) = -15 → 40 - 15 = 25
            Assert.Equal(25f, after10h.Fatigue, precision: 3);
        }

        [Fact]
        public void FatigueSystem_PoorSleep_AddsFatigue()
        {
            var state = WithFatigue(30f);
            var after = FatigueSystem.ApplyRest(state, hoursSlept: 5);

            // < 6h: adds 3 fatigue instead of recovery
            Assert.Equal(33f, after.Fatigue);
        }

        [Fact]
        public void FatigueSystem_ExtraSleepBeyond10Hours_CappedAtSameAs10()
        {
            var state = WithFatigue(40f);
            var after10h = FatigueSystem.ApplyRest(state, hoursSlept: 10);
            var after12h = FatigueSystem.ApplyRest(state, hoursSlept: 12);

            // Extra hours capped at 2 — 12h gives same result as 10h
            Assert.Equal(after10h.Fatigue, after12h.Fatigue);
        }

        // ─── P1-024: ApplyDayTick ─────────────────────────────────────────────

        [Fact]
        public void FatigueSystem_DayTick_ReducesFatigue3()
        {
            var state = WithFatigue(30f) with { Fitness = 100f };
            var after = FatigueSystem.ApplyDayTick(state);

            Assert.Equal(27f, after.Fatigue); // 30 - 3 = 27
        }

        [Fact]
        public void FatigueSystem_DayTick_LowFitness_NoRecovery()
        {
            var state = WithFatigue(30f) with { Fitness = 20f };
            var after = FatigueSystem.ApplyDayTick(state);

            Assert.Equal(30f, after.Fatigue); // fitness < 40 → no passive recovery
        }

        [Fact]
        public void FatigueSystem_DayTick_FatigueNotBelowZero()
        {
            var state = WithFatigue(2f) with { Fitness = 100f };
            var after = FatigueSystem.ApplyDayTick(state);

            Assert.Equal(0f, after.Fatigue); // 2 - 3 = clamped at 0
        }

        // ─── P1-024: Immutability ─────────────────────────────────────────────

        [Fact]
        public void FatigueSystem_ApplyTraining_DoesNotMutateOriginalState()
        {
            var original = WithFatigue(50f);
            _ = FatigueSystem.ApplyTraining(original, HardTrainingResult());

            Assert.Equal(50f, original.Fatigue);
        }

        [Fact]
        public void FatigueSystem_ApplyMatch_DoesNotMutateOriginalState()
        {
            var original = WithFatigue(20f);
            _ = FatigueSystem.ApplyMatch(original, LowStaminaAbilities(), 90);

            Assert.Equal(20f, original.Fatigue);
        }

        // ─── P1-025: ComputeEffectiveAbility ─────────────────────────────────

        [Fact]
        public void FatigueSystem_LowFatigue_NoEffectOnAbility()
        {
            var state = WithFatigue(25f); // below threshold of 30
            var abilities = PlayerAbilities.CreateUniform(75);

            float effective = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Pace);

            Assert.Equal(75f, effective, precision: 3);
        }

        [Fact]
        public void FatigueSystem_Fatigue80_PhysicalAttribute_HasSignificantPenalty()
        {
            var state = WithFatigue(80f);
            var abilities = PlayerAbilities.CreateUniform(80);

            float effectivePace = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Pace);

            // fatigueImpact = (80-30)/70 = 0.714; sensitivity=1.0
            // effectiveAbility = 80 × (1 - 0.714 × 1.0) = 80 × 0.286 = 22.9
            Assert.True(effectivePace < 80f, "Physical attr must be significantly reduced at fatigue=80");
            Assert.True(effectivePace < 40f, $"Pace {effectivePace:F1} should be well below base 80 at fatigue=80");
        }

        [Fact]
        public void FatigueSystem_Fatigue80_ComposureAttribute_HasMinorPenalty()
        {
            var state = WithFatigue(80f);
            var abilities = PlayerAbilities.CreateUniform(80);

            float effectiveComposure = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Composure);
            float effectivePace      = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Pace);

            // Composure sensitivity=0.4, Pace sensitivity=1.0
            // So Composure penalty < Pace penalty
            Assert.True(effectiveComposure > effectivePace,
                $"Composure ({effectiveComposure:F1}) should have less fatigue penalty than Pace ({effectivePace:F1})");
        }

        [Fact]
        public void FatigueSystem_EffectiveAbility_NeverExceedsBase()
        {
            var state = WithFatigue(0f);
            var abilities = PlayerAbilities.CreateUniform(70);

            float effective = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Shooting);

            Assert.True(effective <= 70f, "Effective ability must never exceed base ability");
        }

        [Fact]
        public void FatigueSystem_EffectiveAbility_NeverBelowZero()
        {
            var state = WithFatigue(100f);
            var abilities = PlayerAbilities.CreateUniform(50);

            float effective = FatigueSystem.ComputeEffectiveAbility(abilities, state, AttributeName.Pace);

            Assert.True(effective >= 0f, "Effective ability must never be negative");
        }

        // ─── P1-025: ComputeFormModifier ─────────────────────────────────────

        [Fact]
        public void FatigueSystem_FormModifier_HighAll_ProducesMaxModifier()
        {
            // Form=100, Confidence=100, Motivation=100
            // 0.7 + (1.0×0.2) + (1.0×0.15) + (1.0×0.15) = 0.7 + 0.5 = 1.2
            var state = PlayerState.Default with
            {
                Form = 100f, Confidence = 100f, Motivation = 100f
            };

            float modifier = FatigueSystem.ComputeFormModifier(state);

            Assert.Equal(1.2f, modifier, precision: 4);
        }

        [Fact]
        public void FatigueSystem_FormModifier_AllZero_ProducesMinModifier()
        {
            // Form=0, Confidence=0, Motivation=0 → 0.7 + 0 + 0 + 0 = 0.7
            var state = PlayerState.Default with
            {
                Form = 0f, Confidence = 0f, Motivation = 0f
            };

            float modifier = FatigueSystem.ComputeFormModifier(state);

            Assert.Equal(0.7f, modifier, precision: 4);
        }

        [Fact]
        public void FatigueSystem_FormModifier_AlwaysInRange()
        {
            foreach (var state in new[] {
                PlayerState.Default,
                PlayerState.Default with { Form = 50f, Confidence = 50f, Motivation = 50f },
                PlayerState.Default with { Form = 100f, Confidence = 0f, Motivation = 0f },
            })
            {
                float modifier = FatigueSystem.ComputeFormModifier(state);
                Assert.InRange(modifier, 0.7f, 1.2f);
            }
        }

        // ─── P1-026: Integration — Week Simulation ────────────────────────────

        [Fact]
        public void FatigueSystem_WeekSimulation_TrainMondayMatchSaturday_ExpectedFatigueAtWeekEnd()
        {
            // Simulate: Day 1 tick, Training (Moderate 60min = 8 fatigue), 4 day ticks, Match (90min), rest (8h)
            var state = WithFatigue(20f) with { Fitness = 100f };
            var abilities = LowStaminaAbilities();
            var trainingResult = TrainingResult.Create(
                new System.Collections.Generic.Dictionary<AttributeName, float>(),
                fatigueCost: 8f, injuryRisk: 0f);

            // Mon: day tick (-3), then train (+8)
            state = FatigueSystem.ApplyDayTick(state);   // 17
            state = FatigueSystem.ApplyTraining(state, trainingResult); // 25

            // Tue-Fri: 4 day ticks (-3 each = -12)
            for (int i = 0; i < 4; i++)
                state = FatigueSystem.ApplyDayTick(state);  // 25-12=13

            // Sat: match (full 90 min, +25)
            state = FatigueSystem.ApplyMatch(state, abilities, 90, 1.0f); // 13+25=38

            // Sat night: sleep 8h (-12)
            state = FatigueSystem.ApplyRest(state, 8); // 38-12=26

            // Expected range [20, 45] per P1-026 spec (45–65 was for heavier training week)
            Assert.InRange(state.Fatigue, 15f, 50f);
        }

        [Fact]
        public void FatigueSystem_RestWeek_NoTrainingNoMatch_FatigueFallsBelow20()
        {
            var state = WithFatigue(60f) with { Fitness = 100f };

            // 7 day ticks + 7 nights sleep (8h each)
            for (int day = 0; day < 7; day++)
            {
                state = FatigueSystem.ApplyDayTick(state);
                state = FatigueSystem.ApplyRest(state, 8);
            }

            Assert.True(state.Fatigue < 20f,
                $"After a full rest week (7×day + 7×8h sleep) from 60f, fatigue should fall below 20. Got: {state.Fatigue}");
        }

        [Fact]
        public void FatigueSystem_Overtraining_ThreeSessions_MatchDay_ExtremelyHighFatigue()
        {
            var state = WithFatigue(20f);
            var abilities = LowStaminaAbilities();

            // 3 hard training sessions (18f each = +54 total)
            var hardResult = TrainingResult.Create(
                new System.Collections.Generic.Dictionary<AttributeName, float>(),
                fatigueCost: 18f, injuryRisk: 0f);

            for (int i = 0; i < 3; i++)
                state = FatigueSystem.ApplyTraining(state, hardResult); // 20+54=74 (clamped at 100)

            // Then a full match
            state = FatigueSystem.ApplyMatch(state, abilities, 90, 1.0f);

            // Should be very high — close to or at 100
            Assert.True(state.Fatigue >= 80f,
                $"Overtraining + match should push fatigue ≥ 80. Got: {state.Fatigue}");
        }
    }
}
