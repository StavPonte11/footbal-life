using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Calculates realistic starting baseline PlayerAbilities for a rookie footballer based on their primary position.
    /// Uses PositionWeightMap to bias starting attribute points toward key positional demands.
    /// </summary>
    public static class StartingAttributeCalculator
    {
        public const int BaselineTargetOverall = 60;

        /// <summary>
        /// Generates starting PlayerAbilities tailored to the specified position.
        /// Primary attributes range ~62-68, secondary ~56-62, non-core ~46-55.
        /// </summary>
        public static PlayerAbilities CalculateStartingAbilities(Position position, int varianceSeed = 0)
        {
            var random = new SimulationRandom(varianceSeed == 0 ? (int)position * 101 + 42 : varianceSeed);

            byte GetAttributeValue(AttributeName attr)
            {
                float weight = PositionWeightMap.GetWeight(position, attr);
                int baseVal = weight switch
                {
                    >= 0.8f => 63 + random.NextInt(0, 6),
                    >= 0.6f => 58 + random.NextInt(0, 5),
                    >= 0.4f => 52 + random.NextInt(0, 5),
                    _ => 47 + random.NextInt(0, 6)
                };

                return (byte)Math.Clamp(baseVal, 35, 75);
            }

            return new PlayerAbilities(
                pace: GetAttributeValue(AttributeName.Pace),
                acceleration: GetAttributeValue(AttributeName.Acceleration),
                stamina: GetAttributeValue(AttributeName.Stamina),
                strength: GetAttributeValue(AttributeName.Strength),
                agility: GetAttributeValue(AttributeName.Agility),
                passing: GetAttributeValue(AttributeName.Passing),
                shooting: GetAttributeValue(AttributeName.Shooting),
                dribbling: GetAttributeValue(AttributeName.Dribbling),
                crossing: GetAttributeValue(AttributeName.Crossing),
                firstTouch: GetAttributeValue(AttributeName.FirstTouch),
                tackling: GetAttributeValue(AttributeName.Tackling),
                vision: GetAttributeValue(AttributeName.Vision),
                composure: GetAttributeValue(AttributeName.Composure),
                positioning: GetAttributeValue(AttributeName.Positioning),
                decisionMaking: GetAttributeValue(AttributeName.DecisionMaking)
            );
        }

        /// <summary>
        /// Calculates weighted overall rating (0-100) for a set of abilities in a specific position.
        /// </summary>
        public static int CalculateOverall(Position position, PlayerAbilities abilities)
        {
            float sum = 0f;
            foreach (AttributeName attr in Enum.GetValues(typeof(AttributeName)))
            {
                float normalizedWeight = PositionWeightMap.GetNormalizedWeight(position, attr);
                float val = abilities.Get(attr);
                sum += val * normalizedWeight;
            }

            return (int)Math.Round(sum);
        }
    }
}
