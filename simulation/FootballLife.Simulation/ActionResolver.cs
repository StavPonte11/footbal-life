using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure, stateless resolver that computes stochastic outcomes for player match actions.
    /// Incorporates player abilities, fatigue/form, situational pressure, and deterministic Gaussian rolls.
    /// </summary>
    public static class ActionResolver
    {
        /// <summary>
        /// Resolves a player's chosen action within a match situation.
        /// </summary>
        public static ActionOutcome Resolve(
            MatchAction action,
            MatchSituation situation,
            PlayerAbilities abilities,
            PlayerState state,
            SimulationRandom rng)
        {
            if (situation is null) throw new ArgumentNullException(nameof(situation));
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Effective ability score accounts for fatigue degradation
            float baseScore = ComputeWeightedAbilityScore(action, abilities, state);
            float stateScore = FatigueSystem.ComputeFormModifier(state); // in [0.7, 1.2]
            float situationModifier = 1.0f - (situation.OpponentPressure / 10f * 0.3f); // in [0.7, 1.0]

            float mean = baseScore * stateScore * situationModifier;
            float stddev = GetStandardDeviation(action);

            float roll = rng.NextGaussian(mean, stddev);
            float threshold = GetSuccessThreshold(action);
            bool success = roll > threshold;

            if (success)
            {
                float xpContribution = situation.ExpectedDifficulty * 10f;
                float confidenceDelta = rng.NextFloat(1.0f, 3.0f);

                OutcomeType type;
                float managerTrustDelta;

                switch (action)
                {
                    case MatchAction.Shot_Close:
                    case MatchAction.Shot_Long:
                    case MatchAction.Header:
                        type = OutcomeType.Goal;
                        managerTrustDelta = 5.0f;
                        break;

                    case MatchAction.GoalkeeperSave:
                    case MatchAction.ClaimCross:
                        type = OutcomeType.SaveMade;
                        managerTrustDelta = 3.0f;
                        break;

                    case MatchAction.Tackle:
                    case MatchAction.Press:
                        type = OutcomeType.TackleWon;
                        managerTrustDelta = rng.NextFloat(0.5f, 1.5f);
                        break;

                    case MatchAction.Interception:
                        type = OutcomeType.InterceptionWon;
                        managerTrustDelta = rng.NextFloat(0.5f, 1.5f);
                        break;

                    case MatchAction.ShortPass:
                    case MatchAction.LongPass:
                    case MatchAction.Cross:
                    case MatchAction.ThroughBall:
                    case MatchAction.DistributionPass:
                    case MatchAction.Dribble:
                    case MatchAction.CutInside:
                    default:
                        type = OutcomeType.PassCompleted;
                        managerTrustDelta = rng.NextFloat(0.5f, 1.5f);
                        break;
                }

                return new ActionOutcome(true, type, xpContribution, confidenceDelta, managerTrustDelta);
            }
            else
            {
                float failureMargin = Math.Max(0.5f, (threshold - roll) / 10f);
                float confidenceDelta = -rng.NextFloat(0.5f, 2.0f) * Math.Min(2.0f, failureMargin);

                OutcomeType type;
                float managerTrustDelta;

                switch (action)
                {
                    case MatchAction.Shot_Close:
                    case MatchAction.Shot_Long:
                    case MatchAction.Header:
                        type = OutcomeType.ChanceMissed;
                        managerTrustDelta = -rng.NextFloat(0.2f, 1.0f);
                        break;

                    case MatchAction.Tackle:
                        type = OutcomeType.TackleLost;
                        managerTrustDelta = -rng.NextFloat(0.5f, 1.5f);
                        break;

                    case MatchAction.GoalkeeperSave:
                        type = OutcomeType.ChanceMissed;
                        managerTrustDelta = -4.0f; // Serious error for GK
                        break;

                    default:
                        type = OutcomeType.Turnover;
                        managerTrustDelta = -rng.NextFloat(0.2f, 1.0f);
                        break;
                }

                return new ActionOutcome(false, type, 0.0f, confidenceDelta, managerTrustDelta);
            }
        }

        public static float ComputeWeightedAbilityScore(MatchAction action, PlayerAbilities abilities, PlayerState? state = null)
        {
            Func<AttributeName, float> getAttr = state != null
                ? attr => FatigueSystem.ComputeEffectiveAbility(abilities, state, attr)
                : attr => abilities.Get(attr);

            return action switch
            {
                MatchAction.ShortPass =>
                    getAttr(AttributeName.Passing) * 0.50f +
                    getAttr(AttributeName.DecisionMaking) * 0.30f +
                    getAttr(AttributeName.Composure) * 0.20f,

                MatchAction.LongPass =>
                    getAttr(AttributeName.Passing) * 0.50f +
                    getAttr(AttributeName.Vision) * 0.30f +
                    getAttr(AttributeName.FirstTouch) * 0.20f,

                MatchAction.Cross =>
                    getAttr(AttributeName.Crossing) * 0.60f +
                    getAttr(AttributeName.Vision) * 0.20f +
                    getAttr(AttributeName.DecisionMaking) * 0.20f,

                MatchAction.ThroughBall =>
                    getAttr(AttributeName.Passing) * 0.40f +
                    getAttr(AttributeName.Vision) * 0.40f +
                    getAttr(AttributeName.DecisionMaking) * 0.20f,

                MatchAction.Dribble =>
                    getAttr(AttributeName.Dribbling) * 0.50f +
                    getAttr(AttributeName.Agility) * 0.30f +
                    getAttr(AttributeName.Acceleration) * 0.20f,

                MatchAction.CutInside =>
                    getAttr(AttributeName.Dribbling) * 0.40f +
                    getAttr(AttributeName.Agility) * 0.30f +
                    getAttr(AttributeName.Acceleration) * 0.30f,

                MatchAction.Shot_Close =>
                    getAttr(AttributeName.Shooting) * 0.60f +
                    getAttr(AttributeName.Composure) * 0.25f +
                    getAttr(AttributeName.FirstTouch) * 0.15f,

                MatchAction.Shot_Long =>
                    getAttr(AttributeName.Shooting) * 0.55f +
                    getAttr(AttributeName.Strength) * 0.25f +
                    getAttr(AttributeName.Composure) * 0.20f,

                MatchAction.Header =>
                    getAttr(AttributeName.Strength) * 0.40f +
                    getAttr(AttributeName.Positioning) * 0.40f +
                    getAttr(AttributeName.Agility) * 0.20f,

                MatchAction.Tackle =>
                    getAttr(AttributeName.Tackling) * 0.60f +
                    getAttr(AttributeName.Strength) * 0.20f +
                    getAttr(AttributeName.Positioning) * 0.20f,

                MatchAction.Interception =>
                    getAttr(AttributeName.Positioning) * 0.50f +
                    getAttr(AttributeName.DecisionMaking) * 0.30f +
                    getAttr(AttributeName.Pace) * 0.20f,

                MatchAction.AerialChallenge =>
                    getAttr(AttributeName.Strength) * 0.50f +
                    getAttr(AttributeName.Positioning) * 0.30f +
                    getAttr(AttributeName.Agility) * 0.20f,

                MatchAction.Press =>
                    getAttr(AttributeName.Stamina) * 0.40f +
                    getAttr(AttributeName.Pace) * 0.30f +
                    getAttr(AttributeName.Tackling) * 0.30f,

                MatchAction.GoalkeeperSave =>
                    getAttr(AttributeName.Agility) * 0.40f +
                    getAttr(AttributeName.Positioning) * 0.35f +
                    getAttr(AttributeName.Composure) * 0.25f,

                MatchAction.ClaimCross =>
                    getAttr(AttributeName.Positioning) * 0.40f +
                    getAttr(AttributeName.Strength) * 0.35f +
                    getAttr(AttributeName.Composure) * 0.25f,

                MatchAction.DistributionPass =>
                    getAttr(AttributeName.Passing) * 0.60f +
                    getAttr(AttributeName.Vision) * 0.25f +
                    getAttr(AttributeName.DecisionMaking) * 0.15f,

                _ => abilities.CalculateAverage()
            };
        }

        public static float GetStandardDeviation(MatchAction action) => action switch
        {
            MatchAction.ShortPass or MatchAction.DistributionPass => 5.0f,
            MatchAction.Cross or MatchAction.Dribble or MatchAction.Tackle or MatchAction.Header => 10.0f,
            MatchAction.Shot_Long or MatchAction.ThroughBall or MatchAction.Shot_Close => 15.0f,
            _ => 10.0f
        };

        public static float GetSuccessThreshold(MatchAction action) => action switch
        {
            MatchAction.ShortPass => 68.0f,
            MatchAction.LongPass => 55.0f,
            MatchAction.Cross => 55.0f,
            MatchAction.Dribble => 60.0f,
            MatchAction.ThroughBall => 48.0f,
            MatchAction.Shot_Close => 62.0f,
            MatchAction.Shot_Long => 38.0f,
            MatchAction.Header => 55.0f,
            MatchAction.Tackle => 60.0f,
            MatchAction.Interception => 55.0f,
            MatchAction.AerialChallenge => 60.0f,
            MatchAction.GoalkeeperSave => 55.0f,
            MatchAction.ClaimCross => 58.0f,
            MatchAction.DistributionPass => 65.0f,
            MatchAction.Press => 55.0f,
            MatchAction.CutInside => 56.0f,
            _ => 50.0f
        };
    }
}
