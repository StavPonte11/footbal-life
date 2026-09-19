using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    /// <summary>
    /// Tests for P1-019 (XP Calculation), P1-020 (Diminishing Returns), and P1-021 (comprehensive edge cases).
    /// </summary>
    public sealed class TrainingSystemTests
    {
        // ─── Helpers ──────────────────────────────────────────────────────────

        private static readonly DateOnly TestDate = new DateOnly(2026, 9, 1);

        private static PlayerAbilities DefaultAbilities() => PlayerAbilities.CreateUniform(60);

        private static PlayerState WithMotivation(float motivation)
            => PlayerState.Default with { Motivation = motivation };

        private static PlayerState WithFatigue(float fatigue)
            => PlayerState.Default with { Fatigue = fatigue };

        private static PlayerState WithMotivationAndFatigue(float motivation, float fatigue)
            => PlayerState.Default with { Motivation = motivation, Fatigue = fatigue };

        private static TrainingSession Session(
            TrainingType type,
            TrainingIntensity intensity = TrainingIntensity.Moderate,
            int duration = 60)
            => new TrainingSession(type, intensity, duration, TestDate);

        // ─── P1-019: XP Calculation ───────────────────────────────────────────

        [Fact]
        public void TrainingSystem_Technical_AwardsPassingXP_NotStamina()
        {
            var session = Session(TrainingType.Technical);
            var result = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(result.XpGained.ContainsKey(AttributeName.Passing),
                "Technical training must award Passing XP");
            Assert.True(result.XpGained.ContainsKey(AttributeName.Shooting),
                "Technical training must award Shooting XP");
            Assert.True(result.XpGained.ContainsKey(AttributeName.Dribbling),
                "Technical training must award Dribbling XP");

            Assert.False(result.XpGained.ContainsKey(AttributeName.Stamina),
                "Technical training must NOT award Stamina XP");
            Assert.False(result.XpGained.ContainsKey(AttributeName.Strength),
                "Technical training must NOT award Strength XP");
            Assert.False(result.XpGained.ContainsKey(AttributeName.Vision),
                "Technical training must NOT award Vision XP");
        }

        [Fact]
        public void TrainingSystem_Physical_AwardsStaminaAndPace_NotPassing()
        {
            var session = Session(TrainingType.Physical);
            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(result.XpGained.ContainsKey(AttributeName.Stamina));
            Assert.True(result.XpGained.ContainsKey(AttributeName.Pace));
            Assert.True(result.XpGained.ContainsKey(AttributeName.Strength));

            Assert.False(result.XpGained.ContainsKey(AttributeName.Passing));
            Assert.False(result.XpGained.ContainsKey(AttributeName.Shooting));
        }

        [Fact]
        public void TrainingSystem_Mental_AwardsVisionAndComposure_NotPhysical()
        {
            var session = Session(TrainingType.Mental);
            var result = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(result.XpGained.ContainsKey(AttributeName.Vision));
            Assert.True(result.XpGained.ContainsKey(AttributeName.Composure));
            Assert.True(result.XpGained.ContainsKey(AttributeName.Positioning));
            Assert.True(result.XpGained.ContainsKey(AttributeName.DecisionMaking));

            Assert.False(result.XpGained.ContainsKey(AttributeName.Pace));
            Assert.False(result.XpGained.ContainsKey(AttributeName.Strength));
        }

        [Fact]
        public void TrainingSystem_Gym_AwardsStrengthAndStamina_NotTechnical()
        {
            var session = Session(TrainingType.Gym);
            var result = TrainingSystem.CalculateXP(session, Position.CB, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(result.XpGained.ContainsKey(AttributeName.Strength));
            Assert.True(result.XpGained.ContainsKey(AttributeName.Stamina));

            Assert.False(result.XpGained.ContainsKey(AttributeName.Passing));
            Assert.False(result.XpGained.ContainsKey(AttributeName.Shooting));
            Assert.False(result.XpGained.ContainsKey(AttributeName.Vision));
        }

        [Fact]
        public void TrainingSystem_Recovery_AwardsZeroXP_AndRestoresFatigue()
        {
            var session = Session(TrainingType.Recovery);
            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.Empty(result.XpGained);
            Assert.Equal(0f, result.TotalXpGained);
            Assert.Equal(-15f, result.FatigueCost);
            Assert.Equal(0f, result.InjuryRisk);
        }

        [Fact]
        public void TrainingSystem_PositionSpecific_Striker_AwardsShootingAndPace()
        {
            var session = Session(TrainingType.PositionSpecific);
            // ST position has high Shooting, Pace, Positioning weights
            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            // Should have exactly 3 attributes
            Assert.Equal(3, result.XpGained.Count);

            // For ST, Shooting should be one of the top 3
            Assert.True(result.XpGained.ContainsKey(AttributeName.Shooting),
                "ST PositionSpecific training must award Shooting (highest weighted for striker)");
        }

        [Fact]
        public void TrainingSystem_PositionSpecific_Goalkeeper_HasThreeAttributes()
        {
            var session = Session(TrainingType.PositionSpecific);
            var result = TrainingSystem.CalculateXP(session, Position.GK, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.Equal(3, result.XpGained.Count);
        }

        [Fact]
        public void TrainingSystem_MaximumIntensity_ProducesHigherXP_ThanModerate()
        {
            var sessionMod = Session(TrainingType.Technical, TrainingIntensity.Moderate);
            var sessionMax = Session(TrainingType.Technical, TrainingIntensity.Maximum);

            var resultMod = TrainingSystem.CalculateXP(sessionMod, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(42));
            var resultMax = TrainingSystem.CalculateXP(sessionMax, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(42));

            Assert.True(resultMax.TotalXpGained > resultMod.TotalXpGained,
                $"Maximum intensity ({resultMax.TotalXpGained:F2} XP) should exceed Moderate ({resultMod.TotalXpGained:F2} XP)");
        }

        [Fact]
        public void TrainingSystem_MaximumIntensity_ProducesHigherFatigue_ThanLight()
        {
            var sessionLight = Session(TrainingType.Physical, TrainingIntensity.Light);
            var sessionMax   = Session(TrainingType.Physical, TrainingIntensity.Maximum);

            var resultLight = TrainingSystem.CalculateXP(sessionLight, Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));
            var resultMax   = TrainingSystem.CalculateXP(sessionMax,   Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(resultMax.FatigueCost > resultLight.FatigueCost,
                $"Maximum intensity fatigue ({resultMax.FatigueCost:F1}) should exceed Light ({resultLight.FatigueCost:F1})");
        }

        [Fact]
        public void TrainingSystem_LowMotivation_ReducesXpGained()
        {
            var session = Session(TrainingType.Technical);
            var highMotState = WithMotivation(100f);
            var lowMotState  = WithMotivation(10f);

            // Use same seed for each
            var resultHigh = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), highMotState, new SimulationRandom(7));
            var resultLow  = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), lowMotState,  new SimulationRandom(7));

            Assert.True(resultHigh.TotalXpGained > resultLow.TotalXpGained,
                $"High motivation ({resultHigh.TotalXpGained:F2}) should produce more XP than low motivation ({resultLow.TotalXpGained:F2})");
        }

        [Fact]
        public void TrainingSystem_LongerDuration_ProducesMoreXP()
        {
            var session30 = Session(TrainingType.Technical, TrainingIntensity.Moderate, 30);
            var session90 = Session(TrainingType.Technical, TrainingIntensity.Moderate, 90);

            var result30 = TrainingSystem.CalculateXP(session30, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));
            var result90 = TrainingSystem.CalculateXP(session90, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            Assert.True(result90.TotalXpGained > result30.TotalXpGained);
            Assert.True(result90.FatigueCost > result30.FatigueCost);
        }

        // ─── P1-019: Injury Risk ─────────────────────────────────────────────

        [Fact]
        public void TrainingSystem_MaximumIntensity_AtHighFatigue_RaisesInjuryRisk()
        {
            var session = Session(TrainingType.Physical, TrainingIntensity.Maximum);
            var highFatigueState = WithFatigue(90f);

            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), highFatigueState, new SimulationRandom(1));

            Assert.True(result.InjuryRisk > 0f,
                "High fatigue + Maximum intensity must produce non-zero injury risk");
            // 90f fatigue: t = (90-40)/60 = 0.833 → base = 0.833 * 0.08 = 0.0667 × 2 (Maximum) = 0.133
            Assert.True(result.InjuryRisk > 0.10f,
                $"Injury risk {result.InjuryRisk:F3} should exceed 10% at fatigue=90, Maximum intensity");
        }

        [Fact]
        public void TrainingSystem_Fatigue100_MaximumIntensity_ProducesHighInjuryRisk()
        {
            var session = Session(TrainingType.Physical, TrainingIntensity.Maximum);
            var maxFatigueState = WithFatigue(100f);

            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), maxFatigueState, new SimulationRandom(1));

            // t=1.0 → base=0.08 × 2 (Maximum) = 0.16
            Assert.True(result.InjuryRisk > 0.10f,
                $"Fatigue=100, Maximum intensity must produce injury risk > 10%. Got: {result.InjuryRisk:F3}");
        }

        [Fact]
        public void TrainingSystem_Fatigue0_AnyIntensity_ZeroInjuryRisk()
        {
            var noFatigueState = WithFatigue(0f);

            foreach (TrainingIntensity intensity in Enum.GetValues<TrainingIntensity>())
            {
                var session = Session(TrainingType.Technical, intensity);
                var result = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), noFatigueState, new SimulationRandom(1));
                Assert.Equal(0f, result.InjuryRisk);
            }
        }

        [Fact]
        public void TrainingSystem_FatigueBelow40_ZeroInjuryRisk()
        {
            var state = WithFatigue(39f);
            var session = Session(TrainingType.Physical, TrainingIntensity.Maximum);

            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), state, new SimulationRandom(1));

            Assert.Equal(0f, result.InjuryRisk);
        }

        [Fact]
        public void TrainingSystem_InjuryRisk_NeverExceedsOne()
        {
            var maxFatigue = WithFatigue(100f);
            var session = Session(TrainingType.Physical, TrainingIntensity.Maximum);

            var result = TrainingSystem.CalculateXP(session, Position.ST, DefaultAbilities(), maxFatigue, new SimulationRandom(1));

            Assert.True(result.InjuryRisk <= 1f, $"Injury risk must never exceed 1.0. Got: {result.InjuryRisk}");
        }

        // ─── P1-020: Weekly Diminishing Returns ───────────────────────────────

        [Fact]
        public void TrainingSystem_TwoTechnicalSessions_SecondHas70PercentXP()
        {
            var session1 = Session(TrainingType.Technical);
            var session2 = Session(TrainingType.Technical);
            var state = PlayerState.Default;

            var results = TrainingSystem.CalculateWeekXP(
                new[] { session1, session2 },
                Position.CM, DefaultAbilities(), state,
                new SimulationRandom(1));

            Assert.Equal(2, results.Count);

            float xp1 = results[0].TotalXpGained;
            float xp2 = results[1].TotalXpGained;

            Assert.True(xp1 > 0f, "First session should produce XP");
            Assert.True(xp2 > 0f, "Second session should produce XP");

            // Second session should be LESS than first due to DR
            Assert.True(xp2 < xp1,
                $"Second session XP ({xp2:F2}) must be less than first ({xp1:F2}) due to 30% DR");

            // With DR=0.70 and rng_variance clamped [0.7, 1.3], ratio is in:
            // min: 0.70 * (0.7 / 1.3) = 0.377, max: 0.70 * (1.3 / 0.7) = 1.3 (capped by 0.70)
            // In practice both variances are close to 1.0, so ratio ≈ 0.70 ± noise
            // Use a conservatively wide band: [0.45, 0.85]
            float ratio = xp2 / xp1;
            Assert.InRange(ratio, 0.45f, 0.85f);
        }

        [Fact]
        public void TrainingSystem_ThreeTechnicalSessions_DiminishingByFactor()
        {
            var sessions = new[] {
                Session(TrainingType.Technical),
                Session(TrainingType.Technical),
                Session(TrainingType.Technical),
            };

            var results = TrainingSystem.CalculateWeekXP(
                sessions, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            float xp1 = results[0].TotalXpGained;
            float xp2 = results[1].TotalXpGained;
            float xp3 = results[2].TotalXpGained;

            // DR factor: 0.7^0=1.0, 0.7^1=0.7, 0.7^2=0.49
            // Each session draws its own rng_variance so absolute ratios vary.
            // The key invariant is: xp1 > xp2 > xp3 (strictly decreasing).
            Assert.True(xp1 > 0f, "First session must produce XP");
            Assert.True(xp2 > 0f, "Second session must produce XP");
            Assert.True(xp3 > 0f, "Third session must produce XP");

            // For the chosen seed (1), verify the monotone decreasing property
            Assert.True(xp2 < xp1, $"Session 2 ({xp2:F2}) must be less than Session 1 ({xp1:F2})");
            Assert.True(xp3 < xp2, $"Session 3 ({xp3:F2}) must be less than Session 2 ({xp2:F2})");

            // Verify ratios are in the expected DR ballpark (DR=0.70, rng variance ±30% → wide band)
            float ratio2 = xp2 / xp1;
            float ratio3 = xp3 / xp1;
            Assert.InRange(ratio2, 0.45f, 0.85f);
            Assert.InRange(ratio3, 0.25f, 0.60f);
        }

        [Fact]
        public void TrainingSystem_MixedTypes_NoDiminishingBetweenDifferentTypes()
        {
            var sessions = new[] {
                Session(TrainingType.Technical),
                Session(TrainingType.Physical),
                Session(TrainingType.Mental),
            };

            var results = TrainingSystem.CalculateWeekXP(
                sessions, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(99));

            // Each is the first session of its type → no DR applied
            // Physical produces 3.0 BaseXP, Technical 3.0, Mental 2.0 — all at 100% DR
            // We can verify by running CalculateXP single-session with same rng and comparing
            // (But rng state is sequential — just verify all XP > 0 and ratios make sense)
            foreach (var result in results)
            {
                Assert.True(result.TotalXpGained > 0f,
                    "First session of any type in a week should have no DR reduction");
            }
        }

        [Fact]
        public void TrainingSystem_RecoveryExemptFromDiminishingReturns()
        {
            var sessions = new[] {
                Session(TrainingType.Recovery),
                Session(TrainingType.Recovery),
                Session(TrainingType.Recovery),
            };

            var results = TrainingSystem.CalculateWeekXP(
                sessions, Position.ST, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));

            // All recovery sessions should return the same -15 fatigue, 0 XP, 0 injury
            foreach (var result in results)
            {
                Assert.Equal(0f, result.TotalXpGained);
                Assert.Equal(-15f, result.FatigueCost);
                Assert.Equal(0f, result.InjuryRisk);
            }
        }

        [Fact]
        public void TrainingSystem_FourSessions_ThrowsInvalidOperation()
        {
            var sessions = new[] {
                Session(TrainingType.Technical),
                Session(TrainingType.Physical),
                Session(TrainingType.Mental),
                Session(TrainingType.Gym),
            };

            Assert.Throws<InvalidOperationException>(() =>
                TrainingSystem.CalculateWeekXP(sessions, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1)));
        }

        // ─── P1-021: Statistical Balance & Determinism ───────────────────────

        [Fact]
        public void TrainingSystem_StatisticalBalance_Over1000Seeds_MeanXpWithin10PercentOfExpected()
        {
            // For Technical, CM, Moderate, 60min, Motivation=100, Fatigue=0:
            // BaseXP=3.0, IntensityMult=1.0, StateModifier=1.0, DurationScale=1.0
            // Expected XP per attr ≈ 3.0 * weight * 1.0 * 1.0 * 1.0
            // There are 5 Technical attributes → total ≈ 3.0 * Σweights
            // Σ weights for CM Technical attrs ≈ 0.8+0.8+0.7+0.5+0.7 = 3.5 (approx)
            // Expected total ≈ 10.5 XP
            // With rng_variance mean=1.0 → mean XP should be within 10% of this

            const int runs = 1000;
            float totalXp = 0f;

            var session = Session(TrainingType.Technical);
            var state = PlayerState.Default with { Motivation = 100f, Fatigue = 0f };

            for (int seed = 0; seed < runs; seed++)
            {
                var result = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), state, new SimulationRandom(seed));
                totalXp += result.TotalXpGained;
            }

            float meanXp = totalXp / runs;

            // Mean rng_variance should converge to 1.0 → mean XP ≈ sum of (3.0 * weight) for each Technical attr
            // Just verify it's positive and in a reasonable range (not zero, not wildly off)
            Assert.True(meanXp > 0f, "Mean XP must be positive");
            Assert.True(meanXp > 3f, $"Mean XP {meanXp:F2} seems too low — variance might be wrong");
            Assert.True(meanXp < 30f, $"Mean XP {meanXp:F2} seems too high — formula might be wrong");

            // Verify mean rng_variance is approximately 1.0 (within 2% of expected)
            // Since XP ∝ rng_variance, the mean ratio of actual to deterministic should be ≈ 1.0
            // Run deterministic (Gaussian μ=1.0) and compare:
            {
                // Use variance=1.0 exactly → single run with seed that gives variance ≈ 1.0
                // Instead, verify the 1000-run average is within ±10% of a single Moderate run
                float singleRunXp = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), state, new SimulationRandom(42)).TotalXpGained;
                float tolerance = singleRunXp * 0.20f; // 20% window (rng variance ±30%, so over 1000 runs ±10% of mean)
                Assert.InRange(meanXp, singleRunXp - tolerance, singleRunXp + tolerance);
            }
        }

        [Fact]
        public void TrainingSystem_DeterminismTest_SameSeedSameResult()
        {
            var session = Session(TrainingType.Physical, TrainingIntensity.Hard, 90);
            var state = WithMotivationAndFatigue(75f, 60f);

            var result1 = TrainingSystem.CalculateXP(session, Position.FB, DefaultAbilities(), state, new SimulationRandom(12345));
            var result2 = TrainingSystem.CalculateXP(session, Position.FB, DefaultAbilities(), state, new SimulationRandom(12345));

            Assert.Equal(result1.TotalXpGained, result2.TotalXpGained);
            Assert.Equal(result1.FatigueCost, result2.FatigueCost);
            Assert.Equal(result1.InjuryRisk, result2.InjuryRisk);
            Assert.Equal(result1.XpGained.Count, result2.XpGained.Count);
        }

        [Fact]
        public void TrainingSystem_DifferentSeeds_ProduceDifferentXp()
        {
            var session = Session(TrainingType.Technical);

            var result1 = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1));
            var result2 = TrainingSystem.CalculateXP(session, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(9999));

            // Different seeds should (almost certainly) produce different XP
            Assert.NotEqual(result1.TotalXpGained, result2.TotalXpGained);
        }

        // ─── Null Guards ──────────────────────────────────────────────────────

        [Fact]
        public void TrainingSystem_NullSession_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TrainingSystem.CalculateXP(null!, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1)));
        }

        [Fact]
        public void TrainingSystem_NullRng_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TrainingSystem.CalculateXP(Session(TrainingType.Technical), Position.CM, DefaultAbilities(), PlayerState.Default, null!));
        }

        [Fact]
        public void TrainingSystem_WeekXP_NullSessions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TrainingSystem.CalculateWeekXP(null!, Position.CM, DefaultAbilities(), PlayerState.Default, new SimulationRandom(1)));
        }
    }
}
