using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FootballLife.Domain
{
    /// <summary>
    /// Provides position- and situation-aware attribute relevance queries for match simulation
    /// and overall ability-to-position scoring.
    /// </summary>
    public static class AttributeRelevanceMatrix
    {
        /// <summary>
        /// Retrieves the evaluated weighted attributes for a position and optional situation context.
        /// </summary>
        public static AttributeRelevance Get(Position position, SituationType? situation = null)
        {
            var baseWeights = PositionWeightMap.For(position);
            var result = new List<(AttributeName Attribute, float Weight)>(15);

            foreach (var kvp in baseWeights)
            {
                float weight = kvp.Value;

                if (situation.HasValue)
                {
                    weight = ApplySituationModifier(kvp.Key, weight, situation.Value);
                }

                result.Add((kvp.Key, weight));
            }

            // Order by weight descending for convenient consumption
            result.Sort((a, b) => b.Weight.CompareTo(a.Weight));

            return new AttributeRelevance(position, situation, result.AsReadOnly());
        }

        private static float ApplySituationModifier(AttributeName attribute, float baseWeight, SituationType situation)
        {
            float modifier = situation switch
            {
                SituationType.CounterAttack => attribute switch
                {
                    AttributeName.Pace or AttributeName.Acceleration => 1.3f,
                    AttributeName.DecisionMaking or AttributeName.Passing => 1.15f,
                    _ => 1.0f
                },
                SituationType.DefensiveTransition => attribute switch
                {
                    AttributeName.Tackling or AttributeName.Positioning => 1.3f,
                    AttributeName.Pace or AttributeName.Stamina => 1.15f,
                    _ => 1.0f
                },
                SituationType.SetPiece => attribute switch
                {
                    AttributeName.Crossing or AttributeName.Positioning => 1.3f,
                    AttributeName.Strength or AttributeName.Composure => 1.2f,
                    _ => 0.9f
                },
                SituationType.Penalty => attribute switch
                {
                    AttributeName.Shooting or AttributeName.Composure => 1.5f,
                    _ => 0.5f
                },
                SituationType.OneOnOne => attribute switch
                {
                    AttributeName.Dribbling or AttributeName.Agility or AttributeName.Composure => 1.35f,
                    _ => 0.85f
                },
                _ => 1.0f
            };

            return Math.Min(1.0f, baseWeight * modifier);
        }

        /// <summary>
        /// Computes a position relevance score for the given player abilities.
        /// </summary>
        public static float ComputeScore(PlayerAbilities abilities, Position position)
        {
            var weights = PositionWeightMap.For(position);
            return ComputeScore(abilities, weights);
        }

        /// <summary>
        /// Computes a relevance score using a pre-fetched weight dictionary.
        /// </summary>
        public static float ComputeScore(PlayerAbilities abilities, ReadOnlyDictionary<AttributeName, float> weights)
        {
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (weights is null)   throw new ArgumentNullException(nameof(weights));

            float weightedSum = 0f;
            float totalWeight = 0f;

            foreach (var kvp in weights)
            {
                float attributeValue = GetNormalisedValue(abilities, kvp.Key);
                weightedSum += attributeValue * kvp.Value;
                totalWeight += kvp.Value;
            }

            if (totalWeight <= 0f) return 0f;

            float raw = weightedSum / totalWeight;
            return Math.Max(0f, Math.Min(1f, raw));
        }

        /// <summary>
        /// Computes relevance scores for all nine positions and returns them as
        /// an ordered dictionary keyed by <see cref="Position"/>.
        /// </summary>
        public static ReadOnlyDictionary<Position, float> ComputeAllPositions(PlayerAbilities abilities)
        {
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));

            var result = new Dictionary<Position, float>(9);
            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                result[pos] = ComputeScore(abilities, pos);
            }
            return new ReadOnlyDictionary<Position, float>(result);
        }

        /// <summary>
        /// Returns the position for which the player's abilities yield the highest relevance score.
        /// </summary>
        public static Position BestFitPosition(PlayerAbilities abilities)
        {
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));

            Position bestPos = Position.GK;
            float bestScore = -1f;

            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                float score = ComputeScore(abilities, pos);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = pos;
                }
            }

            return bestPos;
        }

        private static float GetNormalisedValue(PlayerAbilities abilities, AttributeName attribute)
        {
            byte raw = attribute switch
            {
                AttributeName.Pace           => abilities.Pace,
                AttributeName.Acceleration   => abilities.Acceleration,
                AttributeName.Stamina        => abilities.Stamina,
                AttributeName.Strength       => abilities.Strength,
                AttributeName.Agility        => abilities.Agility,
                AttributeName.Passing        => abilities.Passing,
                AttributeName.Shooting       => abilities.Shooting,
                AttributeName.Dribbling      => abilities.Dribbling,
                AttributeName.Crossing       => abilities.Crossing,
                AttributeName.FirstTouch     => abilities.FirstTouch,
                AttributeName.Tackling       => abilities.Tackling,
                AttributeName.Vision         => abilities.Vision,
                AttributeName.Composure      => abilities.Composure,
                AttributeName.Positioning    => abilities.Positioning,
                AttributeName.DecisionMaking => abilities.DecisionMaking,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown attribute.")
            };

            return raw / 100f;
        }
    }
}
