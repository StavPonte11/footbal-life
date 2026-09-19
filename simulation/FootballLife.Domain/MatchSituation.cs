using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an active match situation presented to a player.
    /// Tracks context parameters such as pressure, difficulty, positional advantage, and selectable choices.
    /// </summary>
    public sealed record MatchSituation
    {
        public SituationType Type { get; init; }
        public float OpponentPressure { get; init; }
        public float ExpectedDifficulty { get; init; }
        public float PositionalAdvantage { get; init; }
        public ActionChoice[] AvailableChoices { get; init; }

        public MatchSituation(
            SituationType type,
            float opponentPressure,
            float expectedDifficulty,
            float positionalAdvantage,
            ActionChoice[] availableChoices)
        {
            if (float.IsNaN(opponentPressure) || opponentPressure < 0f || opponentPressure > 10f)
                throw new ArgumentOutOfRangeException(nameof(opponentPressure), $"OpponentPressure must be in [0, 10]. Actual: {opponentPressure}");
            if (float.IsNaN(expectedDifficulty) || expectedDifficulty < 0f || expectedDifficulty > 1f)
                throw new ArgumentOutOfRangeException(nameof(expectedDifficulty), $"ExpectedDifficulty must be in [0, 1]. Actual: {expectedDifficulty}");
            if (float.IsNaN(positionalAdvantage) || positionalAdvantage < -1f || positionalAdvantage > 1f)
                throw new ArgumentOutOfRangeException(nameof(positionalAdvantage), $"PositionalAdvantage must be in [-1, 1]. Actual: {positionalAdvantage}");
            if (availableChoices is null)
                throw new ArgumentNullException(nameof(availableChoices));

            Type = type;
            OpponentPressure = opponentPressure;
            ExpectedDifficulty = expectedDifficulty;
            PositionalAdvantage = positionalAdvantage;
            AvailableChoices = availableChoices;
        }
    }
}
