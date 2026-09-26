using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Generates position-authentic match situations for player interaction during matches.
    /// Employs static readonly situation and action tables to maintain zero heap allocations in hot paths.
    /// </summary>
    public static class MatchSituationGenerator
    {
        // Baseline situation generation probability per minute
        private const float OutfieldBaseProbability = 2f / 90f;   // ~2 situations per 90 mins
        private const float GoalkeeperBaseProbability = 1.5f / 90f; // ~1.5 situations per 90 mins

        // Position-specific situation distribution tables (cumulative percent [0..100])
        // ST: RunningInBehind (40%), ReceivingInBox (30%), LongShot (15%), Pressing (10%), AerialChallenge (5%)
        private static readonly (SituationType Type, int Weight)[] StrikerTable = new[]
        {
            (SituationType.RunningInBehind, 40),
            (SituationType.ReceivingInBox, 30),
            (SituationType.LongShot, 15),
            (SituationType.Pressing, 10),
            (SituationType.AerialChallenge, 5)
        };

        // LW/RW: ReceiveBall1v1 (35%), Cross (25%), CutInside (20%), Pressing (15%), Tackle (5%)
        private static readonly (SituationType Type, int Weight)[] WingerTable = new[]
        {
            (SituationType.ReceiveBall1v1, 35),
            (SituationType.Cross, 25),
            (SituationType.CutInside, 20),
            (SituationType.Pressing, 15),
            (SituationType.Tackle, 5)
        };

        // CM: ReceiveUnderPressure (30%), ThroughBall (25%), LongPass (20%), Pressing (15%), Tackle (10%)
        private static readonly (SituationType Type, int Weight)[] CentralMidfieldTable = new[]
        {
            (SituationType.ReceiveUnderPressure, 30),
            (SituationType.ThroughBall, 25),
            (SituationType.LongPass, 20),
            (SituationType.Pressing, 15),
            (SituationType.Tackle, 10)
        };

        // DM: Interception (30%), Tackle (30%), DistributionPass (25%), PositioningRun (15%)
        private static readonly (SituationType Type, int Weight)[] DefensiveMidfieldTable = new[]
        {
            (SituationType.Interception, 30),
            (SituationType.Tackle, 30),
            (SituationType.DistributionPass, 25),
            (SituationType.PositioningRun, 15)
        };

        // CB: Tackle (35%), AerialChallenge (25%), Interception (20%), BuildUpPass (20%)
        private static readonly (SituationType Type, int Weight)[] CentreBackTable = new[]
        {
            (SituationType.Tackle, 35),
            (SituationType.AerialChallenge, 25),
            (SituationType.Interception, 20),
            (SituationType.BuildUpPass, 20)
        };

        // Fullbacks (LB / RB)
        private static readonly (SituationType Type, int Weight)[] FullbackTable = new[]
        {
            (SituationType.Tackle, 30),
            (SituationType.Cross, 25),
            (SituationType.Interception, 20),
            (SituationType.ReceiveBall1v1, 15),
            (SituationType.Pressing, 10)
        };

        // Attacking Midfield (CAM)
        private static readonly (SituationType Type, int Weight)[] AttackingMidfieldTable = new[]
        {
            (SituationType.ThroughBall, 30),
            (SituationType.RunningInBehind, 25),
            (SituationType.ReceivingInBox, 20),
            (SituationType.LongShot, 15),
            (SituationType.Pressing, 10)
        };

        // GK: Save (50%), ClaimCross (25%), Distribution (25%)
        private static readonly (SituationType Type, int Weight)[] GoalkeeperTable = new[]
        {
            (SituationType.Save, 50),
            (SituationType.ClaimCross, 25),
            (SituationType.Distribution, 25)
        };

        // Pre-allocated static readonly choice arrays per situation
        private static readonly ActionChoice[] RunningInBehindChoices = new[]
        {
            new ActionChoice(MatchAction.Shot_Close, 0.6f, 0.85f),
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.65f),
            new ActionChoice(MatchAction.Dribble, 0.5f, 0.70f)
        };

        private static readonly ActionChoice[] ReceivingInBoxChoices = new[]
        {
            new ActionChoice(MatchAction.Shot_Close, 0.5f, 0.80f),
            new ActionChoice(MatchAction.Header, 0.4f, 0.75f),
            new ActionChoice(MatchAction.ShortPass, 0.25f, 0.60f)
        };

        private static readonly ActionChoice[] LongShotChoices = new[]
        {
            new ActionChoice(MatchAction.Shot_Long, 0.7f, 0.60f),
            new ActionChoice(MatchAction.ThroughBall, 0.5f, 0.70f),
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.50f)
        };

        private static readonly ActionChoice[] PressingChoices = new[]
        {
            new ActionChoice(MatchAction.Press, 0.3f, 0.60f),
            new ActionChoice(MatchAction.Tackle, 0.5f, 0.70f)
        };

        private static readonly ActionChoice[] AerialChallengeChoices = new[]
        {
            new ActionChoice(MatchAction.AerialChallenge, 0.4f, 0.65f),
            new ActionChoice(MatchAction.Header, 0.4f, 0.65f)
        };

        private static readonly ActionChoice[] ReceiveBall1v1Choices = new[]
        {
            new ActionChoice(MatchAction.Dribble, 0.5f, 0.75f),
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.55f),
            new ActionChoice(MatchAction.Cross, 0.4f, 0.65f)
        };

        private static readonly ActionChoice[] CrossChoices = new[]
        {
            new ActionChoice(MatchAction.Cross, 0.45f, 0.75f),
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.55f),
            new ActionChoice(MatchAction.CutInside, 0.5f, 0.70f)
        };

        private static readonly ActionChoice[] CutInsideChoices = new[]
        {
            new ActionChoice(MatchAction.Shot_Long, 0.6f, 0.70f),
            new ActionChoice(MatchAction.ThroughBall, 0.5f, 0.75f),
            new ActionChoice(MatchAction.ShortPass, 0.25f, 0.55f)
        };

        private static readonly ActionChoice[] ReceiveUnderPressureChoices = new[]
        {
            new ActionChoice(MatchAction.ShortPass, 0.3f, 0.60f),
            new ActionChoice(MatchAction.Dribble, 0.55f, 0.65f),
            new ActionChoice(MatchAction.LongPass, 0.45f, 0.60f)
        };

        private static readonly ActionChoice[] ThroughBallChoices = new[]
        {
            new ActionChoice(MatchAction.ThroughBall, 0.55f, 0.80f),
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.55f),
            new ActionChoice(MatchAction.LongPass, 0.4f, 0.65f)
        };

        private static readonly ActionChoice[] LongPassChoices = new[]
        {
            new ActionChoice(MatchAction.LongPass, 0.45f, 0.70f),
            new ActionChoice(MatchAction.ShortPass, 0.15f, 0.50f)
        };

        private static readonly ActionChoice[] InterceptionChoices = new[]
        {
            new ActionChoice(MatchAction.Interception, 0.4f, 0.70f),
            new ActionChoice(MatchAction.Tackle, 0.5f, 0.65f)
        };

        private static readonly ActionChoice[] TackleChoices = new[]
        {
            new ActionChoice(MatchAction.Tackle, 0.45f, 0.70f),
            new ActionChoice(MatchAction.Interception, 0.35f, 0.60f)
        };

        private static readonly ActionChoice[] DistributionPassChoices = new[]
        {
            new ActionChoice(MatchAction.DistributionPass, 0.2f, 0.60f),
            new ActionChoice(MatchAction.LongPass, 0.45f, 0.65f),
            new ActionChoice(MatchAction.ShortPass, 0.15f, 0.55f)
        };

        private static readonly ActionChoice[] PositioningRunChoices = new[]
        {
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.60f),
            new ActionChoice(MatchAction.ThroughBall, 0.5f, 0.70f)
        };

        private static readonly ActionChoice[] BuildUpPassChoices = new[]
        {
            new ActionChoice(MatchAction.ShortPass, 0.15f, 0.55f),
            new ActionChoice(MatchAction.LongPass, 0.4f, 0.65f)
        };

        private static readonly ActionChoice[] SaveChoices = new[]
        {
            new ActionChoice(MatchAction.GoalkeeperSave, 0.5f, 0.80f)
        };

        private static readonly ActionChoice[] ClaimCrossChoices = new[]
        {
            new ActionChoice(MatchAction.ClaimCross, 0.4f, 0.75f),
            new ActionChoice(MatchAction.GoalkeeperSave, 0.3f, 0.60f)
        };

        private static readonly ActionChoice[] DistributionChoices = new[]
        {
            new ActionChoice(MatchAction.DistributionPass, 0.2f, 0.60f),
            new ActionChoice(MatchAction.LongPass, 0.45f, 0.65f)
        };

        // Phase 8.1 — New situation choice arrays
        private static readonly ActionChoice[] FreeKickChoices = new[]
        {
            new ActionChoice(MatchAction.FreeKick_Direct, 0.7f, 0.75f),
            new ActionChoice(MatchAction.FreeKick_Cross, 0.3f, 0.60f),
            new ActionChoice(MatchAction.ShortPass, 0.1f, 0.40f)
        };

        private static readonly ActionChoice[] PenaltyKickChoices = new[]
        {
            new ActionChoice(MatchAction.PenaltyKick, 0.5f, 0.90f)
        };

        private static readonly ActionChoice[] CornerKickChoices = new[]
        {
            new ActionChoice(MatchAction.CornerDelivery, 0.4f, 0.65f),
            new ActionChoice(MatchAction.Cross, 0.35f, 0.60f),
            new ActionChoice(MatchAction.ShortPass, 0.15f, 0.45f)
        };

        private static readonly ActionChoice[] Dribbling1v1Choices = new[]
        {
            new ActionChoice(MatchAction.SkillMove, 0.65f, 0.80f),
            new ActionChoice(MatchAction.Dribble, 0.5f, 0.70f),
            new ActionChoice(MatchAction.ShortPass, 0.15f, 0.50f)
        };

        private static readonly ActionChoice[] HeaderOpportunityChoices = new[]
        {
            new ActionChoice(MatchAction.Header, 0.5f, 0.75f),
            new ActionChoice(MatchAction.DivingHeader, 0.7f, 0.85f),
            new ActionChoice(MatchAction.AerialChallenge, 0.35f, 0.60f)
        };

        private static readonly ActionChoice[] CounterAttackRunChoices = new[]
        {
            new ActionChoice(MatchAction.Dribble, 0.5f, 0.75f),
            new ActionChoice(MatchAction.ThroughBall, 0.45f, 0.80f),
            new ActionChoice(MatchAction.Shot_Long, 0.6f, 0.65f)
        };

        private static readonly ActionChoice[] GKOneOnOneChoices = new[]
        {
            new ActionChoice(MatchAction.Shot_Close, 0.5f, 0.85f),
            new ActionChoice(MatchAction.ChipShot, 0.7f, 0.80f),
            new ActionChoice(MatchAction.Dribble, 0.6f, 0.70f)
        };

        // Defensive situation choice arrays
        private static readonly ActionChoice[] DefensiveSlidingTackleChoices = new[]
        {
            new ActionChoice(MatchAction.SlidingTackle, 0.6f, 0.70f),
            new ActionChoice(MatchAction.Tackle, 0.4f, 0.65f),
            new ActionChoice(MatchAction.Interception, 0.3f, 0.55f)
        };

        private static readonly ActionChoice[] BlockShotChoices = new[]
        {
            new ActionChoice(MatchAction.BlockShot, 0.5f, 0.75f),
            new ActionChoice(MatchAction.Tackle, 0.4f, 0.60f)
        };

        private static readonly ActionChoice[] GKRushChoices = new[]
        {
            new ActionChoice(MatchAction.GoalkeeperRush, 0.6f, 0.70f),
            new ActionChoice(MatchAction.GoalkeeperSave, 0.4f, 0.75f)
        };

        private static readonly ActionChoice[] DefaultChoices = new[]
        {
            new ActionChoice(MatchAction.ShortPass, 0.2f, 0.50f)
        };

        /// <summary>
        /// Generates a match situation for the player during a single minute tick.
        /// Returns null if no player interaction occurs this minute.
        /// Integrates set pieces (FK, PK, CK) and special situations while preserving match pacing.
        /// </summary>
        public static MatchSituation? GenerateSituation(
            MatchState state,
            Position playerPosition,
            PlayerState playerState,
            SimulationRandom rng,
            bool playerIsHome = true)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (playerState is null) throw new ArgumentNullException(nameof(playerState));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            bool isGoalkeeper = playerPosition == Position.GK;
            int scoreDiff = playerIsHome ? state.HomeScore - state.AwayScore : state.AwayScore - state.HomeScore;

            // ── Baseline situation pacing roll ──
            float baseChance = isGoalkeeper ? GoalkeeperBaseProbability : OutfieldBaseProbability;

            float chanceMultiplier = 1.0f;
            if (scoreDiff <= -2)
            {
                chanceMultiplier *= isGoalkeeper ? 1.1f : 1.20f;
            }
            else if (scoreDiff >= 1 && state.Minute >= 85)
            {
                chanceMultiplier *= 0.70f;
            }

            float finalChance = baseChance * chanceMultiplier;
            if (!rng.NextBool(finalChance))
            {
                return null;
            }

            // ── A situation occurs this minute! Determine the situation type ──
            if (!isGoalkeeper)
            {
                // 1. Set pieces (PK, FK, CK)
                var setPiece = TryGenerateSetPiece(playerPosition, state, playerState, rng, scoreDiff);
                if (setPiece != null)
                {
                    return setPiece;
                }

                // 2. Special situations (breakaway, counter-attack, dribble 1v1, header)
                var special = TryGenerateSpecialSituation(playerPosition, state, playerState, rng, scoreDiff, isGoalkeeper);
                if (special != null)
                {
                    return special;
                }
            }

            // 3. Standard position-weighted situation
            var table = GetSituationTable(playerPosition);
            SituationType situationType = PickFromTable(table, rng);

            float opponentPressure = rng.NextFloat(2.5f, 6.5f);
            if (playerState.Fatigue > 70f)
            {
                opponentPressure += 2.0f;
            }
            opponentPressure = Math.Min(10f, Math.Max(0f, opponentPressure));

            float baseDifficulty = GetBaseDifficulty(situationType);

            float expectedDifficulty = Math.Min(1.0f, Math.Max(0.0f, baseDifficulty + (opponentPressure / 10f * 0.25f)));
            float positionalAdvantage = Math.Min(1.0f, Math.Max(-1.0f, rng.NextFloat(-0.5f, 0.5f) + (scoreDiff > 0 ? 0.1f : -0.1f)));

            var choices = GetChoicesForSituation(situationType);

            return new MatchSituation(
                type: situationType,
                opponentPressure: opponentPressure,
                expectedDifficulty: expectedDifficulty,
                positionalAdvantage: positionalAdvantage,
                availableChoices: choices);
        }

        /// <summary>
        /// Attempts to select a set piece situation (free kick, penalty, corner) when an interaction is triggered.
        /// Returns null if this situation is not a set piece.
        /// </summary>
        private static MatchSituation? TryGenerateSetPiece(
            Position playerPosition,
            MatchState state,
            PlayerState playerState,
            SimulationRandom rng,
            int scoreDiff)
        {
            // Set piece takers: attacking midfielders, wingers, and strikers
            bool isSetPieceTaker = playerPosition == Position.AM || playerPosition == Position.ST ||
                                   playerPosition == Position.LW || playerPosition == Position.RW;

            if (isSetPieceTaker)
            {
                float roll = rng.NextFloat(0f, 1f);
                if (roll < 0.03f)
                {
                    // Penalty kick (~3% of attacking situations)
                    return BuildSituation(SituationType.PenaltyKick, rng, playerState, scoreDiff,
                        basePressure: 3.0f, pressureRange: 4.0f);
                }
                if (roll < 0.11f)
                {
                    // Direct/Crossing Free Kick (~8% of attacking situations)
                    return BuildSituation(SituationType.FreeKick, rng, playerState, scoreDiff,
                        basePressure: 2.0f, pressureRange: 4.0f);
                }
            }

            // Corner kicks
            bool isCornerTaker = playerPosition == Position.AM || playerPosition == Position.LW ||
                                 playerPosition == Position.RW || playerPosition == Position.CM;
            if (isCornerTaker && rng.NextBool(0.07f))
            {
                return BuildSituation(SituationType.CornerKick, rng, playerState, scoreDiff,
                    basePressure: 2.5f, pressureRange: 3.5f);
            }

            return null;
        }

        /// <summary>
        /// Attempts to select a special situation (counter-attack, breakaway 1v1, dribbling 1v1, header).
        /// </summary>
        private static MatchSituation? TryGenerateSpecialSituation(
            Position playerPosition,
            MatchState state,
            PlayerState playerState,
            SimulationRandom rng,
            int scoreDiff,
            bool isGoalkeeper)
        {
            if (isGoalkeeper) return null;

            bool isAttacker = playerPosition == Position.ST || playerPosition == Position.LW ||
                              playerPosition == Position.RW || playerPosition == Position.AM;

            if (isAttacker)
            {
                float roll = rng.NextFloat(0f, 1f);
                // GK one-on-one breakaway: ~5%
                if (roll < 0.05f)
                {
                    return BuildSituation(SituationType.GKOneOnOne, rng, playerState, scoreDiff,
                        basePressure: 1.0f, pressureRange: 3.0f);
                }
                // Counter-attack run: ~8% (boosted when trailing)
                float counterThreshold = scoreDiff < 0 ? 0.16f : 0.13f;
                if (roll < counterThreshold)
                {
                    return BuildSituation(SituationType.CounterAttackRun, rng, playerState, scoreDiff,
                        basePressure: 1.5f, pressureRange: 3.5f);
                }
                // Dribbling 1v1 take-on: ~9%
                if (roll < counterThreshold + 0.09f)
                {
                    return BuildSituation(SituationType.Dribbling1v1, rng, playerState, scoreDiff,
                        basePressure: 3.0f, pressureRange: 4.0f);
                }
            }

            // Header opportunity: for aerial-strong players (ST, CB)
            bool isAerialPlayer = playerPosition == Position.ST || playerPosition == Position.CB;
            if (isAerialPlayer && rng.NextBool(0.07f))
            {
                return BuildSituation(SituationType.HeaderOpportunity, rng, playerState, scoreDiff,
                    basePressure: 3.0f, pressureRange: 3.5f);
            }

            return null;
        }

        /// <summary>
        /// Builds a MatchSituation from a type with configurable pressure parameters.
        /// </summary>
        private static MatchSituation BuildSituation(
            SituationType type,
            SimulationRandom rng,
            PlayerState playerState,
            int scoreDiff,
            float basePressure,
            float pressureRange)
        {
            float opponentPressure = rng.NextFloat(basePressure, basePressure + pressureRange);
            if (playerState.Fatigue > 70f)
            {
                opponentPressure += 1.5f;
            }
            opponentPressure = Math.Min(10f, Math.Max(0f, opponentPressure));

            float baseDifficulty = GetBaseDifficulty(type);
            float expectedDifficulty = Math.Min(1.0f, Math.Max(0.0f, baseDifficulty + (opponentPressure / 10f * 0.25f)));
            float positionalAdvantage = Math.Min(1.0f, Math.Max(-1.0f, rng.NextFloat(-0.3f, 0.5f) + (scoreDiff > 0 ? 0.1f : -0.1f)));

            return new MatchSituation(
                type: type,
                opponentPressure: opponentPressure,
                expectedDifficulty: expectedDifficulty,
                positionalAdvantage: positionalAdvantage,
                availableChoices: GetChoicesForSituation(type));
        }

        private static float GetBaseDifficulty(SituationType type) => type switch
        {
            SituationType.RunningInBehind or SituationType.ReceivingInBox => 0.65f,
            SituationType.LongShot => 0.75f,
            SituationType.ThroughBall => 0.60f,
            SituationType.Save => 0.70f,
            SituationType.Tackle or SituationType.Interception => 0.55f,
            SituationType.FreeKick => 0.70f,
            SituationType.PenaltyKick => 0.40f,
            SituationType.CornerKick => 0.55f,
            SituationType.Dribbling1v1 => 0.60f,
            SituationType.HeaderOpportunity => 0.65f,
            SituationType.CounterAttackRun => 0.55f,
            SituationType.GKOneOnOne => 0.50f,
            _ => 0.45f
        };

        private static (SituationType Type, int Weight)[] GetSituationTable(Position position) => position switch
        {
            Position.ST => StrikerTable,
            Position.LW or Position.RW => WingerTable,
            Position.CM => CentralMidfieldTable,
            Position.DM => DefensiveMidfieldTable,
            Position.CB => CentreBackTable,
            Position.FB => FullbackTable,
            Position.AM => AttackingMidfieldTable,
            Position.GK => GoalkeeperTable,
            _ => CentralMidfieldTable
        };

        private static SituationType PickFromTable((SituationType Type, int Weight)[] table, SimulationRandom rng)
        {
            int totalWeight = 0;
            for (int i = 0; i < table.Length; i++)
            {
                totalWeight += table[i].Weight;
            }

            int roll = rng.NextInt(0, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < table.Length; i++)
            {
                cumulative += table[i].Weight;
                if (roll < cumulative)
                {
                    return table[i].Type;
                }
            }

            return table[0].Type;
        }

        public static ActionChoice[] GetChoicesForSituation(SituationType type) => type switch
        {
            SituationType.RunningInBehind => RunningInBehindChoices,
            SituationType.ReceivingInBox => ReceivingInBoxChoices,
            SituationType.LongShot => LongShotChoices,
            SituationType.Pressing => PressingChoices,
            SituationType.AerialChallenge => AerialChallengeChoices,
            SituationType.ReceiveBall1v1 => ReceiveBall1v1Choices,
            SituationType.Cross => CrossChoices,
            SituationType.CutInside => CutInsideChoices,
            SituationType.ReceiveUnderPressure => ReceiveUnderPressureChoices,
            SituationType.ThroughBall => ThroughBallChoices,
            SituationType.LongPass => LongPassChoices,
            SituationType.Interception => InterceptionChoices,
            SituationType.Tackle => TackleChoices,
            SituationType.DistributionPass => DistributionPassChoices,
            SituationType.PositioningRun => PositioningRunChoices,
            SituationType.BuildUpPass => BuildUpPassChoices,
            SituationType.Save => SaveChoices,
            SituationType.ClaimCross => ClaimCrossChoices,
            SituationType.Distribution => DistributionChoices,
            // Phase 8.1 — New situations
            SituationType.FreeKick => FreeKickChoices,
            SituationType.PenaltyKick => PenaltyKickChoices,
            SituationType.CornerKick => CornerKickChoices,
            SituationType.Dribbling1v1 => Dribbling1v1Choices,
            SituationType.HeaderOpportunity => HeaderOpportunityChoices,
            SituationType.CounterAttackRun => CounterAttackRunChoices,
            SituationType.GKOneOnOne => GKOneOnOneChoices,
            _ => DefaultChoices
        };
    }
}
