using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FootballLife.Domain
{
    /// <summary>
    /// Provides data-driven attribute weight profiles for each on-pitch position.
    /// Supports both raw domain relevance scale [0.0, 1.0] and normalized weights that sum to 1.0.
    /// </summary>
    public static class PositionWeightMap
    {
        private static readonly Dictionary<Position, ReadOnlyDictionary<AttributeName, float>> RawWeights = new();
        private static readonly Dictionary<Position, ReadOnlyDictionary<AttributeName, float>> NormalizedWeights = new();

        static PositionWeightMap()
        {
            InitializeDefaults();
        }

        /// <summary>
        /// Gets the raw importance weight of an attribute for a given position [0.0, 1.0].
        /// </summary>
        public static float GetWeight(Position position, AttributeName attribute)
        {
            if (!RawWeights.TryGetValue(position, out var map))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown position.");
            }

            if (!map.TryGetValue(attribute, out var weight))
            {
                throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown attribute.");
            }

            return weight;
        }

        /// <summary>
        /// Gets the normalized weight of an attribute for a given position such that the sum of all 15 attributes equals 1.0.
        /// </summary>
        public static float GetNormalizedWeight(Position position, AttributeName attribute)
        {
            if (!NormalizedWeights.TryGetValue(position, out var map))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown position.");
            }

            if (!map.TryGetValue(attribute, out var weight))
            {
                throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown attribute.");
            }

            return weight;
        }

        /// <summary>
        /// Returns the full raw weight map for a given position.
        /// </summary>
        public static ReadOnlyDictionary<AttributeName, float> For(Position position)
        {
            if (!RawWeights.TryGetValue(position, out var map))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown position.");
            }
            return map;
        }

        /// <summary>
        /// Returns the normalized weight map for a given position (sum of weights = 1.0).
        /// </summary>
        public static ReadOnlyDictionary<AttributeName, float> GetNormalizedWeights(Position position)
        {
            if (!NormalizedWeights.TryGetValue(position, out var map))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown position.");
            }
            return map;
        }

        /// <summary>
        /// Verifies whether normalized weights for a given position sum to 1.0 within float tolerance.
        /// </summary>
        public static bool WeightsSumToOne(Position position)
        {
            var weights = GetNormalizedWeights(position);
            float sum = 0f;
            foreach (var w in weights.Values)
            {
                sum += w;
            }
            return Math.Abs(sum - 1.0f) < 0.001f;
        }

        /// <summary>
        /// Loads position weight configurations from a JSON string.
        /// </summary>
        public static void LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON content cannot be null or empty.", nameof(json));

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("positions", out var positionsElement) || positionsElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("Invalid JSON format: missing 'positions' array.");
            }

            foreach (var elem in positionsElement.EnumerateArray())
            {
                string posStr = elem.GetProperty("position").GetString()!;
                if (!Enum.TryParse<Position>(posStr, out var pos)) continue;

                var weightsElem = elem.GetProperty("weights");
                var dict = new Dictionary<AttributeName, float>(15);
                foreach (var prop in weightsElem.EnumerateObject())
                {
                    if (Enum.TryParse<AttributeName>(prop.Name, out var attr))
                    {
                        dict[attr] = (float)prop.Value.GetDouble();
                    }
                }

                if (dict.Count == 15)
                {
                    SetPositionWeights(pos, dict);
                }
            }
        }

        /// <summary>
        /// Loads position weights from file path if it exists.
        /// </summary>
        public static bool TryLoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                string json = File.ReadAllText(filePath);
                LoadFromJson(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SetPositionWeights(Position position, Dictionary<AttributeName, float> weights)
        {
            RawWeights[position] = new ReadOnlyDictionary<AttributeName, float>(new Dictionary<AttributeName, float>(weights));

            float sum = 0f;
            foreach (var v in weights.Values) sum += v;
            if (sum <= 0f) sum = 1f;

            var norm = new Dictionary<AttributeName, float>(15);
            foreach (var kvp in weights)
            {
                norm[kvp.Key] = kvp.Value / sum;
            }
            NormalizedWeights[position] = new ReadOnlyDictionary<AttributeName, float>(norm);
        }

        private static void InitializeDefaults()
        {
            RegisterPosition(Position.GK,
                (AttributeName.Pace, 0.30f), (AttributeName.Acceleration, 0.25f), (AttributeName.Stamina, 0.40f),
                (AttributeName.Strength, 0.50f), (AttributeName.Agility, 0.85f), (AttributeName.Passing, 0.50f),
                (AttributeName.Shooting, 0.05f), (AttributeName.Dribbling, 0.10f), (AttributeName.Crossing, 0.05f),
                (AttributeName.FirstTouch, 0.40f), (AttributeName.Tackling, 0.20f), (AttributeName.Vision, 0.70f),
                (AttributeName.Composure, 1.00f), (AttributeName.Positioning, 1.00f), (AttributeName.DecisionMaking, 0.90f));

            RegisterPosition(Position.CB,
                (AttributeName.Pace, 0.65f), (AttributeName.Acceleration, 0.55f), (AttributeName.Stamina, 0.70f),
                (AttributeName.Strength, 1.00f), (AttributeName.Agility, 0.50f), (AttributeName.Passing, 0.60f),
                (AttributeName.Shooting, 0.10f), (AttributeName.Dribbling, 0.20f), (AttributeName.Crossing, 0.10f),
                (AttributeName.FirstTouch, 0.55f), (AttributeName.Tackling, 1.00f), (AttributeName.Vision, 0.65f),
                (AttributeName.Composure, 0.80f), (AttributeName.Positioning, 1.00f), (AttributeName.DecisionMaking, 0.85f));

            RegisterPosition(Position.FB,
                (AttributeName.Pace, 1.00f), (AttributeName.Acceleration, 0.95f), (AttributeName.Stamina, 1.00f),
                (AttributeName.Strength, 0.60f), (AttributeName.Agility, 0.80f), (AttributeName.Passing, 0.70f),
                (AttributeName.Shooting, 0.20f), (AttributeName.Dribbling, 0.55f), (AttributeName.Crossing, 0.90f),
                (AttributeName.FirstTouch, 0.65f), (AttributeName.Tackling, 0.85f), (AttributeName.Vision, 0.60f),
                (AttributeName.Composure, 0.60f), (AttributeName.Positioning, 0.80f), (AttributeName.DecisionMaking, 0.75f));

            RegisterPosition(Position.DM,
                (AttributeName.Pace, 0.55f), (AttributeName.Acceleration, 0.50f), (AttributeName.Stamina, 1.00f),
                (AttributeName.Strength, 0.80f), (AttributeName.Agility, 0.65f), (AttributeName.Passing, 0.80f),
                (AttributeName.Shooting, 0.25f), (AttributeName.Dribbling, 0.45f), (AttributeName.Crossing, 0.25f),
                (AttributeName.FirstTouch, 0.75f), (AttributeName.Tackling, 1.00f), (AttributeName.Vision, 0.85f),
                (AttributeName.Composure, 0.80f), (AttributeName.Positioning, 0.90f), (AttributeName.DecisionMaking, 1.00f));

            RegisterPosition(Position.CM,
                (AttributeName.Pace, 0.55f), (AttributeName.Acceleration, 0.50f), (AttributeName.Stamina, 1.00f),
                (AttributeName.Strength, 0.65f), (AttributeName.Agility, 0.70f), (AttributeName.Passing, 1.00f),
                (AttributeName.Shooting, 0.50f), (AttributeName.Dribbling, 0.60f), (AttributeName.Crossing, 0.40f),
                (AttributeName.FirstTouch, 0.90f), (AttributeName.Tackling, 0.70f), (AttributeName.Vision, 1.00f),
                (AttributeName.Composure, 0.80f), (AttributeName.Positioning, 0.80f), (AttributeName.DecisionMaking, 0.95f));

            RegisterPosition(Position.AM,
                (AttributeName.Pace, 0.65f), (AttributeName.Acceleration, 0.70f), (AttributeName.Stamina, 0.75f),
                (AttributeName.Strength, 0.40f), (AttributeName.Agility, 0.85f), (AttributeName.Passing, 0.90f),
                (AttributeName.Shooting, 0.80f), (AttributeName.Dribbling, 0.90f), (AttributeName.Crossing, 0.55f),
                (AttributeName.FirstTouch, 1.00f), (AttributeName.Tackling, 0.25f), (AttributeName.Vision, 1.00f),
                (AttributeName.Composure, 0.90f), (AttributeName.Positioning, 0.80f), (AttributeName.DecisionMaking, 0.95f));

            RegisterPosition(Position.LW,
                (AttributeName.Pace, 1.00f), (AttributeName.Acceleration, 1.00f), (AttributeName.Stamina, 0.85f),
                (AttributeName.Strength, 0.35f), (AttributeName.Agility, 1.00f), (AttributeName.Passing, 0.65f),
                (AttributeName.Shooting, 0.70f), (AttributeName.Dribbling, 1.00f), (AttributeName.Crossing, 0.90f),
                (AttributeName.FirstTouch, 0.85f), (AttributeName.Tackling, 0.20f), (AttributeName.Vision, 0.70f),
                (AttributeName.Composure, 0.75f), (AttributeName.Positioning, 0.65f), (AttributeName.DecisionMaking, 0.80f));

            RegisterPosition(Position.RW,
                (AttributeName.Pace, 1.00f), (AttributeName.Acceleration, 1.00f), (AttributeName.Stamina, 0.85f),
                (AttributeName.Strength, 0.35f), (AttributeName.Agility, 1.00f), (AttributeName.Passing, 0.65f),
                (AttributeName.Shooting, 0.70f), (AttributeName.Dribbling, 1.00f), (AttributeName.Crossing, 0.90f),
                (AttributeName.FirstTouch, 0.85f), (AttributeName.Tackling, 0.20f), (AttributeName.Vision, 0.70f),
                (AttributeName.Composure, 0.75f), (AttributeName.Positioning, 0.65f), (AttributeName.DecisionMaking, 0.80f));

            RegisterPosition(Position.ST,
                (AttributeName.Pace, 0.85f), (AttributeName.Acceleration, 0.90f), (AttributeName.Stamina, 0.70f),
                (AttributeName.Strength, 0.75f), (AttributeName.Agility, 0.75f), (AttributeName.Passing, 0.50f),
                (AttributeName.Shooting, 1.00f), (AttributeName.Dribbling, 0.70f), (AttributeName.Crossing, 0.25f),
                (AttributeName.FirstTouch, 0.85f), (AttributeName.Tackling, 0.10f), (AttributeName.Vision, 0.65f),
                (AttributeName.Composure, 1.00f), (AttributeName.Positioning, 1.00f), (AttributeName.DecisionMaking, 0.85f));
        }

        private static void RegisterPosition(Position position, params (AttributeName attr, float weight)[] entries)
        {
            var raw = new Dictionary<AttributeName, float>(entries.Length);
            float sum = 0f;
            foreach (var (attr, weight) in entries)
            {
                raw[attr] = weight;
                sum += weight;
            }
            RawWeights[position] = new ReadOnlyDictionary<AttributeName, float>(raw);

            var norm = new Dictionary<AttributeName, float>(entries.Length);
            foreach (var (attr, weight) in entries)
            {
                norm[attr] = weight / sum;
            }
            NormalizedWeights[position] = new ReadOnlyDictionary<AttributeName, float>(norm);
        }
    }
}
