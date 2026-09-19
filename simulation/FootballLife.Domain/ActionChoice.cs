using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an available choice of action presented to the player during a situation,
    /// alongside its associated risk level and expected value.
    /// </summary>
    public sealed record ActionChoice
    {
        public MatchAction Action { get; init; }
        public float RiskLevel { get; init; }
        public float ExpectedValue { get; init; }

        public ActionChoice(MatchAction action, float riskLevel, float expectedValue)
        {
            if (float.IsNaN(riskLevel) || riskLevel < 0f || riskLevel > 1f)
                throw new ArgumentOutOfRangeException(nameof(riskLevel), $"RiskLevel must be in [0, 1]. Actual: {riskLevel}");
            if (float.IsNaN(expectedValue) || expectedValue < 0f || expectedValue > 1f)
                throw new ArgumentOutOfRangeException(nameof(expectedValue), $"ExpectedValue must be in [0, 1]. Actual: {expectedValue}");

            Action = action;
            RiskLevel = riskLevel;
            ExpectedValue = expectedValue;
        }
    }
}
